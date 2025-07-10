using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactAttributeCollection : DependentBusinessObjectCollection<OrgContactAttribute, OrgContact>
	{
		public OrgContactAttributeCollection(OrgContact contact) : base(contact)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery additionalFilter = base.CreateAdditionalFilter();
			additionalFilter.AddToFilter(OrgContactAttributeSchema.PC_IsAllocatedContact, SQLComparisonOperator.Equal, false);
			return additionalFilter;
		}
	}
}
