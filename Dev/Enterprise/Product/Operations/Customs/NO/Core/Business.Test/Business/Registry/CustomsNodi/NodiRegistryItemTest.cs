using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(NodiRegistryItem))]
sealed class NodiRegistryItemTest : StronglyTypedRegistryItemTestCase<NodiRegistryCollection>
{
	protected override StronglyTypedRegistryItem<NodiRegistryCollection, NodiRegistryCollection> GetNewRegistryItem() => new NodiRegistryItem("", null, null, null, RegistryStorageFlags.All, new NodiRegistryCollection().DefaultCollection);
}
