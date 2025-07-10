using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class TariffColumn2DataRowTest
	{
		[Test]
		public void TestTariffColumn2DataRow()
		{
			string lineData = "01012100GT9999999920131129          0000000000                                    GT        ";
			var tariffColumn2DataRow = new TariffColumn2DataRow(lineData);
			Assert.AreEqual("01012100", tariffColumn2DataRow.TariffCode);
			Assert.AreEqual("GT", tariffColumn2DataRow.CountryCode);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), tariffColumn2DataRow.EndDate);
			Assert.AreEqual(new DateTime(2013, 11, 29), tariffColumn2DataRow.StartDate);
			Assert.AreEqual(0m, tariffColumn2DataRow.SpecificRate);
			Assert.AreEqual(0m, tariffColumn2DataRow.AdValoremRate);
			Assert.AreEqual(DateTime.MinValue, tariffColumn2DataRow.ProvisionalEndDate);
			Assert.AreEqual(DateTime.MinValue, tariffColumn2DataRow.ProvisionalStartDate);
			Assert.AreEqual(0m, tariffColumn2DataRow.ProvisionalSpecificRate);
			Assert.AreEqual(0m, tariffColumn2DataRow.ProvisionalAdValoremRate);
			Assert.AreEqual("GT", tariffColumn2DataRow.Region);
			Assert.AreEqual(lineData, tariffColumn2DataRow.LineData);

			lineData = "00000000US201911112018072400000003000000000400201705032016050300000100000000030000XX";
			tariffColumn2DataRow = new TariffColumn2DataRow(lineData);
			Assert.AreEqual("00000000", tariffColumn2DataRow.TariffCode);
			Assert.AreEqual("US", tariffColumn2DataRow.CountryCode);
			Assert.AreEqual(new DateTime(2019, 11, 11, 23, 59, 59), tariffColumn2DataRow.EndDate);
			Assert.AreEqual(new DateTime(2018, 07, 24), tariffColumn2DataRow.StartDate);
			Assert.AreEqual(0.003m, tariffColumn2DataRow.SpecificRate);
			Assert.AreEqual(0.004m, tariffColumn2DataRow.AdValoremRate);
			Assert.AreEqual(new DateTime(2017, 05, 03, 23, 59, 59), tariffColumn2DataRow.ProvisionalEndDate);
			Assert.AreEqual(new DateTime(2016, 05, 03), tariffColumn2DataRow.ProvisionalStartDate);
			Assert.AreEqual(0.1m, tariffColumn2DataRow.ProvisionalSpecificRate);
			Assert.AreEqual(0.3m, tariffColumn2DataRow.ProvisionalAdValoremRate);
			Assert.AreEqual("XX", tariffColumn2DataRow.Region);
			Assert.AreEqual(lineData, tariffColumn2DataRow.LineData);
		}
	}
}
