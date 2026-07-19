from pathlib import Path

import pypdf
from docx import Document


def extract_text(file_path: str) -> str:
    extension = Path(file_path).suffix.lower()

    if extension in (".txt", ".md"):
        return Path(file_path).read_text(encoding="utf-8")

    if extension == ".pdf":
        reader = pypdf.PdfReader(file_path)
        return "\n".join(page.extract_text() or "" for page in reader.pages)

    if extension == ".docx":
        document = Document(file_path)
        return "\n".join(paragraph.text for paragraph in document.paragraphs)

    raise ValueError(f"Desteklenmeyen dosya türü: {extension}")
