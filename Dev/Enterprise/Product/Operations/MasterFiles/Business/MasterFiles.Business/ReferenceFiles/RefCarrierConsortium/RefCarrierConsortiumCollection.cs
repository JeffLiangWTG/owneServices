using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefCarrierConsortium)]
	public class RefCarrierConsortiumCollection : ActiveBusinessObjectCollection<RefCarrierConsortium>
	{
		public RefCarrierConsortiumCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
