using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class LVPVNCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Latvia;

		string CodeType => OrgCusCode.LatviaCodeTypes.PVN;

		readonly ZString[] validCodes = new ZString[] { "LV12345678901", "12345678901", "74136985124" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "123456789", "1478523693", "LVX12345678", "LV1234567" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "12345678LV", "1lv23456789", "LV-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Latvia are either 'LV99999999999' or '99999999999'.";

		#endregion
	}
}
