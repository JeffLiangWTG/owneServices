using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBeneficiaryBranchDepartmentCollection : ActiveBusinessObjectCollection<GlbBeneficiaryBranchDepartment>
	{
		public GlbBeneficiaryBranchDepartmentCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbBeneficiaryBranchDepartmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbBeneficiaryBranchDepartmentCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
