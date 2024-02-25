using CMS.Api.Domain.Entities;
using ErrorOr;

namespace CMS.Api.Domain.Errors;

public class DomainErrors
{

    public class Common
    {
        public static Error NotImplement(string type) => 
            Error.Failure($"{nameof(Common)}.{nameof(NotImplement)}", $"Not implement '{type}'");
    }
    public class Internal
    {
        public static Error CompanyNotFound(string companyId) =>
            Error.Failure($"{nameof(Internal)}.{nameof(CompanyNotFound)}", $"Company '{companyId}' not found");
    }

    public class Message
    {
        public static Error MessageNotFound(string messageId) =>
            Error.Failure($"{nameof(Message)}.{nameof(MessageNotFound)}", $"Message '{messageId}' not found");

        public static Error MessageAttachmentNotSupported(string messageId) =>
            Error.Failure($"{nameof(Message)}.{nameof(MessageAttachmentNotSupported)}", $"Message '{messageId}' file attachments are not supported");
    }
}
