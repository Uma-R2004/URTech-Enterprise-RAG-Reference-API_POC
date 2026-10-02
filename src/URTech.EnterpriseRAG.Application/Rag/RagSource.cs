/*
File role: RagSource.cs
Layer: Application
Purpose: Application data contract used to move strongly typed information between the RAG workflow stages without coupling to HTTP or storage details.
Business flow: Application workflow stage -> typed contract -> next workflow stage/API mapping.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Rag;

public sealed record RagSource(
    Guid DocumentId,
    Guid ChunkId,
    int ChunkSequence,
    double SimilarityScore,
    string Text);
