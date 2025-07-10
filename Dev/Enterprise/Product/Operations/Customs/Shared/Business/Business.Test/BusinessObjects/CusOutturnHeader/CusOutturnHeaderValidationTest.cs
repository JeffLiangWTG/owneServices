using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class CusOutturnHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestVesselNameValidation()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();

			header.C6_VesselName = "";
			AssertNoErrors("Not mandatory", header.C6_VesselNameInfo);

			header.C6_VesselName = "ZUBIN";
			AssertHasErrors("List validation", header.C6_VesselNameInfo);

			header.C6_VesselName = Factory.NewWithValidTestData<RefVessel>().RV_Code;
			AssertNoErrors("Valid vessel name", header.C6_VesselNameInfo);
		}
	}
}
