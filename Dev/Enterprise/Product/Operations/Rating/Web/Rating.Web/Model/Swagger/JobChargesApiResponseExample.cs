using Swashbuckle.Examples;

namespace Enterprise.Rating.Web.Model
{
	class JobChargesApiResponseExample : IExamplesProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Api response examples")]

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
							CWCode = "123",
							SCAC = null,
							C1CCode = null,
							IATACode = "CC"
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
						StartDate = "2012-11-28",
						ExpiryDate = "",
						PayTermOverride = "",
						AircraftType = "",
						RateProvider = "CW",
						IsCWGlobal = false,
						Charges = new []
						{
							new ChargeInfo()
							{
								ChargeCode = new ChargeCodeInfo() {
									CWCode = "FRT",
									UniversalCodes = new [] { "SEC" }
								},
								ChargeGroup = "FRT",
								ChargeSubGroup = "",
								ChargeDesc = "International Freight",
								Unit = "KG",
								ConversionFactor = new ConversionFactor() {
									Factor = "6000.000",
									NumeratorUnit = "CC",
									DenominatorUnit = "KG"
								},
								Currency = "AUD",
								Calculators = new []
								{
									new CalculatorInfo() {
										CWCode = "UNT",
										IsAgentRate = false,
										Attributes = new []
										{
											new CalculatorAttribute()
											{
												Name = "PerUnit",
												Type = "Decimal",
												Value = 3.2400
											}
										}
									}
								},
								Cost = new CalculationItem() {
									Currency = "AUD",
									Amount = 1620.00M,
									LocalAmount = 1620.00M,
									LocalCurrency = "AUD",
									ExchangeRate = 1.0M,
									Audit = "FRT: 500 Kilogram(s) @ AUD 3.24/KG"
								},
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
						ServiceProvider = null,
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
						StartDate = "2020-01-10",
						ExpiryDate = "",
						PayTermOverride = "",
						AircraftType = "",
						RateProvider = "CW",
						IsCWGlobal = false,
						Charges = new []
						{
							new ChargeInfo()
							{
								ChargeCode = new ChargeCodeInfo() {
									CWCode = "2ST",
									UniversalCodes = new [] { "BAF" }
								},
								ChargeGroup = "FRT",
								ChargeSubGroup = "",
								ChargeDesc = "2nd Stop Off",
								Unit = "",
								ConversionFactor = new ConversionFactor() {
									Factor = "0",
									NumeratorUnit = null,
									DenominatorUnit = null
								},
								Currency = "AUD",
								Calculators = new []
								{
									new CalculatorInfo()
									{
										CWCode = "FLT",
										IsAgentRate = false,
										Attributes = new []
										{
											new CalculatorAttribute()
											{
												Name = "BaseRate",
												Type = "Decimal",
												Value = 102.0000
											}
										}
									}
								},
								Cost = null,
								Revenue = new CalculationItem() {
									Currency = "AUD",
									Amount = 102.00M,
									LocalAmount = 102.00M,
									LocalCurrency = "AUD",
									ExchangeRate = 1.0M,
									Audit = "2ST: Base Rate AUD 102.00"
								},
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
