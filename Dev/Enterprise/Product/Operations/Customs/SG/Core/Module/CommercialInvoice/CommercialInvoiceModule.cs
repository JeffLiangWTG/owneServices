using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.SG.V4.Module
{
	public class CommercialInvoiceModule : Customs.Module.CommercialInvoiceModule
	{
		public CommercialInvoiceModule()
		{
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CommercialInvoiceFilterBusinessObject();
		}
	}
}
