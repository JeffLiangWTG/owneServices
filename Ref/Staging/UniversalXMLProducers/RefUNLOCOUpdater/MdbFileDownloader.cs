using System;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Polly;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public class MdbFileDownloader : IDisposable
	{
		string downloadFilePath;
		DateTime publicationDate;
		IScrapePageHTMLDownloader scrapePageHTMLDownloader;
		IHttpClientHelper httpClientHelper;

		public MdbFileDownloader(IScrapePageHTMLDownloader scrapePageHTMLDownloader, IHttpClientHelper httpClientHelper)
		{
			Argument.NotNull(scrapePageHTMLDownloader, nameof(scrapePageHTMLDownloader));
			Argument.NotNull(httpClientHelper, nameof(httpClientHelper));
			this.scrapePageHTMLDownloader = scrapePageHTMLDownloader;
			this.httpClientHelper = httpClientHelper;
		}

		public (string DownloadFilePath, DateTime PublicationDate) GetMdbFileAndPublicationDate()
		{
			if (ConfigurationProvider.UNECESpecificFileEnableUntil >= DateTime.Today
				&& ConfigurationProvider.UNECESpecificFilePublicationDate > DateTime.MinValue)
			{
				downloadFilePath = ConfigurationProvider.ProgramSpecificConfigurationsUNECEFilePath;
				publicationDate = ConfigurationProvider.UNECESpecificFilePublicationDate;
			}
			else
			{
				var policy=Policy.Handle<Exception>()
					.OrResult<ScrapePageHTML>(r=> !ValidateScrapeResult(r))
					.WaitAndRetry(
					2,
					retryAttempt => TimeSpan.FromSeconds(5),
					(outcome, timeSpan, retryCount, context) =>
					{
						if (outcome.Exception != null)
						{
							var errorMessage = $"Retry {retryCount} failed with exception: {outcome.Exception.Message}";
							Console.WriteLine(errorMessage);
						}
					});
				var result = policy.Execute(Scrape);
				if (ValidateScrapeResult(result))
				{
					publicationDate = result.PublicationTime;
					downloadFilePath = scrapePageHTMLDownloader.DownloadFile(result);
				}
				else
				{
					Console.Error.WriteLine($"Failed to scrape page.");
				}
			}

			return (downloadFilePath, publicationDate);
		}

		public void Dispose()
		{
			if (File.Exists(downloadFilePath))
			{
				File.Delete(downloadFilePath);
			}
		}

		ScrapePageHTML Scrape()
		{
			return new ScrapePageHTML(httpClientHelper, ConfigurationProvider.UNECESourceUri);
		}

		static bool ValidateScrapeResult(ScrapePageHTML scrapePageHTML)
		{
			return scrapePageHTML != null
				&& !string.IsNullOrEmpty(scrapePageHTML.UNLOCOHyperLink)
				&& scrapePageHTML.PublicationTime != DateTime.MinValue;
		}
	}
}
