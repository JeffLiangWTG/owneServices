namespace Enterprise.Customs.NZ.Business.Testing
{
	using NUnit.Framework;

	public class PackageTypeListTest : TestCase
	{
		public void TestIsBulkType()
		{
			PackageTypeList list = new PackageTypeList();
			AssertEquals("VA is not a bulk type", false, list.IsBulkType("VA"));			AssertEquals("VN is not a bulk type", false, list.IsBulkType("VN"));			AssertEquals("VG is a bulk type", true, list.IsBulkType("VG"));			AssertEquals("VI is not a bulk type", false, list.IsBulkType("VI"));			AssertEquals("VK is not a bulk type", false, list.IsBulkType("VK"));			AssertEquals("VL is a bulk type", true, list.IsBulkType("VL"));			AssertEquals("VO is a bulk type", true, list.IsBulkType("VO"));			AssertEquals("VP is not a bulk type", false, list.IsBulkType("VP"));			AssertEquals("VQ is a bulk type", true, list.IsBulkType("VQ"));			AssertEquals("VR is a bulk type", true, list.IsBulkType("VR"));			AssertEquals("VY is a bulk type", true, list.IsBulkType("VY"));
		}

		public void TestVehicleCodeIsVN()
		{
			AssertEquals("Vehicle should now have the code VN, WI00194967", "VN", PackageTypeList.Codes.Vehicle);
		}
	}
}
