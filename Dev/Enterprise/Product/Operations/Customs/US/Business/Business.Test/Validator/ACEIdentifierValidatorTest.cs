using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEIdentifierValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(ACEIdentifierValidator.ACEIdRightFormat, ACEIdentifierValidator.Validate(string.Empty));
			AssertEquals(ACEIdentifierValidator.ACEIdRightFormat, ACEIdentifierValidator.Validate("123"));
			AssertEquals(ACEIdentifierValidator.ACEIdRightFormat, ACEIdentifierValidator.Validate("12345 a789"));
			AssertEquals(string.Empty, ACEIdentifierValidator.Validate("123A456B78"));
		}
	}
}
