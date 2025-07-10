using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class MRNValidationHelperTest : TestCaseWithFactory
	{
		public void TestMRNFormat_LettersAndNumbers()
		{
			AssertEquals("MRN does not contain both letters and numbers", invalidMRNFormatError, CheckMRNFormatAndGetFirstError("219307502"));
			AssertEquals("MRN does not contain both letters and numbers", invalidMRNFormatError, CheckMRNFormatAndGetFirstError("DEBLAHBLAH"));
			AssertEquals("MRN does not contain both letters and numbers", invalidMRNFormatError, CheckMRNFormatAndGetFirstError("!-0*&^)w*($&"));
			AssertEquals("MRN does not contain both letters and numbers", invalidMRNFormatError, CheckMRNFormatAndGetFirstError("15DE-219307502"));
		}

		public void TestMRNFormat_Length()
		{
			AssertEquals("MRN is too short", invalidMRNFormatError, CheckMRNFormatAndGetFirstError("15DE"));
		}

		public void TestMRNFormat_LowerCase()
		{
			AssertEquals("MRN contains lower case letters", invalidMRNFormatError, CheckMRNFormatAndGetFirstError("15dE33344445dog55E2"));
		}

		public void TestMRNFormat_CheckDigit()
		{
			AssertEquals("MRN has the wrong check digit", invalidMRNFormatError, CheckMRNFormatAndGetFirstError("13DE485101472466XX"));

			ZString invalidCheckDigitError = "MRN does not have a valid check (last) digit. The check digit should be {0}";

			AssertEquals("Expected a message error as MRN has the wrong check digit", string.Format(invalidCheckDigitError, "9"), CheckMRNFormatAndGetFirstError("10AD00000117716870"));
			AssertEquals("Expected a message error as MRN has the wrong check digit", string.Format(invalidCheckDigitError, "2"), CheckMRNFormatAndGetFirstError("10AT100200TN8VECI5"));
			AssertEquals("Expected a message error as MRN has the wrong check digit", string.Format(invalidCheckDigitError, "1"), CheckMRNFormatAndGetFirstError("10DE210119797085T8"));
		}

		public void TestMRNFormat_CountryCode()
		{
			ZString invalidCountryError = "Please enter a valid country/region code.";

			AssertEquals("MRN does not have a valid country/region code", invalidCountryError, CheckMRNFormatAndGetFirstError("15XX333444455555E0"));
			AssertEquals("MRN does not have a valid country/region code", invalidCountryError, CheckMRNFormatAndGetFirstError("15JI333444455555E0"));
			AssertEquals("MRN has a valid country/region code", ZString.Empty, CheckMRNFormatAndGetFirstError("15DE333444455555E2"));
		}

		public void TestMRNFormat()
		{
			AssertEquals("MRN is valid", ZString.Empty, CheckMRNFormatAndGetFirstError("10AT100200CT8VDY30"));
			AssertEquals("MRN is valid", ZString.Empty, CheckMRNFormatAndGetFirstError("10BE10100016194298"));
			AssertEquals("MRN is valid", ZString.Empty, CheckMRNFormatAndGetFirstError("10DE210119796487M8"));
			AssertEquals("MRN is valid", ZString.Empty, CheckMRNFormatAndGetFirstError("10DE210119797085T1"));
			AssertEquals("MRN is valid", ZString.Empty, CheckMRNFormatAndGetFirstError("10DK0004601366B740"));
		}

		ZString CheckMRNFormatAndGetFirstError(ZString mrnNumber)
		{
			return MRNValidationHelper.CheckMRNFormat(mrnNumber, Factory).FirstOrDefault();
		}

		const string invalidMRNFormatError = @"MRN does not conform to the correct format. Please enter the MRN in the following format with only numbers and upper case letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• thirteen alphanumeric characters for unique identification and
• one number check digit";
	}
}
