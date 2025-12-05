namespace NuitInfo.Rubeus.GastonBergeur.Dtos;

public class ChatMessageDto
{
    public bool IsUser { get; set; }
    public string Content { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
}
