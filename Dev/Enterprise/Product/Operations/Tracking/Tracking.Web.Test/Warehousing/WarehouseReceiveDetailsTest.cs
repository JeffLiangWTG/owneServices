using System.Collections.Generic;
using System.Web.UI;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class WarehouseReceiveDetailsTest : BasePageWithAuthorisationTest
	{
		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebWarehouseReceiptsView; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerWarehouse }; }
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebWarehouseReceiptsModule; }
		}

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.WarehouseReceiveDetails;
		}

		protected override Control GetNewControl()
		{
			return new TestWarehouseReceiveDetails();
		}

		protected override void SetUp()
		{
			base.SetUp();
			((TestWarehouseReceiveDetails)TestPage).SetupPageForTesting();
		}

		public void TestDuplicateReceive()
		{
			var page = (TestWarehouseReceiveDetails)TestPage;
			page.OnDuplicateReceiveClickForTest();
			var newReceivePK = GetRedirectReferencePK();
			var newReceive = page.Factory.Load<TrackingWhsReceive>(newReceivePK);
			AssertEquals(page.SiteUser.ContactAndCompanyReference, newReceive.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestButtonVisibilityDependsOnRights()
		{
			TestWarehouseReceiveDetails testPage = TestPage as TestWarehouseReceiveDetails;
			TrackingSiteUser user = testPage.SiteUser;
			AssertNotNull(user);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			OrgSecurity orgRight = org.SecurityRights.AddNew();
			orgRight.OX_Granted = true;
			orgRight.OX_SecurityItemName = WebSecurityRightsList.WebWarehouseReceiptsView.Code;
			OrgSecurityContacts userRight = contact.SecurityRightsForBindingOnly.AddNew();
			userRight.OZ_OX = orgRight.PK;
			userRight.OZ_Granted = true;

			Factory.Save();

			user.Login(org.OH_Code, "user@user.com", "password");
			testPage.SetupPageForTesting();
			testPage.OnLoadForTest();
			Assert(!testPage.CancelReceiveForTest.Enabled);
			Assert(!testPage.EditReceiveForTest.Enabled);

			org.OH_IsWarehouseClient = true;
			Factory.Save();
			// changing OH_IsWarehouseClient requires logout and login again to see the effect
			user.Login(org.OH_Code, "user@user.com", "password");
			testPage.SetupPageForTesting();
			testPage.OnLoadForTest();
			Assert(testPage.CancelReceiveForTest.Enabled);
			Assert(testPage.EditReceiveForTest.Enabled);

			userRight.OZ_Granted = false;
			Factory.Save();
			user.Login(org.OH_Code, "user@user.com", "password");
			testPage.SetupPageForTesting();
			testPage.OnLoadForTest();
			Assert(!testPage.CancelReceiveForTest.Enabled);
			Assert(!testPage.EditReceiveForTest.Enabled);
		}

		protected override void AssertAuthorisedContent(BooleanRegistryItem useModule, OrgSecurityContacts contactSecurity)
		{
			base.AssertAuthorisedContent(useModule, contactSecurity);

			AssertEquals("Cancel Button Enabled", useModule.Value && contactSecurity.OZ_Granted, ((TestWarehouseReceiveDetails)TestPage).CancelReceiveForTest.Enabled);
			AssertEquals("Edit Button Enabled", useModule.Value && contactSecurity.OZ_Granted, ((TestWarehouseReceiveDetails)TestPage).EditReceiveForTest.Enabled);
		}
	}
}
