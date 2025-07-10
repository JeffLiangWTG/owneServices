using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public sealed class CIN750DeconsNotificationBuilderTest : CIN750NotificationBuilderTest<CIN750DeconsNotificationBuilder, WhsItemDispatchConsignment, CIN750DeconsNotification>
	{
		public void TestBuild()
		{
			var dcn = CreateGeneralDCN();
			dcn.WDC_HouseBillNumber = "HSB1";

			var deconsNotification = new CIN750DeconsNotificationBuilder(dcn).Build();
			AssertNotNull(deconsNotification);

			AssertEquals("DeclaredInWarehouse", "WH1Address", deconsNotification.DeclaredInWarehouse.AddressLine1);
		}

		protected override void TestRemoveHypenCore()
		{
			var dcn = CreateGeneralDCN();
			Factory.Save();

			dcn.WDC_HouseBillNumber = "HB-1";

			var deconsNotification = new CIN750DeconsNotificationBuilder(dcn).Build();
			AssertNotNull(deconsNotification);

			AssertEquals("RefType", "HWB", deconsNotification.RefType.Code);
			AssertEquals("RefCode", "HB1", deconsNotification.RefCode);
		}

		protected override CIN750DeconsNotification GetGeneralNotification()
		{
			var dcn = CreateGeneralDCN();

			return new CIN750DeconsNotificationBuilder(dcn).Build();
		}
	}
}
