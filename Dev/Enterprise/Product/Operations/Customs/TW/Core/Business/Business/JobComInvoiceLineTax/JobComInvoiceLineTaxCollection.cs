using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceLineTaxCollection : DependentBusinessObjectCollection<JobComInvoiceLineTax, JobComInvoiceLine>
	{
		public JobComInvoiceLineTaxCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{ }

		public ZBool HasType(ZString type) => this.Cast<JobComInvoiceLineTax>().Any(x => x.JLT_Type == type);
	}
}
