using MongoDB.Driver;
using NuitInfo.Rubeus.Modeles;
using NuitInfo.Rubeus.Repositories;
using Xunit;

namespace NuitInfo.Rubeus.Tests.Connexions;

public class TestConnexionMongo : IAsyncLifetime
{
    private readonly string _mongoConnectionString;
    private readonly ChatMessageRepository _repository;
    private readonly IMongoDatabase _database;

    public TestConnexionMongo()
    {
        // Récupère la variable d'environnement
        _mongoConnectionString = Environment.GetEnvironmentVariable("AUTH_STRING_MONGO") 
            ?? throw new InvalidOperationException("AUTH_STRING_MONGO n'est pas définie");

        var mongoUrl = new MongoUrl(_mongoConnectionString);
        var client = new MongoClient(mongoUrl);
        _database = client.GetDatabase(mongoUrl.DatabaseName);
        _repository = new ChatMessageRepository(_database);
    }

    // 🧹 Exécuté AVANT chaque test
    public async Task InitializeAsync()
    {
        await _repository.DeleteAllAsync();
    }

    // 🧹 Exécuté APRÈS chaque test
    public async Task DisposeAsync()
    {
        await _repository.DeleteAllAsync();
    }

    [Fact]
    public async Task TestMongoConnexion()
    {
        // Act : on teste une opération simple (ping)
        var pingCommand = new MongoDB.Bson.BsonDocument("ping", 1);
        var result = await _database.RunCommandAsync<MongoDB.Bson.BsonDocument>(pingCommand);

        // Assert : le serveur répond
        Assert.NotNull(result);
        Assert.Equal(1.0, result["ok"].AsDouble);
    }

    [Fact]
    public async Task TestChatMessageRepository_GetAllAsync_ReturnsEmptyList()
    {
        // Act
        var messages = await _repository.GetAllAsync();

        // Assert : la collection est vide (cleanup avant le test)
        Assert.NotNull(messages);
        Assert.Empty(messages);
    }

    [Fact]
    public async Task TestAjoutMessage()
    {
        // Arrange
        var testMessage = new ChatMessage
        {
            Author = "Test_" + Guid.NewGuid().ToString("N")[..8],
            Text = "Message de test unitaire"
        };

        // Act
        await _repository.AddAsync(testMessage);
        var messages = await _repository.GetAllAsync();

        // Assert
        Assert.Single(messages);
        Assert.Contains(messages, m => m.Author == testMessage.Author);
    }

    [Fact]
    public async Task TestSuppressionMessage()
    {
        // Arrange : ajoute un message
        var testMessage = new ChatMessage
        {
            Author = "ToDelete",
            Text = "Ce message sera supprimé"
        };
        await _repository.AddAsync(testMessage);

        var messagesAvant = await _repository.GetAllAsync();
        var messageId = messagesAvant.First().Id;

        // Act : supprime le message
        await _repository.DeleteAsync(messageId);

        // Assert : plus de messages
        var messagesApres = await _repository.GetAllAsync();
        Assert.Empty(messagesApres);
    }
}
