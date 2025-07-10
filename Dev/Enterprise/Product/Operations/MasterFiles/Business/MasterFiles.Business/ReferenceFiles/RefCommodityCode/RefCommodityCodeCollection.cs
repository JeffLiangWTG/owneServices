using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefCommodityCode)]
	public class RefCommodityCodeCollection : ActiveBusinessObjectCollection<RefCommodityCode>
	{
		public RefCommodityCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefCommodityCodeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefCommodityCodeCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
