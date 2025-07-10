using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate;
using CargoWise.RefDbRepo.USReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[Explicit]
	sealed class ExchangeRateParserManualTest
	{
		[Test]
		public void TestParseToXmlWithLatestData()
		{
			if (File.Exists(ExchangeRateParser.LogFilePath))
			{
				File.Delete(ExchangeRateParser.LogFilePath);
			}

			var url = @"https://www.cbp.gov/trade/document/report/daily-foreign-currency-exchange-rate-multipliers";
			var prefix = @"https://www.cbp.gov";
			var outputPath = @"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles";

			var serviceClient = new DownLoadService();

			var parser = new ExchangeRateParser(url, prefix, outputPath, serviceClient);

			var result = parser.ParseToXml();

			Assert.IsTrue(Regex.IsMatch(result, @"Processed -?\d+ exchange rates."));

			var files = Directory.GetFiles(outputPath, "*_RefExchangeRateZZ_US_CBP_CUS_Excel.xml");
			Assert.IsTrue(files.Length > 0, "Should download the latest data and convert it to a xml file.");
		}
	}
}
