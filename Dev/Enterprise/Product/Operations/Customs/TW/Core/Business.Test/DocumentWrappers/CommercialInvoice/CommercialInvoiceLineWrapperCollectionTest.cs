using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CommercialInvoiceLineWrapperCollection))]
	sealed class CommercialInvoiceLineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommercialInvoiceLineWrapperCollection>
	{
		public void TestLoadFromInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INVOICE";

			var collection = new CommercialInvoiceLineWrapperCollection(Factory);
			collection.AddLinesFrom(invoiceHeader.JobComInvoiceLines);
			AssertEquals("collection.Count", 0, collection.Count);

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();

			collection = new CommercialInvoiceLineWrapperCollection(Factory);
			collection.AddLinesFrom(invoiceHeader.JobComInvoiceLines);
			AssertEquals("collection.Count", 3, collection.Count);
			AssertEquals("collection[0].LineNo", (ZShort)1, collection[0].LineNo);
			AssertEquals("collection[1].LineNo", (ZShort)2, collection[1].LineNo);
			AssertEquals("collection[2].LineNo", (ZShort)3, collection[2].LineNo);
		}

		#region Imeplementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLineBO = CommercialInvoiceLineWrapper.New(invoiceLine, Factory);
			return invoiceLineBO;
		}

		protected override CommercialInvoiceLineWrapperCollection GetCollectionToTest()
		{
			return new CommercialInvoiceLineWrapperCollection(Factory);
		}

		#endregion
	}
}
