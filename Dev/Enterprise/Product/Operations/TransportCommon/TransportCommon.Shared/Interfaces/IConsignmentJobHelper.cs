using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportCommon.Shared
{
	public interface IConsignmentJobHelper
	{
		void AttachLTConsignmentJobsToParentJob(IJobHeader parentJob, ZGuid parentPK);
		void AttachLTConsignmentJobsToParentJob(IJobHeader parentJob, ZGuid parentPK, bool attachEvenIfInDatabase);
	}
}
