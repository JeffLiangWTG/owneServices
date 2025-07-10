using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(US7501DocPrintingAddInfo))]
	sealed class US7501DocPrintingAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateDocDataDetails()
		{
			JobComInvoiceLine invLine = Declaration.InvoiceLines.AddNew();
			invLine.JI_JZ = InvoiceHeader.PK;
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_CH = entry.PK;
			entryLine.CL_LineNumber = 1;
			entry.CreateDocPrintingDetails(new ZGuid());
			US7501DocPrinting docData = entry.US7501DocPrintingData[0];
			AssertEquals("Free", docData.US_RateAsString);
			AssertEquals(1, docData.US_LineNo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobComInvoiceLine invLine = Declaration.InvoiceLines.AddNew();
			invLine.JI_JZ = InvoiceHeader.PK;
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_CH = entry.PK;
			entry.CreateDocPrintingDetails(new ZGuid());
			US7501DocPrinting docData = entry.US7501DocPrintingData[0];
			return new US7501DocPrintingAddInfo(docData.B7_AddInfoDataInfo);
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceHeader InvoiceHeader => invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
	}
}
