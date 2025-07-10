using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class PassportNumberValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(PassportNumberValidator.PassportNumberRightFormat, PassportNumberValidator.Validate(""));
			AssertEquals(PassportNumberValidator.PassportNumberRightFormat, PassportNumberValidator.Validate("1234"));
			AssertEquals(PassportNumberValidator.PassportNumberRightFormat, PassportNumberValidator.Validate("123-56-S12"));
			AssertEquals(PassportNumberValidator.PassportNumberRightFormat, PassportNumberValidator.Validate("1234564568"));
			AssertEquals("", PassportNumberValidator.Validate("N123456A1"));
			AssertEquals("", PassportNumberValidator.Validate("123456"));
			AssertEquals("", PassportNumberValidator.Validate("123456789"));
		}

		public void TestIsValidPassport()
		{
			AssertEquals(false, PassportNumberValidator.IsValidPassport(""));
			AssertEquals(false, PassportNumberValidator.IsValidPassport("1234"));
			AssertEquals(false, PassportNumberValidator.IsValidPassport("123-456-S12"));
			AssertEquals(false, PassportNumberValidator.IsValidPassport("1234564555"));
			AssertEquals(true, PassportNumberValidator.IsValidPassport("N123456A1"));
			AssertEquals(true, PassportNumberValidator.IsValidPassport("123456"));
			AssertEquals(true, PassportNumberValidator.IsValidPassport("123456789"));
		}
	}
}
