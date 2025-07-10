using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public partial class InvoiceLineCharge : AutoInvoiceLineCharge, Integration.Customs.US.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
