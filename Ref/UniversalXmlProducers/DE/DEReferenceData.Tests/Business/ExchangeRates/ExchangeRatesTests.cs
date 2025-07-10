using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.DEReferenceData.Services;
using CargoWise.RefDbRepo.DEReferenceData.Services.ExchangeRates;
using CargoWise.RefDbRepo.DEReferenceData.Testing;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.ExchangeRates.Testing
{
	sealed class ExchangeRatesTests
	{
		[Test]
		public async Task ListedExchangeRates()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(ListedExchangeRatesURL).Respond("application/html", TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.ExchangeRates.TestFiles.Input.ListedExchangeRatesResourceDetails.xml"));
				var client = mockHttp.ToHttpClient();

				var ratesXML = await DownloadExchangeRates.Download(DownloadExchangeRates.ListedKursartValue, new DateTime(2021, 11, 01), new DateTime(2021, 12, 01), client, nexDocDateTimeProviderMock.Object);
				var exchangerateParser = new ExchangeRateParser(ratesXML);

				exchangerateParser.ConvertToXMLFile(outputFile, "DE Listed Exchange Rates", DownloadExchangeRates.ListedKursartValue);
			}

			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_DE_Listed.xml");
			AssertFileTextContainsPublicationTimeToMatchServerTimezone(outputFile, expectedXML);
		}

		[Test]
		public async Task UnListedExchangeRates()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(UnlistedExchangeRatesURL).Respond("application/html", TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.ExchangeRates.TestFiles.Input.UnlistedExchangeRatesResourceDetails.xml"));
				var client = mockHttp.ToHttpClient();

				var ratesXML = await DownloadExchangeRates.Download(DownloadExchangeRates.UnListedKursartValue, new DateTime(2021, 11, 01), new DateTime(2021, 12, 01), client, nexDocDateTimeProviderMock.Object);
				var exchangerateParser = new ExchangeRateParser(ratesXML);

				exchangerateParser.ConvertToXMLFile(outputFile, "DE UnListed Exchange Rates", DownloadExchangeRates.UnListedKursartValue);
			}

			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_DE_UnListed.xml");
			AssertFileTextContainsPublicationTimeToMatchServerTimezone(outputFile, expectedXML);
		}

		[Test]
		public async Task IATAExchangeRates()
		{
			var expectedErrorMessage = @"Unable to parse Exchange Rate due to empty currency, zero/negative rate or invalid Dates.
DETAILS:
Currency: 
Rate: 2843,145
Start Date: 01.02.2022
End Date: 28.02.2022
";

			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(IATAExchangeRatesURL).Respond("application/html", TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.ExchangeRates.TestFiles.Input.IATAExchangeRatesResourceDetails.xml"));
				var client = mockHttp.ToHttpClient();

				var ratesXML = await DownloadExchangeRates.Download(DownloadExchangeRates.IATAKursartValue, new DateTime(2022, 01, 10), new DateTime(2022, 02, 10), client, nexDocDateTimeProviderMock.Object);
				var exchangerateParser = new ExchangeRateParser(ratesXML);

				var result = exchangerateParser.ConvertToXMLFile(outputFile, "DE IATA Exchange Rates", DownloadExchangeRates.IATAKursartValue);
				Assert.Multiple(() =>
				{
					var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.ExchangeRates.TestFiles.Output.RefExchangeRateZZ_DE_IATA.xml");
					AssertFileTextContainsPublicationTimeToMatchServerTimezone(outputFile, expectedXML);
					Assert.AreEqual(expectedErrorMessage, result);
				});
			}
		}

		[SetCulture("fr-FR")]
		[Test]
		public void FileOutputCultureInfoCorrect()
		{
			UnListedExchangeRates().Wait();
		}

		[Test]
		public void InvalidStartDate()
		{
			var invalidStartDate =
@"<kurse>
    <kurs id=""242505"">
        <land>Australien</land>
        <iso2>AU</iso2>
        <kurswert>1,5528</kurswert>
        <iso3>AUD</iso3>
        <startdatum>01.15.2021</startdatum>
        <enddatum>30.11.2021</enddatum>
    </kurs>
</kurse>";

			AssertExchageRateError(invalidStartDate, "AUD", "1,5528", "01.15.2021", "30.11.2021");
		}

		[Test]
		public void InvalidEndDate()
		{
			var invalidEndDate =
@"<kurse>
    <kurs id=""242505"">
        <land>Australien</land>
        <iso2>AU</iso2>
        <kurswert>1,5528</kurswert>
        <iso3>AUD</iso3>
        <startdatum>01.11.2021</startdatum>
        <enddatum>32.11.2021</enddatum>
    </kurs>
</kurse>";

			AssertExchageRateError(invalidEndDate, "AUD", "1,5528", "01.11.2021", "32.11.2021");
		}

		[Test]
		public void StartDateIsLaterThanEndDate()
		{
			var startDate =
@"<kurse>
    <kurs id=""242505"">
        <land>Australien</land>
        <iso2>AU</iso2>
        <kurswert>1,5528</kurswert>
        <iso3>AUD</iso3>
        <startdatum>01.11.2021</startdatum>
        <enddatum>31.10.2021</enddatum>
    </kurs>
</kurse>";

			AssertExchageRateError(startDate, "AUD", "1,5528", "01.11.2021", "31.10.2021");
		}

		[Test]
		public void ZeroRate()
		{
			var zeroRate =
@"<kurse>
    <kurs id=""242505"">
        <land>Australien</land>
        <iso2>AU</iso2>
        <kurswert>0</kurswert>
        <iso3>AUD</iso3>
        <startdatum>01.11.2021</startdatum>
        <enddatum>30.11.2021</enddatum>
    </kurs>
</kurse>";

			AssertExchageRateError(zeroRate, "AUD", "0", "01.11.2021", "30.11.2021");
		}

		[Test]
		public void NegativeRate()
		{
			var negativeRate =
@"<kurse>
    <kurs id=""242505"">
        <land>Australien</land>
        <iso2>AU</iso2>
        <kurswert>-12,34</kurswert>
        <iso3>AUD</iso3>
        <startdatum>01.11.2021</startdatum>
        <enddatum>30.11.2021</enddatum>
    </kurs>
</kurse>";

			AssertExchageRateError(negativeRate, "AUD", "-12,34", "01.11.2021", "30.11.2021");
		}

		[Test]
		public void NegativeRateSuffix()
		{
			var negativeRate =
@"<kurse>
    <kurs id=""242505"">
        <land>Australien</land>
        <iso2>AU</iso2>
        <kurswert>12,34-</kurswert>
        <iso3>AUD</iso3>
        <startdatum>01.11.2021</startdatum>
        <enddatum>30.11.2021</enddatum>
    </kurs>
</kurse>";

			AssertExchageRateError(negativeRate, "AUD", "12,34-", "01.11.2021", "30.11.2021");
		}

		[Test]
		public void EmptyCurrency()
		{
			var emptyCurrency =
@"<kurse>
    <kurs id=""242505"">
        <land>Australien</land>
        <iso2>AU</iso2>
        <kurswert>1,5528</kurswert>
        <iso3></iso3>
        <startdatum>01.11.2021</startdatum>
        <enddatum>30.11.2021</enddatum>
    </kurs>
</kurse>";

			AssertExchageRateError(emptyCurrency, "", "1,5528", "01.11.2021", "30.11.2021");
		}

		[Test]
		public void InvalidRate()
		{
			var emptyRate =
   @"<kurse>
    <kurs id=""242505"">
        <land>Australien</land>
        <iso2>AU</iso2>
        <kurswert/>
        <iso3>AUD</iso3>
        <startdatum>01.11.2021</startdatum>
        <enddatum>30.11.2021</enddatum>
    </kurs>
</kurse>";
			var downloadResult = new DownloadResult
			{
				LastModified = DateTimeOffset.MinValue,
				Content = emptyRate
			};
			var exchangerateParser = new ExchangeRateParser(downloadResult);
			var result = exchangerateParser.ConvertToXMLFile(outputFile, "DE Exchange Rates", DownloadExchangeRates.ListedKursartValue);

			Assert.Multiple(() =>
			{
				Assert.That(File.Exists(outputFile), Is.False, "Do not generate rate when 'rateOk' is false");
				Assert.That(result, Is.EqualTo(string.Empty), "No error");
			});
		}

		[Test]
		public void EmptyRates()
		{
			var downloadResultWithError = new DownloadResult
			{
				LastModified = DateTimeOffset.MinValue,
				Content = @"<kurse></kurse>"
			};
			var exchangerateParser = new ExchangeRateParser(downloadResultWithError);

			var result = exchangerateParser.ConvertToXMLFile(outputFile, "DE Exchange Rates", DownloadExchangeRates.ListedKursartValue);

			Assert.Multiple(() =>
			{
				Assert.That(File.Exists(outputFile), Is.False, "No output with empty rates generated");
				Assert.That(result, Is.EqualTo(string.Empty), "No error");
			});
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputFile = Path.Combine(Path.GetDirectoryName(assembly.Location), "DE", "TestFiles", "ExchangeRates", "RefExchangeRateZZ_DE.xml");
			nexDocDateTimeProviderMock = new Mock<IDateTimeProvider>();
			nexDocDateTimeProviderMock.Setup(x => x.CurrentLocalDateTimeOffset).Returns(new DateTimeOffset(2019, 6, 12, 0, 0, 0, TimeSpan.Zero));
		}
		Assembly assembly;
		string outputFile;
		Mock<IDateTimeProvider> nexDocDateTimeProviderMock;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}

		void AssertExchageRateError(string contentWithError, string expectedCurrency, string expectedRate, string expectedStartDate, string expectedEndDate)
		{
			var downloadResultWithError = new DownloadResult
			{
				LastModified = DateTimeOffset.MinValue,
				Content = contentWithError
			};

			var exchangerateParser = new ExchangeRateParser(downloadResultWithError);
			var error = exchangerateParser.ConvertToXMLFile(outputFile, "DE Exchange Rates", DownloadExchangeRates.ListedKursartValue);

			var expectedErrorMessage =
$@"Unable to parse Exchange Rate due to empty currency, zero/negative rate or invalid Dates.
DETAILS:
Currency: {expectedCurrency}
Rate: {expectedRate}
Start Date: {expectedStartDate}
End Date: {expectedEndDate}
";

			Assert.That(error, Is.EqualTo(expectedErrorMessage));
		}


		void AssertFileTextContainsPublicationTimeToMatchServerTimezone(string outputFilePath, string expectedXML)
		{
			var actualUniversalXml = File.ReadAllText(outputFilePath);
			var regex = new Regex(@"<PublicationTime>.*?</PublicationTime>");
			var match = regex.Match(actualUniversalXml);
			if (match.Success)
			{
				actualUniversalXml = actualUniversalXml.Replace(match.Value, $"<PublicationTime>{new DateTime(2019, 06, 12, 0, 0, 0, DateTimeKind.Utc).AddHours(10):s}</PublicationTime>");
			}
			Assert.That(actualUniversalXml, Is.EqualTo(expectedXML));
		}

		const string ListedExchangeRatesURL = "https://www.zoll.de/SiteGlobals/Functions/Kurse/KursExport.xml?view=xmlexportkursesearchresultzoll&kursart=1&startdatum_tag2=01&startdatum_monat2=11&startdatum_jahr2=2021&enddatum_tag2=01&enddatum_monat2=12&enddatum_jahr2=2021&sort=asc&spalte=gueltigkeit";
		const string UnlistedExchangeRatesURL = "https://www.zoll.de/SiteGlobals/Functions/Kurse/KursExport.xml?view=xmlexportkursesearchresultzoll&kursart=2&startdatum_tag2=01&startdatum_monat2=11&startdatum_jahr2=2021&enddatum_tag2=01&enddatum_monat2=12&enddatum_jahr2=2021&sort=asc&spalte=gueltigkeit";
		const string IATAExchangeRatesURL = "https://www.zoll.de/SiteGlobals/Functions/Kurse/KursExport.xml?view=xmlexportkursesearchresultzoll&kursart=3&startdatum_tag2=10&startdatum_monat2=01&startdatum_jahr2=2022&enddatum_tag2=10&enddatum_monat2=02&enddatum_jahr2=2022&sort=asc&spalte=gueltigkeit";
	}
}
