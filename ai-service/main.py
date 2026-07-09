import os

from anthropic import Anthropic
from dotenv import load_dotenv
from fastapi import FastAPI
from pydantic import BaseModel

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


class ChatResponse(BaseModel):
    reply: str


@app.get("/health")
def health():  #FastAPI ayakta mı kontrolü
    return {"status": "ok"}


@app.post("/chat", response_model=ChatResponse)
def chat(request: ChatRequest):
    response = client.messages.create(
        model="claude-sonnet-5",
        max_tokens=1024,
        system=SYSTEM_PROMPT,
        messages=[{"role": "user", "content": request.message}],
    )
    text_block = next(block for block in response.content if block.type == "text")
    return ChatResponse(reply=text_block.text)
