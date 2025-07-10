using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Business
{
	public static class OneStopEventHandler
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constants")]
		public static class EventTypeCodes
		{
			public const string Empty = "Empty";
			public const string VGM = "VGM";
			public const string Reefer = "Reefer";
			public const string Haz = "Haz";
		}

		public static bool IsEventTypeAllowedForUpdatingDates(string eventType)
		{
			return eventType.IsNullOrEmpty() || eventType != EventTypeCodes.Empty && eventType != EventTypeCodes.VGM && eventType != EventTypeCodes.Reefer && eventType != EventTypeCodes.Haz;
		}

		public static void ProcessOneStopEvent(this Transport transport, IStmALog log)
		{
			log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out var eventFacitlity);
			log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out var eventLocation);
			log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Type, out var eventType);

			if (eventFacitlity == EventConstants.Facilities.Code.Terminal)
			{
				ZDateTime eventTime = log.SL_EventTime;
				var column = string.Empty;

				if (log.SL_SE_NKEvent == Events.CutOffDate.Code && eventLocation == transport.JW_RL_NKLoadPort)
				{
					switch (eventType)
					{
						case EventTypeCodes.VGM:
							column = AutoJobConsolTransport.Schema.JW_VGMCutOff;
							break;
						case EventTypeCodes.Reefer:
							column = AutoJobConsolTransport.Schema.JW_ReeferCutOff;
							break;
						case EventTypeCodes.Haz:
							column = AutoJobConsolTransport.Schema.JW_DGCutOff;
							break;
						case null:
						case "":
							column = AutoJobConsolTransport.Schema.JW_TerminalCutOff;
							break;
					}
				}
				else if (log.SL_SE_NKEvent == Events.ReceiptCommenced.Code && eventLocation == transport.JW_RL_NKLoadPort)
				{
					switch (eventType)
					{
						case EventTypeCodes.Reefer:
							column = AutoJobConsolTransport.Schema.JW_ReeferReceivalCommences;
							break;
						case EventTypeCodes.Haz:
							column = AutoJobConsolTransport.Schema.JW_DGReceivalCommences;
							break;
						case null:
						case "":
							column = AutoJobConsolTransport.Schema.JW_TerminalReceivalCommences;
							break;
					}
				}
				else if (log.SL_SE_NKEvent == Events.CargoAvailable.Code && eventLocation == transport.JW_RL_NKDiscPort && string.IsNullOrEmpty(eventType))
				{
					column = AutoJobConsolTransport.Schema.JW_TerminalAvailabilityDate;
				}
				else if (log.SL_SE_NKEvent == Events.StorageCommenced.Code && eventLocation == transport.JW_RL_NKDiscPort && string.IsNullOrEmpty(eventType))
				{
					column = AutoJobConsolTransport.Schema.JW_TerminalStorageDate;
				}

				if (!string.IsNullOrEmpty(column))
				{
					transport[column] = eventTime;
				}
			}
			else if (eventFacitlity == EventConstants.Facilities.Code.Depot)
			{
				var eventTime = log.SL_EventTime;
				var column = string.Empty;

				if (log.SL_SE_NKEvent == Events.CargoAvailable.Code && eventLocation == transport.JW_RL_NKDiscPort && string.IsNullOrEmpty(eventType))
				{
					column = AutoJobConsolTransport.Schema.JW_DepotAvailabilityDate;
				}

				if (!string.IsNullOrEmpty(column))
				{
					transport[column] = eventTime;
				}
			}
		}
	}
}
