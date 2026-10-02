using URTech.EnterpriseRAG.Application.Documents;

/*
File role: DemoDataSeeder.cs
Layer: API
Purpose: Seeds a small enterprise AI-governance document only when the local repository is empty so reviewers can test the RAG flow immediately.
Business flow: Application startup -> check documents -> ingest demo policy -> create chunks/vectors.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Api.Demo;

public static class DemoDataSeeder
{
    private const string DemoPolicy = """
Enterprise AI Governance Policy

AI solutions must be designed with data minimization, least-privilege access, auditable decision paths, and human oversight for high-impact actions.

Before a generative AI application is promoted to production, the engineering team performs architecture review, security review, privacy review, model and prompt evaluation, and business-owner acceptance testing.

Production secrets must be stored in an approved secret store such as Azure Key Vault. API keys, passwords, connection strings, and tokens must never be committed to source control.

Retrieval-Augmented Generation solutions should ground answers in approved enterprise knowledge sources. Retrieved passages should be preserved as citations so users can verify the evidence supporting an answer.

For privileged access requests, the requester's manager provides business approval and the security team validates the requested level of access before provisioning.

AI incidents are logged, triaged, and reviewed. Corrective actions are tracked through completion and significant incidents receive a post-incident review.
""";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var ingestion = scope.ServiceProvider.GetRequiredService<IDocumentIngestionService>();
        var existing = await ingestion.GetDocumentsAsync(cancellationToken);
        if (existing.Count == 0)
        {
            await ingestion.IngestTextAsync("enterprise-ai-governance-policy.txt", DemoPolicy, cancellationToken);
        }
    }
}
