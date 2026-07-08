using ChatBot.Domain.Entities;

namespace ChatBot.Application.Interfaces;

public interface IHealthCheckService
{
    HealthStatus Check();
}
