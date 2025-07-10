using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance.DominicanRepublic;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RNCValidatorTest : TestCaseWithFactory
	{
		string CodeType => DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RNC;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.DORNC;
		string CountryCode => Core.Constants.CountryCodes.DominicanRepublic;

		public void TestRNCCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestRNCCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		public void TestRNCCodeCheckDigitValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidCheckDigit, validCodes, InvalidCheckDigitMessage, OrganisationRegistryCodeType);
		}

		public void TestValidModCheckDigit_IntParse()
		{
			CombineAssertions("Validator should not throw exception even input number can't be parsed.", () =>
			{
				AssertNoExceptionThrown(() => DomicanRepublicValidatorHelper.ValidMod10CheckDigit("A3300310541"));
				AssertEquals(false, DomicanRepublicValidatorHelper.ValidMod10CheckDigit("A3300310541"));

				AssertNoExceptionThrown(() => DomicanRepublicValidatorHelper.ValidMod11CheckDigit("A31803282"));
				AssertEquals(false, DomicanRepublicValidatorHelper.ValidMod11CheckDigit("A31803282"));
			});
			CombineAssertions("Validator can still check digit.", () =>
			{
				AssertEquals(false, DomicanRepublicValidatorHelper.ValidMod10CheckDigit("00105853403"));
				AssertEquals(true, DomicanRepublicValidatorHelper.ValidMod10CheckDigit("03300310541"));

				AssertEquals(false, DomicanRepublicValidatorHelper.ValidMod11CheckDigit("531649013"));
				AssertEquals(true, DomicanRepublicValidatorHelper.ValidMod11CheckDigit("531803282"));
			});
		}

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "123", "1234", "12345", "123456", "1234567", "12345678", "12345678901234", "123456789012345", "1234567890123456", "12345678901234567" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "53108559.2", "50.562485.5", "5-33658296", "53-109051-2", "51J0-011-9", "53298380X", "0-0118597756", "03-101453904", "0020-0060648",
																			"071000442-00", "054003-04514", "08R00152131", "0310133035X", "A0102881927" };

		readonly ZString[] codesWithInvalidCheckDigit = new ZString[] { "53096553-6", "50351908-9", "502-87511-5", "531649013", "53063324-8", "53337169-9", "532-18735-3", "4022043925-2", "0340005388-3", "0011707858-1",
																"034-0025419-2", "0500041023-3", "068-0029044-3", "03102261521", "00105853403" };

		readonly ZString[] validCodes = new ZString[] { "112103422", "533-65779-6", "530-29372-9", "501-18422-6", "531-12749-1", "53366098-3", "53318605-1", "50401715-8", "531803282" ,"533657796", "402-2043925-7", "034-0005388-4",
												"001-1707858-4", "0340025419-3", "0500041023-2", "0680029044-4", "03102261520", "00105853402", "03300310541" };

		string InvalidLengthMessage
		{
			get { return @"The RNC registration code length is invalid.

The RNC Registration code needs to be 9, 10, 11, 12 or 13 in length."; }
		}

		string InvalidPatternMessage
		{
			get { return @"The RNC registration code pattern is invalid.

Valid patterns are:

nnn-nnnnn-n OR nnn-nnnnnnn-n
nnnnnnnn-n OR nnnnnnnnnn-n
nnnnnnnnn OR nnnnnnnnnnn

where 'n' is a digit from 0 to 9."; }
		}

		string InvalidCheckDigitMessage => @"The check digit in the RNC registration code is incorrect.";
	}
}
