using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using URTech.EnterpriseRAG.Application.Documents;

/*
File role: HtmlTextExtractor.cs
Layer: Infrastructure
Purpose: Extracts readable text from HTML/HTM documents for the ingestion pipeline.
Business flow: HTML stream -> markup cleanup -> text.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Infrastructure.Extraction;

public sealed class HtmlTextExtractor : IDocumentTextExtractor
{
    private static readonly Regex Tags = new(@"<[^>]+>", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".html", ".htm" };

    public bool CanExtract(string fileExtension) => Extensions.Contains(Normalize(fileExtension));

    public async Task<string> ExtractAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream.CanSeek) stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen: true);
        var html = await reader.ReadToEndAsync(cancellationToken);
        var noTags = Tags.Replace(html, " ");
        return WebUtility.HtmlDecode(noTags);
    }

    private static string Normalize(string extension) => string.IsNullOrWhiteSpace(extension) ? string.Empty : (extension.StartsWith('.') ? extension : $".{extension}");
}
