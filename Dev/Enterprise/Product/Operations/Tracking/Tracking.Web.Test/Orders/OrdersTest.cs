using System.Collections.Generic;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class OrdersTest : BasePageWithAuthorisationTest
	{
		#region Overrides

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.Orders;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new TestOrders();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebForwardingOrdersModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebOrdersView; }
		}

		protected override void AssertAuthorisedContent(BooleanRegistryItem useModule, OrgSecurityContacts contactSecurity)
		{
			base.AssertAuthorisedContent(useModule, contactSecurity);
			AssertEquals("Hyperlink visibility", useModule.Value && contactSecurity.OZ_Granted, TestPage.SwitchHyperLinkForTesting.Visible);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestPage.SetupPageForTesting();
		}

		new TestOrders TestPage
		{
			get { return base.TestPage as TestOrders; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerOrderManager }; }
		}

		#endregion

		public void TestGetTimeLineViewMode()
		{
			TestPage.InitControls();
			TestPage.ViewModeInSessionForTest = null;
			TestPage.ReadCount = 0;
			TestPage.WriteCount = 0;
			TestPage.HandleModeSwitchClickForTest();

			AssertEquals("Mode should be read from the registry once", 1, TestPage.ReadCount);
			AssertEquals("Mode should not be written to the registry", 0, TestPage.WriteCount);
			AssertEquals("TestPage.fTimeLineSession should be TimeLine", Orders.Orders.TimeLine, TestPage.ViewModeInSessionForTest);
			AssertEquals("Orders.IsTimeline should be true", true, TestPage.IsTimeLineForTest);

			TestPage.IsSwitchHyperLinkClickForTest = true;
			TestPage.HandleModeSwitchClickForTest();

			AssertEquals("Mode should be read from the registry once", 1, TestPage.ReadCount);
			AssertEquals("Mode should be written to the registry once", 1, TestPage.WriteCount);
			AssertEquals("TestPage.fTimeLineSession should be Normal", Orders.Orders.Normal, TestPage.ViewModeInSessionForTest);
			AssertEquals("Orders.IsTimeline should be false", false, TestPage.IsTimeLineForTest);

			TestPage.IsSwitchHyperLinkClickForTest = false;
			TestPage.HandleModeSwitchClickForTest();

			AssertEquals("Mode should be read from the registry once", 1, TestPage.ReadCount);
			AssertEquals("Mode should be written to the registry once", 1, TestPage.WriteCount);
			AssertEquals("TestPage.fTimeLineSession should be Normal", Orders.Orders.Normal, TestPage.ViewModeInSessionForTest);
			AssertEquals("Orders.IsTimeline should be false", false, TestPage.IsTimeLineForTest);

			TestPage.IsSwitchHyperLinkClickForTest = true;
			TestPage.HandleModeSwitchClickForTest();

			AssertEquals("Mode should be read from the registry once", 1, TestPage.ReadCount);
			AssertEquals("Mode should be written to the registry once", 2, TestPage.WriteCount);
			AssertEquals("TestPage.fTimeLineSession should be Normal", Orders.Orders.TimeLine, TestPage.ViewModeInSessionForTest);
			AssertEquals("Orders.IsTimeline should be false", true, TestPage.IsTimeLineForTest);

			TestPage.IsSwitchHyperLinkClickForTest = false;
			TestPage.HandleModeSwitchClickForTest();

			AssertEquals("Mode should be read from the registry once", 1, TestPage.ReadCount);
			AssertEquals("Mode should be written to the registry once", 2, TestPage.WriteCount);
			AssertEquals("TestPage.fTimeLineSession should be Normal", Orders.Orders.TimeLine, TestPage.ViewModeInSessionForTest);
			AssertEquals("Orders.IsTimeline should be false", true, TestPage.IsTimeLineForTest);
		}
	}
}
