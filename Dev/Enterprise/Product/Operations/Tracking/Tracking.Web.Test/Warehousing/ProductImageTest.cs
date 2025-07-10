using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ProductImageTest : WarehousingPageBaseTest
	{
		#region Overrides

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.ProductImage;
		}

		public override void TestShowLoginStatus()
		{
			AssertEquals("Should not be visible for logged in SiteUser", false, TestPage.ShowLoginStatusForTest);
			TestPage.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			AssertEquals("Should not be visible for CurrentCompany.OrgProxy web users", false, TestPage.ShowLoginStatusForTest);
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ProductImageForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebWarehouseProductsModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebWarehouseProductsView; }
		}

		#endregion
	}
}
