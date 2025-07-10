using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module
{
	public class CommercialInvoiceModule : Customs.Module.CommercialInvoiceModule
	{
		public CommercialInvoiceModule()
		{
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CommercialInvoiceFilterBusinessObject();
	}
}
