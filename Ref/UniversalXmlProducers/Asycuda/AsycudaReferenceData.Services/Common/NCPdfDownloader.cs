using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Services;

public class NCPdfDownloader
{
	private readonly HttpClient _httpClient;
	private readonly Downloader _downloader;

	public NCPdfDownloader(HttpClient httpClient)
	{
		_httpClient = httpClient;
		_downloader = new Downloader(_httpClient);
	}

	public async Task<string> DownloadLatestPdf(Uri url, string destinationFolder)
	{
		try
		{
			var htmlContent = await _httpClient.GetStringAsync(url);

			var nodes = HtmlParser.FindNodes(htmlContent, node =>
				node.Name == "a" && node.GetAttributeValue("href", "")
					.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
			if (nodes == null || !nodes.Any())
			{
				Console.WriteLine("No Exchange Rate PDF files were found on the provided page.");
				return null;
			}

			var downloadUrl = nodes
				.Select(node => node.GetAttributeValue("href", string.Empty))
				.ToList()
				.Last();

			return await _downloader.DownloadFileAsync(new Uri(downloadUrl), destinationFolder);
		}
		catch (HttpRequestException ex)
		{
			Console.WriteLine($"Error downloading file: {ex.Message}");
			return null;
		}
#pragma warning disable CA1031
		catch (Exception ex)
#pragma warning restore CA1031
		{
			Console.WriteLine($"An unexpected error occurred: {ex.Message}");
			return null;
		}
	}
}

