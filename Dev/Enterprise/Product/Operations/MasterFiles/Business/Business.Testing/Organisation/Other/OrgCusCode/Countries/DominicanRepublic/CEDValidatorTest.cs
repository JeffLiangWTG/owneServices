using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CEDValidatorTest : TestCaseWithFactory
	{
		string CodeType => DominicanRepublicOrgCusCodeInfo.OrgCusCodes.CED;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.DOCED;
		string CountryCode => Core.Constants.CountryCodes.DominicanRepublic;

		public void TestCEDCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestCEDCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		public void TestCEDCodeCheckDigitValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidCheckDigit, validCodes, InvalidCheckDigitMessage, OrganisationRegistryCodeType);
		}

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "123456789", "1234567890", "12345678901234", "123456789012345", "1234567890123456", "12345678901234567" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "0-3300223538", "00-201440377", "049-00567423", "0010-9430496", "044000459-79", "02301558-488", "DD115969479", "0010072376X", "0310240391N", "NNN-15191405", "0010Y836306" };

		readonly ZString[] codesWithInvalidCheckDigit = new ZString[] { "001-1067231-0", "00100845914", "0310118299-2", "04600039557", "03700195342", "00201627792", "40235533577", "003-0054231-1", "00118347085" };

		readonly ZString[] validCodes = new ZString[] { "012-0002973-2", "223-0049556-5", "0600017416-6", "0650000037-4", "0310372088-8", "40223366689", "00116245606", "07100451173", "001-1769470-3", "055-0037360-9",
												"0310274265-1", "4022143823-3", "00106110968", "04800131775", "224-0058507-5", "2250024708-9" };

		string InvalidLengthMessage
		{
			get { return @"The CED registration code length is invalid.

The CED Registration code needs to be 11, 12 or 13 in length."; }
		}

		string InvalidPatternMessage => @"The CED registration code pattern is invalid.

Valid patterns are:

nnn-nnnnnnn-n OR
nnnnnnnnnn-n  OR 
nnnnnnnnnnn   
 
where 'n' is a digit from 0 to 9.";

		string InvalidCheckDigitMessage => "The check digit in the CED registration code is incorrect.";
	}
}
