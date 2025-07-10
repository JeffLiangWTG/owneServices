using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusVehicleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCVH_Mileage()
		{
			var vehicle = Factory.New<CusVehicle>();

			ValidationTestHelper.AssertErrorIfValueIsNegative(vehicle.CVH_MileageInfo);
		}

		public void TestCheckCVH_MileageUQ()
		{
			var vehicle = Factory.New<CusVehicle>();

			ValidationTestHelper.AssertErrorIfInvalidCode(vehicle.CVH_MileageUQInfo, "LL", "KM");

			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(vehicle.CVH_MileageUQInfo, vehicle.CVH_MileageInfo);
		}
	}
}
