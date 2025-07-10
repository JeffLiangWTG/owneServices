using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	sealed class JobComInvoiceLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		public void TestTypedIndexer()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			InvoiceLineCompleteCollection lineCollection = new InvoiceLineCompleteCollection(declaration);
			JobComInvoiceLineViewCollection collection = new JobComInvoiceLineViewCollection(header, lineCollection);
			JobComInvoiceLine invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		protected override JobComInvoiceLineViewCollection GetCollectionToTest()
		{
			return Invoice.JobComInvoiceLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = Invoice.PK;
			if (Invoice.JobComInvoiceLines.Contains(invoiceLine))
			{
				Invoice.JobComInvoiceLines.Remove(invoiceLine);
			}

			return invoiceLine;
		}

		#region TestDec
		JobDeclaration TestDec
		{
			get
			{
				if (testDec == null)
				{
					testDec = Factory.New<JobDeclaration>();
				}

				return testDec;
			}
		}

		JobDeclaration testDec;
		#endregion

		#region Invoice
		JobComInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = TestDec.Invoices.AddNew();
				}

				return invoice;
			}
		}

		JobComInvoiceHeader invoice;
		#endregion
	}
}
