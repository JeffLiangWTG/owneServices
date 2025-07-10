using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public static class RoutingUpdaterHelper
	{
		public static ZString GetUNLOCOFromIATACode(string iata, BusinessObjectFactory factory)
		{
			var loco = RefUNLOCO.LoadFromIATA(factory, iata);
			return loco != null ? loco.RL_Code : ZString.Empty;
		}

		public static ZDateTime UpdateDateTime(ZDateTime initialDateTime, string delta)
		{
			var days = 0;
			var hours = 0;
			var minutes = 0;

			if (!string.IsNullOrEmpty(delta))
			{
				var regex = new Regex(@"^(?:([1-9])\+|)((?:[0-1][0-9])|(?:2[0-3])):([0-5][0-9])$");
				var match = regex.Match(delta);
				if (match.Success && match.Groups.Count == 4)
				{
					var daysStr = match.Groups[1].Value;
					if (!string.IsNullOrEmpty(daysStr))
					{
						days = int.Parse(daysStr, CultureInfo.InvariantCulture);
					}

					hours = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
					minutes = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
				}
			}

			return initialDateTime.Date.AddDays(days).AddHours(hours).AddMinutes(minutes);
		}
	}
}
