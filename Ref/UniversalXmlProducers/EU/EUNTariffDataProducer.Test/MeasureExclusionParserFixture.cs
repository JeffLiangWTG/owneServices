using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class MeasureExclusionParserFixture
	{
		[Test]
		public void AssertParseMeasureExclusionsFiltersMeasureTypes()
		{
			var sampleMeasureExclusionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\SampleMeasureExclusion.xlsx");
			var measureExclusionParser = new MeasureExclusionParser();
			var results = measureExclusionParser.Parse(sampleMeasureExclusionFile).ToList();

			Assert.IsNotNull(results);
			Assert.AreEqual(43, results.Count);

			var firstRecord = results[0];
			Assert.AreEqual(firstRecord.TariffHeader, "0100000000");
			Assert.AreEqual(firstRecord.AdditionalCode, null);
			Assert.AreEqual(firstRecord.OrderNumber, null);
			Assert.AreEqual(firstRecord.StartDate, new DateTime(2017, 1, 1));
			Assert.AreEqual(firstRecord.TradeGroup, "1011");
			Assert.AreEqual(firstRecord.MeasureTypeId, "750");
			Assert.AreEqual(firstRecord.ExcludedTradeGroup, "IS");
		}
	}
}
