using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsOrderXmlExportToFileDirector : WhsDocketXmlExportToFileDirector<WhsOrder>
	{
		public WhsOrderXmlExportToFileDirector(WhsOrderValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override ZString GetDocketTypeDescription()
		{
			return Res.GetString("70a579a4-3338-4efa-93c0-0bcc28b18f6c", "Order");
		}

		protected override WhsXmlExporter<WhsOrder, Xsd.WhsDocket> GetNewXmlExporter()
		{
			return new WhsOrderXmlExporter((WhsOrderValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
