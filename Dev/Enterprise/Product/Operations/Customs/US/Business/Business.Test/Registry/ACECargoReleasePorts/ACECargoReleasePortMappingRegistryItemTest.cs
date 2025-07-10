using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(ACECargoReleaseTypePortRegistryItem))]
	sealed class ACECargoReleaseTypePortRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<ACECargoReleaseTypePortMapping>
	{
		protected override StronglyTypedRegistryItem<ACECargoReleaseTypePortMapping, ACECargoReleaseTypePortMapping> GetNewRegistryItem() => new ACECargoReleaseTypePortRegistryItem("", null, null, null, RegistryStorageFlags.System);
	}

	[TestedType(typeof(ACECargoReleaseTypePortRegistryDataType))]
	sealed class ACECargoReleaseTypePortRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ACECargoReleaseTypePortRegistryDataType>
	{
		protected override string ExpectedEditorName => "ACECargoReleaseTypePortMappingItemEditor";

		protected override ACECargoReleaseTypePortRegistryDataType GetNewDataType() => new ACECargoReleaseTypePortRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var data = new ACECargoReleaseTypePortMapping();
			data.FillWithValidTestData();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(data, new ACECargoReleaseTypePortRegistryDataType().Serialise(data))
			};
		}
	}
}
