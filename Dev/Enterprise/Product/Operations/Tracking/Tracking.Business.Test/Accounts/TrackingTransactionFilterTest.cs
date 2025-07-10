using System;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingTransactionFilterBusinessObject))]
	[HttpContextEnabledTest]
	sealed class TrackingTransactionFilterTest : ARTransactionFilterTest
	{
		#region TestNumberFilter

		public void TestNumberFilterListDoesNotContainNoneOption()
		{
			Assert("Expected NumberFilter List to not contain None option", !TestARFilterBizO.AH_NumberFilter_List.ContainsCode(FilterBusinessObject.QueryDeciderNoSelectionCode));
		}

		#endregion TestNumberFilter

		#region TestDateFilterList

		public override void TestDateFilterList()
		{
			AssertEquals("Expected List to contain 3 elements", 3, TestARFilterBizO.AH_DateFilter_List.Count);
			Assert("Expected List to contain ALL option", TestARFilterBizO.AH_DateFilter_List.ContainsCode("All"));
			Assert("Expected List to contain Transaction Date option", TestARFilterBizO.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.TransactionDate));
			Assert("Expected List to contain Due Date option", TestARFilterBizO.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.DueDate));
		}

		#endregion TestDateFilterList

		#region TestTransactionTypeList

		public void TestTransactionTypeList()
		{
			AssertEquals("Expected 3 elements in TransactionTypeList", 3, TestARFilterBizO.TransactionTypeList.Count);
			Assert("Expected TransactionTypeList to contain ALL transaction type", TestARFilterBizO.TransactionTypeList.ContainsCode("ALL"));
			Assert("Expected TransactionTypeList to contain CRD transaction type", TestARFilterBizO.TransactionTypeList.ContainsCode("CRD"));
			Assert("Expected TransactionTypeList to contain INV transaction type", TestARFilterBizO.TransactionTypeList.ContainsCode("INV"));
		}

		#endregion TestTransactionTypeList

		#region TestNoCurrentOrgReturnsNoTransactions

		public void TestNoCurrentOrgReturnsNoTransactions()
		{
			AccTransactionHeader trans1 = Factory.New<AccTransactionHeader>();
			trans1.AH_OH = TestOrg.PK;
			trans1.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionHeader trans2 = Factory.New<AccTransactionHeader>();
			trans2.AH_OH = TestOrg.PK;
			trans2.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionHeader trans3 = Factory.New<AccTransactionHeader>();
			trans3.AH_OH = TestOrg.PK;
			trans3.AH_TransactionType = TransactionTypes.Invoice;

			TestARFilterBizO.AH_OH = ZGuid.Empty;
			TestCollection.Load(TestARFilterBizO.Filter);

			AssertEquals("Expected collection to contain 0 elements", 0, TestCollection.Count);
		}

		#endregion TestNoCurrentOrgReturnsNoTransactions

		#region TestOnlyTransactionsFromCurrentOrgAreReturned

		public void TestOnlyTransactionsFromCurrentOrgAreReturned()
		{
			OrgHeader otherOrg = Factory.NewWithValidTestData<OrgHeader>();

			AccTransactionHeader trans1 = GetNewTransaction();
			trans1.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionHeader trans2 = GetNewTransaction();
			trans2.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionHeader trans3 = GetNewTransaction();
			trans3.AH_OH = otherOrg.PK;
			trans3.AH_TransactionType = TransactionTypes.Invoice;

			TestCollection.Load(TestARFilterBizO.Filter);

			AssertEquals("Expected collection to contain 2 elements", 2, TestCollection.Count);
			Assert("Expected first transaction to be included in the collection", TestCollection.Contains(trans1));
			Assert("Expected second transaction to be included in the collection", TestCollection.Contains(trans2));
		}

		#endregion TestOnlyTransactionsFromCurrentOrgAreReturned

		#region TestFilterOnlyReturnsInvoicesAndCreditNotes

		public void TestFilterOnlyReturnsInvoicesAndCreditNotes()
		{
			AccTransactionHeader trans1 = GetNewTransaction();
			trans1.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionHeader trans2 = GetNewTransaction();
			trans2.AH_TransactionType = TransactionTypes.CreditNote;

			AccTransactionHeader trans3 = GetNewTransaction();
			trans3.AH_TransactionType = TransactionTypes.AdjustmentNote;

			AccTransactionHeader trans4 = GetNewTransaction();
			trans4.AH_TransactionType = TransactionTypes.Discount;

			AccTransactionHeader trans5 = GetNewTransaction();
			trans5.AH_TransactionType = TransactionTypes.Overpayment;

			AccTransactionHeader trans6 = GetNewTransaction();
			trans6.AH_TransactionType = TransactionTypes.Payment;

			AccTransactionHeader trans7 = GetNewTransaction();
			trans7.AH_TransactionType = TransactionTypes.Receipt;

			TestCollection.Load(TestARFilterBizO.Filter);

			AssertEquals("Expected only 2 transactions in collection", 2, TestCollection.Count);
			Assert("Transaction 1 should be in the collection", TestCollection.Contains(trans1));
			Assert("Transaction 2 should be in the collection", TestCollection.Contains(trans2));
		}

		#endregion TestFilterOnlyReturnsInvoicesAndCreditNotes

		#region TestNumberFilterIgnoresOtherFields

		public new void TestNumberFilterIgnoresOtherFields()
		{
			Assert("Filter should never ignore AH_OH", true);
			Assert("Filter should never ignore PaymentStatus", true);
		}

		public void TestNumberFilterIgnoresOtherFieldsExceptAH_OH()
		{
			AccTransactionHeader trans1 = GetNewTransaction();
			trans1.AH_TransactionType = TransactionTypes.CreditNote;
			trans1.AH_TransactionNum = "00001001";

			AccTransactionHeader trans2 = GetNewTransaction();
			trans2.AH_TransactionType = TransactionTypes.Invoice;
			trans2.AH_TransactionNum = "00001002";

			AccTransactionHeader trans3 = GetNewTransaction();
			trans3.AH_TransactionType = TransactionTypes.AdjustmentNote;
			trans3.AH_TransactionNum = "00001003";

			OrgHeader otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			AccTransactionHeader trans4 = GetNewTransaction();
			trans4.AH_TransactionType = TransactionTypes.Invoice;
			trans4.AH_OH = otherOrg.PK;
			trans4.AH_TransactionNum = "00001001";

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.TransactionNumber;
			TestARFilterBizO.AH_Number = "00001001";
			TestARFilterBizO.AH_TransactionType = TransactionTypes.Invoice;

			TestCollection.Load(TestARFilterBizO.Filter);
			AssertEquals("Expected 1 transactions to be in the collection", 1, TestCollection.Count);
			Assert("Only Trans1 should be in the list", TestCollection.Contains(trans1));
		}

		public void TestNumberFilterDoesNotIgnorePaymentStatus()
		{
			AccTransactionHeader transaction1 = GetNewTransaction();
			transaction1.AH_TransactionNum = "1001";
			transaction1.AH_TransactionType = TransactionTypes.Invoice;
			transaction1.AH_FullyPaidDate = new DateTime(2009, 3, 4);

			AccTransactionHeader transaction2 = GetNewTransaction();
			transaction2.AH_TransactionType = TransactionTypes.Invoice;
			transaction2.AH_TransactionNum = "1002";

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.TransactionNumber;
			TestARFilterBizO.AH_Number = "10";
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.StartsWith;

			TestCollection.Load(TestARFilterBizO.Filter);
			AssertEquals("Expected 2 transactions to be in the collection", 2, TestCollection.Count);

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.Unpaid;
			TestCollection.Load(TestARFilterBizO.Filter);

			AssertEquals("Expected 1 transactions to be in the collection", 1, TestCollection.Count);
			Assert("Only unpaid transaction should be in the list", TestCollection.Contains(transaction2));
		}

		#endregion TestNumberFilterIgnoresOtherFields

		#region TestJobInvNumberFilteringUsingContainsAndStartsWith

		public override void TestJobInvNumberFilteringUsingContainsAndStartsWith()
		{
			Assert("not applicable", true);
		}
		#endregion

		#region TestGetStatementAlwaysUsesLoggedInOrg

		public void TestGetStatementAlwaysUsesLoggedInOrg()
		{
			TestARFilterBizO.AH_OH = ZGuid.Empty;
			Assert("PreCondition: FilterOrg should initially be empty", TestARFilterBizO.AH_OH.IsEmpty);
			try
			{
				GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
				GlbBranch companyBranch = company.Branches.AddNew();

				OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
				companyData.OB_GC = company.PK;
				companyData.OB_OH = Helper.TestOrg.PK;
				companyData.OB_IsDebtor = true;

				Factory.Save();

				TestARFilterBizO.Company = company.PK;

				AssertEquals("PDF result should be empty", 0, TestARFilterBizO.GetStatementForSelectedCompany().Length);
				GlbCompany targetCompany = TestARFilterBizO.Companies.FindByPK(TestARFilterBizO.Company) as GlbCompany;
				AssertNotNull(targetCompany);
				GlbBranch targetBranch = targetCompany.Branches[0];
				ZString expectedError = ZString.Format("Failed to create Statement for branch {0} ({1}) for company {2}\n", targetBranch.GB_BranchName, targetBranch.PK, targetCompany.GC_Name);
				expectedError += string.Format("AccHeader OrgHeader is invalid: {0}\n", ZGuid.Empty);
				expectedError += string.Format("LoggedInUser: {0}\n", string.Format("{0} ({1})", User.OC_Email, User.Header.OH_Code));
				AssertMultilineASCIIEquals("DeveloperError should have been reported", expectedError, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
		#endregion

		#region TestGetCorrectBranchForStatement

		public void TestGetCorrectBranchForStatement()
		{
			DummyTrackingTransactionFilterBusinessObject testFilterBizO = FilterBusinessObjectFactory.New<DummyTrackingTransactionFilterBusinessObject>();
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			Assert("Precondition: TestFilterBizO.AH_OH is empty", testFilterBizO.AH_OH.IsEmpty);
			GlbBranch result = testFilterBizO.GetBranchForTest(GlbCompany.CurrentCompany);
			AssertNotNull(result);
			AssertEquals("Branch returned should be the first branch of CurrentCompany", GlbCompany.CurrentCompany.Branches[0].PK, result.PK);

			testFilterBizO.AH_OH = testOrg.PK;
			result = testFilterBizO.GetBranchForTest(GlbCompany.CurrentCompany);
			AssertNotNull(result);
			AssertEquals("Branch returned should be the first branch of CurrentCompany", GlbCompany.CurrentCompany.Branches[0].PK, result.PK);

			Assert("CurrentCompany should have more than one branch", GlbCompany.CurrentCompany.Branches.Count > 1);

			testOrg.CompanyData.OB_GB_ControllingBranch = GlbCompany.CurrentCompany.Branches[1].PK;

			Factory.Save();

			result = testFilterBizO.GetBranchForTest(GlbCompany.CurrentCompany);
			AssertNotNull(result);
			Assert(GlbCompany.CurrentCompany.Branches.Contains(result));
			AssertEquals("Controlling branch should be the second branch of CurrentCompany", GlbCompany.CurrentCompany.Branches[1].PK, result.PK);
		}

		#endregion TestGetCorrectBranchForStatement

		#region TestGetStatementReportPrintableStatementFailure

		public void TestGetStatementReportPrintableStatementFailure()
		{
			DummyTrackingTransactionFilterBusinessObject testARFilterBizO = FilterBusinessObjectFactory.New<DummyTrackingTransactionFilterBusinessObject>();
			testARFilterBizO.LoggedInUser = Helper.TestContact;
			testARFilterBizO.HasChanges = false;

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch companyBranch = company.Branches.AddNew();

			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company.PK;
			companyData.OB_OH = Helper.TestOrg.PK;
			companyData.OB_IsDebtor = true;

			Factory.Save();

			testARFilterBizO.Company = company.PK;

			AssertEquals("AH_OH should not be empty", false, testARFilterBizO.AH_OH.IsEmpty);
			AssertEquals("AH_OH should be valid", true, testARFilterBizO.AH_OH.IsValid);
			AssertEquals("Company should be valid", true, testARFilterBizO.Company.IsValid);

			GlbCompany expCompany = testARFilterBizO.Companies[0];
			GlbBranch expBranch = expCompany.Branches[0];
			try
			{
				byte[] pDF = testARFilterBizO.GetStatementForSelectedCompany();
				AssertNotNull("PDF should not be null", pDF);
				AssertEquals("PDF should be empty", 0, pDF.Length);
				ZString expectedError = ZString.Format("Failed to create Statement for branch {0} ({1}) for company {2}\nPrintableStatement is null\n", expBranch.GB_BranchName, expBranch.PK, expCompany.GC_Name);
				AssertMultilineASCIIEquals("DeveloperError should have been reported", expectedError, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
		#endregion

		#region TestGetStatementSelectedCompanyNull

		public void TestGetStatementSelectedCompanyNull()
		{
			ZGuid companyPK = ZGuid.NewZGuid();
			TestARFilterBizO.Company = companyPK;
			AssertEquals("AH_OH should not be empty", false, TestARFilterBizO.AH_OH.IsEmpty);
			AssertEquals("AH_OH should be valid", true, TestARFilterBizO.AH_OH.IsValid);
			AssertEquals("Company should be valid", true, TestARFilterBizO.Company.IsValid);
			AssertEquals("CompanyPK should not be in CompaniesList", false, TestARFilterBizO.Companies.Contains(Factory.Load<GlbCompany>(companyPK)));

			try
			{
				byte[] pDF = TestARFilterBizO.GetStatementForSelectedCompany();
				AssertNotNull("PDF should not be null", pDF);
				AssertEquals("PDF should be empty", 0, pDF.Length);

				ZString expectedError = string.Format("Failed to generate web statement for company.\nCannot find company in collection of companies by PK {0}. ", companyPK);
				expectedError += string.Format("\nCompanies collection contain the following {0} items: ", TestARFilterBizO.Companies.Count);
				foreach (GlbCompany company in TestARFilterBizO.Companies)
				{
					expectedError += string.Format("{0} [{1}]\n", company.GC_Name, company.PK);
				}
				expectedError += "\n\nAdditional Information:\n";
				expectedError += string.Format("LoggedInUser: {0}\n", User.OC_Email);
				expectedError += string.Format("Organization: {0}\n", string.Format("{0} ({1})", User.Header.OH_Code, User.Header.OH_FullName));
				expectedError += string.Format("Company: {0}\n", companyPK);
				expectedError += string.Format("SelectedCompany: NULL\n");
				expectedError += string.Format("Filter.AH_OH: {0}\n", TestARFilterBizO.AH_OH);
				AssertMultilineASCIIEquals("DeveloperError should have been reported", expectedError, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
		#endregion

		#region TestGetStatementWithNoBranch

		public void TestGetStatementWithNoBranch()
		{
			GlbCompany testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Name = "Test Company";
			testCompany.GC_Code = "TST";

			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = testCompany.PK;
			companyData.OB_OH = Helper.TestOrg.PK;
			companyData.OB_IsDebtor = true;

			Factory.Save();

			AssertEquals("Should contain no branches", 0, testCompany.Branches.Count);

			AssertEquals("AH_OH should not be empty", false, TestARFilterBizO.AH_OH.IsEmpty);
			AssertEquals("AH_OH should be valid", true, TestARFilterBizO.AH_OH.IsValid);
			TestARFilterBizO.Company = testCompany.PK;

			try
			{
				byte[] pDF = TestARFilterBizO.GetStatementForSelectedCompany();
				AssertNotNull("PDF should not be null", pDF);
				AssertEquals("PDF should be empty", 0, pDF.Length);
				ZString expectedError = ZString.Format("Failed to generate web statement for company.\nNo branches found for company {0}({1}).\n", testCompany.GC_Code, testCompany.GC_Name);
				expectedError += "\n\nAdditional Information:\n";
				expectedError += string.Format("LoggedInUser: {0}\n", User.OC_Email);
				expectedError += string.Format("Organization: {0}\n", string.Format("{0} ({1})", User.Header.OH_Code, User.Header.OH_FullName));
				expectedError += string.Format("Company: {0}\n", testCompany.PK);
				expectedError += string.Format("SelectedCompany: {0}\n", string.Format("{0} ({1})", testCompany.GC_Code, testCompany.GC_Name));
				expectedError += string.Format("Filter.AH_OH: {0}\n", TestARFilterBizO.AH_OH);
				AssertMultilineASCIIEquals("DeveloperError should have been reported", expectedError, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestSettingLoggedInUserSetsAH_OH

		public void TestSettingLoggedInUserSetsAH_OH()
		{
			TrackingTransactionFilterBusinessObject newFilter = (TrackingTransactionFilterBusinessObject)base.GetNewBusinessObject();
			newFilter.LoggedInUser = null;
			AssertNull("TestARFilterBizO LoggedInUser", newFilter.LoggedInUser);
			AssertEquals("TestARFilterBizO AH_OH", ZGuid.Empty, newFilter.AH_OH);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();

			newFilter.LoggedInUser = contact;
			AssertEquals("TestARFilterBizO LoggedInUser", contact.PK, newFilter.LoggedInUser.PK);
			AssertEquals("TestARFilterBizO AH_OH", org.PK, newFilter.AH_OH);
		}
		#endregion

		#region TestLedgerFiltering

		public new void TestLedgerFiltering()
		{
			ARInvoice aRInvoice = GetNewInvoice();
			APInvoice aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			aPInvoice.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote = GetNewCreditNote();
			APCreditNote aPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			aPCreditNote.AH_OH = TestOrg.PK;

			Factory.Save();

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestCollection.Load(TestARFilterBizO.Filter);

			AssertEquals("Collection should contain two transaction", 2, TestCollection.Count);
			Assert("AccountsReceivable Invoice should be returned", TestCollection.Contains(aRInvoice));
			Assert("AccountsReceivable CreditNote should be returned", TestCollection.Contains(aRCreditNote));
		}

		#endregion TestLedgerFiltering

		#region TestNumberFiltering

		public new void TestNumberFiltering()
		{
			ARInvoice trans1 = GetNewInvoice();
			trans1.IsManuallySetTransactionNumber_ForTestOnly = true;
			trans1.AH_TransactionNum = "00001013";
			trans1.AH_OH = TestOrg.PK;

			ARInvoice trans2 = GetNewInvoice();
			trans2.IsManuallySetTransactionNumber_ForTestOnly = true;
			trans2.AH_ConsolidatedInvoiceRef = "S00001013";
			trans2.AH_OH = TestOrg.PK;

			ARInvoice trans3 = GetNewInvoice();
			trans3.IsManuallySetTransactionNumber_ForTestOnly = true;
			trans3.AH_TransactionNum = "99999999";
			trans3.AH_OH = TestOrg.PK;

			Factory.Save();

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.All;
			TestARFilterBizO.AH_Number = "1013";
			TestARFilterBizO.AH_OH = TestOrg.PK;
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Contains;
			TestCollection.Load(TestARFilterBizO.Filter);

			AssertEquals("Collection should contain two transactions", 2, TestCollection.Count);
			Assert("Collection should contain Trans1", TestCollection.Contains(trans1.PK));
			Assert("Collection should contain Trans2", TestCollection.Contains(trans2.PK));
		}

		#endregion TestNumberFiltering

		#region TestValidateTransactionType

		public new void TestValidateTransactionType()
		{
			Assert("Precondition: there should not be any errors for TransactionType", !TestARFilterBizO.AH_TransactionTypeInfo.HasErrors());
			TestARFilterBizO.AH_TransactionType = "#$%";
			Assert("Should not cause errors because no validation on TrackingFilterBizO", !TestARFilterBizO.AH_TransactionTypeInfo.HasErrors());
		}

		#endregion

		#region Test companies

		public void TestCompaniesList()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company.PK;
			companyData.OB_OH = Helper.TestOrg.PK;
			companyData.OB_IsDebtor = true;

			Factory.Save();

			var expectedCompanies = Helper.TestSiteUser.GetTransactionCompanies(Factory);
			AssertEquals("Precondition", 1, expectedCompanies.Count);

			var companies = TestARFilterBizO.Companies;

			AssertEquals(1, companies.Count);
			AssertEquals(expectedCompanies[0].PK, companies[0].PK);
		}

		public void TestFilterTransactonsByACompany()
		{
			GlbCompany anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch anotherCompanyBranch = anotherCompany.Branches.AddNew();

			Factory.Save();

			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARInvoice aRInv = (ARInvoice)creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1m);
			aRInv.AH_GB = GlbBranch.CurrentBranch.PK;
			aRInv.AH_OH = TestOrg.PK;

			ARCreditNote aRCrd = GetNewCreditNote();
			aRCrd.AH_GB = GlbBranch.CurrentBranch.PK;
			aRCrd.AH_OH = TestOrg.PK;

			ARInvoice aRInv2 = (ARInvoice)creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1m);
			aRInv2.AH_GB = anotherCompanyBranch.PK;
			aRInv2.AH_OH = TestOrg.PK;

			TestARFilterBizO.FilterByCompanyPK = ZGuid.Empty;
			TestCollection.Load(TestARFilterBizO.Filter);

			AssertEquals("There should be 3 matching transactions", 3, TestCollection.Count);
			Assert("Collection should contain ARInv", TestCollection.Contains(aRInv));
			Assert("Collection should contain ARCrd", TestCollection.Contains(aRCrd));
			Assert("Collection should contain ARInv2", TestCollection.Contains(aRInv2));

			TestARFilterBizO.FilterByCompanyPK = anotherCompany.PK;
			TestCollection.Load(TestARFilterBizO.Filter);
			AssertEquals("There should be 1 matching transaction", 1, TestCollection.Count);
			Assert("Collection should contain ARInv2", TestCollection.Contains(aRInv2));

			TestARFilterBizO.FilterByCompanyPK = GlbCompany.CurrentCompany.PK;
			TestCollection.Load(TestARFilterBizO.Filter);
			AssertEquals("There should be 2 matching transactions", 2, TestCollection.Count);
			Assert("Collection should contain ARInv", TestCollection.Contains(aRInv));
			Assert("Collection should contain ARCrd", TestCollection.Contains(aRCrd));
		}

		#endregion

		#region Statement

		public void TestGetStatementForSelectedCompany()
		{
			TestARFilterBizO.Company = ZGuid.Empty;
			AssertEquals("PDF result should be empty", 0, TestARFilterBizO.GetStatementForSelectedCompany().Length);

			TestARFilterBizO.Company = ZGuid.Invalid;
			AssertEquals("PDF result should be empty", 0, TestARFilterBizO.GetStatementForSelectedCompany().Length);

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company1.Branches.AddNew();
			branch1.GB_BranchName = "Test Branch";

			var creator = new TestObjectCreator(Factory);
			var header1 = creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);

			var contact1 = header1.Contacts.AddNew();
			contact1.OC_Email = "a@yahoo.com";
			contact1.SetHashedPassword("123");
			contact1.OC_WebAccessEnabled = true;

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company1.PK;
			companyData.OB_GB_ControllingBranch = branch1.PK;
			companyData.OB_OH = header1.PK;
			companyData.OB_IsDebtor = true;

			Factory.Save();

			Helper.TestSiteUser.Login(header1.OH_Code, contact1.OC_Email, contact1.PasswordForTesting);

			var inv1 = (Invoice)creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1m);
			inv1.AH_OH = header1.PK;
			creator.CreateInvoiceLine(inv1, creator.AUD, 1m, 100);
			creator.CreateInvoiceLine(inv1, creator.AUD, 1m, 200);

			var header2 = creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			var inv2 = (Invoice)creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1m);
			inv2.AH_OH = header2.PK;
			creator.CreateInvoiceLine(inv2, creator.AUD, 1m, 300);
			creator.CreateInvoiceLine(inv2, creator.AUD, 1m, 400);

			var inv3 = (Invoice)creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1m);
			inv3.AH_OH = header2.PK;
			creator.CreateInvoiceLine(inv3, creator.AUD, 1m, 500);
			creator.CreateInvoiceLine(inv3, creator.AUD, 1m, 600);

			Factory.Save();

			var newTestARFilterBizO = (TrackingTransactionFilterBusinessObject)GetNewBusinessObject();
			newTestARFilterBizO.Company = company1.PK;
			newTestARFilterBizO.LoggedInUser = contact1;
			Assert("PDF result should NOT be empty", newTestARFilterBizO.GetStatementForSelectedCompany().Length > 0);
		}

		public void TestGetStatementForSelectedCompany_ForCreditStatements()
		{
			TestARFilterBizO.Company = ZGuid.Empty;
			AssertEquals("PDF result should be empty", 0, TestARFilterBizO.GetStatementForSelectedCompany().Length);

			TestARFilterBizO.Company = ZGuid.Invalid;
			AssertEquals("PDF result should be empty", 0, TestARFilterBizO.GetStatementForSelectedCompany().Length);

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company1.Branches.AddNew();
			branch1.GB_BranchName = "Test Branch";

			var creator = new TestObjectCreator(Factory);
			var header1 = creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);

			var contact1 = header1.Contacts.AddNew();
			contact1.OC_Email = "a@yahoo.com";
			contact1.SetHashedPassword("123");
			contact1.OC_WebAccessEnabled = true;

			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company1.PK;
			companyData.OB_GB_ControllingBranch = branch1.PK;
			companyData.OB_OH = header1.PK;
			companyData.OB_IsDebtor = true;

			Factory.Save();

			Helper.TestSiteUser.Login(header1.OH_Code, contact1.OC_Email, contact1.PasswordForTesting);

			var crd1 = (ARCreditNote)creator.CreateInvoice(typeof(ARCreditNote), creator.AUD, 1m);
			crd1.AH_OH = header1.PK;
			creator.CreateInvoiceLine(crd1, creator.AUD, 1m, 100);
			creator.CreateInvoiceLine(crd1, creator.AUD, 1m, 200);

			var header2 = creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			var crd2 = (ARCreditNote)creator.CreateInvoice(typeof(ARCreditNote), creator.AUD, 1m);
			crd2.AH_OH = header2.PK;
			creator.CreateInvoiceLine(crd2, creator.AUD, 1m, 300);
			creator.CreateInvoiceLine(crd2, creator.AUD, 1m, 400);

			var crd3 = (ARCreditNote)creator.CreateInvoice(typeof(ARCreditNote), creator.AUD, 1m);
			crd3.AH_OH = header2.PK;
			creator.CreateInvoiceLine(crd3, creator.AUD, 1m, 500);
			creator.CreateInvoiceLine(crd3, creator.AUD, 1m, 600);

			Factory.Save();

			var newTestARFilterBizO = (TrackingTransactionFilterBusinessObject)GetNewBusinessObject();
			newTestARFilterBizO.Company = company1.PK;
			newTestARFilterBizO.LoggedInUser = contact1;
			Assert("PDF result should NOT be empty", newTestARFilterBizO.GetStatementForSelectedCompany().Length > 0);
		}

		#endregion

		#region TestFilterJobRelatedTransactionsByJobAndCurrentCompany

		public override void TestFilterbyCurrentCompany()
		{
			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = TestOrg.PK;
			aRInv.AH_ConsolidatedInvoiceRef = "S00024561";

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_OH = TestOrg.PK;
			aRCrd.AH_ConsolidatedInvoiceRef = "S00004562";

			ARInvoice aRInv2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv2.AH_OH = TestOrg.PK;
			aRInv2.AH_ConsolidatedInvoiceRef = "S00001556";
			aRInv2.AH_GB = new TestObjectCreator(Factory).NonCurrentCompanyBranch.PK; //	Make this transaction belong to another branch, 
																					  //	and have it come into the collection anyway - WEB SPECIFIC behaviour
			Factory.Save();

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestARFilterBizO.AH_Number = "S000";
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.JobNumber;
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.StartsWith;

			TrackingTransactionHeaderCollection transactions = new TrackingTransactionHeaderCollection(WebFactory);
			transactions.Load(TestARFilterBizO.Filter);

			AssertEquals("There should be 3 matching transactions", 3, transactions.Count);
			Assert("Collection should contain ARInv", transactions.Contains(aRInv));
			Assert("Collection should contain ARCrd", transactions.Contains(aRCrd));
			Assert("Collection should contain ARInv2", transactions.Contains(aRInv2));

			transactions.RemoveAll();
			AssertEquals("Collection should be cleared", 0, transactions.Count);

			TestARFilterBizO.LoggedInUser = null;
			Assert("AH_OH should be empty", TestARFilterBizO.AH_OH.IsEmpty);
			transactions.Load(TestARFilterBizO.Filter);
			AssertEquals("Should not load any records. LoggedInUser is null.", 0, transactions.Count);

			TestARFilterBizO.LoggedInUser = User;
			TestARFilterBizO.AH_OH = ZGuid.Empty;
			transactions.Load(TestARFilterBizO.Filter);
			AssertEquals("Should not load any records. AH_OH is not set.", 0, transactions.Count);
		}

		#endregion

		#region TestFilterBySettlementGroup

		public override void TestFilterBySettlementGroup()
		{
			Assert("not applicable", true);
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			GlbCompany company3 = Factory.NewWithValidTestData<GlbCompany>();

			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company1.PK;
			companyData.OB_OH = Helper.TestOrg.PK;
			companyData.OB_IsDebtor = true;

			OrgCompanyData companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_GC = company2.PK;
			companyData1.OB_OH = Helper.TestOrg.PK;
			companyData1.OB_IsDebtor = true;

			OrgCompanyData companyData2 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData2.OB_GC = company3.PK;
			companyData2.OB_OH = Helper.TestOrg.PK;
			companyData2.OB_IsDebtor = true;

			Factory.Save();

			DummyTrackingTransactionFilterBusinessObject testARFilterBizO = FilterBusinessObjectFactory.New<DummyTrackingTransactionFilterBusinessObject>();
			testARFilterBizO.LoggedInUser = User;
			testARFilterBizO.SetDefaultValuesForTest();

			AssertEquals("AH_OH was set to LoggedInUser.ParentOrg.PK", testARFilterBizO.LoggedInUser.ParentOrg.PK, testARFilterBizO.AH_OH);
			AssertEquals("PaymentStatus is Unpaid", AccountingUtils.PaymentStatusTypes.Unpaid, testARFilterBizO.PaymentStatus);
			AssertEquals("Company filter is assigned to the first company in the list", testARFilterBizO.Companies[0].PK, testARFilterBizO.Company);

			testARFilterBizO.LoggedInUser = null;
			AssertEquals("AH_OH is an Empty Guid", ZGuid.Empty, testARFilterBizO.AH_OH);
			testARFilterBizO.AH_OH = TestOrg.PK;
			AssertEquals("AH_OH was set to TestOrg.PK", TestOrg.PK, testARFilterBizO.AH_OH);

			testARFilterBizO.SetDefaultValuesForTest();
			AssertEquals("AH_OH is an Empty Guid", ZGuid.Empty, testARFilterBizO.AH_OH);
		}

		#endregion

		#region Implementation

		#region Overrides

		new TrackingTransactionFilterBusinessObject TestARFilterBizO
		{
			get { return base.TestARFilterBizO as TrackingTransactionFilterBusinessObject; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			TrackingTransactionFilterBusinessObject filterBizO = (TrackingTransactionFilterBusinessObject)base.GetNewBusinessObject();
			filterBizO.LoggedInUser = User;
			filterBizO.HasChanges = false;
			return filterBizO;
		}

		BusinessObjectFactory WebFactory
		{
			get { return Factory; }
		}

		#endregion Overrides

		ARInvoice GetNewInvoice()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = TestOrg.PK;
			return invoice;
		}

		ARCreditNote GetNewCreditNote()
		{
			ARCreditNote creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_OH = TestOrg.PK;
			return creditNote;
		}

		OrgContact User
		{
			get
			{
				if (fUser == null)
				{
					fUser = TestOrg.Contacts.AddNew();
					fUser.OC_Email = "test@testcompany.com.au";
					fUser.OC_ContactName = "Test Contact";
				}

				return fUser;
			}
		}
		OrgContact fUser;

		AccTransactionHeader GetNewTransaction()
		{
			AccTransactionHeader transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_OH = TestOrg.PK;
			transaction.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			return transaction;
		}

		TrackingTransactionHeaderCollection TestCollection
		{
			get
			{
				if (fTestCollection == null)
				{
					fTestCollection = new TrackingTransactionHeaderCollection(WebFactory);
				}
				return fTestCollection;
			}
		}
		TrackingTransactionHeaderCollection fTestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new TestHelper(Factory);
			Factory.Save();
			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			TestCaseHelper.ClearTable(AutoAccTransactionMatchLink.Schema.TableName);
			TestCaseHelper.ClearTable(AutoAccTransactionHeader.Schema.TableName);
		}
		TestHelper Helper;

		#endregion Implementation

		#region DummyTrackingTransactionFilterTest

		class DummyTrackingTransactionFilterBusinessObject : TrackingTransactionFilterBusinessObject
		{
			public DummyTrackingTransactionFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override Statement GetStatementForBranch(GlbBranch branch)
			{
				return null;
			}

			public GlbBranch GetBranchForTest(GlbCompany company)
			{
				return GetBranchForStatementPrinting(company);
			}

			public void SetDefaultValuesForTest()
			{
				base.SetDefaultValues();
			}
		}
		#endregion
	}
}
