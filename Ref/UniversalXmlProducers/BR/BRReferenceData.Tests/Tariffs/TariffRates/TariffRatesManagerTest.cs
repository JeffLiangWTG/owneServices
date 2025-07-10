using NUnit.Framework;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	sealed class TariffRatesManagerTest : BaseTariffRatesTest
	{
		[Test]
		public void TestGetTariffsMultiThread()
		{
			using var client = GetMockedHttpClient().ToHttpClient();
			var downloadedTariffs = new TariffRatesManagerForTesting().GetTariffsMultiThread(client);

			AssertDownloadedTariffs(downloadedTariffs);
		}

		public class TariffRatesManagerForTesting : TariffRatesManager
		{
			protected override HttpClient HttpClient => GetMockedHttpClient().ToHttpClient();
		}
	}
}
