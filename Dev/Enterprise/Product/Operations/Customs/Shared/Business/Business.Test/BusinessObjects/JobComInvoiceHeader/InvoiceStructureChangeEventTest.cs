using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceStructureChangeEventTest : TestCaseWithFactory
	{
		public void TestInvoiceStructureChanged()
		{
			AssertNoExceptionThrown(delegate
			{ InvoiceStructureChangeEvent.OnInvoiceStructureChanged(Factory); });//null checking

			int index = 0;

			EventHandler handler = new EventHandler(delegate
			{ index++; });
			InvoiceStructureChangeEvent.AddInvoiceStructureChangedEventHandler(Factory, handler);
			InvoiceStructureChangeEvent.OnInvoiceStructureChanged(Factory);
			AssertEquals("index should have been increased", 1, index);

			InvoiceStructureChangeEvent.RemoveInvoiceStructureChangedEventHandler(Factory, handler);
			InvoiceStructureChangeEvent.OnInvoiceStructureChanged(Factory);
			AssertEquals("index should have stayed", 1, index);
		}
	}
}
