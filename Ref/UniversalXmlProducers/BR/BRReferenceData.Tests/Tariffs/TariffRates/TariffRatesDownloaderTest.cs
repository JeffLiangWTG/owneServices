using NUnit.Framework;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	sealed class TariffRatesDownloaderTest : BaseTariffRatesTest
	{
		[Test]
		public void TestDownloadTariffRates()
		{
			var tariffs = new[] { "29039917", "01041019" };

			var downloadedTariffs = new List<TariffDTO>();

			using var client = GetMockedHttpClient().ToHttpClient();
			new TariffRatesDownloader().DownloadRates(client, tariffs, SaveInMemory);

			AssertDownloadedTariffs(downloadedTariffs);

			void SaveInMemory(IEnumerable<TariffDTO> tariffs) => downloadedTariffs.AddRange(tariffs);
		}
	}
}
