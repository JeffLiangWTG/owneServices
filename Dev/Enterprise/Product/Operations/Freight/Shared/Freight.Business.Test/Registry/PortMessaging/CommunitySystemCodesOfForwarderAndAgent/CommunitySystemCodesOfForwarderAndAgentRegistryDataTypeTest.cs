using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommunitySystemCodesOfForwarderAndAgentRegistryDataType))]
	sealed class CommunitySystemCodesOfForwarderAndAgentRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CommunitySystemCodesOfForwarderAndAgentRegistryDataType>
	{
		protected override CommunitySystemCodesOfForwarderAndAgentRegistryDataType GetNewDataType()
		{
			return new CommunitySystemCodesOfForwarderAndAgentRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CommunitySystemCodesOfForwarderAndAgentRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var item = collection.AddNew();
			item.Port = "FRPAS";
			item.PCS = "MGI";
			item.ForwarderCode = "F01";
			item.AgentCode = "A01";

			var collection2 = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var item2 = collection2.AddNew();
			item2.Port = "FRPAR";
			item2.PCS = "MGI";
			item2.ForwarderCode = "F02";
			item2.AgentCode = "A02";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new CommunitySystemCodesOfForwarderAndAgentRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new CommunitySystemCodesOfForwarderAndAgentRegistryDataType().Serialise(collection2))
			};
		}
	}
}
