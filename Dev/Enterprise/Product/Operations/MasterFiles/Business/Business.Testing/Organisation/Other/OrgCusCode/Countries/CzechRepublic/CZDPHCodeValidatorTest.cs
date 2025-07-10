using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class CZDPHCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.CzechRepublic;

		string CodeType => CzechRepublicOrgCusCodeInfo.OrgCusCodes.DPH;

		readonly ZString[] validCodes = new ZString[] { "CZ12345678", "CZ123456789", "CZ1234567890", "12345678", "123456789", "1234567890" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "CZX123456789", "CZ1234567" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123456789CZ", "1cz23456789", "CZ-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Czech Republic are either 'CZ99999999', 'CZ999999999', 'CZ9999999999', '99999999', '999999999' or '9999999999'.";

		#endregion
	}
}
