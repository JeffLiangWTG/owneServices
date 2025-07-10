using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefAccessorial)]
	public class RefAccessorialCollection : ActiveBusinessObjectCollection<RefAccessorial>
	{
		public RefAccessorialCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefAccessorialCollection(BusinessObject master) : base(master)
		{
		}
	}
}
