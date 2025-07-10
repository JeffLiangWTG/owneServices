using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	class CUILCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.ARCUIL;
		HashSet<string> ValidPatternStrings => new HashSet<string> { @"^[0-9]{2}-[0-9]{8}-[0-9]{1}$", @"^[0-9]{11}$" };
		HashSet<int> ValidLengths => new HashSet<int> { 11, 13 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage)
				&& OrgCusCodeValidator.Validate(codeInfo, code => IsValidCheckDigit(code), OrganisationRegistryCodeType, InvalidCheckDigitMessage);
		}

		internal bool IsValidCheckDigit(ZString code)
		{
			ZString codeToWorkWith = code.Replace("-", string.Empty);

			var factors = new int[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
			var accumulated = 0;

			for (int i = 0; i < factors.Length; i++)
			{
				accumulated += int.Parse(codeToWorkWith[i].ToString(), CultureInfo.InvariantCulture) * factors[i];
			}
			accumulated = 11 - (accumulated % 11);
			if (accumulated == 11)
			{
				accumulated = 0;
			}
			if (int.Parse(codeToWorkWith[10].ToString(), CultureInfo.InvariantCulture) != accumulated)
			{
				return false;
			}
			return true;
		}

		string InvalidLengthMessage => Res.GetString("8704B542-318A-4EAF-95DF-247AF423943E", @"The CUIL registration code length is invalid. 
CUIL codes must be 11 or 13 digits long.");
		string InvalidPatternMessage => Res.GetString(@"1CBEB981-4456-4536-BC8C-93419061D988", @"The CUIL registration code pattern is invalid.

Valid patterns are:
	nnnnnnnnnnn	
	nn-nnnnnnnn-n

with 'n' is a digit from 0 to 9. 
Please verify that you are entering a correct number.");
		string InvalidCheckDigitMessage => Res.GetString("128BFB09-044B-4BCB-A393-6823DD861B97", "The check digit in the CUIL registration code is incorrect.");
	}
}
