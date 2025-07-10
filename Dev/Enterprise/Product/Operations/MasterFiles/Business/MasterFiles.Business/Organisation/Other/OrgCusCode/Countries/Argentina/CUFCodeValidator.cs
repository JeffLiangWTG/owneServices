using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	class CUFCodeValidator
	{
		HashSet<string> ValidPatternStrings => new HashSet<string> { @"^5[0-9]{1}-[0-9]{8}-[0-9]{1}$", @"^5[0-9]{10}$" };
		HashSet<int> ValidLengths => new HashSet<int> { 11, 13 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, "", InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, "", InvalidPatternMessage)
				&& OrgCusCodeValidator.Validate(codeInfo, code => IsValidCheckDigit(code), "", InvalidCheckDigitMessage);
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

		string InvalidLengthMessage => Res.GetString("01DA89C9-F645-4833-AC20-0DC44D882EC5", "The CUF registration code needs to be 13 or 11 in length.");
		string InvalidPatternMessage => Res.GetString(@"D8F86298-2C75-40F0-B0FD-83BD2BA3F2E5", @"The CUF registration code pattern is invalid.

Valid patterns are:
	5n-nnnnnnnn-n
	5nnnnnnnnnn

with 'n' a digit from 0 to 9. 
Please verify that you are entering a correct number.");
		string InvalidCheckDigitMessage => Res.GetString("CC458DA3-0CB3-400B-8993-411D728D41CD", "The check digit in the CUF registration code is incorrect.");
	}
}
