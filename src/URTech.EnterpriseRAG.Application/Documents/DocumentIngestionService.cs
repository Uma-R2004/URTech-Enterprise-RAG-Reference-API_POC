using URTech.EnterpriseRAG.Application.Embeddings;
using URTech.EnterpriseRAG.Application.Search;
using URTech.EnterpriseRAG.Domain.Documents;

/*
File role: DocumentIngestionService.cs
Layer: Application
Purpose: Coordinates document ingestion while remaining independent of concrete storage, file-extraction, embedding, and vector implementations.
Business flow: File/text -> extract -> normalize -> chunk -> embed -> vector upsert -> document metadata status.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Documents;

public sealed class DocumentIngestionService : IDocumentIngestionService
{
    private readonly IDocumentRepository _repository;
    private readonly IEnumerable<IDocumentTextExtractor> _extractors;
    private readonly IDocumentTextNormalizer _normalizer;
    private readonly ITextChunker _chunker;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly IVectorStore _vectorStore;

    public DocumentIngestionService(
        IDocumentRepository repository,
        IEnumerable<IDocumentTextExtractor> extractors,
        IDocumentTextNormalizer normalizer,
        ITextChunker chunker,
        IEmbeddingGenerator embeddingGenerator,
        IVectorStore vectorStore)
    {
        _repository = repository;
        _extractors = extractors;
        _normalizer = normalizer;
        _chunker = chunker;
        _embeddingGenerator = embeddingGenerator;
        _vectorStore = vectorStore;
    }

    public async Task<DocumentResponse> IngestTextAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content ?? string.Empty));
        return await IngestCoreAsync(fileName, "text/plain", stream, ".txt", cancellationToken);
    }

    public Task<DocumentResponse> IngestFileAsync(string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        return IngestCoreAsync(fileName, contentType, stream, extension, cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentResponse>> GetDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var docs = await _repository.GetAllAsync(cancellationToken);
        return docs.Select(Map).OrderByDescending(x => x.CreatedAtUtc).ToList();
    }

    public async Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        await _vectorStore.DeleteDocumentAsync(documentId, cancellationToken);
        await _repository.DeleteAsync(documentId, cancellationToken);
    }

    private async Task<DocumentResponse> IngestCoreAsync(string fileName, string contentType, Stream stream, string extension, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        ArgumentNullException.ThrowIfNull(stream);

        var document = new KnowledgeDocument
        {
            FileName = Path.GetFileName(fileName),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            Status = KnowledgeDocumentStatus.Processing
        };

        await _repository.UpsertAsync(document, cancellationToken);

        try
        {
            var extractor = _extractors.FirstOrDefault(x => x.CanExtract(extension));
            if (extractor is null)
            {
                throw new NotSupportedException($"File type '{extension}' is not supported. Use .txt, .md, .html, .htm or .pdf.");
            }

            var extracted = await extractor.ExtractAsync(stream, cancellationToken);
            var normalized = _normalizer.Normalize(extracted);
            if (string.IsNullOrWhiteSpace(normalized)) throw new InvalidOperationException("No readable text was extracted from the document.");

            var chunks = _chunker.CreateChunks(document.Id, normalized, 1200, 180);
            if (chunks.Count == 0) throw new InvalidOperationException("The document did not produce any searchable chunks.");

            foreach (var chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var vector = await _embeddingGenerator.GenerateAsync(chunk.Text, cancellationToken);
                await _vectorStore.UpsertAsync(new VectorRecord(chunk, vector), cancellationToken);
            }

            document.CharacterCount = normalized.Length;
            document.ChunkCount = chunks.Count;
            document.Status = KnowledgeDocumentStatus.Ready;
            document.ProcessedAtUtc = DateTimeOffset.UtcNow;
            await _repository.UpsertAsync(document, cancellationToken);
            return Map(document);
        }
        catch (Exception ex)
        {
            document.Status = KnowledgeDocumentStatus.Failed;
            document.FailureReason = ex.Message;
            await _repository.UpsertAsync(document, cancellationToken);
            throw;
        }
    }

    private static DocumentResponse Map(KnowledgeDocument d) =>
        new(d.Id, d.FileName, d.ContentType, d.Status, d.CharacterCount, d.ChunkCount, d.CreatedAtUtc, d.ProcessedAtUtc);
}
