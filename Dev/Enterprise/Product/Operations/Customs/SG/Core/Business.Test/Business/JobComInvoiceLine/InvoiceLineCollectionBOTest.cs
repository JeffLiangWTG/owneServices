using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(InvoiceLineViewCollection))]
	public class InvoiceLineCollectionBOTest : Customs.Business.Testing.InvoiceLineCollectionBOTest<InvoiceLineViewCollection>
	{
		public void TestAllowNew()
		{
			InvoiceLineViewCollection invoiceLineViewCollection = GetCollectionToTest();
			AssertEquals(true, invoiceLineViewCollection.AllowNew);
			invoiceLineViewCollection.NoAddingOrRemoving = true;
			AssertEquals(false, invoiceLineViewCollection.AllowNew);
			invoiceLineViewCollection.NoAddingOrRemoving = false;
			AssertEquals(true, invoiceLineViewCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			InvoiceLineViewCollection invoiceLineViewCollection = GetCollectionToTest();
			AssertEquals(true, invoiceLineViewCollection.AllowRemove);
			invoiceLineViewCollection.NoAddingOrRemoving = true;
			AssertEquals(false, invoiceLineViewCollection.AllowRemove);
			invoiceLineViewCollection.NoAddingOrRemoving = false;
			AssertEquals(true, invoiceLineViewCollection.AllowRemove);
		}

		public void TestSetDefaultsForNewChild()
		{
			InvoiceLineViewCollection invoiceLineViewCollection = GetCollectionToTest();
			JobComInvoiceLine invoiceLine = invoiceLineViewCollection.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine = invoiceLineViewCollection.AddNew();
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, invoiceLine.JI_CountryOfOrigin);
		}

		public void TestTypedIndexer()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			InvoiceLineViewCollection collection = new InvoiceLineViewCollection(declaration);
			JobComInvoiceLine invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		#region Overrides
		protected override InvoiceLineViewCollection GetCollectionToTest()
		{
			JobComInvoiceHeader invoiceAccessed = Invoice;
			return new InvoiceLineViewCollection(JobDeclaration);
		}

		protected new JobDeclaration JobDeclaration
		{
			get
			{
				return (JobDeclaration)base.JobDeclaration;
			}
		}

		protected new JobComInvoiceHeader Invoice
		{
			get
			{
				return (JobComInvoiceHeader)base.Invoice;
			}
		}
		#endregion
	}
}
