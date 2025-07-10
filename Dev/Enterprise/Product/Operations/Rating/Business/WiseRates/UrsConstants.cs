namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Constants found in TradeServiceDto class, a Universal Rates Service rate.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Should not be translated")]
	public static class UrsConstants
	{
		public const string None = "None";

		/// <summary>
		/// ModeOfTransportDto codes.
		/// </summary>
		public static class ModeOfTransport
		{
			public const string Ocean = "Ocean";
			public const string Air = "Air";
			public const string Road = "Road";
			public const string Rail = "Rail";
			public const string ShortSea = "ShortSea";
			public const string InlandNavigation = "InlandNavigation";
		}

		/// <summary>
		/// TradeServiceDto.Type possible values.
		/// </summary>
		public static class ComposedTradeServiceType
		{
			public const string Direct = "Direct";
			public const string ComposedSegment = "ComposedSegment";
			public const string ComposedInland = "ComposedInland";
		}

		/// <summary>
		/// GeoScopeEntryDto.WaypointType possible values.
		/// Comes from enum RouteWaypointCode.
		/// https://devops.wisetechglobal.com/wtg/RatesService/_git/NativeRates?path=%2Fsrc%2FUniversalRateService.Domain%2FEnums%2FRouteWaypointCode.cs&version=GBmaster
		/// </summary>
		public static class RouteWaypointCode
		{
			public const string Origin = "Org";
			public const string OriginDoorOrPickup = "Odr";
			public const string PortOfLoading = "Pol";
			public const string Via = "Via";
			public const string Via2 = "Via2";
			public const string Destination = "Dst";
			public const string PortOfDischarge = "Pod";
			public const string DestinationDoorOrDelivery = "Ddr";
			public const string PublicationScope = "Pub";
		}

		/// <summary>
		/// LocationDto.FunctionCode possible values. From enum LocationFunctionCode.
		/// </summary>
		public static class LocationFunctionCode
		{
			public const string TownOrCity = "Twn";
			public const string Airport = "Apt";
			public const string Seaport = "Sea";
			public const string RailwayTerminal = "Rlt";
			public const string ContainerTerminal = "Cnt";
			public const string InlandPort = "Ipt";
			public const string IndustrialZone = "Ind";
			public const string BusTerminal = "Bus";
			public const string Island = "Isl";
			public const string Heliport = "Hpt";
			public const string DryPort = "Dry";
			public const string AdministrativeZone = "Adm";
			public const string Other = "Oth";
		}

		/// <summary>
		/// TradeServiceDto.ServiceClassCode possible values. From enum ServiceClassCode
		/// </summary>
		public static class ServiceClassCode
		{
			public const string Market = "Mkt";
			public const string Promotional = "Pro";
			public const string Adhoc = "Adh";
			public const string Contract = "Cnt";
			public const string Gateway = "Gtw";
			public const string Custom = "Cst";
			public const string NamedAccount = "Nac";
			public const string TACT = "Tct";
			public const string PreCarriage = "Pre";
			public const string OnCarriage = "Onc";
			public const string Selling = "Sel";
			public const string PriceList = "Prc";
			public const string SourceReferenceRate = "Src";
			public const string UserRate = "Usr";
			public const string SharedContractRate = "Shr";
		}

		public static class PaymentTermCode
		{
			public const string Collect = "Collect";
			public const string Prepaid = "Prepaid";
		}

		/// <summary>
		/// Values for IUniversalRateEntryDto.BreakType.
		/// From enum RateBreakTypeCode.
		/// </summary>
		public static class RateBreakTypeCode
		{
			/// <summary>
			/// Flat or normal
			/// </summary>
			public const string Flat = "Flat";
			/// <summary>
			/// Base
			/// Normally in combination with another rate
			/// </summary>
			public const string Base = "Base";
			/// <summary>
			/// Minimum price
			/// Only valid in combination with other rate components
			/// </summary>
			public const string Min = "Min";
			/// <summary>
			/// Break less. Unit price under given break quantity
			/// </summary>
			public const string BreakLess = "BrkMin";
			/// <summary>
			/// Break plus. Unit price above and including given break quantity
			/// </summary>
			public const string BreakPlus = "BrkPlus";
			/// <summary>
			/// Maximum
			/// Only valid in combination with other rate components
			/// </summary>
			public const string Max = "Max";
			/// <summary>
			/// Range or tier rate
			/// </summary>
			public const string Range = "Rng";
			/// <summary>
			/// Progressive break plus
			/// </summary>
			public const string BreakPlusProgressive = "BrkPlusPro";
			/// <summary>
			/// Pivot break in combination with quantity unit type Weight
			/// </summary>
			public const string Pivot = "Pivot";
		}

		/// <summary>
		/// Values for IUniversalRateEntryDto.Applicable.
		/// From enum RateApplicableCode.
		/// </summary>
		public static class RateApplicableCode
		{
			/// <summary>
			/// Unit price
			/// </summary>
			public const string UnitPrice = "Upr";
			/// <summary>
			/// Percentage
			/// </summary>
			public const string Percentage = "Pct";
			/// <summary>
			/// On request
			/// </summary>
			public const string OnRequest = "OR";
			/// <summary>
			/// All in
			/// </summary>
			public const string AllIn = "All";
			/// <summary>
			/// IATA Reference rate
			/// </summary>
			public const string Iata = "Iata";
			/// <summary>
			/// TACT Reference rate
			/// </summary>
			public const string Tact = "Tact";
			/// <summary>
			/// Not applicable
			/// </summary>
			public const string NotApplicable = "NA";
			/// <summary>
			/// Nihil, almost nothing
			/// </summary>
			public const string Nihil = "Nhl";
			/// <summary>
			/// Intended zero value
			/// </summary>
			public const string Zero = "Zero";
			/// <summary>
			/// Valid at time of shipping. The final charge is determined the moment the goods are loaded onto the vessel/aircraft. It can’t be calculated.
			/// </summary>
			public const string Vatos = "Vatos";
			/// <summary>
			/// Fixed
			/// </summary>
			public const string Fixed = "Fix";
			/// <summary>
			/// Free of charge
			/// </summary>
			public const string Free = "Free";

			public static class Description
			{
				public const string OnRequest = "OR – On Request at Carrier";
				public const string Iata = "IATA – IATA Reference rate";
				public const string Tact = "TACT – TACT Reference rate";
				public const string NotApplicable = "Not Applicable – Break NA in certain scenarios";
				public const string Nihil = "Nihil – Insignificant in value";
				public const string Zero = "Zero – costs nothing";
				public const string Vatos = "VATOS – Final charge to be determined";
			}

			public static class DescriptionInC3
			{
				public const string OnRequest = "OR – On Request at Carrier";
				public const string NotApplicable = "Not Applicable – Not applicable charge";
				public const string Vatos = "VATOS – Valid at time of shipment";
			}

			public static string GetDescription(string code)
			{
				return code switch
				{
					OnRequest => Description.OnRequest,
					Iata => Description.Iata,
					Tact => Description.Tact,
					NotApplicable => Description.NotApplicable,
					Nihil => Description.Nihil,
					Zero => Description.Zero,
					Vatos => Description.Vatos,
					_ => null
				};
			}

			public static string GetC3Description(string code)
			{
				return code switch
				{
					OnRequest => DescriptionInC3.OnRequest,
					NotApplicable => DescriptionInC3.NotApplicable,
					Vatos => DescriptionInC3.Vatos,
					_ => null
				};
			}
		}

		/// <summary>
		/// Values for IUniversalRateEntryDto.PricingQuantityUnit
		/// From enum UnitOfMeasurementCode.
		/// </summary>
		public static class UnitOfMeasurementCode
		{
			/// <summary>
			/// Unit not applicable
			/// </summary>
			public const string None = "None";
			public const string Gram = "G";
			public const string MetricTon = "Mt";
			public const string Pound = "Lb";
			public const string Ston = "Ston";
			public const string LongTonUS = "Lton";
			public const string Kilogram = "Kg";
			public const string KilogramVerbose = "Kilogram";
			public const string Ounce = "Oz";
			public const string Day = "Day";
			public const string Hour = "Hour";
			public const string Hawb = "Hawb";
			public const string Mawb = "Mawb";
			public const string Container = "Cnt";
			public const string Package = "Pkg";
			public const string UNnumber = "Un";
			public const string Teu = "Teu";
			public const string Object = "Object";
			public const string Shipment = "Shipment";
			public const string Km = "Km";
			public const string Inch = "In";
			public const string Foot = "Ft";
			public const string Mile = "Mi";
			public const string Meter = "M";
			public const string Centimeter = "Cm";
			public const string Millimeter = "Mm";
			public const string CubicMeter = "Cbm";
			public const string CubicMeterVerbose = "CubicMeter";
			public const string CubicCentimeter = "Ccm";
			public const string CubicMillimeter = "Cmm";
			public const string CubicFoot = "CuFt";
			public const string CubicInch = "CuIn";
			public const string Percentage = "Pct";
			public const string Week = "Week";
			public const string Fortnight = "Fortnight";
			public const string Month = "Month";
			public const string Year = "Year";
			public const string Celsius = "C";
			public const string Fahrenheit = "F";
			public const string BillOfLading = "Bol";
			public const string Case = "Case";
			public const string Document = "Document";
			public const string Declaration = "Declaration";
			public const string HsCode = "HsCode";
			public const string DangerousPackage = "DgrPackage";
			public const string ShipmentValue = "ShipmentValue";
			public const string WorkingDay = "WorkingDay";
			/// <summary>
			/// Day of Interchange + working day
			/// </summary>
			public const string DoIPlusWorkingDay = "DoIPlusWorkingDay";
		}

		/// <summary>
		/// Values for ShippingPhaseDt.Code.
		/// From enum ShippingPhaseCode.
		/// </summary>
		public static class ShippingPhaseCode
		{
			public const string MainCarriage = "Main";
			/// <summary>
			/// Origin door/ pickup location
			/// </summary>
			public const string OriginDoor = "Odr";
			public const string PreCarriageInland = "Pre";
			public const string PortfLoading = "Pol";
			public const string PortOfDischarge = "Pod";
			/// <summary>
			/// Destination door/ delivery location
			/// </summary>
			public const string DestinationDoor = "Ddr";
			public const string OnCarriageInland = "Onc";
			/// <summary>
			/// Freight phase without specific main pre or on carriage
			/// </summary>
			public const string Freight = "Frt";
		}

		public static class TaggedValueKey
		{
			/// <summary>
			/// Rate type 1
			/// </summary>
			public const string RateType1 = "RateType1";

			/// <summary>
			/// Rate type 2
			/// </summary>
			public const string RateType2 = "RateType2";

			/// <summary>
			/// Rate type 3
			/// </summary>
			public const string RateType3 = "RateType3";

			/// <summary>
			/// Arbitrary indicator
			/// </summary>
			public const string ArbitraryIndicator = "ArbitraryIndicator";

			/// <summary>
			/// CS property to store the shipping phase/segment. Alternative usage is the RouteInfo
			/// </summary>
			public const string RouteSequence = "RouteSequence";

			/// <summary>
			/// CS property to store the update sequence of commodity related charges?
			/// </summary>
			public const string CommoditySequence = "CommoditySequence";
		}

		/// <summary>
		/// Categorize the charges by usability.
		/// From enum ChargeUsabilityGroupCode
		/// Values for IBaseChargeDto.ChargeDefinition.UsabilityGroup.
		/// </summary>
		public static class ChargeUsabilityGroupCode
		{
			/// <summary>
			/// Standard undefined charge, fee or
			/// </summary>
			public const string None = "None";

			/// <summary>
			/// Service charge. For instance add hangers in container, guards for security
			/// </summary>
			public const string Srv = "Srv";

			/// <summary>
			/// Product surcharge paid on top of the base freight costs
			/// </summary>
			public const string Pds = "Pds";

			/// <summary>
			/// Taxation costs
			/// </summary>
			public const string Tax = "Tax";

			/// <summary>
			/// Accessorial fees. For instance fuel and security related charges. Often these fees are mandatory
			/// </summary>
			public const string Acc = "Acc";

			/// <summary>
			/// Charge is applicable whether an event occurs. For instance Broken Seal needs or Bol amendment
			/// </summary>
			public const string Evt = "Evt";

			/// <summary>
			/// Additional charges that depend on shipment details like length or weight per piece
			/// </summary>
			public const string Dim = "Dim";

			/// <summary>
			/// Penalty charges 
			/// </summary>
			public const string Pen = "Pen";

			/// <summary>
			/// Discount or reimbursement
			/// 
			/// </summary>
			public const string Disc = "Disc";

			/// <summary>
			/// Storage charges like demurrage, detention and other related
			/// </summary>
			public const string Storage = "Storage";
		}

		/// <summary>
		/// From enum ScheduleTransportValue.
		/// </summary>
		public static class ScheduleTransportValue
		{
			public const string FlagCode = "FlagCode";
			public const string FlagName = "FlagName";
			public const string ImoNumber = "ImoNumber";
			public const string RouteCode = "RouteCode";
			public const string VesselName = "VesselName";
			public const string VoyageNumber = "VoyageNumber";
			public const string FlightNumber = "FlightNumber";
			public const string ServiceCode = "ServiceCode";
			public const string ServiceName = "ServiceName";
			public const string TradeLane = "TradeLane";
		}

		public static class UrsPenaltyType
		{
			public const string Storage = "STO";
			public const string Demurrage = "DEM";
			public const string Detention = "DET";
		}

		/// <summary>
		/// From enum ProductClassCode.
		/// </summary>
		public static class ProductClassCode
		{
			/// <summary>
			/// None
			/// </summary>
			public const string None = "None";
			/// <summary>
			/// Live animals
			/// </summary>
			public const string Avi = "Avi";
			/// <summary>
			/// Dangerous goods
			/// </summary>
			public const string Dgr = "Dgr";
			/// <summary>
			/// General cargo
			/// </summary>
			public const string Gen = "Gen";
			/// <summary>
			/// Valuable goods
			/// </summary>
			public const string Val = "Val";
			/// <summary>
			/// Domestic
			/// </summary>
			public const string Dom = "Dom";
			/// <summary>
			/// Automotive and vehicles
			/// </summary>
			public const string Av = "Av";
			/// <summary>
			/// Vulnerable
			/// </summary>
			public const string Vun = "Vun";
			/// <summary>
			/// Diplomatic
			/// </summary>
			public const string Dip = "Dip";
			/// <summary>
			/// Small package
			/// </summary>
			public const string Spg = "Spg";
			/// <summary>
			/// Personal effects
			/// </summary>
			public const string Pef = "Pef";
			/// <summary>
			/// Human remains
			/// </summary>
			public const string Hum = "Hum";
			/// <summary>
			/// Pharmacy
			/// </summary>
			public const string Pha = "Pha";
			/// <summary>
			/// Aerospace
			/// </summary>
			public const string Aero = "Aero";
			/// <summary>
			/// Temperature control
			/// </summary>
			public const string Tem = "Tem";
			/// <summary>
			/// Other commodity specified cargo
			/// </summary>
			public const string Com = "Com";
			/// <summary>
			/// Perishables
			/// </summary>
			public const string Per = "Per";
		}

		public static class CommodityCategory
		{
			public const string Hazardous = "Hazardous";
			public const string NonHazardous = "Non-hazardous";
		}
	}
}
