using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class JobNumberResolver
	{
		public static ZString GetJobNumber(BusinessObject parent)
		{
			var jobNumber = ZString.Empty;

			var jobNumberForWorkflowSource = parent as IJobNumberForWorkflow;
			if (jobNumberForWorkflowSource != null)
			{
				jobNumber = jobNumberForWorkflowSource.JobNumber;
			}
			else
			{
				var jobNumberSource = parent as IJobNumber;
				if (jobNumberSource != null)
				{
					jobNumber = jobNumberSource.JobNumber;
				}
			}

			return jobNumber;
		}
	}
}
