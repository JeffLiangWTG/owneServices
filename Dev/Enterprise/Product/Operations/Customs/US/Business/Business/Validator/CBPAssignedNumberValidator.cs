using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class CBPAssignedNumberValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidCBPAssignedNumber(number) ? "" : CBPAssignedNumberRightFormat;
		}

		public const string CBPAssignedNumberRightFormat = "CBP Assigned Number should be in the format, YYDDPP-NNNNN\nwhere YY is the last two digits of the calendar year,\nDDPP is the district/port code where the number is assigned\nand N indicates a number.";

		public static bool IsValidCBPAssignedNumber(ZString cBPAssignedNumber)
		{
			return Regex.IsMatch(cBPAssignedNumber, @"^[0-9]{2}[A-Z0-9]{4}-[0-9]{5}$", RegexOptions.IgnoreCase);
		}
	}
}
