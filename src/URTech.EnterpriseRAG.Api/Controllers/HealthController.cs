using Microsoft.AspNetCore.Mvc;
using URTech.EnterpriseRAG.Application.Embeddings;
using URTech.EnterpriseRAG.Application.Rag;
using URTech.EnterpriseRAG.Application.Search;

/*
File role: HealthController.cs
Layer: API
Purpose: Provides a lightweight runtime health endpoint showing the active embedding provider, answer provider, UTC time, and indexed chunk count.
Business flow: Client -> HealthController -> DI-resolved provider metadata/vector count -> health response.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromServices] IEmbeddingGenerator embeddings, [FromServices] IAnswerGenerator answers, [FromServices] IVectorStore vectors) =>
        Ok(new
        {
            status = "Healthy",
            utc = DateTimeOffset.UtcNow,
            embeddingProvider = embeddings.ProviderName,
            answerProvider = answers.ProviderName,
            indexedChunks = vectors.Count
        });
}
