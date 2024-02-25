using CMS.Api.Domain.Entities;

namespace CMS.Api.Domain.Models;

public record UploadAttachment(string BucketName, Attachment Attachment, Stream Stream);
