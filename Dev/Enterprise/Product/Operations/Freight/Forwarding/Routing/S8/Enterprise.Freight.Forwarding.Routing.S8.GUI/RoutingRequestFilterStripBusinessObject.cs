using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI
{
	public class RoutingRequestFilterStripBusinessObject : FilterStripBusinessObject
	{
		public RoutingRequestFilterStripBusinessObject(RoutingManager manager)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "RoutingRequestFilterStripBusinessObject";
			this.Manager = manager;
		}

		public RoutingRequestFilterStripBusinessObject()
		{
		}

		public readonly RoutingManager Manager; // for binding

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant")]
		static class Descriptions
		{
			public const string MinimumConnectionTime = "Minimum Connection Time (hours)";
			public const string IncludeWeeklyTimetable = "Include Weekly Timetable";
			public const string ConnectionsCount = "Max. Connections";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			ModuleNkFilter originFilter = filters.AddNkFilter("Origin", GetEmptyZQuery, ModuleIDs.Location, new LocationCollection(Factory, true, false));
			originFilter.Visibility = FilterVisibility.AlwaysVisible;
			originFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|Origin", "Origin");
			originFilter.PropertyValidation += ValidateLocation;

			ModuleNkFilter destinationFilter = filters.AddNkFilter("Destination", GetEmptyZQuery, ModuleIDs.Location, new LocationCollection(Factory, true, false));
			destinationFilter.Visibility = FilterVisibility.AlwaysVisible;
			destinationFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|Destination", "Destination");
			destinationFilter.PropertyValidation += ValidateLocation;

			ModuleSingleDateFilter departureDateFilter = filters.AddSingleDateFilter("Departure Date", delegate
			{ return new ZQuery(); });
			departureDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			departureDateFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|DepartureDate", "Departure Date");

			if (departureDateFilter.Property1.IsEmpty)
			{
				departureDateFilter.Property1 = ZDateTime.Now.Date;
			}
			departureDateFilter.Property1Validation += delegate(ZPropertyInfo info)
			{ MandatoryValidation.CheckEntered(info); };

			ModuleNkFilter airlineFilter = filters.AddNkFilter("Airline", GetEmptyZQuery, ModuleIDs.RefAirline, new RefAirlineCollection(Factory));
			airlineFilter.Visibility = FilterVisibility.AlwaysVisible;
			airlineFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|Airline", "Airline");

			var codeShareInterlineFilter = filters.AddTextFilter("Code Share/Interline", value => new ZQuery(), CodeShareInterLineList);
			codeShareInterlineFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|CodeShareInterline", "Code Share/Interline");
			codeShareInterlineFilter.DefaultProperty = CodeShareInterLineConstants.SingleLineOnly;
			codeShareInterlineFilter.Visibility = FilterVisibility.AlwaysVisible;

			ModuleTextFilter flightTypeFilter = filters.AddTextFilter("Flight Type", delegate(ZString value)
			{ return new ZQuery(); }, FlightTypeList);
			flightTypeFilter.Visibility = FilterVisibility.AlwaysApplied;
			flightTypeFilter.Property = FlightTypeConstants.CargoOnly;
			flightTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|FlightType", "Flight Type");

			var equipmentTypeFilter = filters.AddTextFilter("Equipment Type", value => new ZQuery(), EquipmentTypeList);
			equipmentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|EquipmentType", "Equipment Type");
			equipmentTypeFilter.DefaultProperty = EquipmentTypeConstants.WideBodyAndFreighter;
			equipmentTypeFilter.Visibility = FilterVisibility.AlwaysVisible;

			var minimumConnectionTimeFilter = new RealTimeRoutingModuleNumberFilter(Descriptions.MinimumConnectionTime, (comparisonOperator, value) => new ZQuery());
			filters.AddFilter(minimumConnectionTimeFilter);
			minimumConnectionTimeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|MinimumConnectionTime", Descriptions.MinimumConnectionTime);
			minimumConnectionTimeFilter.PropertyValidation += ValidateMinimumConnectionTime;

			var connectionsCountFilter = new RealTimeRoutingModuleNumberFilter(Descriptions.ConnectionsCount, (comparisonOperator, value) => new ZQuery());
			filters.AddFilter(connectionsCountFilter);
			connectionsCountFilter.Visibility = FilterVisibility.AlwaysVisible;
			connectionsCountFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|RoutingRequestFilter|MaxConnections", "Max. Connections");
			connectionsCountFilter.PropertyValidation = ValidateConnectionsCount;

			return filters;
		}

		ZQuery GetEmptyZQuery(ZString value)
		{
			return new ZQuery();
		}

		void ValidateLocation(ZPropertyInfo info)
		{
			MandatoryValidation.CheckEntered(info);
			if (!info.HasErrors())
			{
				var locationType = LocationHelper.GetLocationType((ZString)info.Value);
				if (locationType == LocationHelper.LocationType.Port)
				{
					var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, (ZString)info.Value);
					ValidateUNLOCO(unloco, info, Res.GetString("Forwarding|RoutingRequestFilter|NoIATACode", "No IATA code can be found for this location."));
				}
				else if (locationType == LocationHelper.LocationType.Zone)
				{
					var zone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, (ZString)info.Value));
					if (zone == null || !zone.FZ_ZoneType.EqualsIgnoringCase(RefZoneHeaderLookups.ZoneTypeCodes.Schedules))
					{
						info.AddError(Res.GetString("Forwarding|RoutingRequestFilter|NotSchedulesZone", "International Zone type must be SCH – Schedules."));
						return;
					}

					foreach (var unloco in zone.UNLOCOs.Cast<RefUNLOCO>())
					{
						ValidateUNLOCO(unloco, info, Res.GetString("Forwarding|RoutingRequestFilter|ZonePortDoesNotLinkToIATA", "International zone contains location {0} not linked to IATA code", unloco.Code));
						if (info.HasErrors())
						{
							return;
						}
					}
				}
			}
		}

		void ValidateUNLOCO(RefUNLOCO unloco, ZPropertyInfo info, ZString message)
		{
			if (unloco == null || unloco.RL_IATA.IsEmpty)
			{
				info.AddError(message);
			}
		}

		void ValidateMinimumConnectionTime(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty && (!int.TryParse(info.Value.ToString(), out int value) || value < 0 || value > 9))
			{
				info.AddError(Res.GetString("Forwarding|RoutingRequestFilter|OnlyNumbersCanBeEnteredHereMinHoursMaxHours",
					"Only numbers can be entered here. Min 0 hours, Max 9 hours."));
			}
		}

		void ValidateConnectionsCount(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty && (!int.TryParse(info.Value.ToString(), out int value) || value < 0 || value > 3))
			{
				info.AddError(Res.GetString("Forwarding|RoutingRequestFilter|ConnectionsCountMustBeANumber",
					"Connections Count must be a number between 0 and 3."));
			}
		}

		#endregion

		#region Set Filter Defaults

		public void SetFilterDefaults(ZString origin, ZString destination, ZDateTime departureDate, ZString airlineCode)
		{
			Origin = origin;
			Destination = destination;
			DepartureDate = departureDate;
			AirlineCode = airlineCode;
		}

		#endregion

		#region Properties

		ZString Origin
		{
			get { return this["Origin"].IsActive ? ((ModuleNkFilter)this["Origin"]).Property : ZString.Empty; }
			set { ((ModuleNkFilter)this["Origin"]).Property = value; }
		}

		ZString Destination
		{
			get { return this["Destination"].IsActive ? ((ModuleNkFilter)this["Destination"]).Property : ZString.Empty; }
			set { ((ModuleNkFilter)this["Destination"]).Property = value; }
		}

		ZString AirlineCode
		{
			get { return this["Airline"].IsActive ? ((ModuleNkFilter)this["Airline"]).Property : ZString.Empty; }
			set { ((ModuleNkFilter)this["Airline"]).Property = value; }
		}

		ZDateTime DepartureDate
		{
			get { return this["Departure Date"].IsActive ? ((ModuleSingleDateFilter)this["Departure Date"]).Property1.Date : ZDateTime.Empty; }
			set { ((ModuleSingleDateFilter)this["Departure Date"]).Property1 = value.Date; }
		}

		ZInt MinimumConnectionTime
		{
			get
			{
				var minimumConnectionTimeString = this[Descriptions.MinimumConnectionTime].IsActive ? ((ModuleTextFilter)this[Descriptions.MinimumConnectionTime]).Property : ZString.Empty;

				if (int.TryParse(minimumConnectionTimeString, out int minimumConnectionTime))
				{
					return minimumConnectionTime;
				}

				return 0;
			}
		}

		ZString FlightType
		{
			get
			{
				string flightType = this["Flight Type"].IsActive ? ((ModuleTextFilter)this["Flight Type"]).Property : ZString.Empty;
				if (flightType == FlightTypeConstants.CargoOnly)
				{
					return "C";
				}
				else if (flightType == FlightTypeConstants.PassengerOnly)
				{
					return "P";
				}
				return "";
			}
		}

		ZString EquipmentType
		{
			get
			{
				string equipment = this["Equipment Type"].IsActive ? ((ModuleTextFilter)this["Equipment Type"]).Property : ZString.Empty;
				if (equipment == EquipmentTypeConstants.WideBody)
				{
					return "W";
				}
				else if (equipment == EquipmentTypeConstants.Freighter)
				{
					return "F";
				}
				else if (equipment == EquipmentTypeConstants.WideBodyAndFreighter)
				{
					return "B";
				}
				return "";
			}
		}

		ZString CodeShareOption
		{
			get
			{
				string codeShareOption = this["Code Share/Interline"].IsActive ? ((ModuleTextFilter)this["Code Share/Interline"]).Property : ZString.Empty;
				if (codeShareOption == CodeShareInterLineConstants.SingleLineAndCodeShare)
				{
					return ".";
				}
				else if (codeShareOption == CodeShareInterLineConstants.SingleLineOnly)
				{
					return "C";
				}
				else if (codeShareOption == CodeShareInterLineConstants.InterLineAndCodeShare)
				{
					return "X";
				}
				else if (codeShareOption == CodeShareInterLineConstants.InterLineOnly)
				{
					return "Y";
				}
				return "";
			}
		}

		public ZBool IncludeWeeklyTimetable => Manager.IncludeWeeklyTimetable;
		public ZBool IncludeCO2EmissionValue => ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled && FreightDataRegistry.Instance.EnableGlobalFlightSchedulesCalculation.Value;

		ZString ConnectionsCount => this[Descriptions.ConnectionsCount].IsActive
				? ((ModuleNumberFilter)this[Descriptions.ConnectionsCount]).Property
				: ZString.Empty;

		#endregion

		#region Build Request

		public IReadOnlyCollection<RoutingRequest> BuildRequests()
		{
			var requests = new List<RoutingRequest>();
			var originUNLOCOs = GetUNLOCOsFromLocation(Origin);
			var destinationUNLOCOs = GetUNLOCOsFromLocation(Destination);

			foreach (var originUNLOCO in originUNLOCOs)
			{
				foreach (var destinationUNLOCO in destinationUNLOCOs)
				{
					var request = new RoutingRequest(Factory);

					request.OriginUNLOCOCode = originUNLOCO;
					request.DestinationUNLOCOCode = destinationUNLOCO;
					request.AirlineCode = AirlineCode;
					request.DepartureDate = DepartureDate;

					request.MinimumConnectionTime = MinimumConnectionTime;
					request.CargoPassengerFlightOption = FlightType;
					request.EquipmentType = EquipmentType;
					request.CodeShareInterlineOption = CodeShareOption;
					request.IncludeWeeklyTimetable = IncludeWeeklyTimetable;
					request.IncludeCO2EmissionValue = IncludeCO2EmissionValue;
					request.ConnectionsCount = ConnectionsCount;

					requests.Add(request);
				}
			}

			return requests.AsReadOnly();
		}

		IEnumerable<ZString> GetUNLOCOsFromLocation(ZString locationCode)
		{
			var locationType = LocationHelper.GetLocationType(locationCode);
			switch (locationType)
			{
				case LocationHelper.LocationType.Port:
					return new[] { locationCode };
				case LocationHelper.LocationType.Zone:
					var zone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, locationCode));
					if (zone != null)
					{
						return zone.UNLOCOs.Cast<RefUNLOCO>().Select(unloco => unloco.RL_Code);
					}
					break;
			}

			return Enumerable.Empty<ZString>();
		}

		#endregion

		#region Lookups

		CodeDescriptionPairList CodeShareInterLineList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(CodeShareInterLineConstants.SingleLineAndCodeShare, Res.GetString("Forwarding|RoutingRequestFilter|SingleLineAndCodeShares", "Single-Line and Code Shares"));
				result.AddPair(CodeShareInterLineConstants.SingleLineOnly, Res.GetString("Forwarding|RoutingRequestFilter|SingleLineOnly", "Single-Line Only"));
				result.AddPair(CodeShareInterLineConstants.InterLineAndCodeShare, Res.GetString("Forwarding|RoutingRequestFilter|InterLineAndCodeShares", "Inter-Line and Code Shares"));
				result.AddPair(CodeShareInterLineConstants.InterLineOnly, Res.GetString("Forwarding|RoutingRequestFilter|InterLineOnly", "Inter-Line Only"));
				return result;
			}
		}

		public static class CodeShareInterLineConstants
		{
			public const string SingleLineAndCodeShare = "CDS";
			public const string SingleLineOnly = "SIN";
			public const string InterLineAndCodeShare = "INC";
			public const string InterLineOnly = "INT";
		}

		CodeDescriptionPairList FlightTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(FlightTypeConstants.AllFlightTypes, Res.GetString("Forwarding|RoutingRequestFilter|AllFlights", "All Flights"));
				result.AddPair(FlightTypeConstants.CargoOnly, Res.GetString("Forwarding|RoutingRequestFilter|CargoFlightsOnly", "Cargo Flights Only"));
				result.AddPair(FlightTypeConstants.PassengerOnly, Res.GetString("Forwarding|RoutingRequestFilter|PassengerFlightsOnly", "Passenger Flights Only"));
				return result;
			}
		}

		CodeDescriptionPairList EquipmentTypeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(EquipmentTypeConstants.AllEquipmentTypes, Res.GetString("Forwarding|RoutingRequestFilter|AllEquipmentTypes", "All Equipment Types"));
				result.AddPair(EquipmentTypeConstants.WideBody, Res.GetString("Forwarding|RoutingRequestFilter|WidebodyTypesOnly", "Widebody Types Only"));
				result.AddPair(EquipmentTypeConstants.Freighter, Res.GetString("Forwarding|RoutingRequestFilter|FreighterTypesOnly", "Freighter Types Only"));
				result.AddPair(EquipmentTypeConstants.WideBodyAndFreighter, Res.GetString("Forwarding|RoutingRequestFilter|WidebodyAndFreighterTypesOnly", "Widebody and Freighter Types Only"));
				return result;
			}
		}

		public static class EquipmentTypeConstants
		{
			public const string AllEquipmentTypes = "ALL";
			public const string WideBody = "WID";
			public const string Freighter = "FRT";
			public const string WideBodyAndFreighter = "WDF";
		}

		#endregion

		#region International Zones

		public ZBool AreOriginDestinationBothZone()
		{
			return LocationHelper.GetLocationType(Origin) == LocationHelper.LocationType.Zone && LocationHelper.GetLocationType(Destination) == LocationHelper.LocationType.Zone;
		}

		#endregion
	}
}
