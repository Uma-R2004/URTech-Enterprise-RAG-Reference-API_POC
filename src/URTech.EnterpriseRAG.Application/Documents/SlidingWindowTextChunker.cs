/*
File role: SlidingWindowTextChunker.cs
Layer: Application
Purpose: Splits normalized text into overlapping chunks, preferring paragraph/sentence/word boundaries to preserve readable retrieval context.
Business flow: Normalized text -> boundary-aware chunks -> embeddings/vector index.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Documents;

/*
Simple production-friendly baseline chunker.  It prefers paragraph/sentence
boundaries and keeps a configurable overlap so context can survive across
adjacent chunks.
*/
public sealed class SlidingWindowTextChunker : ITextChunker
{
    public IReadOnlyList<DocumentChunk> CreateChunks(Guid documentId, string text, int maxCharacters, int overlapCharacters)
    {
        if (documentId == Guid.Empty) throw new ArgumentException("Document ID is required.", nameof(documentId));
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<DocumentChunk>();
        if (maxCharacters < 200) throw new ArgumentOutOfRangeException(nameof(maxCharacters));
        if (overlapCharacters < 0 || overlapCharacters >= maxCharacters) throw new ArgumentOutOfRangeException(nameof(overlapCharacters));

        var chunks = new List<DocumentChunk>();
        var start = 0;
        var sequence = 0;

        while (start < text.Length)
        {
            var remaining = text.Length - start;
            var length = Math.Min(maxCharacters, remaining);
            var end = start + length;

            if (end < text.Length)
            {
                var minBoundary = start + (int)(maxCharacters * 0.55);
                var search = text.AsSpan(start, length);
                var relativeBoundary = FindBoundary(search, minBoundary - start);
                if (relativeBoundary > 0)
                {
                    end = start + relativeBoundary;
                }
            }

            var chunkText = text[start..end].Trim();
            if (!string.IsNullOrWhiteSpace(chunkText))
            {
                chunks.Add(new DocumentChunk(Guid.NewGuid(), documentId, sequence++, chunkText));
            }

            if (end >= text.Length) break;
            start = Math.Max(end - overlapCharacters, start + 1);
        }

        return chunks;
    }

    private static int FindBoundary(ReadOnlySpan<char> text, int minimumIndex)
    {
        for (var i = text.Length - 1; i >= Math.Max(1, minimumIndex); i--)
        {
            var c = text[i - 1];
            if (c == '\n' || c == '\r') return i;
            if ((c == '.' || c == '?' || c == '!') && i < text.Length && char.IsWhiteSpace(text[i])) return i;
        }

        for (var i = text.Length - 1; i >= Math.Max(1, minimumIndex); i--)
        {
            if (char.IsWhiteSpace(text[i - 1])) return i;
        }

        return text.Length;
    }
}
