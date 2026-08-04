using ChatBot.Domain.Entities;

namespace ChatBot.Application.Interfaces;

public interface ISupportTicketRepository
{
    Task<List<SupportTicket>> GetByUserIdAsync(Guid userId);
    Task AddAsync(SupportTicket ticket);
}
