from typing import cast

import voyageai

EMBEDDING_MODEL = "voyage-4-lite"

# Voyage API tek istekte sınırlı sayıda metin kabul ediyor, uzun listeleri parça parça göndeririz.
BATCH_SIZE = 128

_client: voyageai.Client | None = None


def _get_client() -> voyageai.Client:
    # Lazy init: client ilk kullanımda oluşturulur, böylece modül import edildiği anda
    # (load_dotenv çalışmadan önce) VOYAGE_API_KEY aranmaz.
    global _client
    if _client is None:
        _client = voyageai.Client()  # API key'i VOYAGE_API_KEY ortam değişkeninden okur
    return _client


def embed_texts(texts: list[str], input_type: str = "document") -> list[list[float]]:
    """Metin listesini embedding vektörlerine çevirir.

    input_type: doküman chunk'ları için "document", kullanıcı sorusu için "query".
    Voyage bu bilgiyi kullanarak arama kalitesini artırır.
    """
    client = _get_client()
    embeddings: list[list[float]] = []

    for start in range(0, len(texts), BATCH_SIZE):
        batch = texts[start:start + BATCH_SIZE]
        result = client.embed(batch, model=EMBEDDING_MODEL, input_type=input_type)
        # output_dtype istemediğimiz için API her zaman float döner; kütüphanenin
        # "float veya int olabilir" tip tanımını cast ile daraltıyoruz.
        embeddings.extend(cast(list[list[float]], result.embeddings))

    return embeddings
