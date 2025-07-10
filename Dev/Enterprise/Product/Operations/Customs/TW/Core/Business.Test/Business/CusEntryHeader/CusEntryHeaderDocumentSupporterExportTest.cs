using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	sealed class CusEntryHeaderDocumentSupporterExportTest : Customs.Business.Testing.CusEntryHeaderDocumentSupportTest
	{
		[ExpectNoExceptions]
		public void TestGetBODocDataProviders()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			var providers = entryHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ExportCustomsDeclarationDocument), null);
			NUnit.Framework.Assert.That(providers.Length, NUnit.Framework.Is.EqualTo(1), "Provider for ExportCustomsDeclarationDocument");
		}

		[ExpectNoExceptions]
		public override void TestGetDocBusinessObjects()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			var result = entryHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo(default(Enterprise.DocumentEngineCore.DocWrappers.DocumentWrapper)));
		}

		[ExpectNoExceptions]
		public void TestGetExportCustomsDeclarationDocumentFilterValue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoice0 = declaration.Invoices.AddNew();
			var line0 = invoice0.InvoiceLines.AddNew() as JobComInvoiceLine;
			line0.JI_CEI = entryInstruction.PK;
			line0.JI_Description = @"1";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			line0.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY), NUnit.Framework.Is.EqualTo("EXPTW"), "Filtervalue for MSGBKRCTY");
		}

		[ExpectNoExceptions]
		public void TestExportCustomsDeclarationDocumentIsSupported()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.IsDataContextSupported(new DataContextValue(CusEntryHeaderDocumentSupporter.ExportCustomsDeclarationDocument)), NUnit.Framework.Is.EqualTo(true));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
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
			new LineMerger(declaration).DoMerge();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();
			return declaration.ActiveEntryHeaders[0];
		}
	}
}
