using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	class EditContainerTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.EditContainer;
		}

		#region WebContainersSequence

		public void TestWebContainersSequenceGranted()
		{
			AssertControlWithPermissions(TestPage.Sequence, WebSecurityRightsList.WebContainersSequence, true);
		}

		public void TestWebContainersSequenceDenied()
		{
			AssertControlWithPermissions(TestPage.Sequence, WebSecurityRightsList.WebContainersSequence, false);
		}

		#endregion

		#region WebContainersRequiredDeliveryDate

		public void TestWebContainersReqDeliveryDateGranted()
		{
			AssertControlWithPermissions(TestPage.RequiredDelivery, WebSecurityRightsList.WebContainersReqDeliveryDate, true);
		}

		public void TestWebContainersReqDeliveryDateDenied()
		{
			AssertControlWithPermissions(TestPage.RequiredDelivery, WebSecurityRightsList.WebContainersReqDeliveryDate, false);
		}

		#endregion

		#region WebContainersEstDehireDate

		public void TestWebContainersEstDehireDateGranted()
		{
			AssertControlWithPermissions(TestPage.EmptyReady, WebSecurityRightsList.WebContainersEstDehireDate, true);
		}

		public void TestWebContainersEstDehireDateDenied()
		{
			AssertControlWithPermissions(TestPage.EmptyReady, WebSecurityRightsList.WebContainersEstDehireDate, false);
		}

		#endregion

		#region WebContainersConDeliveryDate

		public void TestWebContainersConDeliveryDateGranted()
		{
			AssertControlWithPermissions(TestPage.ConfirmedDelivery, WebSecurityRightsList.WebContainersConDeliveryDate, true);
		}

		public void TestWebContainersConDeliveryDateDenied()
		{
			AssertControlWithPermissions(TestPage.ConfirmedDelivery, WebSecurityRightsList.WebContainersConDeliveryDate, false);
		}

		#endregion

		#region WebContainersEmptyPickupDate

		public void TestWebContainersPickupGranted()
		{
			AssertControlWithPermissions(TestPage.EmptyPickup, WebSecurityRightsList.WebContainersPickup, true);
		}

		public void TestWebContainersPickupDenied()
		{
			AssertControlWithPermissions(TestPage.EmptyPickup, WebSecurityRightsList.WebContainersPickup, false);
		}

		#endregion

		#region WebContainersActualDeliveryDate

		public void TestWebContainersActualDeliveryDateGranted()
		{
			AssertControlWithPermissions(TestPage.ActualDelivery, WebSecurityRightsList.WebContainersActDeliveryDate, true);
		}

		public void TestWebContainersActualDeliveryDateDenied()
		{
			AssertControlWithPermissions(TestPage.ActualDelivery, WebSecurityRightsList.WebContainersActDeliveryDate, false);
		}

		#endregion

		#region WebContainersActualDehire

		public void TestWebContainersActualDehireGranted()
		{
			AssertControlWithPermissions(TestPage.ActualDehire, WebSecurityRightsList.WebContainersActualDehire, true);
		}

		public void TestWebContainersActualDehireDenied()
		{
			AssertControlWithPermissions(TestPage.ActualDehire, WebSecurityRightsList.WebContainersActualDehire, false);
		}

		#endregion

		public void TestSetupSetupOrdersGrid()
		{
			TestPage.SetupOrdersGridForTest();

			AssertEquals("Should be 9 columns", 9, TestPage.OrdersGrid.Columns.Count);
			AssertEquals("First column's header should be as expected", "Order #", TestPage.OrdersGrid.Columns[0].HeaderText);
			AssertEquals("Second column's header should be as expected", "Shipment #", TestPage.OrdersGrid.Columns[1].HeaderText);
			AssertEquals("Third column's header should be as expected", "House Bill", TestPage.OrdersGrid.Columns[2].HeaderText);
			AssertEquals("Fourth column's header should be as expected", "Supplier", TestPage.OrdersGrid.Columns[3].HeaderText);
			AssertEquals("Fifth column's header should be as expected", "Product", TestPage.OrdersGrid.Columns[4].HeaderText);
			AssertEquals("Sixth column's header should be as expected", "Supplier Part", TestPage.OrdersGrid.Columns[5].HeaderText);
			AssertEquals("Seventh column's header should be as expected", "Description", TestPage.OrdersGrid.Columns[6].HeaderText);
			AssertEquals("Eighth column's header should be as expected", "Container Qty", TestPage.OrdersGrid.Columns[7].HeaderText);
			AssertEquals("Ninth column's header should be as expected", "Packs", TestPage.OrdersGrid.Columns[8].HeaderText);
		}

		public void TestOrdersPanelVisibility()
		{
			var user = TestPage.SiteUser;
			AssertNotNull(user);

			AddWebUserRightAndLogIn(WebSecurityRightsList.WebOrdersView, true);

			TestPage.OnLoad(EventArgs.Empty);

			AssertEquals(true, TestPage.OrdersPanel.Visible);

			TestOrg.SecurityRights.RemoveAndDeleteAll();
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebOrdersView, false);
			user.OnSecurityRightsChangedForTest();

			TestPage.OnLoad(EventArgs.Empty);

			AssertEquals(false, TestPage.OrdersPanel.Visible);
		}

		#region Implementation

		void AssertControlWithPermissions(WebControl control, WebSecurityRight right, bool granted)
		{
			AddWebUserRightAndLogIn(right, granted);

			TestPage.SetupAuthorisedContentForTest();
			AssertEquals(control.ID + " should be " + (granted ? "enabled" : "disabled"), granted, control.Enabled);
		}

		new EditContainerForTest TestPage
		{
			get { return base.TestPage as EditContainerForTest; }
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new EditContainerForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebForwardingContainersModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebContainersEdit; }
		}

		OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
					fTestOrg.OH_Code = "XXXYYYZZZ";
				}
				return fTestOrg;
			}
		}
		OrgHeader fTestOrg;

		OrgContact TestContact
		{
			get
			{
				if (fTestContact == null)
				{
					fTestContact = TestOrg.Contacts.AddNew();
					fTestContact.OC_Email = "test@testcompany.com";
					fTestContact.SetHashedPassword("testpassword");
					fTestContact.OC_WebAccessEnabled = true;
				}
				return fTestContact;
			}
		}
		OrgContact fTestContact;

		void AddWebUserRightAndLogIn(WebSecurityRight right, bool granted)
		{
			SetupSecurity(right, TestOrg, TestContact, granted);

			Factory.Save();

			TestPage.SiteUser.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");

			AssertEquals("Logged in user should be TestContact", TestContact.PK, TestPage.SiteUser.LoggedInUser.PK);
			Assert("SiteUser should be logged in", TestPage.SiteUser.IsLoggedIn);
			AssertEquals("SiteUser should have security rights as expected", granted, TestPage.SiteUser.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(right));
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerForwarding }; }
		}

		#endregion
	}
}
