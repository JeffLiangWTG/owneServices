using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AEOValidatorTest : TestCaseWithFactory
	{
		string CodeType => UruguayOrgCusCodeInfo.OrgCusCodes.AEO;
		string CountryCode => Core.Constants.CountryCodes.Uruguay;

		public void TestRUTCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, isOnlyWarning: true);
		}

		public void TestAEOCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, isOnlyWarning: true);
		}

		readonly ZString[] codesWithInvalidLength = new ZString[] { "12345678901", "1234567890123", "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "123456789", "123456789" };
		const string InvalidLengthMessage = @"The AEO registration code length is invalid.
AEO codes must be 12 digits long.";

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "2167148400A6", "21075089 010", "A21464845001" };
		const string InvalidPatternMessage = @"The AEO registration code pattern is invalid.
AEO should be 12 numbers, e.g. 123456789012.";

		readonly ZString[] validCodes = new ZString[] { "216714840016", "210750890010", "214648450018" };
	}
}
