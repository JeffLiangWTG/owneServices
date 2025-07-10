using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class ContainerTranshipmentIndicatorRegistryItem : StronglyTypedRegistryItem<ContainerTranshipmentIndicatorCollection>
	{
		public ContainerTranshipmentIndicatorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, ContainerTranshipmentIndicatorCollection.NewAndPopulate()) { }

		public ContainerTranshipmentIndicatorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ContainerTranshipmentIndicatorCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ContainerTranshipmentIndicatorRegistryDataType(defaultValue), storage, RegistryOptions.Default)) { }
	}
}


