using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingInvoiceItemCollection : NonPersistentBusinessObjectCollection<SterlingInvoiceItem>
	{
		public SterlingInvoiceItemCollection(SterlingInvoice master)
		{
			this.Master = master;
			UpdateCollection();
		}
		readonly SterlingInvoice Master;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SterlingInvoiceItem();
		}

		public void UpdateCollection()
		{
			foreach (Xsd.TxnLine iIToAdd in Master.Source.TxnLines)
			{
				SterlingInvoiceItem iItem = this.AddNew();
				iItem.Source = iIToAdd;
			}
		}
	}
}

