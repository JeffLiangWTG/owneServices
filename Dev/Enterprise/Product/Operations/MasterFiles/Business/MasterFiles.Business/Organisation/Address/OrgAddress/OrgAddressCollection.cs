using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.OrgAddresses)]
	public class OrgAddressCollection : BusinessObjectCollection<OrgAddress>
	{
		public OrgAddressCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgAddressCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
