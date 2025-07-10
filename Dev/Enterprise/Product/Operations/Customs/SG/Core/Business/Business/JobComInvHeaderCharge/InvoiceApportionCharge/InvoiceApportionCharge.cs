using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceApportionCharge : TypeSafeInvoiceApportionCharge, Integration.Customs.SG.IInvoiceApportionCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
