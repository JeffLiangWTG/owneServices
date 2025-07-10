using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbGroup)]
	public class GlbGroupActiveBusinessObjectCollection : ActiveBusinessObjectCollection<GlbGroup>
	{
		public GlbGroupActiveBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbGroupActiveBusinessObjectCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbGroupActiveBusinessObjectCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public GlbGroupActiveBusinessObjectCollection(BusinessObjectFactory factory, bool staffGroupOnly) : base(factory, staffGroupOnly ? new ZQuery(GlbGroupSchema.GG_Type, GlbGroupTypeList.Codes.Staff) : new ZQuery())
		{
		}
	}
}
