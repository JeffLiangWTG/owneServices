using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class RateParserFixture
	{
		[Test]
		public void AssertDirectionDependsOnSourceFileName()
		{
			var sampleDutiesImportFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\SampleDutiesImport.xlsx");
			var rateParser = new RateParser(true);
			var results = rateParser.Parse(sampleDutiesImportFile).ToList();
			Assert.That(results[0].IsImport);

			var sampleDutiesExortFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\SampleDutiesExport.xlsx");
			rateParser = new RateParser(false);
			results = rateParser.Parse(sampleDutiesExortFile).ToList();
			Assert.That(!results[0].IsImport);
		}

		[Test]
		public void AssertParseRateExtractsAllRates()
		{
			var sampleDutiesImportFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\SampleDutiesImport.xlsx");
			var rateParser = new RateParser(true);
			var results = rateParser.Parse(sampleDutiesImportFile).ToList();

			Assert.IsNotNull(results);
			Assert.AreEqual(73, results.Count);

			var firstRecord = results[0];
			Assert.AreEqual(firstRecord.TariffHeader, "0100000000");
			Assert.AreEqual(firstRecord.AdditionalCode, null);
			Assert.AreEqual(firstRecord.OrderNumber, null);
			Assert.AreEqual(firstRecord.StartDate, new DateTime(2017, 1, 1));
			Assert.AreEqual(firstRecord.TradeGroup, "1011");
			Assert.AreEqual(firstRecord.MeasureTypeId, "750");
			Assert.AreEqual(firstRecord.Rate, "Cond:  B cert: C-644 (29):; B cert: Y-929 (29):; B (09):");
		}
	}
}
