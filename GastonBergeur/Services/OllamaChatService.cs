using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NuitInfo.Rubeus.GastonBergeur.Dtos;

namespace NuitInfo.Rubeus.GastonBergeur.Services;

public class OllamaChatService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaChatService> _logger;

    public OllamaChatService(HttpClient httpClient, ILogger<OllamaChatService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ChatResponseDto> SendAsync(ChatRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var ollamaRequest = new OllamaRequest
            {
                Model = "chat-rlatan:latest",
                Messages = new[]
                {
                    new OllamaMessage
                    {
                        Role = "user",
                        Content = request.Message
                    }
                },
                Stream = false
            };

            var json = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to Ollama for session {SessionId}", request.SessionId);

            var response = await _httpClient.PostAsync("/api/chat", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseJson);

            if (ollamaResponse?.Message == null)
            {
                throw new InvalidOperationException("Invalid response from Ollama");
            }

            var messages = new List<ChatMessageDto>
            {
                new ChatMessageDto
                {
                    IsUser = false,
                    Content = ollamaResponse.Message.Content,
                    Timestamp = DateTimeOffset.UtcNow
                }
            };

            return new ChatResponseDto { Messages = messages };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama API for session {SessionId}", request.SessionId);
            throw;
        }
    }

    private class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = default!;

        [JsonPropertyName("messages")]
        public OllamaMessage[] Messages { get; set; } = default!;

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    private class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = default!;

        [JsonPropertyName("content")]
        public string Content { get; set; } = default!;
    }

    private class OllamaResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}
