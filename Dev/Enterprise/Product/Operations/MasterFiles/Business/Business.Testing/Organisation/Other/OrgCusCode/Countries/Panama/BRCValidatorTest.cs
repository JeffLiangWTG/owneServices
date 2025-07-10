using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class BRCValidatorTest : TestCaseWithFactory
	{
		string CodeType => PanamaOrgCusCodeInfo.OrgCusCodes.BRC;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.PABRC;
		string CountryCode => Core.Constants.CountryCodes.Panama;

		public void TestBRCCodeValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, invalidValue: codesWithInvalidPattern.Concat(codesWithInvalidLengh).ToArray(), validValue: codesWithValidPatternAndLengh, InvalidMessage, OrganisationRegistryCodeType, isOnlyWarning: false);
		}

		readonly ZString[] codesWithInvalidPattern = new ZString[] { "$123", "WW%1", "W4-1", "6/88", "22.1", "R$45", "!99R" };

		readonly ZString[] codesWithInvalidLengh = new ZString[] { "12345", "123456", "1234567", "12345678" };

		readonly ZString[] codesWithValidPatternAndLengh = new ZString[] { "1", "22", "333", "4444", "A", "BB", "CCC", "DDDD", "WW33", "2S2A", "a", "ab", "abc", "acbd" };

		string InvalidMessage
		{
			get
			{
				return @"Branch Registration Code must be an alphanumeric code from 1 to 4 digits long.";
			}
		}
	}
}
