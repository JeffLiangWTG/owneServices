using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.Interfaces;
using WiseRates.Api.Model;
using static Enterprise.Rating.Business.RatingConstants;
using static Enterprise.Rating.Business.UrsConstants;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Business.Test
{
	public class UrsRatesConverterTest : RatingTestCase
	{
		#region Carrier Conversion

		public void TestConvertAirCarrier()
		{
			var carrier = NewCarrier(carrierCode: "BIRS", fullName: "Bir Carrier", iataCode: "XX", airlineAccountingCode: "0xx");
			Factory.Save();
			AssertCarrierConvertCorrectly(CreateValidUldOrFclRate(TransportMode.Air, "XX"), carrier, 1, 0);
		}

		public void TestConvertSeaCarrier() => AssertCarrierConvertCorrectly(CreateValidUldOrFclRate(TransportMode.Sea, "QANT"), SeaCarrier, 0, 1);

		void AssertCarrierConvertCorrectly(TradeServiceDto tradeService, OrgHeader carrier, int refAirlines, int refShippingLines)
		{
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });
			AssertCarrierConversion(header, carrier.PK, carrier.OH_Code, carrier.ShippingLine?.RSL_CargoWiseOneCode, UrsCarrier.GetIata(carrier), carrier.ShippingLineSCAC, refAirlines, refShippingLines, [], [], isError: false);
		}

		public void TestConvertAirCarrier_WhenMappingCarrierFails_AndRefAirlineInDb()
		{
			var line = Factory.New<RefAirline>();
			line.RM_ThreeLetterCode = "404";
			Factory.Save();

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "404");
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });

			AssertCarrierConversion(header, null, null, null, "404", null, 1, 0, [], ["No active Carrier is assigned with IATA Code '404'"], isError: true);
		}

		public void TestConvertAirCarrier_WhenMappingCarrierFails_AndMultipleMatchingOrgs()
		{
			NewCarrier("Carrier_1", "Carrier with same IATA Code 1", iataCode: "40");
			NewCarrier("Carrier_2", "Carrier with same IATA Code 2", iataCode: "40", airlineAccountingCode: "4xx");
			Factory.Save();

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "40");
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });

			AssertCarrierConversion(header, null, null, null, "40", null, 2, 0, ["Carrier_1", "Carrier_2"], ["More than one of the active Carriers (Carrier_1, Carrier_2) are assigned with IATA Code '40'"], isError: true);
		}

		public void TestConvertAirCarrier_WhenMappingCarrierFails_AndMultipleMatchingLines()
		{
			var line1 = Factory.New<RefAirline>();
			line1.RM_TwoCharacterCode = "40";
			line1.RM_AirlineName1 = "40Line";
			var line2 = Factory.New<RefAirline>();
			line2.RM_TwoCharacterCode = "40";
			line2.RM_AirlineName1 = "50Line";
			Factory.Save();

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "40");
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });

			AssertCarrierConversion(header, null, null, null, "40", null, 2, 0, [], ["No active Carrier is assigned with IATA Code '40'"], isError: true);
		}

		public void TestConvertAirCarrier_WhenMappingCarrierFails_AndMultipleMatchingOrgsAndLines()
		{
			var line1 = Factory.New<RefAirline>();
			line1.RM_TwoCharacterCode = "40";
			line1.RM_AirlineName1 = "40Line";
			var line2 = Factory.New<RefAirline>();
			line2.RM_TwoCharacterCode = "40";
			line2.RM_AirlineName1 = "50Line";
			NewCarrier("Carrier_1", "Carrier with same IATA Code 1", iataCode: "40");
			NewCarrier("Carrier_2", "Carrier with same IATA Code 2", iataCode: "40", airlineAccountingCode: "4xx");
			Factory.Save();

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "40");
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });

			AssertCarrierConversion(header, null, null, null, "40", null, 4, 0, ["Carrier_1", "Carrier_2"], ["More than one of the active Carriers (Carrier_1, Carrier_2) are assigned with IATA Code '40'"], isError: true);
		}

		public void TestConvertAirCarrier_WhenMappingCarrierFails()
		{
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "404");
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });

			AssertCarrierConversion(header, null, null, null, "404", null, 0, 0, [], ["No active Carrier is assigned with IATA Code '404'"], isError: true);
		}

		public void TestConvertAirCarrier_IATACode_UnsupportedLength()
		{
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "TEST");
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });

			AssertCarrierConversion(header, null, null, null, "TEST", null, 0, 0, [], ["IATA Code 'TEST' with length 4 has not been supported"], isError: true);
		}

		public void TestConvertSeaCarrier_WhenMappingCarrierFails()
		{
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "404");
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });

			AssertCarrierConversion(header, null, null, null, null, "404", 0, 0, [], ["No active Carrier is assigned with SCAC Code '404'"], isError: true);
		}

		public void TestConvertSeaCarrier_WhenMappingCarrierFails_AndRefShippinglineInDb()
		{
			var line = Factory.New<RefShippingLine>();
			line.RSL_StandardCarrierAlphaCode = "4040";
			line.RSL_CargoWiseOneCode = "404";
			line.RSL_CarrierName = "ABC";
			Factory.Save();

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "4040");
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });

			AssertCarrierConversion(header, null, null, null, null, "4040" , 0, 1, [], ["No active Carrier is assigned with SCAC Code '4040'"], isError: true);
		}

		public void TestConvertSeaCarrier_OneInactiveCarrierWithSCACCode()
		{
			var carrier = NewCarrier("Carrier_1", scacCode: "QQQQ");
			carrier.OH_IsActive = false;

			Factory.Save();

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QQQQ");
			var header = converter.GetHeader(UrsRatesParseHelper.GetTransportMode(tradeService), UrsRatesParseHelper.GetCarrier(tradeService.TransportProvider), new[] { tradeService });

			var expectedWarnings = "No active Carrier is assigned with SCAC Code 'QQQQ'";

			AssertCarrierConversion(header, null, null, null, null, "QQQQ", 0, 1, [], [expectedWarnings], isError: true);
		}

		void AssertCarrierConversion(
			UrsRatingHeader header,
			ZGuid? expectedCarrierPK,
			string expectedOrgCode,
			string expectedC1c,
			string expectedIata,
			string expectedScac,
			int refAirlines,
			int refShippingLines,
			string[] expectedMultiMapped,
			string[] expectedWarnings,
			bool isError
		)
		{
			AssertEquals(expectedCarrierPK, header.ServiceProvider.OrgHeader?.PK);
			AssertEquals(expectedOrgCode, header.ServiceProvider.OrgCode);
			AssertEquals(expectedC1c, header.ServiceProvider.C1cCode);
			AssertEquals(expectedIata, header.ServiceProvider.IataCode);
			AssertEquals(expectedScac, header.ServiceProvider.ScacCode);
			AssertEquals("Should have refAirlines", refAirlines, header.ServiceProvider.RefAirlines.Length);
			AssertEquals("Should have refShippingLines", refShippingLines, header.ServiceProvider.RefShippingLines.Length);
			AssertContainsExactElementsInAnyOrder(expectedMultiMapped, header.ServiceProvider.OrgHeaderCodes);
			Assert(expectedWarnings.All(Logger.Warnings.Contains));
			if (isError)
			{
				AssertContains("No single Carrier is assigned with the SCAC, IATA Code of the Carrier from Rates Service:", header.Errors[RatingHeaderSchema.TH_OH]);
			}
			else
			{
				AssertNull(header.Errors.GetValueOrDefault(RatingHeaderSchema.TH_OH));
			}
		}

		#endregion

		#region Charge Type

		public void TestConvert_Air_ChargeType_NotRequiresQuantified() => AssertConvert_Air_ChargeType(false, "Subject To");

		public void TestConvert_Air_ChargeType_RequiresQuantified() => AssertConvert_Air_ChargeType(true, "Optional");

		void AssertConvert_Air_ChargeType(bool requiresQuantifiedInput, string expectedChargeType)
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "22G0");
			SetOtherChargeAsPercentageOfBaseFreight(tradeService, "ABC", 2m, requiresQuantifiedInput);

			((RateCollectionDataDto)tradeService.PriceInfo.BaseRates).Inclusive = new[]
			{
				new ChargeDefinitionDto
				{
					Code = "FSCLocal",
					UniversalCode = "FSC",
					Description = "Fuel Surcharge"
				}
			};

			((RateCollectionDataDto)tradeService.PriceInfo.Charges.Items.First().RateCollections).Inclusive = new[]
			{
				new ChargeDefinitionDto {
					Code = "EEELocal",
					UniversalCode = "EEE",
					Description = "Other inclusive charge"
				}
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			AssertEquals(2, entries.Count());

			var freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR);
			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG);

			var freightRateLine = freightRateEntry.ChildRateLines.FirstOrDefault(line => line.ChargeCode.AC_Code == "FRT");
			AssertEquals("Freight charge type is null", "", freightRateLine?.ChargeType);

			var freightInclusiveRateLine = freightRateEntry.ChildRateLines.FirstOrDefault(line => line.ChargeCode.AC_Code == "FSC");
			AssertEquals("Inclusive charge under priceInfo baseRates, chargeType is Included", "Included", freightInclusiveRateLine?.ChargeType);

			var originRateLine = originRateEntry.ChildRateLines.FirstOrDefault(line => line.ChargeCode.AC_Code == "ABC");
			AssertEquals("ChargeDefination requiresQuantifiedInput = true, the type is Optional; otherwise the type is Subject To", expectedChargeType, originRateLine?.ChargeType);

			var originInclusiveRateLine = originRateEntry.ChildRateLines.FirstOrDefault(line => line.ChargeCode.AC_Code == "EEE");
			AssertEquals("Inclusive charge under priceInfo charges, chargeType is Included", "Included", originInclusiveRateLine?.ChargeType);
		}

		public void TestConvert_Sea_ChargeType_Freight() => AssertConvert_Sea_ChargeType(false, UnitOfMeasurementCode.Container, "Freight");

		public void TestConvert_Sea_ChargeType_Bol() => AssertConvert_Sea_ChargeType(false, UnitOfMeasurementCode.BillOfLading, "BOL");
		public void TestConvert_Sea_ChargeType_Additional() => AssertConvert_Sea_ChargeType(true, "", "Additional");

		void AssertConvert_Sea_ChargeType(bool requiresQuantifiedInput, string pricingQuantityUnit, string expectedChargeType)
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargeAsPercentageOfBaseFreight(tradeService, "ABC", 100m, requiresQuantifiedInput, pricingQuantityUnit);

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			AssertEquals(2, entries.Count());

			var freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);
			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG);

			var freightRateLine = freightRateEntry.ChildRateLines.FirstOrDefault(line => line.ChargeCode.AC_Code == "FRT");
			AssertEquals("For FRT charge, the chargeType is always Freight", "Freight", freightRateLine?.ChargeType);

			var originRateLine = originRateEntry.ChildRateLines.FirstOrDefault(line => line.ChargeCode.AC_Code == "ABC");
			AssertEquals("When chargeDefinition.RequiresQuantifiedInput = true, the type is Additional; otherwise see PriceEntities.pricingQuantityUnit, when it's bol, the type is BOL, otherwise the type is Freight",
				expectedChargeType,
				originRateLine.ChargeType);
		}

		#endregion

		#region Container Conversion

		public void TestConvert_Container()
		{
			var container1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container1.RC_ISOType = "22G0";

			var container2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container2.RC_ISOType = "22G0";

			var container3 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR");
			container3.RC_ISOType = "99ZZ"; // So nothing else is mapped to it

			Factory.Save();

			CombineAssertions(() => {
				AssertConvertsContainers(
					"Sea FCL (containerized), ISOType assigned to multiple containers, should return rates for each container",
					TransportMode.Sea, FreightMode.FCL, ZGuid.Empty, "22G0", [container1.PK, container2.PK]
				);

				AssertConvertsContainers(
					"Sea FCL (containerized), ISOType assigned to single container, should return single rate for container",
					TransportMode.Sea, FreightMode.FCL, ZGuid.Empty, "99ZZ", [container3.PK]
				);

				AssertConvertsContainers(
					"Sea FCL (containerized), ISOType not assigned to any containers (unmapped), shouldn't be filtered but should have error",
					TransportMode.Sea, FreightMode.FCL, ZGuid.Empty, "66GP", [ZGuid.Empty]
				);

				AssertConvertsContainers(
					"Sea FCL (containerized), ISOType assigned to single container, container not in criteria, should be filtered.",
					TransportMode.Sea, FreightMode.FCL, container1.PK, "99ZZ", []
				);

				AssertConvertsContainers(
					"Sea FCL (containerized), ISOType assigned to single container, container in criteria, should not be filtered.",
					TransportMode.Sea, FreightMode.FCL, container3.PK, "99ZZ", [container3.PK]
				);

				AssertConvertsContainers(
					"Sea LCL (non-containerized), should pass through container filter fine",
					TransportMode.Sea, FreightMode.LCL, container1.PK, "99ZZ", [ZGuid.Empty]
				);

				AssertConvertsContainers(
					"Air ULD (containerized), should use RC_Code instead of ISOType",
					TransportMode.Air, FreightMode.ULD, ZGuid.Empty, "20GP", [container1.PK]
				);

				AssertConvertsContainers(
					"Air ULD (containerized), unmapped code, shouldn't be filtered but should have error",
					TransportMode.Air, FreightMode.ULD, ZGuid.Empty, "66GP", [ZGuid.Empty]
				);

				AssertConvertsContainers(
					"Air ULD (containerized), code mapped to single container, container not in criteria so should be filtered",
					TransportMode.Air, FreightMode.ULD, container1.PK, "40GP", []
				);

				AssertConvertsContainers(
					"Air ULD (containerized), code mapped to single container, container in criteria so should not be filtered",
					TransportMode.Air, FreightMode.ULD, container2.PK, "40GP", [container2.PK]
				);

				AssertConvertsContainers(
					"Air LSE (non-containerized), should pass through container filter fine",
					TransportMode.Air, FreightMode.LSE, container1.PK, "40GP", [ZGuid.Empty]
				);
			});
		}

		void AssertConvertsContainers(string message, TransportMode transportMode, FreightMode freightMode, ZGuid criteriaContainer, string serviceContainerCode, ZGuid[] expectedContainerPks)
		{
			var criteria = ValidCriteria;
			criteria.FreightMode = freightMode;

			if (!criteriaContainer.IsEmpty)
			{
				var testContainers = new TestContainers(Factory, criteriaContainer, 1);
				testContainers.PopulateContainerList(criteria.RateableMeasures);
			}

			var tradeService = CreateValidUldOrFclRate(transportMode, "QANT", serviceContainerCode);

			if (freightMode == FreightMode.LSE || freightMode == FreightMode.LCL)
			{
				tradeService.Container = null;
			}

			var converter = new UrsRatesConverter(Factory, Logger, criteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries([tradeService]) as IList<UrsRateEntry>;

			if (expectedContainerPks.IsNullOrEmpty())
			{
				Assert(message, entries.IsNullOrEmpty());
			}
			else
			{
				var actualContainerCodes = entries
					.Select(x => $"{x.TI_RC}:{x.UrsContainer?.IsMapped ?? false}")
					.ToList();

				var expectedMappedContainers = expectedContainerPks
					.Select(pk => $"{pk}:{!pk.IsEmpty}")
					.ToList();

				AssertContainsExactElementsInAnyOrder(message, expectedMappedContainers, actualContainerCodes);
			}
		}

		public void TestGetContainer_PayloadValues_MaxNet_Converted()
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "AAA");
			var testContainers = new TestContainers(Factory, container.PK, 1);

			var criteria = ValidCriteria;
			testContainers.PopulateContainerList(criteria.RateableMeasures);

			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			tradeService.Container = new ContainerDto()
			{
				IsoCode = "AAA",
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

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			var expectedEntry = new UrsRateEntry(tradeService, Factory)
			{
				TI_RC = container.PK,
				ContainerPayloadWeightOverride = 1m
			};

			AssertRatesShouldBeConvertedCorrectly(expectedEntry, entry, new[] { "TI_RC", "ContainerPayloadWeightOverride" }, Array.Empty<string>(), Array.Empty<string>());
		}

		public void TestGetContainer_PayloadValues_GivenMaxNetWeightUnitIsNotKilogram_ShouldBeLogged()
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "AAA");
			var testContainers = new TestContainers(Factory, container.PK, 1);
			var criteria = ValidCriteria;
			testContainers.PopulateContainerList(criteria.RateableMeasures);

			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			tradeService.Container = new ContainerDto()
			{
				IsoCode = "AAA",
				Weight = new ContainerWeightDto()
				{
					MaxNet = new UnitDto()
					{
						Quantity = 1,
						Unit = "Gram"
					},
					Pivot = new UnitDto() //Currently, this field is not used in UrsRateEntry.
					{
						Quantity = 2,
						Unit = "Gram"
					}
				}
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			var expectedEntry = new UrsRateEntry(tradeService, Factory)
			{
				TI_RC = container.PK,
				ContainerPayloadWeightOverride = 0m
			};

			AssertRatesShouldBeConvertedCorrectly(expectedEntry, entry, new[] { "TI_RC", "ContainerPayloadWeightOverride" }, Array.Empty<string>(), Array.Empty<string>());

			AssertCollectionContains("Rate Unit 'Gram' is not supported", Logger.Warnings);
		}

		public void TestGetContainer_PayloadValues_InsideVolume_Converted()
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "AAA");
			var testContainers = new TestContainers(Factory, container.PK, 1);
			var criteria = ValidCriteria;
			testContainers.PopulateContainerList(criteria.RateableMeasures);

			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			tradeService.Container = new ContainerDto()
			{
				IsoCode = "AAA",
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

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			var expectedEntry = new UrsRateEntry(tradeService, Factory)
			{
				TI_RC = container.PK,
				ContainerPayloadVolumeOverride = 10m
			};

			AssertRatesShouldBeConvertedCorrectly(expectedEntry, entry, new[] { "TI_RC", "ContainerPayloadVolumeOverride" }, Array.Empty<string>(), Array.Empty<string>());
		}

		public void TestGetContainer_PayloadValues_GivenInsideVolumeUnitIsNotCubicMeter_ShouldBeLogged()
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "AAA");
			var testContainers = new TestContainers(Factory, container.PK, 1);
			var criteria = ValidCriteria;
			testContainers.PopulateContainerList(criteria.RateableMeasures);

			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			tradeService.Container = new ContainerDto()
			{
				IsoCode = "AAA",
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

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			var expectedEntry = new UrsRateEntry(tradeService, Factory)
			{
				TI_RC = container.PK,
				ContainerPayloadVolumeOverride = 0m
			};

			AssertRatesShouldBeConvertedCorrectly(expectedEntry, entry, new[] { "TI_RC", "ContainerPayloadVolumeOverride" }, Array.Empty<string>(), Array.Empty<string>());

			AssertCollectionContains("Rate Unit 'CubicCentimeter' is not supported", Logger.Warnings);
		}

		#endregion

		#region Charge Universal code

		public void TestConvert_ShouldUseUniversalCode_WhenGrouping()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m);

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			AssertContainsExactElementsInAnyOrder(new[] { "FRT", "ABC" }, entries.SelectMany(c => c.ChildRateLines).Select(l => l.ChargeCode.AC_Code.ToString()));
		}

		public void TestConvert_WithUnmappedChargeCode()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m);

			var charges = ((BaseChargeDataDto)tradeService.PriceInfo.Charges);
			var charge = (BaseChargeDto)charges.Items.First();
			charge.Code = "CarrierCode";
			charge.Name = "CarrierDesc";
			((ChargeDefinitionDto)charge.ChargeDefinition).UniversalCode = "UniversalCode";
			((ChargeDefinitionDto)charge.ChargeDefinition).Description = "UniversalDesc";
			charges.Items = [charge];

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var entry = entries
				.SelectMany(entry => entry.ChildRateLines)
				.OfType<UrsRateLine>()
				.FirstOrDefault(line => line.UniversalChargeCode == "UniversalCode");
			AssertNotNull("Has unmapped charge code entry", entry);

			CombineAssertions(() =>
			{
				AssertEquals("CarrierCode", entry.CarrierChargeCode);
				AssertEquals("CarrierDesc", entry.CarrierChargeCodeDescription);
				AssertEquals("UniversalCode", entry.UniversalChargeCode);
				AssertEquals("UniversalDesc", entry.UniversalChargeCodeDescription);
				AssertNull(entry.ChargeCode);
			});
		}

		#endregion

		#region Rate Date

		public void TestConvert_RateStartAndEndDate()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var startDate = tradeService.PriceInfo.BaseRates.Items.First().VersionDateInfo.StartDate.ToZDate();
			var endDate = tradeService.PriceInfo.BaseRates.Items.First().VersionDateInfo.EndDate.ToZDate();

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals(startDate, entry.TI_RateStartDate);
			AssertEquals(endDate, entry.TI_RateEndDate);
		}

		#endregion

		#region Location

		public void TestConvert_OriginDestinationVia_IsValidUNLOCO_UseUNLOCOAsValue()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			var expectedEntry = new UrsRateEntry(tradeService, Factory)
			{
				TI_OriginLRC = "UAIEV",
				TI_ViaLRC = "AEDXB",
				TI_DestinationLRC = "AUSYD",
			};

			AssertRatesShouldBeConvertedCorrectly(expectedEntry, entry, new[] { "TI_OriginLRC", "TI_ViaLRC", "TI_DestinationLRC" }, Array.Empty<string>(), Array.Empty<string>());
		}

		public void TestConvert_OriginDestination_IsInvalidLocation_GenerateError()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");

			tradeService.RouteInfo = new RouteInfoDto()
			{
				Waypoints = new[]
				{
					new GeoScopeEntryDto() { WaypointType = RouteWaypointCode.Origin, Location = new LocationDto() { Code = "XXXXX", FunctionCode = LocationFunctionCode.Airport } },
					new GeoScopeEntryDto() { WaypointType = RouteWaypointCode.Via, Location = new LocationDto() { Code = "XXXXX", FunctionCode = LocationFunctionCode.Airport } },
					new GeoScopeEntryDto() { WaypointType = RouteWaypointCode.Destination, Location = new LocationDto() { Code = "XXXXX", FunctionCode = LocationFunctionCode.Airport } },
				}
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals(ZString.Empty, entry.TI_OriginLRC);
			AssertEquals(ZString.Empty, entry.TI_DestinationLRC);
			AssertEquals(ZString.Empty, entry.TI_ViaLRC);
			AssertContains("No Location matches origin 'XXXXX'", entry.Errors[RateEntrySchema.TI_OriginLRC]);
			AssertContains("No Location matches via 'XXXXX'", entry.Errors[RateEntrySchema.TI_ViaLRC]);
			AssertContains("No Location matches destination 'XXXXX'", entry.Errors[RateEntrySchema.TI_DestinationLRC]);
		}

		public void TestConvert_OriginDestination_IsEmpty_UseEmptyValue()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			tradeService.RouteInfo = new RouteInfoDto();

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals(ZString.Empty, entry.TI_OriginLRC);
			AssertEquals(ZString.Empty, entry.TI_DestinationLRC);
			AssertEquals(ZString.Empty, entry.TI_ViaLRC);
			Assert(entry.Errors.IsNullOrEmpty());
		}

		#endregion

		#region Service Level

		public void TestConvert_ServiceLevel_HasValue_UseMappedServiceLevelOnTheCarrier_Sea()
		{
			var serviceLevel = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XYZ";
			serviceLevel.PL_CarrierServiceCode = "ABC";

			Factory.Save();

			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");

			tradeService.VoyageInfo = new TradeServiceVoyageDto { OceanRoutingTerm = "abc" };

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals("XYZ", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());
		}

		public void TestConvert_ServiceLevel_HasValue_UseMappedServiceLevelOnTheCarrier_Air()
		{
			var serviceLevel = AirCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XYZ";
			serviceLevel.PL_CarrierServiceCode = "ABC";

			Factory.Save();

			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");

			tradeService.Product = new ProductDto()
			{
				Code = "XYZ",
				Name = "Product X",
				Classification = new ProductClassDto()
				{
					Code = "GEN",
					Name = "General"
				},
				ServiceLevel = new ProductServiceLevelDto()
				{
					Code = "abc"
				},
				UniversalCode = "HAZD",
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals("XYZ", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());
		}

		public void TestConvert_ServiceLevel_NoMapping_GenerateError()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");

			tradeService.VoyageInfo = new TradeServiceVoyageDto { OceanRoutingTerm = "lol" };

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals(ZString.Empty, entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.ContainsKey(RateEntrySchema.TI_PL_NKCarrierServiceLevel));
			AssertEquals("No Carrier Service Level under Carrier 'Carrier_SCAC' is assigned to 'LOL'", entry.Errors[RateEntrySchema.TI_PL_NKCarrierServiceLevel]);
		}

		public void TestConvert_ServiceLevel_SameCodeMappedTwice_GenerateError()
		{
			var serviceLevel1 = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "XYZ";
			serviceLevel1.PL_CarrierServiceCode = "ABC";

			var serviceLevel2 = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "TTT";
			serviceLevel2.PL_CarrierServiceCode = "ABC";

			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			tradeService.VoyageInfo = new TradeServiceVoyageDto { OceanRoutingTerm = "abc" };

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals(ZString.Empty, entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.ContainsKey(RateEntrySchema.TI_PL_NKCarrierServiceLevel));
			AssertEquals("Service Code 'ABC' under Carrier 'Carrier_SCAC' has been duplicated and must be unique.", entry.Errors[RateEntrySchema.TI_PL_NKCarrierServiceLevel]);
		}

		public void TestConvert_ServiceLevel_PreferProductCode()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			// When no product code then should match on CarrierServiceCode...
			AssertEquals("STD", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());

			tradeService.Product = new ProductDto()
			{
				Code = "PREMIUM",
				Name = "Product X",
				Classification = new ProductClassDto()
				{
					Code = "GEN",
					Name = "General"
				},
				ServiceLevel = new ProductServiceLevelDto()
				{
					Code = "std"
				},
				UniversalCode = "HAZD",
			};

			var serviceLevel1 = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "XYZ";
			serviceLevel1.PL_CarrierServiceCode = "ABC";
			serviceLevel1.PL_ProductCode = "PREMIUM";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Express";
			entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			// When rate has product code that matches org service level product then take that match
			AssertEquals("XYZ", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());

			tradeService.Product = new ProductDto()
			{
				Code = "ABCDE",
				Name = "Product X",
				Classification = new ProductClassDto()
				{
					Code = "GEN",
					Name = "General"
				},
				ServiceLevel = new ProductServiceLevelDto()
				{
					Code = "std"
				},
				UniversalCode = "HAZD",
			};

			entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;
			// When rate has product code that does NOT match org service level product then fallback to universal code match
			AssertEquals("STD", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());
		}

		public void TestConvert_ServiceLevel_PreferCommaSeparatedProductCode()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			// When no product code then should match on CarrierServiceCode...
			AssertEquals("STD", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());

			tradeService.Product = new ProductDto()
			{
				Code = "PR",
				Name = "Product X",
				Classification = new ProductClassDto()
				{
					Code = "GEN",
					Name = "General"
				},
				ServiceLevel = new ProductServiceLevelDto()
				{
					Code = "std"
				},
				UniversalCode = "HAZD",
			};

			var serviceLevel1 = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "XYZ";
			serviceLevel1.PL_CarrierServiceCode = "ABC";
			serviceLevel1.PL_ProductCode = "P.EX,PR,EX";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Express";
			entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			// When rate has product code that matches org service level product then take that match
			AssertEquals("XYZ", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());

			tradeService.Product = new ProductDto()
			{
				Code = "ABCDE",
				Name = "Product X",
				Classification = new ProductClassDto()
				{
					Code = "GEN",
					Name = "General"
				},
				ServiceLevel = new ProductServiceLevelDto()
				{
					Code = "std"
				},
				UniversalCode = "HAZD",
			};

			entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			// When rate has product code that does NOT match org service level product then fallback to universal code match
			AssertEquals("STD", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());
		}

		public void TestConvert_ServiceLevel_OneUnmappedLocalCodeSameUniversalCode_AnotherLocalCodeMappedToUniversalCode_PreferMappedOne()
		{
			var serviceLevelToBeSkipped = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevelToBeSkipped.PL_Code = "XYZ";                        // => same code
			serviceLevelToBeSkipped.PL_CarrierServiceCode = ZString.Empty;  // => unmapped
			serviceLevelToBeSkipped.PL_CarrierServiceLevelDescription = "This code does not match";

			var serviceLevelToBeMatched = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevelToBeMatched.PL_Code = "ABC";                        // => different code
			serviceLevelToBeMatched.PL_CarrierServiceCode = "XYZ";          // => mapped
			serviceLevelToBeMatched.PL_CarrierServiceLevelDescription = "This code should match";

			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			tradeService.Product = new ProductDto()
			{
				Code = "ABCDE",
				Name = "Product X",
				Classification = new ProductClassDto()
				{
					Code = "GEN",
					Name = "General"
				},
				ServiceLevel = new ProductServiceLevelDto()
				{
					Code = "xyz"
				},
				UniversalCode = "HAZD",
			};
			tradeService.VoyageInfo = new TradeServiceVoyageDto { OceanRoutingTerm = "xyz" };

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals("when matching rate's Service Level, PL_CarrierServiceCode is preferred than PL_Code", "ABC", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());
		}

		public void TestConvert_ServiceLevel_WhenUnmappedLocalCodeMatchUniversalCode_UseLocalCode()
		{
			var serviceLevel = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XYZ";                                   // => same code
			serviceLevel.PL_CarrierServiceCode = ZString.Empty;             // => unmapped

			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");

			tradeService.Product = new ProductDto()
			{
				Code = "ABCDE",
				Name = "Product X",
				Classification = new ProductClassDto()
				{
					Code = "GEN",
					Name = "General"
				},
				ServiceLevel = new ProductServiceLevelDto()
				{
					Code = "xyz"
				},
				UniversalCode = "HAZD",
			};
			tradeService.VoyageInfo = new TradeServiceVoyageDto { OceanRoutingTerm = "xyz" };

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals("PL_Code should match rate's Service Level", "XYZ", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());
		}

		public void TestConvert_ServiceLevel_IsStandard_WhenMapped_UseMappedValue()
		{
			var serviceLevel = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XYZ";
			serviceLevel.PL_CarrierServiceCode = OrgCarrierServiceLevel.StandardCode;

			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals("XYZ", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());
		}

		public void TestConvert_ServiceLevel_IsStandard_WhenMappedWithCsv_UseMappedValue()
		{
			var serviceLevel = SeaCarrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XYZ";
			serviceLevel.PL_CarrierServiceCode = "A,B";

			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			tradeService.Product = new ProductDto()
			{
				Code = "ABCDE",
				Name = "Product X",
				Classification = new ProductClassDto()
				{
					Code = "GEN",
					Name = "General"
				},
				ServiceLevel = new ProductServiceLevelDto()
				{
					Code = "std"
				},
				UniversalCode = "HAZD",
			};
			tradeService.VoyageInfo = new TradeServiceVoyageDto { OceanRoutingTerm = "b" };

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;

			AssertEquals("XYZ", entry.TI_PL_NKCarrierServiceLevel);
			Assert(entry.Errors.IsNullOrEmpty());
		}

		#endregion

		#region Currency

		public void TestConvert_CertainApplicabilities_RequireCurrency()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria, "TestID");

			TestApplicability(RateApplicableCode.UnitPrice, expectedError: true);
			TestApplicability(RateApplicableCode.Nihil, expectedError: true);
			TestApplicability(RateApplicableCode.Zero, expectedError: true);
			TestApplicability(RateApplicableCode.Fixed, expectedError: true);
			TestApplicability(RateApplicableCode.Free, expectedError: true);
			TestApplicability(RateApplicableCode.Tact, expectedError: true);

			TestApplicability(RateApplicableCode.OnRequest, expectedError: false);
			TestApplicability(RateApplicableCode.Iata, expectedError: false);
			TestApplicability(RateApplicableCode.Percentage, expectedError: false);
			TestApplicability(RateApplicableCode.Vatos, expectedError: false);
			TestApplicability(RateApplicableCode.AllIn, expectedError: false);
			TestApplicability(RateApplicableCode.NotApplicable, expectedError: false);

			void TestApplicability(string applicableCode, bool expectedError)
			{
				SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m, applicable: applicableCode, currency: null);
				var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

				if (expectedError)
				{
					AssertEquals("[TraceID: TestID] The charge \"ABCLocal - ABC Name - No Charge ID (likely freight charge)\" has no currency for RateCollections.Items[0].Currency", ErrorReporter.LastMessageReported);
				}
				else
				{
					AssertEquals(2, entries.Count());
					Assert("Should be no error reports", ErrorReporter.ExceptionsThrown.IsNullOrEmpty());
				}
				ErrorReporter.Clear();
			}
		}

		public void TestConvert_Charge_ShouldHaveCurrency_WithOrWithoutCurrency()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var baseRates = tradeService.PriceInfo.BaseRates.Items.First() as RateCollectionDto;
			baseRates.Currency = ZString.Empty;

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria, "TestID");
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			AssertEquals("[TraceID: TestID] The charge \"FRT - Freight - No Charge ID (likely freight charge)\" has no currency for RateCollections.Items[0].Currency", ErrorReporter.LastMessageReported);
			Assert("Freight charge error should stop evaluation of trade service completely", result.IsNullOrEmpty());
			AssertContainsExactElementsInAnyOrder(["The charge \"FRT - Freight - No Charge ID (likely freight charge)\" has no currency for RateCollections.Items[0].Currency"], Logger.Warnings);
			Logger.ClearLogs();
			ErrorReporter.Clear();

			baseRates.Currency = "USD";
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).FirstOrDefault() as UrsRateEntry;
			var rateLine = entry.ChildRateLines.First() as WiseLine;

			Assert("Should be no error reports", ErrorReporter.ExceptionsThrown.IsNullOrEmpty());
			AssertEquals("USD", rateLine.TL_RX_NKCurrency);
			Assert("Should be no errors on the rateline", rateLine.Errors.IsNullOrEmpty());
			Assert("Should be no warnings", Logger.Warnings.IsNullOrEmpty());
		}

		public void TestConvert_Currency_IsInvalidCurrency()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var baseRates = tradeService.PriceInfo.BaseRates.Items.First() as RateCollectionDto;
			baseRates.Currency = "SSA";

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			Assert("Freight charge error should stop evaluation of trade service completely", result.IsNullOrEmpty());
			AssertContainsExactElementsInAnyOrder(["No Currency is assigned with or has the same Code as 'SSA'"], Logger.Warnings);
		}

		public void TestConvert_Currency_IsInvalidCurrency_NonFrtCharge()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m, RateApplicableCode.UnitPrice, currency: "SSA");

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).ElementAt(1) as UrsRateEntry;

			Assert("Should have no lines, invalid currency line ignored", entry.ChildRateLines.IsNullOrEmpty());
			AssertContainsExactElementsInAnyOrder(["Charge ABCLocal was ignored because No Currency is assigned with or has the same Code as 'SSA'"], Logger.Warnings);
		}

		public void TestConvert_Currency_DontNeedCurrency()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 0m, RateApplicableCode.Vatos);

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			AssertEquals(2, entries.Count());
			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG);
			var rateLine = originRateEntry.ChildRateLines.First() as WiseLine;

			AssertEquals(ZString.Empty, rateLine.TL_RX_NKCurrency);
			Assert(rateLine.Errors.IsNullOrEmpty());
		}

		public void TestConvert_Charge_Restricted_TACTShouldConvertCurrency()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m, RateApplicableCode.Tact, currency: "USD");

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG);
			var rateLine = originRateEntry.ChildRateLines.First() as WiseLine;

			Assert("Should be no error reports", ErrorReporter.ExceptionsThrown.IsNullOrEmpty());
			AssertEquals("USD", rateLine.TL_RX_NKCurrency);
			Assert(rateLine.Errors.IsNullOrEmpty());

			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m, RateApplicableCode.Tact, currency: "");
			entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			Assert("Should have error reports", !ErrorReporter.ExceptionsThrown.IsNullOrEmpty());
			ErrorReporter.Clear();
		}

		public void TestConvert_Currency_NoPriceEntries()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var baseRates = tradeService.PriceInfo.BaseRates.Items.First() as RateCollectionDto;
			baseRates.PriceEntries = [];
			baseRates.Currency = "USD";

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			Assert("Trade service conversion should be cancelled", result.IsNullOrEmpty());
		}

		#endregion

		#region Composed Trade Services

		public void TestConvert_ComposedTradeService()
		{
			var firstLeg = CreateValidUldOrFclRate(TransportMode.Sea, "QANT");
			firstLeg
				.CreateRoute()
				.AddWaypoint("USCLT", RouteWaypointCode.Origin)
				.AddWaypoint("USCHS", RouteWaypointCode.PortOfLoading);

			var secondLeg = CreateValidUldOrFclRate(TransportMode.Sea, "QANT");
			secondLeg
				.CreateRoute()
				.AddWaypoint("USCHS", RouteWaypointCode.PortOfLoading)
				.AddWaypoint("BEANR", RouteWaypointCode.Destination);

			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT");
			tradeService
				.CreateRoute()
				.AddWaypoint("USCLT", RouteWaypointCode.Origin)
				.AddWaypoint("USCHS", RouteWaypointCode.PortOfLoading)
				.AddWaypoint("BEANR", RouteWaypointCode.Destination);
			(tradeService.PriceInfo as PriceInfoDto).AddChargeWithSinglePriceEntry(new("ODOC"));
			tradeService.TradeServices = [firstLeg, secondLeg];

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries([tradeService]);

			CombineAssertions(() =>
			{
				AssertEquals("Creates 2 rate entries", 2, entries.Count());
				AssertContainsExactElementsInAnyOrder(
					"Creates FRT rate line per nested trade service",
					["FRT, FRT", "ODOC"],
					entries.Select(entry => string.Join(", ", entry.ChildRateLines.Select(l => l.ChargeCode.AC_Code)))
				);
			});
		}

		#endregion

		#region Inclusive Rates

		public void TestConvertSeaFclRates_InclusiveCharges()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m);

			((RateCollectionDataDto)tradeService.PriceInfo.BaseRates).Inclusive = new[]
			{
				new ChargeDefinitionDto
				{
					Code = "FSCLocal",
					UniversalCode = "FSC",
					Description = "Fuel Surcharge"
				}
			};

			((RateCollectionDataDto)tradeService.PriceInfo.Charges.Items.First().RateCollections).Inclusive = new[]
			{
				new ChargeDefinitionDto {
					Code = "EEELocal",
					UniversalCode = "EEE",
					Description = "Other inclusive charge"
				}
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			AssertEquals(2, entries.Count());

			var freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);
			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG);

			var actualLines = freightRateEntry.ChildRateLines
			.Select(l => new
			{
				Calculator = (string)l.TL_RateCalculator,
				Code = (string)l.ChargeCode.AC_Code,
				ChargeType = (string)l.ChargeType,
				IncludedInChargeCodePK = l.ChildRateLineItems
					.FirstOrDefault(i => i.TM_Type == FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType)?.TM_AC ?? ZGuid.Empty
			})
			.ToList();

			var expectedLines = new[]
			{
				new { Calculator = UnitCalculator.Code, Code = "FRT", ChargeType = "Freight", IncludedInChargeCodePK = ZGuid.Empty },
				new { Calculator = FreightInclusiveCalculator.Code, Code = "FSC", ChargeType = "Included", IncludedInChargeCodePK = Helper.ChargeCodes["FRT"].PK },
			};
			AssertContainsExactElementsInExactOrder(expectedLines, actualLines);

			actualLines = originRateEntry.ChildRateLines
			.Select(l => new
			{
				Calculator = (string)l.TL_RateCalculator,
				Code = (string)l.ChargeCode.AC_Code,
				ChargeType = (string)l.ChargeType,
				IncludedInChargeCodePK = l.ChildRateLineItems
					.FirstOrDefault(i => i.TM_Type == FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType)?.TM_AC ?? ZGuid.Empty
			})
			.ToList();

			expectedLines = new[]
			{
				new { Calculator = FlatCalculator.Code, Code = "ABC", ChargeType = "BOL", IncludedInChargeCodePK = ZGuid.Empty },
				new { Calculator = FreightInclusiveCalculator.Code, Code = "EEE", ChargeType = "Included", IncludedInChargeCodePK = Helper.ChargeCodes["ABC"].PK },
			};
			AssertContainsExactElementsInExactOrder(expectedLines, actualLines);
		}

		#endregion

		#region Rate Category

		public void TestConvert_RateCategorySuccessfully()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m);

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			AssertEquals(2, entries.Count());

			var freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;
			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG) as UrsRateEntry;

			AssertNotNull(freightRateEntry);
			AssertNotNull(originRateEntry);
			AssertEquals(0, freightRateEntry.Errors.Count);
			AssertEquals(0, originRateEntry.Errors.Count);
		}

		public void TestConvert_ChargeCodeRateCategory_CannotMap()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "XXX", 100m);

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			AssertEquals(2, entries.Count());

			var freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;

			var rateEntryWithEmptyRateCategory = entries.FirstOrDefault(entry => entry.TI_RateCategory != RateCategory.FCL) as UrsRateEntry;
			AssertEquals(ZString.Empty, rateEntryWithEmptyRateCategory.TI_RateCategory);
			AssertContains("Category cannot be identified as the Charge(s) under the Rate cannot be converted into CW1 Charges. Please check validation errors of the Rate line.", rateEntryWithEmptyRateCategory.Errors[RateEntrySchema.TI_RateCategory]);

			var rateline = rateEntryWithEmptyRateCategory.ChildRateLines.First() as WiseLine;
			AssertEquals(ZGuid.Empty, rateline.TL_AC);
			AssertContains("No Charge Code is assigned with or has the same Code as Universal 'XXX'.", rateline.Errors[RateLinesSchema.TL_AC]);
		}

		#endregion

		public void TestConvert_ContractNumber_Ocean()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT");
			var contract = (ContractDto)tradeService.Contract;
			contract.Header = new ContractHeaderDto { Reference = "reference", Name = "Contract Name" };

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = entries.FirstOrDefault() as UrsRateEntry;

			AssertEquals("Contract Name", freightRateEntry.TI_ContractNumber);
		}

		public void TestConvert_ContractNumber_Air()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "QANT");
			tradeService.ExternalReference = "abcd";
			tradeService.ServiceClass = ServiceClassCode.Contract;

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = entries.FirstOrDefault() as UrsRateEntry;

			AssertEquals("abcd", freightRateEntry.TI_ContractNumber);
		}

		public void TestConvert_ProductName_Ocean()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT");

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = entries.FirstOrDefault() as UrsRateEntry;

			AssertEquals("Group one", freightRateEntry.ProductName);
		}

		public void TestConvert_ProductName_Air()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "QANT");

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = entries.FirstOrDefault() as UrsRateEntry;

			AssertEquals("Product X", freightRateEntry.ProductName);
		}

		public void TestConvert_CommodityGroup_Ocean()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT");

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = entries.FirstOrDefault() as UrsRateEntry;

			AssertEquals(ZString.Empty, freightRateEntry.CommodityGroup);
		}

		public void TestConvert_CommodityGroup_Air()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "QANT");

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = entries.FirstOrDefault() as UrsRateEntry;

			AssertEquals("HAZD", freightRateEntry.CommodityGroup);
		}

		public void TestConvert_WhenRateHasNoChargeInfo()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m);

			var originCharge = tradeService.PriceInfo.Charges.Items.First() as BaseChargeDto;
			originCharge.RateCollections = null;

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			AssertEquals(2, entries.Count());

			var freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;
			AssertEquals(1, freightRateEntry.ChildRateLines.Count());

			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory != RateCategory.FCL) as UrsRateEntry;
			AssertEquals(0, originRateEntry.ChildRateLines.Count());

			AssertCollectionContains("Charge ABCLocal was ignored because The charge 'ABC - ABC Name' has no rate information", Logger.Warnings);
		}

		public void TestConvert_WhenChargeHasNoUniversalCode()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m);

			var originCharge = tradeService.PriceInfo.Charges.Items.First() as BaseChargeDto;
			originCharge.ChargeDefinition = null;

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			AssertEquals(2, entries.Count());

			var freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;
			AssertEquals(1, freightRateEntry.ChildRateLines.Count());

			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory != RateCategory.FCL) as UrsRateEntry;
			AssertEquals(0, originRateEntry.ChildRateLines.Count());

			AssertCollectionContains("Charge ABCLocal was ignored because The charge 'ABCLocal - ABC Name' has no universal code", Logger.Warnings);
		}

		public void TestConvert_CarrierChargeCodeInfo()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m);

			var originCharge = tradeService.PriceInfo.Charges.Items.First() as BaseChargeDto;
			AssertEquals("ABCLocal", originCharge.ChargeDefinition.Code);
			AssertEquals("ABC", originCharge.ChargeDefinition.UniversalCode);
			AssertEquals("ABC Name", originCharge.Name);

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;

			var rateLine = freightRateEntry.ChildRateLines.First() as WiseLine;
			AssertEquals("FRT", rateLine.CarrierChargeCode);
			AssertEquals("Freight", rateLine.CarrierChargeCodeDescription);

			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory != RateCategory.FCL) as UrsRateEntry;

			rateLine = originRateEntry.ChildRateLines.First() as WiseLine;
			AssertEquals("ABCLocal", rateLine.CarrierChargeCode);
			AssertEquals("ABC Name", rateLine.CarrierChargeCodeDescription);
		}

		#region Rate Mode

		public void TestConvertRateModeAndCategory()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradeService, "ABC", 100m);

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;
			AssertEquals("SEA", freightRateEntry.TI_Mode);

			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG) as UrsRateEntry;
			AssertEquals("FCL", originRateEntry.TI_Mode);

			tradeService.Container = null;

			entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			freightRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.LCL) as UrsRateEntry;
			AssertEquals("LCL", freightRateEntry.TI_Mode);

			originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG) as UrsRateEntry;
			AssertEquals("LCL", originRateEntry.TI_Mode);
		}

		#endregion

		#region Payment term

		public void TestConvertPaymentTerm()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var frtCharge = tradeService.PriceInfo.BaseRates.Items.First() as RateCollectionDto;
			frtCharge.PaymentTerm = PaymentTermCode.Prepaid;

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).First() as UrsRateEntry;
			AssertEquals("PPD", entry.TI_PaymentTerm);

			frtCharge.PaymentTerm = PaymentTermCode.Collect;
			entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).First() as UrsRateEntry;
			AssertEquals("CCX", entry.TI_PaymentTerm);
		}

		#endregion

		#region Calculator

		#region Percentage Calculator

		public void TestConvert_PercentageCharge_NoCurrency_FallbackToFreightCharge()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargeAsPercentageOfBaseFreight(tradeService, "ABC", 2m, currency: null);

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			AssertEquals(2, result.Count());
			var originRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG);

			var expectedOriginRateEntry = new UrsRateEntry(tradeService, Factory);
			var wiseline = CreateUrsLine("ABC", "USD", PercentageCalculator.Code);

			wiseline.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline, CalculatorConstants.Type.PER, string.Empty, 2m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Type.ApplyTo, CalculatorConstants.Text.ChargeCode, 2m, "FRT", ZDecimal.Zero, 0, false),

				new WiseLineItem(wiseline, CalculatorConstants.Type.ValueOrPartThereOf, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.BAS, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.MAX, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.MIN, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Type.Rate, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Text.IncludeGST, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Text.GreaterCharge, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Text.PER_PartThereof, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
			};

			expectedOriginRateEntry.ChildRateLines = new[] { wiseline };

			AssertRatesShouldBeConvertedCorrectly(expectedOriginRateEntry, (UrsRateEntry)originRateEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue" });
		}

		public void TestConvert_PercentageCharge_WithCurrency()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargeAsPercentageOfBaseFreight(tradeService, "ABC", 2m, currency: "EUR");

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			AssertEquals(2, result.Count());
			var originRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG);

			var expectedOriginRateEntry = new UrsRateEntry(tradeService, Factory);
			var wiseline = CreateUrsLine("ABC", "EUR", PercentageCalculator.Code);

			wiseline.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline, CalculatorConstants.Type.PER, string.Empty, 2m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Type.ApplyTo, CalculatorConstants.Text.ChargeCode, 2m, "FRT", ZDecimal.Zero, 0, false),

				new WiseLineItem(wiseline, CalculatorConstants.Type.ValueOrPartThereOf, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.BAS, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.MAX, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.MIN, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Type.Rate, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Text.IncludeGST, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Text.GreaterCharge, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, CalculatorConstants.Text.PER_PartThereof, string.Empty, 0m, string.Empty, ZDecimal.Zero, 0, false),
			};

			expectedOriginRateEntry.ChildRateLines = new[] { wiseline };

			AssertRatesShouldBeConvertedCorrectly(expectedOriginRateEntry, (UrsRateEntry)originRateEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue" });
		}

		#endregion

		#region MPU calculator

		public void TestChargesWithMinAndUntRates_ShouldConvertToMPUCalculator()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradeService.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries = new[]
			{
				CreateSimpleBaseItem(30, "Min", "Upr", "Shipment"),
				CreateSimpleBaseItem(0.5m, "Flat", "Upr", "Kg", roudingMode: 0.01m)
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);

			var expectedFreightRateEntry = new UrsRateEntry(tradeService, Factory);
			var wiseline = CreateUrsLine("FRT", "USD", "MPU");
			wiseline.TL_Rounding = (ZString)RatingRoundingTypes.Custom;
			wiseline.TL_RoundingFactor = 0.01m;
			wiseline.TL_WeightVolume = "KG";
			wiseline.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline, Calculator.Items.Operator.MIN, string.Empty, 30, ZString.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.UNT, string.Empty, 0.5m, ZString.Empty, ZDecimal.Zero, 0, false)
			};

			expectedFreightRateEntry.ChildRateLines = new[] { wiseline };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "TL_Rounding", "TL_RoundingFactor", "TL_WeightVolume", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue" });
		}

		#endregion

		#region FPU calculator

		public void TestChargesWithBaseAndUntRates_ShouldConvertToFPUCalculator()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradeService.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries = new[]
			{
				CreateSimpleBaseItem(30, "Base", "Upr", "Shipment"),
				CreateSimpleBaseItem(0.5m, "Flat", "Upr", "Kg", roudingMode: 0.01m)
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);

			var expectedFreightRateEntry = new UrsRateEntry(tradeService, Factory);
			var wiseline = CreateUrsLine("FRT", "USD", "FPU");
			wiseline.TL_Rounding = (ZString)RatingRoundingTypes.Custom;
			wiseline.TL_RoundingFactor = 0.01m;
			wiseline.TL_WeightVolume = "KG";
			wiseline.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline, Calculator.Items.Operator.BAS, string.Empty, 30, ZString.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.UNT, string.Empty, 0.5m, ZString.Empty, ZDecimal.Zero, 0, false)
			};

			expectedFreightRateEntry.ChildRateLines = new[] { wiseline };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "TL_Rounding", "TL_RoundingFactor", "TL_WeightVolume", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue" });
		}

		#endregion

		#region UNT calculator

		public void TestChargesWithUntRates_ShouldConvertToUNTCalculator()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradeService.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries = new[]
			{
				CreateSimpleBaseItem(0.5m, "Flat", "Upr",  UnitOfMeasurementCode.Kilogram, roudingMode: 0.01m)
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);

			var expectedFreightRateEntry = new UrsRateEntry(tradeService, Factory);
			var wiseline = CreateUrsLine("FRT", "USD", "UNT");
			wiseline.TL_Rounding = (ZString)RatingRoundingTypes.Custom;
			wiseline.TL_RoundingFactor = 0.01m;
			wiseline.TL_WeightVolume = "KG";
			wiseline.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline, Calculator.Items.Operator.UNT, string.Empty, 0.5m, ZString.Empty, ZDecimal.Zero, 0, false)
			};

			expectedFreightRateEntry.ChildRateLines = new[] { wiseline };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "TL_Rounding", "TL_RoundingFactor", "TL_WeightVolume", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue" });
		}

		#endregion

		#region FLT calculator

		public void TestChargesWithFlatRates_WithUntApplicable_ShouldConvertToFlatCalculator() =>
			TestChargesWithFlatRates_ShouldConvertToFlatCalculator(RateApplicableCode.UnitPrice);

		public void TestChargesWithFlatRates_WithZeroApplicable_ShouldConvertToFlatCalculator() =>
			TestChargesWithFlatRates_ShouldConvertToFlatCalculator(RateApplicableCode.Zero);

		void TestChargesWithFlatRates_ShouldConvertToFlatCalculator(string applicable)
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradeService.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries = new[] { CreateSimpleBaseItem(30m, "Flat", applicable, "Mawb") };

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);

			var expectedFreightRateEntry = new UrsRateEntry(tradeService, Factory);
			var wiseline = CreateUrsLine("FRT", "USD", "FLT");
			wiseline.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline, Calculator.Items.Operator.BAS, string.Empty, 30m, ZString.Empty, ZDecimal.Zero, 0, false)
			};
			expectedFreightRateEntry.ChildRateLines = new[] { wiseline };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue" });
		}

		#endregion

		#region CMB Calculator

		public void TestGivenUniversalRateInAirMode_WhenConvertToWiseEntries_ThenIsHigherBreakLowerRateShouldBeTrue()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
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
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR);

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			var wiseline1 = CreateUrsLine("FRT", "USD", CombinedCalculator.Code);
			wiseline1.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline1, Calculator.Items.Operator.MIN, string.Empty, 100m, ZString.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1.2m, ZString.Empty, 1, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1.1, ZString.Empty, 100, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1, ZString.Empty, 500, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseAccumulated, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseInclusiveBreaks, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				// Expected to be Y for Air Combined calculator
				new WiseLineItem(wiseline1, Calculator.Items.HigherChargeableLowerRate, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.BreaksPer, "", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
			};

			expectedFreightRateEntry.ChildRateLines = new[] { wiseline1 };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue", "TM_Break" });
		}

		public void TestConvert_Cargoguide_BreakPlus()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			// +1, +100, +500
			frtRate.PriceEntries =
			[
				CreateSimpleWeightBreakPlus(1, 1.2m),
				CreateSimpleWeightBreakPlus(100, 1.1m),
				CreateSimpleWeightBreakPlus(500, 1m),
			];
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries([tradelane]);
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR);

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			var wiseline1 = CreateUrsLine("FRT", "USD", CombinedCalculator.Code);
			wiseline1.ChildRateLineItems =
			[
				new WiseLineItem(wiseline1, Calculator.Items.Operator.MIN, string.Empty, 0, ZString.Empty, 1, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1.2m, ZString.Empty, 1, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1.1, ZString.Empty, 100, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1, ZString.Empty, 500, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseAccumulated, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseInclusiveBreaks, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.HigherChargeableLowerRate, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.BreaksPer, "", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
			];

			expectedFreightRateEntry.ChildRateLines = [wiseline1];

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, Array.Empty<string>(), ["TL_AC", "TL_RateCalculator", "ChildRateLineItems"], ["TM_Type", "TM_Text", "TM_RelevantValue", "TM_Break"]);
		}

		public void TestConvert_Cargoguide_BreakLess()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			// -100, +100, +500
			frtRate.PriceEntries =
			[
				CreateSimpleWeightBreakLess(100, 1.2m),
				CreateSimpleWeightBreakPlus(100, 1.1m),
				CreateSimpleWeightBreakPlus(500, 1m),
			];
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries([tradelane]);
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR);

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			var wiseline1 = CreateUrsLine("FRT", "USD", CombinedCalculator.Code);
			wiseline1.ChildRateLineItems =
			[
				// Should not add a MIN line item for Cargoguide
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Minus, string.Empty, 1.2m, ZString.Empty, 100, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1.1, ZString.Empty, 100, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1, ZString.Empty, 500, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseAccumulated, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseInclusiveBreaks, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.HigherChargeableLowerRate, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.BreaksPer, "", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
			];

			expectedFreightRateEntry.ChildRateLines = [wiseline1];

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, Array.Empty<string>(), ["TL_AC", "TL_RateCalculator", "ChildRateLineItems"], ["TM_Type", "TM_Text", "TM_RelevantValue", "TM_Break"]);
		}

		public void TestConvert_Min_BreakPlus()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
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
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			var wiseline1 = CreateUrsLine("FRT", "USD", CombinedCalculator.Code);
			wiseline1.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline1, Calculator.Items.Operator.MIN, string.Empty, 100m, ZString.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1.2m, ZString.Empty, 1, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1.1, ZString.Empty, 100, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 1, ZString.Empty, 500, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseAccumulated, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseInclusiveBreaks, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.HigherChargeableLowerRate, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.BreaksPer, "", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
			};

			expectedFreightRateEntry.ChildRateLines = new[] { wiseline1 };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue", "TM_Break" });
		}

		public void TestConvert_Min_BreakLess_BreakPlus()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			// min=1, -45, +45, +100, +250, +300
			frtRate.PriceEntries = new[]
			{
				CreateShipmentMinBreakItem(100m),
				CreateSimpleWeightBreakLess(45, 6.5m),
				CreateSimpleWeightBreakPlus(45, 6.4m),
				CreateSimpleWeightBreakPlus(250, 6.2m),
				CreateSimpleWeightBreakPlus(100, 6.3m),
				CreateSimpleWeightBreakPlus(300, 6.1m),
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			var wiseline1 = CreateUrsLine("FRT", "USD", CombinedCalculator.Code);
			wiseline1.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline1, Calculator.Items.Operator.MIN, string.Empty, 100m, ZString.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Minus, string.Empty, 6.5m, ZString.Empty, 45, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 6.4m, ZString.Empty, 45, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 6.3m, ZString.Empty, 100, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 6.2m, ZString.Empty, 250, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 6.1m, ZString.Empty, 300, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseAccumulated, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseInclusiveBreaks, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.HigherChargeableLowerRate, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.BreaksPer, "", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
			};

			expectedFreightRateEntry.ChildRateLines = new[] { wiseline1 };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue", "TM_Break", "TM_FlatAmount" });
		}

		public void TestConvert_ChargesWithBreaksAndMinMaxUnitBase()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries = new[]
			{
				CreateShipmentMinBreakItem(1000m),
				CreateShipmentMaxBreakItem(123456m),
				CreateSimpleWeightBreakItem(RateBreakTypeCode.Flat, 1, 1000m, RateApplicableCode.UnitPrice),
				CreateSimpleBaseItem(100m, RateBreakTypeCode.Base),
				CreateSimpleWeightBreakPlus(1, 11m),
				CreateSimpleWeightBreakPlus(100, 10m),
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			var wiseline1 = CreateUrsLine("FRT", "USD", CombinedCalculator.Code);
			wiseline1.ChildRateLineItems = [
				new WiseLineItem(wiseline1, Calculator.Items.Operator.MIN, string.Empty, 1000m, ZString.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.MAX, string.Empty, 123456m, ZString.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.UNT, string.Empty, 1000m, ZString.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.BAS, string.Empty, 100m, ZString.Empty, ZDecimal.Zero, 0, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 11m, ZString.Empty, 1, 100, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 10m, ZString.Empty, 100, 100, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseAccumulated, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseInclusiveBreaks, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.HigherChargeableLowerRate, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.BreaksPer, "", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
			];

			expectedFreightRateEntry.ChildRateLines = new[] { wiseline1 };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue", "TM_Break", "TM_FlatAmount" });
		}

		public void TestConvert_PivotCharges()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries = new[]
			{
				CreateSimpleBaseItem(1000m, RateBreakTypeCode.Base, RateApplicableCode.UnitPrice, UnitOfMeasurementCode.Container),
				CreateSimplePivotBreakPlus(200m, 10m)
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR);

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			var wiseline1 = CreateUrsLine("FRT", "USD", CombinedCalculator.Code);
			wiseline1.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Minus, string.Empty, 0, ZString.Empty, 200m, 1000m, false),
				new WiseLineItem(wiseline1, Calculator.Items.Operator.Plus, string.Empty, 10m, ZString.Empty, 200m, 1000m, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseAccumulated, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.UseInclusiveBreaks, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.MultipleEquipmentsOverMaxWeightVolume, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.HigherChargeableLowerRate, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline1, Calculator.Items.BreaksPer, "", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
			};

			expectedFreightRateEntry.ChildRateLines = new[] { wiseline1 };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue", "TM_Break", "TM_FlatAmount" });
		}

		public void TestConvert_RestrictedReference_ORRates()
		{
			// RateApplicableCode.OnRequest, "OR – On Request at Carrier"
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "AC", "AAA");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			frtRate.PriceEntries =
			[
				CreateSimpleWeightBreakLess(45, 0m, RateApplicableCode.OnRequest),
				CreateSimpleWeightBreakPlus(45, 0m, RateApplicableCode.OnRequest),
				CreateSimpleWeightBreakPlus(100, 0m, RateApplicableCode.OnRequest),
				CreateSimpleWeightBreakPlus(250, 0m, RateApplicableCode.OnRequest),
				CreateSimpleWeightBreakPlus(300, 0m, RateApplicableCode.OnRequest)
			];

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries([tradelane]);
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);

			var rateLine = freightRateEntry.ChildRateLines.Single();
			Assert("Rate line should be restricted and its items contain the reason for it",
				rateLine.ChildRateLineItems
					.Where(item => item.TM_Type == "+" || item.TM_Type == "-")
					.All(item => item.TM_CallForPricing && item.TM_Text == "OR – On Request at Carrier"));
		}

		public void TestConvert_VATOSChargeWithoutCurrency()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "AC", "AAA");
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

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries([tradelane]);
			var freightRateEntry = result.Single(e => e.TI_RateCategory == "FCL");
			var freightChargeCodePk = freightRateEntry.ChildRateLines.Single().TL_AC;

			var destinationRateEntry = result.Single(e => e.TI_RateCategory == "DST");
			var freightInclusiveRateLine = destinationRateEntry.ChildRateLines.Single();

			AssertEquals(FreightInclusiveCalculator.Code, freightInclusiveRateLine.TL_RateCalculator);
			AssertEquals("The calculator should accept empty currency", ZString.Empty, freightInclusiveRateLine.TL_RX_NKCurrency);

			var rateLineItemWithChargeCode = freightInclusiveRateLine.ChildRateLineItems.Single(x => !x.TM_AC.IsEmpty);
			AssertEquals("Reference charge", freightChargeCodePk, rateLineItemWithChargeCode.TM_AC);
			Assert("Should be restricted", rateLineItemWithChargeCode.TM_CallForPricing);
			AssertEquals("Restricted FRT calculator has no restriction reason", ZString.Empty, rateLineItemWithChargeCode.TM_Text);

			var rateLineItemWithFreightCalcType = freightInclusiveRateLine.ChildRateLineItems.Single(x => x.TM_Type == FreightInclusiveCalculator.Items.FreightCalcType);
			AssertEquals("Should be Subject To", "SUB", rateLineItemWithFreightCalcType.TM_Text);
			Assert("Should be restricted", rateLineItemWithChargeCode.TM_CallForPricing);
			AssertEquals("Restricted FRT calculator has no restriction reason", ZString.Empty, rateLineItemWithChargeCode.TM_Text);
		}

		public void TestConvert_ZeroApplicableCharge()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "AC", "AAA");
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

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries([tradelane]);
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);

			var rateLine = freightRateEntry.ChildRateLines.Single();

			AssertEquals("ZERO applicable charge should be converted to a UNT calculator rate line", UnitCalculator.Code, rateLine.TL_RateCalculator);

			var rateLineItem = rateLine.ChildRateLineItems.Single();
			Assert("Charge line should not be restricted", !rateLineItem.TM_CallForPricing);
			AssertEquals("Unit price should be zero", 0m, rateLineItem.TM_RelevantValue);
		}

		#endregion

		#region TL_Rounding & TL_RoundingFactor

		public void TestConvert_UrsRatesWithPrecision()
		{
			var precision = 0.5m;
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var firstPriceEntry = tradeService.PriceInfo.BaseRates.Items.First().PriceEntries.First();
			((UniversalRateEntryDto)firstPriceEntry).MeasurementPrecision = precision;

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR);

			var expectedFreightRateEntry = new UrsRateEntry(tradeService, Factory);
			var wiseline1 = CreateUrsLine("FRT", "USD", UnitCalculator.Code);

			wiseline1.TL_Rounding = (ZString)RatingRoundingTypes.Custom;
			wiseline1.TL_RoundingFactor = precision;
			wiseline1.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline1, Calculator.Items.Operator.UNT, string.Empty, 1000m, ZString.Empty, ZDecimal.Zero, 0, false)
			};
			expectedFreightRateEntry.ChildRateLines = new[] { wiseline1 };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, Array.Empty<string>(), new[] { "TL_Rounding", "TL_RoundingFactor" }, Array.Empty<string>());
		}

		#endregion

		public void TestConvert_TACTReferenceRate()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();

			// min=1, -45, +45, +100, +250, +300
			frtRate.PriceEntries = new[]
			{
				CreateShipmentMinBreakItem(0m, RateApplicableCode.Tact),
				CreateSimpleWeightBreakPlus(45, 0m, RateApplicableCode.Tact),
				CreateSimpleWeightBreakPlus(100, 0m, RateApplicableCode.UnitPrice),
				CreateSimpleWeightBreakPlus(250, 0m, RateApplicableCode.UnitPrice),
				CreateSimpleWeightBreakPlus(300, 0m, RateApplicableCode.UnitPrice),
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR);

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			var wiseline = CreateUrsLine("FRT", "USD", CombinedCalculator.Code);
			wiseline.ChildRateLineItems = new[]
			{
				new WiseLineItem(wiseline, Calculator.Items.Operator.MIN, "TACT – TACT Reference rate", 0m, ZString.Empty, 0, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.Plus, "TACT – TACT Reference rate", 0m, ZString.Empty, 45, 0, true),
				new WiseLineItem(wiseline, Calculator.Items.Operator.Plus, "", 0m, ZString.Empty, 100, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.Plus, "", 0m, ZString.Empty, 250, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.Operator.Plus, "", 0m, ZString.Empty, 300, 0, false),
				new WiseLineItem(wiseline, Calculator.Items.UseAccumulated, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline, Calculator.Items.UseInclusiveBreaks, "N", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline, Calculator.Items.HigherChargeableLowerRate, "Y", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
				new WiseLineItem(wiseline, Calculator.Items.BreaksPer, "", ZDecimal.Zero, string.Empty, ZDecimal.Zero, ZDecimal.Zero, false),
			};

			expectedFreightRateEntry.ChildRateLines = new[] { wiseline };

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, Array.Empty<string>(), new[] { "TL_AC", "TL_RateCalculator", "ChildRateLineItems" }, new[] { "TM_Type", "TM_Text", "TM_RelevantValue", "TM_Break" });
		}

		public void TestConvert_RangeBreaksAreIgnoredForNow()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();
			frtRate.PriceEntries = new[]
			{
				new UniversalRateEntryDto()
				{
					Applicable = RateApplicableCode.UnitPrice,
					BreakType = RateBreakTypeCode.Range,
					TierLowerBoundQuantity = 100,
					TierUpperBoundQuantity = 200,
					Price = 100m,
					BreakQuantity = 1,
					QuantityUnit = UnitOfMeasurementCode.Kilogram,
					PricingQuantityUnit = UnitOfMeasurementCode.Kilogram,
					RoundingMode = 0.01m,
					UseVolumetric = true,
					PricingQuantity = 1,
					MeasurementPrecision = 0.5m,
				}
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });
			Assert("Freight charge failed, should return empty list", result.IsNullOrEmpty());
			AssertContainsExactElementsInAnyOrder([$"Charges with break criteria 'Rng' are not supported at the moment"], Logger.Warnings);
		}

		#endregion

		#region TL_WeightVolumeMultiple

		public void TestConvert_MinimumOrPerUnitCalculator_SetsWeightVolumeMultiple()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();
			frtRate.PriceEntries = [
				CreateSimpleWeightBreakItem(RateBreakTypeCode.Flat, 1, 10m, RateApplicableCode.UnitPrice, pricingQuantity: 1000),
				CreateSimpleWeightBreakItem(RateBreakTypeCode.Min, 1, 100m, RateApplicableCode.UnitPrice),
			];

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries([tradelane]);

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);
			var rateLine = freightRateEntry.ChildRateLines.First() as WiseLine;

			AssertEquals(MinimumOrPerUnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals(1000m, rateLine.TL_WeightVolumeMultiple);
		}

		public void TestConvert_FlatPlusPerUnitCalculator_SetsWeightVolumeMultiple()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();
			frtRate.PriceEntries = [
				CreateSimpleWeightBreakItem(RateBreakTypeCode.Flat, 1, 10m, RateApplicableCode.UnitPrice, pricingQuantity: 1000),
				CreateSimpleWeightBreakItem(RateBreakTypeCode.Base, 1, 100m, RateApplicableCode.UnitPrice),
			];

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries([tradelane]);

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);
			var rateLine = freightRateEntry.ChildRateLines.First() as WiseLine;

			AssertEquals(FlatPlusPerUnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals(1000m, rateLine.TL_WeightVolumeMultiple);
		}

		public void TestConvert_UnitCalculator_SetsWeightVolumeMultiple()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			var priceInfo = (PriceInfoDto)tradelane.PriceInfo;
			priceInfo.Charges = null;
			var frtRate = (RateCollectionDto)priceInfo.BaseRates.Items.First();
			frtRate.PriceEntries = [
				CreateSimpleWeightBreakItem(RateBreakTypeCode.Flat, 1, 10m, RateApplicableCode.UnitPrice, pricingQuantity: 1000),
			];

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries([tradelane]);

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL);
			var rateLine = freightRateEntry.ChildRateLines.First() as WiseLine;

			AssertEquals(UnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals(1000m, rateLine.TL_WeightVolumeMultiple);
		}

		#endregion

		#region Named Accounts

		public void TestConvertNamedAccounts_ShouldReturnCommonNamedAccountsInAllGroups()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			tradeService.NamedAccount = new NamedAccountGroupDataDto()
			{
				Groups = new[]
				{
					new NamedAccountGroupDto()
					{
						Code = "A",
						Description = "Group A",
						Items = new[]
						{
							new NamedAccountDto()
							{
								Code = "Nike US",
								Name = "Nike US"
							}
						}
					},
					new NamedAccountGroupDto()
					{
						Code = "B",
						Description = "Group B",
						Items = new[]
						{
							new NamedAccountDto()
							{
								Code = "Nike US",
								Name = "Nike US"
							},
							new NamedAccountDto()
							{
								Code = "Nike EU",
								Name = "Nike EU"
							}
						}
					}
				}
			};
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entry = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService }).First() as UrsRateEntry;

			AssertCollectionContains("Nike US", entry.NamedAccounts);
		}

		#endregion

		#region Conversion Factor

		public void TestConvert_ConversionFactor()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradelane, "ABC", 100m, wmRatio: 1000m, useVolumetric: true);

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var entries = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			AssertEquals(2, entries.Count());
			var originRateEntry = entries.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG);
			var rateLine = originRateEntry.ChildRateLines.First() as WiseLine;

			var expectedConversionFactor = new ConversionFactor(1000, "CC", "KG");
			AssertEquals(expectedConversionFactor, rateLine.ConversionFactor);
		}

		#endregion

		#region Custom Fields

		public void TestConvert_Rate_CustomFields_Routing()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			tradelane.CreateRoute()
				.AddWaypoint("UAIEV", RouteWaypointCode.Origin, LocationFunctionCode.Airport)
				.AddWaypoint("AUSYD", RouteWaypointCode.Destination, LocationFunctionCode.Airport)
				.AddWaypoint("SGSIN", RouteWaypointCode.Via, LocationFunctionCode.Airport)
				.AddWaypoint("HKHKG", RouteWaypointCode.Via2, LocationFunctionCode.Airport);
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR) as UrsRateEntry;
			AssertCollectionContains(new CustomField
			{
				Code = Rate.CustomFields.Common.Routing,
				Description = "Routing",
				Value = "UAIEV -> SGSIN -> HKHKG -> AUSYD"
			}, freightRateEntry.CustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargoguideReference()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			tradeService.ExternalReference = "Some Stuff";
			tradeService.ServiceClass = ServiceClassCode.Contract;

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR);

			var expectedFreightRateEntry = new UrsRateEntry(tradeService, Factory);
			expectedFreightRateEntry.TI_ContractNumber = "Some Stuff";

			AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, (UrsRateEntry)freightRateEntry, new[] { "TI_ContractNumber" }, Array.Empty<string>(), Array.Empty<string>());
		}

		public void TestConvert_Rate_CustomFields_CargoguideRateClass()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			tradeService.ServiceClass = ServiceClassCode.Gateway;

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradeService });
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR) as UrsRateEntry;
			AssertCollectionContains(new CustomField
			{
				Code = Rate.CustomFields.Cargoguide.RateClass,
				Description = "Rate/Service class",
				Value = "Gateway"
			}, freightRateEntry.CustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargoguideProductCode()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var product = (ProductDto)tradelane.Product;
			product.Code = "XYZ";

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR) as UrsRateEntry;
			AssertCollectionContains(new CustomField
			{
				Code = Rate.CustomFields.Cargoguide.ProductCode,
				Description = "Product Code",
				Value = "XYZ"
			}, freightRateEntry.CustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargoguideProductName()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var product = (ProductDto)tradelane.Product;
			product.Name = "Some Product Name";

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR) as UrsRateEntry;

			AssertCollectionContains(new CustomField
			{
				Code = Rate.CustomFields.Cargoguide.ProductName,
				Description = "Product Name",
				Value = "Some Product Name"
			}, freightRateEntry.CustomFields);
		}

		public void TestConvert_Rate_CustomFields_CargosphereArbitraryPermission()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			tradelane.TaggedValues = new TaggedValueDataDto()
			{
				Items = new[] { new TaggedValueDto { Key = "ArbitraryIndicator", Value = "Some Permission" } }
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;

			var customField = freightRateEntry.CustomFields.First(c => c.Code == Rate.CustomFields.CargoSphere.ArbitraryPermission);
			AssertEquals("Arbitrary Permission", customField.Description);
			AssertEquals("Some Permission", customField.Value);
		}

		public void TestConvert_Rate_CustomFields_CargosphereTradeLane()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			tradelane.VoyageInfo = new TradeServiceVoyageDto() { TradeScope = "Some Trade Lane" };

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;

			var customField = freightRateEntry.CustomFields.First(c => c.Code == Rate.CustomFields.CargoSphere.TradeLane);
			AssertEquals("Trade Lane", customField.Description);
			AssertEquals("Some Trade Lane", customField.Value);
		}

		public void TestConvert_Rate_CustomFields_HandlingOffice()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			SetOtherChargesAsOriginFlat(tradelane, "ABC", 100m);
			(tradelane.PriceInfo.Charges.Items.First() as BaseChargeDto).Company = new CompanyEntityDto
			{
				Name = "Handling Office"
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			var orgRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.ORG) as UrsRateEntry;

			var line = orgRateEntry.ChildRateLines.OfType<UrsRateLine>().FirstOrDefault((line) => !string.IsNullOrEmpty(line.HandlingOfficeName));
			AssertEquals("Handling Office", line.HandlingOfficeName);
		}

		public void TestConvert_Rate_Aircraft()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var product = (ProductDto)tradelane.Product;
			product.Aircraft = new AircraftFlightDeckDto
			{
				FlightDeck = "deck",
				Cao = true
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR) as UrsRateEntry;

			AssertCollectionContains(new CustomField
			{
				Code = Rate.CustomFields.Cargoguide.DeckType,
				Description = "Deck Type",
				Value = "deck"
			}, freightRateEntry.CustomFields);

			AssertEquals((ZString)Core.Constants.AircraftType.CAO, freightRateEntry.TI_AircraftType);
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
		}

		void TestContainerQualityMapping(bool isShipperOwned, string containerType, bool isOverweight, string cargoFit, string[] cargoOversizes, string expectedContainerQualityCode, string reason = null)
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			tradelane.Container = new ContainerDto
			{
				IsoCode = "22G0",
				IsShipperOwned = isShipperOwned,
				Type = containerType,
				IsOverweight = isOverweight,
				CargoFit = cargoFit,
				CargoOversize = cargoOversizes,
			};
			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			string PropertiesToString(bool isShipperOwned, string containerType, bool isOverweight, string cargoFit, string[] cargoOversizes) =>
				$"IsShipperOwned: {isShipperOwned}, ContainerType: {(string.IsNullOrWhiteSpace(containerType) ? "Empty" : containerType)}, "
				+ $"IsOverweight: {isOverweight}, CargoFit: {(string.IsNullOrWhiteSpace(cargoFit) ? "Empty" : cargoFit)}, "
				+ $"CargoOversizes: {(cargoOversizes.IsNullOrEmpty() ? "Empty" : string.Join(", ", cargoOversizes))}";
			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;

			if (expectedContainerQualityCode != null)
			{
				var message = $"{PropertiesToString(isShipperOwned, containerType, isOverweight, cargoFit, cargoOversizes)}\nShould be mapped to {expectedContainerQualityCode}";
				if (!string.IsNullOrEmpty(reason))
				{
					message += $"\nReason: {reason}";
				}
				AssertCollectionContains(
					message,
					new CustomField
					{
						Code = Rate.CustomFields.CargoSphere.ContainerQuality,
						Description = "Container Quality",
						Value = expectedContainerQualityCode
					},
					freightRateEntry.CustomFields);
			}
			else
			{
				var message = $"{PropertiesToString(isShipperOwned, containerType, isOverweight, cargoFit, cargoOversizes)}\nShould not be mapped.";
				if (!string.IsNullOrEmpty(reason))
				{
					message += $"\nReason: {reason}";
				}
				AssertCollectionNotContains(
					message,
					new CustomField
					{
						Code = Rate.CustomFields.CargoSphere.ContainerQuality,
						Description = "Container Quality",
						Value = expectedContainerQualityCode
					},
					freightRateEntry.CustomFields);
			}
		}

		public void TestGivenUniversalRateInAirMode_WhenConvertToWiseRate_ThenContainerQualityShouldNotBeMapped()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR) as UrsRateEntry;

			AssertCollectionNotContains(new CustomField
			{
				Code = Rate.CustomFields.CargoSphere.ContainerQuality,
				Description = "Container Quality",
				Value = "None"
			}, freightRateEntry.CustomFields);
		}

		#endregion

		#region Carrier Commodities

		public void TestConvert_GetCarrierCommodity_FromCargoguide()
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Air, "AC", "AAA");
			var product = (ProductDto)tradelane.Product;
			product.Name = "Some Product Name";
			product.UniversalCode = "TEST";
			product.Classification = new ProductClassDto
			{
				Code = ProductClassCode.Gen,
				Name = "Test Name",
			};

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

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.AIR) as UrsRateEntry;

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			expectedFreightRateEntry.Commodities = ["HEALTH FOOD", "MILK POWDER"];
			expectedFreightRateEntry.CommodityGroup = "TEST";
			expectedFreightRateEntry.ProductName = "Some Product Name";
			expectedFreightRateEntry.CarrierSpecificCommodity = new CarrierSpecificCommodity
			{
				Code = "PRD",
				GroupName = "Some Product Name",
				GroupType = CommodityCategory.NonHazardous,
				IncludedCommodities = ["HEALTH FOOD", "MILK POWDER"]
			};

			CombineAssertions(() =>
			{
				AssertEquals(ProductClassCode.Gen, expectedFreightRateEntry.ProductClassCode);
				AssertEquals("Test Name", expectedFreightRateEntry.ProductClassName);
				AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, freightRateEntry, new[] { "Commodities", "CommodityGroup", "ProductName", "ProductClassCode", "ProductClassName", "CarrierSpecificCommodity" }, Array.Empty<string>(), Array.Empty<string>());
			});
		}

		public void TestConvert_CarrierCommodity_FromCargoSphere_Dgr() =>
			TestConvert_CarrierCommodity_FromCargoSphere(ProductClassCode.Dgr, CommodityCategory.Hazardous);

		public void TestConvert_CarrierCommodity_FromCargoSphere_Gen() =>
			TestConvert_CarrierCommodity_FromCargoSphere(ProductClassCode.Gen, CommodityCategory.NonHazardous);

		public void TestConvert_CarrierCommodity_FromCargoSphere_Com() =>
			TestConvert_CarrierCommodity_FromCargoSphere(ProductClassCode.Com, CommodityCategory.NonHazardous);

		public void TestConvert_CarrierCommodity_FromCargoSphere(string classificationCode, string groupType)
		{
			var tradelane = CreateValidUldOrFclRate(TransportMode.Sea, "AC", "20GP");
			var product = (ProductDto)tradelane.Product;
			product.Name = "Some Product Name";
			product.Code = "Some Product Code";
			product.UniversalCode = "TEST";
			product.Classification = new ProductClassDto
			{
				Code = classificationCode,
				Name = "Test Name",
			};

			tradelane.Commodity = new CommodityDataDto()
			{
				Groups = new List<ICommodityGroupDto> {
					new CommodityGroupDto()
					{
						Description = "Some Group Description",
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
						},
						ExcludedItems = new List<ICommodityDto> {
							new CommodityDto()
							{
								Code = string.Empty,
								Name = "BABY FOOD",
								HsCode = string.Empty,
							},
							new CommodityDto()
							{
								Code = string.Empty,
								Name = "CHEESE",
								HsCode = string.Empty,
							},
						},
					}
				}
			};

			var converter = new UrsRatesConverter(Factory, Logger, ValidCriteria);
			var result = converter.ConvertTradeServiceDtoToWiseEntries(new[] { tradelane });

			var freightRateEntry = result.FirstOrDefault(entry => entry.TI_RateCategory == RateCategory.FCL) as UrsRateEntry;

			var expectedFreightRateEntry = new UrsRateEntry(tradelane, Factory);
			expectedFreightRateEntry.Commodities = ["HEALTH FOOD", "MILK POWDER"];
			expectedFreightRateEntry.ProductName = "Some Product Name";
			expectedFreightRateEntry.CarrierSpecificCommodity = new CarrierSpecificCommodity
			{
				IncludedCommodities = ["HEALTH FOOD", "MILK POWDER"],
				ExcludedCommodities = ["BABY FOOD", "CHEESE"],
				Code = "Some Product Code",
				GroupName = "Some Group Description",
				GroupType = groupType,
			};

			CombineAssertions(() =>
			{
				AssertEquals(classificationCode, expectedFreightRateEntry.ProductClassCode);
				AssertEquals("Test Name", expectedFreightRateEntry.ProductClassName);
				AssertRatesShouldBeConvertedCorrectly(expectedFreightRateEntry, freightRateEntry, new[] { "Commodities", "CommodityGroup", "ProductName", "ProductClassCode", "ProductClassName", "CarrierSpecificCommodity" }, Array.Empty<string>(), Array.Empty<string>());
			});
		}

		#endregion

		#region Spot Terms

		public void TestConvert_StorageCharges_FilteredFromCharges()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			(tradeService.PriceInfo as PriceInfoDto)
				.AddChargeWithSinglePriceEntry(new ("BAF", usabilityGroupCode: ChargeUsabilityGroupCode.None))
				.AddPenaltyWithSinglePriceEntry(new ("ODOC", usabilityGroupCode: ChargeUsabilityGroupCode.Storage));

			var entries = ConvertTradeServices(tradeService);

			AssertContainsExactElementsInAnyOrder(
				["FRT", "BAF"],
				entries
					.SelectMany(c => c.ChildRateLines)
					.Select(l => l.ChargeCode.AC_Code.ToString())
			);
		}

		public void TestConvert_PenCharges_FilteredFromCharges()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			(tradeService.PriceInfo as PriceInfoDto)
				.AddChargeWithSinglePriceEntry(new ("BAF", usabilityGroupCode: ChargeUsabilityGroupCode.None))
				.AddFreeTimeWithSinglePriceEntry(new ("ODOC", usabilityGroupCode: ChargeUsabilityGroupCode.Pen));

			var entries = ConvertTradeServices(tradeService);

			AssertContainsExactElementsInAnyOrder(
				["FRT", "BAF"],
				entries
					.SelectMany(c => c.ChildRateLines)
					.Select(l => l.ChargeCode.AC_Code.ToString())
			);
		}

		public void TestConvert_BookingInfo()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			UniversalToWiseRateConverterTest.AddTerms(tradeService);
			UniversalToWiseRateConverterTest.AddPenalties(tradeService);
			UniversalToWiseRateConverterTest.AddSchedules(tradeService);

			var bookingInfo = ConvertTradeServices(tradeService)[0].BookingInfo;
			CombineAssertions(() =>
			{
				AssertNotNull(bookingInfo);
				AssertEquals(2, bookingInfo.BookingTerms.Items.Length);
				AssertEquals(1, bookingInfo.Penalties.Length);
				AssertEquals(2, bookingInfo.Schedule.ScheduleDetails.Length);
			});
		}

		public void TestConvert_BookingInfo_CreateEntry_ForEachSchedule()
		{
			var tradeService = CreateValidUldOrFclRate(TransportMode.Sea, "QANT", "22G0");
			// Add 2 Schedules
			UniversalToWiseRateConverterTest.AddSchedules(tradeService);
			UniversalToWiseRateConverterTest.AddSchedules(tradeService);

			var rateEntries = ConvertTradeServices(tradeService);
			CombineAssertions(() =>
			{
				AssertEquals(2, rateEntries.Count);
				AssertEquals(2, rateEntries[0].BookingInfo.Schedule.ScheduleDetails.Length);
				AssertEquals(2, rateEntries[1].BookingInfo.Schedule.ScheduleDetails.Length);
			});
		}

		#endregion

		#region implement

		enum TransportMode
		{
			Air,
			Sea
		}

		UrsRateLine CreateUrsLine(string chargeCode, string currency, string calculatorCode)
		{
			var line = new UrsRateLine(Factory, new UrsCharge(), ChargeType.None)
			{
				TL_AC = Helper.ChargeCodes[chargeCode].PK,
				TL_RX_NKCurrency = currency,
				TL_RateCalculator = calculatorCode
			};
			return line;
		}

		UniversalRateEntryDto CreateSimpleBaseItem(decimal price, string breakType = RateBreakTypeCode.Flat, string applicable = RateApplicableCode.UnitPrice, string priceUnit = UnitOfMeasurementCode.BillOfLading, bool useVolumetric = false, decimal roudingMode = 0.01m)
		{
			return new UniversalRateEntryDto()
			{
				Applicable = applicable,
				BreakType = breakType,
				Price = price,
				BreakQuantity = 1,
				RoundingMode = roudingMode,
				UseVolumetric = useVolumetric,
				PricingQuantity = 1,
				MeasurementPrecision = 0.01m,
				QuantityUnit = priceUnit,
				PricingQuantityUnit = priceUnit
			};
		}

		UniversalRateEntryDto CreateShipmentMinBreakItem(decimal price, string applicable = RateApplicableCode.UnitPrice)
		{
			return new UniversalRateEntryDto()
			{
				Applicable = applicable,
				BreakType = RateBreakTypeCode.Min,
				Price = price,
				BreakQuantity = 1,
				QuantityUnit = UnitOfMeasurementCode.Shipment,
				PricingQuantityUnit = UnitOfMeasurementCode.Shipment,
				RoundingMode = 0.01m,
				UseVolumetric = false,
				PricingQuantity = 1,
				MeasurementPrecision = 0.01m,
			};
		}

		UniversalRateEntryDto CreateShipmentMaxBreakItem(decimal price)
		{
			return new UniversalRateEntryDto()
			{
				Applicable = RateApplicableCode.UnitPrice,
				BreakType = RateBreakTypeCode.Max,
				Price = price,
				BreakQuantity = 1,
				QuantityUnit = UnitOfMeasurementCode.Shipment,
				PricingQuantityUnit = UnitOfMeasurementCode.Shipment,
				RoundingMode = 0.01m,
				UseVolumetric = false,
				PricingQuantity = 1,
				MeasurementPrecision = 0.01m,
			};
		}

		UniversalRateEntryDto CreateSimpleWeightBreakLess(decimal breakQuantity, decimal price, string applicable = RateApplicableCode.UnitPrice)
			=> CreateSimpleWeightBreakItem(RateBreakTypeCode.BreakLess, breakQuantity, price, applicable);

		UniversalRateEntryDto CreateSimpleWeightBreakPlus(decimal breakQuantity, decimal price, string applicable = RateApplicableCode.UnitPrice)
			=> CreateSimpleWeightBreakItem(RateBreakTypeCode.BreakPlus, breakQuantity, price, applicable);

		UniversalRateEntryDto CreateSimplePivotBreakPlus(decimal breakQuantity, decimal price, string applicable = RateApplicableCode.UnitPrice)
		=> CreateSimpleWeightBreakItem(RateBreakTypeCode.Pivot, breakQuantity, price, applicable);

		UniversalRateEntryDto CreateSimpleWeightBreakItem(string breakType, decimal breakQuantity, decimal price, string applicable, decimal pricingQuantity = 1)
		{
			return new UniversalRateEntryDto()
			{
				Applicable = applicable,
				BreakType = breakType,
				Price = price,
				BreakQuantity = breakQuantity,
				QuantityUnit = UnitOfMeasurementCode.Kilogram,
				PricingQuantityUnit = UnitOfMeasurementCode.Kilogram,
				RoundingMode = 0.01m,
				UseVolumetric = true,
				PricingQuantity = pricingQuantity,
				MeasurementPrecision = 0.5m,
			};
		}

		TradeServiceDto CreateValidUldOrFclRate(TransportMode transportMode, string carrierCode, string containerCode = "22G0")
		{
			return new TradeServiceDto()
			{
				ModesOfTransport = new ModesOfTransportDataDto()
				{
					Items =
					[
						new ModeOfTransportDto()
						{
							Code = transportMode == TransportMode.Air ? ModeOfTransport.Air : ModeOfTransport.Ocean
						}
					]
				},

				Type = ComposedTradeServiceType.Direct,

				// Code is the IATA code for AIR rate.
				TransportProvider = new TransportProviderDto() { Code = carrierCode },

				ServiceClass = ServiceClassCode.Selling,
				Container = new ContainerDto()
				{
					IsoCode = containerCode,
					IsShipperOwned = false
				},
				Product = new ProductDto()
				{
					Code = "PRD",
					Name = "Product X",
					Classification = new ProductClassDto()
					{
						Code = "GEN",
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
				VoyageInfo = new TradeServiceVoyageDto() { OceanRoutingTerm = "Std" },

				RouteInfo = new RouteInfoDto()
				{
					Waypoints = new[]
					{
						new GeoScopeEntryDto() { WaypointType = RouteWaypointCode.Origin, Location = new LocationDto() { Code = "UAIEV", FunctionCode = LocationFunctionCode.Airport } },
						new GeoScopeEntryDto() { WaypointType = RouteWaypointCode.Via, Location = new LocationDto() { Code = "AEDXB", FunctionCode = LocationFunctionCode.Airport } },
						new GeoScopeEntryDto() { WaypointType = RouteWaypointCode.Destination, Location = new LocationDto() { Code = "AUSYD", FunctionCode = LocationFunctionCode.Airport } },
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
								PaymentTerm = PaymentTermCode.Prepaid,
								PriceEntries = new []
								{
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
								}
							}
						}
					},
				}
			};
		}

		OrgHeader NewCarrier(string carrierCode, string fullName = null, string scacCode = null, string c1Code = null, string iataCode = null, string airlineAccountingCode = null)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = carrierCode;
			carrier.OH_FullName = !string.IsNullOrWhiteSpace(fullName) ? fullName : "Carrier Full Name";
			carrier.OH_IsShippingProvider = true;

			if (scacCode != null || c1Code != null)
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CarrierName = carrier.OH_Code;
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
				shippingLine.RSL_StandardCarrierAlphaCode = scacCode;
				shippingLine.RSL_CargoWiseOneCode = c1Code ?? scacCode;
			}

			if (iataCode != null)
			{
				var airlineOnDb = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_ThreeLetterCode, iataCode));
				if (airlineOnDb == null)
				{
					var airline = Factory.NewWithValidTestData<RefAirline>();
					airline.RM_EagleAddedAirlinePrefixOrAccountingCode = airlineAccountingCode ?? "9xx";
					airline.RM_ThreeLetterCode = iataCode.Length == 3 ? iataCode : "";
					airline.RM_TwoCharacterCode = iataCode.Length == 2 ? iataCode : "";
					carrier.MiscServ.OM_RM_Airline = airline.PK;
				}
				else
				{
					carrier.MiscServ.OM_RM_Airline = airlineOnDb.PK;
				}
			}

			return carrier;
		}

		/// <summary>
		/// Set the non-freight charges to be a single flat charge.
		/// </summary>
		void SetOtherChargesAsOriginFlat(TradeServiceDto tradelane, string universalChargeCode, decimal price, string applicable = RateApplicableCode.UnitPrice, string priceUnit = UnitOfMeasurementCode.BillOfLading, string currency = "USD", decimal wmRatio = 0m, bool useVolumetric = false)
		{
			((PriceInfoDto)tradelane.PriceInfo).Charges = new BaseChargeDataDto
			{
				Items = new BaseChargeDto[]
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
								Code = ShippingPhaseCode.PortfLoading
							}
						},
						WmRatioCubicCentimeter = wmRatio,
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
											Price = price,
											Applicable = applicable,
											BreakType = RateBreakTypeCode.Flat,
											BreakQuantity = 1,
											PricingQuantityUnit = priceUnit,
											UseVolumetric = useVolumetric
										}
									}
								}
							}
						}
					}
				}
			};
		}
		void SetOtherChargeAsPercentageOfBaseFreight(TradeServiceDto tradelane, string universalChargeCode, decimal percentage,
			bool requiresQuantifiedInput = false, string pricingQuantityUnit = UnitOfMeasurementCode.Shipment, string currency = "USD")
		{
			((PriceInfoDto)tradelane.PriceInfo).Charges = new BaseChargeDataDto
			{
				Items = new BaseChargeDto[]
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
							RequiresQuantifiedInput = requiresQuantifiedInput,
							ShippingPhase = new ShippingPhaseDto()
							{
								Code = ShippingPhaseCode.Freight
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
										new UniversalRateEntryDto()
										{
											// Is this the correct field to store percentage, or is it another?
											// There's no example rates in the test db to tell.
											// The CS converter gets percentage from CargoSphereCharge.OriginalRate, which isn't in mapping doc.
											// CG converter doesn't do percentage.
											Price = percentage,
											Applicable = RateApplicableCode.Percentage,
											BreakType = RateBreakTypeCode.Flat,
											BreakQuantity = 1,
											PricingQuantityUnit = pricingQuantityUnit,
											QuantityUnit = UnitOfMeasurementCode.Container,
										}
									}
								}
							}
						}
					}
				}
			};
		}

		List<UrsRateEntry> ConvertTradeServices(params TradeServiceDto[] services) =>
			new UrsRatesConverter(Factory, Logger, ValidCriteria)
				.ConvertTradeServiceDtoToWiseEntries(services)
				.Cast<UrsRateEntry>().ToList();

		#endregion

		#region helper

		void AssertRatesShouldBeConvertedCorrectly(UrsRateEntry expected, UrsRateEntry actual, IEnumerable<string> rateEntryProperties, IEnumerable<string> childRateLineProperties, IEnumerable<string> childRateLineItemProperties)
		{
			if (rateEntryProperties.Any())
			{
				foreach (var property in rateEntryProperties.Where(property => property != "ChildRateLines"))
				{
					var expectedValue = typeof(UrsRateEntry).GetProperty(property).GetValue(expected);
					var actualValue = typeof(UrsRateEntry).GetProperty(property).GetValue(actual);

					if (expectedValue is IEnumerable<ZString> expectedEnumerable && actualValue is IEnumerable<ZString> actualEnumerable)
					{
						AssertContainsExactElementsInAnyOrder($"Mismatch in property '{property}'", expectedEnumerable, actualEnumerable);
					}
					else
					{
						AssertEquals($"Mismatch in property '{property}'", expectedValue, actualValue);
					}
				}
			}

			if (childRateLineProperties.Any())
			{
				var expectedLines = expected.ChildRateLines.Cast<UrsRateLine>().ToList();
				var actualLines = actual.ChildRateLines.Cast<UrsRateLine>().ToList();

				AssertEquals("Mismatch in ChildRateLines count", expectedLines.Count, actualLines.Count);

				foreach (var expectedLine in expectedLines)
				{
					var actualLine = actualLines.FirstOrDefault(line => line.TL_AC == expectedLine.TL_AC && line.TL_RateCalculator == expectedLine.TL_RateCalculator);
					AssertNotNull($"Missing matching ChildRateLine for ChargeCode '{expectedLine.ChargeCode?.AC_Code}' and Calculator '{expectedLine.TL_RateCalculator}'", actualLine);

					foreach (var property in childRateLineProperties.Where(property => property != "ChildRateLineItems"))
					{
						var expectedValue = typeof(UrsRateLine).GetProperty(property).GetValue(expectedLine);
						var actualValue = typeof(UrsRateLine).GetProperty(property).GetValue(actualLine);
						AssertEquals($"Mismatch in ChildRateLine property '{property}'", expectedValue, actualValue);
					}

					if (childRateLineProperties.Contains("ChildRateLineItems") && childRateLineItemProperties.Any())
					{
						var expectedItems = expectedLine.ChildRateLineItems.ToList();
						var actualItems = actualLine.ChildRateLineItems.ToList();

						AssertEquals($"Mismatch in ChildRateLineItems count for charge '{expectedLine.ChargeCode.AC_Code}'", expectedItems.Count, actualItems.Count);

						foreach (var expectedItem in expectedItems)
						{
							var actualItem = actualItems.FirstOrDefault(item => childRateLineItemProperties.All(prop =>
								Equals(typeof(WiseLineItem).GetProperty(prop).GetValue(item), typeof(WiseLineItem).GetProperty(prop).GetValue(expectedItem))));

							AssertNotNull($"Missing ChildRateLineItem matching properties for charge '{expectedLine.ChargeCode.AC_Code}'", actualItem);

							foreach (var property in childRateLineItemProperties)
							{
								var expectedValue = typeof(WiseLineItem).GetProperty(property).GetValue(expectedItem);
								var actualValue = typeof(WiseLineItem).GetProperty(property).GetValue(actualItem);
								AssertEquals($"Mismatch in ChildRateLineItem property '{property}' for charge '{expectedLine.ChargeCode.AC_Code}'", expectedValue, actualValue);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new ElementaryLogger();
			SeaCarrier = NewCarrier("Carrier_SCAC", scacCode: "QANT");
			AirCarrier = NewCarrier(carrierCode: "AIRC", fullName: "Air Carrier", iataCode: "AC", airlineAccountingCode: "7xx");
			var seaContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			seaContainer.RC_ISOType = "22G0";
			seaContainer.RC_FreightRateClass = "20TD";
			seaContainer.RC_HandlingRateClass = "22PF";
			Helper.ChargeCodes.CreateGlobalCharge("FSC");
			Helper.ChargeCodes.CreateGlobalCharge("ABC", chargeGroup: ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.CreateGlobalCharge("EEE", chargeGroup: ChargeCodeGroupList.Codes.Origin);

			Factory.Save();

			ValidCriteria = new TestRatingCriteria();
		}
		protected OrgHeader AirCarrier { get; set; }
		protected OrgHeader SeaCarrier { get; set; }
		protected DateTime utcToday { get; } = ZDateTime.UtcToday.ToDateTime();
		protected ElementaryLogger Logger { get; private set; }
		protected RatingCriteria ValidCriteria { get; private set; }

		#endregion
	}
}
