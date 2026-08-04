using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;

namespace ChatBot.Application.Services;

public class SupportTicketService : ISupportTicketService
{
    private readonly ISupportTicketRepository _supportTicketRepository;

    public SupportTicketService(ISupportTicketRepository supportTicketRepository)
    {
        _supportTicketRepository = supportTicketRepository;
    }

    public async Task<List<SupportTicketDto>> GetTicketsAsync(Guid userId)
    {
        var tickets = await _supportTicketRepository.GetByUserIdAsync(userId);

        return tickets
            .Select(t => new SupportTicketDto(t.Id, t.ConversationId, t.Title, t.Description, t.Status, t.CreatedAt))
            .ToList();
    }

    public async Task<SupportTicketDto> CreateAsync(Guid userId, Guid conversationId, string title, string description)
    {
        var ticket = new SupportTicket
        {
            UserId = userId,
            ConversationId = conversationId,
            Title = title,
            Description = description
        };

        await _supportTicketRepository.AddAsync(ticket);

        return new SupportTicketDto(ticket.Id, ticket.ConversationId, ticket.Title, ticket.Description, ticket.Status, ticket.CreatedAt);
    }
}
