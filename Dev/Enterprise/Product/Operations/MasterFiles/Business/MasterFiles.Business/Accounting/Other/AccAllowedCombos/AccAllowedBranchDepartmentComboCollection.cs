using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAllowedBranchDepartmentComboCollection : ActiveBusinessObjectCollection<AccAllowedBranchDepartmentCombo>
	{
		public AccAllowedBranchDepartmentComboCollection(GlbBranch branch)
			: base(branch.Factory, branch, new ZQuery(), AccAllowedBranchDepartmentComboSchema.AAB_GB_Branch)
		{
		}
	}
}
