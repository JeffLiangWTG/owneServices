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
	sealed class WhsOrderLineAllocationTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.WarehouseOrderLineAllocation;
		}

		public override void TestShowLoginStatus()
		{
			AssertEquals("Should not be visible for logged in SiteUser", false, TestPage.ShowLoginStatusForTest);
			TestPage.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			AssertEquals("Should not be visible for CurrentCompany.OrgProxy web users", false, TestPage.ShowLoginStatusForTest);
		}

		#region Overrides

		protected override System.Web.UI.Control GetNewControl()
		{
			return new WhsOrderLineAllocationZPageTest.WhsOrderLineAllocationForTest("EDICUS");
		}

		protected override BooleanRegistryItem UseWebModule => WebDataRegistry.Instance.UseWebWarehouseOrdersModule;

		protected override WebSecurityRight SiteUserSecurityRight => WebSecurityRightsList.WebWarehouseOrdersView;

		protected override void SetUp()
		{
			base.SetUp();
			((WhsOrderLineAllocationZPageTest.WhsOrderLineAllocationForTest)TestPage).SetupPageForTesting();
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
			=> new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerWarehouse };

		#endregion
	}
}
