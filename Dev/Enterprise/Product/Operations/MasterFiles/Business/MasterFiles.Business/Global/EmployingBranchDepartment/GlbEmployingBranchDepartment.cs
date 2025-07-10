using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmployingBranchDepartment : AutoGlbEmployingBranchDepartment
	{
		public GlbEmployingBranchDepartment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GHB_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}
	}
}
