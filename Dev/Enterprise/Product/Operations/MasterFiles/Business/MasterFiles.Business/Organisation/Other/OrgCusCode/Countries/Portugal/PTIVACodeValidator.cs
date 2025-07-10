using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class PTIVACodeValidator
	{
		string CodeType => OrgCusCode.CodeTypes.IVA;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.PTIVA;
		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(?i)PT[0-9]{9}$", @"^[0-9]{9}$" };
		HashSet<int> ValidLengths => new HashSet<int> { 11, 9 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage)
				&& ValidateLastDigit(codeInfo);
		}

		bool ValidateLastDigit(ZPropertyInfo codeInfo)
		{
			var result = CheckLastDigit(codeInfo.Value.ToString());
			if (!result)
			{
				codeInfo.AddErrorIfEnforced(InvalidCheckDigitMessage, OrganisationRegistryCodeType);
			}

			return result;
		}

		bool CheckLastDigit(string value)
		{
			var digits = value.Length == 11 ? value.Substring(2, 8) : value.Substring(0, 8);

			var factor = 9;
			var digitsResult = 0;
			for (int i = 0; i < digits.Length; i++)
			{
				digitsResult += (int)char.GetNumericValue(digits[i]) * factor--;
			}

			var remainder = digitsResult % 11;
			var expectedCheckDigit = remainder <= 1 ? 0 : 11 - remainder;
			var actualCheckDigit = (int)char.GetNumericValue(value[value.Length - 1]);

			return expectedCheckDigit == actualCheckDigit;
		}

		string InvalidLengthMessage => Res.GetString("f7f33193-3624-4a16-8f75-57043ee2b488", @"The {0} registration code length is invalid.

{1}", CodeType, validPatternMessage);

		string InvalidPatternMessage => Res.GetString("0730b4e2-ca22-4b64-a50e-790426e94998", @"The {0} registration code pattern is invalid.

{1}", CodeType, validPatternMessage);

		string validPatternMessage => Res.GetString("ac309780-7c50-49cc-8447-17e09da14421", @"Valid patterns are:
	{0}

where 'X' is a letter from A to Z and indicates the two character country identifier 'PT', and 'n' is a digit from 0 to 9.", validPatterns);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Valid pattern of IVA for Portugal")]
		const string validPatterns = @"XXnnnnnnnnn
	nnnnnnnnn";

		string InvalidCheckDigitMessage => Res.GetString("9962c9e9-fc3c-4363-a536-72b2dc71cd7a", @"The check digit of the IVA registration code (last digit in the code) is incorrect.
Please review and ensure you are entering a valid IVA Number.");
	}
}
