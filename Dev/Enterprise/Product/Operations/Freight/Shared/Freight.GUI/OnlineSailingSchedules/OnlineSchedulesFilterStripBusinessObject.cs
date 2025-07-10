using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	public class OnlineSchedulesFilterStripBusinessObject : FilterStripBusinessObject
	{
		public OnlineSchedulesFilterStripBusinessObject(OnlineSchedules onlineSchedules)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "OnlineScheduleFilterStripBusinessObject";
			this.OnlineSchedules = onlineSchedules;
		}

		public OnlineSchedulesFilterStripBusinessObject()
		{
		}

		public readonly OnlineSchedules OnlineSchedules;

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant")]
		public static class Descriptions
		{
			// If new filters are added, ensure FilterDefaults and SetFilterDefaults/Visibility are extended too

			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string Departure = "Departure";
			public const string Arrival = "Arrival";
			public const string Carrier = "Carrier";
			public const string VoyageVessel = "Voyage # and Vessel";
			public const string TransitTime = "Transit Time";
			public const string LegsCount = "Max. Legs";
			public const string CarrierScac = "Carrier SCAC";
			public const string IncludeRelatedPorts = "Show Related UNLOCOs";
			public const string RoutesOptions = "Routes Options";
			public const string ServiceString = "Service String";
		}

		#endregion

		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			var originFilter = filters.AddNkFilter(Descriptions.Origin, value => new ZQuery(), ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory));
			originFilter.Visibility = FilterVisibility.AlwaysVisible;
			originFilter.Category = FilterCategories.Locations;
			originFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|Origin", "Origin");

			var destinationFilter = filters.AddNkFilter(Descriptions.Destination, value => new ZQuery(), ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory));
			destinationFilter.Visibility = FilterVisibility.AlwaysVisible;
			destinationFilter.Category = FilterCategories.Locations;
			destinationFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|Destination", "Destination");

			var departureDateFilter = new OnlineSchedulesModuleDateFilter(Descriptions.Departure, (comparisonOperator, value1, value2) => new ZQuery(), false);
			filters.AddFilter(departureDateFilter);
			departureDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			departureDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			departureDateFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|DepartureDate", "Departure Date");

			var arrivalDateFilter = new OnlineSchedulesModuleDateFilter(Descriptions.Arrival, (comparisonOperator, value1, value2) => new ZQuery(), false);
			filters.AddFilter(arrivalDateFilter);
			arrivalDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			arrivalDateFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|ArrivalDate", "Arrival Date");

			var carrierFilter = filters.AddGuidFilter(Descriptions.Carrier, ModuleIDs.Organisation, value => new ZQuery(), CarrierList);
			carrierFilter.PropertyValidation += ValidateCarrier;
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|Carrier", "Carrier");

			var voyageVesselFilter = new OnlineSchedulesVoyageVesselFilter(Descriptions.VoyageVessel, (comparisonOperator, value, k) => new ZQuery(), VesselList);
			filters.AddCustomFilter(voyageVesselFilter);
			voyageVesselFilter.Visibility = FilterVisibility.AlwaysVisible;
			voyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|VoyageAndVessel", "Voyage # and Vessel");

			var includeRelatedPortsNames = new string[] { ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|IncludeRelatedPortsName", "Show Related UNLOCOs") };
			var includeRelatedPortsFilter = filters.AddFlagsFilter(Descriptions.IncludeRelatedPorts, includeRelatedPortsNames, new GetFlagsQuery[] { value => new ZQuery() });
			includeRelatedPortsFilter.Visibility = FilterVisibility.AlwaysVisible;
			includeRelatedPortsFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|IncludeRelatedPorts", "Show Related UNLOCOs");
			includeRelatedPortsFilter.Property0 = true;

			var routesOptionsNames = new string[] { ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|SameCarrierRoutes", "Same Carrier Routes"), ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|DirectRoutesOnly", "Direct Routes Only") };
			var routesOptionsFilter = filters.AddFlagsFilter(Descriptions.RoutesOptions, routesOptionsNames, new GetFlagsQuery[] { value => new ZQuery(), value => new ZQuery() });
			routesOptionsFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|RoutesOptions", "Routes Options");

			var transitTimeFilter = new OnlineSchedulesModuleNumberFilter(Descriptions.TransitTime, (comparisonOperator, value) => new ZQuery());
			filters.AddFilter(transitTimeFilter);
			transitTimeFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|TransitTime", "Max. Transit Time(days)");
			transitTimeFilter.PropertyValidation = ValidateTransitTime;

			var legsCountFilter = new OnlineSchedulesModuleNumberFilter(Descriptions.LegsCount, (comparisonOperator, value) => new ZQuery());
			filters.AddFilter(legsCountFilter);
			legsCountFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|MaxLegs", "Max. Legs");
			legsCountFilter.PropertyValidation = ValidateLegsCount;

			var carrierScacFilter = new OnlineSchedulesModuleTextFilter(Descriptions.CarrierScac, (comparisonOperator, value) => new ZQuery());
			filters.AddFilter(carrierScacFilter);
			carrierScacFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|CarrierSCAC", "Carrier SCAC");
			carrierScacFilter.PropertyValidation = ValidateCarrierScac;

			var serviceStringFilter = new OnlineSchedulesModuleTextFilter(Descriptions.ServiceString, (comparisonOperator, value) => new ZQuery());
			filters.AddFilter(serviceStringFilter);
			serviceStringFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|OnlineSchedulesFilter|ServiceString", "Service String");

			return filters;
		}

		protected override IReadOnlyList<FilterCategory> CategorySortOrderCore
		{
			get
			{
				return new FilterCategory[]
				{
					FilterCategories.Locations,
					FilterCategories.Dates,
					FilterCategories.Organisations,
					FilterCategories.NumbersAndReferences,
					FilterCategories.StatusAndFlags,
					FilterCategories.TextSearch
				};
			}
		}

		#endregion

		#region No Custom SQL Filter

		protected override bool ShouldAddCustomSqlFilter { get { return false; } }

		#endregion

		#region Filter Defaults

		public class FilterDefaults
		{
			public ZString Origin { get; set; }
			public ZString Destination { get; set; }
			public ZDateTime Departure { get; set; }
			public ZDateTime Arrival { get; set; }
			public ZString Carrier { get; set; }
			public ZString Voyage { get; set; }
			public ZString Vessel { get; set; }
			public ZString TransitTime { get; set; }
			public ZString LegsCount { get; set; }
			public ZBool IncludeRelatedPorts { get; set; }
			public ZBool SameCarrierRoutes { get; set; }
			public ZBool DirectRoutesOnly { get; set; }
			public ZString ServiceString { get; set; }
		}

		public void SetFilterDefaults(FilterDefaults filterDefaults)
		{
			Origin = filterDefaults.Origin;
			Destination = filterDefaults.Destination;

			DepartureFrom = filterDefaults.Departure;
			DepartureTo = ZDateTime.Empty;

			ArrivalFrom = ZDateTime.Empty;
			ArrivalTo = filterDefaults.Arrival;

			VesselName = filterDefaults.Vessel;
			VoyageNumber = filterDefaults.Voyage;

			if (!filterDefaults.Carrier.IsEmpty)
			{
				var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery(ZArchitecture.Schema.OrgHeaderSchema.OH_Code, filterDefaults.Carrier));
				if (carrier != null)
				{
					var carrierFilter = this[Descriptions.Carrier] as ModuleGuidFilter;
					if (carrierFilter != null)
					{
						carrierFilter.Property = carrier.PK;
					}
				}
			}

			TransitTime = filterDefaults.TransitTime;
			LegsCount = filterDefaults.LegsCount;
			IncludeRelatedPorts = filterDefaults.IncludeRelatedPorts;
			SameCarrierRoutes = filterDefaults.SameCarrierRoutes;
			DirectRoutesOnly = filterDefaults.DirectRoutesOnly;
			ServiceString = filterDefaults.ServiceString;
		}

		public void SetFilterVisibility()
		{
			this[Descriptions.Origin].Visibility = FilterVisibility.AlwaysVisible;
			this[Descriptions.Destination].Visibility = FilterVisibility.AlwaysVisible;

			this[Descriptions.Departure].Visibility = FilterVisibility.AlwaysVisible;
			this[Descriptions.Arrival].Visibility = FilterVisibility.AlwaysVisible;

			this[Descriptions.VoyageVessel].Visibility = FilterVisibility.AlwaysVisible;
			this[Descriptions.Carrier].Visibility = FilterVisibility.AlwaysVisible;
			this[Descriptions.ServiceString].Visibility = FilterVisibility.AlwaysVisible;

			this[Descriptions.TransitTime].Visibility = FilterVisibility.AlwaysVisible;
			this[Descriptions.LegsCount].Visibility = FilterVisibility.AlwaysVisible;
			this[Descriptions.CarrierScac].Visibility = FilterVisibility.AlwaysVisible;

			this[Descriptions.RoutesOptions].Visibility = FilterVisibility.AlwaysVisible;
			this[Descriptions.IncludeRelatedPorts].Visibility = FilterVisibility.AlwaysVisible;
		}

		public void SetFilterSpecifiedDateRangeOptions()
		{
			((ModuleDateFilter)this[Descriptions.Departure]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)this[Descriptions.Arrival]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		}

		#endregion

		public OnlineSchedulesFilterRequest BuildRequest()
		{
			const string DateFormat = "yyyy-MM-dd";

			return new OnlineSchedulesFilterRequest
			{
				LoadPort = Origin,
				DischargePort = Destination,

				EtdFrom = DepartureFrom.ToString(DateFormat, CultureInfo.InvariantCulture),
				EtdTo = DepartureTo.ToString(DateFormat, CultureInfo.InvariantCulture),
				EtaFrom = ArrivalFrom.ToString(DateFormat, CultureInfo.InvariantCulture),
				EtaTo = ArrivalTo.ToString(DateFormat, CultureInfo.InvariantCulture),

				CarrierCode = CarrierCode,
				VoyageNumber = VoyageNumber,
				VesselName = VesselName,
				ImoNumber = ImoNumber,
				ServiceString = ServiceString,

				TransitTime = TransitTime,
				LegsCount = (DirectRoutesOnly ? "1" : LegsCount.ToString()),

				IncludeRelatedPorts = ((bool)IncludeRelatedPorts).ToString(),
				SameCarrierRoutes = (DirectRoutesOnly || SameCarrierRoutes).ToString()
			};
		}

		#region Filter Values

		ZString Origin
		{
			get { return this[Descriptions.Origin].IsActive ? ((ModuleNkFilter)this[Descriptions.Origin]).Property : ZString.Empty; }
			set { ((ModuleNkFilter)this[Descriptions.Origin]).Property = value; }
		}

		ZString Destination
		{
			get { return this[Descriptions.Destination].IsActive ? ((ModuleNkFilter)this[Descriptions.Destination]).Property : ZString.Empty; }
			set { ((ModuleNkFilter)this[Descriptions.Destination]).Property = value; }
		}

		ZDateTime DepartureFrom
		{
			get { return this[Descriptions.Departure].IsActive ? ((ModuleDateFilter)this[Descriptions.Departure]).Property1 : ZDateTime.Empty; }
			set { ((ModuleDateFilter)this[Descriptions.Departure]).Property1 = value; }
		}
		ZDateTime DepartureTo
		{
			get { return this[Descriptions.Departure].IsActive ? ((ModuleDateFilter)this[Descriptions.Departure]).Property2 : ZDateTime.Empty; }
			set { ((ModuleDateFilter)this[Descriptions.Departure]).Property2 = value; }
		}

		ZDateTime ArrivalFrom
		{
			get { return this[Descriptions.Arrival].IsActive ? ((ModuleDateFilter)this[Descriptions.Arrival]).Property1 : ZDateTime.Empty; }
			set { ((ModuleDateFilter)this[Descriptions.Arrival]).Property1 = value; }
		}
		ZDateTime ArrivalTo
		{
			get { return this[Descriptions.Arrival].IsActive ? ((ModuleDateFilter)this[Descriptions.Arrival]).Property2 : ZDateTime.Empty; }
			set { ((ModuleDateFilter)this[Descriptions.Arrival]).Property2 = value; }
		}

		ZString ServiceString
		{
			get { return this[Descriptions.ServiceString].IsActive ? ((ModuleTextFilter)this[Descriptions.ServiceString]).Property : ZString.Empty; }
			set { ((ModuleTextFilter)this[Descriptions.ServiceString]).Property = value; }
		}

		ZString CarrierCode => CarrierScacFilterValue == ZString.Empty
			? (Factory.Load<OrgHeader>(CarrierId)?.ShippingLineSCAC ?? ZString.Empty)
			: CarrierScacFilterValue;

		ZGuid CarrierId => this[Descriptions.Carrier].IsActive ? ((ModuleGuidFilter)this[Descriptions.Carrier]).Property : ZGuid.Empty;

		ZString CarrierScacFilterValue => this[Descriptions.CarrierScac].IsActive ? ((ModuleTextFilter)this[Descriptions.CarrierScac]).Property : ZString.Empty;

		ZString VoyageNumber
		{
			get
			{
				return this[Descriptions.VoyageVessel].IsActive ? ((OnlineSchedulesVoyageVesselFilter)this[Descriptions.VoyageVessel]).Property : ZString.Empty;
			}
			set { ((OnlineSchedulesVoyageVesselFilter)this[Descriptions.VoyageVessel]).Property = value; }
		}

		ZString VesselName
		{
			get
			{
				return this[Descriptions.VoyageVessel].IsActive ? ((OnlineSchedulesVoyageVesselFilter)this[Descriptions.VoyageVessel]).Vessel : ZString.Empty;
			}
			set { ((OnlineSchedulesVoyageVesselFilter)this[Descriptions.VoyageVessel]).Vessel = value; }
		}

		ZString ImoNumber
		{
			get
			{
				if (!VesselName.IsEmpty)
				{
					var vessel = RefVessel.LookupVesselByName(VesselName, Factory).FirstOrDefault();
					if (vessel != null)
					{
						return vessel.RV_LloydsNumber;
					}
				}

				return ZString.Empty;
			}
		}

		ZString TransitTime
		{
			get
			{
				return this[Descriptions.TransitTime].IsActive
					? ((ModuleNumberFilter)this[Descriptions.TransitTime]).Property
					: ZString.Empty;
			}
			set { ((ModuleNumberFilter)this[Descriptions.TransitTime]).Property = value; }
		}

		ZString LegsCount
		{
			get
			{
				return this[Descriptions.LegsCount].IsActive
					? ((ModuleNumberFilter)this[Descriptions.LegsCount]).Property
					: ZString.Empty;
			}
			set { ((ModuleNumberFilter)this[Descriptions.LegsCount]).Property = value; }
		}

		ZBool IncludeRelatedPorts
		{
			get { return this[Descriptions.IncludeRelatedPorts].IsActive ? ((ModuleFlagsFilter)this[Descriptions.IncludeRelatedPorts]).Property0 : ZBool.True; }
			set { ((ModuleFlagsFilter)this[Descriptions.IncludeRelatedPorts]).Property0 = value; }
		}

		ZBool SameCarrierRoutes
		{
			get { return this[Descriptions.RoutesOptions].IsActive ? ((ModuleFlagsFilter)this[Descriptions.RoutesOptions]).Property0 : ZBool.False; }
			set { ((ModuleFlagsFilter)this[Descriptions.RoutesOptions]).Property0 = value; }
		}

		ZBool DirectRoutesOnly
		{
			get { return this[Descriptions.RoutesOptions].IsActive ? ((ModuleFlagsFilter)this[Descriptions.RoutesOptions]).Property1 : ZBool.False; }
			set { ((ModuleFlagsFilter)this[Descriptions.RoutesOptions]).Property1 = value; }
		}

		#endregion

		public bool AreCarrierAndCarrierScacBothSelected => CarrierScacFilterValue != ZString.Empty && CarrierId != ZGuid.Empty;

		#region Lookups

		OrgHeaderCollection CarrierList
		{
			get { return carrierList ?? (carrierList = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection carrierList;

		RefVesselCollection VesselList
		{
			get { return vesselList ?? (vesselList = new RefVesselCollection(Factory)); }
		}
		RefVesselCollection vesselList;

		#endregion

		#region Validation

		void ValidateTransitTime(ZPropertyInfo info)
		{
			int value;
			if (!info.Value.IsEmpty && (!int.TryParse(info.Value.ToString(), out value) || value < 0))
			{
				info.AddError(Res.GetString("Freight|OnlineSchedulesFilter|TransitTimeMustBeANumber", "Transit Time must be a number greater than or equal to zero."));
			}
		}

		void ValidateLegsCount(ZPropertyInfo info)
		{
			int value;
			if (!info.Value.IsEmpty && (!int.TryParse(info.Value.ToString(), out value) || value <= 0))
			{
				info.AddError(Res.GetString("Freight|OnlineSchedulesFilter|LegsCountMustBeANumber", "Legs Count must be a number greater than zero."));
			}
		}

		void ValidateCarrier(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty && string.IsNullOrEmpty(CarrierCode))
			{
				info.AddError(Res.GetString("Freight|OnlineSchedulesFilter|CarrierDoesNotHaveSCAC", "Carrier doesn't have SCAC."));
			}
		}

		void ValidateCarrierScac(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty && info.Value.ToString().Length != 4)
			{
				info.AddError(Res.GetString("Freight|OnlineSchedulesFilter|CarrierSCACMustContainOnly4Symbols", "Carrier SCAC must consist of 4 characters."));
			}
		}

		#endregion
	}
}
