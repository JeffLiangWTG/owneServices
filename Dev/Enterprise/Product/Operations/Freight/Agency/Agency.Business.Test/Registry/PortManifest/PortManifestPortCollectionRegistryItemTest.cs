using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortManifestPortCollectionRegistryItem))]
	internal sealed class PortManifestPortCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<PortManifestPortCollection>
	{
		protected override StronglyTypedRegistryItem<PortManifestPortCollection, PortManifestPortCollection> GetNewRegistryItem()
		{
			return new PortManifestPortCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}
	}
}
