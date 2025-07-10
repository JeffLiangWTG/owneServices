using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	[TestFixture]
	sealed class DownloadExchangeRatesTests
	{
		[Test]
		public void TestDownloadXML_MainUrl()
		{
			Setup("kouji-rate-english20200524-20200530.pdf");

			var (errors, exchangeRates, sourcePath) = downloader.Download(AppConfig.ExchangeRate.Url, mockHttpClientHelper.Object);
			Assert.That(errors, Is.Empty);
			Assert.That(exchangeRates.StartDate, Is.EqualTo("20200524"));
			Assert.That(exchangeRates.EndDate, Is.EqualTo("20200530"));
			Assert.That(exchangeRates.ExchangeRateDetails.Count(), Is.EqualTo(92));

			var usdRate = exchangeRates.ExchangeRateDetails.Single(x => x.Code == "USD");
			Assert.That(usdRate.Rate, Is.EqualTo(107.19m));

			var krwRate = exchangeRates.ExchangeRateDetails.Single(x => x.Code == "KRW");
			Assert.That(krwRate.Rate, Is.EqualTo(0.0875m));
		}

		[Test]
		public void TestDownloadInvalidURL()
		{
			Setup("kouji-rate-english20200524-20200530.pdf");
			var (errors, exchangeRates, sourcePath) = downloader.Download("InvalidURL", new HttpClientHelper());
			Assert.That(errors, Does.Contain("Unable to Load JP Exchange Rate Html from the following URL: InvalidURL"));
			Assert.That(exchangeRates, Is.EqualTo(null));
		}

		[Test]
		public void TestDownloadEmptyPDFURL()
		{
			Setup("kouji-rate-english20200524-20200530.pdf", true);
			var (errors, exchangeRates, sourcePath) = downloader.Download(AppConfig.ExchangeRate.Url, mockHttpClientHelper.Object);
			Assert.That(errors, Does.Contain("Fail to download file from ."));
		}

		[Test]
		public void TestDateOverTheYear()
		{
			Setup("kouji-rate-english20191229-20200104.pdf", false, true);
			var (errors, exchangeRates, sourcePath) = downloader.Download(AppConfig.ExchangeRate.Url, mockHttpClientHelper.Object);
			Assert.That(exchangeRates.StartDate, Is.EqualTo("20191229"));
			Assert.That(exchangeRates.EndDate, Is.EqualTo("20200104"));
			Assert.That(exchangeRates.ExchangeRateDetails.Count(), Is.EqualTo(93));
		}

		[Test]
		public void TestForConvertException()
		{
			Setup("ForConvert-kouji-rate-english20200524-20200530.pdf");
			var (errors, exchangeRates, sourcePath) = downloader.Download(AppConfig.ExchangeRate.Url, mockHttpClientHelper.Object);
			Assert.That(errors, Is.EqualTo("Unable to convert rate for Currency: USD /r/n Rate: \r\n"));
		}

		[Test]
		public void TestNoBruneiDollarRecordInPDF()
		{
			Setup("kouji-rate-english20200524-20200530.pdf");
			var (errors, exchangeRates, sourcePath) = downloader.Download(AppConfig.ExchangeRate.Url, mockHttpClientHelper.Object);
			var bndRate = exchangeRates.ExchangeRateDetails.Single(x => x.Code == DownloadExchangeRates.BruneiDollarCurrencyCode);
			var sgdRate = exchangeRates.ExchangeRateDetails.Single(x => x.Code == DownloadExchangeRates.SingaporeDollarCurrencyCode);

			Assert.That(bndRate.Rate, Is.EqualTo(sgdRate.Rate));
		}

		[Test]
		public void TestNoSingaporeDollarRecordInPDF()
		{
			Setup("NoSGD-kouji-rate-english20200524-20200530.pdf");
			var (errors, exchangeRates, sourcePath) = downloader.Download(AppConfig.ExchangeRate.Url, mockHttpClientHelper.Object);

			Assert.That(errors, Is.Empty);
			Assert.That(exchangeRates.ExchangeRateDetails.Any(x => x.Code == DownloadExchangeRates.BruneiDollarCurrencyCode), Is.EqualTo(false));
			Assert.That(exchangeRates.ExchangeRateDetails.Any(x => x.Code == DownloadExchangeRates.SingaporeDollarCurrencyCode), Is.EqualTo(false));
		}

		void Setup(string inputFileName, bool setUrlEmpty = false, bool setHtmlDateOverTheYear = false)
		{
			assembly = Assembly.GetExecutingAssembly();
			downloader = new DownloadExchangeRatesTester();
			if (setUrlEmpty)
			{
				var helper = new Mock<IFileHelper>();
				helper.Setup(x => x.GetFullPDFPath(@"kawase2020/kouji-rate-english20200524-20200530.pdf")).Returns("");
				downloader.TestFileHelper = helper.Object;
			}
			else
			{
				downloader.TestFileHelper = new FileHelper();
			}

			var expectedFileUrl = "https://www.customs.go.jp/english/kawase/kawase2020/kouji-rate-english20200524-20200530.pdf";
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRate\TestFiles\Input\" + inputFileName);

			string basePagePath;
			if (setHtmlDateOverTheYear)
			{
				expectedFileUrl = "https://www.customs.go.jp/english/kawase/kawase2019/kouji-rate-english20191229-20200104.pdf";
				basePagePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRate\TestFiles\Input\RateForOverTheYear.html");
			}
			else
			{
				basePagePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRate\TestFiles\Input\MainRateUrl.html");
			}

			mockHttpClientHelper = new Mock<IHttpClientHelper>();
			mockBasePageFileStream = new FileStream(basePagePath, FileMode.Open);
			mockPDFFileStream = new FileStream(inputFilePath, FileMode.Open);

			string htmlContent;
			using (var reader = new StreamReader(mockBasePageFileStream))
			{
				htmlContent = reader.ReadToEnd();
			}

			mockHttpClientHelper.Setup(x => x.GetAsync(expectedFileUrl)).Returns(Task.FromResult<Stream>(mockPDFFileStream));
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(AppConfig.ExchangeRate.Url)).Returns(Task.FromResult(htmlContent));
		}

		[TearDown]
		public void TearDown()
		{
			mockBasePageFileStream?.Close();
			mockPDFFileStream?.Close();
		}

		Mock<IHttpClientHelper> mockHttpClientHelper;
		FileStream mockBasePageFileStream;
		FileStream mockPDFFileStream;
		Assembly assembly;
		DownloadExchangeRatesTester downloader;
	}

	sealed class DownloadExchangeRatesTester : DownloadExchangeRates
	{
		protected override IFileHelper GetFileHelper() => TestFileHelper;

		public IFileHelper TestFileHelper { get; set; }
	}
}
