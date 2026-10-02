# Code Walkthrough

This guide explains how the main files interact from first request to final RAG answer.

## Startup and dependency composition

`src/URTech.EnterpriseRAG.Api/Program.cs`

The API project is the composition root. It registers controllers, Swagger, persistence adapters, extractors, the chunker, embedding provider, answer provider, ingestion service and RAG service. Configuration selects **Local** or **AzureOpenAI** providers without changing Application-layer code.

## Document ingestion

`DocumentsController` accepts text or a file and calls `IDocumentIngestionService`.

`DocumentIngestionService` performs the use-case sequence:

1. Create document metadata with `Processing` status.
2. Choose the correct `IDocumentTextExtractor` by file extension.
3. Extract readable text.
4. Normalize whitespace and line breaks.
5. Create overlapping chunks.
6. Generate an embedding for every chunk.
7. Store the chunk/vector pair through `IVectorStore`.
8. Update metadata to `Ready`, including character count and chunk count.
9. If processing fails, update the metadata to `Failed` with a failure reason.

## Retrieval and RAG

`RagController` accepts an `AskQuestionRequest` and calls `IRagService`.

`RagService`:

1. Validates and bounds Top-K and minimum similarity.
2. Generates a question embedding.
3. Searches the vector store using cosine similarity.
4. Stops with a safe no-evidence answer if nothing relevant is found.
5. Passes only retrieved evidence to the answer generator.
6. Returns the answer together with the exact source chunks used for grounding.

## Local versus Azure providers

The default Local mode exists so the repository can run with **zero credentials**:

- `LocalHashEmbeddingGenerator` — deterministic local vector generation
- `LocalGroundedAnswerGenerator` — extractive grounded answer generation

The optional Azure mode demonstrates cloud integration points:

- `AzureOpenAiEmbeddingGenerator`
- `AzureOpenAiAnswerGenerator`

Secrets are deliberately absent from source control and should be supplied with User Secrets, environment variables, Key Vault or managed identity patterns.

## Persistence

The public reference uses simple JSON-backed persistence so reviewers can understand the mechanics without provisioning a database. The Application layer depends on contracts, therefore these adapters can be replaced by SQL Server/Azure SQL and Azure AI Search without rewriting the use cases.
