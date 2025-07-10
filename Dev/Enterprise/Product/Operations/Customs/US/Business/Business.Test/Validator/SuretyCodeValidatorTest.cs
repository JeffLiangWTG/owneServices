using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SuretyCodeValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(SuretyCodeValidator.SuretyCodeRightFormat, SuretyCodeValidator.Validate(""));
			AssertEquals(SuretyCodeValidator.SuretyCodeRightFormat, SuretyCodeValidator.Validate("061234"));
			AssertEquals("", SuretyCodeValidator.Validate("345"));
			AssertEquals(SuretyCodeValidator.SuretyCodeRightFormat, SuretyCodeValidator.Validate("3A5"));
			AssertEquals(SuretyCodeValidator.SuretyCodeRightFormat, SuretyCodeValidator.Validate("3 5"));
		}

		//NNN
		public void TestIsValidSuretyCode()
		{
			AssertEquals(false, SuretyCodeValidator.IsValidSuretyCode(""));
			AssertEquals(false, SuretyCodeValidator.IsValidSuretyCode("061234"));
			AssertEquals(true, SuretyCodeValidator.IsValidSuretyCode("345"));
			AssertEquals(false, SuretyCodeValidator.IsValidSuretyCode("3 5"));
			AssertEquals(false, SuretyCodeValidator.IsValidSuretyCode("3A5"));
		}
	}
}
