using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class InvoiceCharge : Customs.Business.BaseInvoiceCharge, Integration.Customs.NZ.IInvoiceCharge
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
