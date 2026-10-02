using System.Text.RegularExpressions;
using URTech.EnterpriseRAG.Application.Rag;
using URTech.EnterpriseRAG.Application.Search;

/*
File role: LocalGroundedAnswerGenerator.cs
Layer: Infrastructure
Purpose: Local extractive answer generator used when no cloud credentials are configured. It selects relevant sentences from retrieved chunks instead of calling an external model.
Business flow: Question + retrieved chunks -> token relevance -> extractive grounded answer.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Infrastructure.Rag;

public sealed class LocalGroundedAnswerGenerator : IAnswerGenerator
{
    private static readonly Regex SentenceSplit = new(@"(?<=[.!?])\s+|\r?\n+", RegexOptions.Compiled);
    private static readonly Regex WordRegex = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled);
    private static readonly HashSet<string> Stop = new(StringComparer.OrdinalIgnoreCase)
    {
        "a","an","and","are","as","at","be","by","for","from","how","in","is","it","of","on","or","that","the","this","to","was","what","when","where","which","who","why","with","you","your"
    };

    public string ProviderName => "LocalGroundedExtractive";

    public Task<string> GenerateAsync(string question, IReadOnlyList<SearchResult> sources, CancellationToken cancellationToken = default)
    {
        var terms = ImportantTerms(question);
        var best = sources
            .SelectMany(source => SentenceSplit.Split(source.Text)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(sentence => new { Sentence = sentence.Trim(), Score = Score(sentence, terms) + source.SimilarityScore }))
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        var answer = best?.Sentence;
        if (string.IsNullOrWhiteSpace(answer)) answer = sources[0].Text.Trim();
        return Task.FromResult(answer);
    }

    private static string[] ImportantTerms(string text) =>
        WordRegex.Matches(text.ToLowerInvariant()).Select(x => Stem(x.Value)).Where(x => x.Length > 2 && !Stop.Contains(x)).Distinct().ToArray();

    private static double Score(string sentence, string[] terms)
    {
        var words = WordRegex.Matches(sentence.ToLowerInvariant()).Select(x => Stem(x.Value)).ToHashSet();
        return terms.Sum(t => words.Contains(t) ? 2d : words.Any(w => w.StartsWith(t, StringComparison.Ordinal) || t.StartsWith(w, StringComparison.Ordinal)) ? 1d : 0d);
    }

    private static string Stem(string word)
    {
        foreach (var suffix in new[] { "ing", "ed", "es", "s" })
        {
            if (word.Length > suffix.Length + 3 && word.EndsWith(suffix, StringComparison.Ordinal)) return word[..^suffix.Length];
        }
        return word;
    }
}
