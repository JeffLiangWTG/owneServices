using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public interface IFileServiceJSInterop : IJSInterop
{
	Task<bool> DownloadFileAsync(string fileName);
	Task<string> OpenDirectoryDialogAsync(string startIn, string mode);
	Task<BrowserFile[]> OpenFileDialogAsync(bool allowMultiSelect, IEnumerable<Dictionary<string, string[]>>? fileTypes = null);
	Task<bool> SaveFileByPathAsync(string fileName, byte[] data);
	Task<IWritableBrowserFile?> SaveFileDialogAsync(string suggestedFileName, IEnumerable<Dictionary<string, string[]>>? fileTypes);
	Task<IWritableBrowserFile?> SaveFileToDirectoryAsync(string suggestedFileName, IEnumerable<Dictionary<string, string[]>>? fileTypes);
}

public sealed class FileServiceJSInterop : JSInteropBase, IFileServiceJSInterop
{
	public FileServiceJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/fileService.js", fileVersionHash)
	{
	}

	public async Task<BrowserFile[]> OpenFileDialogAsync(bool allowMultiSelect, IEnumerable<Dictionary<string, string[]>>? fileTypes = default)
	{
		return await InvokeJsAsync<BrowserFile[]>("openFileDialog", CancellationToken.None, allowMultiSelect, fileTypes);
	}

	public async Task<IWritableBrowserFile?> SaveFileDialogAsync(string suggestedFileName, IEnumerable<Dictionary<string, string[]>>? fileTypes)
	{
		var result = await InvokeJsAsync<SaveFileDialogResult>("saveFileDialog", CancellationToken.None, suggestedFileName, fileTypes);
		if (!result.cancelled)
		{
			return new WritableBrowserFile(result.fileName, result.fileReference);
		}
		await result.fileReference.DisposeAsync();
		return null;
	}

	public async Task<string> OpenDirectoryDialogAsync(string startIn, string mode)
	{
		return await InvokeJsAsync<string>("OpenDirectoryDialog", CancellationToken.None, startIn, mode);
	}

	public async Task<bool> SaveFileByPathAsync(string fileName, byte[] data)
	{
		return await InvokeJsAsync<bool>("SaveFileByPath", fileName, data);
	}

	public async Task<bool> DownloadFileAsync(string fileName)
	{
		return await InvokeJsAsync<bool>("DownloadFile", fileName);
	}

	public async Task<IWritableBrowserFile?> SaveFileToDirectoryAsync(string suggestedFileName, IEnumerable<Dictionary<string, string[]>>? fileTypes = null)
	{
		var result = await InvokeJsAsync<SaveFileDialogResult>("saveFileToDirectory", CancellationToken.None, suggestedFileName, fileTypes);
		if (!result.cancelled)
		{
			return new WritableBrowserFile(result.fileName, result.fileReference);
		}
		await result.fileReference.DisposeAsync();
		return null;
	}
}

public record SaveFileDialogResult(bool cancelled, string fileName, IJSObjectReference fileReference);
