using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	[TestedType(typeof(WhsInvoiceCollection))]
	internal class WhsInvoiceCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		public void TestCountInvoicesToInclude()
		{
			var invoice1 = Factory.New<WhsInvoice>();
			var invoice2 = Factory.New<WhsInvoice>();
			var invoice3 = Factory.New<WhsInvoice>();

			var invoiceCollection = new WhsInvoiceCollection(Factory);
			invoiceCollection.Add(invoice1);
			invoiceCollection.Add(invoice2);
			invoiceCollection.Add(invoice3);

			AssertEquals(3, invoiceCollection.CountInvoicesToInclude);
			invoice2.IncludeInInvoicing = false;
			AssertEquals(2, invoiceCollection.CountInvoicesToInclude);
			invoice1.IncludeInInvoicing = false;
			AssertEquals(1, invoiceCollection.CountInvoicesToInclude);
			invoice3.IncludeInInvoicing = false;
			AssertEquals(0, invoiceCollection.CountInvoicesToInclude);
		}

		public void TestIndexOf()
		{
			var invoice1 = Factory.New<WhsInvoice>();
			var invoice2 = Factory.New<WhsInvoice>();
			var invoice3 = Factory.New<WhsInvoice>();

			var invoiceCollection = new WhsInvoiceCollection(Factory);
			invoiceCollection.Add(invoice1);
			invoiceCollection.Add(invoice2);
			invoiceCollection.Add(invoice3);

			AssertEquals(0, invoiceCollection.IndexOf(invoice1));
			AssertEquals(2, invoiceCollection.IndexOf(invoice3));
			AssertEquals(1, invoiceCollection.IndexOf(invoice2));
		}

		public void TestAdditionalFilter()
		{
			var invoice1 = Factory.New<WhsInvoice>();
			var invoice2 = Factory.New<WhsInvoice>();
			var invoice3 = Factory.New<WhsInvoice>();

			invoice1.ET_StorageType = WhsInvoice.StorageType;
			invoice2.ET_StorageType = "###";
			invoice3.ET_StorageType = WhsInvoice.StorageType;

			var invoiceCollection = new WhsInvoiceCollection(Factory);
			invoiceCollection.Load();

			Assert(invoiceCollection.Contains(invoice1));
			Assert(!invoiceCollection.Contains(invoice2));
			Assert(invoiceCollection.Contains(invoice3));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsInvoiceCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<WhsInvoice>();
		}
	}
}
