using System.Collections.Concurrent;
using URTech.EnterpriseRAG.Application.Search;

/*
File role: InMemoryVectorStore.cs
Layer: Infrastructure
Purpose: In-memory IVectorStore implementation useful for tests/demos. Performs cosine-similarity ranking and optional document filtering.
Business flow: Query vector -> cosine similarity against stored vectors -> Top-K SearchResult list.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Infrastructure.Search;

public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<Guid, VectorRecord> _records = new();
    public int Count => _records.Count;

    public Task UpsertAsync(VectorRecord record, CancellationToken cancellationToken = default)
    {
        _records[record.Chunk.ChunkId] = record;
        return Task.CompletedTask;
    }

    public Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        foreach (var pair in _records.Where(x => x.Value.Chunk.DocumentId == documentId).ToArray())
        {
            _records.TryRemove(pair.Key, out _);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SearchResult>> SearchAsync(float[] queryVector, Guid? documentId, int topK, double minimumScore, CancellationToken cancellationToken = default)
    {
        var results = _records.Values
            .Where(x => !documentId.HasValue || x.Chunk.DocumentId == documentId.Value)
            .Select(x => new SearchResult(
                x.Chunk.DocumentId,
                x.Chunk.ChunkId,
                x.Chunk.Sequence,
                x.Chunk.Text,
                Cosine(queryVector, x.Vector)))
            .Where(x => x.SimilarityScore >= minimumScore)
            .OrderByDescending(x => x.SimilarityScore)
            .ThenBy(x => x.Sequence)
            .Take(topK)
            .ToList();

        return Task.FromResult<IReadOnlyList<SearchResult>>(results);
    }

    private static double Cosine(float[] a, float[] b)
    {
        if (a.Length == 0 || b.Length == 0 || a.Length != b.Length) return 0d;
        double dot = 0, magA = 0, magB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        if (magA <= 0 || magB <= 0) return 0d;
        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}
