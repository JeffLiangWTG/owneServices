using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(TNPAAccountNumberRegistryItem))]
	sealed class TNPAAccountNumberRegistryItemTest : StronglyTypedRegistryItemTestCase<TNPAAccountNumberCollection>
	{
		protected override StronglyTypedRegistryItem<TNPAAccountNumberCollection, TNPAAccountNumberCollection> GetNewRegistryItem()
		{
			return new TNPAAccountNumberRegistryItem("", null, null, null, RegistryStorageFlags.System, new TNPAAccountNumberCollection());
		}
	}
}
