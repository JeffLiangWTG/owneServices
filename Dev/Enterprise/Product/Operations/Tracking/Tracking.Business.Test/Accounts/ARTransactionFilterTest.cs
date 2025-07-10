using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business
{
	[TestedType(typeof(ARTransactionFilterBusinessObject))]
	class ARTransactionFilterTest : FilterBusinessObjectTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AutoJobCharge.Schema.TableName);
			TestCaseHelper.ClearTable(AutoAccTransactionLines.Schema.TableName);
			TestCaseHelper.ClearTable(AutoAccTransactionMatchLink.Schema.TableName);
			TestCaseHelper.ClearTable(AutoAccTransactionHeader.Schema.TableName);
		}

		internal OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>();
				}
				return fTestOrg;
			}
		}
		OrgHeader fTestOrg;

		protected ARTransactionFilterBusinessObject TestARFilterBizO
		{
			get
			{
				if (fTestARFilterBizO == null)
				{
					fTestARFilterBizO = (ARTransactionFilterBusinessObject)GetNewBusinessObject();
				}
				return fTestARFilterBizO;
			}
		}
		ARTransactionFilterBusinessObject fTestARFilterBizO;

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion

		#region TestNumberFilterIgnoresOtherFields

		public void TestNumberFilterIgnoresOtherFields()
		{
			ARJournal testJournal = Factory.New<ARJournal>();
			testJournal.IsManuallySetTransactionNumber_ForTestOnly = true;
			testJournal.AH_TransactionNum = "00001001";
			Factory.Save();

			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.TransactionNumber;
			TestARFilterBizO.AH_Number = "00001001";
			TestARFilterBizO.AH_TransactionType = "CTR";
			TestARFilterBizO.AH_OH = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;

			ZQuery query = TestARFilterBizO.Filter;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, query);
			testTransactions.Load();
			AssertEquals("The TestJournal should be contained in the list", 1, testTransactions.Count);
			Assert("The TestJournal should be contained in the list", testTransactions.Contains(testJournal.PK));
		}

		#endregion

		#region TestDateFilterList

		public virtual void TestDateFilterList()
		{
			AssertEquals("Expected List to contain 5 elements", 5, TestARFilterBizO.AH_DateFilter_List.Count);
			Assert("Expected List to contain ALL option", TestARFilterBizO.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.All));
			Assert("Expected List to contain None option", TestARFilterBizO.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.None));
			Assert("Expected List to contain Post Date option", TestARFilterBizO.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.PostDate));
			Assert("Expected List to contain Transaction Date option", TestARFilterBizO.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.TransactionDate));
			Assert("Expected List to contain Due Date option", TestARFilterBizO.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.DueDate));
		}

		#endregion TestDateFilterList

		#region TestLedgerFiltering

		public void TestLedgerFiltering()
		{
			APJournal testAPJournal = Factory.NewWithValidTestData<APJournal>();
			ARJournal testARJournal = Factory.NewWithValidTestData<ARJournal>();
			Factory.Save();

			ZQuery query = TestARFilterBizO.Filter;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, query);
			testTransactions.Load();

			AssertEquals("There should only be the ARJournal in the collection", 1,
				testTransactions.Count);
			Assert("There should only be the ARJournal in the collection", testTransactions.Contains(testARJournal.PK));
		}

		#endregion

		#region TestNumberFiltering

		public void TestNumberFiltering()
		{
			Factory.Save();
			ARJournal testARJournalJobNum = Factory.New<ARJournal>();
			ARJournal testARJournalTransNum = Factory.New<ARJournal>();
			testARJournalTransNum.IsManuallySetTransactionNumber_ForTestOnly = true;
			testARJournalTransNum.AH_TransactionNum = "00001013";
			testARJournalJobNum.AH_ConsolidatedInvoiceRef = "S00001013";

			Factory.Save();

			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.All;
			TestARFilterBizO.AH_Number = "1013";
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Contains;
			ZQuery query = TestARFilterBizO.Filter;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, query);
			testTransactions.Load();
			AssertEquals("Collection should contain both TestJournals", 2, testTransactions.Count);
			Assert("Collection contains Trans Num Journal", testTransactions.Contains(testARJournalTransNum.PK));
			Assert("Collection contains Job Num Journal", testTransactions.Contains(testARJournalJobNum.PK));
		}

		#endregion

		#region TestCommonNumberFiltering

		public void TestCommonNumberFiltering()
		{
			ARInvoice testARInvoiceTransNum = Factory.NewWithValidTestData<ARInvoice>();
			testARInvoiceTransNum.AH_OH = TestOrg.PK;

			ARInvoice testARInvoiceJobNum = Factory.NewWithValidTestData<ARInvoice>();
			testARInvoiceJobNum.AH_OH = TestOrg.PK;

			testARInvoiceTransNum.IsManuallySetTransactionNumber_ForTestOnly = true;
			testARInvoiceTransNum.AH_TransactionNum = "00002544";
			testARInvoiceJobNum.AH_ConsolidatedInvoiceRef = "S0002544/A";
			Factory.Save();

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.Common;
			TestARFilterBizO.AH_Number = "2544";
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Contains;
			ZQuery query = TestARFilterBizO.Filter;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, query);
			testTransactions.Load();
			AssertEquals("Collection should contain both invoices", 2, testTransactions.Count);
			Assert("Collection contains Trans Num invoice", testTransactions.Contains(testARInvoiceTransNum.PK));
			Assert("Collection contains Job Num invoice", testTransactions.Contains(testARInvoiceJobNum.PK));
		}

		#endregion

		#region TestIsDisbursementFiltering

		public void TestIsDisbursementFiltering()
		{
			ARInvoice testARInvoiceDsb1 = Factory.NewWithValidTestData<ARInvoice>();
			testARInvoiceDsb1.AH_OH = TestOrg.PK;
			testARInvoiceDsb1.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;

			ARInvoice testARInvoiceDsb2 = Factory.NewWithValidTestData<ARInvoice>();
			testARInvoiceDsb2.AH_OH = TestOrg.PK;
			testARInvoiceDsb2.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;

			ARInvoice testARInvoiceNonDsb = Factory.NewWithValidTestData<ARInvoice>();
			testARInvoiceNonDsb.AH_OH = TestOrg.PK;
			testARInvoiceNonDsb.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

			Factory.Save();

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestARFilterBizO.AH_IsDisbursement = true;
			ZQuery query = TestARFilterBizO.Filter;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, query);
			testTransactions.Load();
			AssertEquals("Collection should contain both disbursement invoices", 2, testTransactions.Count);
			Assert("Collection contains first disbursement invoice", testTransactions.Contains(testARInvoiceDsb1.PK));
			Assert("Collection contains second disbursement invoice", testTransactions.Contains(testARInvoiceDsb2.PK));

			TestARFilterBizO.AH_IsDisbursement = false;
			query = TestARFilterBizO.Filter;
			testTransactions = new TransactionHeaderCollection(Factory, query);
			testTransactions.Load();
			AssertEquals("Collection should contain all invoices", 3, testTransactions.Count);
			Assert("Collection contains first disbursement invoice", testTransactions.Contains(testARInvoiceDsb1.PK));
			Assert("Collection contains second disbursement invoice", testTransactions.Contains(testARInvoiceDsb2.PK));
			Assert("Collection containsnon disbursement invoice", testTransactions.Contains(testARInvoiceNonDsb.PK));
		}

		#endregion

		#region TestDebtorGroupFiltering

		public void TestDebtorGroupFiltering()
		{
			OrgDebtorGroup testDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			OrgDebtorGroup testDebtorGroup2 = Factory.NewWithValidTestData<OrgDebtorGroup>();

			OrgHeader testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup.PK;

			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup2.PK;

			ARInvoice testARInv = Factory.NewWithValidTestData<ARInvoice>();
			testARInv.AH_OH = testOrg1.PK;

			ARInvoice testARInv2 = Factory.NewWithValidTestData<ARInvoice>();
			testARInv2.AH_OH = testOrg2.PK;

			Factory.Save();

			// create a CompanyData for another company
			GlbCompany company_New = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch_New = company_New.Branches.AddNew();
			GlbStaff staff_New = Factory.NewWithValidTestData<GlbStaff>();
			staff_New.GS_GB_HomeBranch = branch_New.PK;
			staff_New.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;
			staff_New.GS_LoginName = "newstaff";
			Factory.Save();

			using (Env.SetTemporaryUserContext("newstaff", branch_New.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				testOrg1 = newFactory.Load<OrgHeader>(testOrg1.PK);
				testOrg1.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup2.PK;

				ARInvoice testARInv3 = newFactory.NewWithValidTestData<ARInvoice>();
				testARInv3.AH_OH = testOrg1.PK;

				newFactory.Save();
			}

			TestARFilterBizO.DebtorCreditorGroup = testDebtorGroup.PK;

			ZQuery filter = TestARFilterBizO.CreditorDebtorGroupFilterPanelFilter;
			TransactionHeaderCollection headers = new TransactionHeaderCollection(Factory, filter);

			Factory.Save();
			headers.Load();

			AssertEquals("There should be 1 transaction from TestDebtorGroup", 1, headers.Count);
			Assert("That transaction should be TestARInv", headers.Contains(testARInv));

			TestARFilterBizO.DebtorCreditorGroup = testDebtorGroup2.PK;
			filter = TestARFilterBizO.CreditorDebtorGroupFilterPanelFilter;
			headers.Load(filter);

			AssertEquals("There should be 1 transaction from TestDebtorGroup2", 1, headers.Count);
			Assert("That transaction should be TestARInv2", headers.Contains(testARInv2));
		}

		#endregion

		#region TestValidateTransactionType

		public void TestValidateTransactionType()
		{
			Assert("Precondition: No errors for TransactionType", !TestARFilterBizO.AH_TransactionTypeInfo.HasErrors());
			TestARFilterBizO.AH_TransactionType = "&*(";
			Assert("Invalid TransactionType - should cause errors", TestARFilterBizO.AH_TransactionTypeInfo.HasErrors());
			TestARFilterBizO.AH_TransactionType = TransactionTypes.Contra;
			Assert("Valid TransactionType - should not cause errors", !TestARFilterBizO.AH_TransactionTypeInfo.HasErrors());
		}

		#endregion

		#region TestValidatePaymentStatus

		public void TestValidatePaymentStatus()
		{
			Assert("Precondition: No errors for PaymentStatus", !TestARFilterBizO.PaymentStatusInfo.HasErrors());
			TestARFilterBizO.PaymentStatus = "#^^";
			Assert("Invalid PaymentStatus - should cause errors", TestARFilterBizO.PaymentStatusInfo.HasErrors());
			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.Unpaid;
			Assert("Valid PaymentStatus - should not cause errors", !TestARFilterBizO.PaymentStatusInfo.HasErrors());
		}

		#endregion

		#region TestTransactionNumberFilteringUsingContainsAndStartsWith

		public virtual void TestTransactionNumberFilteringUsingContainsAndStartsWith()
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = TestOrg.PK;

			ARInvoice aRInv2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv2.AH_OH = TestOrg.PK;

			ARJournal aRJnl = Factory.NewWithValidTestData<ARJournal>();
			aRJnl.AH_OH = TestOrg.PK;

			aRInv.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRInv2.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRJnl.IsManuallySetTransactionNumber_ForTestOnly = true;

			aRInv.AH_TransactionNum = "00035262";
			aRInv2.AH_TransactionNum = "00001626";
			aRJnl.AH_TransactionNum = "00001583";

			Factory.Save();

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestARFilterBizO.AH_Number = "00001";
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.TransactionNumber;
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.StartsWith;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 2 transactions in the collection", 2, transactions.Count);
			Assert("Collection should contain ARInv2", transactions.Contains(aRInv2));
			Assert("Collection should contain ARJnl", transactions.Contains(aRJnl));

			TestARFilterBizO.AH_Number = "26";
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Contains;

			transactions.Load(TestARFilterBizO.Filter);
			AssertEquals("There should be 2 transactions in the collection", 2, transactions.Count);
			Assert("Collection should contain ARInv", transactions.Contains(aRInv));
			Assert("Collection should contain ARIn2", transactions.Contains(aRInv2));
		}

		#endregion

		#region TestJobNumberFilteringUsingContainsAndStartsWith

		public virtual void TestJobNumberFilteringUsingContainsAndStartsWith()
		{
			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_ConsolidatedInvoiceRef = "S00024561";
			aRInv.AH_OH = TestOrg.PK;

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_ConsolidatedInvoiceRef = "S00004562";
			aRCrd.AH_OH = TestOrg.PK;

			ARInvoice aRInv2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv2.AH_ConsolidatedInvoiceRef = "S00001556";
			aRInv2.AH_OH = TestOrg.PK;

			Factory.Save();

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestARFilterBizO.AH_Number = "S0000";
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.JobNumber;
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.StartsWith;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
			Assert("Collection should contain ARCrd", transactions.Contains(aRCrd));
			Assert("Collection should contain ARInv2", transactions.Contains(aRInv2));

			TestARFilterBizO.AH_Number = "155";
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Contains;

			transactions.Load(TestARFilterBizO.Filter);
			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert("Collection should contain ARInv2", transactions.Contains(aRInv2));
		}

		#endregion

		#region DepositBatchNumberFilter Test

		public void TestDepositBatchNumberFilter()
		{
			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_ReceiptBatchNo = "00003495";
			aRRec.AH_OH = TestOrg.PK;
			aRRec.AH_ReceiptType = ReceiptTypes.DirectCredit;

			ARJournal aRJnl = Factory.NewWithValidTestData<ARJournal>();
			aRJnl.AH_ReceiptBatchNo = "00003495";
			aRJnl.AH_OH = TestOrg.PK;

			ARReceipt aRRec2 = Factory.NewWithValidTestData<ARReceipt>();
			aRRec2.AH_ReceiptType = ReceiptTypes.DirectCredit;
			aRRec2.AH_ReceiptBatchNo = "00001498";
			aRRec2.AH_OH = TestOrg.PK;

			TestARFilterBizO.AH_OH = TestOrg.PK;
			TestARFilterBizO.AH_Number = "00003495";
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.DepositBatchNumber;
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Equal;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 ARReceipt in the collection", 1, transactions.Count);
			AssertEquals("The ARReceipt should be ARRec", aRRec.PK, transactions[0].PK);

			TestARFilterBizO.AH_Number = "49";
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Contains;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 2 ARReceipts in the collection", 2, transactions.Count);
			Assert("ARRec2 should be in the collection", transactions.Contains(aRRec2));
			Assert("ARRec should be in the collection", transactions.Contains(aRRec));
		}

		#endregion

		#region DDRBatchNumberFilter Test

		public void TestDDRBatchNumberFilter()
		{
			ARPayment aRPay = Factory.NewWithValidTestData<ARPayment>();
			aRPay.AH_ReceiptType = ReceiptTypes.DirectDebit;
			aRPay.AH_ReceiptBatchNo = "00005839";
			aRPay.AH_OH = TestOrg.PK;

			ARPayment aRPay2 = Factory.NewWithValidTestData<ARPayment>();
			aRPay2.AH_ReceiptType = ReceiptTypes.DirectDebit;
			aRPay2.AH_ReceiptBatchNo = "00004582";
			aRPay2.AH_OH = TestOrg.PK;

			ARJournal aRJnl = Factory.NewWithValidTestData<ARJournal>();
			aRJnl.AH_ReceiptBatchNo = "00005839";
			aRJnl.AH_OH = TestOrg.PK;

			TestARFilterBizO.AH_OH = TestOrg.PK;
			TestARFilterBizO.AH_Number = "00005839";
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.DDRBatchNumber;
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Equal;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 AR Payment in the collection", 1, transactions.Count);
			Assert("ARPay should be in the collection", transactions.Contains(aRPay));

			TestARFilterBizO.AH_Number = "45";
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Contains;

			transactions.Load(TestARFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
			Assert("ARPay2 should be in the collection", transactions.Contains(aRPay2));
		}

		#endregion

		#region TestChequeNumberFilteringUsingContainsAndStartsWith

		public virtual void TestChequeNumberFilteringUsingContainsAndStartsWith()
		{
			ARPayment aRPay = Factory.NewWithValidTestData<ARPayment>();
			aRPay.AH_OH = TestOrg.PK;
			aRPay.AH_ChequeOrReference = "26798";

			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_OH = TestOrg.PK;
			aRRec.AH_ChequeOrReference = "2450";

			TestARFilterBizO.AH_Number = "26";
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.ChequeReferenceNumber;
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.StartsWith;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert("Collection should contain ARPay", transactions.Contains(aRPay));
		}

		#endregion

		#region TestJobInvNumberFilteringUsingContainsAndStartsWith

		public virtual void TestJobInvNumberFilteringUsingContainsAndStartsWith()
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_ConsolidatedInvoiceRef = "S00001032/B";
			aRInv.AH_OH = TestOrg.PK;

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_ConsolidatedInvoiceRef = "S00001220/A";
			aRCrd.AH_OH = TestOrg.PK;

			ARAdjustmentNote aRAdj = Factory.NewWithValidTestData<ARAdjustmentNote>();
			aRAdj.AH_ConsolidatedInvoiceRef = "S00002204/A";
			aRAdj.AH_OH = TestOrg.PK;

			Factory.Save();

			TestARFilterBizO.FilterOperator = SQLComparisonOperator.StartsWith;
			TestARFilterBizO.AH_Number = "S00001";
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.ConsolidationNumber;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
			Assert("ARInv should match", transactions.Contains(aRInv));
			Assert("ARCrd should match", transactions.Contains(aRCrd));

			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Contains;
			TestARFilterBizO.AH_Number = "22";

			transactions.Load(TestARFilterBizO.Filter);

			AssertEquals("There should be 2 matching transacitons", 2, transactions.Count);
			Assert("ARCrd should match", transactions.Contains(aRCrd));
			Assert("ARAdj should match", transactions.Contains(aRAdj));
		}

		#endregion

		#region TestComparisonOperator

		public void TestComparisonOperator()
		{
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.StartsWith;
			AssertEquals("Comparison operator shoudl be startswith", SQLComparisonOperator.StartsWith, TestARFilterBizO.FilterOperator);
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.Contains;
			AssertEquals("Comparison operator should be Contains", SQLComparisonOperator.Contains, TestARFilterBizO.FilterOperator);
		}

		#endregion

		#region TestDefaultValues

		public void TestDefaultValues()
		{
			AssertEquals("Equal should be selected", SQLComparisonOperator.Equal, TestARFilterBizO.FilterOperator);
		}

		#endregion

		#region TestInactiveOrganisationsIncludedInList

		public void TestInactiveOrganisationsIncludedInList()
		{
			OrgHeader inactiveDebtor = Factory.NewWithValidTestData<OrgHeader>();
			inactiveDebtor.OH_IsActive = false;
			inactiveDebtor.OH_IsDebtor = true;
			Factory.Save();

			OrgHeader activeDebtor = Factory.NewWithValidTestData<OrgHeader>();
			activeDebtor.OH_IsActive = true;
			activeDebtor.OH_IsDebtor = true;
			Factory.Save();

			TestARFilterBizO.AH_OHList.Load();
			Assert("Org list should contain the Inactive Org", TestARFilterBizO.AH_OHList.Contains(inactiveDebtor));
			Assert("Org list should contain the Active Org", TestARFilterBizO.AH_OHList.Contains(activeDebtor));
		}

		#endregion

		#region TestFilterbyCurrentCompany

		public virtual void TestFilterbyCurrentCompany()
		{
			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_ConsolidatedInvoiceRef = "S00024561";
			aRInv.AH_OH = TestOrg.PK;

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_ConsolidatedInvoiceRef = "S00004562";
			aRCrd.AH_OH = TestOrg.PK;

			ARInvoice aRInv2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv2.AH_OH = TestOrg.PK;
			aRInv2.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			aRInv2.AH_ConsolidatedInvoiceRef = "S00006543";

			Factory.Save();

			TestARFilterBizO.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			TestARFilterBizO.AH_Number = "S000";
			TestARFilterBizO.AH_NumberFilter = AccountingUtils.NumberFilterTypes.JobNumber;
			TestARFilterBizO.FilterOperator = SQLComparisonOperator.StartsWith;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
			Assert("Collection should contain ARInv", transactions.Contains(aRInv));
			Assert("Collection should contain ARCrd", transactions.Contains(aRCrd));
		}

		#endregion

		#region TestFilterBySettlementGroup

		public virtual void TestFilterBySettlementGroup()
		{
			Factory.Save();

			OrgHeader newHeaderForGrouping = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeaderInGroup = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderInGroup.ARSettlementGroupPK = newHeaderForGrouping.PK;
			OrgHeader orgHeaderInGroup2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderInGroup2.ARSettlementGroupPK = newHeaderForGrouping.PK;

			OrgHeader orgHeaderOurOfGroup = Factory.NewWithValidTestData<OrgHeader>();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = orgHeaderInGroup.PK;

			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_OH = orgHeaderInGroup2.PK;

			ARInvoice aRInv2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv2.AH_OH = orgHeaderOurOfGroup.PK;

			Factory.Save();

			TestARFilterBizO.SettlementGroup = ZGuid.Empty;
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 3 transactions in the collection", 3, transactions.Count);

			TestARFilterBizO.SettlementGroup = newHeaderForGrouping.PK;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 2 transactions in the collection", 2, transactions.Count);
			Assert("Collection should contain ARInv", transactions.Contains(aRInv));
			Assert("Collection should contain ARCrd", transactions.Contains(aRCrd));

			TestARFilterBizO.SettlementGroup = orgHeaderInGroup.PK;
			transactions = new TransactionHeaderCollection(Factory, TestARFilterBizO.Filter);
			transactions.Load();
			AssertEquals("The collection should be empty", 0, transactions.Count);
		}

		#endregion

		#region TestOrgSettlementGroup_List

		public void TestOrgSettlementGroup_List()
		{
			Assert("OrgSettlementGroup_List should be of type OrganisationsFindBoxCollection", TestARFilterBizO.OrgSettlementGroup_List is OrganisationsFindBoxCollection);
		}

		#endregion

		#region TestTransactionAmountFiltering

		public void TestTransactionAmountFiltering()
		{
			Factory.Save();
			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, -998m, 0m, 0m);
			testAPInv.AH_OSTotal = testAPInv.AH_InvoiceAmount = testAPInv.AH_OutstandingAmount = 998m;
			testAPInv.AH_OH = TestOrg.PK;
			ARInvoice testARInv = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(testARInv, testARInv.TransactionCurrency, testARInv.AH_ExchangeRate, 998m, 0m, 0m);
			testARInv.AH_OSTotal = testARInv.AH_InvoiceAmount = testARInv.AH_OutstandingAmount = 998m;
			testARInv.AH_OH = TestOrg.PK;
			ARCreditNote testARCrd = Factory.NewWithValidTestData<ARCreditNote>();
			TestObjectCreator.CreateInvoiceLine(testARCrd, testARCrd.TransactionCurrency, testARCrd.AH_ExchangeRate, 600m, 0m, 0m);
			testARCrd.AH_OSTotal = testARCrd.AH_InvoiceAmount = testARCrd.AH_OutstandingAmount = -600m;
			testARCrd.AH_OH = TestOrg.PK;
			ARReceipt testARRec = Factory.NewWithValidTestData<ARReceipt>();
			testARRec.AH_OSTotal = testARRec.AH_InvoiceAmount = testARRec.AH_OutstandingAmount = -500m;
			testARRec.AH_OH = TestOrg.PK;
			ARInvoice testARInv2 = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(testARInv2, testARInv2.TransactionCurrency, testARInv2.AH_ExchangeRate, 2500m, 0m, 0m);
			testARInv2.AH_OSTotal = testARInv2.AH_InvoiceAmount = testARInv2.AH_OutstandingAmount = 2500m;
			testARInv2.AH_OH = TestOrg.PK;
			ARJournal testARJrn = Factory.NewWithValidTestData<ARJournal>();
			testARJrn.AH_OSTotal = testARJrn.AH_InvoiceAmount = testARJrn.AH_OutstandingAmount = -250m;
			testARJrn.AH_OH = TestOrg.PK;

			Factory.Save();

			TestARFilterBizO.TransactionFromAmount = 0m;
			TestARFilterBizO.TransactionToAmount = 1000m;
			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory);
			testTransactions.Load(TestARFilterBizO.Filter);
			AssertEquals("Collection should contain only TestARInv", 1, testTransactions.Count);
			Assert("TestARInv should be in collection", testTransactions.Contains(testARInv));

			TestARFilterBizO.TransactionFromAmount = -1000m;
			TestARFilterBizO.TransactionToAmount = 0m;
			testTransactions.Load(TestARFilterBizO.Filter);
			AssertEquals("Collection should contain 3 transactions", 3, testTransactions.Count);
			Assert("TestARCrd should be in collection", testTransactions.Contains(testARCrd));
			Assert("TestARRec should be in collection", testTransactions.Contains(testARRec));
			Assert("TestARJrn should be in collection", testTransactions.Contains(testARJrn));

			TestARFilterBizO.TransactionFromAmount = -500m;
			TestARFilterBizO.TransactionToAmount = 1000m;
			testTransactions.Load(TestARFilterBizO.Filter);
			AssertEquals("Collection should contain 3 transactions", 3, testTransactions.Count);
			Assert("TestARInv should be in collection", testTransactions.Contains(testARInv));
			Assert("TestARRec should be in collection", testTransactions.Contains(testARRec));
			Assert("TestARJrn should be in collection", testTransactions.Contains(testARJrn));

			TestARFilterBizO.TransactionFromAmount = 0m;
			TestARFilterBizO.TransactionToAmount = 0m;
			testTransactions.Load(TestARFilterBizO.Filter);
			AssertEquals("Collection should contain all 5 transactions", 5, testTransactions.Count);
		}

		#endregion

		#region TestTransactionAmountFromValidation

		public void TestTransactionAmountFromValidation()
		{
			TestARFilterBizO.TransactionFromAmount = 0m;
			AssertNoErrors("Should not contain any errors", TestARFilterBizO.TransactionFromAmountInfo);
			TestARFilterBizO.TransactionFromAmount = -10000m;
			AssertNoErrors("Should not contain any errors", TestARFilterBizO.TransactionFromAmountInfo);
			TestARFilterBizO.TransactionFromAmount = 10000m;
			AssertNoErrors("Should not contain any errors", TestARFilterBizO.TransactionFromAmountInfo);
			TestARFilterBizO.TransactionFromAmount = 9999999999999999999999999999m;
			AssertHasErrors("Should contain error as the amount is greater than bounds of Money type", TestARFilterBizO.TransactionFromAmountInfo);
		}

		#endregion

		#region TestTransactionAmountToValidation

		public void TestTransactionAmountToValidation()
		{
			TestARFilterBizO.TransactionToAmount = 0m;
			AssertNoErrors("Should not contain any errors", TestARFilterBizO.TransactionToAmountInfo);
			TestARFilterBizO.TransactionToAmount = -10000m;
			AssertNoErrors("Should not contain any errors", TestARFilterBizO.TransactionToAmountInfo);
			TestARFilterBizO.TransactionToAmount = 10000m;
			AssertNoErrors("Should not contain any errors", TestARFilterBizO.TransactionToAmountInfo);
			TestARFilterBizO.TransactionToAmount = 9999999999999999999999999999m;
			AssertHasErrors("Should contain error as the amount is greater than bounds of Money type", TestARFilterBizO.TransactionToAmountInfo);
		}

		#endregion
	}
}
