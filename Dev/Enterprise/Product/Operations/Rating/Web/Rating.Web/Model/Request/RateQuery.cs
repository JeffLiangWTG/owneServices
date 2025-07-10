using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.GUI;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This object is mandatory for successfully requesting calculated job charges or rates pricing information from the endpoint
	/// </summary>

	public class RateQuery
	{
		/// <summary>
		/// To specify which sources of rates expected to be searched.
		/// This property is only usable for costing endpoint and should not be provided for other endpoints.
		/// It can be one of these values : 'CW', 'CG/CS'.
		/// </summary>
		public string[] RateProviders { get; set; }

		/// <summary>
		/// Match against the Origin for searching of rates.
		/// This property is mandatory for all endpoints.
		/// It can be a UNLOCO, Post Code or City when calling jobcharges.
		/// It can be a Country, UNLOCO, Post Code or an International Zone when calling other endpoints.
		/// </summary>
		public Location Origin { get; set; }

		/// <summary>
		/// Match against the Via for searching of CW rates.
		/// It can be a UNLOCO when calling jobcharges.
		/// It can be Country, UNLOCO or an International Zone when calling clientrates, companytariffs or costing endpoints.
		/// Via in not applicable when calling intercompanytariffs endpoint and will be ignored.
		/// </summary>
		public Location Via { get; set; }

		/// <summary>
		/// Match against the Destination for searching of rates or indicates the job Destination to calculate job charges.
		/// This property is mandatory for all end-points.
		/// It can be a UNLOCO, Post Code or City when calling jobcharges.
		/// It can be a Country, UNLOCO, Post Code or an International Zone when calling other endpoints.
		/// </summary>
		public Location Destination { get; set; }

		/// <summary>
		/// Match against "Matching Locations" for searching of rates.
		/// RelatedField must be specified with either 1LD, LDC, MLD or MDC.
		/// They can be a UNLOCO, Post Code or City when calling jobcharges.
		/// They can be a Country, UNLOCO, Post Code or an International Zone when calling other endpoints.
		/// </summary>
		public Location[] Locations { get; set; }

		/// <summary>
		/// Pickup organisation code for looking up PickupAddress to match against Consignor Pickup Address when searching rates.
		/// This property is only available for JobCharges endpoint.
		/// </summary>
		public string PickupOrg { get; set; }

		/// <summary>
		/// Pickup address code for looking up PickupAddress to match against Consignor Pickup Address when searching rates.
		/// This property is only available for JobCharges endpoint.
		/// </summary>
		public string PickupAddrCode { get; set; }

		/// <summary>
		/// Overridden Pickup city for matching against zone items in CTZ calculator when calculating rates.
		/// This property is only available for JobCharges endpoint.
		/// </summary>
		public string PickupCity { get; set; }

		/// <summary>
		/// Overridden Pickup postcode for matching against FromPostcode code when searching rates and against zone items in CTZ calculator when calculating rates.
		/// This property is only available for JobCharges endpoint.
		/// </summary>
		public string PickupPostcode { get; set; }

		/// <summary>
		/// Delivery organisation code for looking up DeliveryAddress to match against Consignee Delivery Address when searching rates.
		/// This property is only available for JobCharges endpoint.
		/// </summary>
		public string DeliveryOrg { get; set; }

		/// <summary>
		/// Delivery address code for looking up DeliveryAddress to match against Consignee Delivery Address when searching rates.
		/// This property is only available for JobCharges endpoint.
		/// </summary>
		public string DeliveryAddrCode { get; set; }

		/// <summary>
		/// Overridden Delivery city for matching against zone items in CTZ calculator when calculating rates.
		/// This property is only available for JobCharges endpoint.
		/// </summary>
		public string DeliveryCity { get; set; }

		/// <summary>
		/// Overridden Delivery postcode for matching against ToPostcode code when searching rates and against zone items in CTZ calculator when calculating rates.
		/// This property is only available for JobCharges endpoint.
		/// </summary>
		public string DeliveryPostcode { get; set; }

		/// <summary>
		/// Match against the Transport Mode for searching of rates.
		/// This property is mandatory for all endpoints.
		/// It can be one of these values : 'ALL', 'SEA', 'AIR', 'RAI' (not applicable to jobcharges), 'ROA' (not applicable to jobcharges).
		/// 'ALL' is only specific to searching rates and it can be used only with ContainerMode set to blank.
		/// If 'ALL' is specified, only rates setup with mode value of 'ALL' under CW1 > Origin/Destination Charges are returned.
		/// </summary>
		public string TransportMode { get; set; }

		/// <summary>
		/// Match against the Container Mode for searching of rates or indicates the job Container Mode to calculate job charges.
		/// It can be one of these values : 'FCL', 'LCL', 'ULD' or 'LSE'.
		/// This property is mandatory for jobcharges endpoint.
		/// ContainerMode is mandatory for searching CargoSphere or Cargoguide rates when calling costing endpoint.
		/// </summary>
		public string ContainerMode { get; set; }

		/// <summary>
		/// Match against the ServiceProvider/Carrier of CW1 Costings when searching rates using costing endpoint.
		/// Match against the Carrier of CW1 Standard Costings when searching rates using costing endpoint.
		/// Match against the Carrier of CargoGuide rates when searching rates using costing endpoint.
		/// Match against the Carrier of CargoSphere rates when searching rates using costing endpoint.
		/// Match against the Service Provider of CW1 Client Rates when searching rates using clientrates endpoint.
		/// Match against the Service Provider of CW1 Company Tariffs when searching rates using companytariffs endpoint.
		/// Match against the Service Provider of CW1 Intercompany Tariffs when searching rates using intercompanytariffs endpoint.
		/// ServiceProviders is not applicable to jobcharges endpoint.
		/// </summary>
		public Organisation[] ServiceProviders { get; set; }

		/// <summary>
		/// Match against the Carrier and PossibleCarriers when searching rates.
		/// - Carrier the first item
		/// - PossibleCarriers is starting from the second item
		///
		/// Carriers normally use for matching against Rate Entry > Carrier
		/// ServiceProviders normally use for matching against Rate (Client Rate, Costing, Company Tariff, etc.) > Client / Service Provider
		/// </summary>
		public Organisation[] Carriers { get; set; }

		/// <summary>
		/// Match against the Client for searching rates from CW Client Rates.
		/// It is mandatory when using with clientrates endpoint and is not applicable to other endpoints.
		/// Value Reference: CW1's Client Organisation Code
		/// </summary>
		public string Client { get; set; }

		/// <summary>
		/// Match against the Carrier Contract for searching of buy rates from CargoWise, CargoGuide and CargoSphere.
		/// This property is only applicable when calling costing or jobcharges endpoint.
		/// </summary>
		public string[] CarrierContracts { get; set; }

		/// <summary>
		/// Match against the Client Contract for searching of sell rates from CargoWise.
		/// This property is only applicable when calling clinetrates or jobcharges endpoint.
		/// </summary>
		public string[] ClientContracts { get; set; }

		/// <summary>
		/// Match against the Controlling Customer, Consignor, Consignee and/or Named Account for searching of rates.
		/// </summary>
		public NamedAccount[] NamedAccounts { get; set; }

		/// <summary>
		/// Match against the Carrier Service Level for searching of rates.
		/// </summary>
		public CarrierServiceLevel[] CarrierServiceLevels { get; set; }

		/// <summary>
		/// Match against the Service Level for searching of rates.
		/// Value Reference: CW > Maintain > Reference Files > Service Levels > Code.
		/// </summary>
		public string[] ServiceLevels { get; set; }

		/// <summary>
		/// Match against the Gateway Service Level for searching of rates (Intercompany Tariffs).
		/// Value Reference: CW > Maintain > Reference Files > Service Levels > Code.
		/// </summary>
		public string[] GatewayServiceLevels { get; set; }

		/// <summary>
		/// Match against the Shipment Gateway Service Level for searching of rates (Intercompany Tariffs).
		/// Value Reference: CW > Maintain > Reference Files > Service Levels > Code.
		/// </summary>
		public string[] ShipmentGatewayServiceLevels { get; set; }

		/// <summary>
		/// Match against the Container Type for searching of rates.
		/// Not applicable to jobcharges endpoint.
		/// To specify Container Type for sending request to jobcharges endpoint, please use the Container under property JobInfo.
		/// </summary>
		public ContainerType[] ContainerTypes { get; set; }

		/// <summary>
		/// Match against the Commodity for searching of rates.
		/// Not applicable to jobcharges endpoint.
		/// To specify Commodity for sending request to jobcharges endpoint, please use the Commodity under property JobInfo > Container or JobInfo > Container > PackLines.
		/// </summary>
		public CommodityInfo[] Commodities { get; set; }

		/// <summary>
		/// Compare against the Start Date and Expiry Date for searching of rates.
		/// </summary>
		public DateTimeOffset EffectiveDate { get; set; }

		/// <summary>
		/// Applicable to jobcharges endpoint only to specify the scope for calculating job charges.
		/// It can only have one of these values : 'C', 'R', 'A'.
		/// C: Calculate rates and charges from matching buy rates only.
		/// R: Calculate rates and charges from matching sell rates only.
		/// A: Calculate rates and charges from matching buy and sell rates.
		/// </summary>
		public string CalculationScope { get; set; }

		/// <summary>
		/// Provides the measurement at container/equipment level and/or packing lines level for calculating charges.
		/// Is is only applicable to jobcharges endpoint.
		/// </summary>
		public JobInfo JobInfo { get; set; }

		/// <summary>
		/// Match against the Company Tariffs Level for searching of rates per companytariffs endpoint.
		/// </summary>
		public int? CTLevel { get; set; }

		/// <summary>
		/// Match against the Planned Load for searching of CW rates. 
		/// It can be a UNLOCO when calling jobcharges.
		/// It can be a Country, UNLOCO or an International Zone when calling other endpoints.
		/// </summary>
		public Location PlannedLoad { get; set; }

		/// <summary>
		/// Match against the Planned Discharge for searching of CW rates. 
		/// It can be a UNLOCO when calling jobcharges.
		/// It can be a Country, UNLOCO or an International Zone when calling other endpoints.
		/// </summary>
		public Location PlannedDischarge { get; set; }

		/// <summary>
		/// Match against the Rate Origin for searching of CW rates. 
		/// It can be a UNLOCO when calling jobcharges.
		/// It can be a Country, UNLOCO or an International Zone when calling other endpoints.
		/// </summary>
		public Location RateOrigin { get; set; }

		/// <summary>
		/// Match against the Rate Destination for searching of CW rates. 
		/// It can be a UNLOCO when calling jobcharges.
		/// It can be a Country, UNLOCO or an International Zone when calling other endpoints.
		/// </summary>
		public Location RateDestination { get; set; }

		/// <summary>
		/// Attributes specific to CargoSphere serving as filters for searching of CargoSphere rates.
		/// </summary>
		public CSFilter CSFilter { get; set; }

		/// <summary>
		/// Attributes specific to CargoGuide serving as filters for searching of CargoGuide rates.
		/// </summary>
		public CGFilter CGFilter { get; set; }

		/// <summary>
		/// Used for searching corresponding buy and/or sell rates from CargoWise Costing, CargoSphere, Cargoguide Rates, Client Rates and Company Tariffs.
		/// This property is applicable only when calling jobcharges endpoint.
		/// </summary>
		public OrganisationRole[] RateParties { get; set; }

		/// <summary>
		/// Match against the Consol > Payment Type for Autorating Cost/Revenue per jobcharges endpoint.
		/// Value Reference: 'PPD', 'CCX'
		/// </summary>
		public string CarrierPayTerm { get; set; }

		/// <summary>
		/// Match against the Payment Term Override for searching or calculating rates from CW Client Rates, Company Tariffs and Intercompany Tariffs.
		/// Value Reference: 'PPD', 'CCX'
		/// </summary>
		public string PaymentTermOverride { get; set; }

		/// <summary>
		/// It can be used together with Direction determine whether charges of a particular Charge Group to be loaded or filtered out for jobcharges endpoint with the same behaviour/logic as if Autorating Revenue of Forwarding Shipment.
		/// </summary>
		public string Incoterm { get; set; }

		/// <summary>
		///  Match against the HBL Delivery Mode for searching or calculating rates from CW Client Rates, Company Tariffs
		///  Value Reference: ARPT/ARPT, ARPT/CFS, ARPT/DOOR, CFS/ARPT, CFS/CFS, CFS/CY, CFS/DOOR, CY/CFS, CY/CY, CY/DOOR, DOOR/ARPT, DOOR/CFS, DOOR/CY, DOOR/DOOR, DOOR/PORT, PORT/DOOR, PORT/PORT
		/// </summary>
		public string HBLDeliveryMode { get; set; }

		/// <summary>
		/// Match against the FMC Tariff ID for searching of rates.
		/// </summary>
		public string FMCTariffID { get; set; }

		/// <summary>
		/// Internal Attribute
		/// </summary>
		[JsonIgnore]
		public string CW1RateMode
		{
			get
			{
				var key = $"{TransportMode} {ContainerMode}";

				if (RateModeMapping.TryGetValue(key, out var mode))
				{
					return mode;
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// Internal Attribute
		/// </summary>
		[JsonIgnore]
		public bool IsContainerized
		{
			get
			{
				var containerizedContainerModes = new[] { Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.ULD };
				return containerizedContainerModes.Contains(ContainerMode);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Mapping strings")]
		readonly Dictionary<string, string> RateModeMapping = new Dictionary<string, string>()
		{
			{ "ALL ", Core.Constants.RateMode.ALL },
			{ "AIR ", RateEntryFilterUtility.TransportModes.AIF },
			{ "AIR ULD", Core.Constants.RateMode.ULD },
			{ "AIR LSE", Core.Constants.RateMode.LSE },
			{ "SEA ", RateEntryFilterUtility.TransportModes.SEF },
			{ "SEA FCL", Core.Constants.RateMode.FCL },
			{ "SEA LCL", Core.Constants.RateMode.LCL },
			{ "RAI ", RateEntryFilterUtility.TransportModes.RAF },
			{ "RAI FCL", Core.Constants.RateMode.FRA },
			{ "RAI LCL", Core.Constants.RateMode.LRA },
			{ "ROA ", RateEntryFilterUtility.TransportModes.ROF },
			{ "ROA FCL", Core.Constants.RateMode.FRO },
			{ "ROA LCL", Core.Constants.RateMode.LRO },
		};

		/// <summary>
		/// CalculationScopes
		/// </summary>
		public static class CalculationScopes
		{
			/// <summary>
			/// BuyRatesOnly
			/// </summary>
			public const string BuyRatesOnly = "C";

			/// <summary>
			/// SellRatesOnly
			/// </summary>
			public const string SellRatesOnly = "R";

			/// <summary>
			/// BuyAndSellRates
			/// </summary>
			public const string BuyAndSellRates = "A";

			/// <summary>
			/// All
			/// </summary>
			[ThreadSafe]
			public static readonly IReadOnlyCollection<string> All = new string[] { BuyRatesOnly, SellRatesOnly, BuyAndSellRates };
		}
	}
}
