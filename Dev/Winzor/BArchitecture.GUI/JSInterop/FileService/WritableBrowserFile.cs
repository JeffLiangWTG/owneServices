using Microsoft.JSInterop;
using WinzorFramework.Extensions;

namespace WinzorFramework.JSInterop;

public interface IWritableBrowserFile : IAsyncDisposable
{
	public Task WriteAsync(Stream fileData);

	public string FileName { get; }
}

public sealed class WritableBrowserFile : IWritableBrowserFile
{
	internal WritableBrowserFile(string fileName, IJSObjectReference jsObjectReference)
	{
		FileName = fileName;
		this.jsObjectReference = jsObjectReference;
	}

	readonly IJSObjectReference jsObjectReference;

	public async Task WriteAsync(Stream fileData)
	{
		await fileData.FlushAsync();
		fileData.Seek(0, SeekOrigin.Begin);
		using var fileDataStream = new DotNetStreamReference(stream: fileData);
		await jsObjectReference.InvokeVoidAsync("write", fileDataStream);
	}

	public string FileName { get; }

	public async ValueTask DisposeAsync()
	{
		await ExceptionHandlerExtension.HandleJSExceptionAsync(async () =>
		{
			await jsObjectReference.DisposeAsync();
		});
	}
}
