using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class LineNumberAssignerForIMXTest : TestCaseWithFactory
	{
		public void TestLineNumberAssignerForImx()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testInvoice1 = declaration.Invoices.AddNew();
			testInvoice1.JZ_InvoiceNumber = "INV3";
			testInvoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine11 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = testInst.PK;
			invoiceLine11.JI_Tariff = "1";
			invoiceLine11.JI_PreviousEntryLineNumber = 10;
			var invoiceLine12 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine12.JI_CEI = testInst.PK;
			invoiceLine12.JI_Tariff = "1";
			invoiceLine12.JI_PreviousEntryLineNumber = 11;
			testInst.CEI_PreviousMRN = "JBH201706301234567";

			declaration.DoMerge();

			AssertEquals(2, declaration.ActiveEntryHeaders[0].MergedLines.Count);

			new LineNumberAssignerForIMX(declaration.ActiveEntryHeaders[0]).Execute();
			AssertEquals((ZShort)10, invoiceLine11.CusEntryLine.CL_LineNumber);
			AssertEquals((ZShort)11, invoiceLine12.CusEntryLine.CL_LineNumber);
		}
	}
}
