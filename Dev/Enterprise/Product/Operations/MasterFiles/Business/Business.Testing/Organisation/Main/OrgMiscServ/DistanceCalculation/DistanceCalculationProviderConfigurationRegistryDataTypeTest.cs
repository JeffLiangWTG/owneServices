using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DistanceCalculationProviderConfigurationRegistryDataType))]
	sealed class DistanceCalculationProviderConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DistanceCalculationProviderConfigurationRegistryDataType>
	{
		#region Implementation

		protected override DistanceCalculationProviderConfigurationRegistryDataType GetNewDataType()
		{
			return new DistanceCalculationProviderConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DistanceCalculationProviderConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DistanceCalculationProviderConfiguration config = DistanceCalculationProviderConfiguration.GetDefault();

			#region ByteArrayValue

			byte[] byteArrayValue = new byte[]
{
255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,68,0,105,0,115,0,116,0,97,0,110,0,99,0,101,0,67,0,97,0,108,0,99,0,117,0,108,0,97,0,116,0,105,0,111,0,110,0,80,0,114,0,111,0,118,0,105,0,100,
0,101,0,114,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,80,0,114,0,111,0,118,0,105,0,100,0,101,0,114,0,62,0,67,0,87,0,83,0,60,0,47,0,80,0,114,
0,111,0,118,0,105,0,100,0,101,0,114,0,62,0,60,0,86,0,101,0,114,0,115,0,105,0,111,0,110,0,32,0,47,0,62,0,60,0,67,0,97,0,108,0,99,0,117,0,108,0,97,0,116,0,105,0,111,0,110,0,77,0,101,0,116,
0,104,0,111,0,100,0,32,0,47,0,62,0,60,0,47,0,68,0,105,0,115,0,116,0,97,0,110,0,99,0,101,0,67,0,97,0,108,0,99,0,117,0,108,0,97,0,116,0,105,0,111,0,110,0,80,0,114,0,111,0,118,0,105,0,100,
0,101,0,114,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0
};

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(config, byteArrayValue)
			};
		}

		#endregion
	}
}
