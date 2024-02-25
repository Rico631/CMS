using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CMS.Api.Domain.Entities;

public class PublishedMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public required MessageBase Message { get; set; }
    public required DateTime Created { get; set; }
}
