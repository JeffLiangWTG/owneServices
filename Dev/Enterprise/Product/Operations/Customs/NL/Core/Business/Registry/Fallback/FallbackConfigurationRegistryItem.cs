using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

public class FallbackConfigurationRegistryItem : StronglyTypedRegistryItem<FallbackConfiguration>
{
	public FallbackConfigurationRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage)
		: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new FallbackConfigurationRegistryDataType(), storage))
	{
	}

	public FallbackConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, FallbackConfiguration defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new FallbackConfigurationRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.Default, defaultValue))
	{
	}

	[RegistryEditor("Enterprise.Customs.NL.GUI.FallbackRegistryItemEditor, Enterprise.Customs.NL.GUI")]
	public class FallbackConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FallbackConfiguration>
	{
		public FallbackConfigurationRegistryDataType()
		{
		}
	}
}
