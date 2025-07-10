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
	class ContainerSummaryTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.ContainerSummary;
		}

		public void TestSetupSummaryGrid()
		{
			ZDataGrid grid = new ZDataGrid();
			TestPage.SetupSummaryGridForTest(grid);

			AssertEquals("Columns", 5, grid.Columns.Count);
			AssertEquals("Description", grid.Columns[0].HeaderText);
			AssertEquals("Containers", grid.Columns[1].HeaderText);
			AssertEquals("Shipments", grid.Columns[2].HeaderText);
			AssertEquals("Orders", grid.Columns[3].HeaderText);
			AssertEquals("Packages", grid.Columns[4].HeaderText);
		}

		#region Implementation

		new ContainerSummaryForTest TestPage
		{
			get { return base.TestPage as ContainerSummaryForTest; }
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ContainerSummaryForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebForwardingContainersModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebContainers; }
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "need to be checked by code owner")]
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
