using CargoWise.ComponentModel;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CALicenceNumberValidationTest : CusCodeDataValidationTest
	{
		public void TestCY_Data()
		{
			CALicenceNumber cALicenceNumber = Factory.New<CALicenceNumber>();
			cALicenceNumber.Validation.ValidateCY_Data();
			AssertEquals(true, cALicenceNumber.CY_DataInfo.HasErrors());
			cALicenceNumber.CY_Data = "TEST";
			cALicenceNumber.Validation.ValidateCY_Data();
			AssertEquals(false, cALicenceNumber.CY_DataInfo.HasErrors());
		}
	}
}
