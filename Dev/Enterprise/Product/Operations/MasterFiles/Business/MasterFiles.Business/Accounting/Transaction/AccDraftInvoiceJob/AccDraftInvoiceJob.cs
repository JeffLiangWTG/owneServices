using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceJob : AutoAccDraftInvoiceJob
	{
		public AccDraftInvoiceJob(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
