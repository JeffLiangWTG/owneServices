using CargoWise.Common;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public static class XsdSchedule
	{
		public static string GetVessel(Xsd.Schedule schedule)
		{
			string result = null;
			if (schedule != null && schedule.Item != null)
			{
				if (schedule.Item is Xsd.ScheduleRoadRailFlight)
				{
				}
				else if (schedule.Item is Xsd.ScheduleSailing)
				{
					result = ((Xsd.ScheduleSailing)schedule.Item).VesselName;
				}
				else
				{
					ErrorReporter.ReportOnce(
						"UnknownScheduleItemBase" + schedule.Item.GetType().FullName,
						"Unknown schedule.Item thingy " + schedule.Item.GetType().FullName);
				}
			}
			if (result == null)
			{
				result = "";
			}
			return result;
		}

		public static string GetVoyage(Xsd.Schedule schedule)
		{
			string result = null;
			if (schedule != null && schedule.Item != null)
			{
				if (schedule.Item is Xsd.ScheduleRoadRailFlight)
				{
					result = ((Xsd.ScheduleRoadRailFlight)schedule.Item).FlightNoJourneyNoTruckRegNo;
				}
				else if (schedule.Item is Xsd.ScheduleSailing)
				{
					result = ((Xsd.ScheduleSailing)schedule.Item).VoyageNo;
				}
				else
				{
					ErrorReporter.ReportOnce(
						"UnknownScheduleItemBase" + schedule.Item.GetType().FullName,
						"Unknown schedule.Item thingy " + schedule.Item.GetType().FullName);
				}
			}
			if (result == null)
			{
				result = "";
			}
			return result;
		}

		public static Xsd.SailingBase[] GetSailings(Xsd.Schedule schedule)
		{
			Xsd.SailingBase[] result = System.Array.Empty<Xsd.SailingBase>();
			if (schedule != null && schedule.Item != null)
			{
				if (schedule.Item is Xsd.ScheduleRoadRailFlight)
				{
					Xsd.FlightWithLoadDischargePortsAndConsolsCollection flights = ((Xsd.ScheduleRoadRailFlight)schedule.Item).Flights;
					if (flights != null)
					{
						result = new Xsd.SailingBase[flights.Count];
						for (int i = 0; i < result.Length; i++)
						{
							result[i] = flights[i];
						}
					}
				}
				else if (schedule.Item is Xsd.ScheduleSailing)
				{
					Xsd.SailingWithLoadDischargePortsAndConsolsCollection sailings = ((Xsd.ScheduleSailing)schedule.Item).Sailings;
					if (sailings != null)
					{
						result = new Xsd.SailingBase[sailings.Count];
						for (int i = 0; i < result.Length; i++)
						{
							result[i] = sailings[i];
						}
					}
				}
				else
				{
					ErrorReporter.ReportOnce(
						"UnknownScheduleItemBase" + schedule.Item.GetType().FullName,
						"Unknown schedule.Item thingy " + schedule.Item.GetType().FullName);
				}
			}
			return result;
		}
	}
}
