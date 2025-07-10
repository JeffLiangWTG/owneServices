using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DefaultContainerModesRegistryItem))]
	sealed class DefaultContainerModesRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultContainerModesCollection>
	{
		protected override StronglyTypedRegistryItem<DefaultContainerModesCollection, DefaultContainerModesCollection> GetNewRegistryItem()
		{
			return new DefaultContainerModesRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
