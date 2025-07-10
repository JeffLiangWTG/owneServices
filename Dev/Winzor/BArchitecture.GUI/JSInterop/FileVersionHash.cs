using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace WinzorFramework.JSInterop;

public interface IFileVersionHash
{
	string Get(string path);
	string GetHash(Stream stream);
}

public class FileVersionHash : IFileVersionHash
{
	readonly IFileProvider fileProvider;

	public FileVersionHash(IWebHostEnvironment webHostEnvironment)
	{
		fileProvider = webHostEnvironment.WebRootFileProvider;
	}

	public string Get(string path)
	{
		var fileInfo = fileProvider?.GetFileInfo(path);
		if (fileInfo is not null && fileInfo.Exists)
		{
			using (var stream = fileInfo.CreateReadStream())
			{
				var hash = GetHash(stream);
				if (!string.IsNullOrEmpty(hash))
				{
					return $"?v={hash}";
				}
			}
		}
		return string.Empty;
	}

	/// <summary>
	/// Calculate the hash of the stream content.
	/// Result is in base64 url encoding.
	/// </summary>
	/// <param name="stream"></param>
	/// <returns>Base64 url encoded hash</returns>
	public string GetHash(Stream stream)
	{
		using (var sha256 = System.Security.Cryptography.SHA256.Create())
		{
			var hash = sha256.ComputeHash(stream);
			return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
		}
	}
}
