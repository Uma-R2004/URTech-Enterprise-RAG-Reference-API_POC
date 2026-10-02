using URTech.EnterpriseRAG.Api.Demo;
using URTech.EnterpriseRAG.Application.Documents;
using URTech.EnterpriseRAG.Application.Embeddings;
using URTech.EnterpriseRAG.Application.Rag;
using URTech.EnterpriseRAG.Application.Search;
using URTech.EnterpriseRAG.Infrastructure.Embeddings;
using URTech.EnterpriseRAG.Infrastructure.Extraction;
using URTech.EnterpriseRAG.Infrastructure.Rag;
using URTech.EnterpriseRAG.Persistence.Documents;
using URTech.EnterpriseRAG.Persistence.Search;

/*
File role: Program.cs
Layer: API
Purpose: Application composition root. Registers the layered services, selects Local or Azure OpenAI providers from configuration, enables Swagger, maps controllers, and seeds demo knowledge.
Business flow: HTTP request -> controller -> Application service -> Infrastructure/Persistence adapter -> response.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "URTech Enterprise RAG Reference API", Version = "v1" });
});

var dataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "documents.json");
var vectorPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "vectors.json");
builder.Services.AddSingleton<IDocumentRepository>(_ => new JsonDocumentRepository(dataPath));
builder.Services.AddSingleton<IVectorStore>(_ => new JsonVectorStore(vectorPath));
builder.Services.AddSingleton<IDocumentTextNormalizer, DocumentTextNormalizer>();
builder.Services.AddSingleton<ITextChunker, SlidingWindowTextChunker>();
builder.Services.AddSingleton<IDocumentTextExtractor, PlainTextExtractor>();
builder.Services.AddSingleton<IDocumentTextExtractor, HtmlTextExtractor>();
builder.Services.AddSingleton<IDocumentTextExtractor, PdfTextExtractor>();

var embeddingProvider = builder.Configuration["Rag:EmbeddingProvider"] ?? "Local";
var answerProvider = builder.Configuration["Rag:AnswerProvider"] ?? "Local";

if (embeddingProvider.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase) ||
    answerProvider.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
{
    var azure = new AzureOpenAiOptions(
        builder.Configuration["AzureOpenAI:Endpoint"] ?? string.Empty,
        builder.Configuration["AzureOpenAI:ApiKey"] ?? string.Empty,
        builder.Configuration["AzureOpenAI:EmbeddingDeployment"] ?? string.Empty,
        builder.Configuration["AzureOpenAI:ChatDeployment"] ?? string.Empty,
        builder.Configuration["AzureOpenAI:ApiVersion"] ?? "2024-10-21");

    if (string.IsNullOrWhiteSpace(azure.Endpoint) || string.IsNullOrWhiteSpace(azure.ApiKey))
        throw new InvalidOperationException("Azure OpenAI is enabled but Endpoint/ApiKey configuration is missing. Use .NET User Secrets or environment variables.");

    if (embeddingProvider.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
        builder.Services.AddSingleton<IEmbeddingGenerator>(new AzureOpenAiEmbeddingGenerator(azure));
    else
        builder.Services.AddSingleton<IEmbeddingGenerator, LocalHashEmbeddingGenerator>();

    if (answerProvider.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
        builder.Services.AddSingleton<IAnswerGenerator>(new AzureOpenAiAnswerGenerator(azure));
    else
        builder.Services.AddSingleton<IAnswerGenerator, LocalGroundedAnswerGenerator>();
}
else
{
    builder.Services.AddSingleton<IEmbeddingGenerator, LocalHashEmbeddingGenerator>();
    builder.Services.AddSingleton<IAnswerGenerator, LocalGroundedAnswerGenerator>();
}

builder.Services.AddSingleton<IDocumentIngestionService, DocumentIngestionService>();
builder.Services.AddSingleton<IRagService, RagService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

await DemoDataSeeder.SeedAsync(app.Services);
await app.RunAsync();

public partial class Program { }
