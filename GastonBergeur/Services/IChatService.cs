using NuitInfo.Rubeus.GastonBergeur.Dtos;

namespace NuitInfo.Rubeus.GastonBergeur.Services;

public interface IChatService
{
    Task<ChatResponseDto> SendAsync(ChatRequestDto request, CancellationToken cancellationToken = default);
}
