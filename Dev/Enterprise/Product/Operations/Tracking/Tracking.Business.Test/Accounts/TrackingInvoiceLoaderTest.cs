using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Tracking.Business
{
	public abstract class TrackingInvoiceLoaderTest : TestCaseWithFactory
	{
		#region setup

		protected abstract ITransactionSupport GetNewBusinessObject();

		#region TestOrg

		protected OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>();
					fTestOrg.OH_IsConsignee = ZBool.True;

					OrgHeader relatedParty = Factory.New<OrgHeader>();
					relatedParty.OH_Code = "AAA";
					relatedParty.OH_IsActive = true;
					relatedParty.SetRelatedParty(fTestOrg, RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.Pickup);
				}
				return fTestOrg;
			}
		}
		OrgHeader fTestOrg;

		#endregion

		#region TestContact

		protected OrgContact TestContact
		{
			get
			{
				if (fTestContact == null)
				{
					fTestContact = TestOrg.Contacts.AddNew();
				}
				return fTestContact;
			}
		}
		OrgContact fTestContact;

		#endregion

		#region TestSiteUser

		protected TrackingSiteUser TestSiteUser
		{
			get
			{
				if (fTestSiteUser == null)
				{
					fTestSiteUser = new TrackingSiteUser();
					TestContact.OC_WebAccessEnabled = true;
					TestContact.OC_Email = "test@cargowise.com";
					var password = "test";
					TestContact.SetHashedPassword(password);
					TestContact.Factory.Save();
					fTestSiteUser.Login(TestOrg.OH_Code, TestContact.OC_Email, password);
				}
				return fTestSiteUser;
			}
		}
		TrackingSiteUser fTestSiteUser;

		#endregion

		ITransactionSupport TestParent;

		protected override void SetUp()
		{
			base.SetUp();
			TestParent = GetNewBusinessObject();
		}

		#endregion

		public void TestCharges()
		{
			JobHeader testJob = Factory.NewJobForTesting<JobHeader>();
			testJob.JH_ParentID = TestParent.PK;
			testJob.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_TransactionNum = "00001000";
			invoice.AH_TransactionType = "INV";
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_OH = TestOrg.PK;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_JH = testJob.PK;
			invoice.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			AccTransactionLines chargeLine1 = Factory.New<AccTransactionLines>();
			chargeLine1.AL_AH = invoice.PK;
			chargeLine1.AL_JH = testJob.PK;

			AccTransactionLines chargeLine2 = Factory.New<AccTransactionLines>();
			chargeLine2.AL_AH = invoice.PK;
			chargeLine2.AL_JH = testJob.PK;

			AssertNotNull(TestParent);
			AssertNotNull(TestParent.InvoiceLoader.Transactions);
			AssertEquals("Incorrect number of invoices loaded", 1, TestParent.InvoiceLoader.Transactions.Count);
			AssertEquals("Invoice not found", true, TestParent.InvoiceLoader.Transactions.Contains(invoice.PK));
		}

		public void TestChargesRelatedOrg()
		{
			JobHeader testJob = Factory.NewJobForTesting<JobHeader>();
			testJob.JH_ParentID = TestParent.PK;
			testJob.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_TransactionNum = "00001000";
			invoice.AH_TransactionType = "INV";
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_OH = TestOrg.AllParentParties[0].PR_OH_Parent;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_JH = testJob.PK;
			invoice.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			AssertNotNull(TestParent);
			AssertNotNull(TestParent.InvoiceLoader.Transactions);
			AssertEquals("Number of invoices loaded", 1, TestParent.InvoiceLoader.Transactions.Count);
		}

		public void TestChargesReturnsInvoicesForAllCompanies()
		{
			JobHeader testJob1 = Factory.NewJobForTesting<JobHeader>();
			testJob1.JH_ParentID = TestParent.PK;
			testJob1.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader invoice1 = Factory.New<AccTransactionHeader>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_OH = TestOrg.PK;
			invoice1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice1.AH_JH = testJob1.PK;
			invoice1.AH_TransactionType = "INV";
			invoice1.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			AccTransactionLines chargeLine1 = Factory.New<AccTransactionLines>();
			chargeLine1.AL_AH = invoice1.PK;
			chargeLine1.AL_JH = testJob1.PK;

			AccTransactionLines chargeLine2 = Factory.New<AccTransactionLines>();
			chargeLine2.AL_AH = invoice1.PK;
			chargeLine2.AL_JH = testJob1.PK;

			AccTransactionHeader invoice2 = Factory.New<AccTransactionHeader>();
			invoice2.AH_TransactionNum = "00001001";
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice2.AH_OH = TestOrg.PK;
			invoice2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice2.AH_JH = testJob1.PK;
			invoice2.AH_TransactionType = "CRD";
			invoice2.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			JobHeader testJob2 = Factory.NewJobForTesting<JobHeader>();
			testJob2.JH_ParentID = TestParent.PK;
			testJob2.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionLines chargeLine3 = Factory.New<AccTransactionLines>();
			chargeLine3.AL_AH = invoice2.PK;
			chargeLine3.AL_JH = testJob1.PK;

			AccTransactionLines chargeLine4 = Factory.New<AccTransactionLines>();
			chargeLine4.AL_AH = invoice2.PK;
			chargeLine4.AL_JH = testJob2.PK;

			AccTransactionHeader invoice3 = Factory.New<AccTransactionHeader>();
			invoice3.AH_TransactionNum = "00001002";
			invoice3.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice3.AH_OH = GlbCompany.CurrentCompany.OrgProxy.PK; // Different Org
			invoice3.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice3.AH_JH = testJob1.PK;
			invoice3.AH_TransactionType = "INV";
			invoice3.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			JobHeader testJob3 = Factory.NewJobForTesting<JobHeader>();
			testJob3.JH_ParentID = TestParent.PK;
			testJob3.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionLines chargeLine5 = Factory.New<AccTransactionLines>();
			chargeLine5.AL_AH = invoice3.PK;
			chargeLine5.AL_JH = testJob3.PK;

			AssertNotNull(TestParent);
			AssertNotNull(TestParent.InvoiceLoader.Transactions);
			AssertEquals("Incorrect number of charges", 2, TestParent.InvoiceLoader.Transactions.Count);
			AssertEquals("First invoicenot found", true, TestParent.InvoiceLoader.Transactions.Contains(invoice1.PK));
			AssertEquals("Second invoice not found", true, TestParent.InvoiceLoader.Transactions.Contains(invoice2.PK));
		}

		public void TestLocalChargesDetails()
		{
			JobHeader testJob = Factory.NewJobForTesting<JobHeader>();
			testJob.JH_ParentID = TestParent.PK;
			testJob.JH_GC = GlbCompany.CurrentCompany.PK;
			testJob.LocalChargesPK = TestOrg.PK;

			AccTransactionHeader invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_TransactionNum = "00001000";
			invoice.AH_TransactionType = "INV";
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_OH = TestOrg.PK;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_JH = testJob.PK;
			invoice.AH_InvoiceDate = ZDateTime.Now.AddDays(-2);
			invoice.AH_ConsolidatedInvoiceRef = TestParent.Reference;

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

			AccTransactionHeader invoice1 = Factory.New<AccTransactionHeader>();
			invoice1.AH_TransactionNum = "00001001";
			invoice1.AH_TransactionType = "INV";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_OH = TestOrg.PK;
			invoice1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice1.AH_JH = testJob.PK;
			invoice1.AH_InvoiceDate = ZDateTime.Now.AddDays(-1);
			invoice1.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			AccTransactionLines chargeLine3 = Factory.New<AccTransactionLines>();
			chargeLine3.AL_AH = invoice1.PK;
			chargeLine3.AL_JH = testJob.PK;
			chargeLine3.AL_GB = invoice1.AH_GB;
			chargeLine3.AL_GC = invoice1.AH_GC;

			AssertNotNull(TestParent);
			AssertNotNull(TestParent.InvoiceLoader.Transactions);
			AssertNull("LocalChargesDetails should be null because not a ShipmentQuickViewUser", TestParent.InvoiceLoader.LocalChargesDetails);

			TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			AssertNotNull("LocalChargesDetails should be not null", TestParent.InvoiceLoader.LocalChargesDetails);
			AssertEquals("Incorrect number of invoice lines loaded", 2, TestParent.InvoiceLoader.LocalChargesDetails.Count);
			AssertEquals("Invoice line not found", true, TestParent.InvoiceLoader.LocalChargesDetails.Contains(chargeLine1.PK));
			AssertEquals("Invoice line not found", true, TestParent.InvoiceLoader.LocalChargesDetails.Contains(chargeLine2.PK));
		}

		public void TestLocalChargesDetailsReturnLocalChargesOnly()
		{
			JobHeader testJob = Factory.NewJobForTesting<JobHeader>();
			testJob.JH_ParentID = TestParent.PK;
			testJob.JH_GC = GlbCompany.CurrentCompany.PK;
			testJob.LocalChargesPK = TestOrg.PK;

			AccTransactionHeader invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_TransactionNum = "00001000";
			invoice.AH_TransactionType = "INV";
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_JH = testJob.PK;
			invoice.AH_InvoiceDate = ZDateTime.Now.AddDays(-2);
			invoice.AH_ConsolidatedInvoiceRef = TestParent.Reference;

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

			AccTransactionHeader invoice1 = Factory.New<AccTransactionHeader>();
			invoice1.AH_TransactionNum = "00001001";
			invoice1.AH_TransactionType = "INV";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_OH = TestOrg.PK;
			invoice1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice1.AH_JH = testJob.PK;
			invoice1.AH_InvoiceDate = ZDateTime.Now.AddDays(-1);
			invoice1.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			AccTransactionLines chargeLine3 = Factory.New<AccTransactionLines>();
			chargeLine3.AL_AH = invoice1.PK;
			chargeLine3.AL_JH = testJob.PK;
			chargeLine3.AL_GB = invoice1.AH_GB;
			chargeLine3.AL_GC = invoice1.AH_GC;

			AssertNotNull(TestParent);
			AssertNotNull(TestParent.InvoiceLoader.Transactions);
			AssertNull("LocalChargesDetails should be null because not a ShipmentQuickViewUser", TestParent.InvoiceLoader.LocalChargesDetails);

			TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			AssertNotNull("LocalChargesDetails should be not null", TestParent.InvoiceLoader.LocalChargesDetails);
			AssertEquals("Incorrect number of invoice lines loaded", 1, TestParent.InvoiceLoader.LocalChargesDetails.Count);
			AssertEquals("Invoice line not found", true, TestParent.InvoiceLoader.LocalChargesDetails.Contains(chargeLine3.PK));
		}

		public void TestChargesReturnsInvoicesForAllCompaniesAndOrganisations()
		{
			JobHeader testJob1 = Factory.NewJobForTesting<JobHeader>();
			testJob1.JH_ParentID = TestParent.PK;
			testJob1.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader invoice1 = Factory.New<AccTransactionHeader>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			invoice1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice1.AH_JH = testJob1.PK;
			invoice1.AH_TransactionType = "INV";
			invoice1.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			AccTransactionLines chargeLine1 = Factory.New<AccTransactionLines>();
			chargeLine1.AL_AH = invoice1.PK;
			chargeLine1.AL_JH = testJob1.PK;

			AccTransactionLines chargeLine2 = Factory.New<AccTransactionLines>();
			chargeLine2.AL_AH = invoice1.PK;
			chargeLine2.AL_JH = testJob1.PK;

			AccTransactionHeader invoice2 = Factory.New<AccTransactionHeader>();
			invoice2.AH_TransactionNum = "00001001";
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice2.AH_OH = TestOrg.PK;
			invoice2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice2.AH_JH = testJob1.PK;
			invoice2.AH_TransactionType = "CRD";
			invoice2.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			JobHeader testJob2 = Factory.NewJobForTesting<JobHeader>();
			testJob2.JH_ParentID = TestParent.PK;
			testJob2.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionLines chargeLine3 = Factory.New<AccTransactionLines>();
			chargeLine3.AL_AH = invoice2.PK;
			chargeLine3.AL_JH = testJob1.PK;

			AccTransactionLines chargeLine4 = Factory.New<AccTransactionLines>();
			chargeLine4.AL_AH = invoice2.PK;
			chargeLine4.AL_JH = testJob2.PK;

			AssertNotNull(TestParent);

			TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			AssertNotNull(TestParent.InvoiceLoader.Transactions);
			AssertEquals("Incorrect number of charges", 2, TestParent.InvoiceLoader.Transactions.Count);
			AssertEquals("First invoicenot found", true, TestParent.InvoiceLoader.Transactions.Contains(invoice1.PK));
			AssertEquals("Second invoice not found", true, TestParent.InvoiceLoader.Transactions.Contains(invoice2.PK));
		}

		public void TestChargesTotalsAsString()
		{
			JobHeader testJob1 = Factory.NewJobForTesting<JobHeader>();
			testJob1.JH_ParentID = TestParent.PK;
			testJob1.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader invoice1 = Factory.New<AccTransactionHeader>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_OH = TestOrg.PK;
			invoice1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice1.AH_JH = testJob1.PK;
			invoice1.AH_TransactionType = "INV";
			invoice1.AH_OSTotal = 1000;
			invoice1.AH_RX_NKTransactionCurrency = "AUD";
			invoice1.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			AccTransactionHeader invoice2 = Factory.New<AccTransactionHeader>();
			invoice2.AH_TransactionNum = "00000100";
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice2.AH_OH = TestOrg.PK;
			invoice2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice2.AH_JH = testJob1.PK;
			invoice2.AH_TransactionType = "INV";
			invoice2.AH_OSTotal = 100;
			invoice2.AH_RX_NKTransactionCurrency = "AUD";
			invoice2.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			JobHeader testJob2 = Factory.NewJobForTesting<JobHeader>();
			testJob2.JH_ParentID = TestParent.PK;
			testJob2.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader invoice3 = Factory.New<AccTransactionHeader>();
			invoice3.AH_TransactionNum = "00000200";
			invoice3.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice3.AH_OH = TestOrg.PK;
			invoice3.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice3.AH_JH = testJob1.PK;
			invoice3.AH_TransactionType = "INV";
			invoice3.AH_OSTotal = 200;
			invoice3.AH_RX_NKTransactionCurrency = "HKD";
			invoice3.AH_ConsolidatedInvoiceRef = TestParent.Reference;

			AssertNotNull(TestParent);
			AssertNotNull(TestParent.InvoiceLoader.Transactions);
			AssertEquals("Should be three transactions", 3, TestParent.InvoiceLoader.Transactions.Count);
			AssertContains("Totals as string should contain AUD 1100", "AUD 1,100.00", TestParent.InvoiceLoader.ChargesTotalsAsString);
			AssertContains("Totals as string should contain HKD 200", "HKD 200.00", TestParent.InvoiceLoader.ChargesTotalsAsString);
			AssertContains("Totals as string should contain comma", ", ", TestParent.InvoiceLoader.ChargesTotalsAsString);
		}
	}
}
