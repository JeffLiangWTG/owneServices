using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HouseBillsNumberValidationRegistryItem))]
	class HouseBillsNumberValidationRegistryItemTest : StronglyTypedRegistryItemTestCase<HouseBillsNumberValidationCollection>
	{
		protected override StronglyTypedRegistryItem<HouseBillsNumberValidationCollection, HouseBillsNumberValidationCollection> GetNewRegistryItem()
		{
			return new HouseBillsNumberValidationRegistryItem("", null, null, null, RegistryStorageFlags.System, new HouseBillsNumberValidationCollection());
		}
	}
}
