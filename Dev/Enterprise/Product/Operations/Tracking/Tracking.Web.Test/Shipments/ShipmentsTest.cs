using System;
using System.Collections.Generic;
using System.Web.UI;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ShipmentsTest : BasePageWithAuthorisationTest
	{
		#region Overrides

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.Shipments;
		}

		protected override Control GetNewControl()
		{
			return new ShipmentsForTest();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestPage.SetupPageForTest();
		}

		new ShipmentsForTest TestPage
		{
			get { return base.TestPage as ShipmentsForTest; }
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebForwardingShipmentsModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebShipmentsView; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerForwarding }; }
		}

		#endregion

		#region TestModeInSessionGetter

		public void TestModeInSessionGetter()
		{
			TestPage.ZeroModeInSession();
			TestPage.ZeroRegistryVariables();
			bool b = TestPage.ModeInSessionForTest;
			AssertEquals(1, TestPage.RegistryReadCount);
			AssertEquals(1, TestPage.RegistryWriteCount);
			AssertEquals(Shipments.Show, TestPage.ModeInRegistry);
			AssertEquals(true, TestPage.ModeInSessionForTest);

			TestPage.ZeroModeInSession();
			TestPage.ZeroRegistryVariables();
			TestPage.ModeInRegistry = Shipments.DoNotShow;
			b = TestPage.ModeInSessionForTest;
			AssertEquals(1, TestPage.RegistryReadCount);
			AssertEquals(0, TestPage.RegistryWriteCount);
			AssertEquals(Shipments.DoNotShow, TestPage.ModeInRegistry);
			AssertEquals(false, TestPage.ModeInSessionForTest);
		}

		public void TestModeInSessionGetter_InvalidSession()
		{
			TestPage.SiteUser.Logout();
			TestPage.ModeParamForTest = true;
			TestPage.SearchParamExistForTest = true;

			AssertNoExceptionThrown(() => _ = TestPage.ModeInSessionForTest);
		}

		#endregion

		#region TestModeInSessionSetter

		public void TestModeInSessionSetter()
		{
			TestPage.ModeInRegistry = Shipments.Show;
			TestPage.ZeroModeInSession();
			TestPage.ZeroRegistryVariables();
			TestPage.ModeInSessionForTest = false;
			AssertEquals(0, TestPage.RegistryReadCount);
			AssertEquals(0, TestPage.RegistryWriteCount);
			AssertEquals(false, TestPage.ModeInSessionForTest);
			TestPage.ModeInSessionForTest = true;
			AssertEquals(0, TestPage.RegistryReadCount);
			AssertEquals(1, TestPage.RegistryWriteCount);
			AssertEquals(true, TestPage.ModeInSessionForTest);

			TestPage.ModeInRegistry = Shipments.DoNotShow;
			TestPage.ZeroModeInSession();
			TestPage.ZeroRegistryVariables();
			TestPage.ModeInSessionForTest = true;
			AssertEquals(0, TestPage.RegistryReadCount);
			AssertEquals(0, TestPage.RegistryWriteCount);
			AssertEquals(true, TestPage.ModeInSessionForTest);
			TestPage.ModeInSessionForTest = false;
			TestPage.ModeParamForTest = true;
			TestPage.SearchParamExistForTest = true;
			AssertEquals(0, TestPage.RegistryReadCount);
			AssertEquals(1, TestPage.RegistryWriteCount);
			AssertEquals(false, TestPage.ModeInSessionForTest);
		}

		public void TestModeInSessionSetter_InvalidSession()
		{
			TestPage.SiteUser.Logout();
			TestPage.Session["ShowUnshippedOrders"] = false;

			AssertNoExceptionThrown(() => TestPage.ModeInSessionForTest = true);
		}

		#endregion

		#region TestShowUnshippedOrders

		public void TestShowUnshippedOrders()
		{
			TestPage.ZeroModeInSession();
			TestPage.ModeInSessionForTest = true;
			TestPage.ModeParamForTest = true;
			TestPage.SearchParamExistForTest = false;
			AssertEquals(true, TestPage.ShowUnshippedOrdersForTest);

			TestPage.ZeroModeInSession();
			TestPage.ModeInSessionForTest = true;
			TestPage.ModeParamForTest = false;
			TestPage.SearchParamExistForTest = false;
			AssertEquals(true, TestPage.ShowUnshippedOrdersForTest);

			TestPage.ZeroModeInSession();
			TestPage.ModeInSessionForTest = true;
			TestPage.ModeParamForTest = true;
			TestPage.SearchParamExistForTest = true;
			AssertEquals(true, TestPage.ShowUnshippedOrdersForTest);

			TestPage.ZeroModeInSession();
			TestPage.ModeInSessionForTest = false;
			TestPage.ModeParamForTest = true;
			TestPage.SearchParamExistForTest = true;
			AssertEquals(true, TestPage.ShowUnshippedOrdersForTest);
		}

		#endregion

		public void TestUnshippedOrdersFilter()
		{
			TestPage.ModeInSessionForTest = true;
			TestPage.ModeParamForTest = true;
			TestPage.SearchParamExistForTest = false;
			TestPage.Session[Shipments.UseUnshippedOrdersIndexer] = true;
			TestPage.SetupSearchControlsForTest();
			try
			{
				TestPage.SearchControlForTest.IsResultsRelatedOperation = true;
				TestPage.UnshippedOrdersSearchControlForTest.IsResultsRelatedOperation = true;

				var shipmentFilter = (TrackingShipmentFilterBusinessObject)TestPage.SearchControlForTest.FilterStripBizO;
				var filter = shipmentFilter["Transport Mode"];
				filter.IsActive = true;
				TestPage.SearchControlForTest.FindButton_Click(null, EventArgs.Empty);

				var orderFilter = (OrdersFilterBusinessObject)TestPage.UnshippedOrdersSearchControlForTest.FilterStripBizO;
				Assert("Shipment filters should be mapped", orderFilter["Transport Mode"].IsActive);
			}
			finally
			{
				TestPage.SearchControlForTest.Module.Dispose();
				TestPage.UnshippedOrdersSearchControlForTest.Module.Dispose();
			}
		}
	}
}
