using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface IResetParentScreeningStatusHelperForCharges
	{
		void ResetJobParentScreeningStatus(bool hasChanges, IJobHeaderParent jobParent, IEnumerable<JobCharge> charges);
	}
}
