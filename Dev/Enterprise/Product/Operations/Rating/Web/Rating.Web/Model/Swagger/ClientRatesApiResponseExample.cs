using Swashbuckle.Examples;

namespace Enterprise.Rating.Web.Model
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Api response examples")]
	class ClientRatesApiResponseExample : IExamplesProvider
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
									UniversalCodes = new []
									{
										"BAF"
									}
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
												Value = 102.0000M
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
