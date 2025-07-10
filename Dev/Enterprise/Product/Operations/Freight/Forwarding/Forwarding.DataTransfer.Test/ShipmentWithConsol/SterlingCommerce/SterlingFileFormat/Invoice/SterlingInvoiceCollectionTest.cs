using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingInvoiceCollection))]
	class SterlingInvoiceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SterlingInvoiceCollection>
	{
		#region Test Overrides

		protected override SterlingInvoiceCollection GetCollectionToTest()
		{
			SterlingCommerceConsolAndShipmentExporter master = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo());
			return new SterlingInvoiceCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SterlingInvoice();
		}

		#endregion

		public void TestSterlingInvoiceCollection()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.TxnHeader inv = shipment.ARInvoices.AddNew();
			inv = shipment.ARInvoices.AddNew();
			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals(2, sterling.InvoiceInfo.Count);
		}

		public void TestCollectionWithEVTPurposeType()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.TxnHeader inv = shipment.ARInvoices.AddNew();
			inv = shipment.ARInvoices.AddNew();
			Xsd.InterchangeInfo interchange = new Xsd.InterchangeInfo();
			interchange.Source.Purpose = "EVT";
			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, interchange);
			AssertEquals(0, sterling.InvoiceInfo.Count);
		}
	}
}
