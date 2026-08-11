import os

from langchain_core.documents import Document
from langchain_postgres import PGVector
from langchain_voyageai import VoyageAIEmbeddings
from sqlalchemy import delete as sa_delete

from rate_limit_retry import with_rate_limit_retry

COLLECTION_NAME = "document_chunks"
EMBEDDING_MODEL = "voyage-4-lite"

_store: PGVector | None = None


def _get_store() -> PGVector:
    # Lazy init: aynı sebep, .env yuklenmeden VOYAGE_API_KEY/DATABASE_URL aranmasin.
    global _store
    if _store is None:
        embeddings = VoyageAIEmbeddings(model=EMBEDDING_MODEL, batch_size=128)
        _store = PGVector(
            embeddings=embeddings,
            collection_name=COLLECTION_NAME,
            connection=os.environ["DATABASE_URL"],
            use_jsonb=True,
        )
    return _store


def add_chunks(document_id: str, user_id: str, chunks: list[Document]) -> int:
    # Embedding artik ayri bir adim degil: add_documents chunk'lari kendi icinde
    # embed edip ayni cagrida PGVector'e yaziyor.
    if not chunks:
        return 0

    store = _get_store()
    for chunk in chunks:
        chunk.metadata["document_id"] = document_id
        chunk.metadata["user_id"] = user_id

    ids = [f"{document_id}:{i}" for i in range(len(chunks))]
    with_rate_limit_retry(lambda: store.add_documents(chunks, ids=ids)) #store.add_document embbedıng de yapıyor. ve veri tabanına ekliyor. 
    return len(chunks)


def get_retriever(user_id: str, k: int = 3): #reteiever veriyi çekiyor dbden.
    # MMR (Maximal Marginal Relevance): en yakin k sonucu degil, fetch_k aday arasindan
    # hem alakali hem birbirinden CESITLI olan k tanesini seciyor - ayni chunk'in
    # varyasyonlariyla sonuc listesinin dolmasini engelliyor. lambda_mult=0.5 alakalilik/
    # cesitlilik arasinda dengeli varsayilan deger.
    # Kullanici izolasyonu burada saglaniyor: sadece bu user_id'ye ait chunk'larda arar.
    return _get_store().as_retriever(
        search_type="mmr",
        search_kwargs={"k": k, "fetch_k": k * 4, "lambda_mult": 0.5, "filter": {"user_id": user_id}},
    )


def delete_document(document_id: str) -> None:
    # PGVector.delete() sadece id listesiyle siliyor, metadata filtresi desteklemiyor.
    # PGVector'un kendi SQLAlchemy modelini (EmbeddingStore) kullanarak document_id'ye
    # gore siliyoruz.
    store = _get_store()
    with store._make_sync_session() as session:
        stmt = sa_delete(store.EmbeddingStore).where(
            store.EmbeddingStore.cmetadata["document_id"].astext == document_id
        )
        session.execute(stmt)
        session.commit()
