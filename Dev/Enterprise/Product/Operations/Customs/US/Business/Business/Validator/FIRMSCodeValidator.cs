using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class FIRMSCodeValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidFIRMS(number) ? "" : FIRMSCodeRightFormat;
		}

		public const string FIRMSCodeRightFormat = "FIRMS Code should be 4 alpha-numerics";

		public static bool IsValidFIRMS(ZString number)
		{
			return Regex.IsMatch(number, @"^[A-Z0-9]{4}$", RegexOptions.IgnoreCase);
		}
	}
}
