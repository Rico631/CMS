namespace CMS.Api.Domain.Models;

public record UploadMessageFile(string CompanyId, string NewsId, string ContentType, long Size, Stream Stream);
