using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class LineNumberAssignerTest : TestCaseWithFactory
{
	public void TestComprehensiveTest()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
		var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		testInst.CEI_SubStyle = "X";
		var testInvoice1 = declaration.Invoices.AddNew();
		testInvoice1.JZ_InvoiceNumber = "INV3";
		testInvoice1.JZ_RX_NKInvoice_Currency = "PLN";
		var invoiceLine11 = testInvoice1.InvoiceLines.AddNew();
		invoiceLine11.JI_CEI = testInst.PK;
		invoiceLine11.JI_Tariff = "1";
		var invoiceLine12 = testInvoice1.InvoiceLines.AddNew();
		invoiceLine12.JI_CEI = testInst.PK;
		invoiceLine12.JI_Tariff = "1";
		var invoiceLine13 = testInvoice1.InvoiceLines.AddNew();
		invoiceLine13.JI_CEI = testInst.PK;
		invoiceLine13.JI_Tariff = "1";
		var testInvoice2 = declaration.Invoices.AddNew();
		testInvoice2.JZ_InvoiceNumber = "INV1";
		testInvoice2.JZ_RX_NKInvoice_Currency = "PLN";
		var invoiceLine21 = testInvoice2.InvoiceLines.AddNew();
		invoiceLine21.JI_CEI = testInst.PK;
		invoiceLine21.JI_Tariff = "2";
		var invoiceLine22 = testInvoice2.InvoiceLines.AddNew();
		invoiceLine22.JI_CEI = testInst.PK;
		invoiceLine22.JI_Tariff = "2";
		var invoiceLine23 = testInvoice2.InvoiceLines.AddNew();
		invoiceLine23.JI_CEI = testInst.PK;
		invoiceLine23.JI_Tariff = "2";
		var testInvoice3 = declaration.Invoices.AddNew();
		testInvoice3.JZ_InvoiceNumber = "INV2";
		testInvoice3.JZ_RX_NKInvoice_Currency = "PLN";
		var invoiceLine31 = testInvoice3.InvoiceLines.AddNew();
		invoiceLine31.JI_CEI = testInst.PK;
		invoiceLine31.JI_Tariff = "2";
		var invoiceLine32 = testInvoice3.InvoiceLines.AddNew();
		invoiceLine32.JI_CEI = testInst.PK;
		invoiceLine32.JI_Tariff = "2";
		var invoiceLine33 = testInvoice3.InvoiceLines.AddNew();
		invoiceLine33.JI_CEI = testInst.PK;
		invoiceLine33.JI_Tariff = "2";
		invoiceLine33.JI_LineNo = 1;
		invoiceLine32.JI_LineNo = 3;
		invoiceLine31.JI_LineNo = 2;

		invoiceLine11.JI_TargetEntryLineNumber = 7;
		invoiceLine12.JI_TargetEntryLineNumber = 0;
		invoiceLine13.JI_TargetEntryLineNumber = 8;
		invoiceLine21.JI_TargetEntryLineNumber = 0;
		invoiceLine22.JI_TargetEntryLineNumber = 5;
		invoiceLine23.JI_TargetEntryLineNumber = 6;
		invoiceLine31.JI_TargetEntryLineNumber = 0;
		invoiceLine32.JI_TargetEntryLineNumber = 0;
		invoiceLine33.JI_TargetEntryLineNumber = 0;

		var field = typeof(JobDeclaration).GetField("isSupplementaryDeclaration", BindingFlags.NonPublic | BindingFlags.Instance);
		field!.SetValue(declaration, true);

		declaration.DoMerge();

		var header = declaration.CustomsEntryHeaders[0];

		AssertEquals(9, header.MergedLines.Count);

		CombineAssertions(() =>
		{
			AssertEquals("linked_11", new ZShort(7), invoiceLine11.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_12", new ZShort(13), invoiceLine12.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_13", new ZShort(08), invoiceLine13.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_21", new ZShort(09), invoiceLine21.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_22", new ZShort(05), invoiceLine22.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_23", new ZShort(06), invoiceLine23.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_31", new ZShort(11), invoiceLine31.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_32", new ZShort(12), invoiceLine32.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_33", new ZShort(10), invoiceLine33.CusEntryLine.CL_LineNumber);
		});
	}

	public void TestGetAutoNumberAsPerNormal()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
		var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		testInst.CEI_SubStyle = "X";
		var testInvoice1 = declaration.Invoices.AddNew();
		testInvoice1.JZ_InvoiceNumber = "INV3";
		var invoiceLine1 = testInvoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = testInst.PK;
		invoiceLine1.JI_Tariff = "1";
		var testInvoice2 = declaration.Invoices.AddNew();
		testInvoice2.JZ_InvoiceNumber = "INV1";
		var invoiceLine2 = testInvoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = testInst.PK;
		invoiceLine2.JI_Tariff = "2";
		var testInvoice3 = declaration.Invoices.AddNew();
		testInvoice3.JZ_InvoiceNumber = "INV2";
		var invoiceLine3 = testInvoice3.InvoiceLines.AddNew();
		invoiceLine3.JI_CEI = testInst.PK;
		invoiceLine3.JI_Tariff = "3";
		var field = typeof(JobDeclaration).GetField("isSupplementaryDeclaration", BindingFlags.NonPublic | BindingFlags.Instance);
		field!.SetValue(declaration, true);
		declaration.DoMerge();

		var header = declaration.CustomsEntryHeaders[0];
		var line1 = header.MergedLines[0];
		var line2 = header.MergedLines[1];
		var line3 = header.MergedLines[2];

		CombineAssertions(() =>
		{
			AssertEquals("Expected first merged line to be assigned line number 1.", new ZShort(1), line1.CL_LineNumber);
			AssertEquals("Expected second merged line to be assigned line number 2.", new ZShort(2), line2.CL_LineNumber);
			AssertEquals("Expected third merged line to be assigned line number 3.", new ZShort(3), line3.CL_LineNumber);
			AssertEquals("linked_1", new ZShort(3), invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_2", new ZShort(1), invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals("linked_3", new ZShort(2), invoiceLine3.CusEntryLine.CL_LineNumber);
		});
	}

	public void TestGetNumberFromInvoiceLineTargetEntry()
	{
		invoiceLine1.JI_TargetEntryLineNumber = 7;
		invoiceLine2.JI_TargetEntryLineNumber = 8;
		invoiceLine3.JI_TargetEntryLineNumber = 9;
		declaration.DoMerge();

		var header = declaration.CustomsEntryHeaders[0];
		var line1 = header.MergedLines[0];
		var line2 = header.MergedLines[1];
		var line3 = header.MergedLines[2];

		CombineAssertions(() =>
		{
			AssertEquals("Expected first merged line (with target 7) to retain line number 7.", new ZShort(7), line1.CL_LineNumber);
			AssertEquals("Expected second merged line (with target 8) to retain line number 8.", new ZShort(8), line2.CL_LineNumber);
			AssertEquals("Expected third merged line (with target 9) to retain line number 9.", new ZShort(9), line3.CL_LineNumber);
		});
	}

	public void TestAutoGenNumberStartingPoint()
	{
		invoiceLine1.JI_TargetEntryLineNumber = 0;
		invoiceLine2.JI_TargetEntryLineNumber = 0;
		invoiceLine3.JI_TargetEntryLineNumber = 4;
		declaration.DoMerge();

		var header = declaration.CustomsEntryHeaders[0];
		var line1 = header.MergedLines[0];
		var line2 = header.MergedLines[1];
		var line3 = header.MergedLines[2];

		CombineAssertions(() =>
		{
			AssertEquals("Expected first merged line (with target 4) to retain line number 4.", new ZShort(4), line1.CL_LineNumber);
			AssertEquals("Expected second merged line (auto-generated) to get line number 5.", new ZShort(5), line2.CL_LineNumber);
			AssertEquals("Expected third merged line (auto-generated) to get line number 6.", new ZShort(6), line3.CL_LineNumber);
			AssertEquals("InvoiceLine3 had a fixed target entry line number 4; it should retain it.", new ZShort(4), invoiceLine3.CusEntryLine.CL_LineNumber);
			AssertEquals("InvoiceLine1 had no target; should be assigned next available line number (5).", new ZShort(5), invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("InvoiceLine2 had no target; should be assigned next available line number (6).", new ZShort(6), invoiceLine2.CusEntryLine.CL_LineNumber);
		});
	}

	public void TestMixedAssignedAndAutoEntryLineNumberMergedCorrectly()
	{
		var testDeclaration = Factory.New<JobDeclaration>();
		testDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		testDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		testDeclaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
		var testInst = testDeclaration.CustomsEntryInstructions.AddNew();
		testInst.CEI_SubStyle = "X";
		testInst.CEI_Style = "11";
		var testInv = testDeclaration.Invoices.AddNew();
		var testInvLine1 = testInv.InvoiceLines.AddNew();
		testInvLine1.JI_TargetEntryLineNumber = 2;
		testInvLine1.JI_CEI = testInst.PK;
		testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";
		var testInvLine2 = testInv.InvoiceLines.AddNew();
		testInvLine2.JI_TargetEntryLineNumber = 3;
		testInvLine2.JI_CEI = testInst.PK;
		testInvLine2.JI_Procedure = testInvLine2.EntryInstruction.CEI_Style + "00";
		var testInvLine3 = testInv.InvoiceLines.AddNew();
		testInvLine3.JI_TargetEntryLineNumber = 5;
		testInvLine3.JI_CEI = testInst.PK;
		testInvLine3.JI_Procedure = testInvLine3.EntryInstruction.CEI_Style + "00";
		var testInvLine4 = testInv.InvoiceLines.AddNew();
		testInvLine4.JI_TargetEntryLineNumber = 0;
		testInvLine4.JI_CEI = testInst.PK;
		testInvLine4.JI_Procedure = testInvLine4.EntryInstruction.CEI_Style + "00";
		var testInvLine5 = testInv.InvoiceLines.AddNew();
		testInvLine5.JI_TargetEntryLineNumber = 0;
		testInvLine5.JI_CEI = testInst.PK;
		testInvLine5.JI_Procedure = testInvLine5.EntryInstruction.CEI_Style + "00";

		var field = typeof(JobDeclaration).GetField("isSupplementaryDeclaration", BindingFlags.NonPublic | BindingFlags.Instance);
		field!.SetValue(testDeclaration, true);

		testDeclaration.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals("Line 1: Should retain explicitly assigned TargetEntryLineNumber 2.", new ZShort(2), testInvLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 2: Should retain explicitly assigned TargetEntryLineNumber 3.", new ZShort(3), testInvLine2.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 3: Should retain explicitly assigned TargetEntryLineNumber 5.", new ZShort(5), testInvLine3.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 4: No target specified; should be auto-assigned next available number (6).", new ZShort(6), testInvLine4.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 5: No target specified; should be auto-assigned next available number (7).", new ZShort(7), testInvLine5.CusEntryLine.CL_LineNumber);
		});

		var testInvLine6 = testInv.InvoiceLines.AddNew();
		testInvLine6.JI_TargetEntryLineNumber = 0;
		testInvLine6.JI_CEI = testInst.PK;
		testInvLine6.JI_Procedure = testInvLine6.EntryInstruction.CEI_Style + "00";
		var testInvLine7 = testInv.InvoiceLines.AddNew();
		testInvLine7.JI_TargetEntryLineNumber = 0;
		testInvLine7.JI_CEI = testInst.PK;
		testInvLine7.JI_Procedure = testInvLine7.EntryInstruction.CEI_Style + "00";
		testDeclaration.DoMerge();

		CombineAssertions(() =>
		{
			AssertEquals("Line 1 (recheck): Should still retain line number 2.", new ZShort(2), testInvLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 2 (recheck): Should still retain line number 3.", new ZShort(3), testInvLine2.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 3 (recheck): Should still retain line number 5.", new ZShort(5), testInvLine3.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 4 (recheck): Should still be auto-assigned line number 6.", new ZShort(6), testInvLine4.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 5 (recheck): Should still be auto-assigned line number 7.", new ZShort(7), testInvLine5.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 6: Should be auto-assigned next available number (8).", new ZShort(8), testInvLine6.CusEntryLine.CL_LineNumber);
			AssertEquals("Line 7: Should be auto-assigned next available number (9).", new ZShort(9), testInvLine7.CusEntryLine.CL_LineNumber);
		});
	}

	protected override void SetUp()
	{
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

		var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		testInst.CEI_SubStyle = "X";

		var testInvoice = declaration.Invoices.AddNew();
		invoiceLine1 = testInvoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = testInst.PK;
		invoiceLine1.JI_Tariff = "1";

		invoiceLine2 = testInvoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = testInst.PK;
		invoiceLine2.JI_Tariff = "2";

		invoiceLine3 = testInvoice.InvoiceLines.AddNew();
		invoiceLine3.JI_CEI = testInst.PK;
		invoiceLine3.JI_Tariff = "3";

		var field = typeof(JobDeclaration).GetField("isSupplementaryDeclaration", BindingFlags.NonPublic | BindingFlags.Instance);
		field!.SetValue(declaration, true);
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;
	JobComInvoiceLine invoiceLine3;
}
