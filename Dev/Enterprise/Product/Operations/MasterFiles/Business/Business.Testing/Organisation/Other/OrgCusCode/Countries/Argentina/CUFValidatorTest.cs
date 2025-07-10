using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CUFValidatorTest : TestCaseWithFactory
	{
		string CodeType => ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF;
		string CountryCode => Core.Constants.CountryCodes.Argentina;

		public void TestCUFCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, isOnlyWarning: true);
		}

		public void TestCUFCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, isOnlyWarning: true);
		}

		public void TestCUFCodeCheckDigitValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidCheckDigit, validCodes, InvalidCheckDigitMessage, isOnlyWarning: true);
		}

		static string InvalidLengthMessage => "The CUF registration code needs to be 13 or 11 in length.";
		static string InvalidPatternMessage => @"The CUF registration code pattern is invalid.

Valid patterns are:
	5n-nnnnnnnn-n
	5nnnnnnnnnn

with 'n' a digit from 0 to 9. 
Please verify that you are entering a correct number.";
		static string InvalidCheckDigitMessage => "The check digit in the CUF registration code is incorrect.";

		readonly ZString[] validCodes = new ZString[] { "55-00000001-8", "55000000018", "51-60000003-2", "51600000032", "50-00000127-6", "50000001276" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "5123456789", "512345678912", "51234567891234", "51-4567891", "51-4567891-2", "512-45678912-3" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "51-234567-8", "512345678-9", "51-23456789", "5123456789121", "51-2345678912", "51234567891-2", "ABCDEFGHIJK", "AB-CDEFGHIJ-K" };

		readonly ZString[] codesWithInvalidCheckDigit = new ZString[] { "55-00000001-7", "55000000017", "54000000018", "51-60000003-1", "51600000031", "52600000031" };
	}
}
