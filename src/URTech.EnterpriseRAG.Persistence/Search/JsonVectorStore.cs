using System.Text.Json;
using URTech.EnterpriseRAG.Application.Search;

/*
File role: JsonVectorStore.cs
Layer: Persistence
Purpose: File-backed vector store used for local execution. Persists chunk vectors and performs cosine-similarity retrieval.
Business flow: Embedding vector -> JSON persistence; query vector -> cosine ranking -> Top-K results.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Persistence.Search;

/*
File-backed vector store for the public reference application.

This is intentionally small and transparent so reviewers can understand the
retrieval mechanics without requiring a cloud service.  The Application layer
depends only on IVectorStore, so Azure AI Search or another vector database can
replace this implementation without changing RAG orchestration.
*/
public sealed class JsonVectorStore : IVectorStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly List<VectorRecord> _records;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public JsonVectorStore(string filePath)
    {
        _filePath = filePath;
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        _records = Load(filePath);
    }

    public int Count
    {
        get
        {
            lock (_records) return _records.Count;
        }
    }

    public async Task UpsertAsync(VectorRecord record, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var index = _records.FindIndex(x => x.Chunk.ChunkId == record.Chunk.ChunkId);
            if (index >= 0) _records[index] = record; else _records.Add(record);
            await PersistAsync(cancellationToken);
        }
        finally { _gate.Release(); }
    }

    public async Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            _records.RemoveAll(x => x.Chunk.DocumentId == documentId);
            await PersistAsync(cancellationToken);
        }
        finally { _gate.Release(); }
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        float[] queryVector,
        Guid? documentId,
        int topK,
        double minimumScore,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return _records
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
        }
        finally { _gate.Release(); }
    }

    private async Task PersistAsync(CancellationToken cancellationToken)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, _records, JsonOptions, cancellationToken);
    }

    private static List<VectorRecord> Load(string filePath)
    {
        if (!File.Exists(filePath)) return new List<VectorRecord>();
        try
        {
            using var stream = File.OpenRead(filePath);
            return JsonSerializer.Deserialize<List<VectorRecord>>(stream, JsonOptions) ?? new List<VectorRecord>();
        }
        catch
        {
            return new List<VectorRecord>();
        }
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
