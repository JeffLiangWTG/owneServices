using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsReceiveXmlExportToEmailDirector : WhsDocketXmlExportToEmailDirector<WhsReceive>
	{
		public WhsReceiveXmlExportToEmailDirector(WhsReceiveValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override string GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.WarehouseInwards.Code;
		}

		protected override CargoWise.Types.ZString GetDocketTypeDescription()
		{
			return Res.GetString("b416a387-1f38-4abc-a689-7ec390ffbd4d", "Receipt");
		}

		protected override WhsXmlExporter<WhsReceive, Xsd.WhsDocket> GetNewXmlExporter()
		{
			return new WhsReceiveXmlExporter((WhsReceiveValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
