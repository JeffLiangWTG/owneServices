using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbBranch)]
	public class GlbBranchCollection : BusinessObjectCollection<GlbBranch>, IGlbBranchCollection
	{
		public GlbBranchCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlbBranchCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
