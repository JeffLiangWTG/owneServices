using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	sealed class JobDeclarationDocumentSupporterTest : Customs.Business.Testing.BaseJobDeclarationDocumentSupportTest
	{
		public override void TestGetBODocDataProvidersNotFoundMessage()
		{
			base.TestGetBODocDataProvidersNotFoundMessage();
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var importCustomsNonCondensedDeclarationDocumentDataContextValue = new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsNonCondensedDeclarationDocument);
			var applicationAndCertificateDocumentDataContextValue = new DataContextValue(CusEntryHeaderDocumentSupporter.ApplicationAndCertificateDocument);
			AssertNullOrEmpty(declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(importCustomsNonCondensedDeclarationDocumentDataContextValue, null));
			AssertNullOrEmpty(declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(applicationAndCertificateDocumentDataContextValue, null));
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			var expectedString = "Entry Header cannot be found. Please select Brokerage > Generate Entries (Merge) to merge the declaration.";
			AssertEquals(expectedString, declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(importCustomsNonCondensedDeclarationDocumentDataContextValue, null));
			AssertEquals(expectedString, declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(applicationAndCertificateDocumentDataContextValue, null));
			var invoicePackingWeightListDocumentDataContextValue = new DataContextValue(JobDeclarationDocumentSupporter.InvoicePackingWeightListDocument);
			AssertEquals("No Packing Weight List information.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(invoicePackingWeightListDocumentDataContextValue, null));
		}

		[GuiTest]
		public void TestDoNotOverwriteCurrentCommand()
		{
			var documentTemplate = DocumentEngine.Testing.DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[DataContext=CusEntryHeader]
{A}-[Name=<LocalReferenceNumber>]
{A}-[#EndOfReport]");
			var stmTemplate = Factory.New<StmTemplateBase>();
			stmTemplate.SO_Name = "Test Document Template";
			stmTemplate.SO_DataContext = ".ExportCustomsDeclarationDocument";
			stmTemplate.SO_Template = documentTemplate;
			stmTemplate.SO_TemplateType = "DOC";

			var menuItemClicked = Factory.New<IStmMenuItem>();
			menuItemClicked.SU_MenuName = "Test Customs Declaration";
			menuItemClicked.SU_BusinessContext = "Customs";

			var pivotClicked = Factory.New<StmMenuTemplatePivotBase>();
			pivotClicked.SI_DocumentTitle = "Test Document";
			pivotClicked.SI_SU = menuItemClicked.PK;
			pivotClicked.SI_SO = stmTemplate.PK;
			(menuItemClicked.Documents as List<IStmMenuTemplatePivot>).Add(pivotClicked);

			var menuItemTaskRelated = Factory.New<IStmMenuItem>();
			menuItemTaskRelated.SU_MenuName = "Test Customs Declaration (Task Related)";
			menuItemTaskRelated.SU_BusinessContext = "Customs";

			var pivotTaskRelated = Factory.New<StmMenuTemplatePivotBase>();
			pivotTaskRelated.SI_DocumentTitle = "Test Document (Task Related)";
			pivotTaskRelated.SI_SU = menuItemTaskRelated.PK;
			pivotClicked.SI_SO = stmTemplate.PK;
			(menuItemTaskRelated.Documents as List<IStmMenuTemplatePivot>).Add(pivotTaskRelated);

			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();

			var task = declaration.WorkflowItems.Triggers.AddNew();
			var taskNotification = task.ProcessTaskNotifications.AddNew();
			taskNotification.PQ_TriggerType = "EDC";
			taskNotification.PQ_SU_Document = menuItemTaskRelated.PK;

			declaration.DocumentSupporter.GetDataStateBeforeRun(menuItemClicked);
			AssertEquals("Test Customs Declaration", declaration.DocumentSupporter.CurrentCommand.SU_MenuName);
		}

		public void TestGetBODocDataProviders()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();

			var providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.N5167), null);
			AssertEquals("Provider for N5167", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(N5110MessageDocumentSupporter.N5110MessagePair), null);
			AssertEquals("Provider for N5110", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(N5111MessageDocumentSupporter.N5111MessagePair), null);
			AssertEquals("Provider for N5111", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.LetterofAuthorizationPersonal), null);
			AssertEquals("Provider for LetterofAuthorizationPersonal", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ExportCustomsDeclarationDocument), null);
			AssertEquals("Provider for ExportCustomsDeclarationDocument", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsDeclarationDocument), null);
			AssertEquals("Provider for ImportCustomsDeclarationDocument", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.ExportNonCondensedDeclarationDocument), null);
			AssertEquals("Provider for ExportNonCondensedDeclarationDocument", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsNonCondensedDeclarationDocument), null);
			AssertEquals("Provider for ImportCustomsNonCondensedDeclarationDocument", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ApplicationAndCertificateDocument), null);
			AssertEquals("Provider for ApplicationAndCertificateDocument", 2, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.GenericCommercialInvoice), null);
			AssertEquals("Provider for GenericCommercialInvoice", 3, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.InvoicePackingWeightListDocument), null);
			AssertEquals("Provider for InvoicePackingWeightListDocument", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(N5116MessageDocumentSupporter.N5116MessagePair), null);
			AssertEquals("Provider for N5116Message", 1, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(N5204MessageDocumentSupporter.N5204MessagePair), null);
			AssertEquals("Provider for N5204Message", 1, providers.Length);

			var package = declaration.Packages.AddNew();
			package.CW_MarksAndNos = "A1";
			package.CW_PackQty = 1;
			package.CW_PackType = "PKG";
			package = declaration.Packages.AddNew();
			package.CW_MarksAndNos = "A2";
			package.CW_PackQty = 1;
			package.CW_PackType = "PKG";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().FirstOrDefault(x => x.Package.CW_MarksAndNos == "A1").IsLinked = true;
			invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().FirstOrDefault(x => x.Package.CW_MarksAndNos == "A2").IsLinked = true;
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.InvoicePackingWeightListDocument), null);
			AssertEquals("Provider for InvoicePackingWeightListDocument", 3, providers.Length);
			invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().FirstOrDefault(x => x.Package.CW_MarksAndNos == "A1").IsLinked = false;
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.InvoicePackingWeightListDocument), null);
			AssertEquals("Provider for InvoicePackingWeightListDocument", 2, providers.Length);
			providers = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(JobDeclarationDocumentSupporter.CusPackingList), null);
			AssertEquals("Provider for cusPackingList", 1, providers.Length);
		}

		public void TestGetBODocDataProviders_ExportCustomsDeclarationDocument()
		{
			var declaration1 = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var declaration2 = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var declarationList = declaration1.DocumentSupporter.JobDeclarationDocumentAddressConfig.Declarations;
			declarationList.Add(declaration1);
			declarationList.Add(declaration2);
			var providers = declaration1.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ExportCustomsDeclarationDocument), null);
			AssertEquals("Provider for ExportCustomsDeclarationDocument", 2, providers.Length);
		}

		public void TestGetBODocDataProviders_ImportCustomsDeclarationDocument()
		{
			var declaration1 = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var declaration2 = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var declarationList = declaration1.DocumentSupporter.JobDeclarationDocumentAddressConfig.Declarations;
			declarationList.Add(declaration1);
			declarationList.Add(declaration2);
			var providers = declaration1.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsDeclarationDocument), null);
			AssertEquals("Provider for ImportCustomsDeclarationDocument", 2, providers.Length);
		}

		public void TestDataContextSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsDeclarationDocument)));
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsNonCondensedDeclarationDocument)));
		}

		public void TestN5167IsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(CusEntryHeaderDocumentSupporter.N5167)));
		}

		public void TestN5110IsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(N5110MessageDocumentSupporter.N5110MessagePair)));
		}

		public void TestN5111IsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(N5111MessageDocumentSupporter.N5111MessagePair)));
		}

		public void TestLetterofAuthorizationPersonalIsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.LetterofAuthorizationPersonal)));
		}

		public void TestInvoicePackingWeightListIsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.InvoicePackingWeightListDocument)));
		}

		public override void TestGetDocBusinessObjects()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
			AssertNotNull(result[0]);
		}

		public void TestGetDocumentWrappersForSpecificDocuments()
		{
			AssertGetDocumentWrappersForSpecificDocuments("Import Customs Declaration (English)", typeof(ImportCustomsDeclarationDocumentWrapper));
			AssertGetDocumentWrappersForSpecificDocuments("Import Customs Declaration (Formal)", typeof(ImportCustomsDeclarationDocumentWrapper));
			AssertGetDocumentWrappersForSpecificDocuments("Import Customs Declaration (Informal)", typeof(ImportCustomsDeclarationDocumentWrapper));
			AssertGetDocumentWrappersForSpecificDocuments("Import Customs Declaration (Proof)", typeof(ImportCustomsDeclarationDocumentWrapper));

			AssertGetDocumentWrappersForSpecificDocuments("Import Customs Declaration (NCD Formal)", typeof(ImportCustomsDeclarationNonCondensedDocumentWrapper));
			AssertGetDocumentWrappersForSpecificDocuments("Import Customs Declaration (NCD Informal)", typeof(ImportCustomsDeclarationNonCondensedDocumentWrapper));

			AssertGetDocumentWrappersForSpecificDocuments("Export Customs Declaration (English)", typeof(ExportCustomsDeclarationDocumentWrapper));
			AssertGetDocumentWrappersForSpecificDocuments("Export Customs Declaration (Formal)", typeof(ExportCustomsDeclarationDocumentWrapper));
			AssertGetDocumentWrappersForSpecificDocuments("Export Customs Declaration (Informal)", typeof(ExportCustomsDeclarationDocumentWrapper));
			AssertGetDocumentWrappersForSpecificDocuments("Export Customs Declaration (Proof)", typeof(ExportCustomsDeclarationDocumentWrapper));

			AssertGetDocumentWrappersForSpecificDocuments("Export Customs Declaration (NCD Formal)", typeof(ExportNonCondensedDeclarationDocumentWrapper));
			AssertGetDocumentWrappersForSpecificDocuments("Export Customs Declaration (NCD Informal)", typeof(ExportNonCondensedDeclarationDocumentWrapper));
		}

		void AssertGetDocumentWrappersForSpecificDocuments(ZString menuName, Type expectWrapperType)
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = menuName;
			var wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, menu);
			wrappers.Select(x => x.GetType()).ToList();
			Assert($"Should have correct wrapper type {expectWrapperType.FullName} for {menuName}", wrappers.Any(x => expectWrapperType.IsInstanceOfType(x)));
		}

		public void TestExportCustomsDeclarationDocumentIsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(CusEntryHeaderDocumentSupporter.ExportCustomsDeclarationDocument)));
		}

		public void TestExportNonCondensedDeclarationDocumentIsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.ExportNonCondensedDeclarationDocument)));
		}

		public void TestImportCustomsDeclarationDocumentIsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsDeclarationDocument)));
		}

		public void TestApplicationAndCertificateDocumentSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(CusEntryHeaderDocumentSupporter.ApplicationAndCertificateDocument)));
		}

		public void TestGenericCommercialInvoiceIsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(JobDeclarationDocumentSupporter.GenericCommercialInvoice)));
		}

		public void TestN5116MessageIsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(N5116MessageDocumentSupporter.N5116MessagePair)));
		}

		public void TestN5204MessageIsSupported()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			AssertEquals(true, declaration.DocumentSupporter.IsDataContextSupported(new DataContextValue(N5204MessageDocumentSupporter.N5204MessagePair)));
		}

		public void TestGetFilterValue()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MergeBy = "TRF";
			AssertEquals("IMPTWTRF", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.IsNonCondensedDeclaration));
			declaration.JE_MessageType = "EXP";
			declaration.JE_MergeBy = "XX";
			AssertEquals("EXPTWXX", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.IsNonCondensedDeclaration));
			declaration.JE_MessageType = "IMP";
			AssertEquals("IMPVHCTW", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGGDSCTRY));
		}

		public new void TestMasterBillContextIsSupported()
		{
			Declaration.Bills.RemoveAndDeleteAll();
			var masterBill1 = Declaration.Bills.AddNew();
			masterBill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill1.CU_BillNum = "HELLO";
			var houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "GoodBye";
			var masterBill2 = Declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "SAILOR";
			var supporter = new BaseJobDeclarationDocumentSupporter(Declaration);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(".MasterBill")));
			IBODocDataProvider[] result = supporter.GetBODocDataProviders(new DataContextValue(".MasterBill"), null);
			AssertEquals(2, result.Length);
			AssertEquals(masterBill1, BODocDataProvider.GetBusinessObject(result[0]));
			AssertEquals(masterBill2, BODocDataProvider.GetBusinessObject(result[1]));
		}

		public void TestGetDataStateBeforeRunSupportRefDocTypeIsNull()
		{
			var declaration = Factory.New<JobDeclaration>();
			var documentSupporter = declaration.DocumentSupporter;

			var menuItemForTesting = Factory.New<IStmMenuItem>();
			menuItemForTesting.SU_MenuName = "Test Customs Declaration";
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_DocumentTitle = "Test Document";
			pivot.SI_SU = menuItemForTesting.PK;
			(menuItemForTesting.Documents as List<IStmMenuTemplatePivot>).Add(pivot);

			AssertNoExceptionThrown(() => documentSupporter.GetDataStateBeforeRun(menuItemForTesting));
		}

		public void TestCurrentCommand()
		{
			var declaration = Factory.New<JobDeclaration>();
			var documentSupporter = declaration.DocumentSupporter;
			AssertNull(documentSupporter.CurrentCommand);

			documentSupporter.GetDataStateBeforeRun(null);
			AssertNull(documentSupporter.CurrentCommand);

			var menuItemForTesting = Factory.New<IStmMenuItem>();
			documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
			AssertSame(menuItemForTesting, documentSupporter.CurrentCommand);
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice0 = declaration.Invoices.AddNew();
			invoice0.InvoiceLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			var line0 = invoice0.InvoiceLines.AddNew() as JobComInvoiceLine;
			line0.JI_CEI = entryInst.PK;
			line0.JI_Description = @"1). YEAR: 2016 CAR TYPE: WAGON DOOR: 5
2). BRAND: VOLVO MODEL: XC60 D4
3). DISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5
4). LEFT SIDE STEERING: YES
5). TRANSMISSION: AUTO (A8) (WITH AUTO SHIFT LOCK)
6). ENGINE TYPE: DIESEL
7). STANDARD EQUIPMENT WITH EGR & CATALYST CONVERTER: YES
8).NON CFC REFRIGERANT SYSTEM (R134A)
9). CHASSIS NO: YV1DZA8BDG2911087; YV1DZA8BDG2910741; YVIMV29H0G2337910";
			var chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "123456";
			chassis = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "987654";
			var packingGroup = declaration.PackingGroups.AddNew();
			var houseBill = declaration.Bills.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			var package = declaration.Packages.AddNew();
			package.CW_MarksAndNos = "A0";
			package.CW_PackQty = 1;
			package.CW_PackType = "PKG";
			package.CW_CR_HouseContainer = packingGroup.PK;
			line0.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().FirstOrDefault(x => x.Package.CW_MarksAndNos == "A0").IsLinked = true;
			declaration.CreateCusPackingList(Factory);
			return declaration;
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			var dec = declaration as JobDeclaration;
			new LineMerger(dec).DoMerge();
			dec.MergeManager.DisablePreSaveMergeRequirementForTesting();
		}

		protected override BaseJobDeclaration GetDocumentSupportableDeclaration()
		{
			var declaration = base.GetDocumentSupportableDeclaration() as JobDeclaration;
			var n5116Message = Factory.New<N5116EDIMessage>();
			declaration.EntryHeader.Messages.Add(n5116Message);
			var n5204Message = Factory.New<N5204EDIMessage>();
			declaration.EntryHeader.Messages.Add(n5204Message);
			var n5110message = Factory.New<N5110EDIMessage>();
			declaration.EntryHeader.Messages.Add(n5110message);
			var n5111message = Factory.New<N5111EDIMessage>();
			n5111message.EM_MessageType = MessageTypeList.Codes.TAD;
			declaration.EntryHeader.Messages.Add(n5111message);
			return declaration;
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;
				maxHits.Add("EDIMessage", 2);
				maxHits.Add("CusCodeData", 2);
				maxHits.Add("CusEntryInstruction", 2);
				maxHits.Add("JobComInvLineRefs", 4);
				maxHits.Add("CusRefRateCodeView", 2);
				maxHits.Add("CusHouseContPackInvoiceLinePivot", 3);
				return maxHits;
			}
		}
	}
}
