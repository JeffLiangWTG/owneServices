using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsAdjustmentExportToEmailMenuItem : WhsDocketExportToEmailMenuItem<WhsAdjustment, Xsd.WhsDocket>
	{
		public WhsAdjustmentExportToEmailMenuItem(WhsAdjustment adjustment, WhsAdjustmentValueObjectDataAdapter adapter)
			: base(adjustment, adapter)
		{
		}

		#region Overrides

		protected override WhsXmlExportDirector<WhsAdjustment, Xsd.WhsDocket> GetNewDirector()
		{
			return new WhsAdjustmentXmlExportToEmailDirector((WhsAdjustmentValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
