using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class HUVATCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Hungary;

		string CodeType => OrgCusCode.CodeTypes.VATCode;

		readonly ZString[] validCodes = new ZString[] { "HU12345678", "12345678", "74136985" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "123456789", "1478523693", "HUX12345678", "HU1234567" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12345678HU", "1hu23456789", "HU-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Hungary are either 'HU99999999' or '99999999'.";

		#endregion
	}
}
