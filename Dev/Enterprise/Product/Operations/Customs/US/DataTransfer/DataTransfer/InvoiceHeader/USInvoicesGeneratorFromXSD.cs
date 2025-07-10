
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DataTransfer
{
	public class USInvoicesGeneratorFromXSD : InvoicesGeneratorFromXSD
	{
		public USInvoicesGeneratorFromXSD(BaseJobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override InvoiceValueObjectDataAdapter GetInvoiceAdapter()
		{
			return new USInvoiceDataAdapter((JobDeclaration)declaration);
		}
	}
}
