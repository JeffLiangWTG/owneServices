using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class RoutingIntegrationOptionsRegistryItem : StronglyTypedRegistryItem<RoutingIntegrationOptions>
	{
		public RoutingIntegrationOptionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RoutingIntegrationOptions defaultOption)
			: base(new RegistryItemImpl(name, category, caption, hint, new RoutingIntegrationOptionsRegistryItemDataType(), storage, RegistryOptions.Default, defaultOption))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.DataRegistry.GUI.RoutingIntegrationOptionsRegistryItemEditor, Enterprise.Customs.GUI")]
	public class RoutingIntegrationOptionsRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<RoutingIntegrationOptions>
	{
	}
}
