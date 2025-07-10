using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationDocumentSupporterHelperTest : TestCaseWithFactory
	{
		public void TestGetWrappersForInvoice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B11010001";

			var job = new JobHeader.Loader(declaration).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_JH = job.PK;
			transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_RX_NKTransactionCurrency = declaration.LocalCurrencyCode;
			transactionHeader.AH_TransactionNum = "00001001";
			transactionHeader.AH_TransactionReference = "00001001";
			transactionHeader.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transactionHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			transactionHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			transactionHeader.AH_ConsolidatedInvoiceRef = "B11010001";

			Factory.Save();
			job.JH_OA_LocalChargesAddr = transactionHeader.Header.MainAddress.PK;

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobInvoicingEDocsProviderSupporter.DocBuilderInvoiceName;

			var wrappers = ((IDocumentSupportable)declaration).DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobInvoice, menu);
			AssertEquals("Only one Document wrapper should be found", 1, wrappers.Length);
		}
	}
}
