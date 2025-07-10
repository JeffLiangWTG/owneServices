using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	sealed class JobDeclarationDocumentSupporterTest : Customs.Business.Testing.BaseJobDeclarationDocumentSupportTest
	{
		public void TestGetDocumentWrappersInternal()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var menu = Factory.New<IStmMenuItem>();
			menu.SU_MenuName = "Customs Worksheet";

			DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, menu);
			AssertEquals(0, result.Length);

			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, menu);
			AssertEquals(1, result.Length);
		}

		public void TestGetDataStateBeforeRun()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "EFT Request"));
			DocumentSupporterDataState dataState = declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			Assert("Data state is valid", dataState.IsValid);
		}

		public override void TestGetDocBusinessObjects()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();

			DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
			AssertEquals("Document wrapper for data context of declaration is of type DocDeclaration", "Enterprise.Customs.ZA.Business.DocumentWrappers.DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Notes, null);
			AssertEquals("Document wrapper for data context of notes is of type DocDeclaration", "Enterprise.Customs.ZA.Business.DocumentWrappers.DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CommercialInvoice, null);
			AssertEquals("Document wrapper for data context of CommercialInvoice is of type DocJobComInvoiceHeader", "Enterprise.Customs.ZA.Business.DocumentWrappers.DocJobComInvoiceHeader", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ChargeSheet, null);
			AssertEquals("Document wrapper for data context of Charge sheet is of type DocDeclaration", "Enterprise.Customs.ZA.Business.DocumentWrappers.DocDeclaration", result[0].GetType().ToString());

			declaration.CustomsEntryHeaders.AddNew();
			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			AssertEquals("Document wrapper for data context of CusEntryHeader is of type DocDeclaration", "Enterprise.Customs.ZA.Business.DocumentWrappers.DocCusEntryHeader", result[0].GetType().ToString());
		}

		public void TestGetMenuTemplateFilterValueForContinuationPage()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			DocCusEntryTestWrapper testWrapper = DocCusEntryTestWrapper.New(entryHeader, Factory);
			ZString result = declaration.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CONTINUE, testWrapper);
			AssertEquals("No continuation is needed", "No", result);

			entryHeader.MergedLines.AddNew();
			result = declaration.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CONTINUE, testWrapper);
			AssertEquals("No continuation is needed", "No", result);

			entryHeader.MergedLines.AddNew();
			result = declaration.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.CONTINUE, testWrapper);
			AssertEquals("Continuation is needed", "Yes", result);
		}

		public void TestGetMenuTemplateFilterValueForSupplierList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var mockEntryHeader = Factory.NewMoq<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(mockEntryHeader.Object);

			var supplier1 = Factory.New<OrgHeader>();
			var supplier2 = Factory.New<OrgHeader>();
			var supplierColl = new OrgHeaderCollection(Factory);
			supplierColl.Add(supplier1);
			mockEntryHeader.Setup(m => m.Suppliers)
				.Returns(supplierColl);

			var testWrapper = DocCusEntryTestWrapper.New(mockEntryHeader.Object, Factory);
			ZString result = declaration.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.MULTISUPPLIER, testWrapper);
			AssertEquals("No supplier list is needed", "No", result);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			result = declaration.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.MULTISUPPLIER, testWrapper);
			AssertEquals("No supplier list is needed", "No", result);

			supplierColl.Add(supplier2);
			mockEntryHeader.Setup(m => m.Suppliers)
				.Returns(supplierColl);
			result = declaration.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.MULTISUPPLIER, testWrapper);
			AssertEquals("Supplier list is needed", "Yes", result);
		}

		public void TestGetBODocDataProviders()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusProcedure("ZA", "T", "XX", "YY", "5", "XXYY5", "EXP");
			testHelper.CreateRefCusProcedure("ZA", "T", "XX", "ZZ", "", "XXZZ", "EXP");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var entryHeader1 = dec.CustomsEntryHeaders.AddNew();
			var entryHeader2 = dec.CustomsEntryHeaders.AddNew();
			var providers = dec.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null);
			AssertEquals(2, providers.Length);
			providers = dec.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null);
			AssertEquals(2, providers.Length);
			providers = dec.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null);
			AssertEquals(2, providers.Length);
			providers = dec.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.DA63Document), null);
			AssertEquals(0, providers.Length);

			var inst = dec.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = "XX";
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = inst.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "ZZ";
			var entryLine = entryHeader1.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			providers = entryHeader1.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.DA63Document), null);
			AssertEquals("Provider for DA63Document", 0, providers.Length);

			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "YY";
			providers = entryHeader1.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.DA63Document), null);
			AssertEquals("Provider for DA63Document", 1, providers.Length);
		}

		public override void TestGetBODocDataProvidersNotFoundMessage()
		{
			CombineAssertions(() =>
			{
				var dec = Factory.New<JobDeclaration>();
				var testingContext = new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair);
				AssertEquals("No Entry Header has been found for  document.", dec.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
				AssertEquals("No Entry Header has been found for Test Menu document.", dec.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, new IStmMenuItemForTesting()));

				testingContext = new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack);
				AssertEquals("No Entry Header has been found for  document.", dec.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
				AssertEquals("No Entry Header has been found for Test Menu document.", dec.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, new IStmMenuItemForTesting()));

				testingContext = new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack);
				AssertEquals("No Entry Header has been found for  document.", dec.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
				AssertEquals("No Entry Header has been found for Test Menu document.", dec.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, new IStmMenuItemForTesting()));

				testingContext = new DataContextValue("CusEntryHeader");
				AssertEquals("Entry Header cannot be found.", dec.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
				AssertEquals("Entry Header cannot be found.", dec.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, new IStmMenuItemForTesting()));

				testingContext = new DataContextValue(CusEntryHeaderDocumentSupporter.DA63Document);
				AssertEquals("No Entry Header has been found for  document.", dec.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
			});

			CombineAssertions("Test Message for DA63Document", () =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = declaration.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = instruction.PK;
				var invoiceLine2 = declaration.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction.PK;
				var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
				entryHeader1.CH_BGMReference = "BGM1";
				var entryLine1 = entryHeader1.MergedLines.AddNew();
				var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
				entryHeader2.CH_BGMReference = "BGM2";

				var testingContext = new DataContextValue(CusEntryHeaderDocumentSupporter.DA63Document);
				AssertEquals(@"Entry Header BGM1 doesn't contain any DA63 Entry Line
Entry Header BGM2 doesn't contain any DA63 Entry Line", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
			});
		}

		public new void TestTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var supporter = new JobDeclarationDocumentSupporter(declaration);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Air, supporter.TransportMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Sea, supporter.TransportMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Road, supporter.TransportMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Rail, supporter.TransportMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
		}

		public void TestCustomWatermarkForSADDocumentPack()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			var collection = new SADDocumentWatermarkCollection();
			var watermark = collection.AddNew();
			watermark.EntryStatusCode = "1";
			ZACustomsRegistry.Instance.SADDocumentPackWatermarks.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, collection);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "1";
			var supporter = declaration.DocumentSupporter;

			var context = new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack);
			var docCommand = Factory.New<DocumentCommand>();
			docCommand.SU_MenuName = "Customs Declaration Response";
			AssertNull("Custom Watermark should be null", supporter.GetCustomWatermarkText(docCommand, supporter.GetBODocDataProviders(context, docCommand)[0]));
			docCommand.SU_MenuName = "SAD Document Pack";
			AssertEquals("Custom Watermark should be set", "Release", supporter.GetCustomWatermarkText(docCommand, supporter.GetBODocDataProviders(context, docCommand)[0]));
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForRunningDocumentsTest
		{
			get
			{
				var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
				yield return declaration;
			}
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = (JobDeclaration)GetDocumentSupportBusinessObjectWithLandedCosting();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var orgHeader0 = Factory.New<OrgHeader>();
			orgHeader0.OH_Code = "TESTORG1";
			var invoice0 = declaration.Invoices.AddNew();
			invoice0.InvoiceLines.AddNew();
			invoice0.JZ_OH_Supplier = orgHeader0.PK;
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTORG2";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice1.JZ_OH_Supplier = orgHeader1.PK;
			DoMerge(declaration);
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			return declaration;
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			new LineMerger((JobDeclaration)declaration).DoMerge();
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return base.ExcludeDocumentCommandTest(documentCommand)
				&& !documentCommand.SU_MenuName.StartsWith("Bill of Entry")
				&& !documentCommand.SU_MenuName.StartsWith("Exchange Control")
				&& !documentCommand.SU_MenuName.StartsWith("Customs and Excise Declaration")
				&& !documentCommand.SU_MenuName.StartsWith("Post Consolidation")
				&& !documentCommand.SU_MenuName.StartsWith("Pre Consolidation");
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;

				maxHits["CusCodeData"] = 8;
				maxHits["OrgCusCode"] = 3;
				maxHits["OrgContact"] = 3;
				maxHits["CusEntryInstruction"] = 2;
				maxHits["CusVehicle"] = 6;
				return maxHits;
			}
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";
			foreach (JobComInvoiceLine line in declaration.InvoiceLines)
			{
				line.JI_CEI = testInstruction.PK;
			}
		}
	}

	sealed class DocCusEntryTestWrapper : DocumentWrapper
	{
		DocCusEntryTestWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			: base(entryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryTestWrapper New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
		{
			if (entryHeader == null)
			{
				return null;
			}
			else
			{
				return new DocCusEntryTestWrapper(entryHeader, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return ZString.Empty;
		}
	}

	sealed class IStmMenuItemForTesting : IStmMenuItem
	{
		public ZGuid SU_PrimaryDocPackItemId
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_Purpose
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_EmailSenderOverride
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool AllowMultipleCopies
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ICodeDescriptionPairList AttachmentTypes => throw new NotImplementedException();

		public ICollection Documents => throw new NotImplementedException();

		public ZString DocumentId => throw new NotImplementedException();

		public bool IsLocalDocument
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZShort NumberOfCopies
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZGuid PK => throw new NotImplementedException();

		public CultureInfo RenderCulture
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBlob SU_ActionDataUpdateBlob
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBlob SU_ActionMenusAndMethodsBlob
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_AddressCategory
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_AllowRawView
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_BusinessContext
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_ContactType
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_DeliveryRestrictionDescription
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_DeliveryRestrictionMacro
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_DeliveryRestrictionType
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_DocumentDirection
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_DraftOption
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_EmailSubjectLine
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_FilterList
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZDecimal SU_FlexCelLineSpacing
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_GS_NKStaffCode
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_SignBy
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_Hint
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_IncludeDocInArchive
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_IsClientSpecific
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_IsDocPack
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_IsLocalDocument
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_IsModifiable
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_IsPublished
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_IsSystemDefined
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_IsZippedDocPack
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_LicenceLevel
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_MenuDataContext
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZShort SU_MenuIndex
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_MenuName
		{
			get => "Test Menu";
			set => throw new NotImplementedException();
		}

		public MultilingualString SU_MenuNameMultilingual => throw new NotImplementedException();

		public ZString SU_MenuPath
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_MenuShortcut
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_MenuType
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_MustRunOnline
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_PreventAutoDelivery
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_SE_NKDocumentEvent
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_ShowDocToSendTab
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZBool SU_SupportsVisualisation
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ZString SU_DefaultAttachmentType
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		ICollection IStmMenuItem.ChildMenus => throw new NotImplementedException();
	}
}
