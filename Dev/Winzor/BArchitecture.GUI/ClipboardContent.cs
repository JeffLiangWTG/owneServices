using Microsoft.JSInterop;

namespace WinzorFramework;

public record ClipboardContent(string contentString, IJSStreamReference contentStream)
{
	const int MaxAllowedSizeInJSStreamReference = 1024 * 1024 * 100; // 100MB

	public async Task<string> GetContentAsync()
	{
		if (contentStream != null)
		{
			using var stream = await contentStream.OpenReadStreamAsync(maxAllowedSize: MaxAllowedSizeInJSStreamReference);
			using var reader = new StreamReader(stream);
			return await reader.ReadToEndAsync();
		}

		return contentString;
	}
}
