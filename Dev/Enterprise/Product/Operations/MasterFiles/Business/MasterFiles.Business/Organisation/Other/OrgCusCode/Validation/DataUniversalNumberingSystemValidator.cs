using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class DataUniversalNumberingSystemValidator
	{
		public static string DUNSNumberFormat
		{
			get { return Res.GetString("a1b1c1bf-fdda-45f6-888e-f6691197c786", "DUNS Number should be (9 digits) in the format: NNNNNNNNN, where N is a number."); }
		}

		public static string GetDUNSNumberError(ZString code)
		{
			return IsValidDUNS(code) ? string.Empty : DUNSNumberFormat;
		}

		public static bool IsValidDUNS(ZString number)
		{
			return Regex.IsMatch(number, @"^[0-9]{9}$", RegexOptions.IgnoreCase);
		}
	}
}
