/*
File role: KnowledgeDocument.cs
Layer: Domain
Purpose: Core domain entity representing an enterprise knowledge source and its ingestion lifecycle. It intentionally contains no API, Azure, or persistence concerns.
Business flow: Created -> Processing -> Ready/Failed with counts and timestamps.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Domain.Documents;

/*
Domain entity representing one knowledge source that has been ingested into
the RAG pipeline.  The entity intentionally contains no Azure, database, or
HTTP concerns.
*/
public sealed class KnowledgeDocument
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/plain";
    public KnowledgeDocumentStatus Status { get; set; } = KnowledgeDocumentStatus.Pending;
    public int CharacterCount { get; set; }
    public int ChunkCount { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAtUtc { get; set; }
}
