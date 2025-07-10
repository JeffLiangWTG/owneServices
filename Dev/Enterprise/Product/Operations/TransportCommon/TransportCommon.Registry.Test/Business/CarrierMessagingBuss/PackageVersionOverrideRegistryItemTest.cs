using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(PackageVersionOverrideRegistryItem))]
	class PackageVersionOverrideRegistryItemTest : StronglyTypedRegistryItemTestCase<PackageVersionOverrideCollection>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<PackageVersionOverrideCollection, PackageVersionOverrideCollection> GetNewRegistryItem()
			=> new PackageVersionOverrideRegistryItem("TEST_REGISTRY_ITEM", null, null, null,
				RegistryStorageFlags.System, new PackageVersionOverrideCollection(), RegistryOptions.IsOnlyForSupport);

		#endregion
	}
}
