using System.IO;
using System;
using NUnit.Framework;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	sealed class RateDailyParserFixture
	{
		[Test]
		public void AssertParseRateExtractsAllRates()
		{
			var sampleDailyMeasureFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\DailyTariffProducer\SampleDailyMeasures.xlsx");
			var rateParser = new RateDailyParser("SampleDailyMeasures.xlsx");
			var results = rateParser.Parse(sampleDailyMeasureFile).ToList();

			Assert.IsNotNull(results);
			Assert.AreEqual(5, results.Count);


			var firstRecord = results[0];
			Assert.AreEqual(new DateTime(2017, 1, 1), firstRecord.StartDate);
			Assert.AreEqual(new DateTime(2023, 06, 30, 23, 59, 00), firstRecord.EndDate);
			Assert.AreEqual("1001190000", firstRecord.TariffHeader);
			Assert.AreEqual(null, firstRecord.AdditionalCode);
			Assert.AreEqual(null, firstRecord.OrderNumber);
			Assert.AreEqual("1011", firstRecord.TradeGroup);
			Assert.AreEqual("103", firstRecord.MeasureTypeId);
			Assert.AreEqual("0.000 EUR TNE ", firstRecord.Rate);
			Assert.AreEqual(true, firstRecord.IsImport);
			Assert.AreEqual("SampleDailyMeasures.xlsx", firstRecord.FileName);

			var lastRecord = results[2];
			Assert.AreEqual("0102219000", lastRecord.TariffHeader);
			Assert.AreEqual(null, lastRecord.AdditionalCode);
			Assert.AreEqual(null, lastRecord.OrderNumber);
			Assert.AreEqual(new DateTime(2023, 7, 1), lastRecord.StartDate);
			Assert.AreEqual(new DateTime(2023, 06, 30, 23, 59, 00), lastRecord.EndDate);
			Assert.AreEqual("1008", lastRecord.TradeGroup);
			Assert.AreEqual("780", lastRecord.MeasureTypeId);
			Assert.AreEqual("Cond:  Y cert: Y-X45 (29):; Y cert: Y-X46 (29):; Y (09):", lastRecord.Rate);
			Assert.AreEqual(false, lastRecord.IsImport);
			Assert.AreEqual("SampleDailyMeasures.xlsx", firstRecord.FileName);
		}
	}
}
