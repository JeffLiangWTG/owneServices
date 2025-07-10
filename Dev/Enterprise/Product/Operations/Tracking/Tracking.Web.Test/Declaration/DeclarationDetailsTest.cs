using System;
using System.Collections.Generic;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
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
	sealed class DeclarationDetailsTest : BasePageWithAuthorisationTest
	{
		public void TestSetUpCountrySpecificControls()
		{
			TestDeclarationDetails testDecDetailsPage = new TestDeclarationDetails();
			testDecDetailsPage.SetUpPageForTest();
			BaseJobDeclaration bJDecForTest = Factory.New<BaseJobDeclaration>();
			GlbBranch aUBranch = Factory.New<GlbBranch>();
			aUBranch.GB_RL_NKHomePort = "AUSYD";
			bJDecForTest.JE_GB = aUBranch.PK;
			TrackingDeclaration tRDecForTest = new TrackingDeclaration(bJDecForTest);
			testDecDetailsPage.SetDataSource(tRDecForTest);
			testDecDetailsPage.SetUpCountrySpecificControls();

			Assert(testDecDetailsPage.ForTest_ConsolPanel.Visible);

			GlbBranch uSBranch = Factory.New<GlbBranch>();
			uSBranch.GB_RL_NKHomePort = "USCHI";
			uSBranch.GB_GC = Factory.New<GlbCompany>().PK;
			bJDecForTest.JE_GB = uSBranch.PK;
			tRDecForTest = new TrackingDeclaration(bJDecForTest);
			testDecDetailsPage.SetDataSource(tRDecForTest);
			testDecDetailsPage.SetUpCountrySpecificControls();

			Assert(testDecDetailsPage.ForTest_ConsolPanel.Visible);

			bJDecForTest.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			tRDecForTest = new TrackingDeclaration(bJDecForTest);
			testDecDetailsPage.SetDataSource(tRDecForTest);
			testDecDetailsPage.SetUpCountrySpecificControls();

			Assert(!testDecDetailsPage.ForTest_ConsolPanel.Visible);
		}

		public void TestTransportGridVsControl()
		{
			TestDeclarationDetails testDecDetailsPage = new TestDeclarationDetails();
			testDecDetailsPage.SetUpPageForTest();
			BaseJobDeclaration bJDecForTest = Factory.New<BaseJobDeclaration>();
			TrackingDeclaration tRDecForTest = new TrackingDeclaration(bJDecForTest);
			testDecDetailsPage.SetDataSource(tRDecForTest);
			testDecDetailsPage.SetUpTransportControls();

			Assert("standalone but there is no legs", !testDecDetailsPage.ForTest_AreaTransportGrid.Visible);
			Assert(testDecDetailsPage.ForTest_AreaTransportControl.Visible);

			Transport tr = bJDecForTest.Transports.AddNew();
			tr.JW_ATA = new ZDateTime(2006, 12, 12);

			testDecDetailsPage.SetUpTransportControls();
			Assert(testDecDetailsPage.ForTest_AreaTransportGrid.Visible);
			Assert(!testDecDetailsPage.ForTest_AreaTransportControl.Visible);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			bJDecForTest.JE_JS = shipment.PK;
			Transport tr2 = shipment.Transports.AddNew();
			tr2.JW_ATA = new ZDateTime(2007, 12, 12);

			testDecDetailsPage.SetUpTransportControls();
			Assert(testDecDetailsPage.ForTest_AreaTransportGrid.Visible);
			Assert(!testDecDetailsPage.ForTest_AreaTransportControl.Visible);

			bJDecForTest.JE_OverrideFreightDefaults = ZBool.True;
			testDecDetailsPage.SetUpTransportControls();
			Assert(!testDecDetailsPage.ForTest_AreaTransportGrid.Visible);
			Assert(testDecDetailsPage.ForTest_AreaTransportControl.Visible);
		}

		public void TestDeclarationtDetailsComponentsVisibiltyWhenUserIsShipmentQuickViewUser()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			using (TestDeclarationDetails page = new TestDeclarationDetails())
			{
				AssertNotNull("TestOrg", helper.TestOrg);
				AssertNotNull("TestUser", helper.TestSiteUser);
				AssertEquals("QuickViewUser is logged in", true, page.SiteUser.IsShipmentQuickViewUser);

				BaseJobDeclaration bJDecForTest = Factory.New<BaseJobDeclaration>();
				TrackingDeclaration tRDecForTest = new TrackingDeclaration(bJDecForTest);
				bJDecForTest.JE_DeclarationReference = "test";
				tRDecForTest.SiteUser = helper.TestSiteUser;
				SetupCharges(tRDecForTest, GlbCompany.CurrentCompany.OrgProxy);
				page.SetDataSource(tRDecForTest);
				page.SetUpPageForTest();

				SetupSecurity(WebSecurityRightsList.WebInvoicingAndStatements, helper.TestOrg, helper.TestContact, true);
				WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				WebDataRegistry.Instance.WebTrackerLocalChargesOnShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				page.SetUpTransportControls();
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();

				Assert(!page.TransportGridForTest.ShouldShowControl);
				Assert(!page.OrdersGridForTest.ShouldShowControl);
				Assert(!page.CustomsEntriesDataGridForTest.ShouldShowControl);
				Assert(!page.ContainerGridForTest.ShouldShowControl);
				Assert(!page.InvoicesGriddForTest.ShouldShowControl);
				Assert(!helper.TestSiteUser.CanViewAccounts);
				Assert(!page.ChargesGridForTest.Visible);
				Assert(page.LocalChargesGridForTest.Visible);
				Assert(!page.ChargesGridForTest.ShouldShowControl);
				Assert(!page.LocalChargesGridForTest.ShouldShowControl);
				Assert(!page.HouseBillsGridForTest.ShouldShowControl);
			}
		}

		public void TestHouseBillsGridColumnProvider_USIMP()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var usIMPDec = Factory.New<JobDeclaration>();
				usIMPDec.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

				AssertHouseBillsGridColumnProvider(new TrackingDeclaration(usIMPDec), typeof(USIMPCusDecHouseBillColumnProvider));
			}
		}

		public void TestReconDeclaration_US()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = USJobMessageTypeList.Codes.Recon;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;

			var helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			using var page = new TestDeclarationDetails();
			page.SetUpPageForTest();
			page.SetDataSource(new TrackingDeclaration(declaration));
			page.Validate();

			Assert("No exception was thrown during validation", true);
		}

		public void TestHouseBillsGridColumnProvider_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var auDec = Factory.New<Customs.AU.Declaration.Business.JobDeclaration>();

				AssertHouseBillsGridColumnProvider(new TrackingDeclaration(auDec), typeof(AUCusDecHouseBillColumnProvider));
			}
		}

		public void TestHouseBillsGridColumnProvider_Default()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				var otherDec = Factory.New<BaseJobDeclaration>();

				AssertHouseBillsGridColumnProvider(new TrackingDeclaration(otherDec), typeof(BaseCusDecHouseBillColumnProvider));
			}
		}

		void AssertHouseBillsGridColumnProvider(TrackingDeclaration declaration, Type expectedType)
		{
			var helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			using (var page = new TestDeclarationDetails())
			{
				page.SetUpPageForTest();
				page.SetDataSource(declaration);
				page.ForTest_RunOnPreBind();

				AssertEquals(expectedType, page.HouseBillsGridForTest.ColumnProvider.GetType());
			}
		}

		public void TestOrdersGridVisibility()
		{
			using (var page = new TestDeclarationDetails())
			{
				page.SetUpPageForTest();
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

				page.ForTest_RunOnLoad();

				AssertEquals(true, page.OrdersGridForTest.Visible);

				org.SecurityRights.RemoveAndDeleteAll();
				SetupSecurity(WebSecurityRightsList.WebOrdersView, org, contact, false);
				Factory.Save();
				user.OnSecurityRightsChangedForTest();

				page.ForTest_RunOnLoad();

				AssertEquals(false, page.OrdersGridForTest.Visible);
			}
		}

		void SetupCharges(ITransactionSupport parent, OrgHeader orgHeader)
		{
			JobHeader testJob = Factory.NewJobForTesting<JobHeader>();
			testJob.JH_ParentID = parent.PK;
			testJob.JH_GC = GlbCompany.CurrentCompany.PK;
			testJob.LocalChargesPK = orgHeader.PK;

			AccTransactionHeader invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_TransactionNum = "00001000";
			invoice.AH_TransactionType = "INV";
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_OH = orgHeader.PK;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_JH = testJob.PK;
			invoice.AH_InvoiceDate = ZDateTime.Now.AddDays(-2);
			invoice.AH_ConsolidatedInvoiceRef = parent.Reference;

			AccTransactionLines chargeLine1 = Factory.New<AccTransactionLines>();
			chargeLine1.AL_AH = invoice.PK;
			chargeLine1.AL_JH = testJob.PK;
			chargeLine1.AL_GB = invoice.AH_GB;
			chargeLine1.AL_GC = invoice.AH_GC;

			AccTransactionLines chargeLine2 = Factory.New<AccTransactionLines>();
			chargeLine2.AL_AH = invoice.PK;
			chargeLine2.AL_JH = testJob.PK;
			chargeLine2.AL_GB = invoice.AH_GB;
			chargeLine2.AL_GC = invoice.AH_GC;
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints => new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerImportBrokerage, Environment.Env.Licence.WebTrackerExportBrokerage };

		protected override BooleanRegistryItem UseWebModule => WebDataRegistry.Instance.UseWebDeclarationModule;

		protected override WebSecurityRight SiteUserSecurityRight => WebSecurityRightsList.WebDeclarationView;

		protected override string GetExpectedPageName() => WebTracker.Pages.DeclarationDetails;

		protected override Control GetNewControl() => new TestDeclarationDetails();
	}
}
