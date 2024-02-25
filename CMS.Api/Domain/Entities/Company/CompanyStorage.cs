using CMS.Api.Application.Common;

namespace CMS.Api.Domain.Entities;

public class CompanyStorage
{
    public required string BucketName { get; set; }

    public static CompanyStorage Create() => new()
    {
        BucketName = Utils.GenerateId()
    };
}
