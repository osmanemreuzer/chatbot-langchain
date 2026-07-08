using System.ComponentModel.DataAnnotations;

namespace ChatBot.Application.DTOs;

public class CreateConversationRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }
}
