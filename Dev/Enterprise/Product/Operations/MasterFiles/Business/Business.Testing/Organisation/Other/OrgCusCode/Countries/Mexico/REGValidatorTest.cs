using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class REGValidatorTest : TestCaseWithFactory
	{
		string CodeType => MexicoOrgCusCodeInfo.OrgCusCodes.REG;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.MXREG;
		string CountryCode => Core.Constants.CountryCodes.Mexico;

		public void TestREGCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPattern, validCodesString, InvalidPatternMessage, OrganisationRegistryCodeType, isOnlyWarning: false);
		}

		readonly ZString[] codesWithInvalidPattern = new ZString[] { "602", "6010", "0601", "06010", "0000", "!999", "555", "444", "888", "I22", "3W4", "P&3", "10X", "YY3", "M3M", "SS+22", ".I07-1" };
		readonly ZString[] validCodesString = new ZString[] { "601", "603", "605", "606", "607", "608", "610", "611", "612", "614", "615", "616", "620", "621", "622", "623", "624", "625", "626" };
		string InvalidPatternMessage
		{
			get
			{
				return @"The 'MX REG' registration code is invalid.

Valid codes are:
601, 603, 605, 606, 607, 608, 610, 611, 612, 614, 615, 616, 620, 621, 622, 623, 624, 625 or 626

Please verify that you are entering a valid code.";
			}
		}
	}
}
