namespace NuitInfo.Rubeus.GastonBergeur.Dtos;

public class ChatResponseDto
{
    public IEnumerable<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
}
