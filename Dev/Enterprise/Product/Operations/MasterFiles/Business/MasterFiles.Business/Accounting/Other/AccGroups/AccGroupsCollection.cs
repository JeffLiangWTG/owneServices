
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccGroups)]
	public class AccGroupsCollection : BusinessObjectCollection<AccGroups>
	{
		public AccGroupsCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public AccGroupsCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
