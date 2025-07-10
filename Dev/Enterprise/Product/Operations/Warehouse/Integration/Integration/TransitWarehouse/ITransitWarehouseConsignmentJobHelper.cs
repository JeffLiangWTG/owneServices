using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface ITransitWarehouseConsignmentJobHelper
	{
		void AttachConsignmentJobsToParentJob(IJobHeader parentJob, ZGuid parentPK);
	}
}
