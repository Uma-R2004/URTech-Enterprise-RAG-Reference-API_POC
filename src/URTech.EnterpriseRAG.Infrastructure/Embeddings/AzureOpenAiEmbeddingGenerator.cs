using System.Net.Http.Json;
using System.Text.Json;
using URTech.EnterpriseRAG.Application.Embeddings;

/*
File role: AzureOpenAiEmbeddingGenerator.cs
Layer: Infrastructure
Purpose: Optional Azure OpenAI embedding adapter. It calls the configured Azure OpenAI embeddings deployment and converts the response into a float vector.
Business flow: Application embedding request -> Azure OpenAI REST API -> embedding vector.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Infrastructure.Embeddings;

public sealed record AzureOpenAiOptions(string Endpoint, string ApiKey, string EmbeddingDeployment, string ChatDeployment, string ApiVersion = "2024-10-21");

/*
Optional Azure OpenAI adapter.  No Azure SDK is required; the reference project
uses the documented REST surface through HttpClient.  Secrets belong in User
Secrets/environment variables, never in source control.
*/
public sealed class AzureOpenAiEmbeddingGenerator : IEmbeddingGenerator
{
    private static readonly HttpClient Client = new();
    private readonly AzureOpenAiOptions _options;

    public AzureOpenAiEmbeddingGenerator(AzureOpenAiOptions options) => _options = options;
    public string ProviderName => "AzureOpenAI";

    public async Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        var uri = $"{_options.Endpoint.TrimEnd('/')}/openai/deployments/{Uri.EscapeDataString(_options.EmbeddingDeployment)}/embeddings?api-version={Uri.EscapeDataString(_options.ApiVersion)}";
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("api-key", _options.ApiKey);
        request.Content = JsonContent.Create(new { input = text });

        using var response = await Client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("data")[0].GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToArray();
    }
}
