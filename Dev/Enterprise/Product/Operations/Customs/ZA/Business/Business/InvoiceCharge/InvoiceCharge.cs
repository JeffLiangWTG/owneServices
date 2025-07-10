using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class InvoiceCharge : BaseInvoiceCharge, Integration.Customs.ZA.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceChargeLookups(this);
		}
	}
}
