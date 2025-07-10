using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	class CUITCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.ARCUIT;
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

		string InvalidLengthMessage => Res.GetString("E7857DBF-03F1-42F0-852F-EFEC6DE5199C", "The CUI registration code needs to be 13 or 11 in length.");
		string InvalidPatternMessage => Res.GetString(@"DC961957-807D-4DCE-87BF-3B9F04C4411E", @"The CUI registration code pattern is invalid.

Valid patterns are:
	nn-nnnnnnnn-n
	nnnnnnnnnnn

with 'n' a digit from 0 to 9. 
Please verify that you are entering a correct number.");
		string InvalidCheckDigitMessage => Res.GetString("AB1ACAAC-BC20-4EBD-A178-F844167AE9A9", "The check digit in the CUI registration code is incorrect.");
	}
}
