using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class OrderManagerRequestMappingRegistryItem : StronglyTypedRegistryItem<OrderManagerRequestMappingCollection>
	{
		public OrderManagerRequestMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, OrderManagerRequestMappingCollection defaultValues)
			: base(new RegistryItemImpl(name, category, caption, hint, new OrderManagerRequestMappingDataType(), storage, options, defaultValues))
		{
		}
	}

	[RegistryEditor("Enterprise.Freight.Forwarding.GUI.OrderManagerRequestMappingRegistryItemEditor, Enterprise.Freight.Forwarding.GUI")]
	public class OrderManagerRequestMappingDataType : NonPersistentBusinessObjectRegistryDataType<OrderManagerRequestMappingCollection>
	{
	}
}
