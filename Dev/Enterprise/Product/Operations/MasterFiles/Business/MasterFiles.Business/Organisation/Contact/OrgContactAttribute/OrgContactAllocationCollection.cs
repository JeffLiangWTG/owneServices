using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactAllocationCollection : DependentBusinessObjectCollection<OrgContactAllocation, OrgContact>
	{
		public OrgContactAllocationCollection(OrgContact contact)
			: base(contact)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery additionalFilter = base.CreateAdditionalFilter();
			additionalFilter.AddToFilter(OrgContactAttributeSchema.PC_IsAllocatedContact, SQLComparisonOperator.Equal, true);
			return additionalFilter;
		}
	}
}
