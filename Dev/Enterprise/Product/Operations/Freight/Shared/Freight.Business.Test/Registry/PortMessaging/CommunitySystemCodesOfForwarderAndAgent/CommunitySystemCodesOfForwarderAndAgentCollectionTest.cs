using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommunitySystemCodesOfForwarderAndAgentCollection))]
	sealed class CommunitySystemCodesOfForwarderAndAgentCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CommunitySystemCodesOfForwarderAndAgentCollection>
	{
		public void TestDuplicateRegistryEntry()
		{
			var collection = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var registryEntry = new CommunitySystemCodesOfForwarderAndAgent();
			registryEntry.Port = "FRPAR";
			registryEntry.ForwarderCode = "F001";
			registryEntry.AgentCode = "A001";

			collection.Add(registryEntry);

			var duplicateRegistryEntry = new CommunitySystemCodesOfForwarderAndAgent();
			duplicateRegistryEntry.Port = "FRPAR";
			duplicateRegistryEntry.ForwarderCode = "F002";
			duplicateRegistryEntry.AgentCode = "A002";

			AssertEquals(true, collection.IsDuplicateItem(duplicateRegistryEntry));
			AssertEquals(false, collection.IsDuplicateItem(null));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CommunitySystemCodesOfForwarderAndAgentCollection GetCollectionToTest()
		{
			return new CommunitySystemCodesOfForwarderAndAgentCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CommunitySystemCodesOfForwarderAndAgent();
		}

		#endregion
	}
}
