using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class CargoIMPPhase2RouteMapRegistryItem : StronglyTypedRegistryItem<CargoIMPPhase2RouteMapCollection>
	{
		public CargoIMPPhase2RouteMapRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CargoIMPPhase2RouteMapCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CargoIMPPhase2RouteMapRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Freight.Forwarding.GUI.CargoIMPPhase2RouteMapRegistryItemEditor, Enterprise.Freight.Forwarding.GUI")]
	class CargoIMPPhase2RouteMapRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CargoIMPPhase2RouteMapCollection>
	{
		public CargoIMPPhase2RouteMapRegistryDataType()
		{
		}
	}
}
