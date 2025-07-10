using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsReceiveExportToFileMenuItem : WhsDocketExportToFileMenuItem<WhsReceive, Xsd.WhsDocket>
	{
		public WhsReceiveExportToFileMenuItem(WhsReceive receive, WhsReceiveValueObjectDataAdapter adapter)
			: base(receive, adapter)
		{
		}

		#region Overrides

		protected override WhsXmlExportDirector<WhsReceive, Xsd.WhsDocket> GetNewDirector()
		{
			return new WhsReceiveXmlExportToFileDirector((WhsReceiveValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
