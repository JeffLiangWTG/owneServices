using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class RoutingResponseLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public RoutingResponseLine(string line, BusinessObjectFactory factory, RoutingRequest request = null)
			: base(factory)
		{
			this.request = request;

			if (line.Length > 0)
			{
				Origin = line.Substring(originStartIndex, originLength).Trim();
				OriginTerminal = line.Substring(originTerminalStartIndex, originTerminalLength).Trim();

				Destination = line.Substring(destinationStartLength, destinationLength).Trim();
				DestinationTerminal = line.Substring(destinationTerminalStartIndex, destinationTerminalLength).Trim();

				DepartureTime = line.Substring(departureTimeStartIndex, departureTimeLength).Trim();
				ArrivalTime = line.Substring(arrivalTimeStartIndex, arrivalTimeLength).Trim();

				TicketingCarrier = line.Substring(ticketingCarrierStartIndex, ticketingCarrierLength).Trim();
				FlightNumber = line.Substring(flightNumberStartIndex, flightNumberLength).Trim();

				Aircraft = line.Substring(aircraftStartIndex, aircraftLength).Trim();

				Stops = int.Parse(line.Substring(stopsStartIndex, stopsLength));
				MilesDistance = int.Parse(line.Substring(milesDistanceStartIndex, milesDistanceLength));

				if (line.Length >= lineExtensionLength)
				{
					OperationDay = line.Substring(operationDayStartIndex, operationDayLength).Trim().Replace('.', '_');
					EffectiveDate = ParseStrToLineDate(line.Substring(effectiveDateStartIndex, effectiveDateLength).Trim());
					DiscontinuedDate = ParseStrToLineDate(line.Substring(discontinuedDateStartIndex, discontinuedDateLength).Trim());
					FlightType = ConvertServiceTypeToFlightType(line[FlightTypeStartIndex]);
				}

				if (line.Length >= lineContainsCO2Emission)
				{
					var co2EmissionEndIndex = line.IndexOf('>', co2EmissionStartIndex);
					var co2String = line.Substring(co2EmissionStartIndex, co2EmissionEndIndex - co2EmissionStartIndex).Trim();
					if (ZDecimal.TryParse(co2String, out var co2Val) && co2Val != 0m)
					{
						CO2Emission = co2Val;
					}
				}
			}
		}

		public RoutingResponseLine Clone(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			var line = new RoutingResponseLine(string.Empty, factory, request);

			using (line.GetValidationSuspender())
			using (line.SuspendSettingHasChanges())
			{
				line.Origin = Origin;
				line.OriginTerminal = OriginTerminal;

				line.Destination = Destination;
				line.DestinationTerminal = DestinationTerminal;

				line.DepartureTime = DepartureTime;
				line.ArrivalTime = ArrivalTime;

				line.TicketingCarrier = TicketingCarrier;
				line.FlightNumber = FlightNumber;

				line.Aircraft = Aircraft;

				line.Stops = Stops;
				line.MilesDistance = MilesDistance;

				line.OperationDay = OperationDay;
				line.EffectiveDate = EffectiveDate;
				line.DiscontinuedDate = DiscontinuedDate;
				line.FlightType = FlightType;
			}

			return line;
		}

		#region Properties

		public ZString Origin { get; private set; }
		public ZPropertyInfo OriginInfo { get { return GetZPropertyInfo(nameof(Origin)); } }

		public ZString OriginDescription
		{
			get
			{
				RefUNLOCO loco = RefUNLOCO.LoadFromIATA(Factory, Origin);
				return loco != null ? loco.RL_PortName : ZString.Empty;
			}
		}
		public ZPropertyInfo OriginDescriptionInfo { get { return GetZPropertyInfo(nameof(OriginDescription)); } }

		public ZString Destination { get; private set; }
		public ZPropertyInfo DestinationInfo { get { return GetZPropertyInfo(nameof(Destination)); } }

		public ZString DestinationDescription
		{
			get
			{
				RefUNLOCO loco = RefUNLOCO.LoadFromIATA(Factory, Destination);
				return loco != null ? loco.RL_PortName : ZString.Empty;
			}
		}
		public ZPropertyInfo DestinationDescriptionInfo { get { return GetZPropertyInfo(nameof(DestinationDescription)); } }

		public ZString OriginTerminal { get; private set; }
		public ZPropertyInfo OriginTerminalInfo { get { return GetZPropertyInfo(nameof(OriginTerminal)); } }

		public ZString DestinationTerminal { get; private set; }
		public ZPropertyInfo DestinationTerminalInfo { get { return GetZPropertyInfo(nameof(DestinationTerminal)); } }

		public ZString DepartureTime { get; private set; }
		public ZPropertyInfo DepartureTimeInfo { get { return GetZPropertyInfo(nameof(DepartureTime)); } }

		public ZString DepartureDescription
		{
			get
			{
				return request != null
					? RoutingResponseHeader.ConvertDateFormat(request.IncludeWeeklyTimetable, request.DepartureDate, DepartureTime)
					: ZString.Empty;
			}
		}
		public ZPropertyInfo DepartureDescriptionInfo { get { return GetZPropertyInfo(nameof(DepartureDescription)); } }

		public ZString ArrivalTime { get; private set; }
		public ZPropertyInfo ArrivalTimeInfo { get { return GetZPropertyInfo(nameof(ArrivalTime)); } }

		public ZString ArrivalDescription
		{
			get
			{
				return request != null
					? RoutingResponseHeader.ConvertDateFormat(request.IncludeWeeklyTimetable, request.DepartureDate, ArrivalTime)
					: ZString.Empty;
			}
		}
		public ZPropertyInfo ArrivalDescriptionInfo { get { return GetZPropertyInfo(nameof(ArrivalDescription)); } }

		public ZString TicketingCarrier { get; private set; }
		public ZPropertyInfo TicketingCarrierInfo { get { return GetZPropertyInfo(nameof(TicketingCarrier)); } }

		public ZString FlightNumber { get; private set; }
		public ZPropertyInfo FlightNumberInfo { get { return GetZPropertyInfo(nameof(FlightNumber)); } }

		public ZString Aircraft { get; private set; }
		public ZPropertyInfo AircraftInfo { get { return GetZPropertyInfo(nameof(Aircraft)); } }

		public ZString AircraftForImporting => Aircraft.Substring(aircraftStartIndexForImporting, aircraftLengthForImporting).Trim();
		public bool HasMultipleAircraftTypes => Aircraft.Length > aircraftLengthForImporting;

		public ZInt Stops { get; private set; }
		public ZPropertyInfo StopsInfo { get { return GetZPropertyInfo(nameof(Stops)); } }

		public ZInt MilesDistance { get; private set; }
		public ZPropertyInfo MilesDistanceInfo { get { return GetZPropertyInfo(nameof(MilesDistance)); } }

		public ZString OperationDay { get; private set; }
		public ZPropertyInfo OperationDayInfo { get { return GetZPropertyInfo(nameof(OperationDay)); } }

		public ZDateTime EffectiveDate { get; private set; }
		public ZPropertyInfo EffectiveDateInfo { get { return GetZPropertyInfo(nameof(EffectiveDate)); } }

		public ZDateTime DiscontinuedDate { get; private set; }
		public ZPropertyInfo DiscontinuedDateInfo { get { return GetZPropertyInfo(nameof(DiscontinuedDate)); } }

		public ZString FlightType { get; private set; }
		public ZPropertyInfo FlightTypeInfo { get { return GetZPropertyInfo(nameof(FlightType)); } }

		public ZDecimal CO2Emission { get; private set; }
		public ZPropertyInfo CO2EmissionInfo { get { return GetZPropertyInfo(nameof(CO2Emission)); } }

		#endregion

		#region Start index and length of line fields

		const int originStartIndex = 0;
		const int originLength = 3;
		const int originTerminalStartIndex = 3;
		const int originTerminalLength = 2;
		const int destinationStartLength = 6;
		const int destinationLength = 3;
		const int destinationTerminalStartIndex = 9;
		const int destinationTerminalLength = 2;
		const int departureTimeStartIndex = 12;
		const int departureTimeLength = 7;
		const int arrivalTimeStartIndex = 20;
		const int arrivalTimeLength = 7;
		const int ticketingCarrierStartIndex = 28;
		const int ticketingCarrierLength = 2;
		const int flightNumberStartIndex = 32;
		const int flightNumberLength = 5;
		const int aircraftStartIndex = 38;
		const int aircraftLength = 7;
		const int stopsStartIndex = 48;
		const int stopsLength = 1;
		const int milesDistanceStartIndex = 50;
		const int milesDistanceLength = 5;

		const int lineExtensionLength = 122;
		const int operationDayStartIndex = 95;
		const int operationDayLength = 7;
		const int effectiveDateStartIndex = 103;
		const int effectiveDateLength = 8;
		const int discontinuedDateStartIndex = 112;
		const int discontinuedDateLength = 8;
		const int FlightTypeStartIndex = 121;

		const int aircraftStartIndexForImporting = 0;
		const int aircraftLengthForImporting = 3;

		const int lineContainsCO2Emission = 135;
		const int co2EmissionStartIndex = 123;

		#endregion

		#region Implementation

		static ZString ConvertServiceTypeToFlightType(char serviceType)
		{
			var passengerServiceTypes = new char[] { 'J', 'S', 'U', 'Q', 'G', 'B', 'C', 'O', 'L', 'R' };
			return passengerServiceTypes.Any(s => s == serviceType)
				? FlightTypeConstants.PassengerOnly
				: FlightTypeConstants.CargoOnly;
		}

		static ZDateTime ParseStrToLineDate(string value)
		{
			ZDateTime date;
			if (!ZDateTime.TryParseExact(value, out date, "yy/MM/dd"))
			{
				date = ZDateTime.Empty;
			}

			return date;
		}

		readonly RoutingRequest request;

		#endregion
	}
}
