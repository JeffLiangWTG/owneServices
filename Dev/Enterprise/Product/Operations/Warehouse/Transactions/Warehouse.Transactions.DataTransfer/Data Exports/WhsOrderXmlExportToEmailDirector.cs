using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderXmlExportToEmailDirector : WhsDocketXmlExportToEmailDirector<WhsOrder>
	{
		public WhsOrderXmlExportToEmailDirector(WhsOrderValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override string GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.WarehouseOutwards.Code;
		}

		protected override CargoWise.Types.ZString GetDocketTypeDescription()
		{
			return Res.GetString("b7218afd-b519-47dd-829f-843272799422", "Order");
		}

		protected override WhsXmlExporter<WhsOrder, Xsd.WhsDocket> GetNewXmlExporter()
		{
			return new WhsOrderXmlExporter((WhsOrderValueObjectDataAdapter)Adapter);
		}

		#endregion
	}
}
