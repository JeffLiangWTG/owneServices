using System;
using System.IO;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExchangeRates
{
	[TestFixture]
	class ItalyExchangeRateMetaDataParserFixture
	{
		string GetHtmlContent()
		{
			string htmlContent;
			using (var sr = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRates\Res\italianfx.html")))
			{
				htmlContent = sr.ReadToEnd();
			}
			return htmlContent;
		}

		[Test]
		public void TestParseMetaData()
		{
			var metaData = new ExchangeRateMetaData();
			metaData.Content = GetHtmlContent();

			metaData.Source = "https://www.agenziadoganemonopoli.gov.it/portale/dogane/operatore/e-inoltre/cambi-doganali";
			IExchangeRateMetaDataParser parser = new ItalyExchangeRateMetaDataParser(metaData);
			metaData = parser.Parse();

			var expectedUrl = @"https://www.agenziadoganemonopoli.gov.it/portale/documents/20182/88763883/cambi-giugno-2023.pdf/1dd95cbe-2434-4d29-e2f2-1e5a3cead7dc?t=1685084536462";
			Assert.AreEqual(expectedUrl, metaData.DataLocation);
			Assert.AreEqual("20230630", metaData.Version);
		}
	}
}


