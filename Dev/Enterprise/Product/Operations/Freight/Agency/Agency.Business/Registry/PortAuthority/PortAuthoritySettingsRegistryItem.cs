using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class PortAuthoritySettingsRegistryItem : StronglyTypedRegistryItem<PortAuthoritySettings>
	{
		public PortAuthoritySettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PortAuthoritySettingsDataType(), storage))
		{
		}

		public PortAuthoritySettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new PortAuthoritySettingsDataType(), storage, options))
		{
		}

		public PortAuthoritySetting FindPortSetting(ZString port)
		{
			return Value.Settings.FindPortSetting(port);
		}
	}
}
