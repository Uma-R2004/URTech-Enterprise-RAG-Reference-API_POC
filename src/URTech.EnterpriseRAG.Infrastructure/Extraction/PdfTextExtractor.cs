using System.Text;
using UglyToad.PdfPig;
using URTech.EnterpriseRAG.Application.Documents;

/*
File role: PdfTextExtractor.cs
Layer: Infrastructure
Purpose: Extracts text from text-based PDF files using PdfPig. Scanned/image-only PDFs are intentionally documented as an OCR extension point.
Business flow: PDF stream -> page text extraction -> combined text.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Infrastructure.Extraction;

public sealed class PdfTextExtractor : IDocumentTextExtractor
{
    public bool CanExtract(string fileExtension) => string.Equals(Normalize(fileExtension), ".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        Stream working = stream;
        MemoryStream? owned = null;

        if (!stream.CanSeek)
        {
            owned = new MemoryStream();
            stream.CopyTo(owned);
            owned.Position = 0;
            working = owned;
        }
        else
        {
            stream.Position = 0;
        }

        try
        {
            using var document = PdfDocument.Open(working);
            var builder = new StringBuilder();
            for (var pageNumber = 1; pageNumber <= document.NumberOfPages; pageNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var pageText = document.GetPage(pageNumber).Text;
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    builder.AppendLine($"--- Page {pageNumber} ---");
                    builder.AppendLine(pageText.Trim());
                    builder.AppendLine();
                }
            }

            var result = builder.ToString().Trim();
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException("The PDF contains no machine-readable text. A scanned PDF requires OCR, such as Azure AI Document Intelligence.");
            }
            return Task.FromResult(result);
        }
        finally
        {
            owned?.Dispose();
        }
    }

    private static string Normalize(string extension) => string.IsNullOrWhiteSpace(extension) ? string.Empty : (extension.StartsWith('.') ? extension : $".{extension}");
}
