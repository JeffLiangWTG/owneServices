using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FIRMSCodeValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(FIRMSCodeValidator.FIRMSCodeRightFormat, FIRMSCodeValidator.Validate(""));
			AssertEquals(FIRMSCodeValidator.FIRMSCodeRightFormat, FIRMSCodeValidator.Validate("123"));
			AssertEquals(FIRMSCodeValidator.FIRMSCodeRightFormat, FIRMSCodeValidator.Validate("1 31"));
			AssertEquals("", FIRMSCodeValidator.Validate("12a4"));
		}

		//AAAA
		public void TestIsValidDUNS()
		{
			AssertEquals(false, FIRMSCodeValidator.IsValidFIRMS(""));
			AssertEquals(false, FIRMSCodeValidator.IsValidFIRMS("061"));
			AssertEquals(false, FIRMSCodeValidator.IsValidFIRMS("13-1"));
			AssertEquals(true, FIRMSCodeValidator.IsValidFIRMS("1A31"));
		}
	}
}
