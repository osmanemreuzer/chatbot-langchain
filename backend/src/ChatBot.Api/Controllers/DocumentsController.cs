using ChatBot.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatBot.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        var documents = await _documentService.GetDocumentsAsync(GetUserId());
        return Ok(documents);
    }

    [HttpPost]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { error = "Dosya boş olamaz." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var document = await _documentService.UploadAsync(
                GetUserId(), file.FileName, file.ContentType, file.Length, stream);

            return CreatedAtAction(nameof(GetDocuments), null, document);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var deleted = await _documentService.DeleteDocumentAsync(GetUserId(), id);

        if (!deleted)
        {
            return NotFound(new { error = "Doküman bulunamadı." });
        }

        return NoContent();
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
