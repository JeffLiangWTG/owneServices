using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class GroupInvoiceCharge : Customs.Business.BaseGroupInvoiceCharge, Integration.Customs.NZ.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new GroupInvoiceChargeValidation(this);
		}
	}
}
