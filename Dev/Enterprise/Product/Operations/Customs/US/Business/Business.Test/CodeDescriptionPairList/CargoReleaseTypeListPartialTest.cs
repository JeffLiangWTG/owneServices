using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CargoReleaseTypeListTest : TestCase
	{
		public void TestIsACECargoReleaseType()
		{
			AssertEquals(true, CargoReleaseTypeList.IsACECargoReleaseType(CargoReleaseTypeList.Codes.ACE));
			AssertEquals(false, CargoReleaseTypeList.IsACECargoReleaseType(CargoReleaseTypeList.Codes.ACS));
			AssertEquals(false, CargoReleaseTypeList.IsACECargoReleaseType(CargoReleaseTypeList.Codes.BCR));
			AssertEquals(false, CargoReleaseTypeList.IsACECargoReleaseType(CargoReleaseTypeList.Codes.CR));
			AssertEquals(true, CargoReleaseTypeList.IsACECargoReleaseType(CargoReleaseTypeList.Codes.SE));
		}
	}
}
