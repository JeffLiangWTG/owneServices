using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Registry.Business.Testing
{
	[TestedType(typeof(CargoGuideApiSettingsRegistryItem))]
	class CargoGuideApiSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<CargoGuideApiSettings>
	{
		protected override StronglyTypedRegistryItem<CargoGuideApiSettings, CargoGuideApiSettings> GetNewRegistryItem()
		{
			return new CargoGuideApiSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}

	[TestedType(typeof(CargoGuideApiSettingsRegistryDataType))]
	class CargoGuideApiSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CargoGuideApiSettingsRegistryDataType>
	{
		#region Implementation

		protected override CargoGuideApiSettingsRegistryDataType GetNewDataType()
		{
			return new CargoGuideApiSettingsRegistryDataType();
		}

		protected override string ExpectedEditorName => "CargoGuideApiSettingsRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var item1 = new CargoGuideApiSettings { ApiURL = "https://www.cargoguide.info", ApiVersion = "1.1" };

			byte[] byteArrayValue1 =
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,67,0,97,0,114,0,103,0,111,0,71,0,117,0,105,0,100,0,101,0,65,0,112,0,105,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,65,0,112,0,105,
				0,85,0,82,0,76,0,62,0,104,0,116,0,116,0,112,0,115,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,99,0,97,0,114,0,103,0,111,0,103,0,117,0,105,0,100,0,101,0,46,0,105,0,110,0,102,0,111,0,60,0,47,
				0,65,0,112,0,105,0,85,0,82,0,76,0,62,0,60,0,65,0,112,0,105,0,86,0,101,0,114,0,115,0,105,0,111,0,110,0,62,0,49,0,46,0,49,0,60,0,47,0,65,0,112,0,105,0,86,0,101,0,114,0,115,0,105,0,111,0,
				110,0,62,0,60,0,47,0,67,0,97,0,114,0,103,0,111,0,71,0,117,0,105,0,100,0,101,0,65,0,112,0,105,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0
			};

			var item2 = new CargoGuideApiSettings();

			byte[] byteArrayValue2 =
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,67,0,97,0,114,0,103,0,111,0,71,0,117,0,105,0,100,0,101,0,65,0,112,0,105,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,65,0,112,0,105,
				0,85,0,82,0,76,0,32,0,47,0,62,0,60,0,65,0,112,0,105,0,86,0,101,0,114,0,115,0,105,0,111,0,110,0,32,0,47,0,62,0,60,0,47,0,67,0,97,0,114,0,103,0,111,0,71,0,117,0,105,0,100,0,101,0,65,
				0,112,0,105,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0
			};

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(item1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(item2, byteArrayValue2)
			};
		}

		#endregion
	}
}
