using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(StampDutyLedgerNumberCustomizationRegistryItem))]
	class StampDutyLedgerNumberCustomizationRegistryItemTest : StronglyTypedRegistryItemTestCase<StampDutyLedgerNumberCustomizationRegistrySetting>
	{
		protected override StronglyTypedRegistryItem<StampDutyLedgerNumberCustomizationRegistrySetting, StampDutyLedgerNumberCustomizationRegistrySetting> GetNewRegistryItem()
		{
			return new StampDutyLedgerNumberCustomizationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new StampDutyLedgerNumberCustomizationRegistrySetting()
			{
				ExpiredYear = ZDateTime.Today.Year
			});
		}
	}

	[TestedType(typeof(StampDutyLedgerNumberCustomizationRegistryItemDataType))]
	class StampDutyLedgerNumberCustomizationRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<StampDutyLedgerNumberCustomizationRegistryItemDataType>
	{
		protected override StampDutyLedgerNumberCustomizationRegistryItemDataType GetNewDataType()
		{
			return new StampDutyLedgerNumberCustomizationRegistryItemDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new StampDutyLedgerNumberCustomizationRegistrySetting { StartNumber = 1, ExpiredYear = ZDateTime.Now.Year };
			var sample2 = new StampDutyLedgerNumberCustomizationRegistrySetting { StartNumber = 3456, ExpiredYear = ZDateTime.Now.Year + 1 };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, new StampDutyLedgerNumberCustomizationRegistryItemDataType().Serialise(sample1)),
				new ValidSampleAndBinaryValueInDB(sample2, new StampDutyLedgerNumberCustomizationRegistryItemDataType().Serialise(sample2))
			};
		}

		protected override string ExpectedEditorName => "StampDutyLedgerNumberCustomizationRegistryItemEditor";
	}
}
