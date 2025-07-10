using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers;
using NUnit.Framework;
using PolishReferenceData.Tariff;

namespace PolishReferenceData.Test.Tariff
{
	[TestFixture]
	public class TariffUniversalReferenceDataXmlGeneratorTest : TariffUniversalReferenceDataXmlGenerator
	{
		List<RefCusTariff> plTariffListForTest;
		List<RefCusTariff> eunTariffListForTest;

		[Test]
		public void TestMergeImportTariffsWithEUNTarrifs()
		{
			var expected = new List<RefCusTariff>()
			{
				new RefCusTariff {
					ZZ1_TariffCode = "01012100"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "03011100"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "04011010"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "05010000"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "06011010"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019010"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019020"
				}
			};

			var expander = new CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.EUNTariffExpander(null, eunTariffListForTest);
			var result  = expander.MergeExportTariffsWithEUNTarrifs(plTariffListForTest);

			Assert.AreEqual(expected.Count, result.Count);

			foreach (var item in expected)
			{
				var whereResult = result.Where(x => x.ZZ1_TariffCode == item.ZZ1_TariffCode).ToList();
				Assert.AreEqual(1, whereResult.Count);
			}
		}

		public void MergeImportTariffsWithEUNTarrifsFailingTest()
		{
			var expectedToFailData = new List<RefCusTariff>()
			{
				new RefCusTariff {
					ZZ1_TariffCode = "01012100"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "03011100"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "04011010"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "05010000"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "06011010"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019010"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "07019020"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "13012000"
				},
				new RefCusTariff {
					ZZ1_TariffCode = "24011035"
				}
			};

			var expander = new CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.EUNTariffExpander(null, eunTariffListForTest);
			var result = expander.MergeExportTariffsWithEUNTarrifs(plTariffListForTest);

			Assert.AreNotEqual(expectedToFailData.Count, result.Count);
		}

		[SetUp]
		public void Setup()
		{
			plTariffListForTest = new List<RefCusTariff>();
			eunTariffListForTest = new List<RefCusTariff>();

			// treat as exists in PL Data
			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "01010000" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "01012100" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "03010000" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "03011100" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "04010000" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "04011010" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "05010000" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "05010000" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "06010000" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "06011010" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07000000" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019010" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019020" });

			// treat as not exists in PL Data
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "13012000" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "24011035" });
		}
	}
}
