using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CRDIMCodeValidatorTest : TestCaseWithFactory
	{
		string CodeType => CostaRicaOrgCusCodeInfo.OrgCusCodes.DIMEXDocumentIdentificationNumber;
		string CountryCode => Core.Constants.CountryCodes.CostaRica;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CRDIM;

		public void TestCRDIMCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestCRDIMCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		static string InvalidLengthMessage => @"The DIM registration code length is invalid.

The DIM Registration code needs to be 11 or 12 in length.";

		static string InvalidPatternMessage => @"The DIM registration code pattern is invalid.

Valid pattern is:

nnnnnnnnnnn or nnnnnnnnnnnn

where 'n' is a digit from 0 to 9.";

		readonly ZString[] validCodes = new ZString[] { "12345678901", "98765432101", "111111111111" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "123456789", "1234567890123", "98765432", "9876543210124" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12345A78901", "9876@432101", "11111#111111", "12-345678901", "ABCDEF12345" };
	}
}
