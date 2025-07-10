using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class EUNTariffExpanderFixture
	{
		[Test]
		public void TestMergeImportTariffsWithEUNTarrifs()
		{
			TariffCodeExtractor.ClearCache();

			var expander = new EUNTariffExpander(eunIMPTariffListForTest, eunEXPTariffListForTest);
			var uomForTest = new RefCusTariffUOM() { ZZ8_Type = "CU2", ZZ8_UOM = "KG" };
			var testInput = new List<RefCusTariff>();
			testInput.Add(new RefCusTariff() { ZZ1_TariffCode = "0100", ZZ1_Description = "Test Description" });
			testInput.Add(new RefCusTariff() { ZZ1_TariffCode = "0700", ZZ1_Description = "Test Description", RefCusTariffUOMs = new[] { uomForTest } });
			testInput.Add(new RefCusTariff() { ZZ1_TariffCode = "7501100000", ZZ1_Description = "Test Description" });

			var result = expander.MergeImportTariffsWithEUNTarrifs(testInput);

			Assert.AreEqual(4, result.Count);
			var resultList = result.Select(tariff => tariff.ZZ1_TariffCode).ToArray();
			Assert.Contains("0101210000", resultList);
			Assert.Contains("0701901000", resultList);
			Assert.Contains("0701902000", resultList);
			Assert.Contains("7501100000", resultList);

			Assert.AreEqual("KG", result.FirstOrDefault(tar => tar.ZZ1_TariffCode == "0701901000").RefCusTariffUOMs.FirstOrDefault(uom => uom.ZZ8_Type == "CU2").ZZ8_UOM);
			Assert.AreEqual("KG", result.FirstOrDefault(tar => tar.ZZ1_TariffCode == "0701902000").RefCusTariffUOMs.FirstOrDefault(uom => uom.ZZ8_Type == "CU2").ZZ8_UOM);
		}

		#region Setup

		List<RefCusTariff> eunIMPTariffListForTest;
		List<RefCusTariff> eunEXPTariffListForTest;

		[SetUp]
		public void Setup()
		{
			#region EXP TariffList

			eunEXPTariffListForTest = new List<RefCusTariff>();
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "01012100", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "03011100", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "04011010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "05010000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "06011010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019020", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "08011100", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "09011100", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "10011100", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "11010011", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "12011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "13012000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "24011035", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "25010010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "46012110", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "47010010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "48010000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "64011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "65010000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "66011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "67010000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "68010000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "69010000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "70010010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "71011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "72011011", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "73011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "74010000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "75011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "76011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "78011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "79011100", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "80011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "81011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "82011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "83011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "84011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "85011010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "86011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "87011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "88010010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "89011010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "90011010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "91011100", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "92011010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "93011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "94011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "95030010", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "97011000", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "98800100", ZZ1_Description = "Test Description" });
			eunEXPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "99050000", ZZ1_Description = "Test Description" });

			#endregion

			#region IMP TariffList

			eunIMPTariffListForTest = new List<RefCusTariff>();
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "0101210000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "0301110000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "0401101000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "0501000000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "0601101000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019010", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019020", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "0801110000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "0901110000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "1001110000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "1101001100", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "1201100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "1301200000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "2401103500", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "2501001000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "4601211000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "4701001000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "4801000000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "6401100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "6501000000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "6601100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "6701000000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "6801000000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "6901000000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "7001001000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "7101100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "7201101100", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "7301100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "7401000000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "7501100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "7601100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "7801100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "7901110000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8001100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8101100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8201100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8301100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8401100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8501101000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8601100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8701100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8801001000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "8901101000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "9001101000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "9101110000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "9201101000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "9301100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "9401100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "9503001000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "9701100000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "9880010000", ZZ1_Description = "Test Description" });
			eunIMPTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "9905000000", ZZ1_Description = "Test Description" });

			#endregion
		}

		#endregion
	}
}
