using System.Diagnostics;

namespace WinzorFramework.JSInterop;

public interface IFileService
{
	public Task<BrowserFile[]> ShowOpenFilePickerAsync(bool allowMultiSelect = false, IEnumerable<Dictionary<string, string[]>>? fileTypes = default);
	public Task<IWritableBrowserFile?> ShowSaveFileDialogAsync(string suggestedFileName, IEnumerable<Dictionary<string, string[]>>? fileTypes = default);
	public Task<IWritableBrowserFile?> SaveFileToDirectoryAsync(string suggestedFileName, IEnumerable<Dictionary<string, string[]>>? fileTypes = default);
	public Task<string[]> UploadFilesToServerAsync(BrowserFile[] files, long maxFileSize, CancellationToken cancellationToken = default);
	public Task<string> ShowDirectoryPickerAsync(string startIn = "desktop", string mode = "readwrite");
	public Task<bool> SaveFileByPathAsync(string fileName, byte[] data);
	public Task<bool> DownloadFileAsync(string fileName);
	public string? AddDownloadObject(IDownloadObject downloadObject);
	public ResultOfGetDownloadObject GetAndRemoveDownloadObject(string objectId,
		out IDownloadObject? downloadObject);
}

public sealed class FileService : IFileService, IDisposable
{
	readonly IFileServiceJSInterop fileServiceInterop;

	public FileService(IFileServiceJSInterop fileServiceInterop, IDownloadObjectManager downloadObjectManager)
	{
		this.fileServiceInterop = fileServiceInterop;
		DownloadObjectManager = downloadObjectManager;
	}

	IDownloadObjectManager DownloadObjectManager { get; set; }

	public string? AddDownloadObject(IDownloadObject downloadObject)
	{
		return DownloadObjectManager.AddDownloadObject(downloadObject);
	}

	public ResultOfGetDownloadObject GetAndRemoveDownloadObject(string objectId,
		out IDownloadObject? downloadObject)
	{
		return DownloadObjectManager.GetAndRemoveDownloadObject(objectId, out downloadObject);
	}

	public async Task<IWritableBrowserFile?> ShowSaveFileDialogAsync(string suggestedFileName, IEnumerable<Dictionary<string, string[]>>? fileTypes = default)
	{
		return await fileServiceInterop.SaveFileDialogAsync(suggestedFileName, fileTypes);
	}

	public async Task<IWritableBrowserFile?> SaveFileToDirectoryAsync(string suggestedFileName, IEnumerable<Dictionary<string, string[]>>? fileTypes = default)
	{
		return await fileServiceInterop.SaveFileToDirectoryAsync(suggestedFileName, fileTypes);
	}

	public Task<BrowserFile[]> ShowOpenFilePickerAsync(bool allowMultiSelect = false, IEnumerable<Dictionary<string, string[]>>? fileTypes = default) => fileServiceInterop.OpenFileDialogAsync(allowMultiSelect, fileTypes);

	public Task<string> ShowDirectoryPickerAsync(string startIn = "desktop", string mode = "readwrite") =>
		fileServiceInterop.OpenDirectoryDialogAsync(startIn, mode);

	public Task<bool> SaveFileByPathAsync(string fileName, byte[] data) => fileServiceInterop.SaveFileByPathAsync(fileName, data);

	public Task<bool> DownloadFileAsync(string fileName)
	{
		return fileServiceInterop.DownloadFileAsync(fileName);
	}

	public async Task<string[]> UploadFilesToServerAsync(BrowserFile[] files, long maxFileSize, CancellationToken cancellationToken = default)
	{
		if (!Directory.Exists(TempDirectoryPath))
		{
			Directory.CreateDirectory(TempDirectoryPath);
		}

		var uploadedFileNames = new List<string>();

		foreach (var file in files)
		{
		// This file naming logic is adapted from ZOpenFileDialog ForceLocalFiles
		var tempFileName = Path.Combine(TempDirectoryPath, Path.GetFileName(file.Name));
		for (var i = 0; File.Exists(tempFileName); i++)
		{
			tempFileName = Path.Combine(TempDirectoryPath, Path.GetFileNameWithoutExtension(file.Name) + "[" + i + "]" + Path.GetExtension(file.Name));
		}

		using var tempFileStream = new FileStream(tempFileName, FileMode.Create);
		if (file.FileStream != null)
		{
			using var browserFileStream = await file.FileStream.OpenReadStreamAsync(maxFileSize, cancellationToken);
			await browserFileStream.CopyToAsync(tempFileStream, cancellationToken);
		}

		await file.DisposeAsync();
			uploadedFileNames.Add(tempFileName);
		}
		return uploadedFileNames.ToArray();
	}

#pragma warning disable EDI011 // CW1 Temp Path Rule
	// We will use the same base temp directory that CW1 uses (.../Temp/WiseTechGlobal/{ProcessID})
	// This means that CW1 TempFileCleanup will cleanup any files that survive
	public static string UploadRoot => Path.Combine(Path.GetTempPath(), "WiseTechGlobal", Process.GetCurrentProcess().Id.ToString(), "WinzorUploads");
	string TempDirectoryPath => Path.Combine(UploadRoot, fileServiceInstancePath);
#pragma warning restore EDI011 // CW1 Temp Path Rule

	public void Dispose()
	{
		try
		{
			if (Directory.Exists(TempDirectoryPath))
			{
				Directory.Delete(TempDirectoryPath, true);
			}
		}
		catch (IOException) { }
		catch (UnauthorizedAccessException) { }
	}

	readonly string fileServiceInstancePath = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
}
