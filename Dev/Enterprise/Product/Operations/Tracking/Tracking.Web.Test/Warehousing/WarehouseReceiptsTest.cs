using System.Web.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class WarehouseReceiptsTest : WarehousingPageBaseTest
	{
		#region TestSearchControl

		public void TestSearchControl()
		{
			var control = new WarehouseReceiptsForTest();
			var user = control.SiteUser;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsWarehouseClient = true;
			org.OH_Code = "X";

			org.SecurityRights.RemoveAndDeleteAll();
			var orgRight1 = org.SecurityRights.AddNew();
			orgRight1.OX_Granted = true;
			orgRight1.OX_SecurityItemName = WebSecurityRightsList.WebWarehouseReceiptsView.Code;

			var orgRight2 = org.SecurityRights.AddNew();
			orgRight2.OX_Granted = true;
			orgRight2.OX_SecurityItemName = WebSecurityRightsList.WebWarehouseReceiptsAddEdit.Code;

			var orgRight3 = org.SecurityRights.AddNew();
			orgRight3.OX_Granted = false;
			orgRight3.OX_SecurityItemName = WebSecurityRightsList.WebWarehouseOrdersAddEdit.Code;

			var contact = org.Contacts.AddNew();
			contact.SecurityRightsForBindingOnly.RemoveAll();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			var userRight1 = contact.SecurityRightsForBindingOnly.AddNew();
			userRight1.OZ_OX = orgRight1.PK;
			userRight1.OZ_Granted = true;

			var userRight2 = contact.SecurityRightsForBindingOnly.AddNew();
			userRight2.OZ_OX = orgRight2.PK;
			userRight2.OZ_Granted = true;

			var userRight3 = contact.SecurityRightsForBindingOnly.AddNew();
			userRight3.OZ_OX = orgRight3.PK;
			userRight3.OZ_Granted = false;
			Factory.Save();

			control.SiteUser.Login(org.OH_Code, "user@user.com", "password");
			control.SetupPageForTesting();
			control.CallOnInit();

			var searchControl = (ZSearchControl)control.SearchControl;
			AssertEquals("ModuleID", WebModuleIDs.TrackingWarehouseReceive, searchControl.ModuleID);
			AssertEquals("IsNewButtonVisible", true, searchControl.IsNewButtonVisible);
			AssertEquals("NewButtonUrl", control.AppInstance.WarehouseEditReceive, searchControl.NewButtonUrl);
		}

		public void TestSearchControl_ViewOnly()
		{
			var control = new WarehouseReceiptsForTest();
			var user = control.SiteUser;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsWarehouseClient = true;
			org.OH_Code = "X";

			org.SecurityRights.RemoveAndDeleteAll();
			var orgRight1 = org.SecurityRights.AddNew();
			orgRight1.OX_Granted = true;
			orgRight1.OX_SecurityItemName = WebSecurityRightsList.WebWarehouseReceiptsView.Code;

			var orgRight2 = org.SecurityRights.AddNew();
			orgRight2.OX_Granted = false;
			orgRight2.OX_SecurityItemName = WebSecurityRightsList.WebWarehouseReceiptsAddEdit.Code;

			var contact = org.Contacts.AddNew();
			contact.SecurityRightsForBindingOnly.RemoveAll();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			var userRight1 = contact.SecurityRightsForBindingOnly.AddNew();
			userRight1.OZ_OX = orgRight1.PK;
			userRight1.OZ_Granted = true;

			var userRight2 = contact.SecurityRightsForBindingOnly.AddNew();
			userRight2.OZ_OX = orgRight2.PK;
			userRight2.OZ_Granted = false;
			Factory.Save();

			control.SiteUser.Login(org.OH_Code, "user@user.com", "password");
			control.SetupPageForTesting();
			control.CallOnInit();

			var searchControl = (ZSearchControl)control.SearchControl;
			AssertEquals("ModuleID", WebModuleIDs.TrackingWarehouseReceive, searchControl.ModuleID);
			AssertEquals("IsNewButtonVisible", false, searchControl.IsNewButtonVisible);
			AssertEquals("NewButtonUrl", control.AppInstance.WarehouseEditReceive, searchControl.NewButtonUrl);
		}

		#endregion

		#region Implementation

		protected override BooleanRegistryItem UseWebModule => WebDataRegistry.Instance.UseWebWarehouseReceiptsModule;

		protected override WebSecurityRight SiteUserSecurityRight => WebSecurityRightsList.WebWarehouseReceiptsView;

		protected override string GetExpectedPageName() => WebTracker.Pages.WarehouseReceipts;

		protected override Control GetNewControl() => new WarehouseReceiptsForTest();

		#endregion
	}
}
