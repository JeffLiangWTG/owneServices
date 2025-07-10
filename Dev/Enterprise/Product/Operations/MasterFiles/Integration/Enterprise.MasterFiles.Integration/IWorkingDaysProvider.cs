using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkingDaysProvider
	{
		IWorkTimeArithmetic GetWorkingDays(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK = default(ZGuid));
	}
}