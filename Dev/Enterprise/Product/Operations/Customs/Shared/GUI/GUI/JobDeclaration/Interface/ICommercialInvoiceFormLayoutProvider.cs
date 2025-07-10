using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public interface ICommercialInvoiceFormLayoutProvider
	{
		IPanelLayoutProvider GetInvoiceHeaderDetailsLayout(BaseJobComInvoiceHeader invoice);
	}
}
