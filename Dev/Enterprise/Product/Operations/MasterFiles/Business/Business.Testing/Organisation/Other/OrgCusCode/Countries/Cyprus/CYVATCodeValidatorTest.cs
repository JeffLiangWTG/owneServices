using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class CYVATCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Cyprus;

		string CodeType => OrgCusCode.CodeTypes.VATCode;

		readonly ZString[] validCodes = new ZString[] { "CY123456789", "CYUB01EX245", "123456789", "UB01EX245", "741369852", "ub01EX245" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "1478523693", "CYX123456789", "CY12345678" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123456789CY", "1CY23456789", "CY-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Cyprus are either 'CY99999999L' or '99999999L'.";

		#endregion
	}
}
