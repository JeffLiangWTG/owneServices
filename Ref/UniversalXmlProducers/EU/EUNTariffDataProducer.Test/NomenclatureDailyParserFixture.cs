using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	sealed class NomenclatureDailyParserFixture
	{
		[Test]
		public void Parse_DiscardRowsWithEmptyTariffHeader()
		{
			var nomenclatureFile = GetTestFilePath("DailyNomenclatureWithInvalidRow.xlsx");
			var dailyNomenclature = new NomenclatureDailyParser(nomenclatureFile);
			var results = dailyNomenclature.Parse(nomenclatureFile).ToList();

			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count);

			IRawNomenclatureDailyRecord firstRecord = results.First();

			Assert.Multiple(() =>
			{
				Assert.AreEqual("9880000000 80", firstRecord.TariffHeader);
				Assert.AreEqual(new DateTime(2016, 07, 01), firstRecord.StartDate);
				Assert.AreEqual(new DateTime(2025, 01, 03, 23, 59, 00), (firstRecord as IRawNomenclatureRecord).EndDate);
				Assert.AreEqual("PL", firstRecord.Language);
				Assert.AreEqual(10, firstRecord.HierarchyPosition);
				Assert.AreEqual(4, firstRecord.Level);
				Assert.AreEqual("DESCR TEST", firstRecord.Description);
			});
		}

		[Test]
		public void IncludeRecordsWithInvalidHierarhyPosition()
		{
			var nomenclatureFile = GetTestFilePath("DailyNomenclatureWithInvalidHierarchyPosition.xlsx");
			var dailyNomenclature = new NomenclatureDailyParser(nomenclatureFile);
			var results = dailyNomenclature.Parse(nomenclatureFile).ToList();

			Assert.That(results, Is.Not.Null);

			Assert.Multiple(() =>
			{
				Assert.That(results.Count, Is.EqualTo(5));
				Assert.That(results.Where(x => x.HierarchyPosition == 0).Count(), Is.EqualTo(2), "Hierarchy Position can be taken from matching combined nomenclature record, so it should be allowed in daily record");
			});
		}

		string GetTestFilePath(string fileName) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", fileName);
	}
}
