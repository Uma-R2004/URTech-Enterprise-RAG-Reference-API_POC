# Architecture — URTech Enterprise RAG Reference API

This reference application demonstrates how a Retrieval-Augmented Generation (RAG) solution can be implemented inside a layered .NET architecture instead of placing AI logic directly in controllers.

## Layered architecture

```mermaid
flowchart TD
    UI[Swagger / Angular / React] --> API[API Layer]
    API --> APP[Application Layer]
    APP --> DOM[Domain Layer]
    APP --> INF[Infrastructure Adapters]
    APP --> PER[Persistence Adapters]
    INF --> EXT[PDF / HTML extraction\nLocal or Azure OpenAI\nSimilarity search]
    PER --> STORE[JSON metadata + vectors\nReference implementation]
```

### Dependency rule

The important design rule is that the **Application layer owns the use-case contracts**. Infrastructure and Persistence implement those contracts. The Application layer therefore does not need to know whether embeddings come from the local deterministic provider or Azure OpenAI, and it does not need to know whether vectors are stored in JSON, memory, Azure AI Search, or another approved vector database.

## RAG ingestion flow

```mermaid
flowchart LR
    A[Upload / Ingest text] --> B[Text extractor]
    B --> C[Text normalizer]
    C --> D[Sliding-window chunker]
    D --> E[Embedding generator]
    E --> F[Vector store]
    A --> G[Document metadata repository]
```

## RAG query flow

```mermaid
flowchart LR
    Q[User question] --> E[Question embedding]
    E --> S[Vector similarity search]
    S --> K[Top-K chunks]
    K --> A[Grounded answer generator]
    A --> R[Answer + sources]
```

## Project structure captured from the verified local solution

![Visual Studio project structure](../images/04-project-structure.png)

## Layer responsibilities

| Layer | Responsibility | Examples |
|---|---|---|
| Domain | Business meaning and lifecycle state | `KnowledgeDocument`, `KnowledgeDocumentStatus` |
| Application | Use cases, interfaces, orchestration, DTOs | `DocumentIngestionService`, `RagService`, `IEmbeddingGenerator`, `IVectorStore` |
| Infrastructure | Technical adapters | PDF/HTML extraction, local/Azure OpenAI providers, in-memory vector adapter |
| Persistence | Local reference persistence | JSON document repository, JSON vector store |
| Shared | Cross-cutting contracts | `ApiResponse<T>` |
| API | HTTP boundary and dependency composition | Controllers, Swagger, startup, demo seeding |
| Tests | Automated regression protection | Chunking, deterministic embeddings, end-to-end RAG flow |
| UI | Optional client examples | Angular and React reference clients |

## Production extension path

The reference adapters are intentionally replaceable. A production enterprise version can move to:

- Azure OpenAI embeddings and chat completion
- Azure AI Search or another approved vector database
- SQL Server / Azure SQL metadata persistence
- Azure AI Document Intelligence for OCR/scanned documents
- Microsoft Entra ID authentication and role-based authorization
- Azure Key Vault and managed identity for secrets
- Application Insights / OpenTelemetry for tracing and operational metrics
- API Management for policy, throttling, versioning and governance
