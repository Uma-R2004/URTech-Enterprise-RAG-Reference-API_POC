using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using URTech.EnterpriseRAG.Application.Embeddings;

/*
File role: LocalHashEmbeddingGenerator.cs
Layer: Infrastructure
Purpose: Deterministic local embedding implementation for zero-secret demos and automated tests. It creates a repeatable feature vector from normalized tokens.
Business flow: Text -> token normalization/hashing -> deterministic vector.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Infrastructure.Embeddings;

/*
Deterministic local embedding baseline.  It is intentionally dependency-free
so the repository works immediately.  It hashes normalized terms into a fixed
vector, then L2-normalizes the vector.  Azure OpenAI can be enabled through
configuration for production-style semantic embeddings.
*/
public sealed class LocalHashEmbeddingGenerator : IEmbeddingGenerator
{
    private const int Dimensions = 384;
    private static readonly Regex WordRegex = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled);

    public string ProviderName => "LocalHash384";

    public Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text is required.", nameof(text));
        var vector = new float[Dimensions];

        foreach (Match match in WordRegex.Matches(text.ToLowerInvariant()))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var token = match.Value;
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            var index = (int)(BitConverter.ToUInt32(bytes, 0) % Dimensions);
            var sign = (bytes[4] & 1) == 0 ? 1f : -1f;
            vector[index] += sign;
        }

        Normalize(vector);
        return Task.FromResult(vector);
    }

    private static void Normalize(float[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(x => x * x));
        if (magnitude <= 0) return;
        for (var i = 0; i < vector.Length; i++) vector[i] = (float)(vector[i] / magnitude);
    }
}
