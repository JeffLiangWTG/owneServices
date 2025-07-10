using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class SIDDVCodeValidatorTest : TestCaseWithFactory
	{
		public void TestCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidPatternMessage, isOnlyWarning: true);
		}

		public void TestCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, isOnlyWarning: true);
		}

		#region Implementation

		string CountryCode => Core.Constants.CountryCodes.Slovenia;

		string CodeType => OrgCusCode.SloveniaCodeTypes.DDV;

		readonly ZString[] validCodes = new ZString[] { "SI12345678", "12345678" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "123456789", "14785236934", "SIX123456789", "SI1234567" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12345678SI", "1si2345678", "SI-123657" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Slovenia are either 'SI99999999' or '99999999'.";

		#endregion
	}
}
