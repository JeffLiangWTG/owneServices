using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IrelandCodeValidatorTest : TestCaseWithFactory
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

		string CountryCode => Core.Constants.CountryCodes.Ireland;

		string CodeType => OrgCusCode.CodeTypes.VATCode;

		readonly ZString[] validCodes = new ZString[] { "IE12345678", "IE123456789", "IE12A34B5C", "IE12A34B5CD", "12345678", "123456789", "12A34B5C", "12A34B5CD", "IE1+2345AB", "IE1*2345AB", "1+2345AB", "1*2345AB" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "1478523693", "IEX123456789" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123456789IE", "1ie23456789", "IE-1236578", "1+2345678", "1*2345678" };

		string InvalidPatternMessage => "VAT Business Registration Number structures for Ireland are either 'IE9X99999L', 'IE9999999WI', '9X99999L' or '9999999WI'.";

		#endregion
	}
}
