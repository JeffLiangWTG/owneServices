using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class PTIVACodeValidatorTest : TestCaseWithFactory
	{
		string CodeType => OrgCusCode.CodeTypes.IVA;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.PTIVA;
		string CountryCode => Core.Constants.CountryCodes.Portugal;

		public void TestPTIVACodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestPTIVACodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		public void TestPTIVACheckDigitValidation()
		{
			var expectedMessage = @"The check digit of the IVA registration code (last digit in the code) is incorrect.
Please review and ensure you are entering a valid IVA Number.";

			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidCheckDigit, validCodes, expectedMessage, OrganisationRegistryCodeType);
		}

		string InvalidLengthMessage => @"The IVA registration code length is invalid.

Valid patterns are:
	XXnnnnnnnnn
	nnnnnnnnn

where 'X' is a letter from A to Z and indicates the two character country identifier 'PT', and 'n' is a digit from 0 to 9.";

		string InvalidPatternMessage => @"The IVA registration code pattern is invalid.

Valid patterns are:
	XXnnnnnnnnn
	nnnnnnnnn

where 'X' is a letter from A to Z and indicates the two character country identifier 'PT', and 'n' is a digit from 0 to 9.";

		readonly ZString[] validCodes = new ZString[] { "PT123456789", "Pt012345679", "pT789456125", "pt852369743", "741369850", "111110220" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "1478523693", "PTI123456789", "PT12345678" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123456789PT", "1PT23654789", "PT-12365789", "123654p89", "FR123654789", "PR147852369", "AU123456789" };

		readonly ZString[] codesWithInvalidCheckDigit = new ZString[] { "PT123456780", "Pt012345672", "pT789456121", "pt852369740", "741369852", "111110221" };
	}
}
