using System.Text.RegularExpressions;

/*
File role: DocumentTextNormalizer.cs
Layer: Application
Purpose: Normalizes extracted text before chunking by reducing horizontal whitespace and excessive blank lines.
Business flow: Raw extracted text -> normalized text -> chunker.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Documents;

public sealed class DocumentTextNormalizer : IDocumentTextNormalizer
{
    private static readonly Regex HorizontalWhitespace = new(@"[\t ]+", RegexOptions.Compiled);
    private static readonly Regex ExcessBlankLines = new(@"(\r?\n){3,}", RegexOptions.Compiled);

    public string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var value = text.Replace("\r\n", "\n").Replace('\r', '\n');
        value = HorizontalWhitespace.Replace(value, " ");
        value = ExcessBlankLines.Replace(value, "\n\n");
        return value.Trim();
    }
}
