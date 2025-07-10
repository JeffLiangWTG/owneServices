using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class DpsStatusUpdateSettingRegistryItem : StronglyTypedRegistryItem<DpsStatusUpdateSetting>
	{
		public DpsStatusUpdateSettingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, PhaseSecurityRegistryItem phaseSecurityRegistryItem)
			: base(new RegistryItemImpl(name, category, caption, hint, new DpsStatusUpdateSettingDataType(phaseSecurityRegistryItem), storage, new DpsStatusUpdateSetting(phaseSecurityRegistryItem)))
		{
		}
	}
}
