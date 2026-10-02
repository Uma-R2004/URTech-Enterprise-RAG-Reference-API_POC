using Microsoft.AspNetCore.Mvc;
using URTech.EnterpriseRAG.Application.Rag;
using URTech.EnterpriseRAG.Shared.Common;

/*
File role: RagController.cs
Layer: API
Purpose: Exposes the question-answer endpoint for retrieval-augmented generation. It validates the request and delegates orchestration to IRagService.
Business flow: Client question -> RagController -> IRagService -> embedding -> vector retrieval -> grounded answer -> sources.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Api.Controllers;

[ApiController]
[Route("api/rag")]
public sealed class RagController : ControllerBase
{
    private readonly IRagService _ragService;
    public RagController(IRagService ragService) => _ragService = ragService;

    [HttpPost("ask")]
    public async Task<ActionResult<ApiResponse<RagAnswer>>> Ask([FromBody] AskQuestionRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question)) return BadRequest(ApiResponse<RagAnswer>.Fail("Question is required."));
        var result = await _ragService.AskAsync(request, cancellationToken);
        return Ok(ApiResponse<RagAnswer>.Ok(result));
    }
}
