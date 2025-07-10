using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortAuthorityPortCollectionRegistryItem))]
	internal sealed class PortAuthorityPortCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<PortAuthorityPortCollection>
	{
		protected override StronglyTypedRegistryItem<PortAuthorityPortCollection, PortAuthorityPortCollection> GetNewRegistryItem()
		{
			return new PortAuthorityPortCollectionRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System, new PortAuthorityPortCollection());
		}
	}
}
