using System.ComponentModel.DataAnnotations;

namespace ChatBot.Application.DTOs;

public class AddMessageRequest
{
    [Required]
    [RegularExpression("user|assistant", ErrorMessage = "Role 'user' veya 'assistant' olmalı.")]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;
}
