using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceJobCluster : AutoAccDraftInvoiceJobCluster
	{
		public AccDraftInvoiceJobCluster(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public AccDraftInvoiceJobCollection OperationalJobs
		{
			get
			{
				if (operationalJobs == null)
				{
					operationalJobs = new AccDraftInvoiceJobCollection(this, Factory);
					operationalJobs.Load();
				}
				return operationalJobs;
			}
		}
		AccDraftInvoiceJobCollection operationalJobs;
	}
}
