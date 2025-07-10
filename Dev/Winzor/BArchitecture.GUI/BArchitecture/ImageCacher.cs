using WinzorFramework.JSInterop;

namespace WinzorFramework;

public interface IImageCacher
{
	public const string ApiPrefix = "/api/image/";

	/// <summary>
	/// Get uri by base64 image from cache.
	/// </summary>
	public Uri GetImageUri(string base64);

	/// <summary>
	/// Add base64 image into cache.
	/// </summary>
	public Uri AddImage(string base64);

	/// <summary>
	/// Get base64 image by imageId from cache.
	/// </summary>
	/// <param name="imageId">imageId should be a file version string</param>
	public string? GetImageBase64ById(string imageId);
}

public class ImageCacher : IImageCacher
{
	readonly Dictionary<string, string> base64ImageIdDictionary = new ();

	readonly Dictionary<string, string> imageIdBase64Dictionary = new ();

	readonly IFileVersionHash fileVersionHash;

	public ImageCacher(IFileVersionHash fileVersionHash)
	{
		this.fileVersionHash = fileVersionHash;
	}

	public Uri GetImageUri(string base64)
	{
		base64ImageIdDictionary.TryGetValue(base64, out var result);
		return GenerateImageUri(result);
	}

	public Uri AddImage(string base64)
	{
		if (base64ImageIdDictionary.ContainsKey(base64))
		{
			base64ImageIdDictionary.TryGetValue(base64, out var imageId);
			return GenerateImageUri(imageId);
		}
		else
		{
			var byteArray = Convert.FromBase64String(base64);
			using var memoryStream = new MemoryStream(byteArray);
			memoryStream.Position = 0;
			var imageId = fileVersionHash.GetHash(memoryStream);
			base64ImageIdDictionary.Add(base64, imageId);
			imageIdBase64Dictionary.Add(imageId, base64);

			return GenerateImageUri(imageId);
		}
	}

	public string? GetImageBase64ById(string imageId)
	{
		return imageIdBase64Dictionary.GetValueOrDefault(imageId);
	}

	static Uri GenerateImageUri(string? imageId) => new ($"{IImageCacher.ApiPrefix}{imageId}", UriKind.Relative);
}
