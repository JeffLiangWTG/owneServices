using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SocialSecurityNumberValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(SocialSecurityNumberValidator.SocialSecurityNumberRightFormat, SocialSecurityNumberValidator.Validate(""));
			AssertEquals(SocialSecurityNumberValidator.SocialSecurityNumberRightFormat, SocialSecurityNumberValidator.Validate("123456"));
			AssertEquals("", SocialSecurityNumberValidator.Validate("123-12-1234"));
		}

		//NNN-NN-NNNN
		public void TestIsValidSSN()
		{
			AssertEquals(false, SocialSecurityNumberValidator.IsValidSSN(""));
			AssertEquals(false, SocialSecurityNumberValidator.IsValidSSN("061234"));
			AssertEquals(true, SocialSecurityNumberValidator.IsValidSSN("123-12-1234"));
		}
	}
}
