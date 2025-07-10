using CargoWise.ComponentModel;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CMDPermitNumberValidationTest : CusCodeDataValidationTest
	{
		public void TestCY_Data()
		{
			var cmdPermitNumber = Factory.New<CMDPermitNumber>();
			cmdPermitNumber.Validation.ValidateCY_Data();
			AssertEquals(true, cmdPermitNumber.CY_DataInfo.HasErrors());
			cmdPermitNumber.CY_Data = "TEST";
			cmdPermitNumber.Validation.ValidateCY_Data();
			AssertEquals(false, cmdPermitNumber.CY_DataInfo.HasErrors());
		}
	}
}
