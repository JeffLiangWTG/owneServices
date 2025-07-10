using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RUTValidatorTest : TestCaseWithFactory
	{
		string CodeType => UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.UYRUT;
		string CountryCode => Core.Constants.CountryCodes.Uruguay;

		public void TestRUTCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestRUTCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		public void TestRUTCodeCheckDigitValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidCheckDigit, validCodesWithCheckDigit, InvalidCheckDigitMessage, OrganisationRegistryCodeType);
		}

		readonly ZString[] codesWithInvalidLength = new ZString[] { "12345678901", "1234567890123", "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "123456789", "123456789" };
		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "21100-420012", "2-1003420-16", "ABCDEFGHIJKT", "AB-DEFGHI-KT", "21-216-200-6" };
		readonly ZString[] codesWithInvalidCheckDigit = new ZString[] { "663689171275","356239837835","092745991373","216348549033","989752441672","987889636435","898345527829","939854678655","237900823228",
																 "237898954663","238954789565","678235239056","789856699047","091212434348","213423778690","116348347755","010788344646","213654120019","217789310010","213300370015" };
		readonly ZString[] validCodes = new ZString[] { "216714840016", "210750890010", "214648450018" };
		readonly ZString[] validCodesWithCheckDigit = new ZString[] { "211428290017","216216220016","212087090011","216547290011","010167760014","180232530011","080017620013","210058770011",
												"214172500012","213401300018","213869970016","040199740010","214994110017","213348730017","216405320018","212407330018","210767220013",
												"140215500011","211597970015","110002560015","214742940015","120003270018","030161320017","214822400017","040003080012","211951780016","100595600014","213654120018","217789310012","213300370010" };

		const string InvalidLengthMessage = @"The RUT registration code length is invalid.

				RUT codes must be 12 digits long.";

		const string InvalidPatternMessage = @"The RUT registration code pattern is invalid.

Valid pattern is:
nnnnnnnnnnnn

where 'n' is a digit from 0 to 9.";

		const string InvalidCheckDigitMessage = "The RUT registration code entered is not valid. The last character (check digit) is incorrect.";
	}
}
