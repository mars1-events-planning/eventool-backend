using Amazon.S3;
using Amazon.S3.Model;
using Eventool.Domain.Common;
using Microsoft.Extensions.Options;

namespace Eventool.Infrastructure.Images;

public record S3Options
{
    public const string Section = "S3Images";
    
    public required string AccessKey { get; init; }

    public required string SecretAccessKey { get; init; }
    
    public required string BucketName { get; init; }
}

public class S3ImageStorage(IOptions<S3Options> options)
    : IImageStorage
{
    private readonly AmazonS3Client _s3Client = new(options.Value.AccessKey, options.Value.SecretAccessKey, new AmazonS3Config()
    {
        ServiceURL = "https://s3.timeweb.cloud"
    });

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        try
        {
            var putRequest = new PutObjectRequest
            {
                InputStream = imageStream,
                BucketName = options.Value.BucketName,
                Key = fileName,
                CannedACL = S3CannedACL.PublicRead // Make the object publicly accessible
            };

            await _s3Client.PutObjectAsync(putRequest);

            return $"https://s3.timeweb.cloud/{options.Value.BucketName}/{fileName}";
        }
        catch (Exception ex)
        {
            // Handle exceptions
            throw new InvalidOperationException("Failed to upload image.", ex);
        }
    }
}