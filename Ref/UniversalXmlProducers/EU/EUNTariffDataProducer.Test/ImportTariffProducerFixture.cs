using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class ImportTariffProducerFixture
	{
		[Test]
		public void TestFilesToLocate()
		{
			var tariffProducer = new ImportTariffProducer();
			Assert.That(tariffProducer.FilesToLocate, Is.EqualTo(new[] { "Duties Import", "Duties Export", "Measure exclusions", "Measure conditions" }));
		}

		[Test]
		public void TestGetNewTariffXmlProducer()
		{
			var tariffProducer = new ImportTariffProducerForTest();
			Assert.That(tariffProducer.GetNewTariffXmlProducerExposed().GetType(), Is.EqualTo(typeof(ImportTariffXmlProducer)));
		}

		[Test]
		public void GetNewTariffXmlProducerIncludingCompositeKey()
		{
			var tariffProducer = new ImportTariffProducerForTest();
			Assert.That(tariffProducer.GetNewTariffXmlProducerIncludingCompositeKeyExposed(true).GetType(), Is.EqualTo(typeof(ImportTariffXmlProducer)));
		}

		[Test]
		public void TestLoadTariffs()
		{
			var allTariffs = new List<RefCusTariff>
			{
				new RefCusTariff() { ZZ1_TariffCode = "0806000000", ZZ1_Description = "Horses", RefCusTariffLanguages = new RefCusTariffLanguage[] { new RefCusTariffLanguage() { ZX7_Description = "French Desc" } } },
				new RefCusTariff() { ZZ1_TariffCode = "0806100000", ZZ1_Description = "Bovines", RefCusTariffLanguages = new RefCusTariffLanguage[] { new RefCusTariffLanguage() { ZX7_Description = "Spanish Desc" } } },
				new RefCusTariff() { ZZ1_TariffCode = "0806200000", ZZ1_Description = "Cattle", RefCusTariffLanguages = new RefCusTariffLanguage[] { new RefCusTariffLanguage() { ZX7_Description = "Italian Desc" } } },
				new RefCusTariff() { ZZ1_TariffCode = "0806203000" },
				new RefCusTariff() { ZZ1_TariffCode = "0102291050", ZZ1_Description = "Heifers", RefCusTariffLanguages = new RefCusTariffLanguage[] { new RefCusTariffLanguage() { ZX7_Description = "German Desc" } } }
			};

			var processedTariff = new List<RefCusTariff>
			{
				new RefCusTariff() { ZZ1_TariffCode = "0806000000", ZZ1_Description = "Horses" },
				new RefCusTariff() { ZZ1_TariffCode = "0806100000", ZZ1_Description = "Bovines" },
				new RefCusTariff() { ZZ1_TariffCode = "0806200000", ZZ1_Description = "Cattle" },
				new RefCusTariff() { ZZ1_TariffCode = "0806203000", ZZ1_Description = "Heifers" }
			};

			var tariffProducer = new ImportTariffProducerForTest();
			var result = tariffProducer.LoadTariffsExposed(allTariffs, processedTariff);

			Assert.That(result.Count(), Is.EqualTo(4));

			AssertRefCusTariff(result, "0806000000", new string[] { "French Desc" });
			AssertRefCusTariff(result, "0806100000", new string[] { "Spanish Desc" });
			AssertRefCusTariff(result, "0806200000", new string[] { "Italian Desc" });
			AssertRefCusTariff(result, "0806203000", null);
		}

		[Test]
		public void LoadTariffs_EdgeCaseWhenTariffsHasTheSameCompositeKey()
		{
			var tariffsWithDuplicatedCompositeKey = new List<RefCusTariff>
			{
				new RefCusTariff() { ZZ1_TariffCode = "0806000000", ZZ1_Description = "Horses", ZZ1_CompositeKeyOnZZ5 = "08..06.01.00", RefCusTariffLanguages = new[] { new RefCusTariffLanguage() { ZX7_Description = "French Desc" } } },
				new RefCusTariff() { ZZ1_TariffCode = "0806100000", ZZ1_Description = "Bovines", ZZ1_CompositeKeyOnZZ5 = "08..06.01.00", RefCusTariffLanguages = new[] { new RefCusTariffLanguage() { ZX7_Description = "Spanish Desc" } } },
				new RefCusTariff() { ZZ1_TariffCode = "0806200000", ZZ1_Description = "Cattle", ZZ1_CompositeKeyOnZZ5 = "08..06.01.01", RefCusTariffLanguages = new[] { new RefCusTariffLanguage() { ZX7_Description = "Italian Desc" } } },
				new RefCusTariff() { ZZ1_TariffCode = "0806203000" },
				new RefCusTariff() { ZZ1_TariffCode = "0102291050", ZZ1_Description = "Heifers", ZZ1_CompositeKeyOnZZ5 = "08..06.01.03", RefCusTariffLanguages = new[] { new RefCusTariffLanguage() { ZX7_Description = "German Desc" } } }
			};

			var processedTariff = new List<RefCusTariff>
			{
				new RefCusTariff() { ZZ1_TariffCode = "0806000000", ZZ1_Description = "Horses" },
				new RefCusTariff() { ZZ1_TariffCode = "0806100000", ZZ1_Description = "Bovines" },
				new RefCusTariff() { ZZ1_TariffCode = "0806200000", ZZ1_Description = "Cattle" },
				new RefCusTariff() { ZZ1_TariffCode = "0806203000", ZZ1_Description = "Heifers" }
			};

			var tariffProducer = new ImportTariffProducerForTest();
			var result = tariffProducer.LoadTariffsExposed(tariffsWithDuplicatedCompositeKey, processedTariff);

			Assert.That(result.Count(), Is.EqualTo(4));

			AssertRefCusTariff(result, "0806000000", new string[] { "French Desc" });
			AssertRefCusTariff(result, "0806100000", new string[] { "Spanish Desc" });
			AssertRefCusTariff(result, "0806200000", new string[] { "Italian Desc" });
			AssertRefCusTariff(result, "0806203000", null);

		}

		void AssertRefCusTariff(IEnumerable<RefCusTariff> result, string tariffCode, string[] expectedRefCusTariffLanguages)
		{
			Assert.Contains(tariffCode, result.Select(x => x.ZZ1_TariffCode).ToArray());

			var tariff = result.Single(x => x.ZZ1_TariffCode == tariffCode);

			if (expectedRefCusTariffLanguages == null)
			{
				Assert.That(tariff.RefCusTariffLanguages, Is.Null);
			}
			else
			{
				Assert.That(tariff.RefCusTariffLanguages, Is.Not.Null);
				CollectionAssert.AreEqual(expectedRefCusTariffLanguages, tariff.RefCusTariffLanguages.Select(x => x.ZX7_Description).ToArray());
			}
		}

		class ImportTariffProducerForTest : ImportTariffProducer
		{
			public TariffXmlProducer GetNewTariffXmlProducerExposed() => base.GetNewTariffXmlProducer();

			public TariffXmlProducer GetNewTariffXmlProducerIncludingCompositeKeyExposed(bool includeCompositeKey) => base.GetNewTariffXmlProducerIncludingCompositeKey(includeCompositeKey);

			public IEnumerable<RefCusTariff> LoadTariffsExposed(IEnumerable<RefCusTariff> allTariffs, IEnumerable<RefCusTariff> processedTariffs) => LoadTariffs(allTariffs, processedTariffs);
		}
	}
}
