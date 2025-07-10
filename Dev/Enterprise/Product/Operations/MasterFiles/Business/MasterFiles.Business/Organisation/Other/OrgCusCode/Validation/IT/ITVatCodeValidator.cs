using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	abstract class ITVatCodeValidator : IITCusCodeValidator
	{
		ITCusCodeValidationResult IITCusCodeValidator.Validate(ZString code)
		{
			if (!IsValidPattern(code))
			{
				return ITCusCodeValidationResult.InvalidPattern;
			}

			if (!IsValidCheckDigit(code))
			{
				return ITCusCodeValidationResult.InvalidCheckDigit;
			}

			return ITCusCodeValidationResult.Valid;
		}

		protected abstract Regex GetPatternForValidation();

		protected abstract ZString GetCleansedCode(ZString code);

		#region Implementation

		bool IsValidCheckDigit(ZString code)
		{
			var codeToVerify = GetCleansedCode(code);

			int a = int.Parse(codeToVerify[0].ToString(), CultureInfo.InvariantCulture)
				+ int.Parse(codeToVerify[2].ToString(), CultureInfo.InvariantCulture)
				+ int.Parse(codeToVerify[4].ToString(), CultureInfo.InvariantCulture)
				+ int.Parse(codeToVerify[6].ToString(), CultureInfo.InvariantCulture)
				+ int.Parse(codeToVerify[8].ToString(), CultureInfo.InvariantCulture);

			int b = DigitSum(int.Parse(codeToVerify[1].ToString(), CultureInfo.InvariantCulture) * 2)
				+ DigitSum(int.Parse(codeToVerify[3].ToString(), CultureInfo.InvariantCulture) * 2)
				+ DigitSum(int.Parse(codeToVerify[5].ToString(), CultureInfo.InvariantCulture) * 2)
				+ DigitSum(int.Parse(codeToVerify[7].ToString(), CultureInfo.InvariantCulture) * 2)
				+ DigitSum(int.Parse(codeToVerify[9].ToString(), CultureInfo.InvariantCulture) * 2);

			int c = a + b;
			int m = c % 10;
			int checkDigit = m == 0 ? 0 : 10 - m;
			return int.Parse(codeToVerify[10].ToString(), CultureInfo.InvariantCulture) == checkDigit;
		}

		bool IsValidPattern(ZString code)
		{
			var validPattern = GetPatternForValidation();
			return validPattern?.IsMatch(code) ?? false;
		}

		int DigitSum(int value)
		{
			var result = 0;
			while (value != 0)
			{
				result += value % 10;
				value /= 10;
			}
			return result;
		}

		#endregion
	}
}
