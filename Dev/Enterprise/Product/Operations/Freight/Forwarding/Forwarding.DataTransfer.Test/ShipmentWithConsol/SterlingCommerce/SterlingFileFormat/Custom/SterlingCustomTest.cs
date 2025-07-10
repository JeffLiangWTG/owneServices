using CargoWise.EntityFramework;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingCustom))]
	public class SterlingCustomTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingCustom(new SterlingCommerceConsolAndShipmentExporter(new BusinessObjectFactory(), new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo()));
		}

		public void TestSterlingCustomRecord()
		{
			AssertEquals("Parsed record is different from expected", ExpectedSterlingCustomRecord1, SterlingForTest.CustomInfo.Record);
		}
		const string ExpectedSterlingCustomRecord1 = "CST|CustomAttribute1|CustomAttribute2|2008-02-02 03:03:03 +11:00|2008-03-03 04:04:04 +11:00|1|2|true|false>\r\n";
	}
}
