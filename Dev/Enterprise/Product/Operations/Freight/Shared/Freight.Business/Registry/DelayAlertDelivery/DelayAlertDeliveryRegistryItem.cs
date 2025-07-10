using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	public class DelayAlertDeliveryRegistryItem : StronglyTypedRegistryItem<DelayAlertDeliveryRuleCollection>
	{
		public DelayAlertDeliveryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DelayAlertDeliveryRegistryDataType(), storage, RegistryOptions.Default)) { }

		public DelayAlertDeliveryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new DelayAlertDeliveryRegistryDataType(), storage, options)) { }

		public DelayAlertDeliveryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DelayAlertDeliveryRuleCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DelayAlertDeliveryRegistryDataType(defaultValue), storage, RegistryOptions.Default)) { }

		public DelayAlertDeliveryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DelayAlertDeliveryRuleCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DelayAlertDeliveryRegistryDataType(defaultValue), storage, options)) { }
	}
}
