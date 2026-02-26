using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace TeamManagementSystem.Web.Services;

public class FileStorageOptions
{
    public string LocalRootPath { get; set; } = "wwwroot/uploads";
}

public class FileStorageService : Interfaces.IFileStorageService
{
    private readonly string _rootPath;
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env, IOptions<FileStorageOptions>? options = null)
    {
        _env = env;
        var path = options?.Value?.LocalRootPath ?? "wwwroot/uploads";
        _rootPath = Path.IsPathRooted(path) ? path : Path.Combine(_env.ContentRootPath, path);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<(string StoredFileName, string ContentType, long SizeBytes)> SaveAsync(Stream fileStream, string originalFileName)
    {
        var ext = Path.GetExtension(originalFileName);
        var stored = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(_rootPath, stored);
        await using var fs = File.Create(fullPath);
        await fileStream.CopyToAsync(fs);
        var contentType = GetContentTypeFromExtension(ext);
        return (stored, contentType, fs.Length);
    }

    public Task<Stream?> GetAsync(string storedFileName)
    {
        var fullPath = Path.Combine(_rootPath, storedFileName);
        if (!File.Exists(fullPath)) return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public Task<bool> DeleteAsync(string storedFileName)
    {
        var fullPath = Path.Combine(_rootPath, storedFileName);
        if (!File.Exists(fullPath)) return Task.FromResult(false);
        try { File.Delete(fullPath); return Task.FromResult(true); }
        catch { return Task.FromResult(false); }
    }

    public string GetContentType(string storedFileName)
    {
        var ext = Path.GetExtension(storedFileName);
        return GetContentTypeFromExtension(ext);
    }

    private static string GetContentTypeFromExtension(string ext)
    {
        return ext.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
