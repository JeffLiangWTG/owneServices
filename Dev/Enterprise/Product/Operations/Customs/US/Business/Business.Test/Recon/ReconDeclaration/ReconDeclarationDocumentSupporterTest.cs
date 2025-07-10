using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconDeclarationDocumentSupporter))]
	sealed class ReconDeclarationDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconDeclarationDocumentSupporter docSupporter = new ReconDeclarationDocumentSupporter(reconDec);
			AssertEquals("Recon job cannot be found.", docSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericFreightJob), null));
			AssertEquals("Recon job cannot be found.", docSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericFreightJobInvoice), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconDeclarationDocumentSupporter docSupporter = new ReconDeclarationDocumentSupporter(reconDec);
			AssertEquals(true, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.GenericFreightJob, null));
			AssertEquals(true, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.GenericFreightJobInvoice, null));
			AssertEquals(false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		public void TestGetSupportedDataSources()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconDeclarationDocumentSupporter docSupporter = new ReconDeclarationDocumentSupporter(reconDec);
			AssertEquals(".ReconDeclaration is supported", true, docSupporter.IsDataContextSupported(new DataContextValue(".ReconDeclaration")));
		}

		public void TestGetContactOrganisationInReconciliation()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			var contact = importer.Contacts.AddNew();
			contact.OC_Email = "Recon.Declaration@Doc.Support";
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.OriginalEntries.AddNew();
			var docSupporter = new ReconDeclarationDocumentSupporter(reconDec);
			var reconDeclaration = Factory.New<CusStatementHeader>();
			reconDec.JE_OH_Importer = importer.PK;
			AssertEquals(importer.PK, docSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY).OrgHeader.PK);
		}

		public void TestGetSupportedDataContexts()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var docSupporter = new ReconDeclarationDocumentSupporter(reconDeclaration);
			AssertEquals("Data context supported", "GenericFreightJob, .ReconDeclaration, .Business.ReconDeclaration, .US.Business.ReconDeclaration, .ReconDeclaration", docSupporter.CommaSeparatedListOfSupportedDataContexts);
			var wrappers = ((IDocumentSupportable)reconDeclaration).DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, null);
			AssertEquals("Only one Document wrapper should be found", 1, wrappers.Length);
			AssertEquals("Document wrapper for data context of GenericFreightJob is of type FreightWrapperFromReconDeclaration", "FreightWrapperFromReconDeclaration", wrappers[0].GetType().Name);
			reconDeclaration.ReconWrappedJobDeclaration.JE_DeclarationReference = "B11010001";
			var job = new JobHeader.Loader(reconDeclaration).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_JH = job.PK;
			transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_RX_NKTransactionCurrency = reconDeclaration.ReconWrappedJobDeclaration.LocalCurrencyCode;
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
			wrappers = ((IDocumentSupportable)reconDeclaration).DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobInvoice, menu);
			AssertEquals("Only one Document wrapper should be found", 1, wrappers.Length);
		}

		public void TestGetBODocDataProviders()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.OriginalEntries.AddNew();
			ReconDeclarationDocumentSupporter docSupporter = new ReconDeclarationDocumentSupporter(reconDec);
			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = ReconDeclarationDocumentSupporter.AggregateReconMenuName;
			reconDec.US_IsAggregate = true;
			AssertEquals("BODocData", BODocDataProvider.Get(reconDec), docSupporter.GetBODocDataProviders(new DataContextValue(".ReconDeclaration"), menu)[0]);
			AssertEquals("one aggregated entry is created", 1, reconDec.AggregatedEntries.Count);
			menu.SU_MenuName = ReconDeclarationDocumentSupporter.LineSummaryMenuItem;
			SetUpForLineSummaryRecon(reconDec);
			AssertEquals("BODocData", BODocDataProvider.Get(reconDec), docSupporter.GetBODocDataProviders(new DataContextValue(".ReconDeclaration"), menu)[0]);
			AssertEquals("InvoiceLines are merged into changed lines", 1, reconDec.ChangedLines.Count);
			AssertEquals(ZString.Empty, reconDec.ChangedLines[0].US_OrigDutyRateDesc);
		}

		public void TestGetDataStateBeforeRun()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var docSupporter = new ReconDeclarationDocumentSupporter(reconDec);
			var menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, ReconDeclarationDocumentSupporter.AggregateReconMenuName));
			AssertNotNull(menu);
			reconDec.US_IsAggregate = false;
			var dataState = docSupporter.GetDataStateBeforeRun(menu);
			AssertEquals("trying to run a document for aggregate from a non-aggregate job", false, dataState.IsValid);
			AssertEquals(dataState.ErrorMessage, ReconDeclarationDocumentSupporter.IsNotAggregate);
			reconDec.US_IsAggregate = true;
			AssertEquals("should be valid now", true, docSupporter.GetDataStateBeforeRun(menu).IsValid);
			menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, ReconDeclarationDocumentSupporter.EntryByEntryReconMenuName));
			AssertNotNull(menu);
			reconDec.US_R_IsNoChangeAgg = true;
			dataState = docSupporter.GetDataStateBeforeRun(menu);
			AssertEquals("trying to run an invalid document", false, dataState.IsValid);
			AssertEquals(ReconDeclarationDocumentSupporter.NoChangeAggregate, dataState.ErrorMessage);
			menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, ReconDeclarationDocumentSupporter.LineSummaryMenuItem));
			dataState = docSupporter.GetDataStateBeforeRun(menu);
			AssertEquals("trying to run an invalid document", false, dataState.IsValid);
			AssertEquals(ReconDeclarationDocumentSupporter.NoChangeAggregate, dataState.ErrorMessage);
			reconDec.US_IsAggregate = false;
			menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, ReconDeclarationDocumentSupporter.EntryByEntryReconMenuName));
			AssertEquals("should be valid now", true, docSupporter.GetDataStateBeforeRun(menu).IsValid);
			menu = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, ReconDeclarationDocumentSupporter.LineSummaryMenuItem));
			dataState = docSupporter.GetDataStateBeforeRun(menu);
			AssertEquals("No invoice line has been entered", false, dataState.IsValid);
			AssertEquals(ReconChangedLinesMerger.NoInvoiceLinesHaveBeenEntered, dataState.ErrorMessage);
			reconDec.InvoiceLines.AddNew();
			dataState = docSupporter.GetDataStateBeforeRun(menu);
			AssertEquals("invoice lines are entered", true, dataState.IsValid);
			reconDec.US_IsAggregate = true;
			reconDec.US_R_IsNoChangeAgg = true;
			dataState = docSupporter.GetDataStateBeforeRun(menu);
			AssertEquals("Total charges are overridden", false, dataState.IsValid);
			AssertEquals(ReconDeclarationDocumentSupporter.NoChangeAggregate, dataState.ErrorMessage);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => new ReconDeclaration(Factory.New<JobDeclaration>());

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable businessObject)
		{
			base.DoSetupForDocument(command, businessObject);
			string menuName = command.SU_MenuName;
			switch (menuName)
			{
				case ReconDeclarationDocumentSupporter.AggregateReconMenuName:
					SetUpForAggregateRecon((ReconDeclaration)businessObject);
					break;
				case ReconDeclarationDocumentSupporter.EntryByEntryReconMenuName:
					SetUpForEntryByEntryRecon((ReconDeclaration)businessObject);
					break;
				case ReconDeclarationDocumentSupporter.LineSummaryMenuItem:
					SetUpForLineSummaryRecon((ReconDeclaration)businessObject);
					break;
			}
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand) => documentCommand.SU_MenuName.Contains("DocBuilder Invoice");

		void SetUpForAggregateRecon(ReconDeclaration reconDec)
		{
			reconDec.US_IsAggregate = true;
			reconDec.OriginalEntries.AddNew();
		}

		void SetUpForEntryByEntryRecon(ReconDeclaration reconDec)
		{
			reconDec.US_IsAggregate = false;
			reconDec.OriginalEntries.AddNew();
		}

		void SetUpForLineSummaryRecon(ReconDeclaration reconDec)
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration1.US_EnableENS = true;
			declaration1.US_BondProducerAccNo = "12";
			declaration1.Invoices.AddNew();
			var invoiceLine = declaration1.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9102.11.1010";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_LinePrice = 3406.00m;
			invoiceLine.US_98GoodsValue = 1852.00m;
			declaration1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var ensEntry1 = declaration1.CustomsEntryHeaders[0];
			var reconOriginalEntry = reconDec.OriginalEntries.Count > 0 ? reconDec.OriginalEntries[0] : reconDec.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ5" + ensEntry1.EntryNumber;
			AssertEquals("CH_CH_OrigEntry is updated", ensEntry1.PK, reconOriginalEntry.CH_CH_OriginalEntry);
			var invoice = reconOriginalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";
			var recinvoiceLine = invoice.JobComInvoiceLines.AddNew();
			recinvoiceLine.JI_Tariff = "3201.90.1000";
			recinvoiceLine.JI_CustomsQuantity = 50m;
			recinvoiceLine.JI_LinePrice = 3000m;
			recinvoiceLine.US_Duty = 125m;
			recinvoiceLine.US_R_OrigEntryLineNo = "1";
			recinvoiceLine.US_R_OrigTariff = recinvoiceLine.JI_Tariff;
			recinvoiceLine.US_R_OrigCV = 1000m;
			recinvoiceLine.US_R_OrigDuty = 17m;
		}
	}
}
