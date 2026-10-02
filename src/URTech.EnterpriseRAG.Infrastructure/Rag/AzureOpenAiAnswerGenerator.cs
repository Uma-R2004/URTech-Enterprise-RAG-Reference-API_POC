using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using URTech.EnterpriseRAG.Application.Rag;
using URTech.EnterpriseRAG.Application.Search;
using URTech.EnterpriseRAG.Infrastructure.Embeddings;

/*
File role: AzureOpenAiAnswerGenerator.cs
Layer: Infrastructure
Purpose: Optional Azure OpenAI grounded-answer adapter. It sends the user question plus retrieved source passages to the configured chat deployment.
Business flow: Question + retrieved sources -> Azure OpenAI chat -> grounded answer text.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Infrastructure.Rag;

public sealed class AzureOpenAiAnswerGenerator : IAnswerGenerator
{
    private static readonly HttpClient Client = new();
    private readonly AzureOpenAiOptions _options;

    public AzureOpenAiAnswerGenerator(AzureOpenAiOptions options) => _options = options;
    public string ProviderName => "AzureOpenAI";

    public async Task<string> GenerateAsync(string question, IReadOnlyList<SearchResult> sources, CancellationToken cancellationToken = default)
    {
        var context = new StringBuilder();
        for (var i = 0; i < sources.Count; i++)
        {
            context.AppendLine($"[Source {i + 1}] {sources[i].Text}");
        }

        var uri = $"{_options.Endpoint.TrimEnd('/')}/openai/deployments/{Uri.EscapeDataString(_options.ChatDeployment)}/chat/completions?api-version={Uri.EscapeDataString(_options.ApiVersion)}";
        var payload = new
        {
            temperature = 0.1,
            messages = new object[]
            {
                new { role = "system", content = "You are an enterprise RAG assistant. Answer only from the provided sources. If the sources do not support an answer, say so clearly." },
                new { role = "user", content = $"Question: {question}\n\nSources:\n{context}" }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("api-key", _options.ApiKey);
        request.Content = JsonContent.Create(payload);
        using var response = await Client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }
}
