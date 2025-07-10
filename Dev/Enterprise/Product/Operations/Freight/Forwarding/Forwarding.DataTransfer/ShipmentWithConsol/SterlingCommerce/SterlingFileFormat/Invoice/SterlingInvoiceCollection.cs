using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingInvoiceCollection : NonPersistentBusinessObjectCollection<SterlingInvoice>
	{
		public SterlingInvoiceCollection(SterlingCommerceConsolAndShipmentExporter master)
		{
			this.Master = master;
			UpdateCollection();
		}
		readonly SterlingCommerceConsolAndShipmentExporter Master;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SterlingInvoice();
		}

		public void UpdateCollection()
		{
			if (Master.Shipment.ARInvoices.Count > 0)
			{
				foreach (Xsd.TxnHeader invoiceToAdd in Master.Shipment.ARInvoices)
				{
					if (Count < 20 && Master.Interchange.Source.Purpose != "EVT")
					{
						SterlingInvoice invoice = this.AddNew();
						invoice.Source = invoiceToAdd;
					}
				}
			}
		}
	}
}

