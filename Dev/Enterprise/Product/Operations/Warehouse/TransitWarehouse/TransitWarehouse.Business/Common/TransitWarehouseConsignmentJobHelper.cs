using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Common
{
	public class TransitWarehouseConsignmentJobHelper : ITransitWarehouseConsignmentJobHelper
	{
		public void AttachConsignmentJobsToParentJob(IJobHeader parentJob, ZGuid parentPK)
		{
			if (!parentJob.IsInDatabase)
			{
				var rcnJobs = parentJob.Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ParentID, parentPK))
					?.Where(rcn => rcn.JobHeader != null)
					.Select(rcn => rcn.JobHeader);

				var dcnJobs = parentJob.Factory.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, parentPK))
					?.Where(dcn => dcn.JobHeader != null)
					.Select(dcn => dcn.JobHeader);

				foreach (var childJob in rcnJobs.Union(dcnJobs))
				{
					AttachConsignmentJobToParentJob(childJob, (JobHeader)parentJob);
				}
			}
		}

		public static void AttachConsignmentJobToParentJob(JobHeader job, ZGuid parentID, ZString parentTableCode)
		{
			if (!parentID.IsEmpty && !parentTableCode.IsEmpty)
			{
				if (job.Factory.Load(parentTableCode, parentID) is IJobHeaderParent parent)
				{
					AttachConsignmentJobToParentJob(job, parent);
				}
			}
		}

		static void AttachConsignmentJobToParentJob(JobHeader jobHeader, IJobHeaderParent parent)
		{
			Argument.NotNull(jobHeader, nameof(jobHeader));
			Argument.NotNull(parent, nameof(parent));

			var parentJobHeader = new JobHeader.Loader(parent).Load();
			if (parentJobHeader != null)
			{
				AttachConsignmentJobToParentJob(jobHeader, parentJobHeader);
			}
		}

		static void AttachConsignmentJobToParentJob(JobHeader jobHeader, JobHeader parentJobHeader)
		{
			Argument.NotNull(jobHeader, nameof(jobHeader));
			Argument.NotNull(parentJobHeader, nameof(parentJobHeader));

			jobHeader.JH_JH_ParentJob = parentJobHeader.PK;
		}
	}
}
