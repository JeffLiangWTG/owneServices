using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing
{
	[TestedType(typeof(NodiDataType))]
	sealed class NodiDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NodiDataType>
	{
		protected override NodiDataType GetNewDataType() => new NodiDataType();
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var customsProductionRegistry = new NodiRegistry
			{
				SystemName = NodiRegistry.CustomsProductionSystemName,
				NodiNumber = NodiRegistry.CustomsProductionDefaultNodiNumber
			};

			var nctsProductionRegistry = new NodiRegistry
			{
				SystemName = NodiRegistry.NctsProductionSystemName,
				NodiNumber = NodiRegistry.NctsProductionDefaultNodiNumber
			};

			var collection = new NodiRegistryCollection { customsProductionRegistry, nctsProductionRegistry };
			var emptyCollection = new NodiRegistryCollection();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(emptyCollection, DataType.Serialise(emptyCollection))
			};
		}

		protected override string ExpectedEditorName => "NodiRegistryItemEditor";
	}
}
