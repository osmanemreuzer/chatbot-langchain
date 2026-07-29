from pathlib import Path

import pypdf
from docx import Document


def extract_text(file_path: str) -> str:
    extension = Path(file_path).suffix.lower()

    if extension in (".txt", ".md"):
        raw = Path(file_path).read_bytes()
        # cp1252 gibi bir fallback'e düşmek tehlikeli: dosyada tek bir geçersiz
        # UTF-8 baytı olsa bile tüm dosyayı yanlış encoding ile yeniden yorumlar
        # ve zaten doğru olan çok baytlı UTF-8 karakterlerini (Ü, İ, Ş, Ğ, Ö...)
        # mojibake'e çevirir. Sadece geçersiz baytları değiştirmek çok daha güvenli.
        try:
            return raw.decode("utf-8")
        except UnicodeDecodeError:
            return raw.decode("utf-8", errors="replace")

    if extension == ".pdf":
        reader = pypdf.PdfReader(file_path)
        return "\n".join(page.extract_text() or "" for page in reader.pages)

    if extension == ".docx":
        document = Document(file_path)
        # document.paragraphs sadece düz paragrafları döner, tablo hücrelerindeki
        # metni atlar (python-docx'te tablolar ayrı bir koleksiyon) — bu yüzden
        # ayrıca document.tables'ı da gezip satırları ekliyoruz.
        parts = [paragraph.text for paragraph in document.paragraphs]
        for table in document.tables:
            for row in table.rows:
                parts.append(" | ".join(cell.text for cell in row.cells))
        return "\n".join(parts)

    raise ValueError(f"Desteklenmeyen dosya türü: {extension}")
