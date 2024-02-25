using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CMS.Api.Domain.Entities;

[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(NewsMessage), typeof(TelegramMessage))]
public abstract class MessageBase
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public required Company Company { get; set; }
    public required DateTime Created { get; set; }
    public bool Published { get; set; }
}
