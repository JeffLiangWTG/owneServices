namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// The attributes of the Rate response class are the same attributes used in setting up rates under Tariffs and Rates modules in CW.
	/// Attributes specific to CargoSphere or CargoGuide are provided under the CSRateInfo and CGRateInfo properties respectively.
	/// </summary>
	public class Rate
	{
		/// <summary>
		/// Transport Mode
		/// </summary>
		public string TransportMode { get; set; }

		/// <summary>
		/// Container Mode
		/// </summary>
		public string ContainerMode { get; set; }

		/// <summary>
		/// Origin
		/// </summary>
		public Location Origin { get; set; }

		/// <summary>
		/// Destination
		/// </summary>
		public Location Destination { get; set; }

		/// <summary>
		/// Locations
		/// </summary>
		public Location[] Locations { get; set; }

		/// <summary>
		/// Rate Origin applicable to CW sell rates only.
		/// </summary>
		public Location RateOrigin { get; set; }

		/// <summary>
		/// Rate Destination applicable to CW sell rates only.
		/// </summary>
		public Location RateDestination { get; set; }

		/// <summary>
		/// Planned Load applicable to CW sell rates only.
		/// </summary>
		public Location PlannedLoad { get; set; }

		/// <summary>
		/// Planned Discharge applicable to CW sell rates only.
		/// </summary>
		public Location PlannedDischarge { get; set; }

		/// <summary>
		/// Via applicable to CW rates only.
		/// </summary>
		public Location Via { get; set; }

		/// <summary>
		/// Service Provider applicable to CW rates.
		/// Carrier of CargoSphere and/or CargoGuide rates.
		/// </summary>
		public Organisation ServiceProvider { get; set; }

		/// <summary>
		/// Controlling Customer applicable to CW rates only.
		/// Value Reference: CW > Maintain > Master Data > Organisation > Code
		/// </summary>
		public string ControllingCustomer { get; set; }

		/// <summary>
		/// Consignor applicable to CW rates only.
		/// Value Reference: CW > Maintain > Master Data > Organisation > Code
		/// </summary>
		public string Consignor { get; set; }

		/// <summary>
		/// Consignee applicable to CW rates only.
		/// Value Reference: CW > Maintain > Master Data > Organisation > Code
		/// </summary>
		public string Consignee { get; set; }

		/// <summary>
		/// Carrier Contract Number appliable to the buy rates from CW Costing, CargoSphere and/or CargoGuide.
		/// </summary>
		public string CarrierContract { get; set; }

		/// <summary>
		/// Client Contract Number appliable to the CW sell rates.
		/// </summary>
		public string ClientContract { get; set; }

		/// <summary>
		/// Carrier Service Level
		/// </summary>
		public CarrierServiceLevel CarrierServiceLevel { get; set; }

		/// <summary>
		/// Service Level
		/// Value Reference: CW > Maintain > Reference Files > Service Levels > Code
		/// </summary>
		public string ServiceLevel { get; set; }

		/// <summary>
		/// Gateway Service Level
		/// Value Reference: CW > Maintain > Reference Files > Service Levels > Code
		/// </summary>
		public string GatewayServiceLevel { get; set; }

		/// <summary>
		/// Shipment Gateway Service Level
		/// Value Reference: CW > Maintain > Reference Files > Service Levels > Code
		/// </summary>
		public string ShipmentGatewayServiceLevel { get; set; }

		/// <summary>
		/// Container Type
		/// </summary>
		public ContainerType ContainerType { get; set; }

		/// <summary>
		/// Gateway Agent Type applicable to the Intercompany Tariffs rate.
		/// </summary>
		public string GWAgentType { get; set; }

		/// <summary>
		/// Commodity
		/// </summary>
		public CommodityInfo Commodity { get; set; }

		/// <summary>
		/// Transit Time
		/// </summary>
		public string TransitTime { get; set; }

		/// <summary>
		/// Frequency
		/// </summary>
		public int Frequency { get; set; }

		/// <summary>
		/// Frequency Unit
		/// </summary>
		public string FrequencyUnit { get; set; }

		/// <summary>
		/// Start Date
		/// </summary>
		public string StartDate { get; set; }

		/// <summary>
		/// Expiry Date
		/// </summary>
		public string ExpiryDate { get; set; }

		/// <summary>
		/// Payment Term Override applicable to CW sell rates and ICT rates.
		/// </summary>
		public string PayTermOverride { get; set; }

		/// <summary>
		/// Aircraft Type applicable to CW Air rates only.
		/// </summary>
		public string AircraftType { get; set; }

		/// <summary>
		/// Identify the Rate Provider or Source of the rate.
		/// Value Reference: CW(CargoWise), CGGD(CargoGuide), CGSP(CargoSphere)
		/// </summary>
		public string RateProvider { get; set; }

		/// <summary>
		/// Refer to 'Is Published' of rates setup under CW to identify whether the rates is 'Global' or 'Local' to a specific Company in CW.
		/// </summary>
		public bool IsCWGlobal { get; set; }

		/// <summary>
		/// This attribute refers to the calculation amount or pricing details of the charges applicable to the rate.
		/// </summary>
		public ChargeInfo[] Charges { get; set; }

		/// <summary>
		/// This attribute refers to the rate information that are specific and applicable to CargoSphere rates only.
		/// </summary>
		public CSRateInfo CargoSphere { get; set; }

		/// <summary>
		/// This attribute refers to the rate information that are specific and applicable to CargoGuide rates only.
		/// </summary>
		public CGRateInfo CargoGuide { get; set; }

		/// <summary>
		/// Carrier - Refers to CW > Maintain > Master Data > Organization > Organization Code
		/// </summary>
		public string Carrier { get; set; }

		/// <summary>
		/// Carrier Code - Refers to Airline Two Character code
		/// </summary>
		public string CarrierCode { get; set; }

		/// <summary>
		/// Container Class
		/// </summary>
		public string ContainerClass { get; set; }

		/// <summary>
		/// Destination Post Code
		/// </summary>
		public string DestPostCode { get; set; }

		/// <summary>
		/// Origin Post Code
		/// </summary>
		public string OriginPostCode { get; set; }

		/// <summary>
		/// Creation Source
		/// </summary>
		public string CreationSource { get; set; }

		/// <summary>
		/// Is Container Class Match
		/// </summary>
		public bool IsCntrClassMatch { get; set; }

		/// <summary>
		/// Is Cross Trade
		/// </summary>
		public bool IsCrossTrade { get; set; }

		/// <summary>
		/// Is Excluded From AutoRating
		/// </summary>
		public bool ExcludeAutorate { get; set; }

		/// <summary>
		/// Shipment Consolidation Status
		/// </summary>
		public string SHPConsolStatus { get; set; }

		/// <summary>
		/// Contract Number Linked
		/// </summary>
		public bool ContractNumberLinked { get; set; }

		/// <summary>
		/// Port Transport Address for Origin/Destination Rates
		/// </summary>
		public string PortTrpAddr { get; set; }

		/// <summary>
		/// Module where the Rate Entry is coming from
		/// </summary>
		public string RateModule { get; set; }

		/// <summary>
		/// Organisation where the Rate Entry is setup under
		/// </summary>
		public string RateParty { get; set; }

		/// <summary>
		/// HBL Delivery Mode
		/// </summary>
		public string HBLDeliveryMode { get; set; }

		/// <summary>
		/// FMC Tariff ID applicable to CargoWise sell rates.
		/// </summary>
		public string FMCTariffID { get; set; }
	}
}
