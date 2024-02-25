using CMS.Api.Domain.Entities;
using MongoDB.Driver;

namespace CMS.Api.Application.Repositories;

public interface IMessageRepository
{
    Task<T> CreateMessage<T>(Company company, T message, CancellationToken token = default) where T : MessageBase;
    Task<List<T>> GetMessagesAsync<T>(Company company, CancellationToken token = default) where T : MessageBase;
    Task<T> GetMessageByIdAsync<T>(Company company, string id, CancellationToken token = default) where T : MessageBase;
    Task<T> UpdateMessageAsync<T>(Company company, T message, CancellationToken token = default) where T : MessageBase;
}

public class MessageRepository(IMongoClient client) : IMessageRepository
{
    public async Task<T> CreateMessage<T>(Company company, T message, CancellationToken token = default) where T : MessageBase
    {
        var collection = GetCollectionMessage<T>(company);
        await collection.InsertOneAsync(message, token);

        return message;
    }

    public async Task<List<T>> GetMessagesAsync<T>(Company company, CancellationToken token = default) where T : MessageBase
    {
        var collection = GetCollectionMessage<T>(company);
        var result = await collection.FindAsync(_ => true, cancellationToken: token);

        return await result.ToListAsync(token);
    }

    public async Task<T> GetMessageByIdAsync<T>(Company company, string id, CancellationToken token = default) where T : MessageBase
    {
        var collection = GetCollectionMessage<T>(company);
        var filter = Builders<T>.Filter.Eq("Id", id);
        var result = await collection.FindAsync(filter, cancellationToken: token);

        return await result.FirstOrDefaultAsync(token);
    }

    public async Task<T> UpdateMessageAsync<T>(Company company, T message, CancellationToken token = default) where T : MessageBase
    {
        var collection = GetCollectionMessage<T>(company);
        var filter = Builders<T>.Filter.Eq("Id", message.Id);

        var result = await collection.ReplaceOneAsync(filter, message, new ReplaceOptions { IsUpsert = true }, token);

        return await GetMessageByIdAsync<T>(company, message.Id, token);
    }

    private IMongoCollection<T> GetCollectionMessage<T>(Company company) where T : MessageBase =>
        client.GetDatabase(company.Database.DatabaseName)
              .GetCollection<T>(company.Database.MessageCollectionName);
}
