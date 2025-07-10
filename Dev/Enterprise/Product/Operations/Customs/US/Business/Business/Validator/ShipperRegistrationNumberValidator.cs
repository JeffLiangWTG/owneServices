using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class ShipperRegistrationNumberValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidSFR(number) ? "" : SFRRightFormat;
		}

		public const string SFRRightFormat = "Shipper Registration Number should be (11 digits) in the format, NNNNNNNNNNN where N is a number.";

		public static bool IsValidSFR(ZString suretyCode)
		{
			return Regex.IsMatch(suretyCode, @"^[0-9]{11}$");
		}
	}
}
