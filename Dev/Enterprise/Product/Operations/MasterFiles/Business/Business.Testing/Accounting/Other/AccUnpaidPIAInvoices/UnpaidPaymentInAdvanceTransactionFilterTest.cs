using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UnpaidPaymentInAdvanceTransactionFilter))]
	sealed class UnpaidPaymentInAdvanceTransactionFilterTest : NonPersistentBusinessObjectTestCase
	{
		class TestRelatedJobNumber : IRelatedJobNumber
		{
			public TestRelatedJobNumber(string[] jobNumber)
			{
				this.fJobNumber = jobNumber;
			}

			readonly string[] fJobNumber;

			public string[] JobNumber
			{
				get { return fJobNumber; }
			}
		}

		public void TestNoSQLExceptionWhenJobNumbersTooLarge()
		{
			var numberOfJobs = 525;
			var jobNumbers = new string[numberOfJobs];
			for (int i = 0; i < numberOfJobs; i++)
			{
				jobNumbers[i] = i.ToString();
			}

			var relatedJobNumber = new TestRelatedJobNumber(jobNumbers);
			var filterObj = new UnpaidPaymentInAdvanceTransactionFilter(relatedJobNumber);

			bool hasUnPaidInvoices = false;
			AssertNoExceptionThrown(() => hasUnPaidInvoices = filterObj.HasUnPaidPIAInvoices);

			AccTransactionHeaderCollection collection;
			AssertNoExceptionThrown(() => collection = filterObj.Transactions);
		}

		public void TestTransactions()
		{
			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			GlbBranch currentCompanyBranch = currentCompany.Branches[0];

			GlbCompany otherCompany = Factory.New<GlbCompany>();
			GlbBranch otherCompanyBranch = Factory.New<GlbBranch>();
			otherCompanyBranch.GB_GC = otherCompany.PK;
			Factory.Save();

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "ORG3";
			Factory.Save();

			AccTransactionHeader invoiceOrg1CurrentCompany = GetInvoiceForTest(org1, currentCompanyBranch, "00001001", "S00009999");
			invoiceOrg1CurrentCompany.AH_FullyPaidDate = ZDateTime.Now;
			AccTransactionHeader invoiceOrg1OtherCompany = GetInvoiceForTest(org1, otherCompanyBranch, "00001002", "S00009999");
			invoiceOrg1OtherCompany.AH_FullyPaidDate = ZDateTime.Now;
			AccTransactionHeader invoiceOrg2CurrentCompany = GetInvoiceForTest(org2, currentCompanyBranch, "00001003", "S00009999");
			invoiceOrg2CurrentCompany.AH_FullyPaidDate = ZDateTime.Now;
			AccTransactionHeader invoiceOrg2OtherCompany = GetInvoiceForTest(org2, otherCompanyBranch, "00001004", "S00009999");
			invoiceOrg2OtherCompany.AH_FullyPaidDate = ZDateTime.Now;
			AccTransactionHeader invoiceOrg3CurrentCompany = GetInvoiceForTest(org1, currentCompanyBranch, "00001005", "S00009999/A");
			invoiceOrg3CurrentCompany.AH_FullyPaidDate = ZDateTime.Now;
			AccTransactionHeader invoiceOrg3OtherCompany = GetInvoiceForTest(org3, otherCompanyBranch, "00001006", "S00009999/A");
			invoiceOrg3OtherCompany.AH_FullyPaidDate = ZDateTime.Now;

			invoiceOrg1CurrentCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg2OtherCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg2OtherCompany.AH_FullyPaidDate = ZDateTime.Empty;
			invoiceOrg3OtherCompany.AH_InvoiceTerm = "PIA";
			Factory.Save();

			UnpaidPaymentInAdvanceTransactionFilter transactionFilter = GetNewBusinessObject() as UnpaidPaymentInAdvanceTransactionFilter;
			AssertNotNull(transactionFilter);
			AssertEquals("There are not unpaid PIA transactions.", 0, transactionFilter.Transactions.Count);

			invoiceOrg1CurrentCompany.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();
			transactionFilter.ResetTransactions();
			AssertEquals("Should be 1 unpaid PIA transaction.", 1, transactionFilter.Transactions.Count);
			AssertContains(invoiceOrg1CurrentCompany.PK, transactionFilter.Transactions);

			invoiceOrg2CurrentCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg2CurrentCompany.AH_FullyPaidDate = ZDateTime.Empty;
			invoiceOrg2OtherCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg2OtherCompany.AH_FullyPaidDate = ZDateTime.Empty;
			invoiceOrg3OtherCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg3OtherCompany.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();
			transactionFilter.ResetTransactions();
			AssertEquals("Should be 2 unpaid PIA transactions.", 2, transactionFilter.Transactions.Count);
			AssertContains(invoiceOrg1CurrentCompany.PK, transactionFilter.Transactions);
			AssertContains(invoiceOrg2CurrentCompany.PK, transactionFilter.Transactions);

			invoiceOrg3CurrentCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg3CurrentCompany.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();
			transactionFilter.ResetTransactions();
			AssertEquals("Should be 3 unpaid PIA transactions.", 3, transactionFilter.Transactions.Count);
			AssertContains(invoiceOrg1CurrentCompany.PK, transactionFilter.Transactions);
			AssertContains(invoiceOrg2CurrentCompany.PK, transactionFilter.Transactions);
			AssertContains(invoiceOrg3CurrentCompany.PK, transactionFilter.Transactions);
		}

		[TestDate(2020, 1, 30)]
		public void TestTransactionWithEmptyJobNumber()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var currentCompanyBranch = currentCompany.Branches[0];

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";
			Factory.Save();

			var invoiceOrg1CurrentCompany = GetInvoiceForTest(org1, currentCompanyBranch, "00001001", "S00009999");
			var invoiceOrg2CurrentCompany = GetInvoiceForTest(org2, currentCompanyBranch, "00001003", "S00009999");
			invoiceOrg1CurrentCompany.AH_FullyPaidDate = ZDateTime.Empty;
			invoiceOrg2CurrentCompany.AH_FullyPaidDate = ZDateTime.Empty;
			invoiceOrg1CurrentCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg2CurrentCompany.AH_InvoiceTerm = "PIA";
			Factory.Save();

			TestJobNumber = new TestRelatedJobNumber(new string[] { "S00009999" });
			var transactionFilter = GetNewBusinessObject() as UnpaidPaymentInAdvanceTransactionFilter;

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (Db.Connection.TrackExecutedCommands())
			{
				AssertEquals(2, transactionFilter.Transactions.Count);
				AssertEquals("Should create Job Number Filter", true, Db.Connection.ExecutedCommands.Any(x => x.Contains("AH_JobNumber")));
			}

			TestJobNumber = new TestRelatedJobNumber(new string[] { string.Empty });
			transactionFilter = GetNewBusinessObject() as UnpaidPaymentInAdvanceTransactionFilter;

			using (Db.Connection.TrackExecutedCommands())
			{
				AssertEquals("Job with empty job number has NO transactions.", 0, transactionFilter.Transactions.Count);
				AssertEquals("Should not create Job Number Filter", false, Db.Connection.ExecutedCommands.Any(x => x.Contains("AH_JobNumber")));
			}
		}

		public void TestResetTransactions()
		{
			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			GlbBranch currentCompanyBranch = currentCompany.Branches[0];

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			Factory.Save();

			AccTransactionHeader invoiceOrg1CurrentCompany = GetInvoiceForTest(org1, currentCompanyBranch, "00001001", "S00009999");
			invoiceOrg1CurrentCompany.AH_ConsolidatedInvoiceRef = TestJobNumber.JobNumber[0];
			invoiceOrg1CurrentCompany.AH_FullyPaidDate = ZDateTime.Now;

			invoiceOrg1CurrentCompany.AH_InvoiceTerm = "PIA";
			Factory.Save();

			UnpaidPaymentInAdvanceTransactionFilter transactionFilter = GetNewBusinessObject() as UnpaidPaymentInAdvanceTransactionFilter;

			AssertNotNull(transactionFilter);
			AssertEquals("There are not unpaid PIA transactions.", 0, transactionFilter.Transactions.Count);

			invoiceOrg1CurrentCompany.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("There are not unpaid PIA transactions whithout reseting collection.", 0, transactionFilter.Transactions.Count);

			transactionFilter.ResetTransactions();
			AssertEquals("There is 1 unpaid PIA transaction after reseting collection.", 1, transactionFilter.Transactions.Count);
		}

		public void TestHasUnPaidPIAInvoices()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var currentCompanyBranch = currentCompany.Branches[0];

			var otherCompany = Factory.New<GlbCompany>();
			var otherCompanyBranch = Factory.New<GlbBranch>();
			otherCompanyBranch.GB_GC = otherCompany.PK;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";

			var invoiceOrg1CurrentCompany = GetInvoiceForTest(org1, currentCompanyBranch, "00001001", "S00008888");
			var invoiceOrg1OtherCompany = GetInvoiceForTest(org1, otherCompanyBranch, "00001002", "S00009999");
			var invoiceOrg2CurrentCompany = GetInvoiceForTest(org2, currentCompanyBranch, "00001003", "S00008888");
			var invoiceOrg2OtherCompany = GetInvoiceForTest(org2, otherCompanyBranch, "00001004", "S00009999");
			Factory.Save();

			var transactionFilter = GetNewBusinessObject() as UnpaidPaymentInAdvanceTransactionFilter;
			AssertNotNull(transactionFilter);

			Assert("Shipment must not have unpaid PIA invoices when one have not any PIA invoices.", !transactionFilter.HasUnPaidPIAInvoices);
			Assert("Org1 has no unpaid PIA invoices", !transactionFilter.HasUnPaidPIAInvoicesForOrg(org1.PK));
			Assert("Org2 has no unpaid PIA invoices", !transactionFilter.HasUnPaidPIAInvoicesForOrg(org2.PK));

			invoiceOrg1CurrentCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg2OtherCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg2CurrentCompany.AH_InvoiceTerm = "PIA";
			Factory.Save();
			Assert("Shipment must have unpaid PIA invoices.", !transactionFilter.HasUnPaidPIAInvoices);
			Assert("Org1 has no unpaid PIA invoices", !transactionFilter.HasUnPaidPIAInvoicesForOrg(org1.PK));
			Assert("Org2 has no unpaid PIA invoices", !transactionFilter.HasUnPaidPIAInvoicesForOrg(org2.PK));

			transactionFilter = new UnpaidPaymentInAdvanceTransactionFilter(
				new TestRelatedJobNumber(new string[] { "S00009999", "S00008888" }));
			Assert("Shipment must have unpaid PIA invoices.", transactionFilter.HasUnPaidPIAInvoices);
			Assert("Org1 has unpaid PIA invoices", transactionFilter.HasUnPaidPIAInvoicesForOrg(org1.PK));
			Assert("Org2 has unpaid PIA invoices", transactionFilter.HasUnPaidPIAInvoicesForOrg(org2.PK));

			invoiceOrg1CurrentCompany.AH_FullyPaidDate = ZDateTime.Now;
			invoiceOrg2CurrentCompany.AH_FullyPaidDate = ZDateTime.Now;
			Factory.Save();
			Assert("Shipment must not have unpaid PIA invoices.", !transactionFilter.HasUnPaidPIAInvoices);
			Assert("Org1 has no unpaid PIA invoices", !transactionFilter.HasUnPaidPIAInvoicesForOrg(org1.PK));
			Assert("Org2 has no unpaid PIA invoices", !transactionFilter.HasUnPaidPIAInvoicesForOrg(org2.PK));
		}

		[TestDate(2020, 1, 30)]
		public void TestHasUnPaidPIAInvoicesWithEmptyJobNumber()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var currentCompanyBranch = currentCompany.Branches[0];

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";
			Factory.Save();

			var invoiceOrg1CurrentCompany = GetInvoiceForTest(org1, currentCompanyBranch, "00001001", "S00008888");
			var invoiceOrg2CurrentCompany = GetInvoiceForTest(org2, currentCompanyBranch, "00001003", "S00009999");
			invoiceOrg1CurrentCompany.AH_FullyPaidDate = ZDateTime.Empty;
			invoiceOrg2CurrentCompany.AH_FullyPaidDate = ZDateTime.Empty;
			invoiceOrg1CurrentCompany.AH_InvoiceTerm = "PIA";
			invoiceOrg2CurrentCompany.AH_InvoiceTerm = "PIA";
			Factory.Save();

			TestJobNumber = new TestRelatedJobNumber(new string[] { "S00009999", "S00008888" });
			var transactionFilter = GetNewBusinessObject() as UnpaidPaymentInAdvanceTransactionFilter;

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (Db.Connection.TrackExecutedCommands())
			{
				AssertEquals(true, transactionFilter.HasUnPaidPIAInvoices);
				AssertEquals("Should create Job Number Filter", true, Db.Connection.ExecutedCommands.Any(x => x.Contains("AH_JobNumber")));
			}

			TestJobNumber = new TestRelatedJobNumber(new string[] { "", "" });
			transactionFilter = GetNewBusinessObject() as UnpaidPaymentInAdvanceTransactionFilter;

			using (Db.Connection.TrackExecutedCommands())
			{
				AssertEquals("Job with empty job number has NO unpaid PIA invoices.", false, transactionFilter.HasUnPaidPIAInvoices);
				AssertEquals("Should NOT create Job Number Filter", false, Db.Connection.ExecutedCommands.Any(x => x.Contains("AH_JobNumber")));
			}
		}

		#region Implementation

		void AssertContains(ZGuid pK, System.Collections.IEnumerable collection)
		{
			bool isContain = false;
			foreach (BusinessObject bizo in collection)
			{
				if (pK.Equals(bizo.PK))
				{
					isContain = true;
					break;
				}
			}
			AssertEquals(true, isContain);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UnpaidPaymentInAdvanceTransactionFilter(TestJobNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestJobNumber = new TestRelatedJobNumber(new string[] { "S00009999" });
		}

		TestRelatedJobNumber TestJobNumber;

		#region Create Business Objects

		AccTransactionHeader GetInvoiceForTest(OrgHeader organisation, GlbBranch branch, ZString invoiceNumber, ZString jobNumber)
		{
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = organisation.PK;
			newInvoice.AH_TransactionNum = invoiceNumber;
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = branch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = jobNumber;

			return newInvoice;
		}

		#endregion

		#endregion
	}
}
