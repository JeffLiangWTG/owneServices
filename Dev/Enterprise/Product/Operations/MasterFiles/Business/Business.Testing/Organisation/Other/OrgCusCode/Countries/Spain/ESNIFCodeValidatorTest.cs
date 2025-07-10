using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ESNIFCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Spain;

		string CodeType => OrgCusCode.SpainCodeTypes.NIF;

		readonly ZString[] validCodes = new ZString[] { "ES12345678X", "ESX12345678", "ESUB01EX245", "ESA1234567B", "12345678X", "X12345678", "UB01EX245", "741A6985B", "ub01EX245", "a1234567B" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "1478523693", "ESX123456789", "ES12345678" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "ES123456789", "123456789ES", "1es23456789", "ES-1236578" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Spain are either 'ESX9999999X' or 'X9999999X'.";

		#endregion
	}
}
