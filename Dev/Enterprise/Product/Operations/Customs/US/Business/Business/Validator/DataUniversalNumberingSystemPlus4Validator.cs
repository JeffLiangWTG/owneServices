using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class DataUniversalNumberingSystemPlus4Validator
	{
		public static string Validate(ZString number)
		{
			return IsValidDUNSPlus4(number) ? "" : DUNSPlust4RightFormat;
		}

		public const string DUNSPlust4RightFormat = "DUNS+4 Number should be in the format, NNNNNNNNNNNNN where N is a number.";

		public static bool IsValidDUNSPlus4(ZString number)
		{
			return Regex.IsMatch(number, @"^[0-9]{13}$", RegexOptions.IgnoreCase);
		}
	}
}
