namespace CMS.Api.Domain.Entities.Messages;

public interface IMessageWithAttachment
{
    public List<Attachment> Attachments { get; set; }
}
