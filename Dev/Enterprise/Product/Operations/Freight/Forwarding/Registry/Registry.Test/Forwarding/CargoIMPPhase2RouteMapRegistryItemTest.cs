using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(CargoIMPPhase2RouteMapRegistryItem))]
	class CargoIMPPhase2RouteMapRegistryItemTest : StronglyTypedRegistryItemTestCase<CargoIMPPhase2RouteMapCollection>
	{
		protected override StronglyTypedRegistryItem<CargoIMPPhase2RouteMapCollection, CargoIMPPhase2RouteMapCollection> GetNewRegistryItem()
		{
			return new CargoIMPPhase2RouteMapRegistryItem(string.Empty,
					null, null, null, RegistryStorageFlags.System, new CargoIMPPhase2RouteMapCollection());
		}
	}
}
