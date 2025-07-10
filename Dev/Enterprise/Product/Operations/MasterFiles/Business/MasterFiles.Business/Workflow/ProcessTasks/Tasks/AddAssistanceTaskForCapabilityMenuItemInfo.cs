using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AddAssistanceTaskForCapabilityMenuItemInfo
	{
		public AddAssistanceTaskForCapabilityMenuItemInfo(ZGuid capabilityPK, MultilingualString menuItemDescription)
		{
			CapabilityPK = capabilityPK;
			MenuItemDescription = menuItemDescription;
		}

		public ZGuid CapabilityPK { get; }
		public MultilingualString MenuItemDescription { get; }
	}
}
