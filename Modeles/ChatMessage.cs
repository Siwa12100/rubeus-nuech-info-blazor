using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NuitInfo.Rubeus.Modeles;


public class ChatMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Author { get; set; } = default!;
    public string Text { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
