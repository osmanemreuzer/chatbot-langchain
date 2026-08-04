using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatBot.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/support-tickets")]
public class SupportTicketsController : ControllerBase
{
    private readonly ISupportTicketService _supportTicketService;

    public SupportTicketsController(ISupportTicketService supportTicketService)
    {
        _supportTicketService = supportTicketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets()
    {
        var tickets = await _supportTicketService.GetTicketsAsync(GetUserId());
        return Ok(tickets);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketRequest request)
    {
        var ticket = await _supportTicketService.CreateAsync(
            GetUserId(), request.ConversationId, request.Title, request.Description);

        return CreatedAtAction(nameof(GetTickets), null, ticket);
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
