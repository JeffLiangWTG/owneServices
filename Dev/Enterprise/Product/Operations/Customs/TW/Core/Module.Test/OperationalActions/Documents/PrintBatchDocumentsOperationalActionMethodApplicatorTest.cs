using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(PrintBatchDocumentsOperationalActionMethodApplicator))]
	sealed class PrintBatchDocumentsOperationalActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestDefaultScheduleActionCode()
		{
			var testApplicator = new PrintBatchDocumentsOperationalActionMethodApplicator("test name");
			CombineAssertions(() =>
			{
				AssertEquals("ReMergeAndCalculate", false, testApplicator.ReMergeAndCalculate);
				AssertEquals("SuppressNotificationPopout", true, testApplicator.SuppressNotificationPopout);
				AssertEquals("IgnoreMessageWarnings", false, testApplicator.IgnoreMessageWarnings);
				AssertEquals("Name", "test name", testApplicator.Name);
			});
		}

		public void TestApplyCore()
		{
			var testOperationalLog = new DummyOperationalActionSectionLog();
			var applicator = new PrintBatchDocumentsOperationalActionMethodApplicator(JobDeclarationDocumentSupporter.DocumentName.ExportCustomsDeclarationInformal);
			applicator.ReMergeAndCalculate = false;
			applicator.SuppressNotificationPopout = true;
			applicator.IgnoreMessageWarnings = true;

			var decl1 = Factory.New<JobDeclaration>();
			decl1.JE_MessageType = JobMessageTypeList.Codes.Export;
			decl1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			decl1.JE_CustomsOffice = "BT";
			decl1.JE_DeclarationReference = "TWB000001";
			var entryInstruction1 = decl1.CusEntryInstruction;
			entryInstruction1.CEI_Style = Constants.DeclarationTypes.Export.G3;
			entryInstruction1.CEI_CustomsOffice = "BT";

			var decl2 = Factory.New<JobDeclaration>();
			decl2.JE_MessageType = JobMessageTypeList.Codes.Export;
			decl2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			decl2.JE_CustomsOffice = "BT";
			decl2.JE_DeclarationReference = "TWB000002";
			var entryInstruction2 = decl2.CusEntryInstruction;
			entryInstruction2.CEI_Style = Constants.DeclarationTypes.Export.G3;
			entryInstruction2.CEI_CustomsOffice = "BT";
			Factory.Save();

			applicator.Apply(testOperationalLog, new BusinessObject[] { decl1, decl2, entryInstruction1 });
			CombineAssertions("Warning", () =>
			{
				AssertEquals("Notification count", 3, testOperationalLog.messages.Count);
				AssertEquals("Line 1", @"INFO: 3 jobs has been selected.", testOperationalLog.messages[0]);
				AssertEquals("Line 2", @"WARNING: There are no entry to print customs declaration for Job Number : TWB000001, TWB000002, and so on.", testOperationalLog.messages[1]);
				AssertEquals("Line 3", @"INFO: No jobs have been printed.", testOperationalLog.messages[2]);
			});

			testOperationalLog.messages.Clear();
			applicator.ReMergeAndCalculate = true;
			applicator.Apply(testOperationalLog, new BusinessObject[] { decl1, decl2 });
			CombineAssertions("ReMergeAndCalculate", () =>
			{
				AssertEquals("Notification count", 5, testOperationalLog.messages.Count);
				AssertEquals("Line 1", @"INFO: 2 jobs has been selected.", testOperationalLog.messages[0]);
				AssertEquals("Line 2", @"ERROR: The job TWB000001 does not have 'Submit Type: BLT - Submit entry using built-in messaging system' selected.", testOperationalLog.messages[1]);
				AssertEquals("Line 3", @"ERROR: The job TWB000002 does not have 'Submit Type: BLT - Submit entry using built-in messaging system' selected.", testOperationalLog.messages[2]);
				AssertEquals("Line 4", @"WARNING: There are no entry to print customs declaration for Job Number : TWB000001, TWB000002, and so on.", testOperationalLog.messages[3]);
				AssertEquals("Line 5", @"INFO: No jobs have been printed.", testOperationalLog.messages[4]);
			});

			decl1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			testOperationalLog.messages.Clear();
			applicator.Apply(testOperationalLog, new BusinessObject[] { decl1, decl2 });
			CombineAssertions("JE_ApplicationCode：Builtin", () =>
			{
				AssertEquals("Notification count", 7, testOperationalLog.messages.Count);
				AssertEquals("Line 1", @"INFO: 2 jobs has been selected.", testOperationalLog.messages[0]);
				AssertEquals("Line 2", @"INFO: Job Number: TWB000001, Merging ...", testOperationalLog.messages[1]);
				AssertEquals("Line 3", @"ERROR: You can't merge this entry because there are no invoice headers.", testOperationalLog.messages[2]);
				AssertEquals("Line 4", @"ERROR: Job Number: TWB000001, Merge failed.", testOperationalLog.messages[3]);
				AssertEquals("Line 5", @"ERROR: The job TWB000002 does not have 'Submit Type: BLT - Submit entry using built-in messaging system' selected.", testOperationalLog.messages[4]);
				AssertEquals("Line 6", @"WARNING: There are no entry to print customs declaration for Job Number : TWB000001, TWB000002, and so on.", testOperationalLog.messages[5]);
				AssertEquals("Line 7", @"INFO: No jobs have been printed.", testOperationalLog.messages[6]);
			});

			entryInstruction1.CEI_DateForDuty = new ZDateTime(2024, 4, 17);
			var invoiceHeader1 = decl1.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.JI_Procedure = "PR";
			invoiceLine1.JI_Tariff = "87031000002";
			invoiceLine1.JI_CountryOfOrigin = "TW";
			invoiceLine1.JI_PrimaryPreference = "STD";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_EnteredUnitPrice = 1000m;
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			testOperationalLog.messages.Clear();
			applicator.Apply(testOperationalLog, new BusinessObject[] { decl2, decl1 });
			AssertEquals("Last message", @"INFO: 1 job successfully printed.", testOperationalLog.messages.Last());

			entryInstruction2.CEI_DateForDuty = new ZDateTime(2024, 4, 17);
			decl2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader2 = decl2.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 2000m;
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			invoiceLine2.JI_Procedure = "PR";
			invoiceLine2.JI_Tariff = "87031000002";
			invoiceLine2.JI_CountryOfOrigin = "TW";
			invoiceLine2.JI_PrimaryPreference = "STD";
			invoiceLine2.JI_InvoiceQuantity = 3;
			invoiceLine2.JI_EnteredUnitPrice = 562;
			invoiceLine2.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			testOperationalLog.messages.Clear();
			applicator.Apply(testOperationalLog, new BusinessObject[] { decl2, decl1 });
			AssertEquals("Last message", @"INFO: 2 jobs successfully printed.", testOperationalLog.messages.Last());

			var decl3 = Factory.New<JobDeclaration>();
			decl3.JE_MessageType = JobMessageTypeList.Codes.Export;
			decl3.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			decl3.JE_CustomsOffice = "BT";
			decl3.JE_DeclarationReference = "TWB000003";
			var entryInstruction3 = decl3.CusEntryInstruction;
			entryInstruction3.CEI_Style = Constants.DeclarationTypes.Export.G3;
			entryInstruction3.CEI_CustomsOffice = "BT";
			Factory.Save();

			applicator.ReMergeAndCalculate = false;
			applicator.IgnoreMessageWarnings = false;
			testOperationalLog.messages.Clear();
			applicator.Apply(testOperationalLog, new BusinessObject[] { decl1, decl2, decl3 });
			CombineAssertions("IgnoreMessageWarnings is false and only two no entry declarations", () =>
			{
				AssertEquals("Notification count", 4, testOperationalLog.messages.Count);
				AssertEquals("Line 1", @"INFO: 3 jobs has been selected.", testOperationalLog.messages[0]);
				AssertEquals("Line 2", @"WARNING: There are no entry to print customs declaration for Job Number : TWB000003, and so on.", testOperationalLog.messages[1]);
				AssertEquals("Line 3", @"ERROR: *Please generate entry before printing customs declaration documents.*", testOperationalLog.messages[2]);
				AssertEquals("Line 4", @"INFO: No jobs have been printed.", testOperationalLog.messages[3]);
			});

			applicator.ReMergeAndCalculate = true;
			testOperationalLog.messages.Clear();
			applicator.Apply(testOperationalLog, new BusinessObject[] { decl1, decl2, decl3 });
			CombineAssertions("ReMergeAndCalculate is true and only two no entry declarations", () =>
			{
				AssertEquals("Notification count", 6, testOperationalLog.messages.Count);
				AssertEquals("Line 1", @"INFO: 3 jobs has been selected.", testOperationalLog.messages[0]);
				AssertEquals("Line 2", @"INFO: Job Number: TWB000003, Merging ...", testOperationalLog.messages[1]);
				AssertEquals("Line 3", @"ERROR: You can't merge this entry because there are no invoice headers.", testOperationalLog.messages[2]);
				AssertEquals("Line 4", @"ERROR: Job Number: TWB000003, Merge failed.", testOperationalLog.messages[3]);
				AssertEquals("Line 5", @"WARNING: There are no entry to print customs declaration for Job Number : TWB000003, and so on.", testOperationalLog.messages[4]);
				AssertEquals("Line 6", @"INFO: No jobs have been printed.", testOperationalLog.messages[5]);
			});

			var decl4 = Factory.New<JobDeclaration>();
			decl4.JE_MessageType = JobMessageTypeList.Codes.Export;
			decl4.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			decl4.JE_CustomsOffice = "BT";
			decl4.JE_DeclarationReference = "TWB000004";
			var entryInstruction4 = decl4.CusEntryInstruction;
			entryInstruction4.CEI_Style = Constants.DeclarationTypes.Export.G3;
			entryInstruction4.CEI_CustomsOffice = "BT";
			Factory.Save();

			applicator.ReMergeAndCalculate = false;
			testOperationalLog.messages.Clear();
			applicator.Apply(testOperationalLog, new BusinessObject[] { decl3, decl4 });
			CombineAssertions("ReMergeAndCalculate is false and only two no entry declarations", () =>
			{
				AssertEquals("Notification count", 4, testOperationalLog.messages.Count);
				AssertEquals("Line 1", @"INFO: 2 jobs has been selected.", testOperationalLog.messages[0]);
				AssertEquals("Line 2", @"WARNING: There are no entry to print customs declaration for Job Number : TWB000003, TWB000004, and so on.", testOperationalLog.messages[1]);
				AssertEquals("Line 3", @"ERROR: *Please generate entry before printing customs declaration documents.*", testOperationalLog.messages[2]);
				AssertEquals("Line 4", @"INFO: No jobs have been printed.", testOperationalLog.messages[3]);
			});
		}

		public void TestPrintImportCustomsDeclarationFormal()
		{
			AssertPrintDeclarations("IMP", Constants.DeclarationTypes.Import.G1, JobDeclarationDocumentSupporter.DocumentName.ImportCustomsDeclarationFormal);
		}

		public void TestPrintImportCustomsDeclarationInformal()
		{
			AssertPrintDeclarations("IMP", Constants.DeclarationTypes.Import.G1, JobDeclarationDocumentSupporter.DocumentName.ImportCustomsDeclarationInformal);
		}

		public void TestPrintImportCustomsDeclarationProof()
		{
			AssertPrintDeclarations("IMP", Constants.DeclarationTypes.Import.G1, JobDeclarationDocumentSupporter.DocumentName.ImportCustomsDeclarationProof);
		}

		public void TestPrintImportCustomsDeclarationEnglish()
		{
			AssertPrintDeclarations("IMP", Constants.DeclarationTypes.Import.G1, JobDeclarationDocumentSupporter.DocumentName.ImportCustomsDeclarationEnglish);
		}

		public void TestPrintExportCustomsDeclarationFormal()
		{
			AssertPrintDeclarations("EXP", Constants.DeclarationTypes.Export.G3, JobDeclarationDocumentSupporter.DocumentName.ExportCustomsDeclarationFormal);
		}

		public void TestPrintExportCustomsDeclarationProof()
		{
			AssertPrintDeclarations("EXP", Constants.DeclarationTypes.Export.G3, JobDeclarationDocumentSupporter.DocumentName.ExportCustomsDeclarationProof);
		}

		public void TestPrintExportCustomsDeclarationEnglish()
		{
			AssertPrintDeclarations("EXP", Constants.DeclarationTypes.Export.G3, JobDeclarationDocumentSupporter.DocumentName.ExportCustomsDeclarationEnglish);
		}

		void AssertPrintDeclarations(string messageType, string declarationType, string documentName)
		{
			var testOperationalLog = new DummyOperationalActionSectionLog();
			var applicator = new PrintBatchDocumentsOperationalActionMethodApplicator(documentName);
			applicator.ReMergeAndCalculate = false;
			applicator.SuppressNotificationPopout = true;
			applicator.IgnoreMessageWarnings = true;

			var decl = Factory.New<JobDeclaration>();
			decl.JE_MessageType = messageType;
			decl.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			decl.JE_CustomsOffice = "BT";
			decl.JE_DeclarationReference = "TWB000001";
			var entryInstruction = decl.CusEntryInstruction;
			entryInstruction.CEI_Style = declarationType;
			entryInstruction.CEI_CustomsOffice = "BT";
			var invoiceHeader = decl.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 2000m;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_Tariff = "87031000002";
			invoiceLine.JI_CountryOfOrigin = "TW";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_InvoiceQuantity = 3;
			invoiceLine.JI_EnteredUnitPrice = 562;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			Factory.Save();
			new LineMerger(decl).DoMerge();

			testOperationalLog.messages.Clear();
			applicator.Apply(testOperationalLog, new BusinessObject[] { decl });
			AssertEquals("Last message", @"INFO: 1 job successfully printed.", testOperationalLog.messages.Last());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PrintBatchDocumentsOperationalActionMethodApplicator(JobDeclarationDocumentSupporter.DocumentName.ExportCustomsDeclarationInformal);
		}
	}
}
