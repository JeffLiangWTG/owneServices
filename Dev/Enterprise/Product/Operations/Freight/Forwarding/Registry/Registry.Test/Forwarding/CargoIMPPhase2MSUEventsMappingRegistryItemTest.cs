using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(CargoIMPPhase2MSUEventsMappingRegistryItem))]
	class CargoIMPPhase2MSUEventsMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<CargoIMPPhase2MSUEventsMappingCollection>
	{
		protected override StronglyTypedRegistryItem<CargoIMPPhase2MSUEventsMappingCollection, CargoIMPPhase2MSUEventsMappingCollection> GetNewRegistryItem()
		{
			return new CargoIMPPhase2MSUEventsMappingRegistryItem(string.Empty,
					null, null, null, RegistryStorageFlags.System, new CargoIMPPhase2MSUEventsMappingCollection());
		}
	}
}
