using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class InvoiceLineCharge : BaseInvoiceLineCharge, Integration.Customs.NZ.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineChargeValidation(this);
		}
	}
}
