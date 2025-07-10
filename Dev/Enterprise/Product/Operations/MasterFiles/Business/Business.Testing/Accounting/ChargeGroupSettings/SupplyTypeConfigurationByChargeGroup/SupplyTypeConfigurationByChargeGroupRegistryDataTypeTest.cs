using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SupplyTypeConfigurationByChargeGroupRegistryDataType))]
	sealed class SupplyTypeConfigurationByChargeGroupRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SupplyTypeConfigurationByChargeGroupRegistryDataType>
	{
		#region Implementation

		protected override SupplyTypeConfigurationByChargeGroupRegistryDataType GetNewDataType()
		{
			return new SupplyTypeConfigurationByChargeGroupRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "SupplyTypeConfigurationByChargeGroupRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new SupplyTypeConfigurationByChargeGroupCollection();
			var config1 = collection.AddNew();
			config1.ChargeGroup = "001";

			var collection2 = new SupplyTypeConfigurationByChargeGroupCollection();
			var config2 = collection2.AddNew();
			config2.ChargeGroup = "002";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new SupplyTypeConfigurationByChargeGroupRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new SupplyTypeConfigurationByChargeGroupRegistryDataType().Serialise(collection2))
			};
		}

		#endregion
	}
}
