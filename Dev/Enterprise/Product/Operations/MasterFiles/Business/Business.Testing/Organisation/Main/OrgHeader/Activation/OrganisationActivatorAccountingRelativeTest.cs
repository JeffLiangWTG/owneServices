using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.Testing.AccTransactionMatchLinkCriticalValidationTest;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrganisationActivatorAccountingRelativeTest : TestCaseWithFactory
	{
		public void TestDeactivateOrgWithActiveTransactions()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "999.888.88";
			glHeader.AG_AccountType = "BSH";

			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "TSTBNK";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_AG = glHeader.PK;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsActive = true;
			org1.OH_Code = "AAA";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsActive = true;
			org2.OH_Code = "BBB";

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = true;
			org3.OH_Code = "CCC";

			var unpaidARinvoice1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			unpaidARinvoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			unpaidARinvoice1.AH_TransactionType = TransactionTypes.Invoice;
			unpaidARinvoice1.AH_OH = org2.PK;
			unpaidARinvoice1.AH_InvoiceAmount = 10m;
			unpaidARinvoice1.AH_OutstandingAmount = 10m;

			var unpaidARinvoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			unpaidARinvoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			unpaidARinvoice2.AH_TransactionType = TransactionTypes.Invoice;
			unpaidARinvoice2.AH_OH = org3.PK;
			unpaidARinvoice2.AH_InvoiceAmount = 10m;
			unpaidARinvoice2.AH_OutstandingAmount = 10m;

			Factory.Save();

			Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			INotification notif = null;
			var notificationHeaderAction = new OrganisationActivator.ActivateOrDeactivateNotificationHeaderAction((x) =>
			{
				notif = x;
			});

			OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org1.PK, org2.PK, org3.PK }, false, null, null, notificationHeaderAction, null);
			var expectedMessageShown = @"Selected Organizations are De-activated except these Organizations because active transactions still exist for them: BBB, CCC.
For a list of system companies with active transactions, please De-activate each organization separately and the list will be provided." == notif.Message;

			expectedMessageShown = expectedMessageShown || @"Selected Organizations are De-activated except these Organizations because active transactions still exist for them: CCC, BBB.
For a list of system companies with active transactions, please De-activate each organization separately and the list will be provided." == notif.Message;

			Assert("Cannot control order of organization names so expect org code order in BBB,CCC or CCC,BBB", expectedMessageShown);

			ReloadOrgs(org1, org2, org3);
			Assert("Should be deactivated.", !org1.OH_IsActive);
			Assert("Should not be deactivated because it has an outstanding transaction.", org2.OH_IsActive);
			Assert("Should not be deactivated because it has an outstanding transaction.", org3.OH_IsActive);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org2.PK, org3.PK }, false, null, null, notificationHeaderAction, null);
			expectedMessageShown = @"These Organizations were not De-activated because active transactions still exist for them: BBB, CCC.
For a list of system companies with active transactions, please De-activate each organization separately and the list will be provided." == notif.Message;

			expectedMessageShown = expectedMessageShown || @"These Organizations were not De-activated because active transactions still exist for them: CCC, BBB.
For a list of system companies with active transactions, please De-activate each organization separately and the list will be provided." == notif.Message;

			Assert("Cannot control order of organization names so expect org code order in BBB,CCC or CCC,BBB", expectedMessageShown);

			ReloadOrgs(org2, org3);
			Assert("Should not be deactivated because it has an outstanding transaction.", org2.OH_IsActive);
			Assert("Should not be deactivated because it has an outstanding transaction.", org3.OH_IsActive);

			CreateReceiptAndPayARInvoice(unpaidARinvoice1, bankAccount.PK);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org2.PK, org3.PK }, false, null, null, notificationHeaderAction, null);
			AssertEquals(@"Selected Organizations are De-activated except organization CCC as there are active AR and/or AP transactions in the following system companies.
	- Company Code: EDI: Accounts Receivable", notif.Message);

			ReloadOrgs(org2, org3);
			Assert("Should be deactivated.", !org2.OH_IsActive);
			Assert("Should not be deactivated because it has an outstanding transaction.", org3.OH_IsActive);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org3.PK }, false, null, null, notificationHeaderAction, null);
			AssertEquals(@"You cannot De-activate this organization as there are active AR and/or AP transactions in the following system companies.
	- Company Code: EDI: Accounts Receivable", notif.Message);

			ReloadOrgs(org3);
			Assert("Should not be deactivated because it has an outstanding transaction.", org3.OH_IsActive);

			CreateReceiptAndPayARInvoice(unpaidARinvoice2, bankAccount.PK);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			OrganisationActivator.ActivateOrDeactivate(new ZGuid[] { org1.PK, org2.PK, org3.PK }, false, null, null, notificationHeaderAction, null);
			AssertEquals("Selected Organizations are De-activated.", notif.Message);

			ReloadOrgs(org3);
			Assert("Should be deactivated.", !org3.OH_IsActive);
		}

		void CreateReceiptAndPayARInvoice(AccTransactionHeader invoice, ZGuid bankAccountPK)
		{
			var receipt = Factory.NewWithValidTestData<AccTransactionHeader>();
			receipt.AH_Ledger = LedgerTypes.AccountsReceivable;
			receipt.AH_TransactionType = TransactionTypes.Receipt;
			receipt.AH_PostDate = ZDateTime.Today;
			receipt.AH_DueDate = ZDateTime.Today;
			receipt.AH_OH = invoice.AH_OH;
			receipt.AH_ReceiptType = ReceiptTypes.Cash;
			receipt.AH_AB = bankAccountPK;
			receipt.AH_ChequeOrReference = "CASH";
			receipt.AH_InvoiceAmount = -10m;
			receipt.AH_OutstandingAmount = 0m;
			receipt.AH_FullyPaidDate = ZDateTime.Today;

			var matchLink1 = Factory.New<AccTransactionMatchLink>();
			matchLink1.AP_AH = receipt.PK;
			matchLink1.AP_Amount = -10m;
			matchLink1.AP_MatchDate = ZDateTime.Today;
			matchLink1.AP_MatchGroupNum = "M001";

			invoice.AH_FullyPaidDate = ZDateTime.Today;
			invoice.AH_OutstandingAmount = 0m;
			invoice.AH_FullyPaidDate = ZDateTime.Today;

			var matchLink2 = Factory.New<AccTransactionMatchLink>();
			matchLink2.AP_AH = invoice.PK;
			matchLink2.AP_Amount = 10m;
			matchLink2.AP_MatchDate = ZDateTime.Today;
			matchLink2.AP_MatchGroupNum = "M001";

			var matchLinkGroup = new MatchLinkGroupForTest(Factory);
			matchLinkGroup.Add(matchLink1);
			matchLinkGroup.Add(matchLink2);
		}

		void ReloadOrgs(params OrgHeader[] orgs)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			int count = orgs.Length;
			for (int i = 0; i < count; i++)
			{
				orgs[i] = factory.Load<OrgHeader>(orgs[i].PK);
			}
		}
	}
}
