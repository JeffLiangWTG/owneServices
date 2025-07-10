using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefMessagingBussCarrierInfo)]
	public class RefMessagingBussCarrierInfoCollection : ActiveBusinessObjectCollection<RefMessagingBussCarrierInfo>
	{
		public RefMessagingBussCarrierInfoCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefMessagingBussCarrierInfoCollection(BusinessObject master) : base(master)
		{
		}
	}
}
