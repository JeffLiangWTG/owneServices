using Swashbuckle.Examples;

namespace Enterprise.Rating.Web.Model
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "API response examples")]
	class CompanyTariffsApiResponseExample : IExamplesProvider
	{
		public object GetExamples()
		{
			return new ApiResponse()
			{
				Rates = new[] {
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
						StartDate = "2022-10-05",
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
									new CalculatorInfo()
									{
										CWCode = "UNT",
										IsAgentRate = false,
										Attributes = new []
										{
											new CalculatorAttribute()
											{
												Name = "PerUnit",
												Type = "Decimal",
												Value = 12.8700M
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
