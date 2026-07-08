using ChatBot.Domain.Entities;

namespace ChatBot.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
