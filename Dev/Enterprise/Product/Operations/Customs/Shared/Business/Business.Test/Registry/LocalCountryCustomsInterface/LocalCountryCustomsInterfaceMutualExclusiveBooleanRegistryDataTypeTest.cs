using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryDataType))]
	sealed class LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryDataTypeTest : RegistryDataTypeTestCase<LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryDataType>
	{
		protected override LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryDataType GetNewDataType()
		{
			return new LocalCountryCustomsInterfaceMutualExclusiveBooleanRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(false, Encoding.Unicode.GetBytes("0")),
				new ValidSampleAndBinaryValueInDB(true, Encoding.Unicode.GetBytes("1")),
			};
		}
	}
}
