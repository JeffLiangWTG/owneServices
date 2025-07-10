using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class ShipmentTypeCodeList : TestCaseWithFactory
	{
		public void TestShipmentTypeExportCodeList()
		{
			var shipmentTypeExport = new ShipmentTypeExport();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, shipmentTypeExport.Count);
				AssertEquals("ShipmentTypeExport: EU", "Export of Goods to an EU, EEA or EFTA Member State", shipmentTypeExport.GetDescriptionFromCode("EU"));
				AssertEquals("ShipmentTypeExport: EX", "Export of Goods (to all other not covered by EU)", shipmentTypeExport.GetDescriptionFromCode("EX"));
			});
		}

		public void TestShipmentTypeImportCodeList()
		{
			var shipmentTypeImport = new ShipmentTypeImport();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, shipmentTypeImport.Count);
				AssertEquals("ShipmentTypeExport: EU", "Import of Goods from an EU, EEA or EFTA Member State", shipmentTypeImport.GetDescriptionFromCode("EU"));
				AssertEquals("ShipmentTypeExport: IM", "Import of Goods (from all other not covered by EU)", shipmentTypeImport.GetDescriptionFromCode("IM"));
			});
		}
	}
}
