*[English](README.md) | Türkçe*

# EE Asistanı — RAG Tabanlı Teknik Destek Chatbotu

Çevre mühendisliği / su arıtma alanında (RO ve NF membran sistemleri) çalışan teknik ekiplere yardımcı olmak için geliştirilen, doküman tabanlı (RAG) bir yapay zeka asistanı. Kullanıcılar teknik kılavuz dokümanlarını yükler, asistan bu dokümanlardan öğrendiği bilgilerle sorulara cevap verir ve gerektiğinde otomatik olarak destek talebi (ticket) açabilir.

Bu proje, full-stack bir yazılım stajı kapsamında sıfırdan, adım adım geliştirilmiştir.

## Öne Çıkan Özellikler

- 🔐 **JWT tabanlı kimlik doğrulama** — kayıt/giriş, korumalı endpoint'ler
- 💬 **Çoklu sohbet yönetimi** — birden fazla konuşma, geçmişe dönük mesaj listesi, çoklu-tur (multi-turn) bağlam
- 📄 **Doküman yükleme ve işleme** — PDF / DOCX / TXT / MD dosyalarından metin çıkarma, chunk'lama ve embedding üretimi
- 🔎 **RAG (Retrieval-Augmented Generation)** — kullanıcı sorusu embed edilip pgvector ile en alakalı doküman parçaları bulunur, Claude'a bağlam olarak verilir
- 🎯 **Kapsam kontrolü** — asistan yalnızca kendi uzmanlık alanındaki sorulara cevap verir, alan dışı sorularda (matematik, genel kültür vb.) nazikçe reddeder
- 🛠️ **Tool calling ile otomatik destek talebi** — kullanıcı açıkça bir arıza/destek talebi oluşturulmasını istediğinde, Claude ilgili tool'u çağırır ve sistem otomatik olarak ticket kaydı oluşturur
- 🎨 **Modern sohbet arayüzü** — ChatGPT/Claude tarzında, doküman-özel öneri kartları, gerçek zamanlı "yazıyor..." göstergesi

## Mimari

```mermaid
flowchart LR
    subgraph Frontend["React + TypeScript (Vite)"]
        UI[Sohbet Arayüzü]
    end

    subgraph Backend[".NET Core Web API (Clean Architecture)"]
        API[Controllers]
        APP[Application: Servisler]
        DOM[Domain: Entity'ler]
        INFRA[Infrastructure: EF Core / Repository]
    end

    subgraph AIService["Python FastAPI — AI Servisi"]
        CHAT[/chat/]
        EMBED[/embeddings/query/]
        PROC[/documents/process/]
    end

    DB[(PostgreSQL + pgvector)]
    CLAUDE[[Anthropic Claude API]]
    VOYAGE[[Voyage AI Embeddings]]

    UI -->|REST + JWT| API
    API --> APP --> INFRA --> DB
    APP -->|HTTP| CHAT
    APP -->|HTTP| EMBED
    APP -->|HTTP| PROC
    CHAT --> CLAUDE
    EMBED --> VOYAGE
    PROC --> VOYAGE
```

Sorumluluk paylaşımı net bir sınırla ayrılmıştır: **.NET** kimlik doğrulama, veri kalıcılığı, iş kuralları ve vektör benzerlik aramasını (pgvector) yönetir; **Python** yalnızca AI/ML işlerini (LLM çağrısı, embedding üretimi, doküman parçalama) yapar ve HTTP üzerinden .NET'e hizmet verir.

## Teknoloji Yığını

| Katman | Teknolojiler |
|---|---|
| **Frontend** | React 19, TypeScript, Vite, React Router, Axios, react-markdown |
| **Backend** | .NET 10 (ASP.NET Core Web API), Clean Architecture (Domain / Application / Infrastructure / Api), Entity Framework Core, JWT Bearer Auth, BCrypt |
| **AI Servisi** | Python, FastAPI, Anthropic SDK (Claude), Voyage AI (embeddings), pypdf / python-docx (doküman çıkarma) |
| **Veritabanı** | PostgreSQL 16 + pgvector (vektör benzerlik araması) |
| **Altyapı** | Docker Compose (PostgreSQL) |

## Proje Yapısı

```
chatbot-app/
├── frontend/           # React + TypeScript SPA
│   └── src/
│       ├── api/            # Backend'e HTTP istekleri
│       ├── components/     # Sohbet, doküman, layout bileşenleri
│       └── store/          # Auth state
├── backend/
│   └── src/
│       ├── ChatBot.Domain/          # Entity'ler (User, Conversation, Message, Document, SupportTicket...)
│       ├── ChatBot.Application/     # Servisler, DTO'lar, interface'ler
│       ├── ChatBot.Infrastructure/  # EF Core, repository implementasyonları, AI servis istemcisi
│       └── ChatBot.Api/             # Controller'lar, DI konfigürasyonu
├── ai-service/          # Python FastAPI — Claude entegrasyonu, embedding, chunking
└── docker-compose.yml   # PostgreSQL + pgvector
```

## Kurulum ve Çalıştırma

### Gereksinimler
- .NET 10 SDK
- Node.js 20+
- Python 3.12+
- Docker (PostgreSQL için)

### 1. Veritabanı

```bash
docker compose up -d
```

### 2. Backend (.NET)

```bash
cd backend/src/ChatBot.Api
dotnet ef database update
dotnet run
```

`appsettings.Development.json` içinde bağlantı dizesi ve JWT secret'ını kendinize göre ayarlayın (bu dosya `.gitignore`'dadır, repoda yer almaz).

### 3. AI Servisi (Python)

```bash
cd ai-service
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt
cp .env.example .env   # ANTHROPIC_API_KEY ve VOYAGE_API_KEY'i girin
uvicorn main:app --reload --port 8000
```

### 4. Frontend

```bash
cd frontend
npm install
npm run dev
```

Uygulama varsayılan olarak `http://localhost:5173` üzerinde açılır.

## Yol Haritası

Proje adım adım (küçük, doğrulanmış commit'lerle) geliştiriliyor. Tamamlanan ana aşamalar:

- [x] Kimlik doğrulama (JWT, kayıt/giriş)
- [x] Sohbet ve mesaj yönetimi
- [x] Claude API entegrasyonu, çoklu-tur konuşma geçmişi
- [x] Doküman yükleme, metin çıkarma ve chunk'lama
- [x] Voyage AI embedding + pgvector entegrasyonu
- [x] Uçtan uca RAG akışı
- [x] Tool calling ile otomatik destek talebi oluşturma
- [ ] Aynı RAG akışının LangChain ile yeniden yazılıp karşılaştırılması
- [ ] Docker Compose ile tüm sistemin tek komutla paketlenmesi

## Lisans

Bu proje bir staj çalışması kapsamında geliştirilmiştir.
