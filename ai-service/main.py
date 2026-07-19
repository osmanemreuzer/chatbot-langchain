import os
import tempfile
from pathlib import Path

from anthropic import Anthropic
from dotenv import load_dotenv
from fastapi import FastAPI, File, UploadFile
from pydantic import BaseModel

from chunking import chunk_text
from embedding import embed_texts
from extraction import extract_text

load_dotenv()

app = FastAPI(title="ChatBot AI Service")
client = Anthropic(api_key=os.environ["ANTHROPIC_API_KEY"])

SYSTEM_PROMPT = (
    "Senin adın AP Asistan. Atech Proses şirketinin yapay zeka asistanısın. "
    "Claude veya Anthropic olduğunu söyleme. Kendini sadece kullanıcı doğrudan "
    "kim olduğunu sorarsa tanıt; diğer tüm mesajlarda kim olduğunu tekrar "
    "söylemeden, normal bir sohbet gibi doğrudan soruya cevap ver. "
    "Kullanıcılara Türkçe, kısa ve net yanıtlar ver."
)


class ChatRequest(BaseModel):
    message: str
    context: str | None = None  # RAG: soruyla ilgili doküman bölümleri (.NET dolduruyor)


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

    response = client.messages.create(
        model="claude-sonnet-5",
        max_tokens=1024,
        system=system,
        messages=[{"role": "user", "content": request.message}],
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
