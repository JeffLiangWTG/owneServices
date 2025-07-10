using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CRUBICodeValidatorTest : TestCaseWithFactory
	{
		string CodeType => CostaRicaOrgCusCodeInfo.OrgCusCodes.UBILocacionCode;
		string CountryCode => Core.Constants.CountryCodes.CostaRica;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CRUBI;

		public void TestCRUBICodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestCRUBICodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		static string InvalidLengthMessage => @"The UBI registration code length is invalid.

The UBI Registration code needs to be 5 in length.";

		static string InvalidPatternMessage => @"The UBI registration code pattern is invalid.

Valid pattern is:

nnnnn

where 'n' is a digit from 0 to 9.";

		readonly ZString[] validCodes = new ZString[] { "12345", "00001", "99999" };
		readonly ZString[] codesWithInvalidLength = new ZString[] { "1234", "123456", "1" };
		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12A45", "123@5", "1 345", "abcde", "0000A" };
	}
}
