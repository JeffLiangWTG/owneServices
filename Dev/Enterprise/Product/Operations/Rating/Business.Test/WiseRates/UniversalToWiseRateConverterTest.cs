using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Business.WiseRates;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Schedules;
using Urs.Api.Integration.Interfaces;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Rating.Business.UrsConstants;

namespace Enterprise.Rating.Business.Test
{
	public class UniversalToWiseRateConverterTest : RatingTestCase
	{
		public void TestConvertAirFcl()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			var actual = result.First();

			AssertEquals("UAIEV", actual.Origin);
			AssertEquals("AEDXB", actual.Via);
			AssertEquals("AUSYD", actual.Destination);
			AssertEquals("RAC", actual.Carrier);

			var expectedCommodityInfo = new CarrierSpecificCommodity()
			{
				Code = "PRD",
				GroupName = "Product X",
				GroupType = CommodityCategory.NonHazardous,
				IncludedCommodities = Enumerable.Empty<string>()
			};
			AssertEquals(expectedCommodityInfo, actual.CarrierCommodityInfo);

			AssertNullOrEmpty(actual.ContractNumber);
			AssertEquals("STD", actual.ServiceLevel);
			AssertEquals(WRConstants.TransportModes.AIR, actual.TransportMode);
			AssertEquals(utcToday.AddMonths(-5), actual.StartDate);
			AssertEquals(utcToday.AddMonths(5), actual.ExpiryDate);
			AssertEquals(new DateTime(2024, 12, 12), actual.IssueDate);
			AssertEquals("HAZD", actual.Commodity);

			AssertEquals("22G0", actual.Container.Code);
			AssertEquals("22G0", actual.Container.ISOType);

			AssertEquals(WRConstants.ContainerModes.FCL, actual.ContainerMode);
			AssertEquals("URS", actual.Provider);

			var expected = new Charge[]
			{
				new Charge()
				{
					ChargeCode = "FRT",
					FlatRate = null,
					PerUnitRate = 1000m,
					Unit = QuantityUnit.CN,
					Currency = "USD",
				}
			};

			AssertChargesEqual(
				expected,
				actual.Charges,
				c => new { c.ChargeCode, c.FlatRate, c.PerUnitRate, c.Unit, c.Currency }
			);
		}

		public void TestConvert_InclusiveCharges()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			((RateCollectionDataDto)tradelane.PriceInfo.BaseRates).Inclusive = new[]
			{
				new ChargeDefinitionDto
				{
					Code = "FSCLocal",
					UniversalCode = "FSC",
					Description = "Fuel Surcharge"
				}
			};

			SetOtherChargesAsOriginFlat(tradelane, "ABC", 490m);
			((RateCollectionDataDto)tradelane.PriceInfo.Charges.Items.First().RateCollections).Inclusive = new[] { new ChargeDefinitionDto
			{
				Code = "EEELocal",
				UniversalCode = "EEE",
				Description = "Other inclusive charge"
			} };

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			var expected = new Charge[]
			{
				new Charge()
				{
					ChargeCode = "FRT",
					FlatRate = null,
					PerUnitRate = 1000m,
					Unit = QuantityUnit.CN,
					Currency = "USD",
					ChargeType = ChargeType.None,
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FRT",
						Description = "Freight"
					}
				},
				new Charge()
				{
					ChargeCode = "FSC",
					FlatRate = null,
					PerUnitRate = null,
					Unit = null,
					Currency = null,
					ChargeType = ChargeType.Included,
					FreightInclusiveCarriageCharge = "FRT",
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FSCLocal",
						Description = "Fuel Surcharge",
					}
				},
				new Charge
				{
					ChargeCode = "ABC",
					FlatRate = 490m,
					PerUnitRate = null,
					Unit = null,
					Currency = "USD",
					ChargeType = ChargeType.SubjectTo,
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "ABCLocal",
					}
				},
				new Charge
				{
					ChargeCode = "EEE",
					FlatRate = null,
					PerUnitRate = null,
					Unit = null,
					Currency = null,
					ChargeType = ChargeType.Included,
					FreightInclusiveCarriageCharge = "ABC",
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "EEELocal",
						Description = "Other inclusive charge",
					}
				}
			};

			AssertEquals(1, result.Count);
			var actual = result.First();
			AssertChargesEqual(
				expected,
				actual.Charges,
				charge => new
				{
					charge.ChargeCode,
					charge.FlatRate,
					charge.PerUnitRate,
					charge.Unit,
					charge.Currency,
					charge.FreightInclusiveCarriageCharge,
					charge.CarrierChargeCodeInfo.Code
				}
			);
		}

		public void TestConvert_VatosCharge_ShouldBeConvertedAsSubjectToCharge()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			SetOtherChargesAsOriginFlat(tradelane, "123", 0m);

			var rateCollection = (RateCollectionDataDto)tradelane.PriceInfo.Charges.Items.First().RateCollections;

			rateCollection.Items = new[]
			{
				new RateCollectionDto
				{
					Currency = "USD",
					PriceEntries = new[]
					{
						new UniversalRateEntryDto {
							Applicable = RateApplicableCode.Vatos,
							Price = 100m,
							BreakQuantity = 1,
							QuantityUnit = UrsConstants.UnitOfMeasurementCode.Shipment,
							PricingQuantityUnit = UrsConstants.UnitOfMeasurementCode.Shipment,
							BreakType = UrsConstants.RateBreakTypeCode.Flat
						}
					}
				}
			};

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			var actual = result[0];

			var expected = new Charge[]
			{
				new Charge()
				{
					Restricted = false,
					ChargeCode = "FRT",
					FlatRate = null,
					PerUnitRate = 1000m,
					Unit = QuantityUnit.CN,
					Currency = "USD",
					ChargeType = ChargeType.None,
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FRT",
						Description = "Freight"
					}
				},
				new Charge()
				{
					ChargeCode = "123",
					FlatRate = 100,
					PerUnitRate = null,
					Unit = null,
					Currency = "USD",
					ChargeType = ChargeType.SubjectTo,
					Restricted = true,
					FreightInclusiveCarriageCharge = "FRT",
				},
			};

			AssertChargesEqual(
				expected,
				actual.Charges,
				c => new { c.ChargeCode, c.FlatRate, c.PerUnitRate, c.Unit, c.Currency, c.ChargeType, c.Restricted, c.FreightInclusiveCarriageCharge }
			);
		}

		public void TestConvert_CertainApplicabilities_RequireCurrency()
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea, "22G0");
			var errorLogger = new TestErrorLogger();
			var converter = new UniversalToWiseRateConverter(Logger, errorLogger);

			TestApplicability(RateApplicableCode.UnitPrice, expectedError: true);
			TestApplicability(RateApplicableCode.Nihil, expectedError: true);
			TestApplicability(RateApplicableCode.Zero, expectedError: true);
			TestApplicability(RateApplicableCode.Fixed, expectedError: true);
			TestApplicability(RateApplicableCode.Free, expectedError: true);
			TestApplicability(RateApplicableCode.Tact, expectedError: true);

			TestApplicability(RateApplicableCode.Iata, expectedError: false);
			TestApplicability(RateApplicableCode.OnRequest, expectedError: false);
			TestApplicability(RateApplicableCode.Percentage, expectedError: false);
			TestApplicability(RateApplicableCode.Vatos, expectedError: false);
			TestApplicability(RateApplicableCode.AllIn, expectedError: false);
			TestApplicability(RateApplicableCode.NotApplicable, expectedError: false);

			void TestApplicability(string applicableCode, bool expectedError)
			{
				SetOtherChargesAsOriginFlat(tradelane, "ABC", 100m, currency: null, applicability: applicableCode);
				var result = converter.Convert([tradelane]);

				if (expectedError)
				{
					AssertEquals("The charge \"ABC - ABC Name\" has no currency for RateCollections.Items[0].Currency", errorLogger.ErrorsReported.Last());
				}
				else
				{
					AssertEquals(2, result.First().Charges.Count);
					Assert("Should be no error reports", ErrorReporter.ExceptionsThrown.IsNullOrEmpty());
				}

				errorLogger.ErrorsReported.Clear();
			}
		}

		public void TestConvert_ForApplicabilityExpectCurrency()
		{
			void TestConvertExpectCurrency(string applicability)
			{
				var tradelane = CreateValidFclRate(TransportMode.Air);
				var baseRates = (RateCollectionDataDto)tradelane.PriceInfo.BaseRates;

				var firstPriceEntry = (UniversalRateEntryDto)baseRates.Items.First().PriceEntries.First();
				firstPriceEntry.Applicable = applicability;

				var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger()).Convert(new[] { tradelane });

				var currencies = result.SelectMany(x => x.Charges.Select(x => x.Currency));
				AssertContainsExactElementsInAnyOrder(new[] { "USD" }, currencies);
			}

			TestConvertExpectCurrency(RateApplicableCode.OnRequest);
			TestConvertExpectCurrency(RateApplicableCode.Iata);
			TestConvertExpectCurrency(RateApplicableCode.Vatos);
			TestConvertExpectCurrency(RateApplicableCode.Nihil);
			TestConvertExpectCurrency(RateApplicableCode.NotApplicable);
		}

		public void TestConvertAirFcl_GivenNullCurrency_ShouldBeFiltered()
		{
			// Arrange
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.PriceInfo.BaseRates.Items.Cast<RateCollectionDto>().Single().Currency = null;

			// Act
			var errorLogger = new TestErrorLogger();
			var result = new UniversalToWiseRateConverter(Logger, errorLogger).Convert(new[] { tradelane });

			// Assert
			AssertEquals(0, result.Count);
			Assert(Logger.Errors.Any(l => l.Equals("Error:The charge \"FRT - Freight\" has no currency for RateCollections.Items[0].Currency")));
			Assert(errorLogger.ErrorsReported.Any());
		}

		public void TestConvert_ContractNumber_Ocean()
		{
			// Arrange
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			var contract = (ContractDto)tradelane.Contract;
			contract.Header = new ContractHeaderDto { Reference = "reference", Name = "Contract Name" };

			// Act
			var errorLogger = new TestErrorLogger();
			var result = new UniversalToWiseRateConverter(Logger, errorLogger)
				.Convert(new[] { tradelane });

			// Assert
			AssertEquals("Contract Number uses ContractHeader.Name", "Contract Name", result.First().ContractNumber);
		}

		public void TestConvert_ChargeType_Ocean_BOL()
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			SetOtherChargesAsOriginFlat(tradelane, "ABC", 490m);
			AddOriginFlatCharge(tradelane, "DEF", 0m);
			((ChargeDefinitionDto)tradelane.PriceInfo.Charges.Items.First().ChargeDefinition).RequiresQuantifiedInput = true;

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			var actual = result.First();

			var expected = new Charge[]
			{
				new Charge()
				{
					ChargeCode = "FRT",
					ChargeType = ChargeType.Freight,
					IsOptional = false,
				},
				new Charge()
				{
					ChargeCode = "ABC",
					ChargeType = ChargeType.Bol,
					IsOptional = true,
				},
				new Charge
				{
					ChargeCode = "DEF",
					ChargeType = ChargeType.Bol,
					IsOptional = false,
				},
			};

			AssertChargesEqual(
				expected,
				actual.Charges,
				charge => new
				{
					charge.ChargeCode,
					charge.ChargeType,
					charge.IsOptional,
				}
			);
		}

		public void TestConvert_ChargeType_Ocean_Additional()
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			SetOtherChargesAsOriginFlat(tradelane, "ABC", 490m, UnitOfMeasurementCode.Container);
			((ChargeDefinitionDto)tradelane.PriceInfo.Charges.Items.First().ChargeDefinition).RequiresQuantifiedInput = true;

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			var actual = result.First();

			var expected = new Charge[]
			{
				new Charge()
				{
					ChargeCode = "FRT",
					ChargeType = ChargeType.Freight,
					IsOptional = false,
				},
				new Charge()
				{
					ChargeCode = "ABC",
					ChargeType = ChargeType.Additional,
					IsOptional = true,
				},
			};

			AssertChargesEqual(
				expected,
				actual.Charges,
				charge => new
				{
					charge.ChargeCode,
					charge.ChargeType,
					charge.IsOptional,
				}
			);
		}

		public void TestConvert_ChargeType_Ocean_Freight()
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			SetOtherChargesAsOriginFlat(tradelane, "ABC", 490m, UnitOfMeasurementCode.Container);

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			var actual = result.First();

			var expected = new Charge[]
			{
				new Charge()
				{
					ChargeCode = "FRT",
					ChargeType = ChargeType.Freight,
					IsOptional = false,
				},
				new Charge()
				{
					ChargeCode = "ABC",
					ChargeType = ChargeType.Freight,
					IsOptional = false,
				},
			};

			AssertChargesEqual(
				expected,
				actual.Charges,
				charge => new
				{
					charge.ChargeCode,
					charge.ChargeType,
					charge.IsOptional,
				}
			);
		}

		public void TestGetContainer_PayloadValues_MaxNet_Converted()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.Container = new ContainerDto()
			{
				IsoCode = "22G0",
				Weight = new ContainerWeightDto()
				{
					MaxNet = new UnitDto()
					{
						Quantity = 1,
						Unit = "Kilogram"
					},
					Pivot = new UnitDto()
					{
						Quantity = 2,
						Unit = "Kilogram"
					}
				}
			};

			var errorLogger = new TestErrorLogger();
			var result = new UniversalToWiseRateConverter(Logger, errorLogger).Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			AssertEquals(1m, result[0].Container.PayloadWeight);
		}

		public void TestGetContainer_PayloadValues_GivenMaxNetWeightUnitIsNotKilogram_ShouldBeLogged()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.Container = new ContainerDto()
			{
				IsoCode = "22G0",
				Weight = new ContainerWeightDto()
				{
					MaxNet = new UnitDto()
					{
						Quantity = 1,
						Unit = "Gram"
					},
					Pivot = new UnitDto()
					{
						Quantity = 0,
						Unit = "Gram"
					}
				}
			};

			var errorLogger = new TestErrorLogger();
			var result = new UniversalToWiseRateConverter(Logger, errorLogger)
							.Convert(new[] { tradelane });

			AssertContainsExactElementsInAnyOrder(new[] { "Warning:Rate Unit 'Gram' is not supported" }, Logger.Warnings);
		}

		public void TestGetContainer_PayloadValues_InsideVolume_Converted()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.Container = new ContainerDto()
			{
				IsoCode = "22G0",
				Measurements = new ContainerMeasurementsDto()
				{
					Inside = new ContainerMeasurementDto()
					{
						Volume = new UnitDto()
						{
							Quantity = 10,
							Unit = "CubicMeter"
						}
					}
				}
			};

			var errorLogger = new TestErrorLogger();
			var result = new UniversalToWiseRateConverter(Logger, errorLogger)
							.Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			AssertEquals(10m, result[0].Container.PayloadVolume);
		}

		public void TestGetContainer_PayloadValues_GivenInsideVolumeUnitIsNotCubicMeter_ShouldBeLogged()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.Container = new ContainerDto()
			{
				IsoCode = "22G0",
				Measurements = new ContainerMeasurementsDto()
				{
					Inside = new ContainerMeasurementDto()
					{
						Volume = new UnitDto()
						{
							Quantity = 10,
							Unit = "CubicCentimeter"
						}
					}
				}
			};

			var errorLogger = new TestErrorLogger();
			var result = new UniversalToWiseRateConverter(Logger, errorLogger)
							.Convert(new[] { tradelane });

			AssertContainsExactElementsInAnyOrder(new[] { "Warning:Rate Unit 'CubicCentimeter' is not supported" }, Logger.Warnings);
		}

		public void TestConvert_GivenContractServiceClass_CarrierContractNumber_Converted()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.ServiceClass = UrsConstants.ServiceClassCode.Contract;
			tradelane.ExternalReference = "SCR 006";
			var rates = Convert(tradelane);

			var rate = rates.Single();

			var expected = new[]
			{
				new CustomField
				{
					Code = Rate.CustomFields.Cargoguide.Reference, Value = "SCR 006", Description = Rate.CustomFields.Cargoguide.Reference
				}
			};

			var actual = rate.ProviderCustomFields.Where(customField => customField.Code == Rate.CustomFields.Cargoguide.Reference);

			AssertEquals("SCR 006", rate.ContractNumber);
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestConvert_GivenContractNonServiceClass_CarrierContractNumber_NotConverted()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.ExternalReference = "SCR 006";
			var rates = Convert(tradelane);

			var rate = rates.Single();

			var expected = new[]
			{
				new CustomField
				{
					Code = Rate.CustomFields.Cargoguide.Reference, Value = "SCR 006", Description = Rate.CustomFields.Cargoguide.Reference
				}
			};

			var actual = rate.ProviderCustomFields.Where(customField => customField.Code == Rate.CustomFields.Cargoguide.Reference);

			AssertNullOrEmpty(rate.ContractNumber);
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestConvertSeaFCL()
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			var voyageInfo = new TradeServiceVoyageDto { OceanRoutingTerm = "TEST" };
			tradelane.VoyageInfo = voyageInfo;

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			var actual = result.First();

			AssertEquals("UAIEV", actual.Origin);
			AssertEquals("AUSYD", actual.Destination);

			AssertEquals("AEDXB", actual.Via);
			AssertEquals("RAC", actual.Carrier);
			AssertEquals(new CarrierSpecificCommodity()
			{
				Code = "PRD",
				GroupName = "Group one",
				GroupType = CommodityCategory.NonHazardous,
				IncludedCommodities = Enumerable.Empty<string>()
			}, actual.CarrierCommodityInfo);
			AssertEquals(string.Empty, actual.ContractNumber);

			AssertEquals("TEST", actual.ServiceLevel);
			AssertEquals(WRConstants.TransportModes.SEA, actual.TransportMode);
			AssertEquals(utcToday.AddMonths(-5), actual.StartDate);
			AssertEquals(utcToday.AddMonths(5), actual.ExpiryDate);
			AssertEquals(new DateTime(2024, 12, 12), actual.IssueDate);
			AssertEquals("22G0", actual.Container.Code);
			AssertEquals("22G0", actual.Container.ISOType);
			AssertEquals(WRConstants.ContainerModes.FCL, actual.ContainerMode);
			AssertEquals("URS", actual.Provider);
			AssertEquals(string.Empty, actual.Commodity);
		}

		public void TestConvertBillOfLadingCharge()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			SetOtherChargesAsOriginFlat(tradelane, "BOL", 50m);

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			var actual = result.First();

			AssertChargesEqual(
				new Charge[]
				{
					new Charge
					{
						ChargeCode = "FRT",
						FlatRate = null,
						PerUnitRate = 1000m,
						Unit = QuantityUnit.CN,
						Currency = "USD",
					},
					new Charge
					{
						ChargeCode = "BOL",
						FlatRate = 50m,
						PerUnitRate = null,
						Unit = null,
						Currency = "USD",
					}
				},
				actual.Charges,
				c => new { c.ChargeCode, c.FlatRate, c.PerUnitRate, c.Unit, c.Currency }
			);
		}

		public void TestConvert_PercentageCharge_NoCurrency_FallbackToFreightCharge()
		{
			// Setup a charge that's a percentage of the freight base rate
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			SetOtherChargeAsPercentageOfBaseFreight(tradelane, "BAF", 2m, currency: null);

			var result = Convert(tradelane);

			AssertEquals(1, result.Count);
			var actual = result.First();

			AssertChargesEqual(
				[
					new Charge
					{
						ChargeCode = "FRT",
						FlatRate = null,
						PerUnitRate = 1000m,
						Unit = QuantityUnit.CN,
						Percentage = null,
						PercentageAppliesTo = null,
						Currency = "USD",
					},
					new Charge
					{
						ChargeCode = "BAF",
						FlatRate = 2,
						PerUnitRate = null,
						Unit = null,
						Percentage = 2m,
						PercentageAppliesTo = "FRT",
						Currency = "USD" // Should use the same currency as the base rate (FRT)
					}
				],
				actual.Charges,
				c => new { c.ChargeCode, c.FlatRate, c.PerUnitRate, c.Unit, c.Percentage, c.PercentageAppliesTo, c.Currency }
			);
		}

		public void TestConvert_PercentageCharge_HasCurrency()
		{
			// Setup a charge that's a percentage of the freight base rate
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			SetOtherChargeAsPercentageOfBaseFreight(tradelane, "BAF", 2m, currency: "EUR");

			var result = Convert(tradelane);

			AssertEquals(1, result.Count);
			var actual = result.First();

			AssertChargesEqual(
				[
					new Charge
					{
						ChargeCode = "FRT",
						FlatRate = null,
						PerUnitRate = 1000m,
						Unit = QuantityUnit.CN,
						Percentage = null,
						PercentageAppliesTo = null,
						Currency = "USD",
					},
					new Charge
					{
						ChargeCode = "BAF",
						FlatRate = 2,
						PerUnitRate = null,
						Unit = null,
						Percentage = 2m,
						PercentageAppliesTo = "FRT",
						Currency = "EUR" // Should use its own currency
					}
				],
				actual.Charges,
				c => new { c.ChargeCode, c.FlatRate, c.PerUnitRate, c.Unit, c.Percentage, c.PercentageAppliesTo, c.Currency }
			);
		}

		public void TestConvert_Charge_CustomFields_ApplyPrecision()
		{
			var precision = 0.5m;
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var firstPriceEntry = tradelane.PriceInfo.BaseRates.Items.First().PriceEntries.First();
			((UniversalRateEntryDto)firstPriceEntry).MeasurementPrecision = precision;

			var rates = Convert(tradelane);

			var charge = rates.Single().Charges.Single();
			AssertContainsCustomField(Rate.CustomFields.Common.Precision, "Precision", precision, charge.ProviderCustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargoguideReference()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.ExternalReference = "Some Stuff";

			var rates = Convert(tradelane);

			var rate = rates.Single();
			AssertContainsCustomField(Rate.CustomFields.Cargoguide.Reference, "Reference", "Some Stuff", rate.ProviderCustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargoguideRateClass()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.ServiceClass = ServiceClassCode.Gateway;

			var rates = Convert(tradelane);

			var rate = rates.Single();
			AssertContainsCustomField(Rate.CustomFields.Cargoguide.RateClass, "Rate/Service class", "Gateway", rate.ProviderCustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargoguideProductCode()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var product = (ProductDto)tradelane.Product;
			product.Code = "XYZ";

			var rates = Convert(tradelane);

			var rate = rates.Single();
			AssertContainsCustomField(Rate.CustomFields.Cargoguide.ProductCode, "Product Code", "XYZ", rate.ProviderCustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargoguideProductName()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var product = (ProductDto)tradelane.Product;
			product.Name = "Some Product Name";

			var rates = Convert(tradelane);

			var rate = rates.Single();
			AssertContainsCustomField(Rate.CustomFields.Cargoguide.ProductName, "Product Name", "Some Product Name", rate.ProviderCustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargosphereArbitraryPermission()
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			tradelane.TaggedValues = new TaggedValueDataDto()
			{
				Items = new[] { new TaggedValueDto { Key = "ArbitraryIndicator", Value = "Some Permission" } }
			};
			var rates = Convert(tradelane);

			var rate = rates.Single();
			AssertContainsCustomField(Rate.CustomFields.CargoSphere.ArbitraryPermission, "Arbitrary Permission", "Some Permission", rate.ProviderCustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargosphereTradeLane()
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			tradelane.VoyageInfo = new TradeServiceVoyageDto() { TradeScope = "Some Trade Lane" };
			var rates = Convert(tradelane);

			var rate = rates.Single();
			AssertContainsCustomField(Rate.CustomFields.CargoSphere.TradeLane, "Trade Lane", "Some Trade Lane", rate.ProviderCustomFields);
		}

		public void TestGivenUniversalRateInAirMode_WhenConvertToWiseRate_ThenContainerQualityShouldNotBeMapped()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air, "None");

			var rates = Convert(tradelane);

			var rate = rates.Single();
			var unexpectedCustomField = new CustomField
			{
				Code = Rate.CustomFields.CargoSphere.ContainerQuality,
				Description = "Container Quality",
				Value = "None",
			};
			AssertEquals(false, rate.ProviderCustomFields.Any(c => c.Equals(unexpectedCustomField)));
		}

		public void TestConvert_Rate_CargoSphereCustomFields_ContainerQuality()
		{
			//                  ShipperOwned | CntTyp| OverW| CargoFit    | CargoOversizes                    | Container Quality Code
			TestContainerQualityMapping(true, "NOR", false, "NONE", new[] { "NONE" }, "SOR");
			TestContainerQualityMapping(true, "", false, "NONE", new[] { "NONE" }, "SOC");
			TestContainerQualityMapping(true, "BLAH", false, "NONE", new[] { "NONE" }, "SOC", "ContainerType falls back to '' when IsShipperOwned");

			TestContainerQualityMapping(false, "", true, "NONE", new[] { "NONE" }, "OVW");
			TestContainerQualityMapping(false, "", true, "OUTOFGAUGE", new[] { "HEIGHT" }, "OHW");
			TestContainerQualityMapping(false, "BLAH", true, "NONE", new[] { "NONE" }, "OVW", "ContainerType falls back to '' when IsOverweight");
			TestContainerQualityMapping(false, "BLAH", true, "OUTOFGAUGE", new[] { "HEIGHT" }, "OHW", "ContainerType falls back to '' when IsOverweight");
			TestContainerQualityMapping(false, "", true, "BLAH", new[] { "HEIGHT" }, null, "invalid CargoFit when IsOverweight");
			TestContainerQualityMapping(false, "", true, "NONE", new[] { "WEIGHT" }, null, "invalid combination of CargoFit and CargoOversizes when IsOverweight");
			TestContainerQualityMapping(false, "", true, "OUTOFGAUGE", new[] { "WEIGHT" }, null, "invalid combination of CargoFit and CargoOversizes when IsOverweight");

			TestContainerQualityMapping(false, "NOR", false, "NONE", new[] { "NONE" }, "NOR");
			TestContainerQualityMapping(false, "GOH", false, "NONE", new[] { "NONE" }, "GOH");
			TestContainerQualityMapping(false, "GHS", false, "NONE", new[] { "NONE" }, "GHS");
			TestContainerQualityMapping(false, "GHD", false, "NONE", new[] { "NONE" }, "GHD");
			TestContainerQualityMapping(false, " RF ", false, " NONE ", new[] { " NONE " }, "REF", "padding spaces should not affect the mapping");
			TestContainerQualityMapping(false, "FOOD", false, "NONE", new[] { "NONE" }, "FOD");
			TestContainerQualityMapping(false, "FLEX", false, "NONE", new[] { "NONE" }, "FLX");
			TestContainerQualityMapping(false, "BLAH", false, "NONE", new[] { "NONE" }, null, "no ContainerType fallback found");

			TestContainerQualityMapping(false, "", false, "INGAUGE", new[] { "NONE" }, "ING");

			TestContainerQualityMapping(false, "", false, "OUTOFGAUGE", new[] { "WIDTHONESIDE", "HEIGHT" }, "OOS");
			TestContainerQualityMapping(false, "", false, "OUTOFGAUGE", new[] { "NONE" }, "OOG");
			TestContainerQualityMapping(false, "", false, "OUTOFGAUGE", new[] { "WIDTH", "HEIGHT" }, "HWD");
			TestContainerQualityMapping(false, "", false, "OUTOFGAUGE", new[] { "HEIGHT" }, "OVH");
			TestContainerQualityMapping(false, "", false, "OUTOFGAUGE", new[] { "WIDTH" }, "OWD");
			TestContainerQualityMapping(false, "BLAH", false, "OUTOFGAUGE", new[] { "WIDTHONESIDE", "HEIGHT" }, "OOS", "ContainerType falls back to ''");

			TestContainerQualityMapping(false, "", false, "BLAH", new[] { "NONE" }, null, "invalid CargoFit");
			TestContainerQualityMapping(false, "", false, "OUTOFGAUGE", new[] { "WIDTHONESIDE" }, null, "invalid combination");

			// Copied from ING case because ShipperOwned must be false to test the null values.
			TestContainerQualityMapping(false, "", null, "INGAUGE", null, "ING", "null IsOverweight should be treated as false");
			TestContainerQualityMapping(false, "", false, "INGAUGE", null, "ING", "null CargoOversize should be treated as empty");
		}

		void TestContainerQualityMapping(bool isShipperOwned, string containerType, bool? isOverweight, string cargoFit, string[] cargoOversizes, string expectedContainerQualityCode, string reason = null)
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			tradelane.Container = new ContainerDto
			{
				IsoCode = "22G0",
				IsShipperOwned = isShipperOwned,
				Type = containerType,
				IsOverweight = isOverweight,
				CargoFit = cargoFit,
				CargoOversize = cargoOversizes,
			};

			var rates = Convert(tradelane);
			var rate = rates.Single();

			string PropertiesToString() =>
				$"IsShipperOwned: {isShipperOwned}, ContainerType: {(string.IsNullOrWhiteSpace(containerType) ? "Empty" : containerType)}, "
				+ $"IsOverweight: {isOverweight}, CargoFit: {(string.IsNullOrWhiteSpace(cargoFit) ? "Empty" : cargoFit)}, "
				+ $"CargoOversizes: {(cargoOversizes.IsNullOrEmpty() ? "Empty" : string.Join(", ", cargoOversizes))}";

			if (expectedContainerQualityCode != null)
			{
				var message = $"{PropertiesToString()}\nShould be mapped to {expectedContainerQualityCode}";
				if (!string.IsNullOrEmpty(reason))
				{
					message += $"\nReason: {reason}";
				}
				AssertContainsCustomField(Rate.CustomFields.CargoSphere.ContainerQuality, "Container Quality", expectedContainerQualityCode, rate.ProviderCustomFields);
			}
			else
			{
				var message = $"{PropertiesToString()}\nShould not be mapped.";
				if (!string.IsNullOrEmpty(reason))
				{
					message += $"\nReason: {reason}";
				}
				AssertEquals(message, false, rate.ProviderCustomFields.Any(c => c.Code == Rate.CustomFields.CargoSphere.ContainerQuality));
			}
		}

		public void TestGivenUniversalRateInAirMode_WhenConvertToWiseRate_ThenIsHigherBreakLowerRateShouldBeTrue()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);

			var rates = Convert(tradelane);
			var rate = rates.Single();

			foreach (var charge in rate.Charges)
			{
				Assert("HigherBreakLowerRate Should be true", charge.IsHigherBreakLowerRate);
			}
		}

		public void TestConvert_Min_BreakPlus()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			// min=1, +1, +100, +500
			frtRate.PriceEntries = new[]
			{
				CreateShipmentMinBreakItem(100m),
				CreateSimpleWeightBreakPlus(1, 1.2m),
				CreateSimpleWeightBreakPlus(100, 1.1m),
				CreateSimpleWeightBreakPlus(500, 1m),
			};

			var expectedCharges = new[]
			{
				new Charge { ChargeCode = "FRT", PerUnitRate = 1.2m, MinRate = 100m, BreakOperator = ">=", Break = 1, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", PerUnitRate = 1.1m, MinRate = 100m, BreakOperator = ">=", Break = 100, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", PerUnitRate = 1.0m, MinRate = 100m, BreakOperator = ">=", Break = 500, Unit = WRConstants.Units.Weight.Kilograms },
			};

			var convertedRates = Convert(tradelane);

			AssertChargesFromSingleRate(expectedCharges, convertedRates);
		}

		public void TestConvert_ShipmentUnit_ConvertedToHouseOfBill()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries = new[]
			{
				new UniversalRateEntryDto()
				{
					Applicable = UrsConstants.RateApplicableCode.UnitPrice,
					BreakType = UrsConstants.RateBreakTypeCode.BreakPlus,
					Price = 100m,
					BreakQuantity = 1,
					QuantityUnit = UrsConstants.UnitOfMeasurementCode.Shipment,
					PricingQuantityUnit = UrsConstants.UnitOfMeasurementCode.Shipment,
					RoundingMode = 0.01m,
					UseVolumetric = false,
					PricingQuantity = 1,
					MeasurementPrecision = 1,
				},
			};

			var expectedCharges = new[]
			{
				new Charge { ChargeCode = "FRT", PerUnitRate = 100m, BreakOperator = ">=", Break = 1, Unit = WRConstants.Units.HouseOfBill },
			};

			var convertedRates = Convert(tradelane);

			AssertChargesFromSingleRate(expectedCharges, convertedRates);
		}

		public void TestConvert_Min_BreakLess_BreakPlus()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			// min=1, -45, +45, +100, +250, +300
			frtRate.PriceEntries = new[]
			{
				CreateShipmentMinBreakItem(100m),
				CreateSimpleWeightBreakLess(45, 6.5m),
				CreateSimpleWeightBreakPlus(45, 6.4m),
				CreateSimpleWeightBreakPlus(100, 6.3m),
				CreateSimpleWeightBreakPlus(250, 6.2m),
				CreateSimpleWeightBreakPlus(300, 6.1m),
			};

			var expectedCharges = new[]
			{
				new Charge { ChargeCode = "FRT", PerUnitRate = 6.5m, MinRate = 100m, BreakOperator = ">=", Break = 0, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", PerUnitRate = 6.4m, MinRate = 100m, BreakOperator = ">=", Break = 45, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", PerUnitRate = 6.3m, MinRate = 100m, BreakOperator = ">=", Break = 100, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", PerUnitRate = 6.2m, MinRate = 100m, BreakOperator = ">=", Break = 250, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", PerUnitRate = 6.1m, MinRate = 100m, BreakOperator = ">=", Break = 300, Unit = WRConstants.Units.Weight.Kilograms },
			};

			var convertedRates = Convert(tradelane);

			AssertChargesFromSingleRate(expectedCharges, convertedRates);
		}

		public void TestConvert_TACTReferenceRate()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			// min=1, -45, +45, +100, +250, +300
			frtRate.PriceEntries = new[]
			{
				CreateSimpleWeightBreakLess(45, 0m, UrsConstants.RateApplicableCode.Tact),
				CreateSimpleWeightBreakPlus(45, 0m, UrsConstants.RateApplicableCode.Tact),
				CreateSimpleWeightBreakPlus(100, 0m, UrsConstants.RateApplicableCode.Tact),
				CreateSimpleWeightBreakPlus(250, 0m, UrsConstants.RateApplicableCode.Tact),
				CreateSimpleWeightBreakPlus(300, 0m, UrsConstants.RateApplicableCode.Tact),
			};

			var expectedCharges = new[]
			{
				new Charge { BreakOperator = ">=", Break = 0 },
				new Charge { BreakOperator = ">=", Break = 45 },
				new Charge { BreakOperator = ">=", Break = 100 },
				new Charge { BreakOperator = ">=", Break = 250 },
				new Charge { BreakOperator = ">=", Break = 300 },
			};
			expectedCharges.ForEach(charge =>
			{
				charge.ChargeCode = "FRT";
				charge.Unit = WRConstants.Units.Weight.Kilograms;
				charge.Applicability = "TACT – TACT Reference rate";
				charge.Restricted = true;
				charge.PerUnitRate = 0;
			});

			var convertedRates = Convert(tradelane);

			AssertChargesEqual(
				expectedCharges,
				convertedRates.First().Charges,
				charge => new
				{
					charge.ChargeCode,
					charge.PerUnitRate,
					charge.MinRate,
					charge.Break,
					charge.BreakOperator,
					charge.BreakUnit,
					charge.Unit,
					charge.Applicability,
					charge.Restricted
				}
			);
		}

		public void TestConvert_ChargesWithBreaksAndMinMaxBase()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries = new[]
			{
				CreateShipmentMinBreakItem(1000m),
				CreateShipmentMaxBreakItem(123456m),
				CreateSimpleBaseItem(100m),
				CreateSimpleWeightBreakPlus(1, 11m),
				CreateSimpleWeightBreakPlus(100, 10m),
			};

			var expectedCharges = new[]
			{
				new Charge { ChargeCode = "FRT", PerUnitRate = 11m, MinRate = 1000m, MaxRate = 123456m, FlatRate = 100m, BreakOperator = ">=", Break = 1, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", PerUnitRate = 10m, MinRate = 1000m, MaxRate = 123456m, FlatRate = 100m, BreakOperator = ">=", Break = 100, Unit = WRConstants.Units.Weight.Kilograms },
			};

			var convertedRates = Convert(tradelane);

			AssertChargesEqual(
				expectedCharges,
				convertedRates.First().Charges,
				charge => new
				{
					charge.ChargeCode,
					charge.Break,
					charge.BreakOperator,
					charge.PerUnitRate,
					charge.MinRate,
					charge.MaxRate,
					charge.FlatRate
				}
			);
		}

		public void TestConvert_RangeBreaksAreIgnoredForNow()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();
			frtRate.PriceEntries = new[]
			{
				new UniversalRateEntryDto()
				{
					Applicable = UrsConstants.RateApplicableCode.UnitPrice,
					BreakType = UrsConstants.RateBreakTypeCode.Range,
					TierLowerBoundQuantity = 100,
					TierUpperBoundQuantity = 200,
					Price = 100m,
					BreakQuantity = 1,
					QuantityUnit = UrsConstants.UnitOfMeasurementCode.Kilogram,
					PricingQuantityUnit = UrsConstants.UnitOfMeasurementCode.Kilogram,
					RoundingMode = 0.01m,
					UseVolumetric = true,
					PricingQuantity = 1,
					MeasurementPrecision = 0.5m,
				}
			};

			var convertedRates = Convert(tradelane);
			AssertEquals(0, convertedRates.Count);
		}

		void TestConvert_PivotBreaks(TradeServiceDto tradelane, Charge[] expectedCharges, bool useVolumetric)
		{
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries = new[]
			{
				CreateSimpleBaseItem(100m, UrsConstants.UnitOfMeasurementCode.Container, useVolumetric),
				CreateSimplePivotBreakPlus(200m, 10m)
			};

			var convertedRates = Convert(tradelane);

			AssertChargesEqual(
				expectedCharges,
				convertedRates.First().Charges,
				charge => new
				{
					charge.ChargeCode,
					charge.Break,
					charge.BreakOperator,
					charge.PerUnitRate,
					charge.MinRate,
					charge.MaxRate,
					charge.Currency,
					charge.ActualPercentage
				}
			);
		}

		public void TestConvert_HandlePivotBreaks_AirVolumetric()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var expectedCharges = new[]
			{
				new Charge
				{
					ChargeCode = "FRT",
					PerUnitRate = 100m,
					Unit = WRConstants.Units.Weight.Kilograms,
					Currency = "USD",
					EquipmentUnit = QuantityUnit.CN,
					ActualPercentage = null,
				},
				new Charge
				{
					ChargeCode = "FRT",
					PerUnitRate = 10m,
					BreakOperator = ">=",
					Break = 200,
					Unit = WRConstants.Units.Weight.Kilograms,
					Currency = "USD",
					EquipmentUnit = QuantityUnit.CN
				},
			};

			TestConvert_PivotBreaks(tradelane, expectedCharges, true);
		}

		public void TestConvert_HandlePivotBreaks_Air()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			var expectedCharges = new[]
			{
				new Charge
				{
					ChargeCode = "FRT",
					PerUnitRate = 100m,
					Unit = WRConstants.Units.Weight.Kilograms,
					Currency = "USD",
					EquipmentUnit = QuantityUnit.CN,
					ActualPercentage = 100,
				},
				new Charge
				{
					ChargeCode = "FRT",
					PerUnitRate = 10m,
					BreakOperator = ">=",
					Break = 200,
					Unit = WRConstants.Units.Weight.Kilograms,
					Currency = "USD",
					EquipmentUnit = QuantityUnit.CN
				},
			};

			TestConvert_PivotBreaks(tradelane, expectedCharges, false);
		}

		public void TestConvert_Rate_CustomFields_Routing()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.RouteInfo = new RouteInfoDto()
			{
				Waypoints = new[]
				{
					new GeoScopeEntryDto() { WaypointType = UrsConstants.RouteWaypointCode.Origin, Location = new LocationDto() { Code = "UAIEV", FunctionCode = UrsConstants.LocationFunctionCode.Airport } },
					new GeoScopeEntryDto() { WaypointType = UrsConstants.RouteWaypointCode.Destination, Location = new LocationDto() { Code = "AUSYD", FunctionCode = UrsConstants.LocationFunctionCode.Airport } },
					new GeoScopeEntryDto() { WaypointType = UrsConstants.RouteWaypointCode.Via, Location = new LocationDto() { Code = "SGSIN", FunctionCode = UrsConstants.LocationFunctionCode.Airport } },
					new GeoScopeEntryDto() { WaypointType = UrsConstants.RouteWaypointCode.Via2, Location = new LocationDto() { Code = "HKHKG", FunctionCode = UrsConstants.LocationFunctionCode.Airport } },
				}
			};

			var rates = Convert(tradelane);
			var rate = rates.Single();

			AssertContainsCustomField(Rate.CustomFields.Common.Routing, "Routing", "UAIEV -> SGSIN -> HKHKG -> AUSYD", rate.ProviderCustomFields);
		}

		public void TestConvert_SubjectToCharge()
		{
			// Arrange
			var tradelane = CreateValidFclRate(TransportMode.Air);
			SetOtherChargesAsOriginFlat(tradelane, "ABC", 490m);
			AddOriginFlatCharge(tradelane, "DEF", 0m);
			// Act
			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			// Assert
			AssertEquals(1, result.Count);
			var actual = result.First();

			var expectedCharges = new List<Charge>
			{
				new Charge
				{
					ChargeCode = "FRT",
					FlatRate = null,
					PerUnitRate = 1000m,
					Unit = QuantityUnit.CN,
					Currency = "USD",
					ChargeType = ChargeType.None
				},
				new Charge
				{
					ChargeCode = "ABC",
					FlatRate = 490m,
					PerUnitRate = null,
					Unit = null,
					Currency = "USD",
					ChargeType = ChargeType.SubjectTo,
				},
				new Charge
				{
					ChargeCode = "DEF",
					FlatRate = 0m,
					PerUnitRate = null,
					Unit = null,
					Currency = "USD",
					ChargeType = ChargeType.SubjectTo,
				},
			};

			AssertEquals(expectedCharges.Count, actual.Charges.Count);
			AssertChargesEqual(
				expectedCharges,
				actual.Charges,
				c => new { c.ChargeCode, c.FlatRate, c.PerUnitRate, c.Unit, c.Currency, c.ChargeType }
			);
		}

		public void TestConvert_OptionalCharge()
		{
			// Arrange
			var tradelane = CreateValidFclRate(TransportMode.Air);
			SetOtherChargesAsOriginFlat(tradelane, "ABC", 490m);
			((ChargeDefinitionDto)tradelane.PriceInfo.Charges.Items.First().ChargeDefinition).RequiresQuantifiedInput = true;

			// Act
			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			// Assert
			AssertEquals(1, result.Count);
			var actual = result.First();

			var expectedCharges = new List<Charge>
			{
				new Charge
				{
					ChargeCode = "FRT",
					FlatRate = null,
					PerUnitRate = 1000m,
					Unit = QuantityUnit.CN,
					Currency = "USD",
					ChargeType = ChargeType.None
				},
				new Charge
				{
					ChargeCode = "ABC",
					FlatRate = 490m,
					PerUnitRate = null,
					Unit = null,
					Currency = "USD",
					ChargeType = ChargeType.Optional,
				},
			};

			AssertEquals(2, actual.Charges.Count);
			AssertChargesEqual(
				expectedCharges,
				actual.Charges,
				c => new { c.ChargeCode, c.FlatRate, c.PerUnitRate, c.Unit, c.Currency, c.ChargeType }
			);
		}

		public void TestConvert_GetCarrierCommodity_IncludedCommodities()
		{
			var tradelane = CreateValidFclRate(TransportMode.Air);
			tradelane.Commodity = new CommodityDataDto()
			{
				Groups = new List<ICommodityGroupDto> {
					new CommodityGroupDto()
					{
						Items = new List<ICommodityDto> {
							new CommodityDto()
							{
								Code = string.Empty,
								Name = "HEALTH FOOD",
								HsCode = string.Empty,
							},
							new CommodityDto()
							{
								Code = string.Empty,
								Name = "MILK POWDER",
								HsCode = string.Empty,
							},
						}
					}
				}
			};

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert(new[] { tradelane });

			AssertEquals(1, result.Count);
			var actual = result.First();

			var expected = new CarrierSpecificCommodity()
			{
				Code = "PRD",
				GroupName = "Product X",
				GroupType = CommodityCategory.NonHazardous,
				IncludedCommodities = new List<string>
				{
					"HEALTH FOOD",
					"MILK POWDER"
				},
			};

			AssertEquals(expected, actual.CarrierCommodityInfo);
		}

		public void TestConvert_RestrictedReference_VatosChargeWithoutCurrency()
		{
			var expectedCharges = new[]
			{
				new Charge { ChargeCode = "FRT", PerUnitRate = 1000, Applicability = null, Restricted = false,
					Currency = "USD", FreightInclusiveCarriageCharge = null },
				new Charge { ChargeCode = "DTHC", PerUnitRate = null,
					Applicability = "VATOS – Final charge to be determined", Restricted = true, Currency = null,
					FreightInclusiveCarriageCharge = "FRT" },
			};

			var tradelane = CreateValidFclRate(TransportMode.Sea);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			// The extra DTHC charge below is expected to be converted to a rate line with a SubjectTo FRT calculator.
			priceInfo.Charges = new BaseChargeDataDto
			{
				Items =
				[
					new BaseChargeDto
					{
						Level = "TradeServiceCharge",
						Code = "DTHC",
						UniversalCode = "DTHC",
						Name = "",
						ChargeDefinition = new ChargeDefinitionDto
						{
							Code = "DTHC",
							UniversalCode = "DTHC",
							ShippingPhase = new ShippingPhaseDto
							{
								Code = ShippingPhaseCode.MainCarriage
							}
						},
						RateCollections = new RateCollectionDataDto
						{
							Items =
							[
								new RateCollectionDto
								{
									PriceEntries =
									[
										new UniversalRateEntryDto
										{
											Price = 0,
											Applicable = RateApplicableCode.Vatos,
											BreakType = RateBreakTypeCode.Flat,
											BreakQuantity = 1,
											PricingQuantityUnit = UnitOfMeasurementCode.None,
											UseVolumetric = false
										}
									]
								}
							]
						}
					}
				]
			};

			var rateItems = priceInfo.BaseRates.Items;
			var frtRate = (RateCollectionDto)rateItems.First();

			frtRate.PriceEntries =
			[
				new UniversalRateEntryDto
				{
					Price = 1000,
					Applicable = RateApplicableCode.UnitPrice,
					BreakType = RateBreakTypeCode.Flat,
					BreakQuantity = 1,
					QuantityUnit = UnitOfMeasurementCode.Container,
					PricingQuantity = 1,
					PricingQuantityUnit = UnitOfMeasurementCode.Container,
				}
			];

			var convertedRates = Convert(tradelane);

			AssertChargesEqual(
				expectedCharges,
				convertedRates[0].Charges,
				c => new
				{
					c.ChargeCode,
					c.PerUnitRate,
					c.Applicability,
					c.Restricted,
					c.Currency,
					c.IsInclusive,
					c.FreightInclusiveCarriageCharge
				}
			);
		}

		public void TestConvert_RestrictedReference_ORCharge()
		{
			var expectedCharges = new[]
			{
				new Charge { BreakOperator = ">=", Break = 0 },
				new Charge { BreakOperator = ">=", Break = 45 },
				new Charge { BreakOperator = ">=", Break = 100 },
				new Charge { BreakOperator = ">=", Break = 250 },
				new Charge { BreakOperator = ">=", Break = 300 },
			};
			expectedCharges.ForEach(charge =>
			{
				charge.ChargeCode = "FRT";
				charge.Applicability = "OR – On Request at Carrier";
				charge.Restricted = true;
				charge.Currency = "USD";
				charge.PerUnitRate = 0;
				charge.Unit = WRConstants.Units.Weight.Kilograms;
			});

			TestConvert_ReferenceRates(RateApplicableCode.OnRequest, "USD", expectedCharges);
		}

		public void TestConvert_NotApplicableReferenceRates()
		{
			var expectedCharges = new[]
			{
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 0, PerUnitRate = 0, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 45, PerUnitRate = 0, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 100, PerUnitRate = 0, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 250, PerUnitRate = 0, Unit = WRConstants.Units.Weight.Kilograms },
				new Charge { ChargeCode = "FRT", BreakOperator = ">=", Break = 300, PerUnitRate = 0, Unit = WRConstants.Units.Weight.Kilograms },
			};
			expectedCharges.ForEach(c =>
			{
				c.Applicability = "Not Applicable – Break NA in certain scenarios";
				c.Restricted = true;
				c.Currency = "USD";
			});

			TestConvert_ReferenceRates(RateApplicableCode.NotApplicable, "USD", expectedCharges);
		}

		void TestConvert_ReferenceRates(string applicable, string currency, Charge[] expectedCharges)
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();
			frtRate.Currency = currency;

			frtRate.PriceEntries =
			[
				CreateSimpleWeightBreakLess(45, 0m, applicable),
				CreateSimpleWeightBreakPlus(45, 0m, applicable),
				CreateSimpleWeightBreakPlus(100, 0m, applicable),
				CreateSimpleWeightBreakPlus(250, 0m, applicable),
				CreateSimpleWeightBreakPlus(300, 0m, applicable)
			];

			var convertedRates = Convert(tradelane);

			AssertChargesEqual(
				expectedCharges,
				convertedRates[0].Charges,
				c => new
				{
					c.ChargeCode,
					c.PerUnitRate,
					c.MinRate,
					c.Break,
					c.BreakOperator,
					c.BreakUnit,
					c.Unit,
					c.Applicability,
					c.Restricted,
					c.Currency
				}
			);
		}

		public void TestConvert_ZeroReferenceRates()
		{
			var tradelane = CreateValidFclRate(TransportMode.Sea);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries =
			[
				new UniversalRateEntryDto
				{
					Price = 0,
					Applicable = RateApplicableCode.Zero,
					BreakType = RateBreakTypeCode.Flat,
					BreakQuantity = 1,
					PricingQuantityUnit = UnitOfMeasurementCode.Container,
					UseVolumetric = false,
					PricingQuantity = 1,
				}
			];

			var convertedRates = Convert(tradelane);

			var actualCharge = convertedRates[0].Charges.Single();

			// The single charge with PerUnitRate will be converted to a rate line with UNT calculator
			AssertEquals("FRT", actualCharge.ChargeCode);
			AssertEquals(false, actualCharge.Restricted);
			AssertEquals("A condition for UNT calculator resolver", 0m, actualCharge.PerUnitRate);
			AssertNull("A condition for UNT calculator resolver", actualCharge.FlatRate);
			AssertNull("A condition for UNT calculator resolver", actualCharge.FreightInclusiveCarriageCharge);
		}
		
		public void TestConvert_InclusiveChargeType_Sea()
		{
			var scenario =
				new
				{
					RateCode = UrsConstants.RateApplicableCode.NotApplicable,
					Applicability = "Not Applicable – Break NA in certain scenarios",
					Restricted = true
				};

			var tradelane = CreateValidFclRate(TransportMode.Sea);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries =
			[
				CreateSimpleWeightBreakLess(45, 0m, UrsConstants.RateApplicableCode.NotApplicable)
			];
			((RateCollectionDataDto)tradelane.PriceInfo.BaseRates).Inclusive =
			[
				new ChargeDefinitionDto
				{
					Code = "FSCLocal",
					UniversalCode = "FSC",
					Description = "Fuel Surcharge"
				}
			];
			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert([tradelane]);

			var actual = result.First();
			var expected = new Charge[]
			{
				new Charge()
				{
					ChargeCode = "FRT",
					FlatRate = null,
					PerUnitRate = 0,
					Unit = QuantityUnit.KG,
					Currency = "USD",
					ChargeType = ChargeType.Freight,
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FRT",
						Description = "Freight"
					}
				},
				new Charge()
				{
					ChargeCode = "FRT",
					FlatRate = null,
					PerUnitRate = -1,
					Unit = QuantityUnit.KG,
					Currency = "USD",
					ChargeType = ChargeType.Freight,
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FRT",
						Description = "Freight"
					}
				},
				new Charge()
				{
					ChargeCode = "FSC",
					FlatRate = null,
					PerUnitRate = null,
					Unit = null,
					Currency = null,
					ChargeType = ChargeType.Included,
					FreightInclusiveCarriageCharge = "FRT",
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FSCLocal",
						Description = "Fuel Surcharge",
					}
				}
			};

			AssertChargesEqual(expected, actual.Charges, c => new
			{
				c.ChargeCode,
				c.FlatRate,
				c.PerUnitRate,
				c.Unit,
				c.Currency,
				c.ChargeType,
				c.FreightInclusiveCarriageCharge,
				c.CarrierChargeCodeInfo.Code
			});
		}

		public void TestConvert_InclusiveChargeType_Air()
		{
			var scenario =
				new
				{
					RateCode = UrsConstants.RateApplicableCode.NotApplicable,
					Applicability = "Not Applicable – Break NA in certain scenarios",
					Restricted = true
				};

			var tradelane = CreateValidFclRate(TransportMode.Air);
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries =
			[
				CreateSimpleWeightBreakLess(45, 0m, UrsConstants.RateApplicableCode.NotApplicable)
			];
			((RateCollectionDataDto)tradelane.PriceInfo.BaseRates).Inclusive =
			[
				new ChargeDefinitionDto
				{
					Code = "FSCLocal",
					UniversalCode = "FSC",
					Description = "Fuel Surcharge"
				}
			];
			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger())
				.Convert([tradelane]);

			var actual = result.First();
			var expected = new Charge[]
			{
				new Charge()
				{
					ChargeCode = "FRT",
					FlatRate = null,
					PerUnitRate = 0,
					Unit = QuantityUnit.KG,
					Currency = "USD",
					ChargeType = ChargeType.NotApplicable,
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FRT",
						Description = "Freight"
					}
				},
				new Charge()
				{
					ChargeCode = "FRT",
					FlatRate = null,
					PerUnitRate = -1,
					Unit = QuantityUnit.KG,
					Currency = "USD",
					ChargeType = ChargeType.NotApplicable,
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FRT",
						Description = "Freight"
					}
				},
				new Charge()
				{
					ChargeCode = "FSC",
					FlatRate = null,
					PerUnitRate = null,
					Unit = null,
					Currency = null,
					ChargeType = ChargeType.Included,
					FreightInclusiveCarriageCharge = "FRT",
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FSCLocal",
						Description = "Fuel Surcharge",
					}
				}
			};

			AssertChargesEqual(expected, actual.Charges, c => new
			{
				c.ChargeCode,
				c.FlatRate,
				c.PerUnitRate,
				c.Unit,
				c.Currency,
				c.ChargeType,
				c.FreightInclusiveCarriageCharge,
				c.CarrierChargeCodeInfo.Code
			});
		}

		#region Booking Info

		public void TestConvert_BookingTerms()
		{
			var tradeService = CreateValidFclRate(TransportMode.Sea, "22G0");
			AddTerms(tradeService);

			var terms = UniversalToWiseRateConverter.GetBookingTerms(tradeService);
			CombineAssertions(() =>
			{
				AssertEquals(2, terms.Items.Length);
				var term1 = terms.Items[0];
				AssertEquals("USD", term1.Currency);
				AssertEquals(100m, term1.Fee);
				AssertEquals("Description", term1.Name);
				var term2 = terms.Items[1];
				AssertEquals("USD", term2.Currency);
				AssertEquals(200m, term2.Fee);
				AssertEquals("Description2", term2.Name);
			});
		}

		public void TestConvert_Penalties()
		{
			var tradeService = CreateValidFclRate(TransportMode.Sea, "22G0");
			AddPenalties(tradeService);

			var penalties = UniversalToWiseRateConverter.GetPenalties(tradeService);
			CombineAssertions(() =>
			{
				AssertEquals(1, penalties.Length);
				var penalty = penalties[0];
				AssertEquals("USD", penalty.Currency);
				AssertEquals(Core.Constants.FreightShipmentDirection.Code.Export, penalty.Direction);
				AssertEquals(10, penalty.StartDay);
				AssertEquals(14, penalty.EndDay);
				AssertEquals(50m, penalty.PerUnitRate);
				AssertEquals(Core.Constants.ContainerDetentionPenaltyType.STO, penalty.Type);
				AssertEquals("Description", penalty.Name);
			});
		}

		public void TestConvert_Schedules()
		{
			var tradeService = CreateValidFclRate(TransportMode.Sea, "22G0");
			AddSchedules(tradeService);

			var schedule = UniversalToWiseRateConverter.GetSchedules(tradeService).First();
			CombineAssertions(() =>
			{
				AssertEquals("VesselName", schedule.VesselName);
				AssertEquals("VoyageNumber", schedule.VoyageNumber);
				AssertEquals("Reference", schedule.ExternalPriceReference);
				AssertEquals(new DateTime(2020, 12, 5), schedule.ArrivalDate);
				AssertEquals(new DateTime(2021, 12, 5), schedule.DepartureDate);

				AssertEquals(2, schedule.ScheduleDetails.Length);

				var firstSegment = schedule.ScheduleDetails[0];
				AssertEquals("AUSYD", firstSegment.Origin);
				AssertEquals("USLAX", firstSegment.Destination);
				AssertEquals(new DateTime(2020, 12, 5, 5, 0, 0), firstSegment.ArrivalDate);
				AssertEquals(new DateTime(2020, 12, 5, 10, 0, 0), firstSegment.DepartureDate);
				AssertEquals(TimeSpan.FromHours(5), firstSegment.TransitTime);
				AssertEquals("VesselName", firstSegment.VesselName);
				AssertEquals("VoyageNumber", firstSegment.VoyageNumber);
				AssertEquals("ImoNumber", firstSegment.IMONumber);
				AssertEquals("Flag", firstSegment.FlagCode);
				AssertEquals("ServiceCode", firstSegment.ServiceCode);
				AssertEquals("ServiceName", firstSegment.ServiceName);
				AssertEquals("TradeLane", firstSegment.TradeLane);
				AssertEquals(1, firstSegment.DateInfos.Length);
				AssertNull("There should be no VGMCutOff date when no VGM Events are present", firstSegment.VGMCutOff);
				AssertEquals(new DateTime(2020, 12, 12), firstSegment.CTOCutOff);
				AssertNull("There should be no DocsDue date when no SINONAMS Events are present", firstSegment.DocsDue);
				var firstSegmentDateInfo = firstSegment.DateInfos[0];
				AssertEquals("CY", firstSegmentDateInfo.Code);
				AssertEquals("Commercial cargo cutoff", firstSegmentDateInfo.Name);
				AssertEquals("Documentation", firstSegmentDateInfo.Type);
				AssertEquals(new DateTime(2020, 12, 12, 2, 0, 0), firstSegmentDateInfo.Date);

				var secondSegment = schedule.ScheduleDetails[1];
				AssertEquals("AUSYD", secondSegment.Origin);
				AssertEquals("USLAX", secondSegment.Destination);
				AssertEquals(new DateTime(2021, 12, 5, 5, 0, 0), secondSegment.ArrivalDate);
				AssertEquals(new DateTime(2021, 12, 5, 10, 0, 0), secondSegment.DepartureDate);
				AssertEquals(TimeSpan.FromHours(5), secondSegment.TransitTime);
				AssertEquals("VesselName2", secondSegment.VesselName);
				AssertEquals("VoyageNumber2", secondSegment.VoyageNumber);
				AssertEquals("ImoNumber2", secondSegment.IMONumber);
				AssertEquals("Flag2", secondSegment.FlagCode);
				AssertEquals("ServiceCode2", secondSegment.ServiceCode);
				AssertEquals("ServiceName2", secondSegment.ServiceName);
				AssertEquals("TradeLane2", secondSegment.TradeLane);
				AssertEquals(2, secondSegment.DateInfos.Length);
				AssertEquals(new DateTime(2020, 12, 15), secondSegment.VGMCutOff);
				AssertNull("There should be no CTOCutoff date when no CY Events are present", secondSegment.CTOCutOff);
				AssertNull("There should be no DocsDue date when no SINONAMS Events are present", secondSegment.DocsDue);
				var secondSegmentFirstDate = secondSegment.DateInfos[0];
				AssertEquals("VGM", secondSegmentFirstDate.Code);
				AssertEquals("Origin demurrage", secondSegmentFirstDate.Name);
				AssertEquals("STO", secondSegmentFirstDate.Type);
				AssertEquals(new DateTime(2020, 12, 15, 4, 0, 0), secondSegmentFirstDate.Date);
				var secondSegmentSecondDate = secondSegment.DateInfos[1];
				AssertEquals("VGM", secondSegmentSecondDate.Code);
				AssertEquals("Origin demurrage", secondSegmentSecondDate.Name);
				AssertEquals("STO", secondSegmentSecondDate.Type);
				AssertEquals(new DateTime(2020, 12, 17, 3, 0, 0), secondSegmentSecondDate.Date);
			});
		}

		public void TestConvert_BookingInfo_WhenNoTerms()
		{
			var tradeService = CreateValidFclRate(TransportMode.Sea, "22G0");
			AddSchedules(tradeService);
			AddPenalties(tradeService);

			var bookingInfo = UniversalToWiseRateConverter.GetBookingInfo(tradeService).First();
			CombineAssertions(() =>
			{
				AssertNotNull(bookingInfo);
				AssertNull(bookingInfo.BookingTerms);
				AssertEquals(1, bookingInfo.Penalties.Length);
				AssertEquals(2, bookingInfo.Schedule.ScheduleDetails.Length);
			});
		}

		public void TestConvert_BookingInfo_WhenNoSchedules()
		{
			var tradeService = CreateValidFclRate(TransportMode.Sea, "22G0");
			AddTerms(tradeService);
			AddPenalties(tradeService);

			var bookingInfo = UniversalToWiseRateConverter.GetBookingInfo(tradeService).First();
			CombineAssertions(() =>
			{
				AssertNotNull(bookingInfo);
				AssertEquals(2, bookingInfo.BookingTerms.Items.Length);
				AssertEquals(1, bookingInfo.Penalties.Length);
				AssertNull(bookingInfo.Schedule);
			});
		}

		public void TestConvert_BookingInfo_WhenNoPenalties()
		{
			var tradeService = CreateValidFclRate(TransportMode.Sea, "22G0");
			AddSchedules(tradeService);
			AddTerms(tradeService);

			var bookingInfo = UniversalToWiseRateConverter.GetBookingInfo(tradeService).First();
			CombineAssertions(() =>
			{
				AssertNotNull(bookingInfo);
				AssertEquals(2, bookingInfo.BookingTerms.Items.Length);
				AssertEquals(0, bookingInfo.Penalties.Length);
				AssertEquals(2, bookingInfo.Schedule.ScheduleDetails.Length);
			});
		}

		public void TestConvert_BookingInfo()
		{
			var tradeService = CreateValidFclRate(TransportMode.Sea, "22G0");
			AddTerms(tradeService);
			AddPenalties(tradeService);
			AddSchedules(tradeService);

			var bookingInfo = UniversalToWiseRateConverter.GetBookingInfo(tradeService).First();
			CombineAssertions(() =>
			{
				AssertNotNull(bookingInfo);
				AssertEquals(2, bookingInfo.BookingTerms.Items.Length);
				AssertEquals(1, bookingInfo.Penalties.Length);
				AssertEquals(2, bookingInfo.Schedule.ScheduleDetails.Length);
			});
		}

		public void TestConvert_StorageCharges_FilteredFromCharges()
		{
			var tradeService = CreateValidFclRate(TransportMode.Sea, "22G0");
			(tradeService.PriceInfo as PriceInfoDto)
				.AddChargeWithSinglePriceEntry(new ("BAF", usabilityGroupCode: ChargeUsabilityGroupCode.None))
				.AddPenaltyWithSinglePriceEntry(new ("ODOC", usabilityGroupCode: ChargeUsabilityGroupCode.Storage));

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger()).Convert([tradeService]);

			AssertContainsExactElementsInAnyOrder(
				["FRT", "BAF"],
				result[0].Charges.Select(charge => charge.ChargeCode)
			);
		}

		public void TestConvert_PenCharges_FilteredFromCharges()
		{
			var tradeService = CreateValidFclRate(TransportMode.Sea, "22G0");
			(tradeService.PriceInfo as PriceInfoDto)
				.AddChargeWithSinglePriceEntry(new("BAF", usabilityGroupCode: ChargeUsabilityGroupCode.None))
				.AddFreeTimeWithSinglePriceEntry(new("ODOC", usabilityGroupCode: ChargeUsabilityGroupCode.Pen));

			var result = new UniversalToWiseRateConverter(Logger, new TestErrorLogger()).Convert([tradeService]);

			AssertContainsExactElementsInAnyOrder(
				["FRT", "BAF"],
				result[0].Charges.Select(charge => charge.ChargeCode)
			);
		}

		public static void AddTerms(TradeServiceDto tradeService)
		{
			(tradeService.PriceInfo as PriceInfoDto)
				.AddPenaltyWithSinglePriceEntry(new ("ODOC", usabilityGroupCode: ChargeUsabilityGroupCode.Storage, chargeDescription: "Description", price: 100))
				.AddPenaltyWithSinglePriceEntry(new ("ODOC", usabilityGroupCode: ChargeUsabilityGroupCode.Storage, chargeDescription: "Description2", price: 200));
		}

		public static void AddPenalties(TradeServiceDto tradeService)
		{
			(tradeService.PriceInfo as PriceInfoDto)
				.AddFreeTimeWithSinglePriceEntry(new("ODOC",
					usabilityGroupCode: ChargeUsabilityGroupCode.Pen,
					chargeDescription: "Description",
					shippingPhaseCode: ShippingPhaseCode.PortfLoading,
					breakQuantity: 0,
					quantityUnit: "Day",
					chargeDefinitionCode: UrsPenaltyType.Demurrage,
					price: 0));
			(tradeService.PriceInfo.FreeTime.Items.First().RateCollections.Items.First() as RateCollectionDto)
				.AddPriceEntry(new () { Price = 50, BreakQuantity = 10 })
				.AddPriceEntry(new () { Price = 12, BreakQuantity = 15 });
		}

		public static void AddSchedules(TradeServiceDto tradeService)
		{
			var tradeServiceSchedule = tradeService
				.AddSchedule(
					new ScheduleTravelInfoDto
					{
						Arrival = new ScheduleArriveDepartDto { Date = new DateTime(2020, 12, 5), Time = TimeSpan.FromHours(0) },
						Departure = new ScheduleArriveDepartDto { Date = new DateTime(2021, 12, 5), Time = TimeSpan.FromHours(0) },
					},
					"Reference");
			tradeServiceSchedule
				.AddSegment(
					new ScheduleTravelInfoDto
					{
						Arrival = new ScheduleArriveDepartDto { Date = new DateTime(2020, 12, 5), Time = TimeSpan.FromHours(5) },
						Departure = new ScheduleArriveDepartDto { Date = new DateTime(2020, 12, 5), Time = TimeSpan.FromHours(10) },
						TransitDuration = TimeSpan.FromHours(5),
					},
					new ScheduleTransportDto
					{
						Description = new Dictionary<string, object>
						{
							{ ScheduleTransportValue.VesselName, "VesselName" },
							{ ScheduleTransportValue.VoyageNumber, "VoyageNumber" },
							{ ScheduleTransportValue.ImoNumber, "ImoNumber" },
							{ ScheduleTransportValue.FlagCode, "Flag" },
							{ ScheduleTransportValue.ServiceCode, "ServiceCode" },
							{ ScheduleTransportValue.ServiceName, "ServiceName" },
							{ ScheduleTransportValue.TradeLane, "TradeLane" },
						}
					},
					new ScheduleLocationDataDto
					{
						Items = new ScheduleLocationDto[]
						{
							new()
							{
								Location = "AUSYD"
							},
							new()
							{
								Location = "USLAX"
							}
						}
					})
				.AddEvent(
					"CY",
					"Commercial cargo cutoff",
					"Documentation",
					new DateTime(2020, 12, 12),
					TimeSpan.FromHours(2));
			tradeServiceSchedule
				.AddSegment(
					new ScheduleTravelInfoDto
					{
						Arrival = new ScheduleArriveDepartDto { Date = new DateTime(2021, 12, 5), Time = TimeSpan.FromHours(5) },
						Departure = new ScheduleArriveDepartDto { Date = new DateTime(2021, 12, 5), Time = TimeSpan.FromHours(10) },
						TransitDuration = TimeSpan.FromHours(5),
					},
					new ScheduleTransportDto
					{
						Description = new Dictionary<string, object>
						{
							{ ScheduleTransportValue.VesselName, "VesselName2" },
							{ ScheduleTransportValue.VoyageNumber, "VoyageNumber2" },
							{ ScheduleTransportValue.ImoNumber, "ImoNumber2" },
							{ ScheduleTransportValue.FlagCode, "Flag2" },
							{ ScheduleTransportValue.ServiceCode, "ServiceCode2" },
							{ ScheduleTransportValue.ServiceName, "ServiceName2" },
							{ ScheduleTransportValue.TradeLane, "TradeLane2" },
						}
					},
					new ScheduleLocationDataDto
					{
						Items = new ScheduleLocationDto[]
						{
							new()
							{
								Location = "AUSYD"
							},
							new()
							{
								Location = "USLAX"
							}
						}
					})
				.AddEvent(
					"VGM",
					"Origin demurrage",
					"STO",
					new DateTime(2020, 12, 15),
					TimeSpan.FromHours(4))
				.AddEvent(
					"VGM",
					"Origin demurrage",
					"STO",
					new DateTime(2020, 12, 17),
					TimeSpan.FromHours(3));
		}

		#endregion

		#region Helpers

		static void AssertChargesFromSingleRate(IEnumerable<Charge> expectedCharges, IEnumerable<Rate> actualRates)
		{
			if (expectedCharges == null)
			{
				AssertNull(actualRates);
				return;
			}

			AssertEquals(1, actualRates.Count());
			AssertChargesEqual(
				expectedCharges,
				actualRates.First().Charges,
				charge => new
				{
					charge.ChargeCode,
					charge.PerUnitRate,
					charge.MinRate,
					charge.Break,
					charge.BreakOperator,
					charge.BreakUnit,
					charge.Unit
				}
			);
		}

		public static void AssertChargesEqual(IEnumerable<Charge> expected, IEnumerable<Charge> actual, Func<Charge, object> selectProperties = null)
		{
			if (selectProperties == null)
			{
				selectProperties = charge => charge;
			}

			AssertContainsExactElementsInAnyOrder(
				expected.Select(selectProperties),
				actual.Select(selectProperties)
			);
		}

		public static void AssertContainsCustomField(string code, string desc, object value, IEnumerable<CustomField> actual)
		{
			var customField = new CustomField
			{
				Code = code,
				Description = desc,
				Value = value,
			};
			AssertContainsCustomField(customField, actual);
		}

		public static void AssertContainsCustomField(CustomField expected, IEnumerable<CustomField> actual)
		{
			Assert(actual.Any(c => c.Equals(expected)));
		}

		UniversalRateEntryDto CreateSimpleWeightBreakLess(decimal breakQuantity, decimal price, string applicable = UrsConstants.RateApplicableCode.UnitPrice)
			=> CreateSimpleWeightBreakItem(UrsConstants.RateBreakTypeCode.BreakLess, breakQuantity, price, applicable);

		UniversalRateEntryDto CreateSimpleWeightBreakPlus(decimal breakQuantity, decimal price, string applicable = UrsConstants.RateApplicableCode.UnitPrice)
			=> CreateSimpleWeightBreakItem(UrsConstants.RateBreakTypeCode.BreakPlus, breakQuantity, price, applicable);
		UniversalRateEntryDto CreateSimplePivotBreakPlus(decimal breakQuantity, decimal price, string applicable = UrsConstants.RateApplicableCode.UnitPrice)
		=> CreateSimpleWeightBreakItem(UrsConstants.RateBreakTypeCode.Pivot, breakQuantity, price, applicable);

		UniversalRateEntryDto CreateSimpleWeightBreakItem(string breakType, decimal breakQuantity, decimal price, string applicable)
		{
			return new UniversalRateEntryDto()
			{
				Applicable = applicable,
				BreakType = breakType,
				Price = price,
				BreakQuantity = breakQuantity,
				QuantityUnit = UrsConstants.UnitOfMeasurementCode.Kilogram,
				PricingQuantityUnit = UrsConstants.UnitOfMeasurementCode.Kilogram,
				RoundingMode = 0.01m,
				UseVolumetric = true,
				PricingQuantity = 1,
				MeasurementPrecision = 0.5m,
			};
		}

		UniversalRateEntryDto CreateShipmentMinBreakItem(decimal price, string applicable = UrsConstants.RateApplicableCode.UnitPrice)
		{
			return new UniversalRateEntryDto()
			{
				Applicable = applicable,
				BreakType = UrsConstants.RateBreakTypeCode.Min,
				Price = price,
				BreakQuantity = 1,
				QuantityUnit = UrsConstants.UnitOfMeasurementCode.Shipment,
				PricingQuantityUnit = UrsConstants.UnitOfMeasurementCode.Shipment,
				RoundingMode = 0.01m,
				UseVolumetric = false,
				PricingQuantity = 1,
				MeasurementPrecision = 1,
			};
		}

		UniversalRateEntryDto CreateSimpleBaseItem(decimal price, string quantityUnit = UrsConstants.UnitOfMeasurementCode.Kilogram, bool useVolumetric = false)
		{
			return new UniversalRateEntryDto()
			{
				Applicable = UrsConstants.RateApplicableCode.UnitPrice,
				BreakType = UrsConstants.RateBreakTypeCode.Base,
				Price = price,
				BreakQuantity = 1,
				RoundingMode = 0.01m,
				UseVolumetric = useVolumetric,
				PricingQuantity = 1,
				MeasurementPrecision = 1,
				QuantityUnit = quantityUnit
			};
		}

		UniversalRateEntryDto CreateShipmentMaxBreakItem(decimal price)
		{
			return new UniversalRateEntryDto()
			{
				Applicable = UrsConstants.RateApplicableCode.UnitPrice,
				BreakType = UrsConstants.RateBreakTypeCode.Max,
				Price = price,
				BreakQuantity = 1,
				QuantityUnit = UrsConstants.UnitOfMeasurementCode.Shipment,
				PricingQuantityUnit = UrsConstants.UnitOfMeasurementCode.Shipment,
				RoundingMode = 0.01m,
				UseVolumetric = false,
				PricingQuantity = 1,
				MeasurementPrecision = 1,
			};
		}

		/// <summary>
		/// Set the non-freight charges to be a single flat charge.
		/// </summary>
		void SetOtherChargesAsOriginFlat(TradeServiceDto tradelane, string universalChargeCode, decimal price, string priceUnit = UnitOfMeasurementCode.BillOfLading, string currency = "USD", string applicability = "Upr")
		{
			((PriceInfoDto)tradelane.PriceInfo).Charges = new BaseChargeDataDto
			{
				Items = new List<BaseChargeDto>
				{
					CreateOriginFlatCharge(universalChargeCode, price, priceUnit, currency, applicability)
				}
			};
		}

		void AddOriginFlatCharge(TradeServiceDto tradelane, string universalChargeCode, decimal price, string priceUnit = UnitOfMeasurementCode.BillOfLading, string currency = "USD")
		{
			((List<BaseChargeDto>)(tradelane.PriceInfo.Charges.Items)).Add(CreateOriginFlatCharge(universalChargeCode, price, priceUnit, currency));
		}

		static BaseChargeDto CreateOriginFlatCharge(string universalChargeCode, decimal price, string priceUnit, string currency, string applicability = "Upr")
		{
			return new BaseChargeDto()
			{
				Level = "TradeServiceCharge",
				Code = universalChargeCode + "Local",
				UniversalCode = universalChargeCode,
				Name = universalChargeCode + " Name",
				ChargeDefinition = new ChargeDefinitionDto()
				{
					Code = universalChargeCode + "Local",
					UniversalCode = universalChargeCode,
					ShippingPhase = new ShippingPhaseDto()
					{
						Code = ShippingPhaseCode.PortfLoading
					}
				},
				RateCollections = new RateCollectionDataDto()
				{
					Items = new[]
					{
						new RateCollectionDto()
						{
							Currency = currency,
							PriceEntries = new []
							{
								new UniversalRateEntryDto
								{
									Price = price,
									Applicable = price == 0 ? RateApplicableCode.Zero : applicability,
									BreakType = RateBreakTypeCode.Flat,
									BreakQuantity = 1,
									PricingQuantityUnit = priceUnit
								}
							}
						}
					}
				}
			};
		}

		void SetOtherChargeAsPercentageOfBaseFreight(TradeServiceDto tradelane, string universalChargeCode, decimal percentage, string currency = "USD")
		{
			((PriceInfoDto)tradelane.PriceInfo).Charges = new BaseChargeDataDto
			{
				Items = new List<BaseChargeDto>
				{
					new BaseChargeDto()
					{
						Level = "TradeServiceCharge",
						Code = universalChargeCode + "Local",
						UniversalCode = universalChargeCode,
						Name = universalChargeCode + " Name",
						ChargeDefinition = new ChargeDefinitionDto()
						{
							Code = universalChargeCode + "Local",
							UniversalCode = universalChargeCode,
							ShippingPhase = new ShippingPhaseDto()
							{
								Code = UrsConstants.ShippingPhaseCode.Freight
							}
						},
						RateCollections = new RateCollectionDataDto()
						{
							Items = new []
							{
								new RateCollectionDto()
								{
									Currency = currency,
									PriceEntries = new []
									{
										new UniversalRateEntryDto
										{
											// Is this the correct field to store percentage, or is it another?
											// There's no example rates in the test db to tell.
											// The CS converter gets percentage from CargoSphereCharge.OriginalRate, which isn't in mapping doc.
											// CG converter doesn't do percentage.
											Price = percentage,
											Applicable = UrsConstants.RateApplicableCode.Percentage,
											BreakType = UrsConstants.RateBreakTypeCode.Flat,
											BreakQuantity = 1,
											PricingQuantityUnit = UrsConstants.UnitOfMeasurementCode.Shipment,
										}
									}
								}
							}
						}
					}
				}
			};
		}

		enum TransportMode
		{
			Air,
			Sea
		}

		TradeServiceDto CreateValidFclRate(TransportMode transportMode, string containerType = "")
		{
			return new TradeServiceDto()
			{
				ModesOfTransport = new ModesOfTransportDataDto()
				{
					Items = new[]
					{
						new ModeOfTransportDto()
						{
							Code = transportMode == TransportMode.Air ? UrsConstants.ModeOfTransport.Air : UrsConstants.ModeOfTransport.Ocean
						}
					}
				},

				Type = UrsConstants.ComposedTradeServiceType.Direct,

				// Code is the IATA code for AIR rate.
				TransportProvider = new TransportProviderDto() { Code = "RAC" },

				ServiceClass = UrsConstants.ServiceClassCode.Selling,
				Container = new ContainerDto()
				{
					IsoCode = "22G0",
					IsShipperOwned = true,
					Type = containerType,
				},
				Product = new ProductDto()
				{
					Code = "PRD",
					Name = "Product X",
					Classification = new ProductClassDto()
					{
						Code = ProductClassCode.Gen,
						Name = "General"
					},
					ServiceLevel = new ProductServiceLevelDto()
					{
						Code = "Std"
					},
					UniversalCode = "HAZD",
				},
				Commodity = new CommodityDataDto
				{
					Groups = [new CommodityGroupDto
					{
						Code = "ABC",
						Description = "Group one"
					}]
				},

				RouteInfo = new RouteInfoDto()
				{
					Waypoints = new[]
					{
						new GeoScopeEntryDto() { WaypointType = UrsConstants.RouteWaypointCode.Origin, Location = new LocationDto() { Code = "UAIEV", FunctionCode = UrsConstants.LocationFunctionCode.Airport } },
						new GeoScopeEntryDto() { WaypointType = UrsConstants.RouteWaypointCode.Via, Location = new LocationDto() { Code = "AEDXB", FunctionCode = UrsConstants.LocationFunctionCode.Airport } },
						new GeoScopeEntryDto() { WaypointType = UrsConstants.RouteWaypointCode.Destination, Location = new LocationDto() { Code = "AUSYD", FunctionCode = UrsConstants.LocationFunctionCode.Airport } },
					}
				},
				Contract = new ContractDto()
				{
					EffectiveDateInfo = new EffectiveDateInfoDto()
					{
						StartDate = utcToday.AddMonths(-6),
						EndDate = utcToday.AddMonths(6)
					}
				},
				PriceInfo = new PriceInfoDto()
				{
					BaseRates = new RateCollectionDataDto()
					{
						Items = new[]
						{
							new RateCollectionDto()
							{
								VersionDateInfo = new VersionDateInfoDto
								{
									StartDate = utcToday.AddMonths(-5),
									EndDate = utcToday.AddMonths(5),
									IssueDate = new DateTime(2024,12,12)
								},
								Currency = "USD",
								PriceEntries = new []
								{
									new UniversalRateEntryDto
									{
										Price = 1000,
										Applicable = UrsConstants.RateApplicableCode.UnitPrice,
										BreakType = UrsConstants.RateBreakTypeCode.Flat,
										BreakQuantity = 1,
										QuantityUnit = UrsConstants.UnitOfMeasurementCode.Container,
										PricingQuantity = 1,
										PricingQuantityUnit = UrsConstants.UnitOfMeasurementCode.Container,
									}
								}
							}
						}
					},
				}
			};
		}

		IList<Rate> Convert(TradeServiceDto tradelane)
			=> new UniversalToWiseRateConverter(Logger, new TestErrorLogger()).Convert(new[] { tradelane });

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new TestLogger();
		}

		protected DateTime utcToday { get; } = ZDateTime.UtcToday.ToDateTime();
		protected TestLogger Logger { get; private set; }

		#endregion
	}

	class TestErrorLogger : IUniversalToWiseRateErrorReporter
	{
		public void ReportMappingError(string message, string functionName, params (string name, object obj)[] sourceObjects)
		{
			ErrorsReported.Add(message);
		}

		public List<string> ErrorsReported { get; } = new List<string>();
	}
}
