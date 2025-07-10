using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EntryLineComparerAccordingToInvoiceLineOrderTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = (short)2;
			invoiceLine2.JI_LineNo = (short)1;

			CusEntryHeader entry = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = entry.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			new LineNumberAssigner(entry).Execute();

			AssertEquals("InvoiceLineComparer sorts in order of invoice line+Line no", entryLine2, entry.MergedLines[0]);
			AssertEquals("InvoiceLineComparer sorts in order of invoice line+Line no", entryLine1, entry.MergedLines[1]);
		}

		[NUnit.Framework.ExpectNoExceptions()]
		public void TestComparerIfThereIsAnOrphanedEntryLine()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entry = testDec.CustomsEntryHeaders.AddNew();
			entry.MergedLines.AddNew();
			new LineNumberAssigner(entry).Execute();
		}

		public void TestSortedInADeterministicWay()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 10m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_LinePrice = 20m;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 30m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(2, declaration.ActiveEntryHeaders[0].MergedLines.Count);
			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);

			invoiceLine1.CusEntryLine.InvoiceLines.Sort(JobComInvoiceLineSchema.JI_LinePrice.Name, System.ComponentModel.ListSortDirection.Descending);
			invoiceLine2.CusEntryLine.InvoiceLines.Sort(JobComInvoiceLineSchema.JI_LinePrice.Name, System.ComponentModel.ListSortDirection.Descending);

			new LineNumberAssigner(declaration.ActiveEntryHeaders[0]).Execute();

			AssertEquals((ZShort)1, invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals((ZShort)2, invoiceLine2.CusEntryLine.CL_LineNumber);
		}

		public void TestShouldCompletelyReassignNumbers()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(true, entryHeader.ShouldCompletelyReassignNumbers);
			entryHeader.EntryNumber = "EN001";
			AssertEquals(false, entryHeader.ShouldCompletelyReassignNumbers);
		}
	}
}
