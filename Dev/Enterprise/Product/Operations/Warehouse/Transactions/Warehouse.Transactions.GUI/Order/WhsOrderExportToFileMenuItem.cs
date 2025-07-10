using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsOrderExportToFileMenuItem : WhsDocketExportToFileMenuItem<WhsOrder, Xsd.WhsDocket>
	{
		public WhsOrderExportToFileMenuItem(WhsOrder order, WhsOrderValueObjectDataAdapter adapter)
			: base(order, adapter)
		{
		}

		public WhsOrderExportToFileMenuItem(WhsOrder order)
			: this(order, null)
		{
		}

		#region Overrides

		protected override WhsXmlExportDirector<WhsOrder, Xsd.WhsDocket> GetNewDirector()
		{
			return new WhsOrderXmlExportToFileDirector((WhsOrderValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
