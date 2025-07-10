using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsReceiveXmlExportToFileDirector : WhsDocketXmlExportToFileDirector<WhsReceive>
	{
		public WhsReceiveXmlExportToFileDirector(WhsReceiveValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override CargoWise.Types.ZString GetDocketTypeDescription()
		{
			return Res.GetString("928b70b1-7e5d-48b6-a359-9c64c9d2ba32", "Receipt");
		}

		protected override WhsXmlExporter<WhsReceive, Xsd.WhsDocket> GetNewXmlExporter()
		{
			return new WhsReceiveXmlExporter((WhsReceiveValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
