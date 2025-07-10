using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs._CustomsTemplate_.Business
{
	public partial class JobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
