using ChatBot.Application.DTOs;

namespace ChatBot.Application.Interfaces;

public interface IDocumentProcessingService
{
    Task<List<ProcessedChunk>> ProcessAsync(string fileName, string contentType, Stream content);
}
