using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	[HttpContextEnabledTest]
	sealed class EditLinerAndAgencyContainerTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.EditLinerAndAgencyContainer;
		}

		#region WebContainersRequiredDeliveryDate

		public void TestWebContainersReqDeliveryDateGranted()
		{
			AssertControlWithPermissions(TestPage.EstimatedFullDeliveryDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersEstimatedFullDeliveryDate, true);
		}

		public void TestWebContainersReqDeliveryDateDenied()
		{
			AssertControlWithPermissions(TestPage.EstimatedFullDeliveryDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersEstimatedFullDeliveryDate, false);
		}

		#endregion WebContainersRequiredDeliveryDate

		#region WebContainersEstDehireDate

		public void TestWebContainersEstDehireDateGranted()
		{
			AssertControlWithPermissions(TestPage.EmptyReadyForReturnDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersEmptyReadyToReturnDate, true);
		}

		public void TestWebContainersEstDehireDateDenied()
		{
			AssertControlWithPermissions(TestPage.EmptyReadyForReturnDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersEmptyReadyToReturnDate, false);
		}

		#endregion WebContainersEstDehireDate

		#region WebContainersEmptyPickupDate

		public void TestWebContainersPickupGranted()
		{
			AssertControlWithPermissions(TestPage.EmptyReturnReqByDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersEmptyReturnReqByDate, true);
		}

		public void TestWebContainersPickupDenied()
		{
			AssertControlWithPermissions(TestPage.EmptyReturnReqByDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersEmptyReturnReqByDate, false);
		}

		#endregion WebContainersEmptyPickupDate

		#region WebContainersActualDeliveryDate

		public void TestWebContainersActualDeliveryDateGranted()
		{
			AssertControlWithPermissions(TestPage.DeliveryDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersActDeliveryDate, true);
		}

		public void TestWebContainersActualDeliveryDateDenied()
		{
			AssertControlWithPermissions(TestPage.DeliveryDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersActDeliveryDate, false);
		}

		#endregion WebContainersActualDeliveryDate

		#region WebContainersActualDehire

		public void TestWebContainersActualDehireGranted()
		{
			AssertControlWithPermissions(TestPage.EmptyReturnedOnDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersEmptyReadyOnDate, true);
		}

		public void TestWebContainersActualDehireDenied()
		{
			AssertControlWithPermissions(TestPage.EmptyReturnedOnDateEditBox, WebSecurityRightsList.WebLinerAndAgencyContainersEmptyReadyOnDate, false);
		}

		#endregion WebContainersActualDehire

		#region Implementation

		void AssertControlWithPermissions(System.Web.UI.WebControls.WebControl control, WebSecurityRight right, bool granted)
		{
			AddWebUserRightAndLogIn(right, granted);

			TestPage.SetupAuthorisedContentForTest();
			AssertEquals(control.ID + " should be " + (granted ? "enabled" : "disabled"), granted, control.Enabled);
		}

		new EditLinerAndAgencyContainerForTest TestPage
		{
			get { return base.TestPage as EditLinerAndAgencyContainerForTest; }
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new EditLinerAndAgencyContainerForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebLinerAndAgencyContainersModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebLinerAndAgencyContainersEdit; }
		}

		OrgHeader TestOrg
		{
			get
			{
				if (testOrg == null)
				{
					testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
					testOrg.OH_Code = "XXXYYYZZZ";
				}
				return testOrg;
			}
		}

		OrgHeader testOrg;

		OrgContact TestContact
		{
			get
			{
				if (testContact == null)
				{
					testContact = TestOrg.Contacts.AddNew();
					testContact.OC_Email = "test@testcompany.com";
					testContact.SetHashedPassword("testpassword");
					testContact.OC_WebAccessEnabled = true;
				}
				return testContact;
			}
		}

		OrgContact testContact;

		void AddWebUserRightAndLogIn(WebSecurityRight right, bool granted)
		{
			var contactSecurity = new List<OrgSecurityContacts>();
			var orgRight = TestOrg.SecurityRights.AddNew();
			orgRight.OX_Granted = granted;
			orgRight.OX_SecurityItemName = right.Code;

			var userRight = TestContact.SecurityRightsForBindingOnly.AddNew();
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

		#endregion Implementation
	}
}
