using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DangerousGoodsManifestPortCollectionRegistryItem))]
	internal sealed class DangerousGoodsManifestPortCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<DangerousGoodsManifestPortCollection>
	{
		protected override StronglyTypedRegistryItem<DangerousGoodsManifestPortCollection, DangerousGoodsManifestPortCollection> GetNewRegistryItem()
		{
			return new DangerousGoodsManifestPortCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}
	}
}
