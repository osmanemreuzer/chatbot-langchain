namespace ChatBot.Application.Interfaces;

public interface IAIService
{
    Task<string> GetReplyAsync(string message);
}
