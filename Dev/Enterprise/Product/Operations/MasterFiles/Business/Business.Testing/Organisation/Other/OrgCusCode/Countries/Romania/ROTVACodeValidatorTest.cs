using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ROTVACodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Romania;

		string CodeType => OrgCusCode.RomaniaCodeTypes.TVA;

		readonly ZString[] validCodes = new ZString[] { "RO12", "RO123", "RO1234", "RO12345", "RO123456", "RO1234567", "RO12345678", "RO123456789", "RO1234567890",
			"12", "123", "1234", "12345", "123456", "1234567", "12345678", "123456789", "1234567890", "7136985124" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "RO1", "RO12345678901", "12345678901" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12345678RO", "1ro23456789", "RO-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Romania are either 'RO9999999999' or '9999999999'.";

		#endregion
	}
}
