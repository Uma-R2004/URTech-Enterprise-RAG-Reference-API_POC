using URTech.EnterpriseRAG.Application.Search;

/*
File role: IAnswerGenerator.cs
Layer: Application
Purpose: Defines an Application-layer contract so orchestration depends on abstractions rather than concrete infrastructure implementations.
Business flow: Application service -> interface contract -> Infrastructure/Persistence implementation.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Rag;

public interface IAnswerGenerator
{
    string ProviderName { get; }
    Task<string> GenerateAsync(string question, IReadOnlyList<SearchResult> sources, CancellationToken cancellationToken = default);
}
