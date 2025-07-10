using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(LocalCountryCustomsInterfaceMutualExclusiveStringRegistryDataType))]
	sealed class LocalCountryCustomsInterfaceMutualExclusiveStringRegistryDataTypeTest : RegistryDataTypeTestCase<LocalCountryCustomsInterfaceMutualExclusiveStringRegistryDataType>
	{
		protected override LocalCountryCustomsInterfaceMutualExclusiveStringRegistryDataType GetNewDataType()
		{
			return new LocalCountryCustomsInterfaceMutualExclusiveStringRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB("abc", Encoding.Unicode.GetBytes("abc")),
				new ValidSampleAndBinaryValueInDB("def", Encoding.Unicode.GetBytes("def")),
			};
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
