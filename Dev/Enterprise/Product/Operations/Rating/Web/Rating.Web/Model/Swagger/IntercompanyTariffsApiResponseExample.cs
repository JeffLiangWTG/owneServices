using Swashbuckle.Examples;

namespace Enterprise.Rating.Web.Model
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Api response examples")]
	class IntercompanyTariffsApiResponseExample : IExamplesProvider
	{
		public object GetExamples()
		{
			return new ApiResponse()
			{
				Rates = new[]
				{
					new Rate()
					{
						TransportMode = "AIR",
						ContainerMode = "LSE",
						Origin = new Location() {
							Type = "UNLOCO",
							Value = "AUSYD"
						},
						Destination = new Location() {
							Type = "UNLOCO",
							Value = "USLAX"
						},
						RateOrigin = null,
						RateDestination = null,
						PlannedLoad = null,
						PlannedDischarge = null,
						Via = null,
						ServiceProvider = new Organisation() {
							CWCode = "DEMORG",
							SCAC = null,
							C1CCode = null,
							IATACode = null
						},
						ControllingCustomer = null,
						Consignor = null,
						Consignee = null,
						CarrierContract = "",
						ClientContract = "",
						CarrierServiceLevel = null,
						ServiceLevel = "",
						GatewayServiceLevel = "",
						ShipmentGatewayServiceLevel = "",
						ContainerType = null,
						GWAgentType = "",
						Commodity = new CommodityInfo() {
							Type = "CW",
							Value = "GEN"
						},
						TransitTime = "",
						Frequency = 0,
						FrequencyUnit = "",
						StartDate = "2019-10-02",
						ExpiryDate = "",
						PayTermOverride = "",
						AircraftType = "",
						RateProvider = "CW",
						IsCWGlobal = true,
						Charges = new [] {
							new ChargeInfo()
							{
								ChargeCode = new ChargeCodeInfo() {
									CWCode = "FRT",
									UniversalCodes = new [] { "FRT" }
								},
								ChargeGroup = "FRT",
								ChargeSubGroup = "",
								ChargeDesc = "International Freight",
								Unit = "",
								ConversionFactor = new ConversionFactor() {
									Factor = "0",
									NumeratorUnit = null,
									DenominatorUnit = null
								},
								Currency = "AUD",
								Calculators = new [] {
									new CalculatorInfo()
									{
										CWCode = "FLT",
										IsAgentRate = false,
										Attributes = new [] {
											new CalculatorAttribute()
											{
												Name = "BaseRate",
												Type = "Decimal",
												Value = 120.0000M
											}
										}
									}
								},
								Cost = null,
								Revenue = null,
								InternalNote = "",
								IsOptional = false
							}
						},
						CargoSphere = null,
						CargoGuide = null
					},
					new Rate()
					{
						TransportMode = "AIR",
						ContainerMode = "LSE",
						Origin = new Location() {
							Type = "UNLOCO",
							Value = "AUSYD"
						},
						Destination = new Location() {
							Type = "UNLOCO",
							Value = "USLAX"
						},
						RateOrigin = null,
						RateDestination = null,
						PlannedLoad = null,
						PlannedDischarge = null,
						Via = null,
						ServiceProvider = new Organisation() {
							CWCode = "EDICUS",
							SCAC = null,
							C1CCode = null,
							IATACode = null
						},
						ControllingCustomer = null,
						Consignor = null,
						Consignee = null,
						CarrierContract = "",
						ClientContract = "",
						CarrierServiceLevel = null,
						ServiceLevel = "",
						GatewayServiceLevel = "",
						ShipmentGatewayServiceLevel = "",
						ContainerType = null,
						GWAgentType = "",
						Commodity = new CommodityInfo() {
							Type = "CW",
							Value = "GEN"
						},
						TransitTime = "",
						Frequency = 0,
						FrequencyUnit = "",
						StartDate = "2019-10-02",
						ExpiryDate = "",
						PayTermOverride = "",
						AircraftType = "",
						RateProvider = "CW",
						IsCWGlobal = true,
						Charges = new [] {
							new ChargeInfo()
							{
								ChargeCode = new ChargeCodeInfo() {
									CWCode = "FRT",
									UniversalCodes = new [] { "FRT" }
								},
								ChargeGroup = "FRT",
								ChargeSubGroup = "",
								ChargeDesc = "International Freight",
								Unit = "",
								ConversionFactor = new ConversionFactor() {
									Factor = "0",
									NumeratorUnit = null,
									DenominatorUnit = null
								},
								Currency = "AUD",
								Calculators = new [] {
									new CalculatorInfo()
									{
										CWCode = "FLT",
										IsAgentRate = false,
										Attributes = new [] {
											new CalculatorAttribute()
											{
												Name = "BaseRate",
												Type = "Decimal",
												Value = 888.0000
											}
										}
									}
								},
								Cost = null,
								Revenue = null,
								InternalNote = "",
								IsOptional = false
							}
						},
						CargoSphere = null,
						CargoGuide = null
					}
				},
				Warnings = System.Array.Empty<string>()
			};
		}
	}
}
