using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class SKDPHCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Slovakia;

		string CodeType => OrgCusCode.SlovakiaCodeTypes.DPH;

		readonly ZString[] validCodes = new ZString[] { "SK0123456789", "SK7413698523", "0123456789", "7413698523" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "147852369", "SKX12345678", "SK12345678" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "1234567890SK", "SK-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Slovakia are either 'SK9999999999' or '9999999999'.";

		#endregion
	}
}
