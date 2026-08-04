def chunk_text(text: str, chunk_size: int = 1500, overlap: int = 150) -> list[str]:
    chunks = []
    step = chunk_size - overlap
    for start in range(0, len(text), step):
        chunk = text[start:start + chunk_size]
        chunks.append(chunk)
        if start + chunk_size >= len(text):
            break
    return chunks
