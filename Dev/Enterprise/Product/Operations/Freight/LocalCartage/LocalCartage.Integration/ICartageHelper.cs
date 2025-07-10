using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Integration
{
	public interface ICartageHelper
	{
		void AttachCartageJobsToParentJob(IJobHeader parentJob, ZGuid parentPK);
		void AttachCartageJobsToParentJob(IJobHeader parentJob, ZGuid parentPK, bool attachEvenIfParentJobIsInDatabase);
	}
}
