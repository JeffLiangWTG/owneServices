using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingInvoiceItem))]
	public class SterlingInvoiceItemTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingInvoiceItem();
		}

		public void TestSterlingInvoiceItemRecord()
		{
			AssertEquals("Parsed record is different from expected", ExpectedSterlingInvoiceItemRecord1, SterlingForTest.InvoiceInfo[0].InvoiceItemInfo[0].Record);
		}
		const string ExpectedSterlingInvoiceItemRecord1 = "IIT|CST|2|IIChargeCode|IIChargeGroup|IIChargeCodeSalesGroup|IIChargeCodeExpenseGroup|IIDescription|12.1|USD|13.1|USD|1.1|USD|2.1|AUD|2|USD|2.21|SGD|13.1|USD|21.1|AUD|Cartage>\r\n";
	}
}
