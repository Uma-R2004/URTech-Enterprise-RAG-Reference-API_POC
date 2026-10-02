/*
File role: KnowledgeDocumentStatus.cs
Layer: Domain
Purpose: Defines the allowed lifecycle states for a knowledge document during ingestion and indexing.
Business flow: Pending -> Processing -> Ready or Failed.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Domain.Documents;

public enum KnowledgeDocumentStatus
{
    Pending = 0,
    Processing = 1,
    Ready = 2,
    Failed = 3
}
