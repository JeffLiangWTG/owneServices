using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingInvoiceItemCollection))]
	class SterlingInvoiceItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SterlingInvoiceItemCollection>
	{
		#region Test Overrides

		protected override SterlingInvoiceItemCollection GetCollectionToTest()
		{
			SterlingInvoice invoice = new SterlingInvoice();
			invoice.Source = new Xsd.TxnHeader();
			return new SterlingInvoiceItemCollection(invoice);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SterlingInvoiceItem();
		}

		#endregion

		public void TestSterlingInvoiceCollection()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.TxnHeader inv = shipment.ARInvoices.AddNew();
			Xsd.TxnLine line = inv.TxnLines.AddNew();

			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals(1, sterling.InvoiceInfo[0].InvoiceItemInfo.Count);

			line = inv.TxnLines.AddNew();

			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals(2, sterling.InvoiceInfo[0].InvoiceItemInfo.Count);
		}
	}
}
