using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Module
{
	public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
	{
		public CommercialInvoiceController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
		}
	}
}
