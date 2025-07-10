using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class EstablishmentIdentifierValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidEstablishmentIdentifier(number) ? "" : EstablishmentIdentifierRightFormat;
		}

		public const string EstablishmentIdentifierRightFormat = "The FDA Establishment Identifier (FEI) is invalid.  Valid format for FEI is 1 to 10 digits.";

		public static bool IsValidEstablishmentIdentifier(ZString establishmentIdentifier)
		{
			return Regex.IsMatch(establishmentIdentifier, @"^[0-9]{1,10}$");
		}
	}
}
