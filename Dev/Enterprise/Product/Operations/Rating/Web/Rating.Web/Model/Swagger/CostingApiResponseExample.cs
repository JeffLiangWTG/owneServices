using Swashbuckle.Examples;

namespace Enterprise.Rating.Web.Model
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Api response examples")]
	class CostingApiResponseExample : IExamplesProvider
	{
		public object GetExamples()
		{
			var result = new ApiResponse()
			{
				Warnings = new string[] {
					"Rates Service: No active Carrier is assigned with IATA Code 'QR'",
					"An entry returned from 'Rate Service' could not be considered as a valid rate. Reason: No single Carrier is assigned with the SCAC, IATA or C1C Code of the Carrier from Rates Service:\r\n{\r\n  \"Code= \"QR\",\r\n  \"IATACode= \"QR\"\r\n}\r\nCategory cannot be identified as the Charge(s) under the Rate cannot be converted into CW1 Charges. Please check validation errors of the Rate line.\r\nNo Charge Code is assigned with or has the same Code as Universal 'AWB'.\r\nNo Charge Code is assigned with or has the same Code as Universal 'FHL'.\r\nNo Charge Code is assigned with or has the same Code as Universal 'FWB'.\r\nNo Charge Code is assigned with or has the same Code as Universal 'FHL'.\r\nNo Charge Code is assigned with or has the same Code as Universal 'AWB1'.\r\nNo Charge Code is assigned with or has the same Code as Universal 'CCA'.\r\nNo Charge Code is assigned with or has the same Code as Universal 'SSC'.\r\n"
				},
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
									CWCode = null,
							SCAC = null,
							C1CCode = null,
							IATACode = "W1"
						},
						ControllingCustomer = null,
						Consignor = null,
						Consignee = null,
						CarrierContract = "",
						ClientContract = "",
						CarrierServiceLevel = new CarrierServiceLevel() {
							Type = "UC",
							Value = "EXP"
						},
						ServiceLevel = "",
						GatewayServiceLevel = "",
						ShipmentGatewayServiceLevel = "",
						ContainerType = null,
						GWAgentType = "",
						Commodity = new CommodityInfo() {
							Type = "UCG",
							Value = "GENL"
						},
						TransitTime = "",
						Frequency = 0,
						FrequencyUnit = "",
						StartDate = "2019-10-24",
						ExpiryDate = "",
						PayTermOverride = "",
						AircraftType = "",
						RateProvider = "CGGD",
						IsCWGlobal = false,
						Charges = new [] {
							new ChargeInfo()
							{
								ChargeCode = new ChargeCodeInfo() {
									CWCode = "CDDTN",
									UniversalCodes = new [] { "FRT" }
								},
								ChargeGroup = "FRT",
								ChargeSubGroup = "",
								ChargeDesc = "Consol Destination Detention Charge",
								Unit = "KG",
								ConversionFactor = new ConversionFactor() {
									Factor = "6000.0",
									NumeratorUnit = "CC",
									DenominatorUnit = "KG"
								},
								Currency = "USD",
								Calculators = new [] {
									new CalculatorInfo()
									{
										CWCode = "CMB",
										IsAgentRate = false,
										Attributes = new []
										{
											new CalculatorAttribute()
											{
												Name = "MultipleEquipmentsOverMaxWeightVolume",
												Type = "Boolean",
												Value = false
											},
											new CalculatorAttribute()
											{
												Name = "UseInclusiveBreaks",
												Type = "Boolean",
												Value = false
											},
											new CalculatorAttribute()
											{
												Name = "UseHigherChargeableLowerRateRule",
												Type = "Boolean",
												Value = true
											},
											new CalculatorAttribute()
											{
												Name = "IsAccumulated",
												Type = "Boolean",
												Value = false
											},
											new CalculatorAttribute()
											{
												Name = "BreaksPer",
												Type = "String",
												Value = ""
											},
											new CalculatorAttribute()
											{
												Name = "Breaks",
												Type = "CalculatorBreakItem[]",
												Value = new []
												{
													new CalculatorBreakItem()
													{
														CWTransportZone = null,
														Operator = "MIN",
														Break = null,
														UnitPrice = 200.0000M,
														FlatAmount = null,
														BreakMinimum = null,
														UnitMultiple = null,
														Restricted = false,
														Text = "",
														Units = ""
													},
													new CalculatorBreakItem()
													{
														CWTransportZone = null,
														Operator = "UNT",
														Break = null,
														UnitPrice = 2.0000M,
														FlatAmount = null,
														BreakMinimum = null,
														UnitMultiple = null,
														Restricted = false,
														Text = "",
														Units = ""
													}
												}
											}
										}
									}
								},
								Cost = null,
								Revenue = null,
								InternalNote = null,
								IsOptional = false
							},
							new ChargeInfo()
							{
								ChargeCode = new ChargeCodeInfo() {
									CWCode = null,
									UniversalCodes = new [] { "CAG" }
								},
								ChargeGroup = null,
								ChargeSubGroup = null,
								ChargeDesc = null,
								Unit = "CN",
								ConversionFactor = new ConversionFactor() {
										Factor = "6000",
									NumeratorUnit = "CC",
									DenominatorUnit = "KG"
								},
								Currency = "USD",
								Calculators = new []
								{
									new CalculatorInfo()
									{
										CWCode = "CMB",
										IsAgentRate = false,
										Attributes = new []
										{
											new CalculatorAttribute()
											{
												Name = "MultipleEquipmentsOverMaxWeightVolume",
												Type = "Boolean",
												Value = true
											},
											new CalculatorAttribute()
											{
												Name = "UseInclusiveBreaks",
												Type = "Boolean",
												Value = false
											},
											new CalculatorAttribute()
											{
												Name = "UseHigherChargeableLowerRateRule",
												Type = "Boolean",
												Value = true
											},
											new CalculatorAttribute()
											{
												Name = "IsAccumulated",
												Type = "Boolean",
												Value = false
											},
											new CalculatorAttribute()
											{
												Name = "BreaksPer",
												Type = "String",
												Value = ""
											},
											new CalculatorAttribute()
											{
												Name = "Breaks",
												Type = "CalculatorBreakItem[]",
												Value = new []
												{
													new CalculatorBreakItem()
													{
														CWTransportZone = null,
														Operator = "MIN",
														Break = null,
														UnitPrice = 1000.0000M,
														FlatAmount = null,
														BreakMinimum = null,
														UnitMultiple = null,
														Restricted = false,
														Text = "",
														Units = ""
													},
													new CalculatorBreakItem()
													{
														CWTransportZone = null,
														Operator = "UNT",
														Break = null,
														UnitPrice = 700.0000M,
														FlatAmount = null,
														BreakMinimum = null,
														UnitMultiple = null,
														Restricted = false,
														Text = "",
														Units = ""
													}
												}
											}
										}
									}
								},
								Cost = null,
								Revenue = null,
								InternalNote = null,
								IsOptional = true
							},
							new ChargeInfo()
							{
								ChargeCode = new ChargeCodeInfo() {
									CWCode = null,
									UniversalCodes = new [] { "CIF" }
								},
								ChargeGroup = null,
								ChargeSubGroup = null,
								ChargeDesc = null,
								Unit = "CN",
								ConversionFactor = new ConversionFactor() {
									Factor = "6000",
									NumeratorUnit = "CC",
									DenominatorUnit = "KG"
								},
								Currency = "USD",
								Calculators = new []
								{
									new CalculatorInfo()
									{
										CWCode = "CMB",
										IsAgentRate = false,
										Attributes = new []
										{
											new CalculatorAttribute()
											{
												Name = "MultipleEquipmentsOverMaxWeightVolume",
												Type = "Boolean",
												Value = true
											},
											new CalculatorAttribute()
											{
												Name = "UseInclusiveBreaks",
												Type = "Boolean",
												Value = false
											},
											new CalculatorAttribute()
											{
												Name = "UseHigherChargeableLowerRateRule",
												Type = "Boolean",
												Value = true
											},
											new CalculatorAttribute()
											{
												Name = "IsAccumulated",
												Type = "Boolean",
												Value = false
											},
											new CalculatorAttribute()
											{
												Name = "BreaksPer",
												Type = "String",
												Value = ""
											},
											new CalculatorAttribute()
											{
												Name = "Breaks",
												Type = "CalculatorBreakItem[]",
												Value = new []
												{
													new CalculatorBreakItem()
													{
														CWTransportZone = null,
														Operator = "MIN",
														Break = null,
														UnitPrice = 100.0000M,
														FlatAmount = null,
														BreakMinimum = null,
														UnitMultiple = null,
														Restricted = false,
														Text = "",
														Units = ""
													},
													new CalculatorBreakItem()
													{
														CWTransportZone = null,
														Operator = "UNT",
														Break = null,
														UnitPrice = 50.0000M,
														FlatAmount = null,
														BreakMinimum = null,
														UnitMultiple = null,
														Restricted = false,
														Text = "",
														Units = ""
													}
												}
											}
										}
									}
								},
								Cost = null,
								Revenue = null,
								InternalNote = null,
								IsOptional = true
							},
						},
						CargoSphere = null,
						CargoGuide = new CGRateInfo() {
							IssueDate = "2019-10-24",
							PaymentTerm = "PPD",
							Origin = "AUSYD",
							OriginName = "Sydney International Airport",
							OriginCity = "AUSYD",
							OriginCityName = "Sydney",
							POL = null,
							Destination = "USLAX",
							DestinationName = "Los Angeles International Airport",
							DestinationCity = "USLAX",
							DestinationCityName = "Los Angeles",
							POD = null,
							Deck = "All",
							Ratio = "1:6",
							Remarks = "Base < Min-MAX Flat",
							RateClass = "6",
							Reference = "Test 81 - Base",
							Via = null,
							ProductId = "366cb627-29ac-4b70-b522-7ac50fc28731",
							ProductCode = "GCE",
							ProductName = "Express Cargo",
							ProductClass = "General",
							ProductDeck = "All",
							GSAName = null,
							NamedAccounts = System.Array.Empty<string>(),
							CargoAircraftOnly = false,
							TemperatureRange = null
						}
					}
				}
			};

			return result;
		}
	}
}
