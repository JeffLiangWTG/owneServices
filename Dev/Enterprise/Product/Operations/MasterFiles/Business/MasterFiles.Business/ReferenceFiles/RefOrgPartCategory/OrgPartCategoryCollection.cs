using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefOrgPartCategory)]
	public class OrgPartCategoryCollection : ActiveBusinessObjectCollection<OrgPartCategory>
	{
		public OrgPartCategoryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgPartCategoryCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public OrgPartCategoryCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
