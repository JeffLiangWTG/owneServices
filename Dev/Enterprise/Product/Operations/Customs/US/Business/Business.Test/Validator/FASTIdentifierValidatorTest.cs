using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FASTIdentifierValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(FASTIdentifierValidator.FASTIdRightFormat, FASTIdentifierValidator.Validate(string.Empty));
			AssertEquals(FASTIdentifierValidator.FASTIdRightFormat, FASTIdentifierValidator.Validate("123"));
			AssertEquals(FASTIdentifierValidator.FASTIdRightFormat, FASTIdentifierValidator.Validate("12345 a"));
			AssertEquals(string.Empty, FASTIdentifierValidator.Validate("123A456"));
		}
	}
}
