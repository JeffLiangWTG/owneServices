using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class GoodsLocationCodeListTest : TestCaseWithFactory
	{
		public void TestExportGoodsLocationPairs()
		{
			var exportGoodsLocationList = new ExportGoodsLocationCodeList();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 4, exportGoodsLocationList.Count);
				AssertEquals("ExportGoodsLocation: Code A", "Customs Warehouse A", exportGoodsLocationList.GetDescriptionFromCode("A"));
				AssertEquals("ExportGoodsLocation: Code B", "Customs Warehouse B", exportGoodsLocationList.GetDescriptionFromCode("B"));
				AssertEquals("ExportGoodsLocation: Code C", "Customs Warehouse C", exportGoodsLocationList.GetDescriptionFromCode("C"));
				AssertEquals("ExportGoodsLocation: Code D", "Customs Warehouse D", exportGoodsLocationList.GetDescriptionFromCode("D"));
			});
		}
		public void TestImportGoodsLocationPairs()
		{
			var importGoodsLocationList = new ImportGoodsLocationCodeList();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 6, importGoodsLocationList.Count);
				AssertEquals("ImportGoodsLocation: Code A", "Customs Warehouse A", importGoodsLocationList.GetDescriptionFromCode("A"));
				AssertEquals("ImportGoodsLocation: Code B", "Customs Warehouse B", importGoodsLocationList.GetDescriptionFromCode("B"));
				AssertEquals("ImportGoodsLocation: Code C", "Customs Warehouse C", importGoodsLocationList.GetDescriptionFromCode("C"));
				AssertEquals("ImportGoodsLocation: Code D", "Customs Warehouse D", importGoodsLocationList.GetDescriptionFromCode("D"));
				AssertEquals("ImportGoodsLocation: Code A8", "10 DAY Warehouse/Consignee", importGoodsLocationList.GetDescriptionFromCode("A8"));
				AssertEquals("ImportGoodsLocation: Code X", "Other", importGoodsLocationList.GetDescriptionFromCode("X"));
			});
		}
	}
}
