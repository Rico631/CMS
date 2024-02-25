namespace CMS.Api.Domain.Entities;

public class Attachment
{
    public required string CompanyId { get; set; }
    public required string MessageId { get; set; }
    public required string Path { get; set; }
    public required string Name { get; set; }
    public required string ContentType { get; set; }
    public required long Size { get; set; }
    public required DateTime Created { get; set; }
}
