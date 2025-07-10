using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public class DetentionAdviceDeliveryRegistryItem : StronglyTypedRegistryItem<DetentionAdviceDelivery>
	{
		public DetentionAdviceDeliveryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DetentionAdviceDeliveryRegistryDataType(), storage, RegistryOptions.Default)) { }

		public DetentionAdviceDeliveryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DetentionAdviceDelivery defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DetentionAdviceDeliveryRegistryDataType(defaultValue), storage, RegistryOptions.Default)) { }
	}
}


