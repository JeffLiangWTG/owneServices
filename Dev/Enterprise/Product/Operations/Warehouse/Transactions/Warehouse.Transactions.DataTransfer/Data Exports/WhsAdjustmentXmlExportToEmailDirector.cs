using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsAdjustmentXmlExportToEmailDirector : WhsDocketXmlExportToEmailDirector<WhsAdjustment>
	{
		public WhsAdjustmentXmlExportToEmailDirector(WhsAdjustmentValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override string GetJobInvoicingConsumerType()
		{
			return WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode;
		}

		protected override CargoWise.Types.ZString GetDocketTypeDescription()
		{
			return Res.GetString("790b7ac8-5d47-4934-85d9-761b67e9c8ad", "Adjustment");
		}

		protected override WhsXmlExporter<WhsAdjustment, Xsd.WhsDocket> GetNewXmlExporter()
		{
			return new WhsAdjustmentXmlExporter((WhsAdjustmentValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
