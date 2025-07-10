using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class ACEIdentifierValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidACEId(number) ? string.Empty : ACEIdRightFormat;
		}

		public const string ACEIdRightFormat = "ACE identifier should be 10 alpha-numerics.";

		public static bool IsValidACEId(ZString number)
		{
			return Regex.IsMatch(number, @"^[A-Z0-9]{10}$", RegexOptions.IgnoreCase);
		}
	}
}
