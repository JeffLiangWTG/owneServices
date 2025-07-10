using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class USAirlineNumberValidator
	{
		public static string CheckAirFlightNo(ZString flightNumber, BusinessObjectFactory factory)
		{
			var result = string.Empty;

			if (!flightNumber.IsEmpty)
			{
				var regexFormat = "^([A-Z]{0}|[A-Z]{2})[0-9]{3,4}[A-Z]{0,1}$";
				var airlineCode = flightNumber.Left(2);
				if (airlineCode.Length == 2 && !airlineCode.IsNumbersOnlyOrEmpty && !airlineCode.IsLettersOnlyOrEmpty)
				{
					var validAirline = factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, airlineCode));
					if (validAirline != null)
					{
						regexFormat = @"^[A-Z0-9]{2}\d{3,4}\w?$";
					}
				}

				if (!Regex.IsMatch(flightNumber, regexFormat, RegexOptions.IgnoreCase))
				{
					result = InvalidFlightNoFormat;
				}
			}

			return result;
		}

		internal const string InvalidFlightNoFormat = "Please enter a valid Flight Number. The numeric component of the Flight Number must be three or four numbers, or three or four numbers followed by an alpha character. eg: QF001, UA0254B, 5X001, 001, 001A, 1750, etc.";
	}
}
