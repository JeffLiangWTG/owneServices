using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.NZ.Business.Data
{
	public class NZInvoiceValueObjectDataAdapter : InvoiceValueObjectDataAdapter
	{
		public NZInvoiceValueObjectDataAdapter(BaseJobDeclaration jobDec)
			: base(jobDec)
		{
		}

		protected override string AddInfoPrefix
		{
			get { return "ZN_"; }
		}

		protected override InvoiceDataTransferTool GetInvoiceDataTransferTool()
		{
			return new NZInvoiceDataTransferTool(false, AddInfoDataTransferTool);
		}
	}
}
