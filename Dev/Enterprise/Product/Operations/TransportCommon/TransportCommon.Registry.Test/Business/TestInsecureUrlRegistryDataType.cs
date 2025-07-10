using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(InsecureUrlRegistryDataType))]
	class TestInsecureUrlRegistryDataType : RegistryDataTypeTestCase<InsecureUrlRegistryDataType>
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("http://valid", Encoding.Unicode.GetBytes("http://valid")),
				new ValidSampleAndBinaryValueInDB("http://42.42.42.42", Encoding.Unicode.GetBytes("http://42.42.42.42"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "https://invalid", "invalid", "invalid.com", "https://42.42.42.42" };
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override InsecureUrlRegistryDataType GetNewDataType()
		{
			return new InsecureUrlRegistryDataType();
		}
	}
}
