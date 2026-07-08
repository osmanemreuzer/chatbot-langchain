using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatBot.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public ConversationsController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var conversations = await _conversationService.GetConversationsAsync(GetUserId());
        return Ok(conversations);
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation(CreateConversationRequest request)
    {
        var conversation = await _conversationService.CreateConversationAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetConversation(Guid id)
    {
        var conversation = await _conversationService.GetConversationAsync(GetUserId(), id);

        if (conversation is null)
        {
            return NotFound(new { error = "Sohbet bulunamadı." });
        }

        return Ok(conversation);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteConversation(Guid id)
    {
        var deleted = await _conversationService.DeleteConversationAsync(GetUserId(), id);

        if (!deleted)
        {
            return NotFound(new { error = "Sohbet bulunamadı." });
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> AddMessage(Guid id, AddMessageRequest request)
    {
        var message = await _conversationService.AddMessageAsync(GetUserId(), id, request);

        if (message is null)
        {
            return NotFound(new { error = "Sohbet bulunamadı." });
        }

        return CreatedAtAction(nameof(GetConversation), new { id }, message);
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
