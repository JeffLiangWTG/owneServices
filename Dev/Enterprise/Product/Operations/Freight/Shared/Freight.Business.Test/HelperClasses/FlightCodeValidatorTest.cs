using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FlightCodeValidatorTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			AssertEquals("Flight number empty, error expected", false, FlightCodeValidator.IsValid(ZString.Empty));
			AssertEquals("Flight number has more than 7 characters, error expected.", false, FlightCodeValidator.IsValid("12345678"));
			AssertEquals("Either or both of the first two characters of the Flight number are not letters, error expected.", false, FlightCodeValidator.IsValid("123456"));
			AssertEquals("One of the characters 3-6 of the Flight number are letters, error expected.", false, FlightCodeValidator.IsValid("AAA456"));
			AssertEquals("One of the characters 3-6 of the Flight number are letters, error expected.", false, FlightCodeValidator.IsValid("A2A456"));
			AssertEquals("Flight number are contains non-alphanumeric character, error expected.", false, FlightCodeValidator.IsValid("A234!6"));
			AssertEquals("Valid flight number, not expecting errors.", true, FlightCodeValidator.IsValid("QF43"));
			AssertEquals("Valid flight number, not expecting errors.", true, FlightCodeValidator.IsValid("F43"));
			AssertEquals("Valid flight number, not expecting errors.", true, FlightCodeValidator.IsValid("1F1"));
			AssertEquals("Valid flight number, not expecting errors.", true, FlightCodeValidator.IsValid("QF1234"));
			AssertEquals("Valid flight number, not expecting errors.", true, FlightCodeValidator.IsValid("QF1234V"));
			AssertEquals("Valid flight number, not expecting errors.", true, FlightCodeValidator.IsValid("QF12V"));
		}
	}
}
