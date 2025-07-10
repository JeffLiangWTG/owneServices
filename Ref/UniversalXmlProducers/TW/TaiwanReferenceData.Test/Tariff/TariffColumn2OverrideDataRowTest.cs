using System;
using NUnit.Framework;


namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class TariffColumn2OverrideDataRowTest
	{
		[Test]
		public void TestTariffColumn2OverrideDataRow()
		{
			string lineData = "04011010,NZ,20191231,20190101,0,,";
			var tariffColumn2OverrideDataRow = new TariffColumn2OverrideDataRow(lineData);
			Assert.AreEqual("04011010", tariffColumn2OverrideDataRow.TariffCode);
			Assert.AreEqual("NZ", tariffColumn2OverrideDataRow.CountryCode);
			Assert.AreEqual(new DateTime(2019, 12, 31), tariffColumn2OverrideDataRow.ProvisionalEndDate);
			Assert.AreEqual(new DateTime(2019, 01, 01), tariffColumn2OverrideDataRow.ProvisionalStartDate);
			Assert.AreEqual(0m, tariffColumn2OverrideDataRow.ProvisionalSpecificRate);
			Assert.AreEqual(0m, tariffColumn2OverrideDataRow.ProvisionalAdValoremRate);
			Assert.AreEqual(string.Empty, tariffColumn2OverrideDataRow.OrderNumber);

			lineData = "04011010,US,20201231,20200101,0000300000,0000400000,AAA";
			tariffColumn2OverrideDataRow = new TariffColumn2OverrideDataRow(lineData);
			Assert.AreEqual("04011010", tariffColumn2OverrideDataRow.TariffCode);
			Assert.AreEqual("US", tariffColumn2OverrideDataRow.CountryCode);
			Assert.AreEqual(new DateTime(2020, 12, 31), tariffColumn2OverrideDataRow.ProvisionalEndDate);
			Assert.AreEqual(new DateTime(2020, 01, 01), tariffColumn2OverrideDataRow.ProvisionalStartDate);
			Assert.AreEqual(3m, tariffColumn2OverrideDataRow.ProvisionalSpecificRate);
			Assert.AreEqual(4m, tariffColumn2OverrideDataRow.ProvisionalAdValoremRate);
			Assert.AreEqual("AAA", tariffColumn2OverrideDataRow.OrderNumber);
		}
	}
}
