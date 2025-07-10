using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DomesticPhoneNoValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(DomesticPhoneNoValidator.DomesticPhoneNoFormat, DomesticPhoneNoValidator.Validate(""));
			AssertEquals(DomesticPhoneNoValidator.DomesticPhoneNoFormat, DomesticPhoneNoValidator.Validate("061234"));
			AssertEquals("", DomesticPhoneNoValidator.Validate("3459500298"));
			AssertEquals(DomesticPhoneNoValidator.DomesticPhoneNoFormat, DomesticPhoneNoValidator.Validate("3A5"));
			AssertEquals(DomesticPhoneNoValidator.DomesticPhoneNoFormat, DomesticPhoneNoValidator.Validate("3 5"));
			AssertEquals(DomesticPhoneNoValidator.DomesticPhoneNoFormat, DomesticPhoneNoValidator.Validate("+1 82 9384"));
			AssertEquals(DomesticPhoneNoValidator.DomesticPhoneNoFormat, DomesticPhoneNoValidator.Validate("1829384"));
			AssertEquals("", DomesticPhoneNoValidator.Validate("8293845001"));
			AssertEquals(DomesticPhoneNoValidator.DomesticPhoneNoFormat, DomesticPhoneNoValidator.Validate("1829384500"));
		}

		public void TestIsValidSuretyCode()
		{
			AssertEquals(false, DomesticPhoneNoValidator.IsDomesticPhoneNo(""));
			AssertEquals(false, DomesticPhoneNoValidator.IsDomesticPhoneNo("061234"));
			AssertEquals(false, DomesticPhoneNoValidator.IsDomesticPhoneNo("345"));
			AssertEquals(false, DomesticPhoneNoValidator.IsDomesticPhoneNo("3 5"));
			AssertEquals(false, DomesticPhoneNoValidator.IsDomesticPhoneNo("3A5"));
			AssertEquals(false, DomesticPhoneNoValidator.IsDomesticPhoneNo("+1 82 9384"));
			AssertEquals(false, DomesticPhoneNoValidator.IsDomesticPhoneNo("1829384"));
			AssertEquals(false, DomesticPhoneNoValidator.IsDomesticPhoneNo("1829384500"));
			AssertEquals(true, DomesticPhoneNoValidator.IsDomesticPhoneNo("8293845001"));
		}
	}
}
