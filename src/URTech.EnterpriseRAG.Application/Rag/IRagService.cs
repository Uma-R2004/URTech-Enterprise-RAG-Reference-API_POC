/*
File role: IRagService.cs
Layer: Application
Purpose: Defines an Application-layer contract so orchestration depends on abstractions rather than concrete infrastructure implementations.
Business flow: Application service -> interface contract -> Infrastructure/Persistence implementation.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Rag;

public interface IRagService
{
    Task<RagAnswer> AskAsync(AskQuestionRequest request, CancellationToken cancellationToken = default);
}
