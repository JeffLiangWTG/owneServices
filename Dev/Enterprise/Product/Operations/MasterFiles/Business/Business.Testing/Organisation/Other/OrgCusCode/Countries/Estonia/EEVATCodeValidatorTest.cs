using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class EEVATCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Estonia;

		string CodeType => OrgCusCode.CodeTypes.VATCode;

		readonly ZString[] validCodes = new ZString[] { "EE123456789", "123456789", "741369852" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "1478523693", "EEX123456789", "EE12345678" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123456789EE", "1ee23456789", "EE-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Estonia are either 'EE999999999' or '999999999'.";

		#endregion
	}
}
