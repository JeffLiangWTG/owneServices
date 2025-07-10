using CargoWise.EntityFramework;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingHeader))]
	public class SterlingHeaderTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingHeader(new SterlingCommerceConsolAndShipmentExporter(new BusinessObjectFactory(), new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo()));
		}

		public void TestSterlingHeaderRecord()
		{
			AssertEquals("Parsed record is different from expected", ExpectedSterlingHeaderRecord1, SterlingForTest.HeaderInfo.Record);
		}
		const string ExpectedSterlingHeaderRecord1 = "H01|123321SenderCode|321123ReceiverCode|APP>\r\n";
	}
}
