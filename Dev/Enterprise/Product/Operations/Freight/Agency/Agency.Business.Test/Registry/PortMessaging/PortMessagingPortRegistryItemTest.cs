using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortMessagingPortRegistryItem))]
	internal class PortMessagingPortRegistryItemTest : StronglyTypedRegistryItemTestCase<PortMessagingPortCollection>
	{
		protected override StronglyTypedRegistryItem<PortMessagingPortCollection, PortMessagingPortCollection> GetNewRegistryItem()
		{
			return new PortMessagingPortRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System | RegistryStorageFlags.Company, new PortMessagingPortCollection());
		}
	}
}
