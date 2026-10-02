using URTech.EnterpriseRAG.Application.Documents;

/*
File role: SlidingWindowTextChunkerTests.cs
Layer: Tests
Purpose: Automated xUnit test protecting a focused RAG behavior from regression.
Business flow: Arrange inputs/dependencies -> Act on component -> Assert expected behavior.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.UnitTests.Documents;

public sealed class SlidingWindowTextChunkerTests
{
    [Fact]
    public void CreateChunks_LongText_ProducesOrderedChunks()
    {
        var sut = new SlidingWindowTextChunker();
        var documentId = Guid.NewGuid();
        var text = string.Join(" ", Enumerable.Repeat("Enterprise AI governance requires security review and grounded retrieval.", 80));

        var chunks = sut.CreateChunks(documentId, text, 500, 80);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c => Assert.Equal(documentId, c.DocumentId));
        Assert.Equal(Enumerable.Range(0, chunks.Count), chunks.Select(x => x.Sequence));
    }
}
