using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Common.Tests.CommonHelpers;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates
{
	[TestFixture]
	class DeveloperTests
	{
		[Test]
		[Explicit("Debug/Developer Test")]
		public void DownloadActualRatesAndLogToFile()
		{
			var errorCollector = new StringBuilder();
			var webClientWrapper = new Services.Common.WebClientWrapper();
			var dateTimeProvider = new DateTimeProvider();

			var downloader = new DownloaderForTest(dateTimeProvider, webClientWrapper, errorCollector);

			var exchangeData = downloader.GetExchangeRates();
			var rates = exchangeData.Rates.ToList();

			var filename = GetTempFile();

			var output = new StringBuilder();
			output.AppendLine(CultureInfo.InvariantCulture, $"GB Exchange Rates Downloader {DateTime.Now}");
			output.AppendLine("Errors");
			output.Append(errorCollector);
			output.AppendLine("\r\nData:");
			output.AppendLine(CultureInfo.InvariantCulture, $"Publish Date: {exchangeData.PublishDate}");
			rates.ForEach(r => output.AppendLine(CultureInfo.InvariantCulture, $"{r.StartDate} {r.EndDate} {r.Currency} {r.Rate}"));

			File.WriteAllText(filename, output.ToString());

			Console.WriteLine($"Created output file: {filename}");
		}

		string GetTempFile() => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
	}
}
