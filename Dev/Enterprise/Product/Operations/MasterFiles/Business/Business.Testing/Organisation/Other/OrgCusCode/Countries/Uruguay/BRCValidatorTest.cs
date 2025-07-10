using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing.Countries.Uruguay
{
	sealed class BRCValidatorTest : TestCaseWithFactory
	{
		string CodeType => UruguayOrgCusCodeInfo.OrgCusCodes.BRC;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.UYBRC;
		string CountryCode => Core.Constants.CountryCodes.Uruguay;

		public void TestBRCCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestBRCCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithValidLengthPattern, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		readonly ZString[] codesWithInvalidLength = new ZString[] { "12345", "123456", "1234567", "12345678", "123456789", "1234567890" };
		readonly ZString[] codesWithValidLengthPattern = new ZString[] { "A", "1A", "22B", "333C", "D44D" };
		readonly ZString[] validCodes = new ZString[] { "1", "10", "111", "2222" };

		const string InvalidLengthMessage = @"The BRC registration code length is invalid.

BRC codes must be from 1 to 4 digits long.";

		const string InvalidPatternMessage = @"The BRC registration code pattern is invalid.

Valid patterns are:
	n
	nn
	nnn
	nnnn
Where 'n' is a digit from 0 to 9.
Please verify that you are entering a correct number.";
	}
}
