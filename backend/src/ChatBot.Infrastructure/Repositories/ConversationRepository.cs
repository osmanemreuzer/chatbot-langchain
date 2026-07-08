using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _dbContext;

    public ConversationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Conversation>> GetByUserIdAsync(Guid userId)
        => _dbContext.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

    public Task<Conversation?> GetByIdAsync(Guid id)
        => _dbContext.Conversations.FirstOrDefaultAsync(c => c.Id == id);

    public Task<Conversation?> GetByIdWithMessagesAsync(Guid id)
        => _dbContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Conversation conversation)
    {
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AddMessageAsync(Message message)
    {
        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Conversation conversation)
    {
        _dbContext.Conversations.Remove(conversation);
        await _dbContext.SaveChangesAsync();
    }
}
