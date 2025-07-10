using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public class CusCodeDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			DummyCusCodeData cusCode = Factory.New<DummyCusCodeData>();
			cusCode.CY_Code = ZString.Empty;
			AssertHasErrorContaining(cusCode.CY_CodeInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrorContaining(cusCode.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			cusCode.CY_Code = "Z~Z";
			AssertNoErrorContaining(cusCode.CY_CodeInfo, MandatoryValidation.MustBeEntered);
			AssertHasMessageErrorContaining(cusCode.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
