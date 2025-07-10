using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CRNITCodeValidatorTest : TestCaseWithFactory
	{
		string CodeType => CostaRicaOrgCusCodeInfo.OrgCusCodes.NITIdentificationNumber;
		string CountryCode => Core.Constants.CountryCodes.CostaRica;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CRNIT;

		public void TestCRNITCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestCRNITCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		static string InvalidLengthMessage => @"The NIT registration code length is invalid.

The NIT Registration code needs to be 10 in length.";

		static string InvalidPatternMessage => @"The NIT registration code pattern is invalid.

Valid pattern is:

nnnnnnnnnn

where 'n' is a digit from 0 to 9.";

		readonly ZString[] validCodes = new ZString[] { "1234567890", "9876543210", "1111111111" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "123456789", "12345678901", "98765432", "98765432101" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12345A7890", "9876@43210", "11111#1111", "12-4567890", "ABCDEF1234" };
	}
}
