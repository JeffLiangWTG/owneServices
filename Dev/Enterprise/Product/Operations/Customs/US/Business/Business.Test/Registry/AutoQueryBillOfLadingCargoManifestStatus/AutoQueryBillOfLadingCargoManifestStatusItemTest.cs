using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutoQueryBillOfLadingCargoManifestStatusItem))]
	sealed class AutoQueryBillOfLadingCargoManifestStatusItemTest : StronglyTypedRegistryItemTestCaseWithFactory<AutoQueryBillOfLadingCargoManifestStatus>
	{
		protected override StronglyTypedRegistryItem<AutoQueryBillOfLadingCargoManifestStatus, AutoQueryBillOfLadingCargoManifestStatus> GetNewRegistryItem()
			=> new AutoQueryBillOfLadingCargoManifestStatusItem("", null, null, null, RegistryStorageFlags.System);
	}

	[TestedType(typeof(AutoQueryBillOfLadingCargoManifestStatusItemDataType))]
	sealed class AutoQueryBillOfLadingCargoManifestStatusItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AutoQueryBillOfLadingCargoManifestStatusItemDataType>
	{
		protected override string ExpectedEditorName => "AutoQueryBillOfLadingCargoManifestStatusRegistryItemEditor";

		protected override AutoQueryBillOfLadingCargoManifestStatusItemDataType GetNewDataType() => new AutoQueryBillOfLadingCargoManifestStatusItemDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var data1 = new AutoQueryBillOfLadingCargoManifestStatus
			{
				SendOnFirstSave = true,
				SendBasedOnETA = true,
				UpdateEntryWithResults = true
			};

			var data2 = new AutoQueryBillOfLadingCargoManifestStatus
			{
				SendOnFirstSave = false,
				SendBasedOnETA = false,
				UpdateEntryWithResults = false
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(data1, new AutoQueryBillOfLadingCargoManifestStatusItemDataType().Serialise(data1)),
				new ValidSampleAndBinaryValueInDB(data2, new AutoQueryBillOfLadingCargoManifestStatusItemDataType().Serialise(data2))
			};
		}
	}
}
