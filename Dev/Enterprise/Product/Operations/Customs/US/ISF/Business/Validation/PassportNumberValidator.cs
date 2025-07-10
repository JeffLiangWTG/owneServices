using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.ISF.Business
{
	public static class PassportNumberValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidPassport(number) ? "" : PassportNumberRightFormat;
		}

		public const string PassportNumberRightFormat = "Passport Number should be 5 to 9 alphanumeric characters.";

		public static bool IsValidPassport(ZString number)
		{
			return Regex.IsMatch(number, @"^[A-Z0-9]{5,9}$", RegexOptions.IgnoreCase);
		}
	}
}
