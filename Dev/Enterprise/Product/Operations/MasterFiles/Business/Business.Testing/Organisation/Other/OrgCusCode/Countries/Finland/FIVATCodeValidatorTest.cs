using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class FIVATCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Finland;

		string CodeType => OrgCusCode.CodeTypes.VATCode;

		readonly ZString[] validCodes = new ZString[] { "FI12345678", "12345678", "74136985" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "123456789", "1478523693", "FIX12345678", "FI1234567" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12345678FI", "1fi23456789", "FI-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Finland are either 'FI99999999' or '99999999'.";

		#endregion
	}
}
