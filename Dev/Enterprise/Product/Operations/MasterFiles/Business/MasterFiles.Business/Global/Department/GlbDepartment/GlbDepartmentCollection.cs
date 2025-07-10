using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbDepartment)]
	public class GlbDepartmentCollection : ActiveBusinessObjectCollection<GlbDepartment>
	{
		public GlbDepartmentCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public GlbDepartmentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlbDepartmentCollection(BusinessObjectFactory factory, ICollectionRelationship relationship) : base(factory, relationship)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (!BrandingFactory.Instance.GetType().Equals(typeof(ProductivityWiseBranding)))
			{
				return base.CreateRelationshipFilter();
			}

			var excludeNonSystemDeptsFilter = new ZQuery(GlbDepartmentSchema.GE_SystemCode, false);

			var includeSystemMiscDeptsFilter = new ZQuery(GlbDepartmentSchema.GE_SystemCode, true);
			includeSystemMiscDeptsFilter.AddToFilter(GlbDepartmentSchema.GE_Misc, true);

			var totalFilter = excludeNonSystemDeptsFilter.AddToFilter(includeSystemMiscDeptsFilter, JoinCondition.Or);
			var existingRelationship = base.CreateRelationshipFilter();

			return existingRelationship.AddToFilter(totalFilter);
		}
	}
}
