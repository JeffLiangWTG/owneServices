using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class NomenclatureParserFixture
	{
		[Test]
		public void AssertParse()
		{
			var nomenclatureFile = GetTestFilePath("SampleNomenclature.xlsx");
			var nomenclatureParser = new NomenclatureParser();
			var results = nomenclatureParser.Parse(nomenclatureFile).ToList();

			Assert.IsNotNull(results);
			Assert.AreEqual(15, results.Count);

			var firstRecord = results.First();
			Assert.Multiple(() =>
			{
				Assert.AreEqual(firstRecord.TariffHeader, "0100000000 80");
				Assert.AreEqual(firstRecord.StartDate, new DateTime(1971, 12, 31));
				Assert.AreEqual(firstRecord.EndDate, new DateTime(2079, 06, 06, 23, 59, 00));
				Assert.AreEqual(firstRecord.Description, "LIVE ANIMALS");
				Assert.AreEqual(firstRecord.HierarchyPosition, 2);
				Assert.AreEqual(firstRecord.Level, 0);
				Assert.AreEqual(firstRecord.Language, "EN");
			});

			var lastRecord = results.Last();
			Assert.Multiple(() =>
			{
				Assert.AreEqual(lastRecord.TariffHeader, "0102219000 80");
				Assert.AreEqual(lastRecord.StartDate, new DateTime(2012, 01, 01));
				Assert.AreEqual(lastRecord.EndDate, new DateTime(2079, 06, 06, 23, 59, 00));
				Assert.AreEqual(lastRecord.Description, "Other");
				Assert.AreEqual(lastRecord.HierarchyPosition, 8);
				Assert.AreEqual(lastRecord.Level, 3);
				Assert.AreEqual(lastRecord.Language, "EN");
			});
		}

		[Test]
		public void TransformNomenclatureDescription()
		{
			var nomenclatureFile = GetTestFilePath("SampleNomenclatureSpecialCharacters.xlsx");
			var nomenclatureParser = new NomenclatureParser();
			var results = nomenclatureParser.Parse(nomenclatureFile).ToList();

			Assert.IsNotNull(results);
			Assert.AreEqual(3, results.Count);

			Assert.Multiple(() =>
			{
				var firstRecord = results.ElementAt(0);
				Assert.That(firstRecord.TariffHeader, Is.EqualTo("3707100060 80"));
				Assert.That(firstRecord.Description, Does.Contain("\n"));
				Assert.That(firstRecord.Description, Does.Not.Contain("|"));

				var secondRecord = results.ElementAt(1);
				Assert.That(secondRecord.TariffHeader, Is.EqualTo("3801100020 80"));
				Assert.That(secondRecord.Description, Does.Contain("\n"));

				var thirdRecord = results.ElementAt(2);
				Assert.That(thirdRecord.TariffHeader, Is.EqualTo("3801100030 80"));
				Assert.That(thirdRecord.Description, Does.Contain("\n"));
			});
		}

		[Test]
		public void SkipRecordsWithInvalidHierarhyPosition()
		{
			var nomenclatureFile = GetTestFilePath("SampleNomenclatureWithInvalidHierarchyPosition.xlsx");
			var nomenclatureParser = new NomenclatureParser();
			var results = nomenclatureParser.Parse(nomenclatureFile).ToList();

			Assert.That(results, Is.Not.Null);
			Assert.Multiple(() =>
			{
				Assert.That(results.Count, Is.EqualTo(15));
				Assert.That(results.Where(x => x.HierarchyPosition == 0).Count(), Is.EqualTo(0));
			});
		}

		string GetTestFilePath(string fileName)	=> Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", fileName);
	}
}
