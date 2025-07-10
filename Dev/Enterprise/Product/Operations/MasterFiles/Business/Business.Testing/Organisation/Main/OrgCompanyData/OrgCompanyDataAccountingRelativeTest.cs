using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.Testing.AccTransactionMatchLinkCriticalValidationTest;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCompanyDataAccountingRelativeTest : TestCaseWithFactory
	{
		public void TestARTransactionHeader()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "999.888.88";
			glHeader.AG_AccountType = "BSH";

			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "TSTBNK";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_AG = glHeader.PK;

			var aROrg = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var aRInv1 = GetNewTransactionHeader(aROrg, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 90m, GlbBranch.CurrentBranch);
			aRInv1.AH_InvoiceAmount = 90m;
			Factory.Save();

			Assert("Has AR Transaction", aROrg.CompanyData.HasARTransaction);
			Assert("Has No AR Transaction", !otherOrg.CompanyData.HasARTransaction);

			CreateReceiptAndPayInvoice(aRInv1, bankAccount.PK);
			Factory.Save();
			Assert("Has No AR Transaction", !aROrg.CompanyData.HasARTransaction);
			Assert("Has No AR Transaction", !otherOrg.CompanyData.HasARTransaction);

			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var aRInv2 = GetNewTransactionHeader(aROrg, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 90m, newBranch);
			aRInv2.AH_InvoiceAmount = 90m;
			Factory.Save();
			Assert("Has No AR Transaction", !aROrg.CompanyData.HasARTransaction);
			Assert("Has No AR Transaction", !otherOrg.CompanyData.HasARTransaction);

			CreateReceiptAndPayInvoice(aRInv2, bankAccount.PK);
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			var aRInv3 = GetNewTransactionHeader(aROrg, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 90m, newBranch);
			aRInv3.AH_InvoiceAmount = 90m;
			Factory.Save();
			Assert("Has AR Transaction", aROrg.CompanyData.HasARTransaction);
			Assert("Has No AR Transaction", !otherOrg.CompanyData.HasARTransaction);

			CreateReceiptAndPayInvoice(aRInv3, bankAccount.PK);
			var aRInv4 = GetNewTransactionHeader(otherOrg, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 90m, newBranch);
			aRInv4.AH_InvoiceAmount = 90m;
			Factory.Save();
			Assert("Has No AR Transaction", !aROrg.CompanyData.HasARTransaction);
			Assert("Has AR Transaction", otherOrg.CompanyData.HasARTransaction);

			var aRInv5 = GetNewTransactionHeader(aROrg, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 90m, GlbBranch.CurrentBranch);
			aRInv5.AH_InvoiceAmount = 90m;
			Factory.Save();
			Assert("Has AR Transaction", aROrg.CompanyData.HasARTransaction);
			Assert("Has AR Transaction", otherOrg.CompanyData.HasARTransaction);
		}

		public void TestAPTransactionHeader()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "999.888.88";
			glHeader.AG_AccountType = "BSH";

			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "TSTBNK";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_AG = glHeader.PK;

			var aPOrg = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var aPInv1 = GetNewTransactionHeader(aPOrg, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 90m, GlbBranch.CurrentBranch);
			aPInv1.AH_InvoiceAmount = 90m;
			Factory.Save();

			Assert("Has AP Transaction", aPOrg.CompanyData.HasAPTransaction);
			Assert("Has No AP Transaction", !otherOrg.CompanyData.HasAPTransaction);

			CreateReceiptAndPayInvoice(aPInv1, bankAccount.PK);
			Factory.Save();
			Assert("Has No AP Transaction", !aPOrg.CompanyData.HasAPTransaction);
			Assert("Has No AP Transaction", !otherOrg.CompanyData.HasAPTransaction);

			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var aPInv2 = GetNewTransactionHeader(aPOrg, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 90m, newBranch);
			aPInv2.AH_InvoiceAmount = 90m;
			Factory.Save();
			Assert("Has No AP Transaction", !aPOrg.CompanyData.HasAPTransaction);
			Assert("Has No AP Transaction", !otherOrg.CompanyData.HasAPTransaction);

			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			var aPInv3 = GetNewTransactionHeader(aPOrg, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 90m, newBranch);
			aPInv3.AH_InvoiceAmount = 90m;
			Factory.Save();
			Assert("Has AP Transaction", aPOrg.CompanyData.HasAPTransaction);
			Assert("Has No AP Transaction", !otherOrg.CompanyData.HasAPTransaction);

			CreateReceiptAndPayInvoice(aPInv3, bankAccount.PK);
			var aPInv4 = GetNewTransactionHeader(otherOrg, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 90m, newBranch);
			aPInv4.AH_InvoiceAmount = 90m;
			Factory.Save();
			Assert("Has No AP Transaction", !aPOrg.CompanyData.HasAPTransaction);
			Assert("Has AP Transaction", otherOrg.CompanyData.HasAPTransaction);

			var aPInv5 = GetNewTransactionHeader(aPOrg, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 90m, GlbBranch.CurrentBranch);
			aPInv5.AH_InvoiceAmount = 90m;
			Factory.Save();
			Assert("Has AP Transaction", aPOrg.CompanyData.HasAPTransaction);
			Assert("Has AP Transaction", otherOrg.CompanyData.HasAPTransaction);
		}

		void CreateReceiptAndPayInvoice(AccTransactionHeader invoice, ZGuid bankAccountPK)
		{
			var isARInvoice = invoice.AH_Ledger == LedgerTypes.AccountsReceivable;
			var receipt = Factory.NewWithValidTestData<AccTransactionHeader>();
			receipt.AH_Ledger = isARInvoice ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			receipt.AH_TransactionType = TransactionTypes.Receipt;
			receipt.AH_PostDate = ZDateTime.Today;
			receipt.AH_DueDate = ZDateTime.Today;
			receipt.AH_OH = invoice.AH_OH;
			receipt.AH_ReceiptType = ReceiptTypes.Cash;
			receipt.AH_AB = bankAccountPK;
			receipt.AH_ChequeOrReference = "CASH";
			receipt.AH_InvoiceAmount = -invoice.AH_InvoiceAmount;
			receipt.AH_OutstandingAmount = 0m;
			receipt.AH_FullyPaidDate = ZDateTime.Today;

			var matchLink1 = Factory.New<AccTransactionMatchLink>();
			matchLink1.AP_AH = receipt.PK;
			matchLink1.AP_Amount = -invoice.AH_InvoiceAmount;
			matchLink1.AP_MatchDate = ZDateTime.Today;
			matchLink1.AP_MatchGroupNum = "M001";

			invoice.AH_FullyPaidDate = ZDateTime.Today;
			invoice.AH_OutstandingAmount = 0m;

			var matchLink2 = Factory.New<AccTransactionMatchLink>();
			matchLink2.AP_AH = invoice.PK;
			matchLink2.AP_Amount = invoice.AH_InvoiceAmount;
			matchLink2.AP_MatchDate = ZDateTime.Today;
			matchLink2.AP_MatchGroupNum = "M001";

			var matchLinkGroup = new MatchLinkGroupForTest(Factory);
			matchLinkGroup.Add(matchLink1);
			matchLinkGroup.Add(matchLink2);
		}

		public void TestOrgCompanyData_APExcludeFromPaymentReports()
		{
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			Assert("Default value for OB_APExcludeFromPaymentReports is false", !orgCompanyData.OB_APExcludeFromPaymentReports);

			orgCompanyData.OB_APExcludeFromPaymentReports = true;
			Assert("OB_APExcludeFromPaymentReports set to true", orgCompanyData.OB_APExcludeFromPaymentReports);
		}

		#region Implementation

		AccTransactionHeader GetNewTransactionHeader(OrgHeader org, string ledger, string transactionType, decimal outstandingAmount, GlbBranch branch)
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_OH = org.PK;
			invoice.AH_Ledger = ledger;
			invoice.AH_OutstandingAmount = outstandingAmount;
			invoice.AH_IsCancelled = false;
			invoice.AH_GB = branch.PK;
			invoice.AH_TransactionType = transactionType;
			return invoice;
		}

		#endregion

	}
}
