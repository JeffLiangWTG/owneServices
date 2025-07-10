using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBeneficiaryBranchDepartment : AutoGlbBeneficiaryBranchDepartment
	{
		public GlbBeneficiaryBranchDepartment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GBB_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}
	}
}
