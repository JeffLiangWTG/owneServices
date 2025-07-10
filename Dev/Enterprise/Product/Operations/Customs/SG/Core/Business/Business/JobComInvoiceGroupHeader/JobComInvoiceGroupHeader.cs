using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceGroupHeader : TypeSafeJobComInvoiceGroupHeader, Integration.Customs.SG.IJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this, true);
		}
	}
}
