using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CREACCodeValidatorTest : TestCaseWithFactory
	{
		string CodeType => CostaRicaOrgCusCodeInfo.OrgCusCodes.EACEconomicActivityCode;
		string CountryCode => Core.Constants.CountryCodes.CostaRica;

		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CREAC;

		public void TestCREACCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestCREACCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		static string InvalidLengthMessage => @"The EAC registration code length is invalid.

The EAC Registration code needs to be 6 in length.";

		static string InvalidPatternMessage => @"The EAC registration code pattern is invalid.

Valid pattern is:

nnnnnn

where 'n' is a digit from 0 to 9.";

		readonly ZString[] validCodes = new ZString[] { "123456", "987654", "111111" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1234567", "98765", "9876543210" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12345A", "9876@4", "#11111", "12-345", "ABCDE1" };
	}
}
