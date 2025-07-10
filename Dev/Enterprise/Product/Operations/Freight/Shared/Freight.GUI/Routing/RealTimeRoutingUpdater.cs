using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Routing.S8.Business;

namespace Enterprise.Freight.GUI
{
	public static class RealTimeRoutingUpdater
	{
		public static void UpdateRouteFromChosenRealTimeRoute(Transport mainTransport, TransportCollection transports, RoutingResponseHeader realTimeRoute, ZDateTime departureDate, bool isImporting)
		{
			try
			{
				foreach (RoutingResponseLine connection in realTimeRoute.Lines)
				{
					Transport transport = connection == realTimeRoute.Lines[0] ? mainTransport : transports.AddNew();

					transport.JW_TransportMode = Core.Constants.TransportModes.Air;
					transport.JW_RL_NKLoadPort = RoutingUpdaterHelper.GetUNLOCOFromIATACode(connection.Origin, mainTransport.Factory);
					transport.JW_RL_NKDiscPort = RoutingUpdaterHelper.GetUNLOCOFromIATACode(connection.Destination, mainTransport.Factory);
					transport.JW_VoyageFlight = connection.TicketingCarrier + connection.FlightNumber;

					using (transport.PreserveScheduleDates())
					{
						transport.JW_ETD = RoutingUpdaterHelper.UpdateDateTime(departureDate, connection.DepartureTime);
						transport.JW_ETA = RoutingUpdaterHelper.UpdateDateTime(departureDate, connection.ArrivalTime);
					}
					
					transport.JW_AircraftType = connection.AircraftForImporting;
					transport.JW_IsCargoOnly = connection.FlightType == FlightTypeConstants.CargoOnly;

					if (isImporting && !transport.JW_IsLinked)
					{
						var sailing = new SailingLocator(connection.Factory).FindSailingFromSailingManager(
							Core.Constants.TransportModes.Air,
							transport.JW_RL_NKLoadPort,
							transport.JW_RL_NKDiscPort,
							ZString.Empty,
							transport.JW_VoyageFlight,
							ZGuid.Empty,
							transport.JW_ETD,
							transport.JW_ETA)
							.Sailing;
						if (sailing != null)
						{
							transport.JW_IsLinked = true;
							transport.JW_JX = sailing.PK;
						}
					}
				}
			}
			catch (NullReferenceException ex)
			{
				int count = 0;

				#region SuppressResourceStringsCheckRegion

				string key = "Issue 00819046 International Logistics";
				string message = ex.Message
							+ "\r\n"
							+ ex.StackTrace;

				message += "\r\n\r\nMainTransport Information:";

				if (mainTransport != null)
				{
					message += ZString.Format("\r\nGetType(): {0}" +
																	 "\r\nPK: {1}" +
																	 "\r\n{2}: {3}" +
																	 "\r\n{4}: {5}" +
																	 "\r\n{6}: {7}" +
																	 "\r\n{8}: {9}" +
																	 "\r\n{10}: {11}" +
																	 "\r\n{12}: {13}",
																	 mainTransport.GetType(),
																	 mainTransport.PK,
																	 Transport.Schema.JW_TransportMode, (string.IsNullOrEmpty(mainTransport.JW_TransportMode) ? Transport.Schema.JW_TransportMode + " is null" : mainTransport.JW_TransportMode.ToString()),
																	 Transport.Schema.JW_RL_NKLoadPort, (string.IsNullOrEmpty(mainTransport.JW_RL_NKLoadPort) ? Transport.Schema.JW_RL_NKLoadPort + " is null" : mainTransport.JW_RL_NKLoadPort.ToString()),
																	 Transport.Schema.JW_RL_NKDiscPort, (string.IsNullOrEmpty(mainTransport.JW_RL_NKDiscPort) ? Transport.Schema.JW_RL_NKDiscPort + " is null" : mainTransport.JW_RL_NKDiscPort.ToString()),
																	 Transport.Schema.JW_VoyageFlight, (string.IsNullOrEmpty(mainTransport.JW_VoyageFlight) ? Transport.Schema.JW_VoyageFlight + " is null" : mainTransport.JW_VoyageFlight.ToString()),
																	 Transport.Schema.JW_ETD, mainTransport.JW_ETD,
																	 Transport.Schema.JW_ETA, mainTransport.JW_ETA);
				}
				else
				{
					message += "\r\nMainTransport is null";
				}

				message += "\r\nRealTimeRoute Information:";

				if (realTimeRoute != null)
				{
					foreach (RoutingResponseLine connection in realTimeRoute.Lines)
					{
						message += "\r\nGetType(): " + connection.GetType();
						message += "\r\nLine" + count;
						message += "\r\nPK: " + connection.PK;
						message += "\r\nOrigin: " + connection.Origin ?? "Origin is null";
						message += "\r\nDestination: " + connection.Destination ?? "Destination is null";
						message += "\r\nTicketingCarrier: " + connection.TicketingCarrier ?? "TicketingCarrier is null";
						message += "\r\nFlightNumber: " + connection.FlightNumber ?? "FlightNumber is null";
						message += "\r\nDepartureTime: " + connection.DepartureTime ?? "DepartureTime is null";
						message += "\r\nArrivalTime: " + connection.ArrivalTime ?? "ArrivalTime is null";

						count++;
					}
				}
				else
				{
					message += "\r\n\r\nRealTimeRoute is null";
				}

				message += "\r\n"
						+ "\r\nDepartureDate Information:"
						+ "\r\nDepartureDate: " + departureDate;

				#endregion

				ErrorReporter.ReportOnce(key, message);

				throw;
			}
		}
	}
}
