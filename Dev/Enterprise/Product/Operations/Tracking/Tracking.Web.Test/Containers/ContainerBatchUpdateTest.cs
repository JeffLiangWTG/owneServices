using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	class ContainerBatchUpdateTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.ContainerBatchUpdate;
		}

		public void TestSetupContainerGrid()
		{
			TestPage.SetupContainerGridForTest();

			AssertEquals("Should be 11 columns", 11, TestPage.SelectedContainersGridForTest.Columns.Count);
			AssertEquals("First column's header should be as expected", "Container #", TestPage.SelectedContainersGridForTest.Columns[0].HeaderText);
			AssertEquals("Second column's header should be as expected", "Shipment #", TestPage.SelectedContainersGridForTest.Columns[1].HeaderText);
			AssertEquals("Fourth column's header should be as expected", "Pack #", TestPage.SelectedContainersGridForTest.Columns[2].HeaderText);
			AssertEquals("Fifth column's header should be as expected", "Arrival", TestPage.SelectedContainersGridForTest.Columns[3].HeaderText);
			AssertEquals("Sixth column's header should be as expected", "Required Delivery", TestPage.SelectedContainersGridForTest.Columns[4].HeaderText);
			AssertEquals("Seventh column's header should be as expected", "Confirmed Delivery", TestPage.SelectedContainersGridForTest.Columns[5].HeaderText);
			AssertEquals("Eighth column's header should be as expected", "Actual Delivery", TestPage.SelectedContainersGridForTest.Columns[6].HeaderText);
			AssertEquals("Ninth column's header should be as expected", "Empty Ready", TestPage.SelectedContainersGridForTest.Columns[7].HeaderText);
			AssertEquals("Tenth column's header should be as expected", "Empty Pickup", TestPage.SelectedContainersGridForTest.Columns[8].HeaderText);
			AssertEquals("Eleventh column's header should be as expected", "Actual De-hire", TestPage.SelectedContainersGridForTest.Columns[9].HeaderText);
			AssertEquals("Twelvth column's header should be as expected", "Sequence", TestPage.SelectedContainersGridForTest.Columns[10].HeaderText);
		}

		#region WebContainersReqDeliveryDate

		public void TestWebContainersReqDeliveryDateGranted()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersReqDeliveryDate, true);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 4;

			AssertEquals("DateEdit should be editable", true, TestPage.RequiredDelivery.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Required Delivery", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up to allow editing", !((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		public void TestWebContainersReqDeliveryDateDenied()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersReqDeliveryDate, false);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 4;

			AssertEquals("DateEdit should be disabled", false, TestPage.RequiredDelivery.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Required Delivery", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up without editing", ((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		#endregion

		#region WebContainersConDeliveryDate

		public void TestWebContainersConDeliveryDateGranted()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersConDeliveryDate, true);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 5;

			AssertEquals("DateEdit should be editable", true, TestPage.ConfirmedDelivery.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Confirmed Delivery", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up to allow editing", !((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		public void TestWebContainersConDeliveryDateDenied()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersConDeliveryDate, false);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 5;

			AssertEquals("DateEdit should be disabled", false, TestPage.ConfirmedDelivery.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Confirmed Delivery", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up without editing", ((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		#endregion

		#region WebContainersActDeliveryDate

		public void TestWebContainersActDeliveryDateGranted()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersActDeliveryDate, true);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 6;

			AssertEquals("DateEdit should be editable", true, TestPage.ActualDelivery.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Actual Delivery", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up to allow editing", !((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		public void TestWebContainersActDeliveryDateDenied()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersActDeliveryDate, false);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 6;

			AssertEquals("DateEdit should be disabled", false, TestPage.ActualDelivery.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Actual Delivery", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up without editing", ((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		#endregion

		#region WebContainersEstDehireDate

		public void TestWebContainersEstDehireDateGranted()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersEstDehireDate, true);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 7;

			AssertEquals("DateEdit should be editable", true, TestPage.EstimatedDehire.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Empty Ready", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up to allow editing", !((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		public void TestWebContainersEstDehireDateDenied()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersEstDehireDate, false);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 7;

			AssertEquals("DateEdit should be disabled", false, TestPage.EstimatedDehire.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Empty Ready", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up without editing", ((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		#endregion

		#region WebContainersPickup

		public void TestWebContainersPickupGranted()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersPickup, true);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 8;

			AssertEquals("DateEdit should be editable", true, TestPage.Pickup.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Empty Pickup", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up to allow editing", !((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		public void TestWebContainersPickupDenied()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersPickup, false);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 8;

			AssertEquals("DateEdit should be disabled", false, TestPage.Pickup.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Empty Pickup", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up without editing", ((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		#endregion

		#region WebContainersActualDehire

		public void TestWebContainersActualDehireGranted()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersActualDehire, true);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 9;

			AssertEquals("DateEdit should be editable", true, TestPage.ActualDehire.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Actual De-hire", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up to allow editing", !((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		public void TestWebContainersActualDehireDenied()
		{
			AddWebUserRightAndLogIn(WebSecurityRightsList.WebContainersActualDehire, false);

			TestPage.SetupAuthorisedContentForTest();
			TestPage.SetupContainerGridForTest();
			int colIndex = 9;

			AssertEquals("DateEdit should be disabled", false, TestPage.ActualDehire.Enabled);
			Assert("Grid should be set up", TestPage.SelectedContainersGridForTest.Columns.Count > colIndex);
			AssertEquals("Column's header should be as expected", "Actual De-hire", TestPage.SelectedContainersGridForTest.Columns[colIndex].HeaderText);
			Assert("Grid column should be set up without editing", ((ZTemplateColumn)TestPage.SelectedContainersGridForTest.Columns[colIndex]).ReadOnly);
		}

		#endregion

		#region Implementation

		new ContainerBatchUpdateForTest TestPage
		{
			get { return base.TestPage as ContainerBatchUpdateForTest; }
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ContainerBatchUpdateForTest();
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
			List<OrgSecurityContacts> contactSecurity = new List<OrgSecurityContacts>();
			OrgSecurity orgRight = TestOrg.SecurityRights.AddNew();
			orgRight.OX_Granted = granted;
			orgRight.OX_SecurityItemName = right.Code;

			OrgSecurityContacts userRight = TestContact.SecurityRightsForBindingOnly.AddNew();
			userRight.OZ_OX = orgRight.PK;
			userRight.OZ_Granted = granted;
			contactSecurity.Add(userRight);

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
