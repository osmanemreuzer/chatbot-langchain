using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;

namespace ChatBot.Application.Services;

public class HealthCheckService : IHealthCheckService
{
    public HealthStatus Check() => new();
}
