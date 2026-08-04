namespace ChatBot.Application.DTOs;

public record SupportTicketToolCall(string Title, string Description);

public record AiReply(string Reply, SupportTicketToolCall? SupportTicketToolCall);
