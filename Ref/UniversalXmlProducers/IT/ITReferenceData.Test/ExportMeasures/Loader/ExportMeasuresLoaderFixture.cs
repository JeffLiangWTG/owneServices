using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExportMeasures
{
	[TestFixture]
	sealed class ExportMeasuresLoaderFixture
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresLoader(logger: null, dateTimeProvider: dateTimeProviderMock.Object, httpClient: httpClientMock.Object), "When logger is null");
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresLoader(logger: logger.Object, dateTimeProvider: null, httpClient: httpClientMock.Object), "When dateTimeProvider is null");
			Assert.Throws<ArgumentNullException>(() => new ExportMeasuresLoader(logger: logger.Object, dateTimeProvider: dateTimeProviderMock.Object, httpClient: null), "When httpClient is null");
		}

		[Test]
		public void DoHandshake()
		{
			using var httpClient = HttpClientTestHelper.GetHttpClientWithContent(string.Empty);

			var loader = new ExportMeasuresLoader(logger.Object, dateTimeProviderMock.Object, httpClient);
			Assert.DoesNotThrowAsync(loader.DoHandshakeAsync);
		}

		[Test]
		public async Task GetMeasureInformationAsync()
		{
			var content = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExportMeasures\TestFiles\TaricMisureServletTariffWithValidNazionaliData.html"));
			using var httpClient = HttpClientTestHelper.GetHttpClientWithContent(content);

			var loader = new ExportMeasuresLoader(logger.Object, dateTimeProviderMock.Object, httpClient);
			var measures = await loader.GetMeasureInformationAsync(TariffCode);

			var expectedAdditionalCodes = new[]
			{
				"U001", "U002", "U003", "U004", "U005", "U006", "U007", "U008", "U009",
				"U029", "U030", "U041", "U042", "U043", "U044", "U045", "U046", "U047",
				"U048", "U049", "U050", "U051", "U052", "U053", "U054", "U055", "U056",
				"U057", "U058", "U059", "U099", "U100", "U101", "U102", "U103", "U104",
				"U105", "U106", "U107", "U108", "U109", "U110", "U111", "U112", "U113",
				"U123", "U124", "U125", "U126", "U127", "U128"
			};

			Assert.AreEqual(51, measures.Count);
			Assert.AreEqual(measures.Select(x => x.AdditionalCode), expectedAdditionalCodes);
			Assert.IsTrue(measures.All(x => x.TradeGroup == "ERGA OMNES"));
		}

		[Test]
		public async Task GetMeasureInformationWhenEmpty()
		{
			var content = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExportMeasures\TestFiles\TaricMisureServletTariffWithMissingNazionaliData.html"));
			using var httpClient = HttpClientTestHelper.GetHttpClientWithContent(content);

			var loader = new ExportMeasuresLoader(logger.Object, dateTimeProviderMock.Object, httpClient);
			var measures = await loader.GetMeasureInformationAsync(TariffCode);

			CollectionAssert.IsEmpty(measures);
		}

		[SetUp]
		public void SetUp()
		{
			logger = new Mock<ILogger>();
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			httpClientMock = new Mock<HttpClient>();
		}

		const string TariffCode = "22082086";

		Mock<ILogger> logger;
		Mock<IDateTimeProvider> dateTimeProviderMock;
		Mock<HttpClient> httpClientMock;
	}
}
