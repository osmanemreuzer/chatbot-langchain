from langchain_core.documents import Document
from langchain_text_splitters import RecursiveCharacterTextSplitter


def chunk_documents(documents: list[Document], chunk_size: int = 1500, overlap: int = 150) -> list[Document]:
    splitter = RecursiveCharacterTextSplitter(chunk_size=chunk_size, chunk_overlap=overlap)
    return splitter.split_documents(documents)
# eskı rag kodunda ornegın 1500.karakter cumlenın ortasında bıttıgınde keserdı.şimdi 
# ise langchaınde once satır sonuna bakıyorveya bosluga bakar ve anlamlı keser.