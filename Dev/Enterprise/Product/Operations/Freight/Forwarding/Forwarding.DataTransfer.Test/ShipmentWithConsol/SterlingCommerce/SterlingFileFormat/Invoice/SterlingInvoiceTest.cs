using CargoWise.EntityFramework;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingInvoice))]
	public class SterlingInvoiceTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			SterlingInvoice result = new SterlingInvoice();
			result.Source = new Xsd.TxnHeader();
			return result;
		}

		public void TestSterlingInvoiceRecord()
		{
			AssertEquals("Parsed record is different from expected", ExpectedSterlingInvoiceRecord1, SterlingForTest.InvoiceInfo[0].Record);
		}
		const string ExpectedSterlingInvoiceRecord1 = "INV|InvEDICode|ADJ|2|InvTxnNumber|InvJobInvoiceNo|InvDescription|2008-01-01 00:00:00 +11:00|InvTerm1|InvTermDays1|2008-03-03 00:00:00 +11:00|2008-04-04 00:00:00 +11:00|InvBranch1|Department1|123|AUD|321|AUD|12.1|USD|12.2|USD|13.5|SGD|14|SGD|12|RUB|13|RUB|Y|USERID>\r\n";
	}
}
