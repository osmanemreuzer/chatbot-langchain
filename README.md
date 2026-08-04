*English | [Türkçe](README.tr.md)*

# EE Assistant — RAG-Based Technical Support Chatbot

An AI assistant built for technical teams working in environmental engineering / water treatment (RO and NF membrane systems). Users upload technical guideline documents, and the assistant answers questions grounded in that knowledge (RAG) — and can automatically open a support ticket when needed.

This project was built from scratch, step by step, as part of a full-stack software internship.

## Key Features

- 🔐 **JWT-based authentication** — register/login, protected endpoints
- 💬 **Multi-conversation management** — multiple chats, persisted message history, multi-turn context
- 📄 **Document upload & processing** — text extraction, chunking, and embedding generation for PDF / DOCX / TXT / MD files
- 🔎 **RAG (Retrieval-Augmented Generation)** — the user's question is embedded and matched against document chunks via pgvector, which are then passed to Claude as context
- 🎯 **Scope control** — the assistant only answers questions within its domain and politely declines out-of-scope ones (math, general knowledge, etc.)
- 🛠️ **Tool calling for automatic support tickets** — when a user explicitly asks for a support/service request, Claude calls a tool and the backend automatically creates a ticket
- 🎨 **Modern chat UI** — ChatGPT/Claude-style interface with document-specific suggestion cards and a real-time "typing..." indicator

## Architecture

```mermaid
flowchart LR
    subgraph Frontend["React + TypeScript (Vite)"]
        UI[Chat UI]
    end

    subgraph Backend[".NET Core Web API (Clean Architecture)"]
        API[Controllers]
        APP[Application: Services]
        DOM[Domain: Entities]
        INFRA[Infrastructure: EF Core / Repositories]
    end

    subgraph AIService["Python FastAPI — AI Service"]
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

Responsibilities are cleanly split: **.NET** owns authentication, data persistence, business rules, and vector similarity search (pgvector); **Python** handles AI/ML work only (LLM calls, embedding generation, document chunking) and serves it to .NET over HTTP.

## Tech Stack

| Layer | Technologies |
|---|---|
| **Frontend** | React 19, TypeScript, Vite, React Router, Axios, react-markdown |
| **Backend** | .NET 10 (ASP.NET Core Web API), Clean Architecture (Domain / Application / Infrastructure / Api), Entity Framework Core, JWT Bearer Auth, BCrypt |
| **AI Service** | Python, FastAPI, Anthropic SDK (Claude), Voyage AI (embeddings), pypdf / python-docx (document extraction) |
| **Database** | PostgreSQL 16 + pgvector (vector similarity search) |
| **Infrastructure** | Docker Compose (PostgreSQL) |

## Project Structure

```
chatbot-app/
├── frontend/           # React + TypeScript SPA
│   └── src/
│       ├── api/            # HTTP calls to the backend
│       ├── components/     # Chat, document, layout components
│       └── store/          # Auth state
├── backend/
│   └── src/
│       ├── ChatBot.Domain/          # Entities (User, Conversation, Message, Document, SupportTicket...)
│       ├── ChatBot.Application/     # Services, DTOs, interfaces
│       ├── ChatBot.Infrastructure/  # EF Core, repository implementations, AI service client
│       └── ChatBot.Api/             # Controllers, DI configuration
├── ai-service/          # Python FastAPI — Claude integration, embeddings, chunking
└── docker-compose.yml   # PostgreSQL + pgvector
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- Python 3.12+
- Docker (for PostgreSQL)

### 1. Database

```bash
docker compose up -d
```

### 2. Backend (.NET)

```bash
cd backend/src/ChatBot.Api
dotnet ef database update
dotnet run
```

Set your connection string and JWT secret in `appsettings.Development.json` (this file is gitignored and not part of the repo).

### 3. AI Service (Python)

```bash
cd ai-service
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt
cp .env.example .env   # add your ANTHROPIC_API_KEY and VOYAGE_API_KEY
uvicorn main:app --reload --port 8000
```

### 4. Frontend

```bash
cd frontend
npm install
npm run dev
```

The app runs at `http://localhost:5173` by default.

## Roadmap

The project is being built step by step, with small, verified commits. Main milestones completed so far:

- [x] Authentication (JWT, register/login)
- [x] Conversation and message management
- [x] Claude API integration, multi-turn conversation history
- [x] Document upload, text extraction, and chunking
- [x] Voyage AI embeddings + pgvector integration
- [x] End-to-end RAG pipeline
- [x] Tool calling for automatic support ticket creation
- [ ] Rewriting the same RAG pipeline with LangChain for comparison
- [ ] Packaging the whole system with Docker Compose

## License

This project was built as part of a software internship.
