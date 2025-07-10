using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class SocialSecurityNumberValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidSSN(number) ? "" : SocialSecurityNumberRightFormat;
		}
		public const string SSNWithMask = "***-**-****";

		public const string SocialSecurityNumberRightFormat = "Social Security Number should be in the format, NNN-NN-NNNN where N is a number.";

		public const string DoesNotHavePermissionToSendSSNErrorMessage = "You don't have permission to send Social Security Number.";

		public static bool IsValidSSN(ZString number)
		{
			return Regex.IsMatch(number, @"^[0-9]{3}-[0-9]{2}-[0-9]{4}$", RegexOptions.IgnoreCase);
		}
	}
}
