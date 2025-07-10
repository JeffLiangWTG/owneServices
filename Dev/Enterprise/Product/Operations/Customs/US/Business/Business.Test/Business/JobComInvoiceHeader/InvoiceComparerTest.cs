using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class InvoiceComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			AssertEquals("PreCondition", (short)1, invoice1.JZ_InvoiceDisplaySequence);
			AssertEquals("PreCondition", (short)2, invoice2.JZ_InvoiceDisplaySequence);
			AssertEquals("PreCondition", (short)3, invoice3.JZ_InvoiceDisplaySequence);
			var list = new List<JobComInvoiceHeader>();
			list.Add(invoice3);
			list.Add(invoice1);
			list.Add(invoice2);
			list.Sort(new InvoiceComparer());
			AssertEquals(invoice1, list[0]);
			AssertEquals(invoice2, list[1]);
			AssertEquals(invoice3, list[2]);
		}
	}
}
