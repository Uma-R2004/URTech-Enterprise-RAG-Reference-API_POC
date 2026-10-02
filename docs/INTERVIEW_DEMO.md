# 5–8 Minute Technical Demo Flow

This sequence demonstrates the project as an enterprise RAG application rather than a collection of isolated AI utilities.

1. **Show the layered solution structure.** Explain Domain, Application, Infrastructure, Persistence, Shared, API and Tests.
2. **Show `DocumentIngestionService`.** Walk through extraction → normalization → chunking → embeddings → vector storage → document metadata.
3. **Show `RagService`.** Walk through question embedding → Top-K similarity retrieval → answer generation → sources.
4. **Show provider abstractions.** Explain how local zero-secret providers and Azure OpenAI adapters implement the same interfaces.
5. **Run Swagger.** Show the document, health and RAG endpoints.
6. **Upload a PDF.** Demonstrate `POST /api/documents/upload` and then `GET /api/documents`.
7. **Ask a grounded question.** Demonstrate `POST /api/rag/ask` and point out `grounded`, sources and similarity scores.
8. **Open Angular.** Show the same RAG result at `http://localhost:4200`.
9. **Open React.** Show the same backend consumed at `http://localhost:5173`.
10. **Show tests and CI.** Point to the xUnit project and `azure-pipelines.yml`.
11. **Close with production extensions.** Azure AI Search, Azure SQL, Azure Blob Storage, Document Intelligence/OCR, Entra ID, Key Vault, Application Insights and APIM can replace/reference adapters without rewriting Application-layer use cases.

## Runtime evidence

- [Verified Local Results](testing/VERIFIED_LOCAL_RESULTS.md)
- [Angular and React Testing](frontend/FRONTEND_TESTING.md)
- [API Testing](testing/API_TESTING.md)
