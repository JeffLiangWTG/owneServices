using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using ICSharpCode.SharpZipLib.GZip;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.DeTariffs
{
	public class DeTariffsDownloader
	{
		public DeTariffsDownloader(HttpClient client, Uri baselineIndex, Uri updateIndex, string downloadDirectory, string extractionDirectory)
		{
			this.client = client ?? throw new ArgumentNullException(nameof(client));
			BaselineIndex = baselineIndex ?? throw new ArgumentNullException(nameof(baselineIndex));
			UpdateIndex = updateIndex ?? throw new ArgumentNullException(nameof(updateIndex));
			DownloadDirectory = downloadDirectory ?? throw new ArgumentNullException(nameof(downloadDirectory));
			ExtractionDirectory = extractionDirectory ?? throw new ArgumentNullException(nameof(extractionDirectory));
		}

		public Uri BaselineIndex { get; }
		public Uri UpdateIndex { get; }
		public string DownloadDirectory { get; }
		public string ExtractionDirectory { get; }

		internal async Task<DownloadSource> GetBaselineDownloadSource()
		{
			var fileNameRegex = new Regex("^XD[0-9]{8}_GDB.zip$", RegexOptions.IgnoreCase);

			var response = await RetryHelper.RetryWithDelayAsync(async () =>
			{
				var response = await client.GetAsync(BaselineIndex);
				response.EnsureSuccessStatusCode();
				return response;
			});

			var contents = await response.Content.ReadAsStringAsync();

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(contents);

			var links = htmlDoc.DocumentNode
				.Descendants("a")
				.Select(a => a.GetAttributeValue("href", null));
			var gdbLinks = links
				.Where(s => fileNameRegex.IsMatch(s));

			var latestGdbDownloadSource = gdbLinks.Select(f => DownloadSource.CreateOrNull(BaselineIndex, f))
				.Where(e => e is not null).OrderByDescending(e => e.SetNumber).FirstOrDefault();

			return latestGdbDownloadSource;
		}

		internal async Task<List<DownloadSource>> GetUpdateDownloadSources(DownloadSource baseline)
		{
			var fileNameRegex = new Regex("^XD[0-9]{8}.zip$", RegexOptions.IgnoreCase);

			var response = await RetryHelper.RetryWithDelayAsync(async () =>
			{
				var response = await client.GetAsync(UpdateIndex);
				response.EnsureSuccessStatusCode();
				return response;
			});

			var contents = await response.Content.ReadAsStringAsync();

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(contents);

			var links = htmlDoc.DocumentNode
				.Descendants("a").Select(a => a.GetAttributeValue("href", null));

			var updateLinks = links
				.Where(s => fileNameRegex.IsMatch(s));

			var updateSources = updateLinks.Select(f => DownloadSource.CreateOrNull(UpdateIndex, f));

			var updateSourcesAfterBaseline =
				updateSources.Where(e => e is not null && e.SetNumber > baseline.SetNumber);

			var updateSourcesAfterBaselineWithLatestVersion = updateSourcesAfterBaseline
				.GroupBy(e => e.SetNumber)
				.Select(g => g.MaxBy(e => e.Version));

			return updateSourcesAfterBaselineWithLatestVersion.ToList();
		}

		public async Task DownloadTariffs()
		{
			Directory.CreateDirectory(DownloadDirectory);
			var baseline = await GetBaselineDownloadSource();

			var filesToDownload = await GetUpdateDownloadSources(baseline);
			filesToDownload.Add(baseline);

			foreach (var file in Directory.EnumerateFiles(DownloadDirectory, "XD*.zip"))
			{
				if (!FileOnDiskIsInDownloadSet(file, filesToDownload))
				{
					File.Delete(file);
				}
			}

			foreach (var download in filesToDownload)
			{
				await DownloadTariffFileToDiskIfNotExists(download);
			}
			if (Directory.Exists(ExtractionDirectory))
			{
				Directory.Delete(ExtractionDirectory, true);
			}
			Directory.CreateDirectory(ExtractionDirectory);
			await ExtractAllFilesInDownloadDirectory();


			static bool FileOnDiskIsInDownloadSet(string file, List<DownloadSource> downloadTargets)
			{
				return downloadTargets.Any(u => u.FileName == file);
			}
		}

		internal async Task ExtractAllFilesInDownloadDirectory()
		{
			Directory.CreateDirectory(ExtractionDirectory);

			foreach (var file in Directory.EnumerateFiles(DownloadDirectory, "XD*.zip"))
			{
				await using var fileStream = File.OpenRead(file);
				using var zipArchive = new ZipArchive(fileStream);
				foreach (var archiveEntry in zipArchive.Entries)
				{
					if (Path.GetExtension(archiveEntry.Name).Equals(".lzw", StringComparison.OrdinalIgnoreCase))
					{
						var fileNameWithoutLzwExtension = Path.GetFileNameWithoutExtension(archiveEntry.Name);

						var destination = Path.Combine(ExtractionDirectory, fileNameWithoutLzwExtension);

						if (File.Exists(destination))
						{
							continue;
						}

						await using var stream = archiveEntry.Open();
						await using var gzip = new GZipInputStream(stream);
						await using var output = File.Create(destination);
						await gzip.CopyToAsync(output);
					}
					else
					{
						throw new InvalidDataException(
							$"Expected {file} to contain only .lzw entries, but encountered {archiveEntry.Name}");
					}
				}
			}
		}

		internal async Task DownloadTariffFileToDiskIfNotExists(DownloadSource download)
		{
			var filePath = Path.Combine(DownloadDirectory, download.FileName);

			if (File.Exists(filePath))
			{
				return;
			}

			var response = await RetryHelper.RetryWithDelayAsync(async () =>
			{
				var response = await client.GetAsync(download.DownloadUri);
				response.EnsureSuccessStatusCode();
				return response;
			});

			if (!Directory.Exists(DownloadDirectory))
			{
				Directory.CreateDirectory(DownloadDirectory);
			}

			await using var fileStream = File.Create(filePath);
			await response.Content.CopyToAsync(fileStream);
		}

		readonly HttpClient client;
	}
}
