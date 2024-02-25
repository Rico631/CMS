using CMS.Api.Application.Common;
using CMS.Api.Application.Repositories;
using CMS.Api.Domain.Entities;
using CMS.Api.Domain.Entities.Messages;
using CMS.Api.Domain.Errors;
using CMS.Api.Domain.Models;
using ErrorOr;

namespace CMS.Api.Application.Services;

public interface IMessageService
{
    Task<ErrorOr<MessageBase>> CreateDefaultAsync<T>(string companyId, CancellationToken token = default) where T : MessageBase;

    Task<ErrorOr<List<T>>> GetTypedAsync<T>(string companyId, CancellationToken token = default) where T : MessageBase;
    Task<ErrorOr<T>> GetTypedByIdAsync<T>(string companyId, string id, CancellationToken token = default) where T : MessageBase;
    Task<ErrorOr<Attachment>> AddAttachmentAsync(string companyId, string messageId, string fileName, string contentType, long size, Stream stream, CancellationToken token = default);
}

public class MessageService(
    IInternalRepository internalRepository,
    IMessageRepository messageRepository,
    IStorageRepository storageRepository) : IMessageService
{
    public async Task<ErrorOr<MessageBase>> CreateDefaultAsync<T>(string companyId, CancellationToken token = default) where T : MessageBase
    {
        var company = await GetCompanyAsync(companyId, token);
        if (company.IsError)
            return company.FirstError;

        var message = GetDefaultMessage<T>(company.Value, token);

        return await messageRepository.CreateMessage(company.Value, message);
    }

    private T GetDefaultMessage<T>(Company company, CancellationToken token = default) where T : MessageBase
    {
        if (typeof(T) == typeof(NewsMessage))
            return NewsMessage.Create(company) as T;

        throw new NotImplementedException(typeof(T).Name);
    }

    public async Task<ErrorOr<List<T>>> GetTypedAsync<T>(string companyId, CancellationToken token = default) where T : MessageBase
    {
        var company = await GetCompanyAsync(companyId, token);
        if (company.IsError)
            return company.FirstError;

        var result = await messageRepository.GetMessagesAsync<T>(company.Value, token);
        return result;
    }

    public async Task<ErrorOr<T>> GetTypedByIdAsync<T>(string companyId, string id, CancellationToken token = default) where T : MessageBase
    {
        var company = await GetCompanyAsync(companyId, token);
        if (company.IsError)
            return company.FirstError;

        return await messageRepository.GetMessageByIdAsync<T>(company.Value, id, token);
    }



    public async Task<ErrorOr<Attachment>> AddAttachmentAsync(string companyId, string messageId, string fileName, string contentType, long size, Stream stream, CancellationToken token = default)
    {
        var company = await GetCompanyAsync(companyId, token);
        if (company.IsError)
            return company.FirstError;

        var message = await messageRepository.GetMessageByIdAsync<MessageBase>(company.Value, messageId, token);

        if (message is null)
            return DomainErrors.Message.MessageNotFound(messageId);

        if(message is not IMessageWithAttachment messageWithAttachments)
            return DomainErrors.Message.MessageAttachmentNotSupported(messageId);

        var fileId = Utils.GenerateId();
        Attachment attachment = new () 
        { 
            CompanyId = company.Value.Id,
            MessageId = message.Id,
            ContentType = contentType, 
            Size = size, 
            Created = DateTime.Now, 
            Name = fileName, 
            Path = $"{message.Id}/{fileId}" 
        };

        UploadAttachment uploadFile = new(company.Value.Storage.BucketName, attachment, stream);
        var file = await storageRepository.UploadAttachmentAsync(uploadFile, token);

        messageWithAttachments.Attachments.Add(attachment);

        await messageRepository.UpdateMessageAsync(company.Value, message, token);

        return attachment;
    }


    private async Task<ErrorOr<Company>> GetCompanyAsync(string companyId, CancellationToken token = default)
    {
        var company = await internalRepository.GetCompanyByIdAsync(companyId, token);
        if (company is null)
            return DomainErrors.Internal.CompanyNotFound(companyId);
        return company;
    }
}
