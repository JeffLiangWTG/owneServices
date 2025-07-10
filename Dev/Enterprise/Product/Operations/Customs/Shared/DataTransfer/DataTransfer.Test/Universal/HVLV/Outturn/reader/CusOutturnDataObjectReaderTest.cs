using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	sealed class CusOutturnDataObjectReaderTest : OutturnDataObjectReaderTestHelper<CusOutturnHeader, CusOutturn>
	{
		public void TestImportingData()
		{
			var shipment = CreateTestOutturnShipment("FCL", "CON123", "MB123", "HB123");
			Factory.SaveForTesting();

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var reader = new CusOutturnDataObjectReader<CusOutturn>(shipment, logger, Factory, outturnHeader);
			var outturn = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("outturn.C5_C6", outturnHeader.PK, outturn.C5_C6);
				AssertEquals("outturn.C5_CargoType", "FCL", outturn.C5_CargoType);
				AssertEquals("outturn.C5_ContainerNumber", "CON123", outturn.C5_ContainerNumber);
				AssertEquals("outturn.C5_ContainerSeal", "ContainerSeal", outturn.C5_ContainerSeal);
				AssertEquals("outturn.C5_SealIntactIndicator", true, outturn.C5_SealIntactIndicator);
				AssertEquals("outturn.C5_HouseBill", "HB123", outturn.C5_HouseBill);
				AssertEquals("outturn.C5_MasterBill", "MB123", outturn.C5_MasterBill);

				AssertEquals("outturn.C5_OuterPacks", 10, outturn.C5_OuterPacks);
				AssertEquals("outturn.C5_OuterPackUnits", "BAG", outturn.C5_OuterPackUnits);
				AssertEquals("outturn.C5_PackagesOutturned", 5, outturn.C5_PackagesOutturned);
				AssertEquals("outturn.C5_PackagesUnits", "BAG", outturn.C5_PackagesUnits);

				AssertEquals("outturnHeader.C5_CargoUnpackDate", new ZDateTime(2019, 08, 08), outturn.C5_CargoUnpackDate);
				AssertEquals("outturnHeader.C5_CargoReceiptDate", new ZDateTime(2019, 08, 09), outturn.C5_CargoReceiptDate);

				AssertEquals("outturn.C5_DamageIndicator", true, outturn.C5_DamageIndicator);
				AssertEquals("outturn.C5_PillageIndicator", false, outturn.C5_PillageIndicator);
				AssertEquals("outturn.C5_GoodsDescription", "GoodsDescription TEXT", outturn.C5_GoodsDescription);
				AssertEquals("outturn.C5_MarksAndNumbers", "MarksAndNumbersDescription TEXT", outturn.C5_MarksAndNumbers);
				AssertEquals("outturn.C5_OutturnResultType", "", outturn.C5_OutturnResultType);
			});
		}
	}
}
