import os
import tempfile
from pathlib import Path
from typing import Literal

from dotenv import load_dotenv
from fastapi import FastAPI, File, Form, UploadFile
from langchain_anthropic import ChatAnthropic
from langchain_classic.agents import AgentExecutor, create_tool_calling_agent
from langchain_core.messages import AIMessage, BaseMessage, HumanMessage
from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder
from langchain_core.tools import tool
from pydantic import BaseModel

import vectorstore
from chunking import chunk_documents
from extraction import load_document

load_dotenv()

app = FastAPI(title="ChatBot AI Service")

# create_tool_calling_agent, llm.bind_tools()'a ihtiyaç duyuyor; .with_retry() bunu
# saran farklı bir Runnable tipi döndürüp bind_tools'u kaybettiriyor. Bu yüzden retry'ı
# ham llm'e değil, aşağıda kurulan agent_executor'a ekliyoruz.
llm = ChatAnthropic(model="claude-sonnet-5", max_tokens=2048)

SYSTEM_PROMPT = """Sen Çevre Mühendisliği Teknik Asistanısın. Uzmanlık alanın: RO ve NF membran sistemlerinin tasarımı, su kimyası, ön arıtma, kabuk/kirlenme kontrolü, sistem işletmesi, temizlik/sanitizasyon, arıza giderme ve bakım.

Konuşma tarzın:
- Doğal ve akıcı konuş, sanki bir meslektaşınla sohbet ediyormuş gibi. Rapor veya resmi doküman gibi yazma.
- Cevaplarının başında veya sonunda "Bkz. Bölüm X" gibi referanslar, madde madde başlıklar veya kaynak listeleme YAPMA. Sağlanan doküman parçalarında "Tablo 8-14", "Şekil 3.2", "Bölüm 5" gibi kaynağa özgü etiketler geçse bile bunları cevabında TEKRARLAMA; oradaki bilgiyi doğrudan, sanki kendi bilgin gibi anlat. Bilgiyi doğrudan, düz metin (paragraf) halinde anlat.
- Gereksiz kalıp cümlelerden kaçın (örn: "kılavuzda bu konuya yer verilmemiştir" gibi bürokratik ifadeler yerine, konuyu kendi bilgin gibi doğal şekilde açıkla).
- Kısa ve öz cevap ver; kullanıcı detay isterse veya konu karmaşıksa daha uzun anlat.
- Emin olmadığın veya kılavuzda net karşılığı olmayan bir konuda, bunu tek cümlede doğal şekilde belirt, sonra elindeki en yakın bilgiyle yine de yardımcı olmaya çalış.

İçerik kuralları:
- Sadece çevre mühendisliği / su arıtma / RO-NF membran sistemleri kapsamındaki sorulara cevap ver. Matematik işlemi, genel kültür, kod yazma, güncel olaylar gibi uzmanlık alanının dışında bir soru gelirse, konuyla ilgisi olmadığını kibarca belirt ("Bu konu uzmanlık alanımın dışında, ben su arıtma ve RO/NF sistemleri konusunda yardımcı olabilirim" gibi) ve soruyu cevaplama.
- Cevaplarını sağlanan teknik kılavuza dayandır; sayısal değerleri (pH, sıcaklık, basınç limitleri vb.) kılavuzdan al, tahmin etme.
- Arıza gidermede semptomdan başla, olası sebepleri sırala, doğrulama testi öner, çözüm sun ama bunu da sohbet diliyle, madde işaretlemeden anlat.
- Prosedür sorularında adım adım anlat, ama akıcı bir dille.
- Kimyasal kullanımı, yüksek basınç gibi konularda güvenlik uyarısını doğal şekilde cümle içine yerleştir.
- Kullanıcı hangi dilde yazarsa o dilde cevap ver."""

AGENT_PROMPT = ChatPromptTemplate.from_messages([
    ("system", "{system_text}"),
    MessagesPlaceholder("chat_history"),
    ("human", "{input}"),
    MessagesPlaceholder("agent_scratchpad"),
])


@tool
def create_support_ticket(title: str, description: str) -> str:
    """Kullanıcı açıkça bir teknik destek talebi/arıza bildirimi oluşturulmasını
    istediğinde çağır (ör. 'bir talep aç', 'destek ekibine iletir misin',
    'servis çağırın'). Kullanıcı sadece bilgi/tavsiye istiyorsa ÇAĞIRMA.

    Args:
        title: Talebin kısa başlığı (ör. 'RO sisteminde basınç düşüşü')
        description: Sohbetten çıkarılan sorunun özet açıklaması
    """
    # Gerçek ticket kaydı burada oluşmuyor: Python'un kullanıcı/conversation DB'sine
    # erişimi yok. Bu çağrı sadece niyeti/parametreleri çıkarır; .NET, intermediate_steps
    # üzerinden bu bilgiyi okuyup asıl kaydı kendi DB'sinde oluşturur.
    return f"'{title}' başlıklı destek talebi oluşturma isteği alındı."


TOOLS = [create_support_ticket]
_agent = create_tool_calling_agent(llm, TOOLS, AGENT_PROMPT)
_agent_executor = AgentExecutor(
    agent=_agent, tools=TOOLS, return_intermediate_steps=True
).with_retry()


class HistoryMessage(BaseModel):
    role: Literal["user", "assistant"]
    content: str


class ChatRequest(BaseModel):
    message: str
    userId: str
    history: list[HistoryMessage] = []  # bu sohbetteki önceki mesajlar


class ToolCall(BaseModel):
    name: str
    input: dict


class ChatResponse(BaseModel):
    reply: str
    toolCall: ToolCall | None = None


class ProcessDocumentResponse(BaseModel):
    chunkCount: int


def _extract_reply_text(output: str | list) -> str:
    # Claude bazen (extended thinking ile) content'i düz string değil, "thinking" +
    # "text" bloklarından oluşan bir liste olarak döndürüyor — sadece text'i alıyoruz.
    if isinstance(output, str):
        return output
    return "".join(
        block.get("text", "")
        for block in output
        if isinstance(block, dict) and block.get("type") == "text"
    )


def _to_langchain_messages(history: list[HistoryMessage]) -> list[BaseMessage]:
    return [
        HumanMessage(content=h.content) if h.role == "user" else AIMessage(content=h.content)
        for h in history
    ]


def _build_context(user_id: str, message: str) -> str | None:
    # Arama, sohbet gecmisine bakmadan dogrudan son mesajla yapilir - ekstra bir
    # "sorguyu yeniden yaz" LLM turu (contextualize) eklemiyoruz. Bunun bedeli,
    # baglama muhtac takip sorularinda ("peki ya basinci duserse?") aramanin
    # zayif kalabilmesi; karsiligi ise mesaj basina bir tam LLM turu daha az gecikme.
    retriever = vectorstore.get_retriever(user_id, k=3)
    docs = retriever.invoke(message)

    if not docs:
        return None

    return "\n\n---\n\n".join(d.page_content for d in docs)


@app.get("/health")
def health():  # FastAPI ayakta mı kontrolü
    return {"status": "ok"}


@app.post("/chat", response_model=ChatResponse)
def chat(request: ChatRequest):
    chat_history = _to_langchain_messages(request.history)
    context = _build_context(request.userId, request.message)

    system_text = SYSTEM_PROMPT
    if context:
        system_text += (
            "\n\nAşağıda şirket dokümanlarından, kullanıcının sorusuyla ilgili "
            "olabilecek bölümler var. Cevabını öncelikle bu bölümlere dayandır. "
            "Cevap bu bölümlerde yoksa bilgi uydurma; elinde bu konuda bilgi "
            "olmadığını söyle.\n\n--- DOKÜMAN BÖLÜMLERİ ---\n" + context
        )

    result = _agent_executor.invoke({
        "system_text": system_text,
        "input": request.message,
        "chat_history": chat_history,
    })

    tool_call = None
    for action, _observation in result.get("intermediate_steps", []):
        if action.tool == "create_support_ticket":
            tool_call = ToolCall(name=action.tool, input=action.tool_input)
            break

    return ChatResponse(reply=_extract_reply_text(result["output"]), toolCall=tool_call)


@app.post("/documents/process", response_model=ProcessDocumentResponse)
async def process_document(
    file: UploadFile = File(...),
    document_id: str = Form(...),
    user_id: str = Form(...),
):
    suffix = Path(file.filename or "").suffix
    with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as tmp:
        tmp.write(await file.read())
        tmp_path = tmp.name

    try:
        docs = load_document(tmp_path)
        chunks = chunk_documents(docs)
    finally:
        os.remove(tmp_path)

    chunk_count = vectorstore.add_chunks(document_id, user_id, chunks)

    return ProcessDocumentResponse(chunkCount=chunk_count)


@app.delete("/documents/{document_id}")
def delete_document(document_id: str):
    vectorstore.delete_document(document_id)
    return {"status": "deleted"}
