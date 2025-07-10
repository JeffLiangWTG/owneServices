using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryPayInfoBulkUpdateBusinessObject))]
	sealed class CusEntryPayInfoBulkUpdateBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCusEntryPayInfoBulkUpdateBusinessObject_Save()
		{
			var receipt = "B9272091-FE93-4ABA-B";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + UniversalReferenceConstants.ProcedureCodes._00;
			new LineMerger(declaration).DoMerge();
			var header = declaration.ActiveEntryHeaders[0];
			Factory.Save();
			var payInfo1 = Factory.New<CusEntryPayInfo>();
			payInfo1.C9_CH = header.PK;
			var payInfo2 = Factory.New<CusEntryPayInfo>();
			payInfo2.C9_CH = header.PK;
			Factory.Save();
			Assert("Receipt Number 1 Empty", payInfo1.C9_PaymentReference.IsEmpty);
			Assert("Receipt Number 2 Empty", payInfo2.C9_PaymentReference.IsEmpty);
			var dummyParent = new CusEntryPayInfoBulkUpdateBusinessObject(Factory);
			dummyParent.ReceiptNumber = receipt;
			dummyParent.SelectedPayInfos.Add(payInfo1);
			dummyParent.SelectedPayInfos.Add(payInfo2);
			dummyParent.SaveForTest();
			AssertEquals("Receipt Number 1 Check", receipt, payInfo1.C9_PaymentReference);
			AssertEquals("Receipt Number 2 Check", receipt, payInfo2.C9_PaymentReference);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusEntryPayInfoBulkUpdateBusinessObject(Factory);
	}
}
