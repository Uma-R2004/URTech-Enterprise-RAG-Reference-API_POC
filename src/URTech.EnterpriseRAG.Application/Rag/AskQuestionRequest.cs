/*
File role: AskQuestionRequest.cs
Layer: Application
Purpose: Request contract used to carry validated input into an Application use case.
Business flow: API request -> request record -> Application service.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Application.Rag;

public sealed record AskQuestionRequest(
    string Question,
    Guid? DocumentId = null,
    int TopK = 4,
    double MinimumSimilarityScore = 0.02);
