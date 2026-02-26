namespace TeamManagementSystem.Web.Services.Interfaces;

public interface IFileStorageService
{
    Task<(string StoredFileName, string ContentType, long SizeBytes)> SaveAsync(Stream fileStream, string originalFileName);
    Task<Stream?> GetAsync(string storedFileName);
    Task<bool> DeleteAsync(string storedFileName);
    string GetContentType(string storedFileName);
}
