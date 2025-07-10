using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class PedimentoNumberValidator
	{
		public static string Validate(ZString number)
		{
			return IsValidPedimentoNumber(number) ? "" : PedimentoNumberRightFormat;
		}

		public const string PedimentoNumberRightFormat = "Pedimento Number should be in the format, YYPPBBBBDDDDDDD\nwhere YY is the last two digits of the calendar year,\r\nPP is the Mexican Customs Port Code,\r\nBBBB is the Mexican Broker Code\r\nand DDDDDDD is the Document Number.";

		public static bool IsValidPedimentoNumber(ZString pedimentoNumber)
		{
			return Regex.IsMatch(pedimentoNumber, @"^[0-9]{15}$", RegexOptions.IgnoreCase);
		}
	}
}
