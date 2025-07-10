using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceLinkCollection : ActiveBusinessObjectCollection<JobServiceLink>
	{
		public JobServiceLinkCollection(BusinessObject master)
			: base(master.Factory, master, new ZQuery(), JobServiceLinkSchema.ESL_ParentID)
		{
		}

		public JobServiceLinkCollection(JobService master)
			: base(master.Factory, master, new ZQuery(), JobServiceLinkSchema.ESL_ES_JobService)
		{
		}
	}
}
