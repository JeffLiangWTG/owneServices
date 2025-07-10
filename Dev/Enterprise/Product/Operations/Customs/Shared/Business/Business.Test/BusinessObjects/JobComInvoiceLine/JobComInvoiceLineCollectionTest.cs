using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceLineViewCollection))]
	sealed class JobComInvoiceLineCollectionTest : BusinessObjectCollectionViewTestCase<BaseJobComInvoiceLineViewCollection>
	{
		public void TestNoRowNotInTableExceptionThrownWhenAddingLineForDeletedInvoice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.Delete();

			AssertNoExceptionThrown(() => invoice.JobComInvoiceLines.AddNew());
		}

		public void TestLoadStmNoteFetchHintIfNeeded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLines = invoice1.JobComInvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			var invoiceLine2 = invoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			var count = Factory.ActiveTableFetchHints;
			var stmNoteCount = Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName);
			invoiceLines.LoadStmNoteFetchHintIfNeeded();
			AssertEquals(count + 1, Factory.ActiveTableFetchHints);
			AssertEquals(stmNoteCount + 2, Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));
			invoiceLines.LoadStmNoteFetchHintIfNeeded();
			AssertEquals("Should not have changed", count + 1, Factory.ActiveTableFetchHints);
			AssertEquals("Should not have changed", stmNoteCount + 2, Factory.ActiveFetchHintsForTable(StmNoteSchema.Constants.TableName));
		}

		public void TestIAllInvoiceLines()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();

			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();

			AssertEquals("Invoice1 Contains Line1", true, ((IAllInvoiceLines)invoice1.JobComInvoiceLines).Contains(line1));
			AssertEquals("invoice1 does not Contain Line2", false, ((IAllInvoiceLines)invoice1.JobComInvoiceLines).Contains(line2));
		}

		protected override BaseJobComInvoiceLineViewCollection GetCollectionToTest()
		{
			return new BaseJobComInvoiceLineViewCollection(InvoiceHeader, new InvoiceLineCompleteCollection(Declaration));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceLine result = Factory.New<BaseJobComInvoiceLine>();
			result.JI_JZ = InvoiceHeader.PK;
			return result;
		}

		BaseJobDeclaration fDeclaration;
		BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = BaseJobDeclaration.New(Factory);
				}
				return fDeclaration;
			}
		}

		BaseJobComInvoiceHeader fInvoiceHeader;
		BaseJobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null)
				{
					fInvoiceHeader = Declaration.Invoices.AddNew();
				}
				return fInvoiceHeader;
			}
		}
	}
}
