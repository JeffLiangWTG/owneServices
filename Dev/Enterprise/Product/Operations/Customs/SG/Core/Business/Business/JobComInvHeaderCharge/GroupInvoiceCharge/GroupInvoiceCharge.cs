using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class GroupInvoiceCharge : TypeSafeGroupInvoiceCharge, Integration.Customs.SG.IGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
