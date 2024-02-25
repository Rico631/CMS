using CMS.Api.Domain.Entities.Messages;

namespace CMS.Api.Domain.Entities;

public class TelegramMessage : MessageBase, IMessageWithAttachment
{
    public required string Body { get; set; }
    public List<Attachment> Attachments { get; set; } = [];
}
