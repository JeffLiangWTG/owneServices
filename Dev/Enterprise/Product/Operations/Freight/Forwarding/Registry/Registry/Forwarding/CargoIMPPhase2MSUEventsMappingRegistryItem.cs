using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class CargoIMPPhase2MSUEventsMappingRegistryItem : StronglyTypedRegistryItem<CargoIMPPhase2MSUEventsMappingCollection>
	{
		public CargoIMPPhase2MSUEventsMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CargoIMPPhase2MSUEventsMappingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CargoIMPPhase2MSUEventsMappingRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Freight.Forwarding.GUI.CargoIMPPhase2MSUEventsMappingRegistryItemEditor, Enterprise.Freight.Forwarding.GUI")]
	class CargoIMPPhase2MSUEventsMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CargoIMPPhase2MSUEventsMappingCollection>
	{
		public CargoIMPPhase2MSUEventsMappingRegistryDataType()
		{
		}
	}
}
