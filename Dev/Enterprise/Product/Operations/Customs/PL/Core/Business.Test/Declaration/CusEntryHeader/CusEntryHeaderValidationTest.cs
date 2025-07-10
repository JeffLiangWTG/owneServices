using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class CusEntryHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAllMaximumCollectionsAmounts_EntryLines()
	{
		const string messageError = "Entry Lines count exceeds maximum of 999 - create a new Entry Instruction";
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();

		for (var i = 0; i < Constants.MaximumBusinessObjectsAmounts.MaximumEntryLines; i++)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = $"Empty invoiceLine {i}";
			invoiceLine.JI_CEI = instruction.PK;
		}
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		var entryHeader = declaration.CustomsEntryHeaders.First();
		CombineAssertions(() =>
		{
			AssertEquals("999 invoiceLines", 999, entryHeader.AllEntryLines.Count);
			entryHeader.Validation.ValidateAll();
			AssertNoRowMessageError("Entry Header with 999 entry lines", entryHeader, messageError);

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Empty invoiceLine 1000";
			invoiceLine.JI_CEI = instruction.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryHeader = declaration.CustomsEntryHeaders.First();
			AssertEquals("1000 invoiceLines", 1000, entryHeader.AllEntryLines.Count);
			entryHeader.Validation.ValidateAll();
			AssertHasRowMessageError("Entry Header with 1000 entry lines", entryHeader, messageError);
		});
	}
}
