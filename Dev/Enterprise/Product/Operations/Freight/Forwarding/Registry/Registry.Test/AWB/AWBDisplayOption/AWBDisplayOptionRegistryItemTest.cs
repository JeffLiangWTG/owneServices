using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.AWB.Testing
{
	[TestedType(typeof(AWBDisplayOptionRegistryItem))]
	public class AWBDisplayOptionRegistryItemTest : StronglyTypedRegistryItemTestCase<AWBDisplayOptionCollection>
	{
		protected override StronglyTypedRegistryItem<AWBDisplayOptionCollection, AWBDisplayOptionCollection> GetNewRegistryItem()
		{
			return new AWBDisplayOptionRegistryItem(string.Empty,
					null, null, null, RegistryStorageFlags.System, new AWBDisplayOptionCollection());
		}
	}
}
