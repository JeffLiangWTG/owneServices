using CargoWise.Definitions;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public sealed class CIN750CorNotificationBuilderTest : CIN750NotificationBuilderTest<CIN750CorNotificationBuilder, WhsItemReceiveConsignment, CIN750CorNotification>
	{
		public void TestBuild()
		{
			var rcn = CreateGeneralRCN();

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertNotNull(corNotification);

			AssertEquals("DeclaredInWarehouse", "WH1Address", corNotification.DeclaredInWarehouse.AddressLine1);
			AssertEquals("RefType", "AWB", corNotification.RefType.Code);
			AssertEquals("RefCode", "MAB1", corNotification.RefCode);
		}

		public void TestBuild_NoAirWayBill()
		{
			var rcn = CreateGeneralRCN();

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertNotNull(corNotification);

			AssertEquals("RefType", "REF", corNotification.RefType.Code);
			AssertEquals("RefCode", "EDIDATRC0000001", corNotification.RefCode);
		}

		protected override void TestRemoveHypenCore()
		{
			var rcn = CreateGeneralRCN();
			Factory.Save();

			rcn.WRC_ConsignmentID = "RC0000001-1";

			var corNotification = new CIN750CorNotificationBuilder(rcn).Build();
			AssertNotNull(corNotification);

			AssertEquals("RefType", "REF", corNotification.RefType.Code);
			AssertEquals("RefCode", "EDIDATRC00000011", corNotification.RefCode);
		}

		protected override CIN750CorNotification GetGeneralNotification()
		{
			var rcn = CreateGeneralRCN();
			return new CIN750CorNotificationBuilder(rcn).Build();
		}

		WhsItemReceiveConsignment CreateGeneralRCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			return Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
		}
	}
}

