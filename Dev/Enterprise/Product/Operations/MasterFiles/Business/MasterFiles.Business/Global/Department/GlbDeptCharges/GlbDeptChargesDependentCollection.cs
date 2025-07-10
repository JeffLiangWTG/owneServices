using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbDeptChargesDependentCollection : ActiveBusinessObjectCollection<GlbDeptCharges>
	{
		public GlbDeptChargesDependentCollection(GlbDepartment parent)
			: base(parent)
		{
		}
	}
}
