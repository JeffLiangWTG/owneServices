using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsOrderExportToEmailMenuItem : WhsDocketExportToEmailMenuItem<WhsOrder, Xsd.WhsDocket>
	{
		public WhsOrderExportToEmailMenuItem(WhsOrder order, WhsOrderValueObjectDataAdapter adapter)
			: base(order, adapter)
		{
		}

		#region Overrides

		protected override WhsXmlExportDirector<WhsOrder, Xsd.WhsDocket> GetNewDirector()
		{
			return new WhsOrderXmlExportToEmailDirector((WhsOrderValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
