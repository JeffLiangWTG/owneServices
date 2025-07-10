using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbBranchNotCurrentCompanyRelated)]
	public class GlbBranchNotCurrentCompanyRelatedCollection : GlbBranchCollection
	{
		public GlbBranchNotCurrentCompanyRelatedCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlbBranchNotCurrentCompanyRelatedCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ZQuery AdditionalRelationshipFilter { get; set; }

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery relationshipFilter = base.CreateRelationshipFilter();
			if (AdditionalRelationshipFilter != null && !AdditionalRelationshipFilter.IsEmpty)
			{
				relationshipFilter.AddToFilter(AdditionalRelationshipFilter);
			}
			return relationshipFilter;
		}
	}
}
