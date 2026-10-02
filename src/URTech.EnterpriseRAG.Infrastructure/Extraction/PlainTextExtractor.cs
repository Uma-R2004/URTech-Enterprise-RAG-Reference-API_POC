using System.Text;
using URTech.EnterpriseRAG.Application.Documents;

/*
File role: PlainTextExtractor.cs
Layer: Infrastructure
Purpose: Extracts UTF-8 text from plain-text style document formats used by the reference application.
Business flow: Text-like stream -> string content.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Infrastructure.Extraction;

public sealed class PlainTextExtractor : IDocumentTextExtractor
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".txt", ".md", ".csv", ".json" };

    public bool CanExtract(string fileExtension) => Extensions.Contains(Normalize(fileExtension));

    public async Task<string> ExtractAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (stream.CanSeek) stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string Normalize(string extension) => string.IsNullOrWhiteSpace(extension) ? string.Empty : (extension.StartsWith('.') ? extension : $".{extension}");
}
