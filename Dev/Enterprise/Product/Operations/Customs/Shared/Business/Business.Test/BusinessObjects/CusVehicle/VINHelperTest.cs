using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	class VINHelperTest : TestCaseWithFactory
	{
		public void TestCheckCVH_VehicleIdentificationNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("vin has no 17 digits", false, VINHelper.IsValidVINWithTheNinthDigitCheck("12"));
				AssertEquals("vin has 17 digits but contain invalid char 'I'", false, VINHelper.IsValidVINWithTheNinthDigitCheck("123456789ABCDEFGI"));
				AssertEquals("vin has 17 digits but contain invalid char 'O'", false, VINHelper.IsValidVINWithTheNinthDigitCheck("123456789ABCDEFGO"));
				AssertEquals("vin has 17 digits but contain invalid char 'Q'", false, VINHelper.IsValidVINWithTheNinthDigitCheck("123456789ABCDEFGQ"));
				AssertEquals("vin has 17 digits but contain invalid char '$'", false, VINHelper.IsValidVINWithTheNinthDigitCheck("123456789ABCDEFG$"));
				AssertEquals("vin has 17 digits and no invalid char but failed through the ninth digit check'", false, VINHelper.IsValidVINWithTheNinthDigitCheck("0123456789ABCDEFG"));
				AssertEquals("valid vin", true, VINHelper.IsValidVINWithTheNinthDigitCheck("UU6JA69691D713820"));
				AssertEquals("vin is empty", false, VINHelper.IsValidVINWithTheNinthDigitCheck(ZString.Empty));
			});
		}
	}
}
