using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.OrgDebtorGroup)]
	public class OrgDebtorGroupCollection : BusinessObjectCollection<OrgDebtorGroup>, IOrgDebtorGroupCollection
	{
		public OrgDebtorGroupCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgDebtorGroupCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
