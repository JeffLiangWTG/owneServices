using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.KRReferenceData.ExchangeRates.Business;
using CargoWise.RefDbRepo.KRReferenceData.ExchangeRates.Services;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using CargoWise.RefDbRepo.KRReferenceData.Test;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;


namespace CargoWise.RefDbRepo.KRReferenceData.ExchangeRates.Test
{
	class ExchangeRatesTest
	{
		[Test]
		public async Task ExportExchangeRates()
		{
			using (var expectedTestStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.ExchangeRates.Output.KRRefExchangeRateZZ_KR_Export.xml"))
			{
				using (var mockHttp = new MockHttpMessageHandler())
				{
					mockHttp.When(ExportExchangeRatesURL).Respond("application/html", TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.ExchangeRates.Input.KRExchangeRates_Export.xml"));
					var client = mockHttp.ToHttpClient();

					var ratesXML = await DownloadExchangeRates.Download(ExchangeRateTypes.Export, new DateTime(2022, 10, 04), client);
					var exchangeRateParser = new ExchangeRateParser(ratesXML);

					Assert.DoesNotThrow(() => exchangeRateParser.ConvertToXMLFile(outputFile, "KR Export Exchange Rates", ExchangeRateTypes.Export));
					using (var converterResultStream = new FileStream(outputFile, FileMode.Open))
					{
						FileAssert.AreEqual(expectedTestStream, converterResultStream);
					}
				}
			}
		}

		[Test]
		public async Task ImportExchangeRates()
		{
			using (var expectedTestStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.ExchangeRates.Output.KRRefExchangeRateZZ_KR_Import.xml"))
			{
				using (var mockHttp = new MockHttpMessageHandler())
				{
					mockHttp.When(ImportExchangeRatesURL).Respond("application/html", TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.ExchangeRates.Input.KRExchangeRates_Import.xml"));
					var client = mockHttp.ToHttpClient();

					var ratesXML = await DownloadExchangeRates.Download(ExchangeRateTypes.Import, new DateTime(2022, 10, 04), client);
					var exchangeRateParser = new ExchangeRateParser(ratesXML);

					Assert.DoesNotThrow(() => exchangeRateParser.ConvertToXMLFile(outputFile, "KR Import Exchange Rates", ExchangeRateTypes.Import));
					using (var converterResultStream = new FileStream(outputFile, FileMode.Open))
					{
						FileAssert.AreEqual(expectedTestStream, converterResultStream);
					}
				}
			}
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputFile = Path.Combine(Path.GetDirectoryName(assembly.Location), "KR", "TestFiles", "ExchangeRates", "KRRefExchangeRateZZ_KR_Export.xml");
			nexDocDateTimeProviderMock = new Mock<IDateTimeProvider>();
			nexDocDateTimeProviderMock.Setup(x => x.CurrentLocalDateTimeOffset).Returns(new DateTimeOffset(2022, 10, 4, 0, 0, 0, TimeSpan.Zero));
		}
		Assembly assembly;
		string outputFile;
		Mock<IDateTimeProvider> nexDocDateTimeProviderMock;

		const string ExportExchangeRatesURL = "https://unipass.customs.go.kr:38010/ext/rest/trifFxrtInfoQry/retrieveTrifFxrtInfo?crkyCn=j270f135j182f073h020y080m3&qryYymmDd=20221004&imexTp=1";
		const string ImportExchangeRatesURL = "https://unipass.customs.go.kr:38010/ext/rest/trifFxrtInfoQry/retrieveTrifFxrtInfo?crkyCn=j270f135j182f073h020y080m3&qryYymmDd=20221004&imexTp=2";
	}
}
