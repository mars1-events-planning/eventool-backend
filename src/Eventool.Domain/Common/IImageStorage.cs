namespace Eventool.Domain.Common;

public interface IImageStorage
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName);
}