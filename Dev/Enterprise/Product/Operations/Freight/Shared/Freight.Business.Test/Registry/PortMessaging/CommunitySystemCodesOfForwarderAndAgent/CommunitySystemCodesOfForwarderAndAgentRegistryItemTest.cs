using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommunitySystemCodesOfForwarderAndAgentRegistryItem))]
	sealed class CommunitySystemCodesOfForwarderAndAgentRegistryItemTest : StronglyTypedRegistryItemTestCase<CommunitySystemCodesOfForwarderAndAgentCollection>
	{
		protected override StronglyTypedRegistryItem<CommunitySystemCodesOfForwarderAndAgentCollection, CommunitySystemCodesOfForwarderAndAgentCollection> GetNewRegistryItem()
		{
			return new CommunitySystemCodesOfForwarderAndAgentRegistryItem("", null, null, null, RegistryStorageFlags.System, new CommunitySystemCodesOfForwarderAndAgentCollection());
		}
	}
}
