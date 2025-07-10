using CargoWise.EntityFramework;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingShipment))]
	public class SterlingShipmentTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingShipment(new SterlingCommerceConsolAndShipmentExporter(new BusinessObjectFactory(), new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo()));
		}

		public void TestSterlingShipmentRecord()
		{
			SterlingShipment parsedXMLShipment = new SterlingShipment(SterlingForTest);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingShipmentRecord, parsedXMLShipment.Record);
		}
		const string ExpectedSterlingShipmentRecord = "SHP|12345643|HOUSEBILL|2008-12-31 23:45:59 +11:00|AIR|LSE|AUSYD|Australia|Sydney|USCHI|United States|Chicago|Status|2|BOX|1|CTN|GoodsDescription|123|KG|120|KG|200|D3|1330.2|AUD|STD|CPT|CSH|AgentRef|SHP|MarksAndNums|Owner Reference|Book Ref|InterchangeRegNo|2008-02-12 09:00:00 +11:00|2008-02-14 09:00:00 +11:00|2008-02-13 09:00:00 +11:00|2008-02-15 09:00:00 +11:00|2008-01-12 09:00:00 +11:00|2008-01-14 09:00:00 +11:00|2008-01-13 09:00:00 +11:00|2008-02-15 09:00:00 +11:00|2008-03-15 09:05:00 +11:00|2008-12-30 09:05:00 +11:00|2008-02-01 09:00:00 +11:00>\r\n";
	}
}
