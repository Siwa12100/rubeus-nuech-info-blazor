using MongoDB.Driver;
using NuitInfo.Rubeus.Modeles;

namespace NuitInfo.Rubeus.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly IMongoCollection<ChatMessage> _collection;

    public ChatMessageRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ChatMessage>("ChatMessages");
    }

    public async Task<List<ChatMessage>> GetAllAsync()
    {
        return await _collection
            .Find(Builders<ChatMessage>.Filter.Empty)
            .SortByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(ChatMessage message)
    {
        message.CreatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(message);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(m => m.Id == id);
    }

    public async Task DeleteAllAsync()
    {
        await _collection.DeleteManyAsync(Builders<ChatMessage>.Filter.Empty);
    }
}
