using CMS.Api.Domain.Entities.Messages;

namespace CMS.Api.Domain.Entities;

public class NewsMessage : MessageBase, IMessageWithAttachment
{
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public List<Attachment> Attachments { get; set; } = [];
    public static NewsMessage Create(Company company) => new()
    {
        Subject = "",
        Body = "",
        Company = company,
        Created = DateTime.Now,
    };
}
