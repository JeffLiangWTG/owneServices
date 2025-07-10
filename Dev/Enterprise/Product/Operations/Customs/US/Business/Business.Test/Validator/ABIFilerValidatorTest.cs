using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ABIFilerValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(ABIFilerValidator.ABIFilerRightFormat, ABIFilerValidator.Validate(""));
			AssertEquals(ABIFilerValidator.ABIFilerRightFormat, ABIFilerValidator.Validate("061234"));
			AssertEquals("", ABIFilerValidator.Validate("0612ZJ534"));
			AssertEquals("", ABIFilerValidator.Validate("0612ZJ5"));
			AssertEquals(ABIFilerValidator.ABIFilerRightFormat, ABIFilerValidator.Validate("0612ZJ5345"));
		}

		//NNNNXXNN
		public void TestIsValidFiler()
		{
			AssertEquals(false, ABIFilerValidator.IsValidFiler(""));
			AssertEquals(false, ABIFilerValidator.IsValidFiler("061234"));
			AssertEquals(true, ABIFilerValidator.IsValidFiler("0612ZJ534"));
			AssertEquals(true, ABIFilerValidator.IsValidFiler("0612ZJ5"));
			AssertEquals(false, ABIFilerValidator.IsValidFiler("0612ZJ5345"));
		}
	}
}
