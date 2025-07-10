using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class GroupInvoiceCharge : EU.Business.Declaration.GroupInvoiceCharge, Integration.Customs.TR.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new GroupInvoiceChargeLookups Lookups => (GroupInvoiceChargeLookups)base.Lookups;

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new GroupInvoiceChargeLookups(this);
	}
}
