using Microsoft.AspNetCore.Mvc;
using URTech.EnterpriseRAG.Application.Documents;
using URTech.EnterpriseRAG.Shared.Common;

/*
File role: DocumentsController.cs
Layer: API
Purpose: Exposes document-list, text-ingestion, file-upload, and delete endpoints. It delegates business work to IDocumentIngestionService rather than implementing RAG logic in the controller.
Business flow: Client/Swagger -> DocumentsController -> IDocumentIngestionService -> extraction/chunking/embedding/vector persistence.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Api.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentIngestionService _service;
    public DocumentsController(IDocumentIngestionService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentResponse>>>> Get(CancellationToken cancellationToken)
    {
        var documents = await _service.GetDocumentsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DocumentResponse>>.Ok(documents));
    }

    [HttpPost("ingest-text")]
    public async Task<ActionResult<ApiResponse<DocumentResponse>>> IngestText([FromBody] IngestTextRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(ApiResponse<DocumentResponse>.Fail("FileName and Content are required."));

        var result = await _service.IngestTextAsync(request.FileName, request.Content, cancellationToken);
        return Ok(ApiResponse<DocumentResponse>.Ok(result, "Document indexed successfully."));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(15_000_000)]
    public async Task<ActionResult<ApiResponse<DocumentResponse>>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest(ApiResponse<DocumentResponse>.Fail("A non-empty file is required."));
        await using var stream = file.OpenReadStream();
        var result = await _service.IngestFileAsync(file.FileName, file.ContentType, stream, cancellationToken);
        return Ok(ApiResponse<DocumentResponse>.Ok(result, "Document indexed successfully."));
    }

    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> Delete(Guid documentId, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(documentId, cancellationToken);
        return NoContent();
    }
}
