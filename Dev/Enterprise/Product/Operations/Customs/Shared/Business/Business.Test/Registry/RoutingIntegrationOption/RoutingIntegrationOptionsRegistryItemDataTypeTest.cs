using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(RoutingIntegrationOptionsRegistryItemDataType))]
	sealed class RoutingIntegrationOptionsRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<RoutingIntegrationOptionsRegistryItemDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "RoutingIntegrationOptionsRegistryItemEditor"; }
		}

		protected override RoutingIntegrationOptionsRegistryItemDataType GetNewDataType()
		{
			return new RoutingIntegrationOptionsRegistryItemDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var options = new RoutingIntegrationOptions();
			options.NeverLink = true;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(options, new RoutingIntegrationOptionsRegistryItemDataType().Serialise(options))
			};
		}
	}
}
