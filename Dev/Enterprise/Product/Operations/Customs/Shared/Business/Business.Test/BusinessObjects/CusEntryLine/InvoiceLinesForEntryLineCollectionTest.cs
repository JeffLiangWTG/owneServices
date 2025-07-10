using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceLinesForEntryLineCollection))]
	public class InvoiceLinesForEntryLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestInvoiceLinesForEntryLine()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();

			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			invoiceLine.JI_CL = entryLine1.PK;
			AssertEquals(1, entryLine1.InvoiceLines.Count);
			AssertEquals(0, entryLine2.InvoiceLines.Count);
		}

		public void TestRebuildOnContruction()
		{
			BaseJobComInvoiceLine invoiceLine1 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = EntryLine.PK;

			BaseJobComInvoiceLine invoiceLine2 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = EntryLine.PK;

			CusEntryLine entryLine2 = EntryHeader.MergedLines.AddNew();
			BaseJobComInvoiceLine invoiceLine3 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;

			InvoiceLinesForEntryLineCollection testCollectionForEntryLine1 = new InvoiceLinesForEntryLineCollection(EntryLine);
			testCollectionForEntryLine1.Load();
			AssertEquals("Test collection1", 2, testCollectionForEntryLine1.Count);

			InvoiceLinesForEntryLineCollection testCollectionForEntryLine2 = new InvoiceLinesForEntryLineCollection(entryLine2);
			testCollectionForEntryLine2.Load();
			AssertEquals("Test collection2 is rebuilt on Contruction", 1, testCollectionForEntryLine2.Count);
		}

		#region Implementation

		BaseJobDeclaration fTestDec;
		protected BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = BaseJobDeclaration.New(Factory);
				}
				return fTestDec;
			}
		}

		BaseJobComInvoiceHeader fInvoice;
		protected BaseJobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = TestDec.Invoices.AddNew();
				}
				return fInvoice;
			}
		}

		CusEntryHeader fEntryHeader;
		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = TestDec.CustomsEntryHeaders.AddNew();
				}
				return fEntryHeader;
			}
		}

		CusEntryLine fEntryLine;
		protected CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					fEntryLine = EntryHeader.MergedLines.AddNew();
				}
				return fEntryLine;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceLine invoiceLine = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = EntryLine.PK;
			if (TestDec.InvoiceLines.Contains(invoiceLine))
			{
				TestDec.InvoiceLines.Remove(invoiceLine);
			}

			return invoiceLine;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLinesForEntryLineCollection(EntryLine);
		}
		#endregion
	}
}
