using CMS.Api.Application.Common;

namespace CMS.Api.Domain.Entities;

public class CompanyDatabase
{
    public required string DatabaseName { get; set; }
    public required string MessageCollectionName { get; set; }

    public static CompanyDatabase Create() => new()
    {
        DatabaseName = Utils.GenerateId(),
        MessageCollectionName = Constants.CompanyDb.CollectionNames.Messages
    };
}
