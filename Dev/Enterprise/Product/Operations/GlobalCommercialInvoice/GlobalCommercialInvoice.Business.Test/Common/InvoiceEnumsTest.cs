using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(InvoiceEnumsTest))]
	public class InvoiceEnumsTest : TestCase
	{
		public void TestInvoiceOrgHeaderIndexIntegerValue()
		{
			AssertEquals(0, (int)InvoiceOrgHeaderIndex.Supplier);
			AssertEquals(1, (int)InvoiceOrgHeaderIndex.Importer);
		}
	}
}
