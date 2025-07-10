using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsOrderExportToFileMenuItemIFS : WhsDocketExportToFileMenuItem<WhsOrder, Xsd.ConNote>
	{
		public WhsOrderExportToFileMenuItemIFS(WhsOrder order, WhsOrderCartageValueObjectDataAdapterIFS adapter)
			: base(order, adapter)
		{
		}

		public WhsOrderExportToFileMenuItemIFS(WhsOrder order)
			: this(order, null)
		{
		}

		#region Overrides

		protected override WhsXmlExportDirector<WhsOrder, Xsd.ConNote> GetNewDirector()
		{
			return new WhsOrderCartageXmlExportToFileDirectorIFS((WhsOrderCartageValueObjectDataAdapterIFS)Adapter);
		}

		#endregion
	}
}
