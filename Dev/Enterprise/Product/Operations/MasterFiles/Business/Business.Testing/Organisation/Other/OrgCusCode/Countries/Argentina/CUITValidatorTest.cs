using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CUITValidatorTest : TestCaseWithFactory
	{
		string CodeType => ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.ARCUIT;
		string CountryCode => Core.Constants.CountryCodes.Argentina;

		public void TestCUITCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestCUITCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		public void TestCUITCodeCheckDigitValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidCheckDigit, validCodes, InvalidCheckDigitMessage, OrganisationRegistryCodeType);
		}

		string InvalidLengthMessage => @"The CUI registration code needs to be 13 or 11 in length.";
		string InvalidPatternMessage => @"The CUI registration code pattern is invalid.

Valid patterns are:
	nn-nnnnnnnn-n
	nnnnnnnnnnn

with 'n' a digit from 0 to 9. 
Please verify that you are entering a correct number.";
		string InvalidCheckDigitMessage => @"The check digit in the CUI registration code is incorrect.";

		readonly ZString[] validCodes = new ZString[] {  "27-34813676-9","20242935552","27-22008323-9","27-22338912-6","20-10975339-5","27-13152668-2","30-71488885-0","27-24342357-6","27-35140285-2","20-22990775-2",
											"27040092337","30-71495144-7","30714989835","20-33257716-7","30711056617","27220513926","33-71480682-9","20141225287","23933165779","20-93482722-9",
											"27-60080711-6","27959241878","27-95362535-6","20-23454229-0","20-62151943-4","20-95230897-2","27-51065799-9","30695495796","30-71640438-9","20-31128959-5" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "123456789012", "123456789013", "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "123456789", "123456789" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123-456789-01", "1-2345678-901", "1-23456789013", "12--345678---", "ABCDEFGHIJK", "AB-CDEFGHIJ-K", "1-2333-4567" };

		readonly ZString[] codesWithInvalidCheckDigit = new ZString[] { "11-34863676-9","25242935852","22210783239","21223399127","12-12956339-3","10-13156768-5","31-72428881-0","22-24347757-2","21-34477285-7","25119907331",
																 "24-14009233-7","31-71495144-8","34-71698683-2","23-35525776-7","30-71167661-7","27-12051312-6","33724816821","25-24322528-7","23-91316577-9","21-83482722-9",
																 "27-60081111-6","25919241878","27953625316","21-22454219-0","18-62151941-4","21912318972","26510617999","30685498796","31-76640418-9","21-31128155-1" };
	}
}
