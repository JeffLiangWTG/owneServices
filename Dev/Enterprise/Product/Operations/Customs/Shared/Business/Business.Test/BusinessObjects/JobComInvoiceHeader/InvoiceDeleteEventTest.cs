using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceDeleteEventTest : TestCaseWithFactory
	{
		public void TestWhenInvoiceDeleted()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			AssertNoExceptionThrown(delegate
			{ InvoiceDeleteEvent.OnInvoiceDeleted(Factory); });//null checking

			int index = 0;

			EventHandler handler = new EventHandler(delegate
			{ index++; });
			InvoiceDeleteEvent.AddInvoiceDeletedEventHandler(Factory, handler);

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals("index should stay", 0, index);

			invoice.Delete();
			AssertEquals("index should have been increased", 1, index);
		}

		public void TestWhenEventHandlerIsRemoved()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			AssertNoExceptionThrown(delegate
			{ InvoiceDeleteEvent.OnInvoiceDeleted(Factory); });//null checking

			int index = 0;

			var handler = new EventHandler(delegate
			{ index++; });
			InvoiceDeleteEvent.AddInvoiceDeletedEventHandler(Factory, handler);

			var invoice = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals("index should stay", 0, index);

			invoice.Delete();
			AssertEquals("index should have been increased", 1, index);

			InvoiceDeleteEvent.RemoveInvoiceDeletedEventHandler(Factory, handler);
			AssertEquals("index should NOT have been increased as event handler is removed", 1, index);
		}
	}
}
