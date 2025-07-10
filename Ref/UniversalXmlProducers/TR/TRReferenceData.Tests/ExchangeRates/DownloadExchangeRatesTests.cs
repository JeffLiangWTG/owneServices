using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates
{
	[TestFixture]
	class DownloadExchangeRatesTests
	{
		[Test]
		public void DownloadXML()
		{
			var exchangeRatesURL = Path.Combine(TempFolder, "download-exchange.xml");
			TestHelper.SimulateDownload(exchangeRatesURL, "CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates.TestFiles.Input.download-exchange.xml");
			var ratesXML = DownloadExchangeRates.Download(exchangeRatesURL);
			Assert.That(ratesXML.Currency.Length, Is.EqualTo(20));
		}

		[Test]
		public void DownloadInvalidXML()
		{
			var exception = Assert.Throws<ExchangeRatesException>(() => DownloadExchangeRates.Download("InvalidURL"));
			Assert.That(exception.Message, Does.StartWith("Unable to Load TR Exchange Rate XML from the following URL: InvalidURL"));
		}

		[Test]
		public void ExceptionHandling_DeserializeXML()
		{
			var path = Path.Combine(TempFolder, "InvalidExchangeData.xml");
			TestHelper.SimulateDownload(path, "CargoWise.RefDbRepo.TRReferenceData.Tests.ExchangeRates.TestFiles.Input.InvalidExchangeData.xml");
			var exception = Assert.Throws<ExchangeRatesException>(() => DownloadExchangeRates.Download(path));
			Assert.That(exception.Message, Is.EqualTo($@"Unable to Load TR Exchange Rate XML from the following URL: {path}
Message: There is an error in XML document (1, 40).
Inner Exception: <bookstore xmlns=''> was not expected.
XML: <?xml version=""1.0"" encoding=""UTF-8""?><bookstore><book category=""cooking""><title lang=""en"">Everyday Italian</title><author>Giada De Laurentiis</author><year>2005</year><price>30.00</price></book><book category=""children""><title lang=""en"">Harry Potter</title><author>J K. Rowling</author><year>2005</year><price>29.99</price></book><book category=""web""><title lang=""en"">XQuery Kick Start</title><author>James McGovern</author><author>Per Bothner</author><author>Kurt Cagle</author><author>James Linn</author><author>Vaidyanathan Nagarajan</author><year>2003</year><price>49.99</price></book><book category=""web"" cover=""paperback""><title lang=""en"">Learning XML</title><author>Erik T. Ray</author><year>2003</year><price>39.95</price></book></bookstore>
"));
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
	}
}
