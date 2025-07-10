using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs._CustomsTemplate_.Business
{
	public abstract class AutoInvoiceLineCharge : Customs.Business.BaseInvoiceLineCharge
	{
		protected AutoInvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
