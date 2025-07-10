using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(DefaultFilerContactInformationRegistryDataType))]
	sealed class DefaultFilerContactInformationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultFilerContactInformationRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "DefaultFilerContactInformationRegistryItemEditor"; }
		}

		protected override DefaultFilerContactInformationRegistryDataType GetNewDataType()
		{
			return new DefaultFilerContactInformationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var copy = new DefaultFilerContactInformation();
			var copy2 = new DefaultFilerContactInformation();
			copy2.ContactName = "some random name";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(copy, DataType.Serialise(copy)),
				new ValidSampleAndBinaryValueInDB(copy2, DataType.Serialise(copy2))
			};
		}
	}
}
