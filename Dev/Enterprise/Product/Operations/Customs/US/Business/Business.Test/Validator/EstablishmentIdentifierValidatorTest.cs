using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EstablishmentIdentifierValidatorTest : TestCase
	{
		public void TestValidate()
		{
			AssertEquals(EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat, EstablishmentIdentifierValidator.Validate(""));
			AssertEquals("", EstablishmentIdentifierValidator.Validate("061234"));
			AssertEquals("", EstablishmentIdentifierValidator.Validate("0123456789"));
			AssertEquals(EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat, EstablishmentIdentifierValidator.Validate("003545456325"));
			AssertEquals(EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat, EstablishmentIdentifierValidator.Validate("123545456325"));
			AssertEquals(EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat, EstablishmentIdentifierValidator.Validate("3A5456321459"));
			AssertEquals(EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat, EstablishmentIdentifierValidator.Validate("3 5"));
		}

		public void TestIsValidEstablishmentIdentifier()
		{
			AssertEquals(false, EstablishmentIdentifierValidator.IsValidEstablishmentIdentifier(""));
			AssertEquals(true, EstablishmentIdentifierValidator.IsValidEstablishmentIdentifier("061234"));
			AssertEquals(true, EstablishmentIdentifierValidator.IsValidEstablishmentIdentifier("1234567890"));
			AssertEquals(false, EstablishmentIdentifierValidator.IsValidEstablishmentIdentifier("003545456325"));
			AssertEquals(false, EstablishmentIdentifierValidator.IsValidEstablishmentIdentifier("123545456325"));
			AssertEquals(false, EstablishmentIdentifierValidator.IsValidEstablishmentIdentifier("3 5"));
			AssertEquals(false, EstablishmentIdentifierValidator.IsValidEstablishmentIdentifier("3A5456328965"));
		}
	}
}
