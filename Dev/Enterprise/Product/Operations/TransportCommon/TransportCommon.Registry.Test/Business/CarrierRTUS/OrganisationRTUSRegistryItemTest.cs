using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(OrganisationRTUSRegistryItem))]
	class OrganisationRTUSRegistryItemTest : StronglyTypedRegistryItemTestCase<OrganisationRTUSCollection>
	{
		protected override StronglyTypedRegistryItem<OrganisationRTUSCollection, OrganisationRTUSCollection> GetNewRegistryItem()
		{
			return new OrganisationRTUSRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}
}
