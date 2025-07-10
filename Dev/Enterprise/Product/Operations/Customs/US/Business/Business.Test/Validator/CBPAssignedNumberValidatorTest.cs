using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CBPAssignedNumberValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(CBPAssignedNumberValidator.CBPAssignedNumberRightFormat, CBPAssignedNumberValidator.Validate(""));
			AssertEquals(CBPAssignedNumberValidator.CBPAssignedNumberRightFormat, CBPAssignedNumberValidator.Validate("061234"));
			AssertEquals("", CBPAssignedNumberValidator.Validate("061234-12345"));
		}

		//YYDDPP-NNNNN
		public void TestIsValidCBPAssignedNumber()
		{
			AssertEquals(false, CBPAssignedNumberValidator.IsValidCBPAssignedNumber(""));
			AssertEquals(false, CBPAssignedNumberValidator.IsValidCBPAssignedNumber("061234"));
			AssertEquals(true, CBPAssignedNumberValidator.IsValidCBPAssignedNumber("061234-12345"));
		}
	}
}
