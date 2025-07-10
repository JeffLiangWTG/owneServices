using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class BGVATCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Bulgaria;

		string CodeType => OrgCusCode.CodeTypes.VATCode;

		readonly ZString[] validCodes = new ZString[] { "BG123456789", "BG1234567890", "123456789", "1234567890" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "14785236934", "BGX123456789", "BG12345678" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123456789BG", "1bg23456789", "BG-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Bulgaria are either 'BG999999999', 'BG9999999999', '999999999' or '9999999999'.";

		#endregion
	}
}
