using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GRAFMCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Greece;

		string CodeType => OrgCusCode.GreeceCodeTypes.AFM;

		readonly ZString[] validCodes = new ZString[] { "EL123456789", "123456789", "741369852" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "1478523693", "ELX123456789", "EL12345678" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123456789EL", "1el23456789", "EL-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Greece are either 'EL999999999' or '999999999'.";

		#endregion
	}
}
