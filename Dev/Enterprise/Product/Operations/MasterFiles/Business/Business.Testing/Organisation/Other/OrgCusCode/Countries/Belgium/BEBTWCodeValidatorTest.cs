using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class BEBTWCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Belgium;

		string CodeType => OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;

		readonly ZString[] validCodes = new ZString[] { "BE0123456789", "BE7413698523", "0123456789", "7413698523" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "147852369", "BEX12345678", "BE12345678" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "1234567890BE", "BE-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Belgium are either 'BE0999999999', 'BE1999999999', '0999999999' or '1999999999'.";

		#endregion
	}
}
