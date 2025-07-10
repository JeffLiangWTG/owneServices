using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class EncryptedConsigneeNumberValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidEncryptedNumber(number) ? "" : EncryptedNumberRightFormat;
		}

		public const string EncryptedNumberRightFormat = "Encrypted Number should be in the format, -CCCCCCCCCCC where C is a character.";

		public static bool IsValidEncryptedNumber(ZString number)
		{
			return Regex.IsMatch(number, @"^-[A-Z0-9-]{11}$", RegexOptions.IgnoreCase);
		}
	}
}
