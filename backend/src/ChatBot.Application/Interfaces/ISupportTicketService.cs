using ChatBot.Application.DTOs;

namespace ChatBot.Application.Interfaces;

public interface ISupportTicketService
{
    Task<List<SupportTicketDto>> GetTicketsAsync(Guid userId);
    Task<SupportTicketDto> CreateAsync(Guid userId, Guid conversationId, string title, string description);
}
