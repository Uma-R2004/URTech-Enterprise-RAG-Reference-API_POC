using URTech.EnterpriseRAG.Infrastructure.Embeddings;

/*
File role: LocalHashEmbeddingGeneratorTests.cs
Layer: Tests
Purpose: Automated xUnit test protecting a focused RAG behavior from regression.
Business flow: Arrange inputs/dependencies -> Act on component -> Assert expected behavior.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.UnitTests.Embeddings;

public sealed class LocalHashEmbeddingGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_SameText_ReturnsSameVector()
    {
        var sut = new LocalHashEmbeddingGenerator();
        var first = await sut.GenerateAsync("Azure security review and RAG grounding");
        var second = await sut.GenerateAsync("Azure security review and RAG grounding");
        Assert.Equal(first, second);
        Assert.Equal(384, first.Length);
    }
}
