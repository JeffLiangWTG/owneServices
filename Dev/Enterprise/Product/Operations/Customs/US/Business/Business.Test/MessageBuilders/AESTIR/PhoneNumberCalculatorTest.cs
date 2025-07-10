namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PhoneNumberCalculatorTest : NUnit.Framework.TestCase
	{
		public void TestGetUnformattedPhoneNumber()
		{
			AssertEquals("5555555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+15555555555", false));
			AssertEquals("15555555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+15555555555", true));
			AssertEquals("5555555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+1 (555) 555-5555", false));
			AssertEquals("15555555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+1 (555) 555-5555", true));
			AssertEquals("5555555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+1 555 555-5555", false));
			AssertEquals("15555555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+1 555 555-5555", true));
			AssertEquals("5555555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+1(555) 555-5555", false));
			AssertEquals("15555555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+1(555) 555-5555", true));
			AssertEquals("12345555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+44 1234 555 5555", false));
			AssertEquals("4412345555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("+44 1234 555-5555", true));
			AssertEquals("4412345555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("01144 1234 555 5555", true));
			AssertEquals("12345555555", PhoneNumberCalculator.GetUnformattedPhoneNumber("01144 1234 555 5555", false));
			AssertEquals("9495128885", PhoneNumberCalculator.GetUnformattedPhoneNumber("+1 949-512-8885", false));
			AssertEquals("5556316701", PhoneNumberCalculator.GetUnformattedPhoneNumber("+52 55 5631 6701", false));
		}
	}
}
