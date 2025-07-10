using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public class BrowserFile : IAsyncDisposable
{
	public string Name { get; set; } = string.Empty;

	public DateTimeOffset LastModified { get; set; }

	public long Size
	{
		get => size;
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(Size), $"Size must be a non-negative value. Value provided: {value}.");
			}

			size = value;
		}
	}
	long size;

	public string ContentType { get; set; } = string.Empty;

	public IJSStreamReference? FileStream { get; set; }

	public async ValueTask DisposeAsync()
	{
		if (FileStream is not null)
		{
			await FileStream.DisposeAsync();
		}
	}
}
