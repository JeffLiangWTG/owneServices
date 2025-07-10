using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(ASNRefreshOptionsConfigRegistryItemDataType))]
	sealed class ASNRefreshOptionsConfigRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ASNRefreshOptionsConfigRegistryItemDataType>
	{
		protected override ASNRefreshOptionsConfigRegistryItemDataType GetNewDataType()
		{
			return new ASNRefreshOptionsConfigRegistryItemDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ASNRefreshOptionsConfigRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject obj1, NonPersistentBusinessObject obj2)
		{
			var config1 = (ASNRefreshOptionsConfig)obj1;
			var config2 = (ASNRefreshOptionsConfig)obj2;

			AssertEquals(config1.FieldType, config2.FieldType);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new ASNRefreshOptionsConfigCollection();
			var config1 = collection1.AddNew();
			config1.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification;

			var collection2 = new ASNRefreshOptionsConfigCollection();
			var config2 = collection1.AddNew();
			config2.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.CountryOfOrigin;

			return
			[
				new ValidSampleAndBinaryValueInDB(collection1, new ASNRefreshOptionsConfigRegistryItemDataType().Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, new ASNRefreshOptionsConfigRegistryItemDataType().Serialise(collection2))
			];
		}
	}
}
