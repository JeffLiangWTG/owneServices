using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class UYRUTCodeValidator
	{
		string CodeType => UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.UYRUT;
		HashSet<string> ValidPatternStrings => new HashSet<string> { @"[0-9]{12}" }; // Valid pattern of RUT for Uruguay
		HashSet<int> ValidLengths => new HashSet<int> { 12 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage)
				&& OrgCusCodeValidator.Validate(codeInfo, code => IsValidCheckDigit(code), OrganisationRegistryCodeType, InvalidCheckDigitMessage);
		}

		public static bool IsValidCheckDigit(ZString code)
		{
			var checkDigitOriginal = int.Parse(code[11].ToString(), CultureInfo.InvariantCulture);
			var total = 0;
			var factor = 2;
			for (int i = 10; i >= 0; i--)
			{
				total += (factor * int.Parse(code[i].ToString(), CultureInfo.InvariantCulture));
				factor = (factor == 9) ? 2 : (factor + 1);
			}
			var calculatedCheckDigit = 11 - (total % 11);
			if (calculatedCheckDigit == 11)
			{
				calculatedCheckDigit = 0;
			}
			else if (calculatedCheckDigit == 10)
			{
				calculatedCheckDigit = 1;
			}
			return (calculatedCheckDigit == checkDigitOriginal);
		}

		string InvalidLengthMessage => Res.GetString("C954BB52-C36B-4084-97F8-43425CE83FA6", @"The {0} registration code length is invalid.

				RUT codes must be 12 digits long.", CodeType);

		string InvalidPatternMessage => Res.GetString("6ECF3A2A-9D8C-4D9D-917E-9DD4459810F1", @"The {0} registration code pattern is invalid.

Valid pattern is:
nnnnnnnnnnnn

where 'n' is a digit from 0 to 9.", CodeType);

		string InvalidCheckDigitMessage => Res.GetString("A71C8C66-8C9C-4633-93A3-DC2E8F7C8583", @"The {0} registration code entered is not valid. The last character (check digit) is incorrect.", CodeType);
	}
}
