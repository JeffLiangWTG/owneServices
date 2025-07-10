using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class SuretyCodeValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidSuretyCode(number) ? "" : SuretyCodeRightFormat;
		}

		public const string SuretyCodeRightFormat = "Surety Code should be in the format, NNN where N is a number.";

		public static bool IsValidSuretyCode(ZString suretyCode)
		{
			return Regex.IsMatch(suretyCode, @"^[0-9]{3}$");
		}
	}
}
