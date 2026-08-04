using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.Infrastructure.Repositories;

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly AppDbContext _dbContext;

    public SupportTicketRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<SupportTicket>> GetByUserIdAsync(Guid userId)
        => _dbContext.SupportTickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(SupportTicket ticket)
    {
        _dbContext.SupportTickets.Add(ticket);
        await _dbContext.SaveChangesAsync();
    }
}
