using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IBranchDepartmentProvider
	{
		IGlbBranch GetBranch(BusinessObjectFactory factory);
		IGlbDepartment GetDepartment(BusinessObjectFactory factory);
	}
}
