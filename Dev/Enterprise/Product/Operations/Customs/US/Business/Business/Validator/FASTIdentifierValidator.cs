using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class FASTIdentifierValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidFASTId(number) ? string.Empty : FASTIdRightFormat;
		}

		public const string FASTIdRightFormat = "FAST identifier should be 7 alpha-numerics.";

		public static bool IsValidFASTId(ZString number)
		{
			return Regex.IsMatch(number, @"^[A-Z0-9]{7}$", RegexOptions.IgnoreCase);
		}
	}
}
