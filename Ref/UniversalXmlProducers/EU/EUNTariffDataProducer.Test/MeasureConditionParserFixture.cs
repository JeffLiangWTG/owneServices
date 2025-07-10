using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	public class MeasureConditionParserFixture
	{
		[Test]
		public void AssertParseMeasureExclusionsFiltersMeasureTypes()
		{
			var sampleMeasureConditionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\SampleMeasureConditions.xlsx");
			var measureConditionParser = new MeasureConditionParser();
			var results = measureConditionParser.Parse(sampleMeasureConditionFile).ToList();

			Assert.IsNotNull(results);
			Assert.True(results.All(x => ApplicationConfig.MeasureConditionValidMeasureTypeIds.Contains(x.MeasureTypeId)));
			Assert.AreEqual(25, results.Count);

			var firstRecord = results[0];
			Assert.AreEqual(firstRecord.TariffHeader, "0101290000");
			Assert.AreEqual(firstRecord.AdditionalCode, null);
			Assert.AreEqual(firstRecord.OrderNumber, null);
			Assert.AreEqual(firstRecord.StartDate, new DateTime(2017, 09, 21));
			Assert.AreEqual(firstRecord.TradeGroup, "1006");
			Assert.AreEqual(firstRecord.MeasureTypeId, "142");
			Assert.AreEqual(firstRecord.MeasureConditionCode, "B");
			Assert.That(firstRecord.IsImport);
		}

		[SetUp]
		public void Setup()
		{
			ApplicationConfig.ConfigEnvironment();
		}
	}
}
