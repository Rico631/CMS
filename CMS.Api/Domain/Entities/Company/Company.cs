using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CMS.Api.Domain.Entities;

public class Company
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public required string CompanyName { get; set; }
    public required DateTime Created { get; set; }
    public required CompanyStorage Storage { get; set; }
    public required CompanyDatabase Database { get; set; }

    public static Company Create(string companyName) => new()
    {
        CompanyName = companyName,
        Created = DateTime.Now,
        Database = CompanyDatabase.Create(),
        Storage = CompanyStorage.Create(),
    };
}
