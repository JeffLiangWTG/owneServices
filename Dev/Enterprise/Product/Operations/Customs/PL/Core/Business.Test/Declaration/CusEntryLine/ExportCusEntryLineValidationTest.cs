using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportCusEntryLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRuleR0022E()
	{
		const string messageError = "(R0022E) Only one AAD previous document is allowed for Entry Line";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders.First();
		var entryLine = entryHeader.AllEntryLines.First();

		CombineAssertions(() =>
		{
			AssertNoRowMessageError("Empty declaration", entryLine, messageError);

			var previousDocument1 = declaration.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = PreviousDocumentCodes.AAD;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryLine = entryHeader.AllEntryLines.First(x => x.PreviousDocumentCount != 0);
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageError("1 previous document", entryLine, messageError);

			var previousDocument2 = declaration.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = PreviousDocumentCodes.CLE;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryLine = entryHeader.AllEntryLines.First(x => x.PreviousDocumentCount != 0);
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageError("2 different previous documents", entryLine, messageError);

			previousDocument2.CSI_Code = PreviousDocumentCodes.AAD;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryLine = entryHeader.AllEntryLines.First(x => x.PreviousDocumentCount != 0);
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageError("2 merged AAD previous documents", entryLine, messageError);

			previousDocument1.CSI_ReferenceNumber = "qwe";
			previousDocument2.CSI_ReferenceNumber = "asd";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryLine = entryHeader.AllEntryLines.First(x => x.PreviousDocumentCount != 0);
			entryLine.Validation.ValidateAll();
			AssertHasRowMessageError("2 not merged AAD previous documents", entryLine, messageError);

			previousDocument1.CSI_ReferenceNumber = "asd";
			previousDocument2.CSI_ReferenceNumber = "asd";
			var invoiceLinePreviousDocument2 = invoiceLine.PreviousDocuments.AddNew();
			invoiceLinePreviousDocument2.CSI_ReferenceNumber = "qwe";
			invoiceLinePreviousDocument2.CSI_Code = PreviousDocumentCodes.AAD;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryLine = entryHeader.AllEntryLines.First(x => x.PreviousDocumentCount != 0);
			entryLine.Validation.ValidateAll();
			AssertHasRowMessageError("2 merged and 1 not merged AAD previous documents", entryLine, messageError);

			invoiceLinePreviousDocument2.CSI_ReferenceNumber = "asd";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryLine = entryHeader.AllEntryLines.First(x => x.PreviousDocumentCount != 0);
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageError("3 merged AAD previous documents", entryLine, messageError);
		});
	}
}
