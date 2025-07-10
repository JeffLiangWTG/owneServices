using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.US.DataTransfer
{
	public class USStandAloneInvoiceDataAdapter : StandAloneInvoiceValueObjectDataAdapter, Integration.Customs.US.IUSStandAloneInvoiceDataAdapter
	{
		#region Overrides

		protected override string AddInfoPrefix
		{
			get { return "US_"; }
		}

		protected override InvoiceDataTransferTool GetInvoiceDataTransferTool()
		{
			return new USInvoiceDataTransferTool(true);
		}

		#endregion
	}
}
