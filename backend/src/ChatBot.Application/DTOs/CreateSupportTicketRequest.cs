using System.ComponentModel.DataAnnotations;

namespace ChatBot.Application.DTOs;

public class CreateSupportTicketRequest
{
    [Required]
    public Guid ConversationId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;
}
