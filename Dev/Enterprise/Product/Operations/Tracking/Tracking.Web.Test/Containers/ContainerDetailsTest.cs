using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	class ContainerDetailsTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.ContainerDetails;
		}

		public void TestSetupSetupOrdersGrid()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			using (ContainerDetailsForTest page = new ContainerDetailsForTest())
			{
				page.SetupOrdersGridForTest();

				AssertEquals("Should be 9 columns", 9, page.OrdersGrid.Columns.Count);
				AssertEquals("First column's header should be as expected", "Order #", page.OrdersGrid.Columns[0].HeaderText);
				AssertEquals("Second column's header should be as expected", "Shipment #", page.OrdersGrid.Columns[1].HeaderText);
				AssertEquals("Third column's header should be as expected", "House Bill", page.OrdersGrid.Columns[2].HeaderText);
				AssertEquals("Fourth column's header should be as expected", "Supplier", page.OrdersGrid.Columns[3].HeaderText);
				AssertEquals("Fifth column's header should be as expected", "Product", page.OrdersGrid.Columns[4].HeaderText);
				AssertEquals("Sixth column's header should be as expected", "Supplier Part", page.OrdersGrid.Columns[5].HeaderText);
				AssertEquals("Seventh column's header should be as expected", "Description", page.OrdersGrid.Columns[6].HeaderText);
				AssertEquals("Eighth column's header should be as expected", "Container Qty", page.OrdersGrid.Columns[7].HeaderText);
				AssertEquals("Ninth column's header should be as expected", "Packs", page.OrdersGrid.Columns[8].HeaderText);
			}
		}

		public void TestContainerDetailsComponentsVisibiltyWhenUserIsShipmentQuickViewUser()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			using (ContainerDetailsForTest page = new ContainerDetailsForTest())
			{
				AssertNotNull("TestOrg", helper.TestOrg);
				AssertNotNull("TestUser", helper.TestSiteUser);
				AssertEquals("QuickViewUser is logged in", true, page.SiteUser.IsShipmentQuickViewUser);

				TrackingContainer testContainer = Factory.NewWithValidTestData<TrackingContainer>();
				page.ContainerForTest = testContainer;
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();

				Assert(!page.MasterBillCaption.Visible);
				Assert(!page.MasterBillLabel.Visible);
				Assert(!page.ContainerStatusCaption.Visible);
				Assert(!page.ContainerStatusLabel.Visible);
				Assert(!page.PlannedWeightCaption.Visible);
				Assert(!page.PlannedWeightLabel.Visible);
				Assert(!page.QuarantineCaption.Visible);
				Assert(!page.QuarantineLabel.Visible);
				Assert(!page.SequenceCaption.Visible);
				Assert(!page.SequenceLabel.Visible);
				Assert(!page.PickupCaption.Visible);
				Assert(!page.PickupLabel.Visible);
				Assert(!page.DeliverCaption.Visible);
				Assert(!page.DeliverLabel.Visible);
				Assert(!page.SpecialInstructionCaption.Visible);
				Assert(!page.SpecialInstructionLabel.Visible);
				Assert(!page.ConsoleNumberCaption.Visible);
				Assert(!page.ConsoleNumberLabel.Visible);
				Assert(!page.VerifiedByCompanyCaption.Visible);
				Assert(!page.VerifiedByCompanyLabel.Visible);
				Assert(!page.VerifiedByPersonCaption.Visible);
				Assert(!page.VerifiedByPersonLabel.Visible);
				Assert(!page.VerifiedByPhoneCaption.Visible);
				Assert(!page.VerifiedByPhoneLabel.Visible);
				Assert(!page.VerifiedByEmailCaption.Visible);
				Assert(!page.VerifiedByEmailLabel.Visible);
				Assert(!page.OrdersGrid.Visible);
				Assert(!page.DocumentsGrid.Visible);
				Assert(!page.EditContainer.Visible);
			}
		}

		public void TestOrdersGridVisibility()
		{
			using (var page = new ContainerDetailsForTest())
			{
				var user = page.SiteUser;
				AssertNotNull(user);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "X";
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "user@user.com";
				contact.SetHashedPassword("password");
				contact.OC_WebAccessEnabled = true;

				SetupSecurity(WebSecurityRightsList.WebOrdersView, org, contact, true);

				Factory.Save();

				user.Login(org.OH_Code, "user@user.com", "password");

				page.OnLoad(EventArgs.Empty);

				AssertEquals(true, page.OrdersGrid.Visible);

				org.SecurityRights.RemoveAndDeleteAll();
				SetupSecurity(WebSecurityRightsList.WebOrdersView, org, contact, false);
				Factory.Save();
				user.OnSecurityRightsChangedForTest();

				page.OnLoad(EventArgs.Empty);

				AssertEquals(false, page.OrdersGrid.Visible);
			}
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "need to be checked by code owner")]
		new ContainerDetailsForTest TestPage
		{
			get { return base.TestPage as ContainerDetailsForTest; }
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ContainerDetailsForTest();
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "need to be checked by code owner")]
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

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerForwarding }; }
		}

		#endregion
	}
}
