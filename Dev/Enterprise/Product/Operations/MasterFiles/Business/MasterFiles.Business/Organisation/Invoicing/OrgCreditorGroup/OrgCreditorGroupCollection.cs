using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.OrgCreditorGroup)]
	public class OrgCreditorGroupCollection : BusinessObjectCollection<OrgCreditorGroup>, IOrgCreditorGroupCollection
	{
		public OrgCreditorGroupCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgCreditorGroupCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
