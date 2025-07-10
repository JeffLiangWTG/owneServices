using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class EmployerIdentificationNumberValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidEIN(number) ? "" : EINNumberRightFormat;
		}

		public const string EINNumberRightFormat = "Employer Identification Number should be in the format,\nNN-NNNNNNNXX or NN-NNNNNNN\nwhere N is a number and X is alphanumeric.\n";

		public static string Validate(ZString number, bool allowFullFormat)
		{
			return IsValidEIN(number, allowFullFormat) ? "" : EINNumberFullFormatOnly;
		}
		public const string EINNumberFullFormatOnly = "Employer Identification Number should be in the format,\nNN-NNNNNNNXX\nwhere N is a number and X is alphanumeric.\n";

		public static bool IsValidEIN(ZString eINNumber, bool allowFullFormat)
		{
			return allowFullFormat ? Regex.IsMatch(eINNumber, @"^[0-9]{2}-[0-9]{7}([A-Z0-9]{2})$", RegexOptions.IgnoreCase) : IsValidEIN(eINNumber);
		}

		public static bool IsValidEIN(ZString eINNumber)
		{
			return Regex.IsMatch(eINNumber, @"^[0-9]{2}-[0-9]{7}([A-Z0-9]{2})?$", RegexOptions.IgnoreCase);
		}

		public static bool IsValidEINForStandAloneInBondMessage(ZString eINNumber)
		{
			return Regex.IsMatch(eINNumber, @"^[0-9]{2}-[0-9]{7}[A-Z0-9* ]{2}$", RegexOptions.IgnoreCase) ||
					Regex.IsMatch(eINNumber, @"^[0-9]{2}-[0-9]{7}$", RegexOptions.IgnoreCase);
		}

		public static string GetValidEINForInBondMessage(ZString eINNumber)
		{
			string result = eINNumber.Trim();

			if (EmployerIdentificationNumberValidator.IsValidEINForStandAloneInBondMessage(result))
			{
				if (result.Length == 10)
				{
					result += "00";
				}
			}

			return result;
		}
	}
}
