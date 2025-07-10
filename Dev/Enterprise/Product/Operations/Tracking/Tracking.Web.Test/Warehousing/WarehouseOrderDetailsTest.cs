using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class WarehouseOrderDetailsTest : WarehousingPageBaseTest
	{
		#region Overrides

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.WarehouseOrderDetails;
		}

		protected override void AssertAuthorisedContent(BooleanRegistryItem useModule, OrgSecurityContacts contactSecurity)
		{
			base.AssertAuthorisedContent(useModule, contactSecurity);
			AssertEquals("Edit Button Enabled", useModule.Value && contactSecurity.OZ_Granted, ((WarehouseOrderDetailsForTest)TestPage).EditOrderForTesting.Enabled);
			AssertEquals("Cancel Button Enabled", useModule.Value && contactSecurity.OZ_Granted, ((WarehouseOrderDetailsForTest)TestPage).CancelOrderForTesting.Enabled);
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new WarehouseOrderDetailsForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebWarehouseOrdersModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebWarehouseOrdersView; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			((WarehouseOrderDetailsForTest)TestPage).SetupPageForTesting();
		}

		#endregion

		public void TestDuplicateOrder()
		{
			var page = (WarehouseOrderDetailsForTest)TestPage;
			page.OnDuplicateOrderClickForTest();
			var newOrderPK = GetRedirectReferencePK();
			var newOrder = page.Factory.Load<WhsOrder>(newOrderPK);
			AssertEquals(page.SiteUser.ContactAndCompanyReference, newOrder.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestEditOrder_Click_NullRef()
		{
			var testPage = TestPage as WarehouseOrderDetailsForTest;
			var order = testPage.TestOrder;
			testPage.TestOrder = null;
			AssertNoExceptionThrown(testPage.OnEditOrderClickForTest);

			testPage.TestOrder = order;
		}

		public void TestCancelOrder_Click_NullRef()
		{
			var testPage = TestPage as WarehouseOrderDetailsForTest;
			testPage.TestOrder = null;
			AssertNoExceptionThrown(testPage.OnCancelOrderClickForTest);
		}
	}
}
