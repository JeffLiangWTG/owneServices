using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InvoiceLineParentChildComparerTest : TestCaseWithFactory
	{
		public void TestInvoiceLinesAreOrderedCorrectly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("invoice line 2.JI_LineNo > invoiceLine.JI_LineNo", (short)1, invoiceLine.JI_LineNo);
			AssertEquals("invoice line 2.JI_LineNo > invoiceLine.JI_LineNo", (short)2, invoiceLine2.JI_LineNo);

			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine.JI_ParentID = invoiceLine2.PK;

			List<JobComInvoiceLine> list = new List<JobComInvoiceLine>(new TypedEnumerable<JobComInvoiceLine>(declaration.InvoiceLines));
			list.Sort(new InvoiceLineParentChildComparer());

			AssertEquals("InvoiceLine2 should come first", invoiceLine2, list[0]);
			AssertEquals("And then InvoiceLine, as it is a child", invoiceLine, list[1]);
		}

		public void TestMergeLinesInOrderOfInvoiceSequence()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			AssertEquals("PreCondition", (short)1, invoice1.JZ_InvoiceDisplaySequence);
			AssertEquals("PreCondition", (short)2, invoice2.JZ_InvoiceDisplaySequence);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();

			invoiceLine2.JI_JZ = invoice2.PK;
			invoiceLine4.JI_JZ = invoice2.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("lines of an invoice should be sent side by side", (short)1, invoiceLine.CusEntryLine.CL_LineNumber);
			AssertEquals("lines of an invoice should be sent side by side", (short)3, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals("lines of an invoice should be sent side by side", (short)2, invoiceLine3.CusEntryLine.CL_LineNumber);
			AssertEquals("lines of an invoice should be sent side by side", (short)4, invoiceLine4.CusEntryLine.CL_LineNumber);
		}
	}
}
