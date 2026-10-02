using System.Text.Json;
using URTech.EnterpriseRAG.Application.Documents;
using URTech.EnterpriseRAG.Domain.Documents;

/*
File role: JsonDocumentRepository.cs
Layer: Persistence
Purpose: File-backed document metadata repository used by the public reference application. It can be replaced by SQL Server/Azure SQL without changing Application services.
Business flow: Application repository call -> synchronized JSON read/write.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Persistence.Documents;

/*
Small file-backed persistence adapter used by the public reference app.  The
Application layer depends only on IDocumentRepository, so SQL Server/EF Core
can replace this adapter without changing the RAG use cases.
*/
public sealed class JsonDocumentRepository : IDocumentRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public JsonDocumentRepository(string filePath)
    {
        _filePath = filePath;
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
    }

    public async Task UpsertAsync(KnowledgeDocument document, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var items = await ReadInternalAsync(cancellationToken);
            var index = items.FindIndex(x => x.Id == document.Id);
            if (index >= 0) items[index] = document; else items.Add(document);
            await WriteInternalAsync(items, cancellationToken);
        }
        finally { _gate.Release(); }
    }

    public async Task<KnowledgeDocument?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return (await ReadInternalAsync(cancellationToken)).FirstOrDefault(x => x.Id == id); }
        finally { _gate.Release(); }
    }

    public async Task<IReadOnlyList<KnowledgeDocument>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return await ReadInternalAsync(cancellationToken); }
        finally { _gate.Release(); }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var items = await ReadInternalAsync(cancellationToken);
            items.RemoveAll(x => x.Id == id);
            await WriteInternalAsync(items, cancellationToken);
        }
        finally { _gate.Release(); }
    }

    private async Task<List<KnowledgeDocument>> ReadInternalAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath)) return new List<KnowledgeDocument>();
        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<KnowledgeDocument>>(stream, JsonOptions, cancellationToken) ?? new List<KnowledgeDocument>();
    }

    private async Task WriteInternalAsync(List<KnowledgeDocument> items, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, items, JsonOptions, cancellationToken);
    }
}
