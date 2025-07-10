using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class CLRUTAndSOLValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CLRUTSOL;
		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(\d{1,2}(\.+\d{3}){2}\-([\dK]))$", (NoResString)@"^(\d{1,2}(\d{3}){2})\-?([\dK])$" };
		HashSet<int> ValidLengths => new HashSet<int> { 8, 9, 10, 11, 12 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage)
				&& OrgCusCodeValidator.Validate(codeInfo, code => IsValidCheckDigit(code), OrganisationRegistryCodeType, InvalidCheckDigitMessage);
		}

		internal bool IsValidCheckDigit(ZString code)
		{
			ZString codeToWorkWith = code.Replace(DOT.ToString(), string.Empty).Replace(DASH.ToString(), string.Empty);

			int accumulated = 0;
			int multiplier = 2;

			for (var i = codeToWorkWith.Length - 2; i >= 0; i--)
			{
				accumulated += int.Parse(codeToWorkWith[i].ToString()) * multiplier;
				multiplier = (multiplier == 7) ? 2 : (multiplier + 1);
			}

			int total = 11 - (accumulated - (11 * (accumulated / 11)));

			string checkDigit;

			if (total == 10)
			{
				checkDigit = K.ToString();
			}
			else if (total == 11)
			{
				checkDigit = "0";
			}
			else
			{
				checkDigit = total.ToString();
			}

			return checkDigit == codeToWorkWith[codeToWorkWith.Length - 1].ToString();
		}

		string InvalidLengthMessage => Res.GetString("04178E6F-FB2B-4822-AC30-A0124E8855E8", "The RUT / SOL registration code needs to be 8, 9, 10, 11 or 12 in length, including the check digit.");
		string InvalidPatternMessage => Res.GetString("6D680659-614F-40FE-A1DC-B2880ADA40B9", @"The RUT / SOL registration code pattern is invalid.

Valid patterns are:
	{0}

with 'n' a digit from 0 to 9 and X (check digit) either a digit from 0 to 9 or the letter 'K'.", validPatterns);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Specific Technical Formatting")]
		const string validPatterns = @"nn.nnn.nnn-x OR n.nnn.nnn-x
	nnnnnnnn-x OR nnnnnnn-x
	nnnnnnnnx OR nnnnnnnx";

		string InvalidCheckDigitMessage => Res.GetString("4DD86B5C-D565-460C-B516-1D9A01049706", "The check digit in the RUT / SOL registration code is incorrect.");

		const char DOT = '.';
		const char DASH = '-';
		const char K = 'K';
	}
}
