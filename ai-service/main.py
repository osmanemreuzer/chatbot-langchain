import os
import tempfile
from pathlib import Path
from typing import Literal

from anthropic import Anthropic
from anthropic.types import MessageParam
from dotenv import load_dotenv
from fastapi import FastAPI, File, UploadFile
from pydantic import BaseModel

from chunking import chunk_text
from embedding import embed_texts
from extraction import extract_text

load_dotenv()

app = FastAPI(title="ChatBot AI Service")
client = Anthropic(api_key=os.environ["ANTHROPIC_API_KEY"])

SYSTEM_PROMPT = """Sen Çevre Mühendisliği Teknik Asistanısın. Uzmanlık alanın: RO ve NF membran sistemlerinin tasarımı, su kimyası, ön arıtma, kabuk/kirlenme kontrolü, sistem işletmesi, temizlik/sanitizasyon, arıza giderme ve bakım.

Konuşma tarzın:
- Doğal ve akıcı konuş, sanki bir meslektaşınla sohbet ediyormuş gibi. Rapor veya resmi doküman gibi yazma.
- Cevaplarının başında veya sonunda "Bkz. Bölüm X" gibi referanslar, madde madde başlıklar veya kaynak listeleme YAPMA. Bilgiyi doğrudan, düz metin (paragraf) halinde anlat.
- Gereksiz kalıp cümlelerden kaçın (örn: "kılavuzda bu konuya yer verilmemiştir" gibi bürokratik ifadeler yerine, konuyu kendi bilgin gibi doğal şekilde açıkla).
- Kısa ve öz cevap ver; kullanıcı detay isterse veya konu karmaşıksa daha uzun anlat.
- Emin olmadığın veya kılavuzda net karşılığı olmayan bir konuda, bunu tek cümlede doğal şekilde belirt, sonra elindeki en yakın bilgiyle yine de yardımcı olmaya çalış.

İçerik kuralları:
- Cevaplarını sağlanan teknik kılavuza dayandır; sayısal değerleri (pH, sıcaklık, basınç limitleri vb.) kılavuzdan al, tahmin etme.
- Arıza gidermede semptomdan başla, olası sebepleri sırala, doğrulama testi öner, çözüm sun ama bunu da sohbet diliyle, madde işaretlemeden anlat.
- Prosedür sorularında adım adım anlat, ama akıcı bir dille.
- Kimyasal kullanımı, yüksek basınç gibi konularda güvenlik uyarısını doğal şekilde cümle içine yerleştir.
- Kullanıcı hangi dilde yazarsa o dilde cevap ver."""


class HistoryMessage(BaseModel):
    role: Literal["user", "assistant"]
    content: str


class ChatRequest(BaseModel):
    message: str
    context: str | None = None  # RAG: soruyla ilgili doküman bölümleri (.NET dolduruyor)
    history: list[HistoryMessage] = []  # bu sohbetteki önceki mesajlar


class ChatResponse(BaseModel):
    reply: str


class EmbedQueryRequest(BaseModel):
    text: str


class EmbedQueryResponse(BaseModel):
    embedding: list[float]


class ChunkDto(BaseModel):
    index: int
    content: str
    embedding: list[float]


class ProcessDocumentResponse(BaseModel):
    chunkCount: int
    chunks: list[ChunkDto]


@app.get("/health")
def health():  #FastAPI ayakta mı kontrolü
    return {"status": "ok"}


@app.post("/chat", response_model=ChatResponse)
def chat(request: ChatRequest):
    system = SYSTEM_PROMPT
    if request.context:
        system += (
            "\n\nAşağıda şirket dokümanlarından, kullanıcının sorusuyla ilgili "
            "olabilecek bölümler var. Cevabını öncelikle bu bölümlere dayandır. "
            "Cevap bu bölümlerde yoksa bilgi uydurma; elinde bu konuda bilgi "
            "olmadığını söyle.\n\n--- DOKÜMAN BÖLÜMLERİ ---\n" + request.context
        )

    messages: list[MessageParam] = [
        {"role": h.role, "content": h.content} for h in request.history
    ]
    messages.append({"role": "user", "content": request.message})

    response = client.messages.create(
        model="claude-sonnet-5",
        max_tokens=1024,
        system=system,
        messages=messages,
    )
    text_block = next(block for block in response.content if block.type == "text")
    return ChatResponse(reply=text_block.text)


@app.post("/embeddings/query", response_model=EmbedQueryResponse)
def embed_query(request: EmbedQueryRequest):
    # Kullanıcı sorusu tek metin; input_type="query" ile embed edilir
    # (chunk'lar "document" ile embed edilmişti — asimetrik arama).
    [embedding] = embed_texts([request.text], input_type="query")
    return EmbedQueryResponse(embedding=embedding)


@app.post("/documents/process", response_model=ProcessDocumentResponse)
async def process_document(file: UploadFile = File(...)):
    suffix = Path(file.filename or "").suffix
    with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as tmp:
        tmp.write(await file.read())
        tmp_path = tmp.name

    try:
        text = extract_text(tmp_path)
        chunks = chunk_text(text)
    finally:
        os.remove(tmp_path)

    embeddings = embed_texts(chunks) if chunks else []

    return ProcessDocumentResponse(
        chunkCount=len(chunks),
        chunks=[
            ChunkDto(index=i, content=c, embedding=e)
            for i, (c, e) in enumerate(zip(chunks, embeddings))
        ],
    )
