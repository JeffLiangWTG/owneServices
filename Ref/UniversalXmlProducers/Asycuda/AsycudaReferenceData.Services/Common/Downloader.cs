using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Services;

public class Downloader
{
	private readonly HttpClient _httpClient;

	public Downloader(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<string> DownloadFileAsync(Uri fileUrl, string destinationFolder)
	{
		var fileName = Path.GetFileName(fileUrl.ToString());
		var destinationPath = Path.Combine(destinationFolder, fileName);
		var fileBytes = await _httpClient.GetByteArrayAsync(fileUrl);
		await File.WriteAllBytesAsync(destinationPath, fileBytes);
		return destinationPath;
	}
}
