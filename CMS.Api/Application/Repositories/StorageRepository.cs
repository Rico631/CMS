using CMS.Api.Domain.Models;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Tags;
using System.Security.AccessControl;
using System;

namespace CMS.Api.Application.Repositories;

public interface IStorageRepository
{
    Task<StorageFile> UploadAttachmentAsync(UploadAttachment attachment, CancellationToken token = default);
    Task<ReleasableFileStreamModel?> GetFileAsync(string bucketName, string filePath, CancellationToken token = default);
}

public class StorageRepository(IMinioClient minio) : IStorageRepository
{
    public async Task<StorageFile> UploadAttachmentAsync(UploadAttachment uploadFile, CancellationToken token = default)
    {
        await CheckBucketOrCreateAsync(minio, uploadFile.BucketName, token);

        Tagging tags = new(
            new Dictionary<string, string>
            {
                { "CompanyId", uploadFile.Attachment.CompanyId },
                { "MessageId", uploadFile.Attachment.MessageId },
                { "FileName", uploadFile.Attachment.Name },
            }, true);

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(uploadFile.BucketName)
            .WithObject(uploadFile.Attachment.Path)
            .WithStreamData(uploadFile.Stream)
            .WithObjectSize(uploadFile.Attachment.Size)
            .WithContentType(uploadFile.Attachment.ContentType)
            .WithTagging(tags);

        var result = await minio.PutObjectAsync(putObjectArgs, token);

        return new StorageFile(result.ObjectName, result.Size);
    }

    public async Task<ReleasableFileStreamModel?> GetFileAsync(string bucketName, string filePath, CancellationToken token = default)
    {
        await CheckBucketOrCreateAsync(minio, bucketName, token);

        var statArgs = new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(filePath);
        var stat = await minio.StatObjectAsync(statArgs, token);

        if (stat is null)
            return null;

        var getTags = new GetObjectTagsArgs()
                    .WithBucket(bucketName)
                    .WithObject(filePath);
        var tags = await minio.GetObjectTagsAsync(getTags, token);

        var res = new ReleasableFileStreamModel
        {
            ContentType = stat.ContentType,
            FileName = tags.Tags.TryGetValue("FileName", out string? value) ? value : stat.ObjectName,
        };

        var getArgs = new GetObjectArgs()
            .WithObject(filePath)
            .WithBucket(bucketName)
            .WithCallbackStream(res.SetStreamAsync);

        await res.HandleAsync(minio.GetObjectAsync(getArgs,token));

        return res;
    }

    private async Task CheckBucketOrCreateAsync(IMinioClient minio, string bucketName, CancellationToken token = default)
    {
        var beArgs = new BucketExistsArgs().WithBucket(bucketName);
        bool found = await minio.BucketExistsAsync(beArgs, token);

        if (!found)
        {
            var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
            await minio.MakeBucketAsync(mbArgs, token);

            SetBucketTagsArgs args = new SetBucketTagsArgs()
                                         .WithBucket(bucketName);
            await minio.SetBucketTagsAsync(args, token);
        }
    }

}
