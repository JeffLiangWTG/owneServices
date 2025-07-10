using CargoWise.Definitions;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public sealed class CIN750ConsNotificationBuilderTest : CIN750NotificationBuilderTest<CIN750ConsNotificationBuilder, WhsItemDispatchConsignment, CIN750ConsNotification>
	{
		#region TestRefType

		public void TestRefType_HasMasterBill()
		{
			var dcn = CreateGeneralDCN();
			Helper.CreateAdditionalReference(dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			dcn.WDC_HouseBillNumber = "HWB1";
			Factory.Save();

			var notification = new CIN750ConsNotificationBuilder(dcn, null).Build();
			AssertNotNull(notification);
			AssertEquals("AWB", notification.RefType.Code);
			AssertEquals("MAB1", notification.RefCode);
		}

		public void TestRefType_HasHouseBill()
		{
			var dcn = CreateGeneralDCN();
			dcn.WDC_HouseBillNumber = "HWB1";
			Factory.Save();

			var notification = new CIN750ConsNotificationBuilder(dcn, null).Build();
			AssertNotNull(notification);
			AssertEquals("HWB", notification.RefType.Code);
			AssertEquals("HWB1", notification.RefCode);
		}

		protected override void TestRemoveHypenCore()
		{
			var dcn = CreateGeneralDCN();
			Factory.Save();
			dcn.WDC_HouseBillNumber = "HWB-1";

			var consNotification = new CIN750ConsNotificationBuilder(dcn, null).Build();
			AssertNotNull(consNotification);

			AssertEquals("RefType", "HWB", consNotification.RefType.Code);
			AssertEquals("RefCode", "HWB1", consNotification.RefCode);
		}

		#endregion

		protected override CIN750ConsNotification GetGeneralNotification()
		{
			var dcn = CreateGeneralDCN();

			return new CIN750ConsNotificationBuilder(dcn, null).Build();
		}
	}
}
