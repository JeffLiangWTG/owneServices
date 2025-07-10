using CargoWise.Common;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public static class XsdSailingBase
	{
		public static string GetLoadPort(Xsd.SailingBase sailing)
		{
			string result = null;
			if (sailing != null && sailing.IsSpecified)
			{
				if (sailing is Xsd.SailingWithLoadDischargePorts)
				{
					result = ((Xsd.SailingWithLoadDischargePorts)sailing).LoadPort;
				}
				else if (sailing is Xsd.FlightWithLoadDischargePorts)
				{
					result = ((Xsd.FlightWithLoadDischargePorts)sailing).LoadPort;
				}
			}
			if (result == null)
			{
				result = "";
			}
			return result;
		}

		public static string GetDischargePort(Xsd.SailingBase sailing)
		{
			string result = null;
			if (sailing != null && sailing.IsSpecified)
			{
				if (sailing is Xsd.SailingWithLoadDischargePorts)
				{
					result = ((Xsd.SailingWithLoadDischargePorts)sailing).DischargePort;
				}
				else if (sailing is Xsd.FlightWithLoadDischargePorts)
				{
					result = ((Xsd.FlightWithLoadDischargePorts)sailing).DischargePort;
				}
			}
			if (result == null)
			{
				result = "";
			}
			return result;
		}

		public static string GetVoyage(Xsd.SailingBase sailing)
		{
			string result = null;
			if (sailing != null && sailing.IsSpecified)
			{
				if (sailing is Xsd.FlightWithFlightNumber)
				{
					result = ((Xsd.FlightWithFlightNumber)sailing).FlightNoJourneyNoTruckRegNo;
				}
				else if (sailing is Xsd.SailingWithVesselVoyage)
				{
					result = ((Xsd.SailingWithVesselVoyage)sailing).VoyageNo;
				}
				else
				{
					ErrorReporter.ReportOnce(
						"UnknownSailingBase" + sailing.GetType().FullName,
						"Unknown Xsd.SailingBase sub-class " + sailing.GetType().FullName);
				}
			}
			if (result == null)
			{
				result = "";
			}
			return result;
		}
	}
}
