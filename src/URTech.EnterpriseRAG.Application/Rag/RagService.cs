using URTech.EnterpriseRAG.Application.Embeddings;
using URTech.EnterpriseRAG.Application.Search;

/*
File role: RagService.cs
Layer: Application
Purpose: Orchestrates the RAG query path without depending on a specific LLM or vector database.
Business flow: Question -> question embedding -> similarity search -> Top-K sources -> answer generator -> answer + citations.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Rag;

public sealed class RagService : IRagService
{
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly IVectorStore _vectorStore;
    private readonly IAnswerGenerator _answerGenerator;

    public RagService(IEmbeddingGenerator embeddingGenerator, IVectorStore vectorStore, IAnswerGenerator answerGenerator)
    {
        _embeddingGenerator = embeddingGenerator;
        _vectorStore = vectorStore;
        _answerGenerator = answerGenerator;
    }

    public async Task<RagAnswer> AskAsync(AskQuestionRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Question)) throw new ArgumentException("Question is required.", nameof(request));

        var topK = Math.Clamp(request.TopK, 1, 10);
        var minScore = Math.Clamp(request.MinimumSimilarityScore, -1d, 1d);
        var query = await _embeddingGenerator.GenerateAsync(request.Question, cancellationToken);
        var results = await _vectorStore.SearchAsync(query, request.DocumentId, topK, minScore, cancellationToken);

        if (results.Count == 0)
        {
            return new RagAnswer(
                request.Question,
                "I could not find enough supporting information in the indexed documents.",
                false,
                _embeddingGenerator.ProviderName,
                _answerGenerator.ProviderName,
                Array.Empty<RagSource>());
        }

        var answer = await _answerGenerator.GenerateAsync(request.Question, results, cancellationToken);
        var sources = results.Select(x => new RagSource(x.DocumentId, x.ChunkId, x.Sequence, Math.Round(x.SimilarityScore, 4), x.Text)).ToList();

        return new RagAnswer(
            request.Question,
            answer,
            !string.IsNullOrWhiteSpace(answer),
            _embeddingGenerator.ProviderName,
            _answerGenerator.ProviderName,
            sources);
    }
}
