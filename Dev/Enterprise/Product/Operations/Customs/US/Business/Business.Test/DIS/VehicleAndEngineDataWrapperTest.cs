using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.DIS.Testing
{
	sealed class VehicleAndEngineDataWrapperTest : TestCaseWithFactory
	{
		public void TestDetails()
		{
			var vehicle = Factory.New<Vehicle>();
			vehicle.US_VehicleModel = "Model";

			var vehicleDetails = vehicle.VehicleAndEngineDetails.AddNew();
			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetails.US_IdentityNumber = "VIN";
			vehicleDetails.US_BuildMonth = MonthList.Codes._05;
			vehicleDetails.US_BuildYear = "2009";
			vehicleDetails.US_EngineNumber = "E1";
			vehicleDetails.US_EngineBuildDate = ZDateTime.BrettsBirthday.AddYears(41);
			vehicleDetails.US_EngineModel = "EngineModel";
			vehicleDetails.US_EngineNumber = "engineNumber";
			vehicleDetails.US_EngineManufacturer = "engine manufacturer";

			var wrapper = new VehicleAndEngineDataWrapper(vehicleDetails);
			AssertEquals(ZDateTime.BrettsBirthday.AddYears(41), wrapper.EngineManufactureDate);
			AssertEquals("engine manufacturer", wrapper.EngineManufacturer);
			AssertEquals("EngineModel", wrapper.EngineModel);
			AssertEquals("engineNumber", wrapper.EngineSerialNumber);
			AssertEquals(MonthList.Codes._05, wrapper.ManufactureMonth);
			AssertEquals("2009", wrapper.ManufactureYear);
			AssertEquals(ZString.Empty, wrapper.SerialNumber);
			AssertEquals("VIN", wrapper.VIN);

			vehicleDetails.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			vehicleDetails.US_IdentityNumber = "SE1";
			AssertEquals(ZString.Empty, wrapper.VIN);
			AssertEquals("SE1", wrapper.SerialNumber);
		}
	}
}
