using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsAdjustmentExportToFileMenuItem : WhsDocketExportToFileMenuItem<WhsAdjustment, Xsd.WhsDocket>
	{
		public WhsAdjustmentExportToFileMenuItem(WhsAdjustment adjustment, WhsAdjustmentValueObjectDataAdapter adapter)
			: base(adjustment, adapter)
		{
		}

		public WhsAdjustmentExportToFileMenuItem(WhsAdjustment adjustment)
			: this(adjustment, null)
		{
		}

		#region Overrides

		protected override WhsXmlExportDirector<WhsAdjustment, Xsd.WhsDocket> GetNewDirector()
		{
			return new WhsAdjustmentXmlExportToFileDirector((WhsAdjustmentValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
