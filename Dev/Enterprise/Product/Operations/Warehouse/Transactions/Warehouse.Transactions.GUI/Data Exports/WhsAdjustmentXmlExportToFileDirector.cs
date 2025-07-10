using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsAdjustmentXmlExportToFileDirector : WhsDocketXmlExportToFileDirector<WhsAdjustment>
	{
		public WhsAdjustmentXmlExportToFileDirector(WhsAdjustmentValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override CargoWise.Types.ZString GetDocketTypeDescription()
		{
			return Res.GetString("c89c57cc-d6c6-43f2-a235-009cf47c2465", "Adjustment");
		}

		protected override WhsXmlExporter<WhsAdjustment, Xsd.WhsDocket> GetNewXmlExporter()
		{
			return new WhsAdjustmentXmlExporter((WhsAdjustmentValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
