using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BulkSailingConsolGeneratorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestVolumeUnit()
		{
			Generator.WeightUnit = "XX";
			AssertHasError(Generator.WeightUnitInfo, "Enter a valid selection.");

			Generator.WeightUnit = "KG";
			AssertNoNotifications(Generator.WeightUnitInfo);

			Generator.WeightUnit = "";
			AssertHasError(Generator.WeightUnitInfo, "Please enter a value.");
		}

		public void TestWeightUnit()
		{
			Generator.VolumeUnit = "XX";
			AssertHasError(Generator.VolumeUnitInfo, "Enter a valid selection.");

			Generator.VolumeUnit = "M3";
			AssertNoNotifications(Generator.VolumeUnitInfo);

			Generator.VolumeUnit = "";
			AssertHasError(Generator.VolumeUnitInfo, "Please enter a value.");
		}

		#region Implementation

		BulkSailingConsolGenerator Generator
		{
			get { return generator ?? (generator = new BulkSailingConsolGeneratorForTest(Factory)); }
		}

		BulkSailingConsolGenerator generator;

		#endregion
	}
}
