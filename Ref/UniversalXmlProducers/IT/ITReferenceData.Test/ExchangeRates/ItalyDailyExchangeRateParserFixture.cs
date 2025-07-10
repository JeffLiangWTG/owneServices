using System;
using System.IO;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.ExchangeRates
{
	[TestFixture]
	public class ItalyDailyExchangeRateParserFixture
	{
		[Test]
		public void TestParse()
		{
			var metaData = new ExchangeRateMetaData();
			metaData.DataLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRates\Res\eurofxref-hist-90d.xml");
			var parser = new ItalyDailyExchangeRateParser(metaData);
			parser.ParseMeta();
			var result = parser.Parse();
			Assert.AreEqual(31, result.Count);

			var firstElement = result[0];
			Assert.AreEqual("IT", firstElement.ZZN_RN_NKCountry);
			Assert.AreEqual("USD", firstElement.ZZN_RX_NKExCurrency);
			Assert.AreEqual(1.1853, firstElement.ZZN_Rate);
			Assert.AreEqual("CUS", firstElement.ZZN_ExRateType);
			Assert.AreEqual(new DateTime(2017, 12, 22), firstElement.ZZN_StartDate);
			Assert.AreEqual(new DateTime(2017, 12, 22), firstElement.ZZN_EndDate);
		}
	}
}
