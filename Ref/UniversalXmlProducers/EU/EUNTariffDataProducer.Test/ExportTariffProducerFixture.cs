using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class ExportTariffProducerFixture
	{
		[Test]
		public void TestFilesToLocate()
		{
			var exportTariffProducer = new ExportTariffProducer();
			Assert.That(exportTariffProducer.FilesToLocate, Is.EqualTo(new[] { "Duties Export", "Measure exclusions" }));
		}

		[Test]
		public void TestGetNewTariffXmlProducer()
		{
			var exportTariffProducer = new ExportTariffProducerForTest();
			Assert.That(exportTariffProducer.GetNewTariffXmlProducerExposed().GetType(), Is.EqualTo(typeof(ExportTariffXmlProducer)));
		}

		[Test]
		public void GetNewTariffXmlProducerIncludingCompositeKey()
		{
			var exportTariffProducer = new ExportTariffProducerForTest();
			Assert.That(exportTariffProducer.GetNewTariffXmlProducerIncludingCompositeKeyExposed(true).GetType(), Is.EqualTo(typeof(ExportTariffXmlProducer)));
		}

		[Test]
		public void TestLoadTariffs()
		{
			var allTariffs = new List<RefCusTariff>
			{
				new RefCusTariff() { ZZ1_TariffCode = "0806101005" },
				new RefCusTariff() { ZZ1_TariffCode = "0806101090" },
				new RefCusTariff() { ZZ1_TariffCode = "0806203010" },
				new RefCusTariff() { ZZ1_TariffCode = "0806203090" },
				new RefCusTariff() { ZZ1_TariffCode = "0102291050" }
			};

			var processedTariff = new List<RefCusTariff>
			{
				new RefCusTariff() { ZZ1_TariffCode = "0806000000", RefCusConditions = new RefCusCondition[] { new RefCusCondition() { ZX1_Comment = "3 * E" } }, RefCusTariffUOMs = new RefCusTariffUOM[] { new RefCusTariffUOM() { ZZ8_UOM = "KGM" } } },
				new RefCusTariff() { ZZ1_TariffCode = "0806100000", RefCusConditions = new RefCusCondition[] { new RefCusCondition() { ZX1_Comment = "5 * Q" } }, RefCusTariffUOMs = new RefCusTariffUOM[] { new RefCusTariffUOM() { ZZ8_UOM = "KGM" }, new RefCusTariffUOM() { ZZ8_UOM = "NER" } } },
				new RefCusTariff() { ZZ1_TariffCode = "0806200000", RefCusConditions = new RefCusCondition[] { new RefCusCondition() { ZX1_Comment = "9 * KGM" } } },
				new RefCusTariff() { ZZ1_TariffCode = "0806203000", RefCusConditions = new RefCusCondition[] { new RefCusCondition() { ZX1_Comment = "11 * TNE" } } }
			};

			var exportTariffProducer = new ExportTariffProducerForTest();
			var result = exportTariffProducer.LoadTariffsExposed(allTariffs, processedTariff);

			Assert.That(result.Count(), Is.EqualTo(5));

			AssertRefCusTariff(result, "0806101005", new string[] { "3 * E", "5 * Q" }, new string[] { "KGM", "NER" });
			AssertRefCusTariff(result, "0806101090", new string[] { "3 * E", "5 * Q" }, new string[] { "KGM", "NER" });
			AssertRefCusTariff(result, "0806203010", new string[] { "3 * E", "9 * KGM", "11 * TNE" }, new string[] { "KGM" });
			AssertRefCusTariff(result, "0806203090", new string[] { "3 * E", "9 * KGM", "11 * TNE" }, new string[] { "KGM" });
			AssertRefCusTariff(result, "0102291050", null, new string[] { "KGM" });
		}

		void AssertRefCusTariff(IEnumerable<RefCusTariff> result, string tariffCode, string[] expectedRateFormula, string[] expectedUoms)
		{
			Assert.Contains(tariffCode, result.Select(x => x.ZZ1_TariffCode).ToArray());

			var tariff = result.Single(x => x.ZZ1_TariffCode == tariffCode);

			if (expectedRateFormula == null)
			{
				Assert.That(tariff.RefCusConditions, Is.Null);
			}
			else
			{
				Assert.That(tariff.RefCusConditions, Is.Not.Null);
				CollectionAssert.AreEqual(expectedRateFormula, tariff.RefCusConditions.Select(x => x.ZX1_Comment).ToArray());
			}

			if (expectedUoms == null)
			{
				Assert.That(tariff.RefCusTariffUOMs, Is.Null);
			}
			else
			{
				Assert.That(tariff.RefCusTariffUOMs, Is.Not.Null);
				CollectionAssert.AreEqual(expectedUoms, tariff.RefCusTariffUOMs.Select(x => x.ZZ8_UOM).ToArray());
			}
		}

		class ExportTariffProducerForTest : ExportTariffProducer
		{
			public TariffXmlProducer GetNewTariffXmlProducerExposed() => base.GetNewTariffXmlProducer();

			public TariffXmlProducer GetNewTariffXmlProducerIncludingCompositeKeyExposed(bool includeCompositeKey) => base.GetNewTariffXmlProducerIncludingCompositeKey(includeCompositeKey);

			public IEnumerable<RefCusTariff> LoadTariffsExposed(IEnumerable<RefCusTariff> allTariffs, IEnumerable<RefCusTariff> processedTariffs) => LoadTariffs(allTariffs, processedTariffs);
		}
	}
}
