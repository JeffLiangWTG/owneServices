using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CIDValidatorTest : TestCaseWithFactory
	{
		string CodeType => UruguayOrgCusCodeInfo.OrgCusCodes.CID;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.UYCID;
		string CountryCode => Core.Constants.CountryCodes.Uruguay;

		public void TestCIDCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestCIDCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithValidLengthPattern, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1234567", "123456789", "1234567890", "1.477.3650", "123456789012" };
		readonly ZString[] codesWithValidLengthPattern = new ZString[] { "123456-8", "1.234567", "1.234.56", "12345678901", "11222.333.4", "1.2222333.4", "1.222.333.4", "11222-333-4", "1-2222333-4", "1-222-333-4", "1.471.36500" };
		readonly ZString[] validCodes = new ZString[] { "62742000", "12345678", "1.771.366-1", "2.881.522-0" };

		const string InvalidLengthMessage = "The CID (Cédula de identidad / Identity Card number) needs to be 8 or 11 characters in length, including the check digit, points and dash.";

		const string InvalidPatternMessage = @"The CID (Cédula de identidad / Identity Card number) pattern is invalid.

Valid patterns are:
	n.nnn.nnn-n
	nnnnnnnn

with 'n' a digit from 0 to 9";
	}
}
