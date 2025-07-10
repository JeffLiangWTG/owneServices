using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class SACUTradeGroupDownloaderTest
	{
		[Test]
		public void TestCompareDownloadedFile()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_SACU_AGREEMENTS"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.TradeGroups.TestFiles.Input.SACU_TRADE_GROUP_GET.html"));

				using (var client = mockHttp.ToHttpClient())
				{
					var actual = SACUTradeGroupDownloader.Download(client);
					using (var expectedFile = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.TradeGroups.TestFiles.Input.SACUTradeGroup.txt"))
					using (var expected = new StreamReader(expectedFile))
					{
						var expectedResult = expected.ReadToEnd().Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
						Assert.NotNull(actual, nameof(actual));
						Assert.AreEqual(expectedResult, actual);
					}
				}
			}
		}
	}
}
