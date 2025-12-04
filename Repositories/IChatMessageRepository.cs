using NuitInfo.Rubeus.Modeles;

namespace NuitInfo.Rubeus.Repositories;

public interface IChatMessageRepository
{
    Task<List<ChatMessage>> GetAllAsync();
    Task AddAsync(ChatMessage message);
    Task DeleteAsync(string id);
    Task DeleteAllAsync();
}
