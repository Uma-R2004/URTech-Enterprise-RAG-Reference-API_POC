/*
File role: RagAnswer.cs
Layer: Application
Purpose: Application data contract used to move strongly typed information between the RAG workflow stages without coupling to HTTP or storage details.
Business flow: Application workflow stage -> typed contract -> next workflow stage/API mapping.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Rag;

public sealed record RagAnswer(
    string Question,
    string Answer,
    bool Grounded,
    string EmbeddingProvider,
    string AnswerProvider,
    IReadOnlyList<RagSource> Sources);
