using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.NZ.Business.Data
{
	public class NZStandAloneInvoiceValueObjectDataAdapter : StandAloneInvoiceValueObjectDataAdapter, Integration.Customs.NZ.INZStandAloneInvoiceValueObjectDataAdapter
	{
		protected override string AddInfoPrefix
		{
			get { return "ZN_"; }
		}

		protected override InvoiceDataTransferTool GetInvoiceDataTransferTool()
		{
			return new NZInvoiceDataTransferTool(true, AddInfoDataTransferTool);
		}
	}
}
