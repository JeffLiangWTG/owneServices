using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.TW.Business
{
	public class CommercialInvoiceChargeWrapperCollection : DocumentWrapperCollection<CommercialInvoiceChargeWrapper>
	{
		public CommercialInvoiceChargeWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void AddChargesFrom(IEnumerable invoiceCharges)
		{
			foreach (InvoiceCharge invoiceCharge in invoiceCharges)
			{
				Add(CommercialInvoiceChargeWrapper.New(invoiceCharge, Factory));
			}
		}
	}
}
