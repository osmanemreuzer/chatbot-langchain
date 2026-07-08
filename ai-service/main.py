from fastapi import FastAPI

app = FastAPI(title="ChatBot AI Service")


@app.get("/health")
def health():
    return {"status": "ok"}
