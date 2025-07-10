using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryPayInfoBulkUpdateCollection))]
	sealed class CusEntryPayInfoBulkUpdateCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddPayInfoToBulkUpdate()
		{
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
			var collection = new CusEntryPayInfoBulkUpdateCollection(Factory);
			AssertEquals("Initial Collection Size", 0, collection.Count);
			collection.AddRange(payInfo1);
			AssertEquals("Pay Info Added", 1, collection.Count);
			collection.AddRange(payInfo2);
			AssertEquals("Another Pay Info Added", 2, collection.Count);
			collection.AddRange(payInfo1);
			AssertEquals("Duplicate Pay Info Not Allowed", 2, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryPayInfoBulkUpdateCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusEntryPayInfo>();
	}
}
