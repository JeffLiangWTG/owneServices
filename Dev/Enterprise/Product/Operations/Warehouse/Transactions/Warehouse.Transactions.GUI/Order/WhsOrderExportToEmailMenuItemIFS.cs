using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsOrderExportToEmailMenuItemIFS : WhsDocketExportToEmailMenuItem<WhsOrder, Xsd.ConNote>
	{
		public WhsOrderExportToEmailMenuItemIFS(WhsOrder order, WhsOrderCartageValueObjectDataAdapterIFS adapter)
			: base(order, adapter)
		{
		}

		#region Overrides

		protected override WhsXmlExportDirector<WhsOrder, Xsd.ConNote> GetNewDirector()
		{
			return new WhsOrderCartageXmlExportToEmailDirectorIFS((WhsOrderCartageValueObjectDataAdapterIFS)Adapter);
		}

		#endregion
	}
}
