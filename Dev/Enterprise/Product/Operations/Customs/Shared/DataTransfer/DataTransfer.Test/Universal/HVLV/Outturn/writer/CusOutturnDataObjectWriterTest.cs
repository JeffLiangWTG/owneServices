using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	sealed class CusOutturnDataObjectWriterTest : OutturnDataObjectWriterTestHelper
	{
		public void TestMappings()
		{
			var outturn = CreateTestOutturn();

			var writer = new CusOutturnDataObjectWriter<CusOutturn>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, outturn)));
			var outturnData = writer.GetDataObject(outturn);

			AssertTestOutturn(outturn, outturnData);
		}

		CusOutturn CreateTestOutturn()
		{
			var header = Factory.New<CusOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			outturn.C5_MasterBill = "MB1";
			outturn.C5_HouseBill = "HB1";

			outturn.C5_OutturnResultType = "OTT";

			outturn.C5_OuterPacks = 9;
			outturn.C5_OuterPackUnits = "PK";
			outturn.C5_PackagesOutturned = 10;
			outturn.C5_PackagesUnits = "PK";

			outturn.C5_CustomsStatus = "HLD";
			outturn.C5_MessageStatus = "REJ";
			outturn.C5_CommercialStatus = "CMS";

			outturn.C5_DamageIndicator = true;
			outturn.C5_PillageIndicator = true;

			outturn.C5_GoodsDescription = "Cuckoo Squeakers";
			outturn.C5_MarksAndNumbers = "marks";

			outturn.C5_CargoType = "LCL";
			outturn.C5_ContainerNumber = "OCLU1233510";
			outturn.C5_ContainerSeal = "seal";
			outturn.C5_SealIntactIndicator = true;

			outturn.C5_CargoReceiptDate = new ZDateTime(2019, 7, 25, 0, 0, 0);
			outturn.C5_CargoUnpackDate = new ZDateTime(2019, 7, 28, 0, 0, 0);

			outturn.C5_VolumeOutturned = 10.00D;

			return outturn;
		}
	}
}
