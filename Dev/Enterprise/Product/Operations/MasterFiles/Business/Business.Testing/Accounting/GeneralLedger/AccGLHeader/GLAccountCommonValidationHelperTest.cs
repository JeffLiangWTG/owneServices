using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GLAccountCommonValidationHelperTest : TestCaseWithFactory
	{
		public void TestCompareGLAccount()
		{
			AssertEquals(1234.123m, GLAccountCommonValidationHelper.ConvertToDecimal("1234.12.30"));
			AssertEquals(1234.12m, GLAccountCommonValidationHelper.ConvertToDecimal("1234.12"));
			AssertEquals(1234m, GLAccountCommonValidationHelper.ConvertToDecimal("1234"));
			AssertEquals(1234.12m, GLAccountCommonValidationHelper.ConvertToDecimal("1234.120"));
			AssertEquals(1234.12m, GLAccountCommonValidationHelper.ConvertToDecimal("1234.1200"));
			AssertEquals(1234m, GLAccountCommonValidationHelper.ConvertToDecimal("1234.000"));
			AssertEquals(1234.011m, GLAccountCommonValidationHelper.ConvertToDecimal("1234.0.1.1"));
			AssertEquals(1234.01m, GLAccountCommonValidationHelper.ConvertToDecimal("1234.0.1.0"));
		}
	}
}
