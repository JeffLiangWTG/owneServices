using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(ServiceTaskCreatorOptionRegistryItem))]
	class ServiceTaskCreatorOptionRegistryItemTest : StronglyTypedRegistryItemTestCase<ServiceTaskCreatorOptionCollection>
	{
		protected override StronglyTypedRegistryItem<ServiceTaskCreatorOptionCollection, ServiceTaskCreatorOptionCollection> GetNewRegistryItem()
		{
			return new ServiceTaskCreatorOptionRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ServiceTaskCreatorOptionCollection());
		}
	}
}
