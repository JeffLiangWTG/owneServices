using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(US7501DocPrintingCollection))]
	sealed class US7501DocPrintingCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDeleteCollection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MWB123456";
			JobComInvoiceHeader invHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invLine = declaration.InvoiceLines.AddNew();
			invLine.JI_JZ = invHeader.PK;
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			CusEntryLine entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_CH = entry.PK;
			CusEntryLine entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_CH = entry.PK;
			entry.CreateDocPrintingDetails(new ZGuid());
			AssertEquals("US7501DocPrintingCollection count", 2, entry.US7501DocPrintingData.Count);
			entry.US7501DocPrintingData.RemoveAndDeleteAll();
			AssertEquals("US7501DocPrintingCollection count", 0, entry.US7501DocPrintingData.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			return new US7501DocPrintingCollection(entryHeader);
		}
	}
}
