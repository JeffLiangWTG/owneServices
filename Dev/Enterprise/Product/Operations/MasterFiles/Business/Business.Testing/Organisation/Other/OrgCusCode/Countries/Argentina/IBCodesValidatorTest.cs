using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IBCodesValidatorTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.Argentina;

		public void TestIBCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ArgentinaOrgCusCodeInfo.OrgCusCodes.IBL, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistry.RegistrationNumberFormatFields.ARIBL);
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ArgentinaOrgCusCodeInfo.OrgCusCodes.IBM, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistry.RegistrationNumberFormatFields.ARIBM);
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ArgentinaOrgCusCodeInfo.OrgCusCodes.IBS, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistry.RegistrationNumberFormatFields.ARIBS);
		}

		public void TestIBCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ArgentinaOrgCusCodeInfo.OrgCusCodes.IBL, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistry.RegistrationNumberFormatFields.ARIBL);
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ArgentinaOrgCusCodeInfo.OrgCusCodes.IBM, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistry.RegistrationNumberFormatFields.ARIBM);
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ArgentinaOrgCusCodeInfo.OrgCusCodes.IBS, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistry.RegistrationNumberFormatFields.ARIBS);
		}

		readonly ZString[] validCodes = new ZString[] { "20000000028", "30-58011131-5", "33-71074322-9", "921-740368-7", "30584715932", "902-145103-3", "901-915040-5", "901-177567-7" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "20000000028287", "200000000282878", "200028287", "300-58011310-5", "13-71074322-99", "921-7403689-70", "902-145103-300", "7901-915040-50", "9-17567-7" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "30-5811131-5", "33071074322-9", "921-740360", "921-74036-8", "902-14510313", "90-1915040-5", "9010177567-7" };

		string InvalidLengthMessage => @"The IB registration code length is invalid.
IB codes must be 10 to 13 digits long.";
		string InvalidPatternMessage => @"The IB registration code pattern is invalid.

Valid patterns are:
	nnnnnnnnnn
	nnnnnnnnnnn
	nnnnnnnn-nn
	nnn-nnnnnn-n
	nn-nnnnnnnn-n

with 'n' a digit from 0 to 9.
Please verify that you are entering a correct number.";
	}
}
