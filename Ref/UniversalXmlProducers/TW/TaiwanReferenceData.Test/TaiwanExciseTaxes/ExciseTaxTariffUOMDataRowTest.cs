using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	sealed class ExciseTaxTariffUOMDataRowTest
	{
		[Test]
		public void TestExciseTaxTariffUOMDataRow()
		{
			var type = "";
			var uom = "LTR";
			var testData = ExciseTaxTariffUOMDataRow.New(type, uom);
			Assert.IsNull(testData);

			type = "CU1";
			testData = ExciseTaxTariffUOMDataRow.New(type, uom);
			Assert.AreEqual("CU1", testData.ZZ8_Type);
			Assert.AreEqual("LTR", testData.ZZ8_UOM);
		}
	}
}
