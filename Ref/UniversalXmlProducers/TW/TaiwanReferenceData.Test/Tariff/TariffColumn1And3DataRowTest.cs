using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class TariffColumn1And3DataRowTest
	{
		[Test]
		public void TestTariffColumn1And3DataRow()
		{
			string lineData = "010121000039999999920131129          0000002500          0000002500                                                           HEDKGM            401 B01                     441                         ";
			var tariffColumn1And3DataRow = new TariffColumn1And3DataRow(lineData);
			Assert.AreEqual("01012100003", tariffColumn1And3DataRow.TariffCode);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), tariffColumn1And3DataRow.EndDate);
			Assert.AreEqual(new DateTime(2013, 11, 29), tariffColumn1And3DataRow.StartDate);
			Assert.AreEqual(0m, tariffColumn1And3DataRow.Column3SpecificRate);
			Assert.AreEqual(0.025m, tariffColumn1And3DataRow.Column3AdValoremRate);
			Assert.AreEqual(0m, tariffColumn1And3DataRow.Column1SpecificRate);
			Assert.AreEqual(0.025m, tariffColumn1And3DataRow.Column1AdValoremRate);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.SpecificRateUnit);
			Assert.AreEqual(0m, tariffColumn1And3DataRow.Column3ProvisionalSpecificRate);
			Assert.AreEqual(0m, tariffColumn1And3DataRow.Column3ProvisionalAdValoremRate);
			Assert.AreEqual(0m, tariffColumn1And3DataRow.Column1ProvisionalSpecificRate);
			Assert.AreEqual(0m, tariffColumn1And3DataRow.Column1ProvisionalAdValoremRate);
			Assert.AreEqual(DateTime.MinValue, tariffColumn1And3DataRow.ProvisionalEndDate);
			Assert.AreEqual(DateTime.MinValue, tariffColumn1And3DataRow.ProvisionalStartDate);
			Assert.AreEqual("HED", tariffColumn1And3DataRow.QuantityUnit);
			Assert.AreEqual("KGM", tariffColumn1And3DataRow.WeightUnit);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.CustomsRequirementCode);
			Assert.AreEqual("401", tariffColumn1And3DataRow.ImportRegulationCode1);
			Assert.AreEqual("B01", tariffColumn1And3DataRow.ImportRegulationCode2);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ImportRegulationCode3);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ImportRegulationCode4);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ImportRegulationCode5);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ImportRegulationCode6);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ImportRegulationCode7);
			Assert.AreEqual("441", tariffColumn1And3DataRow.ExportRegulationCode1);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ExportRegulationCode2);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ExportRegulationCode3);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ExportRegulationCode4);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ExportRegulationCode5);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ExportRegulationCode6);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.ExportRegulationCode7);

			lineData = "0000000000020191231201807060000002100000000220000000023000000002400KGM00000025000000002600000000270000000028002021063020180930HEDKGMXXXXXXXXXXXX4011B012AAAABBBBCCCCDDDDEEEE5555666677778888999911112222";
			tariffColumn1And3DataRow = new TariffColumn1And3DataRow(lineData);
			Assert.AreEqual("00000000000", tariffColumn1And3DataRow.TariffCode);
			Assert.AreEqual(new DateTime(2019, 12, 31, 23, 59, 59), tariffColumn1And3DataRow.EndDate);
			Assert.AreEqual(new DateTime(2018, 07, 06), tariffColumn1And3DataRow.StartDate);
			Assert.AreEqual(0.021m, tariffColumn1And3DataRow.Column3SpecificRate);
			Assert.AreEqual(0.022m, tariffColumn1And3DataRow.Column3AdValoremRate);
			Assert.AreEqual(0.023m, tariffColumn1And3DataRow.Column1SpecificRate);
			Assert.AreEqual(0.024m, tariffColumn1And3DataRow.Column1AdValoremRate);
			Assert.AreEqual("KGM", tariffColumn1And3DataRow.SpecificRateUnit);
			Assert.AreEqual(0.025m, tariffColumn1And3DataRow.Column3ProvisionalSpecificRate);
			Assert.AreEqual(0.026m, tariffColumn1And3DataRow.Column3ProvisionalAdValoremRate);
			Assert.AreEqual(0.027m, tariffColumn1And3DataRow.Column1ProvisionalSpecificRate);
			Assert.AreEqual(0.028m, tariffColumn1And3DataRow.Column1ProvisionalAdValoremRate);
			Assert.AreEqual(new DateTime(2021, 06, 30, 23, 59, 59), tariffColumn1And3DataRow.ProvisionalEndDate);
			Assert.AreEqual(new DateTime(2018, 09, 30), tariffColumn1And3DataRow.ProvisionalStartDate);
			Assert.AreEqual("HED", tariffColumn1And3DataRow.QuantityUnit);
			Assert.AreEqual("KGM", tariffColumn1And3DataRow.WeightUnit);
			Assert.AreEqual("XXXXXXXXXXXX", tariffColumn1And3DataRow.CustomsRequirementCode);
			Assert.AreEqual("4011", tariffColumn1And3DataRow.ImportRegulationCode1);
			Assert.AreEqual("B012", tariffColumn1And3DataRow.ImportRegulationCode2);
			Assert.AreEqual("AAAA", tariffColumn1And3DataRow.ImportRegulationCode3);
			Assert.AreEqual("BBBB", tariffColumn1And3DataRow.ImportRegulationCode4);
			Assert.AreEqual("CCCC", tariffColumn1And3DataRow.ImportRegulationCode5);
			Assert.AreEqual("DDDD", tariffColumn1And3DataRow.ImportRegulationCode6);
			Assert.AreEqual("EEEE", tariffColumn1And3DataRow.ImportRegulationCode7);
			Assert.AreEqual("5555", tariffColumn1And3DataRow.ExportRegulationCode1);
			Assert.AreEqual("6666", tariffColumn1And3DataRow.ExportRegulationCode2);
			Assert.AreEqual("7777", tariffColumn1And3DataRow.ExportRegulationCode3);
			Assert.AreEqual("8888", tariffColumn1And3DataRow.ExportRegulationCode4);
			Assert.AreEqual("9999", tariffColumn1And3DataRow.ExportRegulationCode5);
			Assert.AreEqual("1111", tariffColumn1And3DataRow.ExportRegulationCode6);
			Assert.AreEqual("2222", tariffColumn1And3DataRow.ExportRegulationCode7);
		}


		[Test]
		public void TestTariffUOM_OnlyHasQuantityUnit()
		{
			string lineData = "271600000019999999920130101          0000000000          0000000000                                                           KVA                                                                       ";
			var tariffColumn1And3DataRow = new TariffColumn1And3DataRow(lineData);
			Assert.AreEqual("KVA", tariffColumn1And3DataRow.QuantityUnit);
			Assert.AreEqual(string.Empty, tariffColumn1And3DataRow.WeightUnit);
			var tariffUOM = tariffColumn1And3DataRow.GetTariffUOM().ToList();
			Assert.AreEqual(1, tariffUOM.Count);
			var refCusTariffUOM = tariffUOM[0];
			Assert.AreEqual("CU2", refCusTariffUOM.ZZ8_Type);
			Assert.AreEqual("KVA", refCusTariffUOM.ZZ8_UOM);
		}
	}
}
