using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class LTPVMCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Lithuania;

		string CodeType => OrgCusCode.LithuaniaCodeTypes.PVM;

		readonly ZString[] validCodes = new ZString[] { "LT123456789012", "123456789012", "741369851234", "LT123456789", "123456789" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "1234567890", "12345678901", "1478523693", "LT12345678", "LT1234567" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123456789012LT", "1lt23456789012", "LT-1236578901" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Lithuania are either 'LT999999999', 'LT999999999999', '999999999' or '999999999999'.";

		#endregion
	}
}
