using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DataTransfer
{
	public class USInvoiceDataAdapter : InvoiceValueObjectDataAdapter
	{
		public USInvoiceDataAdapter(JobDeclaration jobDec)
			: base(jobDec)
		{
		}

		#region Overrides

		protected override string AddInfoPrefix
		{
			get { return "US_"; }
		}

		protected override InvoiceDataTransferTool GetInvoiceDataTransferTool()
		{
			return new USInvoiceDataTransferTool(false);
		}

		#endregion
	}
}
