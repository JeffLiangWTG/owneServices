using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NITValidatorTest : TestCaseWithFactory
	{
		string CodeType => ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CONIT;
		string CountryCode => Core.Constants.CountryCodes.Colombia;

		public void TestNITCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestNITCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		public void TestNITCodeCheckDigitValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidCheckDigit, validCodes, InvalidCheckDigitMessage, OrganisationRegistryCodeType);
		}

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678901234", "1234567890123456", "12345678901234567" };
		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "80..0.197.268-4", "1-405491-6", "16.36.89.28.8", "10.2.6.4492-1", "7.0.01.336.15-1", "1.13.0.632.2209",
			"12--345678---", "ABCDEFGHIJK", "AB-CDEFGHIJ-K", "1-2333-4567", "10-264-492-1" };
		readonly ZString[] codesWithInvalidCheckDigit = new ZString[] { "450.197.268-3", "1856491-8", "163779287", "10.264.601-2", "700134425-3", "1.405.491-5", "10.264.492-4",
		"617.000.352-2", "1.119.886.064-9"  };
		readonly ZString[] validCodes = new ZString[] { "10.264.492-1", "700133615-1", "11306322209", "800.197.268-4", "1405491-6", "163689288", "617.000.352-6", "1.119.886.064-6",
		 "3.558.823-1", "4.238.601-4", "10.215.488-0" };

		string InvalidLengthMessage
		{
			get { return @"The NIT registration code length is invalid.

NIT codes must be 8, 9, 10, 11, 12, 13 or 15 digits long"; }
		}

		string InvalidPatternMessage
		{
			get { return @"The NIT registration code pattern is invalid.

Valid patterns are:
n.nnn.nnn-n OR nn.nnn.nnn-n OR nnn.nnn.nnn-n OR n.nnn.nnn.nnn-n OR
nnnnnnn-n OR nnnnnnnn-n OR nnnnnnnnn-n OR nnnnnnnnnn-n OR
nnnnnnnn OR nnnnnnnnn OR nnnnnnnnnn OR nnnnnnnnnnn

where 'n' is a digit from 0 to 9."; }
		}

		string InvalidCheckDigitMessage => @"The check digit in the NIT registration code is incorrect.";
	}
}
