using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutomaticDeferredSelectionRegistryItem))]
	sealed class AutomaticDeferedSelectionRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<AutomaticDeferredSelection>
	{
		protected override StronglyTypedRegistryItem<AutomaticDeferredSelection, AutomaticDeferredSelection> GetNewRegistryItem()
		{
			return new AutomaticDeferredSelectionRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}

	[TestedType(typeof(AutomaticDeferredSelectionRegistryDataType))]
	sealed class AutomaticDeferedSelectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AutomaticDeferredSelectionRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "AutomaticDeferredSelectionRegistryItemEditor"; }
		}

		protected override AutomaticDeferredSelectionRegistryDataType GetNewDataType()
		{
			return new AutomaticDeferredSelectionRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var automaticDeferedSelection1 = new AutomaticDeferredSelection();
			automaticDeferedSelection1.AllowAutomaticDeferredSelection = false;
			var automaticDeferedSelection2 = new AutomaticDeferredSelection();
			automaticDeferedSelection2.AllowAutomaticDeferredSelection = true;
			automaticDeferedSelection2.DaysBeforeETA = 2;

			var dataType = new AutomaticDeferredSelectionRegistryDataType();
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(automaticDeferedSelection1, dataType.Serialise(automaticDeferedSelection1)),
				new ValidSampleAndBinaryValueInDB(automaticDeferedSelection2, dataType.Serialise(automaticDeferedSelection2))
			};
		}
	}
}
