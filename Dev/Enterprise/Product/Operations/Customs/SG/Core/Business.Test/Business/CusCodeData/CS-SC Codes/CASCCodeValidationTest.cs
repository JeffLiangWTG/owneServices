using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CASCCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCY_Data()
		{
			CASCCode1 cASCCode1 = Factory.New<CASCCode1>();
			cASCCode1.Validation.ValidateCY_Data();
			AssertEquals(true, cASCCode1.CY_DataInfo.HasErrors());
			cASCCode1.CY_Data = "TEST";
			cASCCode1.Validation.ValidateCY_Data();
			AssertEquals(false, cASCCode1.CY_DataInfo.HasErrors());
		}
	}
}
