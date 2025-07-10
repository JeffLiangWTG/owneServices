using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	[TestedType(typeof(ProtestDocumentSupporter))]
	sealed class ProtestDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			var protest = new Protest(Factory.New<JobDeclaration>());
			var supporter = new ProtestDocumentSupporter(protest);
			AssertEquals("Protest declaration cannot be found.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericFreightJob), null));
			AssertEquals("Protest declaration cannot be found.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericFreightJobInvoice), null));
			AssertEquals("Protest declaration cannot be found.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.UsProtest), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			var protest = new Protest(Factory.New<JobDeclaration>());
			var supporter = new ProtestDocumentSupporter(protest);
			AssertEquals(true, supporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.GenericFreightJob, null));
			AssertEquals(true, supporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.GenericFreightJobInvoice, null));
			AssertEquals(true, supporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.UsProtest, null));
			AssertEquals(false, supporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.None, null));
		}

		public void TestProtestIsSupported()
		{
			var protest = new Protest(Factory.New<JobDeclaration>());
			var supporter = new ProtestDocumentSupporter(protest);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(".Protest")));

			var docDataProviders = supporter.GetBODocDataProviders(new DataContextValue(".Protest"), null);
			AssertEquals(1, docDataProviders.Length);
			//AssertEquals("Document Wrapper type", protest.GetType().Name, BODocDataProvider.GetBusinessObject(docDataProviders[0]).GetType().Name);
			AssertEquals(protest, BODocDataProvider.GetBusinessObject(docDataProviders[0]));
		}

		public void TestGetContactOrganisationInProtest()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			var contact = importer.Contacts.AddNew();
			contact.OC_Email = "protest@Doc.Support";

			var protest = new Protest(Factory.New<JobDeclaration>());
			protest.Protestant.OrganisationPK = importer.PK;

			var docSupporter = new ProtestDocumentSupporter(protest);
			AssertEquals(importer.PK, docSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY).OrgHeader.PK);

			protest.Protestant.E2_AddressOverride = true;
			AssertEquals(null, docSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY));
		}

		public void TestGetSupportedDataContexts()
		{
			var protest = new Protest(Factory.New<JobDeclaration>());
			protest.Declaration.JE_DeclarationReference = "B11010001";

			var job = new JobHeader.Loader(protest).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_JH = job.PK;
			transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_RX_NKTransactionCurrency = protest.Declaration.LocalCurrencyCode;
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

			var wrappers = ((IDocumentSupportable)protest).DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobInvoice, menu);
			AssertEquals("Only one Document wrapper should be found", 1, wrappers.Length);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => new Protest(Factory.New<JobDeclaration>());

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable businessObject)
		{
			base.DoSetupForDocument(command, businessObject);

			string menuName = command.SU_MenuName;

			switch (menuName)
			{
				case ProtestDocumentSupporter.ProtestMenuName:
					SetUpForProtest((Protest)businessObject);
					break;
			}
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand) => documentCommand.SU_MenuName.Contains("DocBuilder Invoice");

		void SetUpForProtest(Protest protest)
		{
			protest.LinkedEntries.AddNew();
			protest.LinkedEntries[0].US_LE_EntryNumber = "TEST";
		}
	}
}
