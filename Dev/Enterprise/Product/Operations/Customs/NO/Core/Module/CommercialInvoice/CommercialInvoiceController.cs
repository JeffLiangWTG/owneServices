using CargoWise.EntityFramework;
using Enterprise.Customs.NO.Business;
using Enterprise.Customs.NO.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Module
{
	public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
	{
		public CommercialInvoiceController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity) => new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
	}
}
