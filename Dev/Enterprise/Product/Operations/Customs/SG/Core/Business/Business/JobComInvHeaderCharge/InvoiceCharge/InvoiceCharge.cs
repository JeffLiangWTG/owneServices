using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceCharge : TypeSafeInvoiceCharge, Integration.Customs.SG.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceChargeValidation(this);
		}
	}
}
