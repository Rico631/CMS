using CMS.Api.Domain.Entities;

namespace CMS.Api.Application.Dtos;

public enum MessageTypes
{
    News,
    Telegram
}

public abstract record MessageDto(string CompanyId, string Id, DateTime Created, List<MessageAttachmentDto> Files)
{
    public static MessageDto Create(MessageBase message) =>
        message switch
        {
            NewsMessage news => NewsMessageDto.Create(news),
            TelegramMessage tg => TelegramMessageDto.Create(tg),
            _ => throw new NotImplementedException()
        };
}

public record MessageAttachmentDto(string Path, string Name, string ContentType, long Size, DateTime Created)
{
    public static MessageAttachmentDto Create(Attachment data) =>
        new(data.Path, data.Name, data.ContentType, data.Size, data.Created);
}

public record NewsMessageDto(string CompanyId, string Id, string Subject, string Body, DateTime Created, List<MessageAttachmentDto> Files)
    : MessageDto(CompanyId, Id, Created, Files)
{
    public static NewsMessageDto Create(NewsMessage message) =>
        new(message.Company.Id, message.Id, message.Subject, message.Body, message.Created, message.Attachments.Select(MessageAttachmentDto.Create).ToList());
}

public record TelegramMessageDto(string CompanyId, string Id, string Body, DateTime Created, List<MessageAttachmentDto> Files)
    : MessageDto(CompanyId, Id, Created, Files)
{
    public static TelegramMessageDto Create(TelegramMessage message) =>
        new(message.Company.Id, message.Id, message.Body, message.Created, message.Attachments.Select(MessageAttachmentDto.Create).ToList());
}


public record CreateMessageDto(MessageTypes Type);