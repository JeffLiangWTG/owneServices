using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Services.ExchangeRates;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.ExchangeRates
{
	[TestFixture]
	class ExchangeRatesTest
	{
		[Test]
		public void DownloadActualRatesAndConvert()
		{
			using (var expectedTestStream = classType.GetTestStream("TestFiles.Output.ConvertedRates.xml"))
			using(var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(DownloadTodayUrl).WithUserAgent().Respond("application/xml", classType.GetTestStream("TestFiles.Input.DownloadedRates.xml"));
				var client = mockHttp.ToHttpClient();
				var download = DownloadExchangeRates.Download(client);
				var parser = new ExchangeRatesParser(download);

				using (var outputFile = new TemporaryOutputFile(@"ExchangeRates\RefExchangeRateZZ_CH.xml"))
				{
					parser.ConvertToRefXML(outputFile.FullPath);
					using (var converterResultStream = new FileStream(outputFile.FullPath, FileMode.Open))
					{
						var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(converterResultStream));
						var expectedXml = XDocument.Load(expectedTestStream);
						Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml));
					}
				}
			}
		}

		[Test]
		public void DateRangeHasMultipleRefExchangeRates()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.ValidDateRange.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"ExchangeRates\ValidDateRange.xml"))
			{
				new ExchangeRatesParser(download).ConvertToRefXML(outputFile.FullPath);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				using (var expectedStream = classType.GetTestStream("TestFiles.Output.ConvertedValidDateRange.xml"))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));;
					var expectedXml = XDocument.Load(expectedStream);
					Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml));
				}
			}
		}

		[Test]
		public void TestEmptyCode()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.InvalidRates.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"ExchangeRates\TestEmptyCode.xml"))
			{
				var errors = new ExchangeRatesParser(download).ConvertToRefXML(outputFile.FullPath);
				Assert.That(errors, Does.Contain("DETAILS:\r\nCurrency (Code): \r\nDescription (English): Euro Member\r\nCurrency (Waehrung): 1 EUR\r\nRate (Kurs): 1.11342\r\n"));
			}
		}

		[Test]
		public void TestInvalidCurrency()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.InvalidRates.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"ExchangeRates\TestInvalidCurrency.xml"))
			{
				var errors = new ExchangeRatesParser(download).ConvertToRefXML(outputFile.FullPath);
				Assert.That(errors, Does.Contain("DETAILS:\r\nCurrency (Code): usd\r\nDescription (English): United States\r\nCurrency (Waehrung): A USD\r\nRate (Kurs): 0.93368\r\n"));
			}
		}

		[Test]
		public void TestEmptyCurrency()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.InvalidRates.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"ExchangeRates\TestEmptyCurrency.xml"))
			{
				var errors = new ExchangeRatesParser(download).ConvertToRefXML(outputFile.FullPath);
				Assert.That(errors, Does.Contain("DETAILS:\r\nCurrency (Code): egp\r\nDescription (English): Egypt\r\nCurrency (Waehrung): \r\nRate (Kurs): 5.9481\r\n"));
			}
		}

		[Test]
		public void TestNegativeRate()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.InvalidRates.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"ExchangeRates\InvalidRates.xml"))
			{
				var errors = new ExchangeRatesParser(download).ConvertToRefXML(outputFile.FullPath);
				Assert.That(errors, Does.Contain("DETAILS:\r\nCurrency (Code): all\r\nDescription (English): Albania\r\nCurrency (Waehrung): 100 ALL\r\nRate (Kurs): -0.92292\r\n"));
			}
		}

		Type classType => GetType();

		const string DownloadTodayUrl = @"https://www.backend-rates.ezv.admin.ch/api/xmldaily";
	}
}
