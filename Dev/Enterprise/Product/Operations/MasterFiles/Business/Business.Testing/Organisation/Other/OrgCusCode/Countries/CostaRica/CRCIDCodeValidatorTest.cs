using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CRCIDCodeValidatorTest : TestCaseWithFactory
	{
		string CodeType => CostaRicaOrgCusCodeInfo.OrgCusCodes.IndividualIdentificationNumber;
		string CountryCode => Core.Constants.CountryCodes.CostaRica;

		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CRCID;

		public void TestCRCIDCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestCRCIDCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		static string InvalidLengthMessage => @"The CID registration code length is invalid.

The CID Registration code needs to be 9 in length.";

		static string InvalidPatternMessage => @"The CID registration code pattern is invalid.

Valid pattern is:

nnnnnnnnn

where 'n' is a digit from 0 to 9.";

		readonly ZString[] validCodes = new ZString[] { "123456789", "987654321", "111111111" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1234567890", "98765432", "9876543210" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12345A789", "9876@4321", "11111#111", "12-345678", "ABCDEF123" };
	}
}
