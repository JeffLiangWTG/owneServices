using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class OrgCusCodeValidationTest : TestCaseWithFactory
{
	public void TestCheckOK_CustomsRegNo_TIN_ValidCodes()
	{
		ZString[] tinValidCodes =
		{
			"PL1234567890", "ATU12345678", "BE123456789", "BE1234567890", "DK12345678", "FI12345678",
			"FR12345678901", "FRB1234567890", "FR2A123456789", "FRAA123456789", "EL123456789", "ESF12345678",
			"ES12345678Z", "ESA1234578Z", "NL123456789B11", "IE1234567G", "IE1Z12345A", "IE1+12345A",
			"IE1*12345A", "LU12345678", "DE123456789", "PT123456789", "SE123456789012", "GB123456789",
			"GB123456789012", "GBGD123", "GBHA123", "IT12345678901", "CY12345678G", "CZ12345678", "CZ123456789",
			"CZ1234567890", "EE123456789", "LT123456789", "LT123456789012", "LV12345678901", "MT12345678",
			"SK123456789", "SK1234567890", "SI12345678", "HU12345678", "RO12345678",
		};
		AssertCodesAreValid(TINCodeType, tinValidCodes);
	}

	public void TestCheckOK_CustomsRegNo_TIN_PatternValidation()
	{
		const string tinInvalidPatternMessage = "The TIN number must meet the format requirements – G3 rule.";
		ZString[] tinInvalidCodes =
		{
			"PL1234f67890", "PL12345678901", "ATD12345678", "ATU123456781", "BE1F3456789", "BE12345678901",
			"DK12345D78", "DK1234567", "FIF2345678", "FI1234567", "FR123F5678901", "FR123456789011",
			"FRI1234567890", "FRB123456789", "FR4I123456789", "FR2O123456789", "FRIA123456789", "FRAO123456789",
			"EL12345O789", "EL12345678", "ESFF2345678", "ESF123456789", "ES1234Z678Z", "ES123456178Z",
			"ESA134578Z", "ESAA234578Z", "NL123456789A11", "NL123456789B00", "IE123456ZG", "IE123456G",
			"IE1ZD2345A", "IE1Z12345", "IE1+1245A", "IE1+12345", "IE1%12345A", "IE1*2345A", "LU1245678",
			"LU123Z5678", "DEZ23456789", "DE1234567890", "PT3456789", "PT12345678A", "SE12345678901",
			"SE1234567890123", "GB23456789", "GB123AS6789", "GB1234567890123", "GBGF123", "GBHA1234",
			"IT123456789012", "IT1234A678901", "CY1234567G", "CY123456789", "CZ1234567", "CZ12345678A",
			"CZ12345678SD", "EE12345678", "EE123D56789", "LT1234567890", "LT1234567G9012", "LV1234567890",
			"LV123B5678901", "MT1234567", "MT123G5678", "SK12345678", "SK12345S789", "SI123456789",
			"SI1234G678", "HU12345=678", "HU1234678", "RO1234567", "ROE2345678",
		};

		AssertValidationError(TINCodeType, tinInvalidCodes, tinInvalidPatternMessage);
	}

	public void TestCheckOK_CustomsRegNo_EID_LengthValidation()
	{
		const string eidInvalidLengthMessage = "EID number must be 2 chars long. Numbers with other lengths will not be used in customs messages.";
		ZString[] eidInalidLengthCodes = { "", "1", "123", "1234" };
		AssertValidationError(EIDCodeType, eidInalidLengthCodes, eidInvalidLengthMessage);
	}

	public void TestCheckOK_CustomsRegNo_EID_ValidCodes()
	{
		ZString[] eidValidCodes = { "12", "AB", "A1", "1A" };
		AssertCodesAreValid(EIDCodeType, eidValidCodes);
	}

	public void TestCheckOK_CustomsRegNo_NIP_LengthValidation()
	{
		ZString[] nipInalidLengthCodes = { "", "123456789", "12345678901" };
		AssertValidationError(NIPCodeType, nipInalidLengthCodes, NIPInvalidFormatMessage);
	}

	public void TestCheckOK_CustomsRegNo_NIP_DigitValidation()
	{
		ZString[] nipInalidDigitsCodes = { "ABCDEFGHJK", "A123456789", "123456789A", "1234A56789", "+123456789", "123456789+", "1234+56789", "1234-56789", "123456789" };
		AssertValidationError(NIPCodeType, nipInalidDigitsCodes, NIPInvalidFormatMessage);
	}

	public void TestCheckOK_CustomsRegNo_NIP_ValidCodes()
	{
		ZString[] nipValidCodes = { "1234567890", "0987654321", "0000000000", "9999999999", "1122334455" };
		AssertCodesAreValid(NIPCodeType, nipValidCodes);
	}

	public void TestCheckOK_CustomsRegNo_PESEL_LengthValidation()
	{
		const string peselInvalidLengthMessage = "The PESEL Number needs to be 11 in length.";
		ZString[] peselCodesWithInvalidLength = { "6201131954", "7809307703421", "49122113", "0730253921421", "14270826268222", "12" };
		AssertValidationError(PESELCodeType, peselCodesWithInvalidLength, peselInvalidLengthMessage);
	}

	public void TestCheckOK_CustomsRegNo_PESEL_PatternValidation()
	{
		const string peselInvalidPatternMessage = @"The PESEL Number pattern is invalid. Valid pattern is: nnnnnnnnnnn with 'n' a digit from 0 to 9. Please verify that you are entering a correct number.";
		ZString[] peselCodesWithInvalidPatternAndValidLength = { "62011s19544", "7809-077034", "36112 18477", "491sg113623", "$4270826268", "-5120669566", "d2ddd087207" };
		AssertValidationError(PESELCodeType, peselCodesWithInvalidPatternAndValidLength, peselInvalidPatternMessage);
	}

	public void TestCheckOK_CustomsRegNo_PESEL_DigitValidation()
	{
		const string peselInvalidCheckDigitMessage = "The check digit in the PESEL number is incorrect.";
		ZString[] peselCodesWithInvalidCheckDigit = { "62011319540", "78093077032", "36112018473", "49122113620", "14270826261", "05120669563" };
		AssertValidationError(PESELCodeType, peselCodesWithInvalidCheckDigit, peselInvalidCheckDigitMessage);
	}

	public void TestCheckOK_CustomsRegNo_PESEL_ValidCodes()
	{
		ZString[] peselValidCodes = { "62011319544", "78093077034", "36112018477", "49122113623", "14270826268", "05120669566", "05052087207", "07302539214", "03011969995" };
		AssertCodesAreValid(PESELCodeType, peselValidCodes);
	}

	public void TestCheckOK_CustomsRegNo_PTU_PatternValidation()
	{
		const string ptuInvalidPatternMessage = "VAT Business Registration Number structures for Poland are either 'PL9999999999' or '9999999999'.";
		ZString[] ptuCodesWithInvalidPattern = { "12345678", "123456AB", "12345678AB", "PL12345678512346" };
		AssertValidation(PTUCodeType, ptuCodesWithInvalidPattern, ptuInvalidPatternMessage);
	}

	public void TestCheckOK_CustomsRegNo_PTU_ValidCodes()
	{
		ZString[] ptuValidCodes = { "1234567890", "PL1234567890" };
		AssertCodesAreValid(PTUCodeType, ptuValidCodes);
	}

	public void TestCheckOK_CustomsRegNo_REGON_LengthValidation()
	{
		const string regonInvalidLengthMessage = "REGON number must consist of 9 or 14 digits and correct control digit.";
		ZString[] regonCodesWithInvalidLength = { "12345678", "12345678A", "1234567851234A" };
		AssertValidationError(REGONCodeType, regonCodesWithInvalidLength, regonInvalidLengthMessage);
	}

	public void TestCheckOK_CustomsRegNo_REGON_DigitValidation()
	{
		const string regonInvalidCheckDigitMessage = "REGON number is not valid.";
		ZString[] regonCodesWithInvalidCheckDigit = { "123456784", "12345678512346" };
		AssertValidationError(REGONCodeType, regonCodesWithInvalidCheckDigit, regonInvalidCheckDigitMessage);
	}

	public void TestCheckOK_CustomsRegNo_REGON_ValidCodes()
	{
		ZString[] regonValidCodes = { "123456785", "12345678512347" };
		AssertCodesAreValid(REGONCodeType, regonValidCodes);
	}

	OrgCusCode CreateOrgCusCodePL(ZString codeType)
	{
		var orgCusCode = Factory.New<OrgCusCode>();
		orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		orgCusCode.OK_CodeType = codeType;
		return orgCusCode;
	}

	void AssertCodesAreValid(ZString codeType, ZString[] validCodes)
	{
		var orgCusCode = CreateOrgCusCodePL(codeType);
		CombineAssertions(() =>
		{
			foreach (var currentCode in validCodes)
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				AssertNoWarnings("Assertion shouldn't have a warning message!", orgCusCode.OK_CustomsRegNoInfo);
				AssertNoErrors("Assertion shouldn't have an error message!", orgCusCode.OK_CustomsRegNoInfo);
			}
		});
	}

	void AssertValidationError(ZString codeType, ZString[] invalidValue, ZString expectedMessage)
	{
		var orgCusCode = CreateOrgCusCodePL(codeType);
		CombineAssertions(() =>
		{
			foreach (var currentCode in invalidValue)
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				AssertHasError("OrgCusCode Should have an Error Message.", orgCusCode.OK_CustomsRegNoInfo, expectedMessage);
			}
		});
	}

	void AssertValidation(ZString codeType, ZString[] invalidValue, ZString expectedMessage)
	{
		var orgCusCode = CreateOrgCusCodePL(codeType);
		CombineAssertions(() =>
		{
			foreach (var currentCode in invalidValue)
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				AssertHasWarning("OrgCusCode Should have an Warning Message.", orgCusCode.OK_CustomsRegNoInfo, expectedMessage);
			}
		});
	}

	const string TINCodeType = OrgCusCode.PolandCodeTypes.TIN;
	const string EIDCodeType = OrgCusCode.CodeTypes.EDISiteID;
	const string NIPCodeType = OrgCusCode.PolandCodeTypes.NIP;
	const string PESELCodeType = OrgCusCode.PolandCodeTypes.PES;
	const string PTUCodeType = OrgCusCode.PolandCodeTypes.PTU;
	const string REGONCodeType = OrgCusCode.CodeTypes.GovBusinessCode;

	const string NIPInvalidFormatMessage = "The NIP number entered is invalid (must be 10 digits long).";
}
