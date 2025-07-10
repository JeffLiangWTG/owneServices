using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ATUIDCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Austria;

		string CodeType => OrgCusCode.AustriaCodeTypes.UID;

		readonly ZString[] validCodes = new ZString[] { "AT123456789", "ATUB01EX245", "123456789", "UB01EX245", "741369852", "ub01EX245" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "1478523693", "ATX123456789", "AT12345678" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123456789AT", "1at23456789", "PT-12365789" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Austria are either 'ATU99999999' or 'U99999999'.";

		#endregion
	}
}
