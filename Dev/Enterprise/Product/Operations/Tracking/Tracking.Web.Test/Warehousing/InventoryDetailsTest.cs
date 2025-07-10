using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class InventoryDetailsTest : WarehousingPageBaseTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.WarehouseInventoryDetails;
		}

		public override void TestShowLoginStatus()
		{
			AssertEquals("Should NOT be visible for logged in SiteUser", false, TestPage.ShowLoginStatusForTest);
			TestPage.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			AssertEquals("Should be invisible for CurrentCompany.OrgProxy web users", false, TestPage.ShowLoginStatusForTest);
		}

		new InventoryDetails TestPage => base.TestPage as InventoryDetails;

		protected override System.Web.UI.Control GetNewControl()
		{
			return new InventoryDetailsForTest();
		}

		protected override BooleanRegistryItem UseWebModule => WebDataRegistry.Instance.UseWebWarehouseInventoryModule;

		protected override WebSecurityRight SiteUserSecurityRight => WebSecurityRightsList.WebInventoryView;

		class InventoryDetailsForTest : InventoryDetails
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
