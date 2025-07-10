using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EUTaxIDDefaultingRegistryDataType))]
	sealed class EUTaxIDDefaultingRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EUTaxIDDefaultingRegistryDataType>
	{
		protected override string ExpectedEditorName => "EUTaxIDDefaultingRegistryItemEditor";

		protected override EUTaxIDDefaultingRegistryDataType GetNewDataType()
		{
			return new EUTaxIDDefaultingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new EUTaxIDDefaultingRuleCollection();
			var rule = collection.AddNew();
			rule.Destination = "abc";

			var collection2 = new EUTaxIDDefaultingRuleCollection();
			var rule2 = collection2.AddNew();
			rule2.Destination = "xyz";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new EUTaxIDDefaultingRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new EUTaxIDDefaultingRegistryDataType().Serialise(collection2))
			};
		}
	}
}
