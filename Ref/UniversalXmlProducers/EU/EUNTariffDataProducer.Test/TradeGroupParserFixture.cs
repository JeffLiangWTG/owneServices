using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class TradeGroupParserFixture
	{
		[Test]
		public void AssertParse()
		{
			var tradeGroupFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\EUNTradeGroup\SampleGeographicalAreasComposition.xlsx");
			var tradeGroupParser = new TradeGroupParser();
			var results = tradeGroupParser.Parse(tradeGroupFile).ToList();

			Assert.IsNotNull(results);
			Assert.AreEqual(6, results.Count);

			var firstRecord = results.First();
			Assert.IsNull(firstRecord.TariffHeader);
			Assert.AreEqual(firstRecord.StartDate, new DateTime(2005, 01, 01));
			Assert.AreEqual(firstRecord.CountryGroup, "1005");
			Assert.AreEqual(firstRecord.CountryGroupDescription, "Statistical surveillance");
			Assert.AreEqual(firstRecord.MemberCountry, "AD");
			Assert.AreEqual(firstRecord.MemberCountryDescription, "Andorra");
			Assert.AreEqual(firstRecord.MemberStartDate, new DateTime(2008, 06, 01));
			Assert.AreEqual(firstRecord.MemberEndDate, new DateTime(2079, 06, 06, 23, 59, 00));

			var lastRecord = results.Last();
			Assert.IsNull(lastRecord.TariffHeader);
			Assert.AreEqual(lastRecord.StartDate, new DateTime(2019, 02, 02));
			Assert.AreEqual(lastRecord.CountryGroup, "5002");
			Assert.AreEqual(lastRecord.CountryGroupDescription, "Countries subject to safeguard measures");
			Assert.AreEqual(lastRecord.MemberCountry, "AE");
			Assert.AreEqual(lastRecord.MemberCountryDescription, "United Arab Emirates");
			Assert.AreEqual(lastRecord.MemberStartDate, new DateTime(2019, 02, 02));
			Assert.AreEqual(lastRecord.MemberEndDate, new DateTime(2079, 06, 06, 23, 59, 00));
		}
	}
}
