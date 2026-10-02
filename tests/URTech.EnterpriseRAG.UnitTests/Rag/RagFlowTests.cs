using URTech.EnterpriseRAG.Application.Documents;
using URTech.EnterpriseRAG.Application.Rag;
using URTech.EnterpriseRAG.Infrastructure.Embeddings;
using URTech.EnterpriseRAG.Infrastructure.Extraction;
using URTech.EnterpriseRAG.Infrastructure.Rag;
using URTech.EnterpriseRAG.Infrastructure.Search;
using URTech.EnterpriseRAG.Persistence.Documents;

/*
File role: RagFlowTests.cs
Layer: Tests
Purpose: Automated xUnit test protecting a focused RAG behavior from regression.
Business flow: Arrange inputs/dependencies -> Act on component -> Assert expected behavior.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.UnitTests.Rag;

public sealed class RagFlowTests
{
    [Fact]
    public async Task AskAsync_AfterIngestion_ReturnsGroundedAnswerAndSources()
    {
        var path = Path.Combine(Path.GetTempPath(), $"urtech-rag-{Guid.NewGuid():N}.json");
        try
        {
            var repository = new JsonDocumentRepository(path);
            var vectors = new InMemoryVectorStore();
            var embeddings = new LocalHashEmbeddingGenerator();
            var ingestion = new DocumentIngestionService(
                repository,
                new IDocumentTextExtractor[] { new PlainTextExtractor() },
                new DocumentTextNormalizer(),
                new SlidingWindowTextChunker(),
                embeddings,
                vectors);

            await ingestion.IngestTextAsync(
                "policy.txt",
                "For privileged access requests, the requester's manager provides business approval. The security team validates the access level before provisioning.");

            var rag = new RagService(embeddings, vectors, new LocalGroundedAnswerGenerator());
            var answer = await rag.AskAsync(new AskQuestionRequest("Who approves privileged access requests?", TopK: 3, MinimumSimilarityScore: -1));

            Assert.True(answer.Grounded);
            Assert.NotEmpty(answer.Sources);
            Assert.Contains("manager", answer.Answer, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
