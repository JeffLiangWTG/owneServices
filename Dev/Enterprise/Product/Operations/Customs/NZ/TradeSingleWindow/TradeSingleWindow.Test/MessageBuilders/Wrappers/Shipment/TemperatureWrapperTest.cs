using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class TemperatureWrapperTest : TestCaseWithFactory
	{
		public void TestTemperatureRequirements()
		{
			var temperatureRequirements = new TemperatureWrapper(5m, -8m, 25m);
			AssertEquals(25m, temperatureRequirements.MaxStorageTemp);
			AssertEquals("CEL", temperatureRequirements.MaxStorageTempUnit);
			AssertEquals(-8m, temperatureRequirements.MinStorageTemp);
			AssertEquals("CEL", temperatureRequirements.MinStorageTempUnit);
			AssertEquals(5m, temperatureRequirements.StorageTemp);
			AssertEquals("CEL", temperatureRequirements.StorageTempUnit);
		}
	}
}
