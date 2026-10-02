/*
File role: IDocumentIngestionService.cs
Layer: Application
Purpose: Defines an Application-layer contract so orchestration depends on abstractions rather than concrete infrastructure implementations.
Business flow: Application service -> interface contract -> Infrastructure/Persistence implementation.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Documents;

public interface IDocumentIngestionService
{
    Task<DocumentResponse> IngestTextAsync(string fileName, string content, CancellationToken cancellationToken = default);
    Task<DocumentResponse> IngestFileAsync(string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentResponse>> GetDocumentsAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default);
}
