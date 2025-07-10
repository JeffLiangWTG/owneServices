using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceLineApportionCharge : TypeSafeInvoiceLineApportionCharge, Integration.Customs.SG.IInvoiceLineApportionCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
