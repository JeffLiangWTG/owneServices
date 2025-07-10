using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class RoutingResponseHeader : NonPersistentBusinessObject, IObsoleteValidation
	{
		public RoutingResponseHeader(string line1, BusinessObjectFactory factory, RoutingRequest request = null)
			: base(factory)
		{
			this.request = request;

			if (line1.Length > 0 && line1.Contains("<") && line1.Contains(">"))
			{
				var pattern = new Regex(@"(\d)(\d)(\d) (\w{3}) (\w{3})\s+(\d+:\d{2}) ((?:\d\+| {2})\d{2}:\d{2}) ((?:\d\+| {2})\d{2}:\d{2}) (\w{2}| {2}) (\w{2}| {2}) (\w{2}| {2}) (\w{2}| {2})\s+(?:((?:\d|\/){10}) ((?:\d|\/){10}) ([.\d]{7}))?");
				var match = pattern.Match(line1);
				if (match.Success)
				{
					Connections = int.Parse(match.Groups[1].Value);
					Stops = int.Parse(match.Groups[2].Value);
					Layers = int.Parse(match.Groups[3].Value);

					Origin = match.Groups[4].Value.Trim();
					Destination = match.Groups[5].Value.Trim();

					Duration = match.Groups[6].Value.Trim();
					DepartureTime = match.Groups[7].Value.Trim();
					ArrivalTime = match.Groups[8].Value.Trim();

					Carrier1 = match.Groups[9].Value.Trim();
					Carrier2 = match.Groups[10].Value.Trim();
					Carrier3 = match.Groups[11].Value.Trim();
					Carrier4 = match.Groups[12].Value.Trim();

					if (match.Groups.Count > 15)
					{
						EffectiveDate = ParseStringToDate(match.Groups[13].Value.Trim());
						DiscontinuedDate = ParseStringToDate(match.Groups[14].Value.Trim());
						OperationDay = match.Groups[15].Value.Trim().Replace('.', '_');
					}
				}

				ProcessRawLines(line1.Substring(line1.IndexOf("<", StringComparison.Ordinal)), factory);

				var linesCount = Lines.Count;

				if (OperationDay.IsEmpty)
				{
					OperationDay = linesCount >= 1 ? Lines[0].OperationDay : ZString.Empty;
				}

				var separator = "-";

				var routingResponseLines = Lines.Cast<RoutingResponseLine>();

				Flight = ZString.Join(separator, routingResponseLines.Where(line => !line.TicketingCarrier.IsEmpty && !line.FlightNumber.IsEmpty)
					.Select(line => ZString.Format("{0}{1}", line.TicketingCarrier, line.FlightNumber))
					.ToArray());

				FlightType = ZString.Join(separator, routingResponseLines.Where(line => !line.FlightType.IsEmpty)
					.Select(line => line.FlightType).ToArray());

				Aircraft = ZString.Join(separator, routingResponseLines.Where(line => !line.Aircraft.IsEmpty)
					.Select(line => line.Aircraft).ToArray());

				var origins = routingResponseLines.Where(line => !line.Origin.IsEmpty).Skip(1);
				Via = origins.Any()
					? ZString.Join(separator, origins.Select(line => line.Origin).ToArray())
					: (ZString)(ZArchitecture.Core.NoResString)"Direct"; // Via port used in online airline schedules
			}
		}

		void ProcessRawLines(string lines, BusinessObjectFactory factory)
		{
			var rawLines = lines.Split('<');

			foreach (var rawLine in rawLines)
			{
				if (rawLine.Trim().Length > 0)
				{
					var newLine = new RoutingResponseLine(rawLine, factory, request);
					Lines.Add(newLine);
				}
			}
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

		public ZInt Connections { get; private set; }
		public ZPropertyInfo ConnectionsInfo { get { return GetZPropertyInfo(nameof(Connections)); } }

		public ZInt Stops { get; private set; }
		public ZPropertyInfo StopsInfo { get { return GetZPropertyInfo(nameof(Stops)); } }

		public ZInt Layers { get; private set; }
		public ZPropertyInfo LayersInfo { get { return GetZPropertyInfo(nameof(Layers)); } }

		public ZString Duration { get; private set; }
		public ZPropertyInfo DurationInfo { get { return GetZPropertyInfo(nameof(Duration)); } }

		public ZString DepartureTime { get; private set; }
		public ZPropertyInfo DepartureTimeInfo { get { return GetZPropertyInfo(nameof(DepartureTime)); } }

		public ZString DepartureDescription
		{
			get
			{
				return request != null
					? ConvertDateFormat(request.IncludeWeeklyTimetable, request.DepartureDate, DepartureTime)
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
					? ConvertDateFormat(request.IncludeWeeklyTimetable, request.DepartureDate, ArrivalTime)
					: ZString.Empty;
			}
		}
		public ZPropertyInfo ArrivalDescriptionInfo { get { return GetZPropertyInfo(nameof(ArrivalDescription)); } }

		public ZString Carrier1 { get; private set; }
		public ZPropertyInfo Carrier1Info { get { return GetZPropertyInfo(nameof(Carrier1)); } }

		public ZString Carrier2 { get; private set; }
		public ZPropertyInfo Carrier2Info { get { return GetZPropertyInfo(nameof(Carrier2)); } }

		public ZString Carrier3 { get; private set; }
		public ZPropertyInfo Carrier3Info { get { return GetZPropertyInfo(nameof(Carrier3)); } }

		public ZString Carrier4 { get; private set; }
		public ZPropertyInfo Carrier4Info { get { return GetZPropertyInfo(nameof(Carrier4)); } }

		public ZString Flight { get; private set; }
		public ZPropertyInfo FlightInfo { get { return GetZPropertyInfo(nameof(Flight)); } }

		public ZString FlightType { get; private set; }
		public ZPropertyInfo FlightTypeInfo { get { return GetZPropertyInfo(nameof(FlightType)); } }

		public ZString Aircraft { get; private set; }
		public ZPropertyInfo AircraftInfo { get { return GetZPropertyInfo(nameof(Aircraft)); } }

		public ZString OperationDay { get; private set; }
		public ZPropertyInfo OperationDayInfo { get { return GetZPropertyInfo(nameof(OperationDay)); } }

		public ZDateTime EffectiveDate { get; private set; }
		public ZPropertyInfo EffectiveDateInfo { get { return GetZPropertyInfo(nameof(EffectiveDate)); } }

		public ZDateTime DiscontinuedDate { get; private set; }
		public ZPropertyInfo DiscontinuedDateInfo { get { return GetZPropertyInfo(nameof(DiscontinuedDate)); } }

		public ZString Via { get; private set; }
		public ZPropertyInfo ViaInfo { get { return GetZPropertyInfo(nameof(Via)); } }

		public ZDecimal CO2EmissionTotal => Lines.Cast<RoutingResponseLine>().Any(l => l.CO2Emission.IsEmpty)
			? new ZDecimal(null)
			: Lines.Cast<RoutingResponseLine>().Sum(l => l.CO2Emission);

		public ZPropertyInfo CO2EmissionTotalInfo { get { return GetZPropertyInfo(nameof(CO2EmissionTotal)); } }

		#endregion

		#region Connection Points

		public RoutingResponseLineCollection Lines
		{
			get { return fLines ?? (fLines = new RoutingResponseLineCollection(Factory)); }
		}
		RoutingResponseLineCollection fLines;

		#endregion

		#region TryCreateEnterpriseVoyageSailing

		public RoutingResponseHeader Clone(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			var header = new RoutingResponseHeader(string.Empty, factory, request);

			using (header.GetValidationSuspender())
			using (header.SuspendSettingHasChanges())
			{
				header.Connections = Connections;
				header.Stops = Stops;
				header.Layers = Layers;

				header.Origin = Origin;
				header.Destination = Destination;

				header.Duration = Duration;
				header.DepartureTime = DepartureTime;
				header.ArrivalTime = ArrivalTime;

				header.Carrier1 = Carrier1;
				header.Carrier2 = Carrier2;
				header.Carrier3 = Carrier3;
				header.Carrier4 = Carrier4;

				foreach (RoutingResponseLine line in Lines)
				{
					header.Lines.Add(line.Clone(factory));
				}

				header.OperationDay = OperationDay;
				header.Flight = Flight;
				header.FlightType = FlightType;
				header.Aircraft = Aircraft;
				header.Via = Via;

				header.EffectiveDate = EffectiveDate;
				header.DiscontinuedDate = DiscontinuedDate;
				header.OperationDay = OperationDay;
			}

			return header;
		}

		public static IReadOnlyList<BusinessObjectCollection> TryCreateEnterpriseVoyagesSailings(RoutingResponseHeader header, IEnumerable<ZDateTime> departureDates, BusinessObjectFactory factory)
		{
			var sailingList = new List<BusinessObjectCollection>();

			foreach (ZDateTime departureDate in departureDates)
			{
				if (header.HasOperation(departureDate))
				{
					var sailings = TryCreateEnterpriseVoyageSailing(header, departureDate, factory);
					if (!sailings.IsNullOrEmpty())
					{
						sailingList.Add(sailings);
					}
				}
			}

			return sailingList;
		}

		static BusinessObjectCollection TryCreateEnterpriseVoyageSailing(RoutingResponseHeader header, ZDateTime departureDate, BusinessObjectFactory factory)
		{
			var sailings = new JobSailingCollection(factory);
			var lines = header.Lines;

			if (lines.Count > 0 && departureDate.IsValid)
			{
				var log = GenerateCreationLog(header, departureDate);

				foreach (RoutingResponseLine line in lines)
				{
					log += GenerateCreationLog(line, departureDate);

					var sailing = TryCreateEnterpriseVoyageSailing(line, departureDate, factory);
					if (sailing != null)
					{
						sailings.Add(sailing);
					}
				}

				header.RouteCreationLogs.Add(log);
			}

			return sailings;
		}

		public bool OperationDayHasNumber(int dayNumber)
		{
			return OperationDay.Contains(dayNumber.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
		}

		public bool HasOperation(ZDateTime date)
		{
			if (date.IsEmpty || !date.IsValid)
			{
				return false;
			}

			var dayNumber = GetDayNumber(date);

			if (!EffectiveDate.IsEmpty && !DiscontinuedDate.IsEmpty)
			{
				return OperationDayHasNumber(dayNumber) && EffectiveDate <= date && date <= DiscontinuedDate;
			}
			else
			{
				return OperationDayHasNumber(dayNumber);
			}
		}

		public static ZString ConvertDateFormat(ZBool includeWeekly, ZDateTime departureDate, ZString timeString)
		{
			if (includeWeekly)
			{
				return timeString;
			}

			var days = 0;

			var regex = new Regex(@"^(?:([1-9])\+|)(.*)$");
			var match = regex.Match(timeString);
			if (match.Success && match.Groups.Count == 3)
			{
				var daysStr = match.Groups[1].Value;
				if (!string.IsNullOrEmpty(daysStr))
				{
					days = int.Parse(daysStr, CultureInfo.InvariantCulture);
				}

				timeString = match.Groups[2].Value;
			}

			return departureDate.AddDays(days).ToShortDateString() + ' ' + timeString;
		}

		public static int GetDayNumber(ZDateTime date)
		{
			var dayOfWeek = date.DayOfWeek;
			return (dayOfWeek == DayOfWeek.Sunday) ? 7 : (int)dayOfWeek;
		}

		public IList<string> RouteCreationLogs { get; } = new List<string>();

		readonly RoutingRequest request;

		#region Implementation

		static BusinessObject TryCreateEnterpriseVoyageSailing(RoutingResponseLine line, ZDateTime departureDate, BusinessObjectFactory factory)
		{
			BusinessObject sailing = null;

			if (line != null)
			{
				var voyageFlight = line.TicketingCarrier + line.FlightNumber;
				var flightDate = RoutingUpdaterHelper.UpdateDateTime(departureDate, line.DepartureTime);

				var voyage = FindSimilarVoyage(voyageFlight, flightDate, factory)
					?? CreateVoyage(voyageFlight, line.FlightType == FlightTypeConstants.CargoOnly, factory);

				voyage[JobVoyageSchema.JV_FlightDate] = flightDate;
				voyage[JobVoyageSchema.JV_AircraftType] = line.AircraftForImporting;

				sailing = CreateVoyageOriginDestinationSailing(voyage, line, departureDate, out bool anyOriginDestOrSailingCreated);
				if (!voyage.IsInDatabase && !anyOriginDestOrSailingCreated)
				{
					voyage.Delete();
				}
			}

			return sailing;
		}

		static BusinessObject CreateVoyageOriginDestinationSailing(BusinessObject voyage, RoutingResponseLine line, ZDateTime departureDate, out bool anyOriginDestOrSailingCreated)
		{
			anyOriginDestOrSailingCreated = false;

			var loadingPort = RoutingUpdaterHelper.GetUNLOCOFromIATACode(line.Origin, voyage.Factory);
			var dischargePort = RoutingUpdaterHelper.GetUNLOCOFromIATACode(line.Destination, voyage.Factory);

			if (!loadingPort.IsEmpty && !dischargePort.IsEmpty)
			{
				var origin = CreateOrUpdateVoyageOrigin(voyage, loadingPort, line.DepartureTime, departureDate);
				var destination = CreateOrUpdateVoyageDestination(voyage, dischargePort, line.ArrivalTime, departureDate);
				var sailing = CreateVoyageSailing(line, origin, destination, voyage.Factory);

				anyOriginDestOrSailingCreated |= (!origin.IsInDatabase || !destination.IsInDatabase || (sailing != null && !sailing.IsInDatabase));
				return sailing;
			}

			return null;
		}

		static BusinessObject CreateVoyageSailing(RoutingResponseLine line, BusinessObject origin, BusinessObject destination, BusinessObjectFactory factory)
		{
			BusinessObject sailing = null;

			if (line.Origin != line.Destination)
			{
				sailing = FindOrCreateSailing(origin, destination, factory);
			}

			return sailing;
		}

		static BusinessObject CreateOrUpdateVoyageOrigin(BusinessObject voyage, ZString loadingPort, ZString lineDepartureTime, ZDateTime departureDate)
		{
			var departureTime = RoutingUpdaterHelper.UpdateDateTime(departureDate, lineDepartureTime);
			var origin = FindMatchingOrigin(loadingPort, voyage);

			if (origin == null)
			{
				origin = (BusinessObject)voyage.Factory.New<IVoyageOrigin>();

				origin[JobVoyOriginSchema.JA_RL_NKPortOfLoading] = loadingPort;
				origin[JobVoyOriginSchema.JA_AutoCreated] = true;
				origin[JobVoyOriginSchema.JA_JV] = voyage.PK;
			}

			origin[JobVoyOriginSchema.JA_E_DEP] = departureTime;
			return origin;
		}

		static BusinessObject CreateOrUpdateVoyageDestination(BusinessObject voyage, ZString dischargePort, ZString lineArrivalTime, ZDateTime departureDate)
		{
			var arrivalTime = RoutingUpdaterHelper.UpdateDateTime(departureDate, lineArrivalTime);
			var destination = FindMatchinDestination(dischargePort, voyage);

			if (destination == null)
			{
				destination = (BusinessObject)voyage.Factory.New<IVoyageDestination>();
				destination[JobVoyDestinationSchema.JB_RL_NKPortOfDischarge] = dischargePort;
				destination[JobVoyDestinationSchema.JB_AutoCreated] = true;
				destination[JobVoyDestinationSchema.JB_JV] = voyage.PK;
			}

			destination[JobVoyDestinationSchema.JB_E_ARV] = arrivalTime;
			return destination;
		}

		static BusinessObject CreateVoyage(ZString voyageFlight, bool isCargoOnly, BusinessObjectFactory factory)
		{
			var voyage = (BusinessObject)factory.New<IJobVoyage>();

			voyage[JobVoyageSchema.JV_VoyageFlight] = voyageFlight;
			voyage[JobVoyageSchema.JV_IsCargoOnly] = isCargoOnly;
			voyage[JobVoyageSchema.JV_AirSeaRoad] = Core.Constants.TransportModes.Air;
			voyage[JobVoyageSchema.JV_IsActive] = true;

			return voyage;
		}

		static BusinessObject FindSimilarVoyage(ZString voyageFlight, ZDateTime flightDate, BusinessObjectFactory factory)
		{
			BusinessObject result = null;

			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, voyageFlight);
			query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Core.Constants.TransportModes.Air);
			query.AddToFilter(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.GreaterThan, flightDate.AddDays(-1));
			query.AddToFilter(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.LessThan, flightDate.AddDays(1));
			query.OrderBy = JobVoyageSchema.JV_FlightDate.Name + OrderByClause.Descending;

			var voyages = factory.Load<IJobVoyage>(query);
			if (voyages.Length > 0)
			{
				var comparer = new ClosestDateComparer(flightDate);
				result = (BusinessObject)voyages.OrderBy(v => (ZDateTime)((BusinessObject)v)[JobVoyageSchema.JV_FlightDate], comparer).FirstOrDefault();
			}

			return result;
		}

		static BusinessObject FindOrCreateSailing(BusinessObject origin, BusinessObject destination, BusinessObjectFactory factory)
		{
			var sailing = FindMatchingSailing(origin, destination);

			if (sailing == null)
			{
				sailing = (BusinessObject)factory.New<IJobSailing>();

				sailing[JobSailingSchema.JX_JA] = origin.PK;
				sailing[JobSailingSchema.JX_JB] = destination.PK;
				sailing[JobSailingSchema.JX_IsPublished] = true;
			}

			return sailing;
		}

		internal static BusinessObject FindMatchingOrigin(ZString loadingPort, BusinessObject voyage)
		{
			var query = new ZQuery(JobVoyOriginSchema.JA_JV, voyage.PK);
			query.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, loadingPort);
			return (BusinessObject)voyage.Factory.LoadTop1<IVoyageOrigin>(query);
		}

		internal static BusinessObject FindMatchinDestination(ZString dischargePort, BusinessObject voyage)
		{
			var query = new ZQuery(JobVoyDestinationSchema.JB_JV, voyage.PK);
			query.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, dischargePort);
			return (BusinessObject)voyage.Factory.LoadTop1<IVoyageDestination>(query);
		}

		internal static BusinessObject FindMatchingSailing(BusinessObject origin, BusinessObject destination)
		{
			var query = new ZQuery(JobSailingSchema.JX_JA, origin.PK);
			query.AddToFilter(JobSailingSchema.JX_JB, destination.PK);
			return (BusinessObject)origin.Factory.LoadTop1<IJobSailing>(query);
		}

		public bool AnyLineHasMultipleAircraftTypes => Lines.Any(line => ((RoutingResponseLine)line).HasMultipleAircraftTypes);

		static string GenerateCreationLog(RoutingResponseHeader header, ZDateTime departureDate)
		{
			var departureTime = RoutingUpdaterHelper.UpdateDateTime(departureDate, header.DepartureTime).ToLongTimeString();
			var arrivalTime = RoutingUpdaterHelper.UpdateDateTime(departureDate, header.ArrivalTime).ToLongTimeString();

			return System.Environment.NewLine + ResString.GetMultilingualString(
							 "72F0BE03-70D0-4116-AD0E-2167267166BC",
							 @"{0} ({1}) - {2} ({3}) {4} - {5}",
							 header.OriginDescription,
							 header.Origin,
							 header.DestinationDescription,
							 header.Destination,
							 departureTime,
							 arrivalTime) + System.Environment.NewLine;
		}

		static string GenerateCreationLog(RoutingResponseLine line, ZDateTime departureDate)
		{
			var departureTime = RoutingUpdaterHelper.UpdateDateTime(departureDate, line.DepartureTime).ToLongTimeString();
			var arrivalTime = RoutingUpdaterHelper.UpdateDateTime(departureDate, line.ArrivalTime).ToLongTimeString();
			return ResString.GetMultilingualString(
							 "7064C271-2E86-4594-A5E3-15C40FE0852B",
							 "{0}{1} ({2}) - {3} ({4}) {5}{6} {7} - {8}",
							 "    ",
							 line.OriginDescription,
							 line.Origin,
							 line.DestinationDescription,
							 line.Destination,
							 line.TicketingCarrier,
							 line.FlightNumber,
							 departureTime,
							 arrivalTime) + System.Environment.NewLine;
		}

		static ZDateTime ParseStringToDate(string value) => ZDateTime.TryParseExact(value, out ZDateTime date, "yyyy/MM/dd") ? date : ZDateTime.Empty;

		#endregion

		#endregion
	}
}
