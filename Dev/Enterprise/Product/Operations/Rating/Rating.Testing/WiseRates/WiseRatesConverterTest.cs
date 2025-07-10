using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.RatingTests.WiseRates;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static System.FormattableString;
using static Enterprise.Rating.Business.UrsConstants;
using Constants = Enterprise.Core.Constants;
using DTO = WiseRates.Api.Model;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Testing.WiseRates
{
	public class WiseRatesConverterTest : RatingTestCase
	{
		#region Basic Fields

		public void TestConvert_BasicFields_UseValuesAsTheyAre()
		{
			var response = ValidSEARatesSearchResponse;
			var rate = response.Rates[0];
			rate.Via = "AUSYD";
			rate.StartDate = new DateTime(2020, 01, 01);
			rate.ExpiryDate = new DateTime(2025, 01, 01);
			rate.ContractNumber = "666";
			rate.PaymentTerm = Constants.PaymentType.Prepaid;
			rate.ReservedForJobIDs = new[] { "Job 1", "Job 2" };
			rate.Provider = "CargoSphere";
			rate.Commodity = "CM1";
			rate.CarrierCommodityInfo = new CarrierSpecificCommodity
			{
				GroupName = "Carrier Product Name",
				IncludedCommodities = new string[] { "Pencils", "Human Remains" },
			};

			var charge = rate.Charges[0];
			charge.CarrierChargeCodeInfo = new CarrierSpecificChargeCode
			{
				Code = "CFRT",
				Description = "Carrier Freight"
			};

			var entry = ConvertSingleEntry(response);

			var line = new WiseLine(Factory, charge);
			line.CarrierChargeCode = "CFRT";
			line.CarrierChargeCodeDescription = "Carrier Freight";

			var expectedEntry = new WiseEntry(rate, Factory);
			expectedEntry.TI_ViaLRC = "AUSYD";
			expectedEntry.TI_RateStartDate = new ZDate(2020, 01, 01);
			expectedEntry.TI_RateEndDate = new ZDate(2025, 01, 01);
			expectedEntry.TI_ContractNumber = "666";
			expectedEntry.TI_PaymentTerm = Constants.PaymentType.Prepaid;
			expectedEntry.ReservedForJobIDs = new[] { "Job 1", "Job 2" };
			expectedEntry.RateProvider = "CargoSphere";
			expectedEntry.TI_MatchContainerRateClass = true;
			expectedEntry.CommodityGroup = "CM1";
			expectedEntry.ChildRateLines = new[] { line };
			expectedEntry.Commodities = new ZString[] { "Pencils", "Human Remains" };

			AssertEquals("Expected TI_ViaLRC to match", expectedEntry.TI_ViaLRC, entry.TI_ViaLRC);
			AssertEquals("Expected TI_RateStartDate to match", expectedEntry.TI_RateStartDate, entry.TI_RateStartDate);
			AssertEquals("Expected TI_RateEndDate to match", expectedEntry.TI_RateEndDate, entry.TI_RateEndDate);
			AssertEquals("Expected TI_ContractNumber to match", expectedEntry.TI_ContractNumber, entry.TI_ContractNumber);
			AssertEquals("Expected TI_PaymentTerm to match", expectedEntry.TI_PaymentTerm, entry.TI_PaymentTerm);
			AssertContainsExactElementsInAnyOrder("Expected ReservedForJobIDs to match", expectedEntry.ReservedForJobIDs, entry.ReservedForJobIDs);
			AssertEquals("Expected RateProvider to match", expectedEntry.RateProvider, entry.RateProvider);
			AssertContainsExactElementsInAnyOrder("Expected Commodities to match", expectedEntry.Commodities, entry.Commodities);

			AssertNotEquals("Entry PK should not be empty.", ZGuid.Empty, entry.PK);
			AssertNotEquals("Parent Rating Header PK should not be empty.", ZGuid.Empty, entry.ParentRatingHeader.PK);
			entry.ChildRateLines.ForEach(c => AssertNotEquals("Child Rate Line PK should not be empty.", ZGuid.Empty, c.PK));
		}

		public void TestConvert_CarrierChargeCodeInfo_Null()
		{
			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.Via = "AUSYD";
			rate.StartDate = new DateTime(2020, 01, 01);
			rate.ExpiryDate = new DateTime(2025, 01, 01);
			rate.ContractNumber = "666";
			rate.PaymentTerm = Constants.PaymentType.Prepaid;
			rate.ReservedForJobIDs = new[] { "Job 1", "Job 2" };
			rate.Provider = "CargoSphere";
			rate.Commodity = "CM1";
			rate.CarrierCommodityInfo = null;

			var charge = rate.Charges[0];
			charge.CarrierChargeCodeInfo = new CarrierSpecificChargeCode
			{
				Code = "CFRT",
				Description = "Carrier Freight"
			};

			var entry = ConvertSingleEntry(response);

			var line = new WiseLine(Factory, charge);
			line.CarrierChargeCode = "CFRT";
			line.CarrierChargeCodeDescription = "Carrier Freight";

			var expectedEntry = new WiseEntry(rate, Factory);
			expectedEntry.TI_ViaLRC = "AUSYD";
			expectedEntry.TI_RateStartDate = new ZDate(2020, 01, 01);
			expectedEntry.TI_RateEndDate = new ZDate(2025, 01, 01);
			expectedEntry.TI_ContractNumber = "666";
			expectedEntry.TI_PaymentTerm = Constants.PaymentType.Prepaid;
			expectedEntry.ReservedForJobIDs = new[] { "Job 1", "Job 2" };
			expectedEntry.RateProvider = "CargoSphere";
			expectedEntry.CommodityGroup = "CM1";
			expectedEntry.ChildRateLines = new[] { line };
			expectedEntry.Commodities = null;

			// Comparing all relevant fields manually due to lack of fluent compatibility
			AssertEquals("Expected TI_ViaLRC to match", expectedEntry.TI_ViaLRC, entry.TI_ViaLRC);
			AssertEquals("Expected TI_RateStartDate to match", expectedEntry.TI_RateStartDate, entry.TI_RateStartDate);
			AssertEquals("Expected TI_RateEndDate to match", expectedEntry.TI_RateEndDate, entry.TI_RateEndDate);
			AssertEquals("Expected TI_ContractNumber to match", expectedEntry.TI_ContractNumber, entry.TI_ContractNumber);
			AssertEquals("Expected TI_PaymentTerm to match", expectedEntry.TI_PaymentTerm, entry.TI_PaymentTerm);
			AssertContainsExactElementsInAnyOrder("Expected ReservedForJobIDs to match", expectedEntry.ReservedForJobIDs, entry.ReservedForJobIDs);
			AssertEquals("Expected RateProvider to match", expectedEntry.RateProvider, entry.RateProvider);
			AssertEquals("Expected CommodityGroup to match", expectedEntry.CommodityGroup, entry.CommodityGroup);
			AssertEquals("Expected only 1 child rate line", 1, entry.ChildRateLines.Count());
			AssertEquals("Expected CarrierChargeCode to match", expectedEntry.ChildRateLines.ElementAt(0).CarrierChargeCode, entry.ChildRateLines.ElementAt(0).CarrierChargeCode);
			AssertEquals("Expected CarrierChargeCodeDescription to match", expectedEntry.ChildRateLines.ElementAt(0).CarrierChargeCodeDescription, entry.ChildRateLines.ElementAt(0).CarrierChargeCodeDescription);
			AssertEquals("Expected Commodities to be null", expectedEntry.Commodities, entry.Commodities);

			AssertNotEquals("Expected PK to not be empty", ZGuid.Empty, entry.PK);
			AssertNotEquals("Expected ParentRatingHeader.PK to not be empty", ZGuid.Empty, entry.ParentRatingHeader.PK);
			entry.ChildRateLines.ForEach(c => AssertNotEquals("Expected ChildRateLine.PK to not be empty", ZGuid.Empty, c.PK));
		}

		#endregion

		#region Carrier

		public void TestConvert_Carrier_IsEmpty_UseEmptyValue()
		{
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "";

			var entry = ConvertSingleEntry(response);

			AssertCarrierConversion(entry, "", "", ZGuid.Empty, null);
		}

		public void TestConvert_Carrier_IsInvalid_SilentErrorWithTraceID()
		{
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "potato";

			ConvertSingleEntry(response);

			AssertEquals("Logger should not have any errors or warnings.", 0, Logger.GetErrorsAndWarnings().Count());
			AssertEquals("[TraceID: traceid] Carrier with code 'potato' not found among carriers in the response (EMR)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestConvert_Carrier_CannotBeMatched_GenerateError()
		{
			var wiseCarrier = new RefCarrier
			{
				Code = "QANTAS",
				C1Code = "QNT",
				SCACCode = "QANT",
				IATACode = "QQQ"
			};
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			var expectedError = GetCarrierConversionError(wiseCarrier);
			var expectedWarnings = new[]
			{
				"Warning:No active Carrier is assigned with SCAC Code 'QANT'",
				"Warning:No active Carrier is assigned with C1 Code 'QNT'"
			};

			AssertCarrierConversion(entry, expectedError, expectedWarnings, ZGuid.Empty, wiseCarrier);
		}

		public void TestConvert_Carrier_FindingBySCACTakesPriorityOverC1()
		{
			var carrierWithSCAC = NewCarrier("Carrier_SCAC", scacCode: "QANT");
			var carrierWithC1 = NewCarrier("Carrier_C1", c1Code: "QNT");
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", SCACCode = "QANT", C1Code = "QNT", IATACode = "QQQ" };
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);

			AssertCarrierConversion(entry, "", "", carrierWithSCAC.PK, wiseCarrier);
		}

		#region SCAC Code

		public void TestConvert_Carrier_TakesSCACOverIATAForTransportModeNonAIR()
		{
			var wiseCarrier = new RefCarrier { Code = "QANTAS", SCACCode = "QQQ*", IATACode = "QQ" };
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			AssertCarrierConversion(
				entry,
				GetCarrierConversionError(wiseCarrier),
				new[] { "Warning:No active Carrier is assigned with SCAC Code 'QQQ*'" },
				ZGuid.Empty,
				wiseCarrier
			);

			AssertCollectionNotContains(
				"Only SCAC is checked. IATA is discarded.",
				"Warning:No active Carrier is assigned with IATA Code 'QQ'",
				Logger.Warnings
			);
		}

		public void TestConvert_Carrier_HasSCACCode_NoCarrierFound()
		{
			var wiseCarrier = new RefCarrier { Code = "QANTAS", SCACCode = "QANT" };
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			var expectedError = GetCarrierConversionError(wiseCarrier);
			var expectedWarning = "Warning:No active Carrier is assigned with SCAC Code 'QANT'";

			AssertCarrierConversion(entry, expectedError, expectedWarning, ZGuid.Empty, wiseCarrier);
		}

		public void TestConvert_Carrier_OneActiveCarrierWithSCACCode()
		{
			var carrier = NewCarrier("Carrier_1", scacCode: "QANT");
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", SCACCode = "QANT" };
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);

			AssertCarrierConversion(entry, "", "", carrier.PK, wiseCarrier);
		}

		public void TestConvert_Carrier_OneInactiveCarrierWithSCACCode()
		{
			var carrier = NewCarrier("Carrier_1", scacCode: "QANT");
			carrier.OH_IsActive = false;
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", SCACCode = "QANT" };
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			var expectedError = GetCarrierConversionError(wiseCarrier);
			var expectedWarning = "Warning:No active Carrier is assigned with SCAC Code 'QANT'";

			AssertCarrierConversion(entry, expectedError, expectedWarning, ZGuid.Empty, wiseCarrier);
		}

		#endregion

		#region C1 Code

		public void TestConvert_Carrier_TakesC1COverIATAForTransportModeNonAIR()
		{
			var wiseCarrier = new RefCarrier { Code = "QANTAS", C1Code = "QQQ*", IATACode = "QQ" };
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			AssertCarrierConversion(
				entry,
				GetCarrierConversionError(wiseCarrier),
				new[] { "Warning:No active Carrier is assigned with C1 Code 'QQQ*'" },
				ZGuid.Empty,
				wiseCarrier);

			AssertCollectionNotContains(
				"Only C1C is checked. IATA is discarded.",
				"Warning:No active Carrier is assigned with IATA Code 'QQ'",
				Logger.Warnings
			);
		}

		public void TestConvert_Carrier_HasC1Code_NoCarrierFound()
		{
			var wiseCarrier = new RefCarrier { Code = "QANTAS", C1Code = "QNT" };
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			var expectedError = GetCarrierConversionError(wiseCarrier);
			var expectedWarning = "Warning:No active Carrier is assigned with C1 Code 'QNT'";

			AssertCarrierConversion(entry, expectedError, expectedWarning, ZGuid.Empty, wiseCarrier);
		}

		public void TestConvert_Carrier_OneActiveCarrierWithC1Code()
		{
			var carrier = NewCarrier("Carrier_1", c1Code: "QNT");
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", C1Code = "QNT" };
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);

			AssertCarrierConversion(entry, "", "", carrier.PK, wiseCarrier);
		}

		public void TestConvert_Carrier_OneInactiveCarrierWithC1Code()
		{
			var carrier = NewCarrier("Carrier_1", c1Code: "QNT");
			carrier.OH_IsActive = false;
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", C1Code = "QNT" };
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			var expectedError = GetCarrierConversionError(wiseCarrier);
			var expectedWarning = "Warning:No active Carrier is assigned with C1 Code 'QNT'";

			AssertCarrierConversion(entry, expectedError, expectedWarning, ZGuid.Empty, wiseCarrier);
		}

		#endregion

		#region IATA Code

		public void TestConvert_Carrier_HasIATACode_NoCarrierFound()
		{
			var wiseCarrier = new RefCarrier { Code = "QANTAS", IATACode = "QQQ" };
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			var expectedError = GetCarrierConversionError(wiseCarrier);
			var expectedWarning = "Warning:No active Carrier is assigned with IATA Code 'QQQ'";

			AssertCarrierConversion(entry, expectedError, expectedWarning, ZGuid.Empty, wiseCarrier);
		}

		public void TestConvert_Carrier_OneActiveCarrierWithIATACode_With2LetterCode()
		{
			var carrier = NewCarrier("Carrier_1", iataCode: "QQ");
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", IATACode = "QQ" };
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);

			AssertCarrierConversion(entry, "", "", carrier.PK, wiseCarrier);
		}

		public void TestConvert_Carrier_OneActiveCarrierWithIATACode_With3LetterCode()
		{
			var carrier = NewCarrier("Carrier_1", iataCode: "QQQ");
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", IATACode = "QQQ" };
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);

			AssertCarrierConversion(entry, "", "", carrier.PK, wiseCarrier);
		}

		public void TestConvert_Carrier_IATACode_UnsupportedLength()
		{
			var wiseCarrier = new RefCarrier { Code = "QANTAS", IATACode = "QQQ*" };
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			AssertCarrierConversion(
				entry,
				GetCarrierConversionError(wiseCarrier),
				"Warning:IATA Code 'QQQ*' with length 4 has not been supported",
				ZGuid.Empty,
				wiseCarrier);
		}

		public void TestConvert_Carrier_OneInactiveCarrierWithIATACode()
		{
			var carrier = NewCarrier("Carrier_1", iataCode: "QQQ");
			carrier.OH_IsActive = false;
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", IATACode = "QQQ" };
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			var expectedError = GetCarrierConversionError(wiseCarrier);
			var expectedWarning = "Warning:No active Carrier is assigned with IATA Code 'QQQ'";

			AssertCarrierConversion(entry, expectedError, expectedWarning, ZGuid.Empty, wiseCarrier);
		}

		public void TestConvert_Carrier_MultipleCarriersWithSameIATACode_MoreThanOneActiveCarrier()
		{
			NewCarrier("Carrier_1", "Carrier with same IATA Code 1", iataCode: "QQQ");
			NewCarrier("Carrier_2", "Carrier with same IATA Code 2", iataCode: "QQQ");
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", IATACode = "QQQ" };
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			var expectedError = GetCarrierConversionError(wiseCarrier);
			var expectedWarning = "Warning:More than one of the active Carriers (Carrier_1, Carrier_2) are assigned with IATA Code 'QQQ'";

			AssertCarrierConversion(entry, expectedError, expectedWarning, ZGuid.Empty, wiseCarrier);
		}

		public void TestConvert_Carrier_MultipleCarriersWithSameIATACode_WithOnlyOneActiveCarrier()
		{
			var activeCarrier = NewCarrier("Carrier_1", "Carrier with same IATA Code 1", iataCode: "QQQ");
			var inactiveCarrier = NewCarrier("Carrier_2", "Carrier with same IATA Code 2", iataCode: "QQQ");
			inactiveCarrier.OH_IsActive = false;
			Factory.Save();

			var wiseCarrier = new RefCarrier { Code = "QANTAS", IATACode = "QQQ" };
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);

			AssertCarrierConversion(entry, "", "", activeCarrier.PK, wiseCarrier);
		}

		public void TestConvert_Carrier_TakesIATAForTransportModeAIR()
		{
			var wiseCarrier = new RefCarrier { Code = "QANTAS", SCACCode = "QQQ*", C1Code = "QQQ*", IATACode = "QQ" };
			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates.Single();
			rate.Carrier = "QANTAS";
			response.Carriers = new[] { wiseCarrier };

			var entry = ConvertSingleEntry(response);
			AssertCarrierConversion(
				entry,
				GetCarrierConversionError(wiseCarrier),
				"Warning:No active Carrier is assigned with IATA Code 'QQ'",
				ZGuid.Empty,
				wiseCarrier);

			var notExpectedWarnings = new[]
			{
				"Warning:No active Carrier is assigned with SCAC Code 'QQQ*'",
				"Warning:No active Carrier is assigned with C1 Code 'QQQ*'",
			};

			AssertCollectionNotContains(
				"Only IATA is checked. SCAC and C1 are discarded",
				notExpectedWarnings,
				Logger.Warnings
			);
		}

		#endregion

		#region Helper Methods

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

		string GetCarrierConversionError(RefCarrier wiseCarrier) =>
			Invariant($"No single Carrier is assigned with the SCAC, IATA or C1C Code of the Carrier from Rates Service:\r\n{wiseCarrier.ToJSON()}");

		void AssertCarrierConversion(WiseEntry entry, string expectedError, string expectedWarning, ZGuid expectedCarrierPK, RefCarrier expectedWiseCarrier) =>
			AssertCarrierConversion(entry, expectedError, new[] { expectedWarning }, expectedCarrierPK, expectedWiseCarrier);

		void AssertCarrierConversion(WiseEntry entry, string expectedError, IEnumerable<string> expectedWarnings, ZGuid expectedCarrierPK, RefCarrier expectedWiseCarrier)
		{
			var header = (WiseHeader)entry.ParentRatingHeader;

			if (!string.IsNullOrWhiteSpace(expectedError))
			{
				AssertCollectionContains(expectedError, header.Errors.Values.ToArray());
				AssertEquals(ZGuid.Empty, header.TH_OH);
				AssertNull(entry.Carrier);

				foreach (var warning in expectedWarnings)
				{
					AssertCollectionContains("Warnings should match the expected collection", warning, Logger.Warnings);
				}
			}
			else
			{
				AssertEquals("Errors should be empty when no expected error", 0, header.Errors.Count);
				AssertEquals(expectedCarrierPK, header.TH_OH);
				if (expectedCarrierPK.IsEmpty)
				{
					AssertNull(entry.Carrier);
				}
				else
				{
					AssertEquals(expectedCarrierPK, entry.Carrier.PK);
				}

				AssertEquals("Warnings should be empty when no error is expected", 0, Logger.Warnings.Count);
			}

			AssertEquals("Wise carrier should match", expectedWiseCarrier?.Name, header.WiseCarrier?.Name);

			AssertEquals("Entry errors should be empty", 0, entry.Errors.Count);
			AssertEquals("Carrier specific costing, i.e. carrier should be set on the header", ZGuid.Empty, entry.TI_OH_TransportProvider);
		}

		#endregion

		#endregion

		#region Locations

		public void TestConvert_OriginDestination_IsValidUNLOCO_UseUNLOCOAsValue()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Origin = "UAIEV";
			response.Rates[0].Destination = "AUSYD";

			var entry = ConvertSingleEntry(response);

			AssertEquals("Origin LRC should match 'UAIEV'", "UAIEV", entry.TI_OriginLRC);
			AssertEquals("Destination LRC should match 'AUSYD'", "AUSYD", entry.TI_DestinationLRC);
			AssertEquals("Errors collection should be empty", 0, entry.Errors.Count);
		}

		public void TestConvert_OriginDestination_IsValidCountry_UseCountryCodeAsValue()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Origin = "UA";
			response.Rates[0].Destination = "AU";

			var entry = ConvertSingleEntry(response);

			AssertEquals("UA", entry.TI_OriginLRC);
			AssertEquals("AU", entry.TI_DestinationLRC);
			AssertEquals(0, entry.Errors.Count);
		}

		public void TestConvert_OriginDestination_IsValidCityCode_UseCityCodeAsValue()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Origin = "IEV";
			response.Rates[0].Destination = "SYD";

			var entry = ConvertSingleEntry(response);

			AssertEquals("Origin city code should match", "IEV", entry.TI_OriginLRC);
			AssertEquals("Destination city code should match", "SYD", entry.TI_DestinationLRC);
			AssertEquals("There should be no errors", 0, entry.Errors.Count);
		}

		public void TestConvert_OriginDestination_IsInvalidLocation_GenerateError()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Origin = "XXXXX";
			response.Rates[0].Destination = "XXX";

			var entry = ConvertSingleEntry(response);

			AssertEquals("Origin LRC should be empty", ZString.Empty, entry.TI_OriginLRC);
			AssertEquals("Destination LRC should be empty", ZString.Empty, entry.TI_DestinationLRC);
			AssertCollectionContains("No Location matches Origin 'XXXXX'", entry.Errors.Values);
			AssertCollectionContains("No Location matches Destination 'XXX'", entry.Errors.Values);
		}

		public void TestConvert_OriginDestination_IsEmpty_UseEmptyValue()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Origin = "";
			response.Rates[0].Destination = "";

			var entry = ConvertSingleEntry(response);

			AssertEquals("Origin LRC should be empty", ZString.Empty, entry.TI_OriginLRC);
			AssertEquals("Destination LRC should be empty", ZString.Empty, entry.TI_DestinationLRC);
			AssertEquals("Errors collection should be empty", 0, entry.Errors.Count);
		}

		public void TestConvert_OriginDestination_IsNull_UseEmptyValue()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Origin = null;
			response.Rates[0].Destination = null;

			var entry = ConvertSingleEntry(response);

			AssertEquals("Origin LRC should be empty", ZString.Empty, entry.TI_OriginLRC);
			AssertEquals("Destination LRC should be empty", ZString.Empty, entry.TI_DestinationLRC);
			AssertEquals("Errors collection should be empty", 0, entry.Errors.Count);
		}

		#endregion

		#region Container

		public void TestConvert_Container()
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.RC_ISOType = "22G0";
			container.RC_FreightRateClass = "20TD";
			container.RC_HandlingRateClass = "22PF";

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Container = new DTO.RefContainer { Code = "22G0" };
			var entry = ConvertSingleEntry(response);

			AssertEquals("The container primary key should match the entry TI_RC.", container.PK, entry.TI_RC);
			AssertEquals("TI_MatchContainerRateClass should be false.", false, entry.TI_MatchContainerRateClass);
			AssertEquals("The errors collection should be empty.", 0, entry.Errors.Count);
		}

		public void TestConvert_Container_WithCriteria_UseMatchedByISOTypeGroupContainer()
		{
			var container1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container1.RC_ISOType = "AAAA";
			container1.RC_FreightRateClass = "44G0";

			var container2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC");
			container2.RC_ISOType = "BBBB";
			container2.RC_FreightRateClass = "44G0";

			Factory.Save();

			var criteria = ValidCriteria;
			var testContainers = new TestContainers(Factory, container1.PK, 1, container2.PK, 1);
			testContainers.PopulateContainerList(criteria.RateableMeasures);

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Container = new DTO.RefContainer { Code = "CCCC", ISOType = "CCCC", ISOTypeGroup = "44G0" };

			var entries = ConvertAllEntries(response, criteria);

			foreach (var entry in entries)
			{
				AssertEquals($"Entry Errors for RateID {entry.RateId}", 0, entry.Errors.Count);
			}

			AssertEquals("Expected exactly 2 entries.", 2, entries.Count);

			var actualResults = entries
				.Select(e => $"{e.RateId}|{e.Container.PK}")
				.ToArray();

			var expectedResults = new[]
			{
				$"{entries[0].RateId}|{container1.PK}",
				$"{entries[0].RateId}|{container2.PK}",
			};

			AssertContainsExactElementsInAnyOrder(
				"Because it matches the IsoTypeGroup the RS Rate is duplicated for each job container, ignoring the rate container.",
				expectedResults,
				actualResults
			);
		}

		public void TestConvert_Container_CannotBeMatchedOrMapped_GenerateError()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Container = new DTO.RefContainer { Code = "66GP" };
			var entry = ConvertSingleEntry(response);

			var expectedError = @"No Container Type is assigned with the ISO Type or Container Code: 66GP";
			AssertEquals("TI_RC should be empty", ZGuid.Empty, entry.TI_RC);
			AssertCollectionContains(expectedError, entry.Errors.Values);
		}

		public void TestConvert_Container_NoCriteria_HasISOType_UseMatchedByISOTypeContainer()
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.RC_ISOType = "22G0";

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Container = new DTO.RefContainer { Code = "22G0" };
			var entry = ConvertSingleEntry(response, null);

			AssertEquals("The container's primary key should match.", container.PK, entry.TI_RC);
			AssertEquals("The entry's errors should be empty.", 0, entry.Errors.Count);
		}

		public void TestConvert_Container_ISOTypeAssignedToMultipleContainers_CreateARatePerContainer()
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.RC_ISOType = "22G0";
			var container2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container2.RC_ISOType = "22G0";
			var container3 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC");
			container3.RC_ISOType = "22G0";
			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Container = new DTO.RefContainer { Code = "22G0" };

			var converter = new WiseRatesConverter(Factory, Logger);
			var entries = converter.Convert(response, null);

			var actualContainers = entries
				.Select(x => x.Container.RC_Code.ToString())
				.ToList();
			AssertContainsExactElementsInAnyOrder(
				new[] { "20GP", "40GP", "40HC" },
				actualContainers
			);

			var actualErrors = entries.SelectMany(x => x.Errors.Keys).ToList();
			AssertCollectionNotContains(RateEntrySchema.TI_RC, actualErrors);
		}

		public void TestConvert_Container_ISOTypeAssignedToMultipleContainers_And_RateHasErrors_DontAddErrorsToContainerField()
		{
			var container1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container1.RC_ISOType = "42G0";
			var container2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC");
			container2.RC_ISOType = "42G0";
			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Container = new DTO.RefContainer { Code = "42G0" };

			// Make sure the rate has error
			response.Rates[0].Charges[0].ChargeCode = "XXX";

			var converter = new WiseRatesConverter(Factory, Logger);
			var entries = converter.Convert(response, null);

			var actualErrors = entries
				.SelectMany(x => x.Errors)
				.Where(x => x.Key == RateEntrySchema.TI_RC)
				.Select(x => x.Value)
				.ToList();

			AssertEquals("Expected no errors to be added to the container field.", 0, actualErrors.Count);
		}

		public void TestConvert_Container_SameISOTypeAndCodeExist_MatchByISOType()
		{
			var container1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container1.RC_ISOType = "XYZ";

			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container2.RC_Code = "XYZ";
			container2.RC_ISOType = "20GP";

			Factory.Save();

			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.Provider = WRConstants.RateProviders.CargoSphere;
			rate.TransportMode = WRConstants.TransportModes.SEA;
			rate.Container = new DTO.RefContainer { Code = "XYZ" };
			var entry = ConvertSingleEntry(response, null);

			AssertEquals("The entry TI_RC should match the primary key of container1.", container1.PK, entry.TI_RC);
			AssertEquals("The entry Errors should be empty.", 0, entry.Errors.Count);
		}

		public void TestConvert_Container_SameISOTypeAndCodeExist_MatchByCode()
		{
			var container1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container1.RC_ISOType = "XYZ";

			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container2.RC_Code = "XYZ";
			container2.RC_ISOType = "20GP";

			Factory.Save();

			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.Provider = WRConstants.RateProviders.CargoGuide;
			rate.Container = new DTO.RefContainer { Code = "XYZ" };
			var entry = ConvertSingleEntry(response, null);

			AssertEquals("The entry TI_RC should match the primary key of container2", container2.PK, entry.TI_RC);
			AssertEquals("The entry Errors should be empty", 0, entry.Errors.Count);
		}

		public void TestConvert_Container_MatchByISOTypeTakesPriority()
		{
			var container1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container1.RC_ISOType = "XYZ";

			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container2.RC_Code = "XYZ";
			container2.RC_ISOType = "20GP";

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Container = new DTO.RefContainer { Code = "XYZ" };
			var entry = ConvertSingleEntry(response, null);

			AssertEquals("The entry's TI_RC should match the primary key of the first container", container1.PK, entry.TI_RC);
			AssertEquals("The entry's Errors collection should be empty", 0, entry.Errors.Count);
		}

		public void TestConvert_Container_IsEmpty_UseEmptyValue()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Container = null;
			var entry = ConvertSingleEntry(response);

			AssertEquals("Expected TI_RC to be assigned with ZGuid.Empty when Container is null.", ZGuid.Empty, entry.TI_RC);
			AssertEquals("Expected there to be no errors in the entry.", 0, entry.Errors.Count);
		}

		public void TestConvert_Container_PopulateContainerDetailsFromNewContainerProperty()
		{
			var container = Helper.Containers["AKN"];
			AssertNotNull("PRECONDITION: There has to be AKN container in the database", container);

			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Container = new DTO.RefContainer
			{
				Code = "AKN",
				PayloadWeight = 666m,
				PayloadVolume = 1.5m
			};

			var criteria = ValidCriteria;
			var testContainers = new TestContainers(Factory, container.PK, 1);
			testContainers.PopulateContainerList(criteria.RateableMeasures);

			var entry = ConvertSingleEntry(response);

			AssertEquals("TI_RC should match container primary key", container.PK, entry.TI_RC);
			AssertEquals("ContainerPayloadWeightOverride should be 666m", 666m, entry.ContainerPayloadWeightOverride);
			AssertEquals("ContainerPayloadVolumeOverride should be 1.5m", 1.5m, entry.ContainerPayloadVolumeOverride);
			AssertEquals("Errors collection should be empty", 0, entry.Errors.Count);
		}

		#endregion

		#region Service Level

		public void TestConvert_ServiceLevel_HasValue_UseMappedServiceLevelOnTheCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier with SCAC Code";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "QANT");
			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XYZ";
			serviceLevel.PL_CarrierServiceCode = "ABC";

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			var rate = response.Rates.Single();
			rate.Carrier = "QANTAS";
			rate.ServiceLevel = "ABC";
			response.Carriers = new[]
			{
				new RefCarrier { Code = "QANTAS", SCACCode = "QANT" }
			};

			var entry = ConvertSingleEntry(response);

			AssertEquals("The mapped service level should match the expected code.", "XYZ", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals("There should be no errors in the entry.", 0, entry.Errors.Count);
		}

		public void TestConvert_ServiceLevel_NoMapping_GenerateError()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "QNTS";
			carrier.OH_FullName = "Carrier with SCAC Code";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "QANT");

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Rates[0].ServiceLevel = "ABC";
			response.Carriers = new[]
			{
				new RefCarrier { Code = "QANTAS", SCACCode = "QANT" }
			};

			var entry = ConvertSingleEntry(response);

			AssertEquals("Carrier Service Level should be empty when no mapping exists", ZString.Empty, entry.TI_PL_NKCarrierServiceLevel);

			AssertCollectionContains(
				"No Carrier Service Level under Carrier 'QNTS' is assigned to 'ABC'",
				entry.Errors.Values
			);
		}

		public void TestConvert_ServiceLevel_SameCodeMappedTwice_GenerateError()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "QNTS";
			carrier.OH_FullName = "Carrier with SCAC Code";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "QANT");

			Factory.Save();

			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "EXP";
			serviceLevel1.PL_CarrierServiceCode = "EXP";

			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "PRI";
			serviceLevel2.PL_CarrierServiceCode = "EXP";

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Rates[0].ServiceLevel = "EXP";
			response.Carriers = new[]
			{
				new RefCarrier { Code = "QANTAS", SCACCode = "QANT" }
			};

			var entry = ConvertSingleEntry(response);

			AssertEquals(
				"Carrier service level should be empty when a duplicate service code exists.",
				ZString.Empty,
				entry.TI_PL_NKCarrierServiceLevel
			);
			AssertCollectionContains(
				"Service Code 'EXP' under Carrier 'QNTS' has been duplicated and must be unique.",
				entry.Errors.Values
			);
		}

		public void TestConvert_ServiceLevel_RateHasNoCarrier_UseEmptyValue()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].ServiceLevel = "STD";
			response.Rates[0].Carrier = "";

			var entry = ConvertSingleEntry(response);

			AssertEquals("The carrier service level should use an empty value", ZString.Empty, entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals("No errors should be present", 0, entry.Errors.Count);
		}

		public void TestConvert_ServiceLevel_IsEmpty_UseEmptyValue()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].ServiceLevel = "";

			var entry = ConvertSingleEntry(response);

			AssertEquals("TI_PL_NKCarrierServiceLevel should be an empty string", ZString.Empty, entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals("Errors should be empty", 0, entry.Errors.Count);
		}

		public void TestConvert_ServiceLevel_IsStandard_WhenNoMapping_UseStandard()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].ServiceLevel = OrgCarrierServiceLevel.StandardCode;

			var entry = ConvertSingleEntry(response);

			AssertEquals(
				"TI_PL_NKCarrierServiceLevel should match the expected service level.",
				OrgCarrierServiceLevel.StandardCode,
				entry.TI_PL_NKCarrierServiceLevel
			);
			AssertEquals(0, entry.Errors.Count);
		}

		public void TestConvert_ServiceLevel_IsStandard_WhenMapped_UseMappedValue()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier with SCAC Code";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "QANT");
			// map STD to XYZ
			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XYZ";
			serviceLevel.PL_CarrierServiceCode = OrgCarrierServiceLevel.StandardCode;

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Rates[0].ServiceLevel = OrgCarrierServiceLevel.StandardCode;
			response.Carriers = new[]
			{
				new RefCarrier { Code = "QANTAS", SCACCode = "QANT" }
			};

			var entry = ConvertSingleEntry(response);

			AssertEquals("Expected mapped carrier service level 'XYZ'", "XYZ", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals("The errors collection should be empty", 0, entry.Errors.Count);
		}

		public void TestConvert_ServiceLevel_IsStandard_WhenMappedWithCsv_UseMappedValue()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier with SCAC Code";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "QANT");
			// Map A,B to XYZ
			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XYZ";
			serviceLevel.PL_CarrierServiceCode = "A,B";

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Carrier = "QANTAS";
			response.Rates[0].ServiceLevel = "B";
			response.Carriers = new[]
			{
				new RefCarrier { Code = "QANTAS", SCACCode = "QANT" }
			};

			var entry = ConvertSingleEntry(response);

			AssertEquals("Expected service level to map correctly.", "XYZ", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals("Expected no errors in the entry.", 0, entry.Errors.Count);
		}

		public void TestConvert_ServiceLevel_WhenUnmappedLocalCodeMatchUniversalCode_UseLocalCode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier with SCAC Code";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "QANT");

			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XYZ";                                   // => same code
			serviceLevel.PL_CarrierServiceCode = ZString.Empty;             // => unmapped

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			var rate = response.Rates.Single();
			rate.Carrier = "QANTAS";
			rate.ServiceLevel = "XYZ";                                      // => Universal code
			response.Carriers = new[]
			{
				new RefCarrier { Code = "QANTAS", SCACCode = "QANT" }
			};

			var entry = ConvertSingleEntry(response);

			AssertEquals("PL_Code should match rate's Service Level", "XYZ", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals(0, entry.Errors.Count);
		}

		public void TestConvert_ServiceLevel_OneUnmappedLocalCodeSameUniversalCode_AnotherLocalCodeMappedToUniversalCode_PreferMappedOne()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier with SCAC Code";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "QANT");

			var serviceLevelToBeSkipped = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevelToBeSkipped.PL_Code = "XYZ";                        // => same code
			serviceLevelToBeSkipped.PL_CarrierServiceCode = ZString.Empty;  // => unmapped
			serviceLevelToBeSkipped.PL_CarrierServiceLevelDescription = "This code does not match";

			var serviceLevelToBeMatched = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevelToBeMatched.PL_Code = "ABC";                        // => different code
			serviceLevelToBeMatched.PL_CarrierServiceCode = "XYZ";          // => mapped
			serviceLevelToBeMatched.PL_CarrierServiceLevelDescription = "This code should match";

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			var rate = response.Rates.Single();
			rate.Carrier = "QANTAS";
			rate.ServiceLevel = "XYZ";                                      // => Universal code
			response.Carriers = new[]
			{
				new RefCarrier { Code = "QANTAS", SCACCode = "QANT" }
			};

			var entry = ConvertSingleEntry(response);

			AssertEquals(
				"when matching rate's Service Level, PL_CarrierServiceCode is preferred than PL_Code",
				"ABC",
				entry.TI_PL_NKCarrierServiceLevel
			);

			AssertEquals(0, entry.Errors.Count);
		}

		public void TestConvert_ServiceLevel_PreferProductCode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier with SCAC Code";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "QANT");
			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "EXP";
			serviceLevel1.PL_CarrierServiceCode = "EXP";
			serviceLevel1.PL_ProductCode = "PREMIUM";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Express";
			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "STD";
			serviceLevel2.PL_CarrierServiceCode = "STD";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Standard";

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			var rate = response.Rates.Single();
			rate.Carrier = "QANTAS";
			rate.ServiceLevel = "STD";
			response.Carriers = new[]
			{
				new RefCarrier { Code = "QANTAS", SCACCode = "QANT" }
			};

			// When no product code then should match on CarrierServiceCode...
			var entry = ConvertSingleEntry(response);
			AssertEquals("STD", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals(0, entry.Errors.Count);

			// When rate has product code that matches org service level product then take that match
			rate.ProviderCustomFields = new[] { new CustomField() { Code = Rate.CustomFields.Cargoguide.ProductCode, Value = "PREMIUM" } };
			entry = ConvertSingleEntry(response);
			AssertEquals("EXP", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals(0, entry.Errors.Count);

			// When rate has product code that does NOT match org service level product then fallback to universal code match
			rate.ProviderCustomFields = new[] { new CustomField() { Code = Rate.CustomFields.Cargoguide.ProductCode, Value = "XYZZY" } };
			entry = ConvertSingleEntry(response);
			AssertEquals("STD", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals(0, entry.Errors.Count);
		}

		public void TestConvert_ServiceLevel_PreferCommaSeparatedProductCode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier with SCAC Code";
			carrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(carrier, "QANT");
			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "EXP";
			serviceLevel1.PL_CarrierServiceCode = "EXP";
			serviceLevel1.PL_ProductCode = "P.EX,PR,EX";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Express";
			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "STD";
			serviceLevel2.PL_CarrierServiceCode = "STD";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Standard";

			Factory.Save();

			var response = ValidSEARatesSearchResponse;
			var rate = response.Rates.Single();
			rate.Carrier = "QANTAS";
			rate.ServiceLevel = "STD";
			response.Carriers = new[]
			{
				new RefCarrier { Code = "QANTAS", SCACCode = "QANT" }
			};

			// When no product code then should match on CarrierServiceCode
			var entry = ConvertSingleEntry(response);
			AssertEquals( "Carrier service level mismatch when no product code is provided", "STD", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals("Errors should be empty when no product code is provided", 0, entry.Errors.Count);

			// When rate has product code that matches org service level product then take that match
			rate.ProviderCustomFields = new[] { new CustomField() { Code = Rate.CustomFields.Cargoguide.ProductCode, Value = "PR" } };
			entry = ConvertSingleEntry(response);
			AssertEquals("Carrier service level mismatch when rate product code matches service level product", "EXP", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals("Errors should be empty when rate product code matches service level product", 0, entry.Errors.Count);

			// When rate has product code that does NOT match org service level product then fallback to universal code match
			rate.ProviderCustomFields = new[] { new CustomField() { Code = Rate.CustomFields.Cargoguide.ProductCode, Value = "XYZZY" } };
			entry = ConvertSingleEntry(response);
			AssertEquals("Carrier service level mismatch when rate product code does not match service level product", "STD", entry.TI_PL_NKCarrierServiceLevel);
			AssertEquals("Errors should be empty when rate product code does not match service level product", 0, entry.Errors.Count);
		}

		public void TestConvertServiceLevel_ProductCode_WhenCarrierNull()
		{
			// No carrier means no match on service level, so expect blank value even if product code is present
			var converter = new WiseRatesConverter(Factory, Logger);
			var actual = converter.ConvertServiceLevel("EXP", new[] { new CustomField() { Code = Rate.CustomFields.Cargoguide.ProductCode, Value = "PREMIUM" } }, null);

			AssertEquals("The code should be an empty string when no carrier is provided.", ZString.Empty, actual.code);
			AssertNull("The error should be null when no carrier is provided.", actual.error);
		}

		#endregion

		#region Rate Category

		public void TestConvert_RateCategory_ChargeCodeCannotBeConverted_GenerateError()
		{
			Factory.Save();

			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.Charges[0].ChargeCode = "XXX";

			var entry = ConvertSingleEntry(response);
			AssertEquals("The RateCategory should be empty when charges cannot be converted.", ZString.Empty, entry.TI_RateCategory);
			AssertCollectionContains(
				"Category cannot be identified as the Charge(s) under the Rate cannot be converted into CW1 Charges. Please check validation errors of the Rate line.",
				entry.Errors.Values
			);
		}

		public void TestConvert_RateCategory_ChargeCodeOnLinesHaveNoGroup_GenerateError()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			TestFRT.AC_ChargeGroup = "";

			Factory.Save();

			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.Charges[0].ChargeCode = TestFRT.AC_Code;

			var entry = ConvertSingleEntry(response);

			AssertEquals("Expected TI_RateCategory to be empty", ZString.Empty, entry.TI_RateCategory);
			AssertCollectionContains(
				"Category cannot be identified as the Charge(s) under the Rate has no Charge Code Group selected.",
				entry.Errors.Values
			);
		}

		public void TestConvert_RateCategory_ChargeCodesOnLinesHaveDifferentChargeCodeGroup_GenerateError()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			TestFRT.AC_ChargeGroup = "FRT";
			TestADF.AC_ChargeGroup = "ORG";
			TestAWB.AC_ChargeGroup = "";        // Invalid group on one of charges should put all charges under the same rate

			Factory.Save();

			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.Charges = new[]
			{
				new Charge { ChargeCode = TestFRT.AC_Code, Currency = "AUD", FlatRate = 100m },
				new Charge { ChargeCode = TestADF.AC_Code, Currency = "AUD", FlatRate = 200m },
				new Charge { ChargeCode = TestAWB.AC_Code, Currency = "AUD", FlatRate = 300m }
			};

			var entry = ConvertSingleEntry(response);
			AssertEquals("Rate category should be empty when charges belong to different charge code groups.", ZString.Empty, entry.TI_RateCategory);
			AssertCollectionContains(
				"Category cannot be identified as the Charge(s) under the Rate are placed under different Charge Code Groups.",
				entry.Errors.Values
			);
		}

		public void TestConvert_RateCategory_RateHasChargesFromDifferentChargeCodeGroups_SplitRatePerChargeCodeGroup()
		{
			var response = ValidAIRRatesSearchResponse;
			response.ChargeCodes = new[]
			{
				new RefChargeCode { Code = "FRT" },
				new RefChargeCode { Code = "OCART" },
				new RefChargeCode { Code = "DCART" },
			};

			var rate = response.Rates[0];
			rate.Origin = "UAIEV";
			rate.Destination = "AUSYD";
			rate.TransportMode = "AIR";
			rate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 100m },
				new Charge { ChargeCode = "OCART", Currency = "USD", FlatRate = 200m },
				new Charge { ChargeCode = "DCART", Currency = "UAH", FlatRate = 300m },
			};

			Helper.ChargeCodes["FRT"].AC_ChargeGroup = "FRT";
			Helper.ChargeCodes["OCART"].AC_ChargeGroup = "ORG";
			Helper.ChargeCodes["DCART"].AC_ChargeGroup = "DST";

			Factory.Save();

			var converter = new WiseRatesConverter(Factory, Logger);

			ConversionOptions conversionOptions;
			conversionOptions.AddRateModeAndCategoryValidation = true;

			var entries = converter.Convert(response, ValidCriteria, conversionOptions);

			var expectedEntries = new[]
			{
				new WiseEntry(rate, Factory)
				{
					TI_RateCategory = "AIR",
					TI_OriginLRC = "UAIEV",
					TI_DestinationLRC = "AUSYD",
					ChildRateLines = new[]
					{
						new WiseLine(Factory, rate.Charges[0])
						{
							TL_AC = Helper.ChargeCodes["FRT"].PK,
							TL_RX_NKCurrency = "AUD"
						}
					}
				},
				new WiseEntry(rate, Factory)
				{
					TI_RateCategory = "ORG",
					TI_OriginLRC = "UAIEV",
					TI_DestinationLRC = "AUSYD",
					ChildRateLines = new[]
					{
						new WiseLine(Factory, rate.Charges[1])
						{
							TL_AC = Helper.ChargeCodes["OCART"].PK,
							TL_RX_NKCurrency = "USD"
						}
					}
				},
				new WiseEntry(rate, Factory)
				{
					TI_RateCategory = "DST",
					TI_OriginLRC = "UAIEV",
					TI_DestinationLRC = "AUSYD",
					ChildRateLines = new[]
					{
						new WiseLine(Factory, rate.Charges[2])
						{
							TL_AC = Helper.ChargeCodes["DCART"].PK,
							TL_RX_NKCurrency = "UAH"
						}
					}
				}
			};

			var actual = entries.Select(e => new
			{
				e.TI_RateCategory,
				e.TI_OriginLRC,
				e.TI_DestinationLRC,
				ChildRateLines = string.Join(", ", e.ChildRateLines.Select(l => $"{l.TL_AC}|{l.TL_RX_NKCurrency}"))
			}).ToList();

			var expected = expectedEntries.Select(e => new
			{
				e.TI_RateCategory,
				e.TI_OriginLRC,
				e.TI_DestinationLRC,
				ChildRateLines = string.Join(", ", e.ChildRateLines.Select(l => $"{l.TL_AC}|{l.TL_RX_NKCurrency}"))
			}).ToList();

			AssertContainsExactElementsInAnyOrder(
				"The resulting entries should match the expected entries",
				expected,
				actual
			);
		}

		public void TestConvert_RateCategory_AllLinesHaveSameChargeCodeGroup_ConvertWithoutErrors()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			AssertRateCategory("FRT", "AIR", "FCL", "AIR");
			AssertRateCategory("FRT", "AIR", "LCL", "AIR");
			AssertRateCategory("FRT", "SEA", "FCL", "FCL");
			AssertRateCategory("FRT", "SEA", "LCL", "LCL");
			AssertRateCategory("ORG", "AIR", "FCL", "ORG");
			AssertRateCategory("ORG", "AIR", "LCL", "ORG");
			AssertRateCategory("ORG", "SEA", "FCL", "ORG");
			AssertRateCategory("ORG", "SEA", "LCL", "ORG");
			AssertRateCategory("DST", "AIR", "FCL", "DST");
			AssertRateCategory("DST", "AIR", "LCL", "DST");
			AssertRateCategory("DST", "SEA", "FCL", "DST");
			AssertRateCategory("DST", "SEA", "LCL", "DST");
		}

		void AssertRateCategory(string chargeCodeGroup, string wrTransportMode, string wrContainerMode, string expectedCategory)
		{
			TestFRT.AC_ChargeGroup = chargeCodeGroup;

			Factory.Save();

			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.TransportMode = wrTransportMode;
			rate.ContainerMode = wrContainerMode;
			rate.Charges[0].ChargeCode = TestFRT.AC_Code;

			var entry = ConvertSingleEntry(response);
			AssertEquals(
				$"chargeCodeGroup={chargeCodeGroup}, wrTransportMode={wrTransportMode}, wrContainerMode={wrContainerMode}",
				expectedCategory,
				entry.TI_RateCategory
			);
			AssertEquals(0, entry.Errors.Count);
		}

		#endregion

		#region Rate Mode

		public void TestConvert_RateMode_CategoryCannotBeConverted_GenerateError()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			TestFRT.AC_ChargeGroup = ""; // Category cannot be converted without charge code group on a charge

			Factory.Save();

			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.TransportMode = "SEA";
			rate.ContainerMode = "FCL";
			rate.Charges[0].ChargeCode = TestFRT.AC_Code;

			var entry = ConvertSingleEntry(response);

			AssertEquals("Expected TI_Mode to be an empty string", ZString.Empty, entry.TI_Mode);

			AssertCollectionContains(
				"Transport Mode cannot be identified as Category on the Rate is not identified. Please check validation errors of the Category.",
				entry.Errors.Values
			);
		}

		public void TestConvert_RateMode_UnsupportedCombinationOfTransportAndContainerMode_GenerateError()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			TestFRT.AC_ChargeGroup = "FRT";

			Factory.Save();

			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.TransportMode = "STARSHIP";
			rate.ContainerMode = "FCL";
			rate.Charges[0].ChargeCode = TestFRT.AC_Code;

			var entry = ConvertSingleEntry(response);

			AssertEquals("Expected TI_Mode to be an empty string when unsupported transport and container mode combination is used.", ZString.Empty, entry.TI_Mode);
			AssertCollectionContains(
				"Transport Mode cannot be identified for the Rate with error report sent to WTG. Please refer to the Autorating Log for details.",
				entry.Errors.Values
			);
		}

		public void TestConvert_RateMode_RateCategoryTransportAndContainerModeAreValid_ConvertWithoutErrors()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			AssertRateMode("AIR", "FCL", "ULD");
			AssertRateMode("AIR", "LCL", "LSE");
			AssertRateMode("AIR", "", "AIR");
			AssertRateMode("SEA", "LCL", "LCL");
			AssertRateMode("SEA", "", "SEA");
		}

		void AssertRateMode(string wrTransportMode, string wrContainerMode, string expectedMode, bool isFCLRateCategory = true)
		{
			TestFRT.AC_ChargeGroup = isFCLRateCategory ? "FRT" : "ORG";

			Factory.Save();

			var response = ValidAIRRatesSearchResponse;
			var rate = response.Rates[0];
			rate.TransportMode = wrTransportMode;
			rate.ContainerMode = wrContainerMode;
			rate.Charges[0].ChargeCode = TestFRT.AC_Code;

			var entry = ConvertSingleEntry(response);
			AssertEquals(Invariant($"wrTransportMode={wrTransportMode}, wrContainerMode={wrContainerMode}, isFCLRateCategory={isFCLRateCategory}"), expectedMode, entry.TI_Mode);
			AssertEquals(0, entry.Errors.Count);
		}

		#endregion

		#region Restricted charges

		public void TestConvert_Charge_Restricted_ToCallForPricing()
		{
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Charges[0].Restricted = true;

			var entry = ConvertSingleEntry(response);

			AssertEquals(
				"The ChildRateLineItem's TM_CallForPricing should be ZBool.True when the charge is restricted.",
				ZBool.True,
				entry.ChildRateLines.Single().ChildRateLineItems.Single().TM_CallForPricing
			);
		}

		public void TestConvert_Charge_Restricted_HasNoCurrency()
		{
			var response = ValidSEARatesSearchResponse;
			var charge = response.Rates[0].Charges[0];
			// If it's marked as restricted, then currency is not copied over
			// during conversion.
			charge.Restricted = true;

			charge.Currency = "EUR";
			var entry = ConvertSingleEntry(response);

			AssertEquals(ZBool.True, entry.ChildRateLines.Single()
				.ChildRateLineItems.Single().TM_CallForPricing);

			AssertContainsExactElementsInAnyOrder(
				new[] { (string)null },
				entry.ChildRateLines.Select(x => x.Currency)
			);

			AssertEquals(0, ErrorReporter.ExceptionsThrown.Count);

			charge.Currency = "USD";
			entry = ConvertSingleEntry(response);

			AssertEquals(ZBool.True, entry.ChildRateLines.Single()
				.ChildRateLineItems.Single().TM_CallForPricing);

			AssertContainsExactElementsInAnyOrder(
				new[] { (string)null },
				entry.ChildRateLines.Select(x => x.Currency)
			);

			AssertEquals(0, ErrorReporter.ExceptionsThrown.Count);
		}

		public void TestConvert_Charge_Restricted_TACTShouldConvertCurrency()
		{
			var response = ValidAIRRatesSearchResponse;
			var charge = response.Rates.First().Charges.First();
			charge.Restricted = true;
			charge.Applicability = RateApplicableCode.GetDescription(RateApplicableCode.Tact);

			charge.Currency = "EUR";
			var entry = ConvertSingleEntry(response);
			AssertEquals(entry.ChildRateLines.Single().Currency.Code, "EUR");

			charge.Currency = string.Empty;
			entry = ConvertSingleEntry(response);
			var line = entry.ChildRateLines.Single() as WiseLine;
			AssertContainsExactElementsInAnyOrder(line.Errors.Select(x => x.Value), new[] { "Currency cannot be identified for the Rate with error report sent to WTG. Please refer to the Autorating Log for details." });
			AssertEquals(ErrorReporter.LastMessageReported, "[TraceID: traceid] Charge from the Rates Service has no Currency");
			ErrorReporter.Clear();
		}

		public void TestConvert_Charge_Percentage_WithOrWithoutCurrency()
		{
			var response = ValidSEARatesSearchResponse;
			var charge = response.Rates[0].Charges[0];
			charge.PercentageAppliesTo = "XYZ";

			charge.Currency = string.Empty;
			var entry = ConvertSingleEntry(response);
			var line = entry.ChildRateLines.Single() as WiseLine;
			var actualErrors = line.Errors.Select(x => x.Value).ToArray();
			var expectedErrors = new[] { "Currency cannot be identified for the Rate with error report sent to WTG. Please refer to the Autorating Log for details." };
			AssertContainsExactElementsInAnyOrder("Verify error message when no currency is identified", expectedErrors, actualErrors);

			AssertEquals("[TraceID: traceid] Charge from the Rates Service has no Currency", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			charge.Currency = "USD";
			entry = ConvertSingleEntry(response);
			var actualCurrencies = entry.ChildRateLines.Select(x => x.Currency.Code).ToArray();
			var expectedCurrencies = new[] { "USD" };
			AssertContainsExactElementsInAnyOrder("Verify currency when provided as USD", expectedCurrencies, actualCurrencies);

			AssertEquals("Logger should not contain any errors or warnings", 0, Logger.GetErrorsAndWarnings().Count());
			AssertEquals("No exceptions should be thrown by the error reporter", 0, ErrorReporter.ExceptionsThrown.Count);
		}

		public void TestConvert_Charge_NotRestricted_ToNotCallForPricing()
		{
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].Charges[0].Restricted = false;

			var entry = ConvertSingleEntry(response);

			var actualValue = entry.ChildRateLines.Single()
				.ChildRateLineItems.Single()
				.TM_CallForPricing;

			AssertEquals("TM_CallForPricing should be false for non-restricted charge", ZBool.False, actualValue);
		}

		#endregion

		#region Charge Code

		public void TestConvert_ChargeCode_LocalChargeCodeWithUniversalChargeCodeExists_UseLocalCode()
		{
			var chargeCodes = new[]
			{
				CreateChargeCode("UXXX", "", Env.CurrentCompanyPK),
				CreateChargeCode("XXX", "UXXX", Env.CurrentCompanyPK),	// X
				CreateChargeCode("UXXX", "", ZGuid.Empty),
				CreateChargeCode("XXX", "", ZGuid.Empty)
			};

			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Charges[0].ChargeCode = "UXXX";

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals(
				"Local charge code with matched universal charge code takes priority",
				chargeCodes[1].PK,
				line.TL_AC
			);

			AssertEquals(0, line.Errors.Count);
		}

		public void TestConvert_ChargeCode_RelatedGlobalChargeCodeWithUniversalChargeCodeExists_UseLocalCode()
		{
			var chargeCodes = new[]
			{
				CreateChargeCode("UXXX", "", Env.CurrentCompanyPK),
				CreateChargeCode("XXX", "", Env.CurrentCompanyPK),		// X
				CreateChargeCode("UXXX", "", ZGuid.Empty),
				CreateChargeCode("XXX", "UXXX", ZGuid.Empty)
			};

			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Charges[0].ChargeCode = "UXXX";

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals(
				"Local charge code with empty universal charge code group is preferred global charge code with the same code and matched universal charge code found",
				chargeCodes[1].PK,
				line.TL_AC
			);
			AssertEquals(0, line.Errors.Count);
		}

		public void TestConvert_ChargeCode_LocalChargeCodeWithCodeEqualUniversalChargeCodeExists_UseLocalCode()
		{
			var chargeCodes = new[]
			{
				CreateChargeCode("UXXX", "", Env.CurrentCompanyPK),       // X
				CreateChargeCode("XXX", "YYY", Env.CurrentCompanyPK),
				CreateChargeCode("UXXX", "", ZGuid.Empty),
				CreateChargeCode("XXX", "", ZGuid.Empty),
			};

			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Charges[0].ChargeCode = "UXXX";

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals(
				"Local charge code with the code matching universal charge code is preferred if no charge code with matched by universal charge code found",
				chargeCodes[0].PK,
				line.TL_AC
			);

			AssertEquals(0, line.Errors.Count);
		}

		public void TestConvert_ChargeCode_IsEmpty_GenerateError()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Charges[0].ChargeCode = "";

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals("The charge code should be an empty GUID", ZGuid.Empty, line.TL_AC);
			AssertCollectionContains("The charge code has NO mapping with any Universal Charge Code.", line.Errors.Values);
		}

		AccChargeCode CreateChargeCode(string code, string universalCode, ZGuid company)
		{
			var newChargeCode = Factory.New<AccChargeCode>();
			newChargeCode.AC_GC = company;
			newChargeCode.AC_Code = code;
			newChargeCode.AC_Desc = "McLaren";
			newChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			newChargeCode.AC_RateCalculator = "FLT";
			newChargeCode.AC_ChargeGroup = "FRT";

			if (!string.IsNullOrWhiteSpace(universalCode))
			{
				var mapping = newChargeCode.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Code = universalCode;
			}

			Factory.Save();

			return newChargeCode;
		}

		#endregion

		#region Unit

		public void ConvertUnit_UnitIsSpecified_UseSpecifiedUnit()
		{
			var response = ValidSEARatesSearchResponse;
			var rate = response.Rates[0];
			var charge = rate.Charges[0];
			charge.Unit = Constants.Weight.Grams;

			var entry = ConvertSingleEntry(response);
			var line = entry.ChildRateLines.First();
			AssertEquals("The weight unit should match the specified unit.", Constants.Weight.Grams, line.TL_WeightVolume);
		}

		public void ConvertUnit_FlatChargeCMBCalculator_UseDefaultUnit()
		{
			var response = ValidSEARatesSearchResponse;
			var rate = response.Rates[0];
			rate.Provider = WRConstants.RateProviders.CargoGuide;
			rate.TransportMode = "AIR";
			rate.ContainerMode = "LCL";

			var charge = rate.Charges[0];
			charge.Unit = string.Empty;
			charge.FlatRate = 100;
			charge.PerUnitRate = null;
			charge.Break = null;
			charge.BreakOperator = string.Empty;

			var entry = ConvertSingleEntry(response);
			var line = entry.ChildRateLines.First();
			AssertEquals("The weight volume should match the expected constant for kilograms.", Constants.Weight.Kilograms, line.TL_WeightVolume);
		}

		#endregion

		#region Calculators

		#region CMB

		public void TestConvert_Charge_NullableFieldsAreAllNull_NullCalculatorPicked()
		{
			var response = ValidSEARatesSearchResponse;
			response.Rates[0].ContainerMode = Constants.ContainerModes.FCL;
			response.Rates[0].TransportMode = Constants.TransportModes.Sea;

			response.Rates[0].Charges[0].FlatRate = null;
			response.Rates[0].Charges[0].Comment = "Aardvark";

			var entry = ConvertSingleEntry(response);

			AssertType(
				typeof(NullCalculator),
				entry.ChildRateLines.Single().Calculator
			);

			var singleLog = Logger.GetAllLogs().Single();
			AssertEquals(
				"Error:Could not create calculator from:\r\n- ChargeCode: FRT\r\n  Currency: AUD\r\n  Comment: Aardvark\r\n  ProviderCustomFields: []\r\n",
				singleLog
			);
		}

		public void TestConvert_ShouldCreateCombinedCalculatorWith_Breaks_AndOnlyOneCharge_WhenProviderIsCargoguide()
		{
			var response = ValidAIRRatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var firstRate = response.Rates[0];
			firstRate.RatesServiceProvider = WRConstants.RateProviders.CargoGuide;
			firstRate.Provider = WRConstants.RateProviders.CargoGuide;
			firstRate.ContainerMode = "LCL";
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", Break = 0, BreakOperator = ">=", PerUnitRate = 7.4M },
			};

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals("The charge code should match the expected FRT charge code.", chargeCodeFRT.PK, line.TL_AC);
			AssertEquals("The rate calculator should be set to the combined calculator.", CombinedCalculator.Code, line.TL_RateCalculator);

			var rateLineItems = line.ChildRateLineItems.ToList();

			var breaksPlus = rateLineItems.Where(i => i.TM_Type == Calculator.Items.Operator.Plus).Select(x => x.TM_Value);
			var breakPoints = rateLineItems
				.Where(i => i.TM_Type == Calculator.Items.Operator.Plus || i.TM_Type == Calculator.Items.Operator.Minus)
				.Where(x => x.TM_Break == 1m);
			var breaksMinus = rateLineItems.Where(i => i.TM_Type == Calculator.Items.Operator.Minus).Select(x => x.TM_Value);

			AssertContainsExactElementsInAnyOrder("The breaks with plus operator should match the expected values.", new[] { (ZDecimal)7.4M }, breaksPlus);
			AssertContainsExactElementsInAnyOrder("The breaks with minus operator should match the expected values.", new[] { (ZDecimal)7.4M }, breaksMinus);
			AssertEquals("The number of break points should be 2.", 2, breakPoints.Count());
		}

		public void TestConvert_ShouldCreateCombinedCalculatorWith_Min_Max_Breaks_WhenProviderIsCargoguide()
		{
			var response = ValidAIRRatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var firstRate = response.Rates[0];
			firstRate.RatesServiceProvider = WRConstants.RateProviders.CargoGuide;
			firstRate.Provider = WRConstants.RateProviders.CargoGuide;
			firstRate.ContainerMode = "LCL";
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", Break = 1, MinRate = 150, MaxRate = 10000, BreakOperator = ">", PerUnitRate = 7.4M },
				new Charge { ChargeCode = "FRT", Currency = "AUD", Break = 50, MinRate = 150, MaxRate = 10000, BreakOperator = ">", PerUnitRate = 6M }
			};

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals("Expected charge code to match", chargeCodeFRT.PK, line.TL_AC);
			AssertEquals("Expected rate calculator to match", CombinedCalculator.Code, line.TL_RateCalculator);

			var rateLineItems = line.ChildRateLineItems.ToList();

			var minCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.MIN);
			AssertEquals("Expected minimum charge to match", 150M, minCharge.TM_Value);

			var maxCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.MAX);
			AssertEquals("Expected maximum charge to match", 10000M, maxCharge.TM_Value);

			var breaks = rateLineItems.Where(i => i.TM_Type == Calculator.Items.Operator.Plus).Select(x => x.TM_Value).ToList();
			var expectedBreaks = new[] { (ZDecimal)7.4M, (ZDecimal)6M };
			AssertContainsExactElementsInAnyOrder("Expected breaks to match", expectedBreaks, breaks);
		}

		public void TestConvert_PerEquipmentCalculatorWithPivotSpecification_ConvertToCMBCalculatorWithMultipleEquipmentsOverMaxWeightVolumeSet()
		{
			var response = ValidAIRRatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var firstRate = response.Rates[0];
			firstRate.RatesServiceProvider = WRConstants.RateProviders.CargoGuide;
			firstRate.Provider = WRConstants.RateProviders.CargoGuide;
			firstRate.ContainerMode = "FCL";
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", EquipmentUnit = "CN", Unit = "CN", PerUnitRate = 100m },
				new Charge { ChargeCode = "FRT", Currency = "AUD", EquipmentUnit = "CN", Unit = "KG", PerUnitRate = 1.8m, Break = 1600, BreakOperator = ">=", BreakUnit = "KG", ActualPercentage = 100 }
			};

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals("Expected charge code to match FRT's primary key.", chargeCodeFRT.PK, line.TL_AC);
			AssertEquals("Expected calculator to be CombinedCalculator.", CombinedCalculator.Code, line.TL_RateCalculator);

			var rateLineItems = line.ChildRateLineItems.ToList();

			var minCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.Minus);
			AssertEquals("Expected TM_Break to be 1600 for the minimum charge item.", 1600m, minCharge.TM_Break);
			AssertEquals("Expected TM_FlatAmount to be 100 for the minimum charge item.", 100m, minCharge.TM_FlatAmount);
			AssertEquals("Expected TM_Value to be 0 for the minimum charge item.", 0m, minCharge.TM_Value);

			var maxCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.Plus);
			AssertEquals("Expected TM_Break to be 1600 for the maximum charge item.", 1600m, maxCharge.TM_Break);
			AssertEquals("Expected TM_FlatAmount to be 100 for the maximum charge item.", 100m, maxCharge.TM_FlatAmount);
			AssertEquals("Expected TM_Value to be 1.8 for the maximum charge item.", 1.8m, maxCharge.TM_Value);

			Assert("Expected MultipleEquipmentsOverMaxWeightVolume to be true.", line.Calculator.MultipleEquipmentsOverMaxWeightVolume);
			Assert("Expected IsAccumulated to be true.", line.Calculator.IsAccumulated);
			AssertEquals("Expected TL_ActualPercentage to be 100.", (byte)100, line.TL_ActualPercentage);
		}

		public void TestConvert_PerEquipmentCalculator_ConvertToCMBCalculatorWithPerUnitRate()
		{
			var response = ValidAIRRatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];
			var chargeCodeBAF = Helper.ChargeCodes["BAF"];

			var rate = response.Rates[0];
			rate.RatesServiceProvider = WRConstants.RateProviders.CargoGuide;
			rate.Provider = WRConstants.RateProviders.CargoGuide;
			rate.ContainerMode = "FCL";
			rate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", EquipmentUnit = "CN", Unit = "CN", PerUnitRate = 100m },
				new Charge { ChargeCode = "BAF", Currency = "AUD", Unit = "KG", PerUnitRate = 0.5m },
			};

			var entry = ConvertSingleEntry(response);

			var lineFRT = entry.ChildRateLines.Cast<WiseLine>().First(l => l.ChargeCode.AC_Code == "FRT");
			AssertEquals("Expected TL_RateCalculator to match CombinedCalculator.Code for lineFRT.", CombinedCalculator.Code, lineFRT.TL_RateCalculator);
			AssertEquals("Expected MultipleEquipmentsOverMaxWeightVolume to be true for lineFRT.", true, lineFRT.Calculator.MultipleEquipmentsOverMaxWeightVolume);

			var perUnitCharge = lineFRT.ChildRateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.UNT);
			AssertEquals("Expected TM_Value to be 100m for lineFRT's per unit charge.", 100m, perUnitCharge.TM_Value);

			var lineBAF = entry.ChildRateLines.Cast<WiseLine>().First(l => l.ChargeCode.AC_Code == "BAF");
			AssertEquals("Expected TL_RateCalculator to match CombinedCalculator.Code for lineBAF.", CombinedCalculator.Code, lineBAF.TL_RateCalculator);
			AssertEquals("Since it is not per CN charge but per weight charge, MultipleEquipmentsOverMaxWeightVolume should be false for lineBAF.", false, lineBAF.Calculator.MultipleEquipmentsOverMaxWeightVolume);

			perUnitCharge = lineBAF.ChildRateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.UNT);
			AssertEquals("Expected TM_Value to be 0.5m for lineBAF's per unit charge.", 0.5m, perUnitCharge.TM_Value);
		}

		public void TestConvert_ShouldCreateCombinedCalculatorWith_Min_Max_Unit_WhenProviderIsCargoguide()
		{
			var response = ValidAIRRatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var firstRate = response.Rates[0];
			firstRate.RatesServiceProvider = WRConstants.RateProviders.CargoGuide;
			firstRate.Provider = WRConstants.RateProviders.CargoGuide;
			firstRate.ContainerMode = "LCL";
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", MinRate = 150, MaxRate = 10000, PerUnitRate = 6M }
			};

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals("Charge code should match", chargeCodeFRT.PK, line.TL_AC);
			AssertEquals("Rate calculator should be combined calculator", CombinedCalculator.Code, line.TL_RateCalculator);

			var rateLineItems = line.ChildRateLineItems.ToList();

			var minCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.MIN);
			AssertEquals("Minimum charge value should be 150", 150M, minCharge.TM_Value);

			var maxCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.MAX);
			AssertEquals("Maximum charge value should be 10000", 10000M, maxCharge.TM_Value);

			var unitCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.UNT);
			AssertEquals("Unit charge value should be 6", 6M, unitCharge.TM_Value);
		}

		public void TestConvert_Calculator_CMB_MinChargeable()
		{
			var response = ValidAIRRatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var firstRate = response.Rates[0];
			firstRate.RatesServiceProvider = WRConstants.RateProviders.CargoGuide;
			firstRate.Provider = WRConstants.RateProviders.CargoGuide;
			firstRate.ContainerMode = "FCL";
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", MinChargeable = 200,  PerUnitRate = 6M }
			};

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals(
				"Expected charge code to match FRT primary key.",
				chargeCodeFRT.PK,
				line.TL_AC
			);
			AssertEquals(
				"Expected the rate calculator to match CombinedCalculator code.",
				CombinedCalculator.Code,
				line.TL_RateCalculator
			);

			var rateLineItems = line.ChildRateLineItems.ToList();

			var minCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.MIN);
			AssertEquals(
				"Expected min charge break to be 200.",
				200m,
				minCharge.TM_Break
			);
			AssertEquals(
				"Expected min charge value to be 0.",
				0m,
				minCharge.TM_Value
			);

			var unitCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.UNT);
			AssertEquals(
				"Expected unit charge value to be 6.",
				6M,
				unitCharge.TM_Value
			);
		}

		#endregion

		public void TestConvert_ShouldCreateFreightInclusiveCalculator()
		{
			var chargeCodeBAF = Helper.ChargeCodes["BAF"];
			var chargeCodeCAF = Helper.ChargeCodes["CAF"];

			var response = ValidAIRRatesSearchResponse;
			var firstRate = response.Rates[0];
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = chargeCodeBAF.AC_Code, ChargeType = ChargeType.Included, FreightInclusiveCarriageCharge = chargeCodeCAF.AC_Code, Currency = "AUD" }
			};

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals("The TL_AC should match the chargeCodeBAF primary key.", chargeCodeBAF.PK, line.TL_AC);
			AssertEquals("The TL_RateCalculator should match FreightInclusiveCalculator.Code.", FreightInclusiveCalculator.Code, line.TL_RateCalculator);

			var rateLineItems = line.ChildRateLineItems;
			var preCarriageOnCarriageCharge = rateLineItems.Single(i => i.TM_Type == FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType);
			AssertEquals("The TM_AC should match the chargeCodeCAF primary key.", chargeCodeCAF.PK, preCarriageOnCarriageCharge.TM_AC);
		}

		public void TestConvert_GivenPercentageAppliesToIsSetToChargeCode_WhenFCL_ThenShouldCreatePercentageCalculator()
		{
			TestConvert_GivenPercentageAppliesToIsSetToChargeCode_ThenShouldCreatePercentageCalculator(Constants.RateMode.FCL);
		}

		public void TestConvert_GivenPercentageAppliesToIsSetToChargeCode_WhenLCL_ThenShouldCreatePercentageCalculator()
		{
			TestConvert_GivenPercentageAppliesToIsSetToChargeCode_ThenShouldCreatePercentageCalculator(Constants.RateMode.LCL);
		}

		void TestConvert_GivenPercentageAppliesToIsSetToChargeCode_ThenShouldCreatePercentageCalculator(string containerMode)
		{
			var mainChargeCode = Helper.ChargeCodes["CAF"];
			var surchargeChargeCode = Helper.ChargeCodes["BAF"];

			var response = ValidSEARatesSearchResponse;
			var firstRate = response.Rates[0];

			firstRate.ContainerMode = containerMode;

			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = mainChargeCode.AC_Code, FlatRate = 100, Currency = Constants.CurrencyCodes.Australia },
				new Charge { ChargeCode = surchargeChargeCode.AC_Code, PercentageAppliesTo = mainChargeCode.AC_Code, Percentage = 10, Currency = Constants.CurrencyCodes.Australia, MinRate = 10 , MaxRate = 20 }
			};

			var entry = ConvertSingleEntry(response);
			AssertEquals("There should be 2 rate lines", 2, entry.ChildRateLines.Count());

			var surchargeChargeCodeLine = entry.ChildRateLines.Single(l => l.TL_AC == surchargeChargeCode.PK);
			AssertEquals("Line calculator should be Percentage", PercentageCalculator.Code, surchargeChargeCodeLine.TL_RateCalculator);

			var rateLineItems = surchargeChargeCodeLine.ChildRateLineItems;
			var lineItemApplyTo = rateLineItems.Single(i => i.TM_Type == CalculatorConstants.Type.ApplyTo);
			AssertEquals("ApplyTo charge code should be CAF", mainChargeCode.AC_Code, lineItemApplyTo.CalculationOrderOrPercentOf);

			var minCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.MIN);
			AssertEquals(10M, minCharge.TM_Value);

			var maxCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.MAX);
			AssertEquals(20M, maxCharge.TM_Value);
		}

		public void TestConvert_GivenPercentageAppliesToIsSetToUnmappedChargeCode_ShouldCreatePercentageCalculatorWithError()
		{
			var surchargeChargeCode = Helper.ChargeCodes["BAF"];
			const string unmappedMainChargeCode = "XXX";

			var response = ValidSEARatesSearchResponse;
			var firstRate = response.Rates[0];
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = surchargeChargeCode.AC_Code, PercentageAppliesTo = unmappedMainChargeCode, Percentage = 10, Currency = Constants.CurrencyCodes.Australia }
			};

			var entry = ConvertSingleEntry(response);

			var rateLine = entry.ChildRateLines.Single();
			AssertEquals("Line calculator should be Percentage", PercentageCalculator.Code, rateLine.TL_RateCalculator);

			var rateLineItems = rateLine.ChildRateLineItems;
			var lineItemApplyTo = rateLineItems.Single(i => i.TM_Type == CalculatorConstants.Type.ApplyTo);
			AssertEquals(ZGuid.Empty, lineItemApplyTo.TM_AC);
			AssertEquals(ZString.Empty, lineItemApplyTo.CalculationOrderOrPercentOf);
			AssertEquals("No Charge Code is assigned with or has the same Code as Universal 'XXX'.", lineItemApplyTo.InvalidReason);
		}

		public void TestConvert_GivenPercentageAppliesToIsSetToChargeGroup_ShouldCreatePercentageCalculatorWithTextIsChargeGroup()
		{
			TestPercentageCalculatorApplyToChargeGroup("Freight group", "Freight", CalculatorConstants.Text.FreightCharges);
			TestPercentageCalculatorApplyToChargeGroup("Origin group", "Origin", CalculatorConstants.Text.OriginCharges);
			TestPercentageCalculatorApplyToChargeGroup("Destination group", "Destination", CalculatorConstants.Text.DestinationCharges);
			TestPercentageCalculatorApplyToChargeGroup("AllCharges", "XXXX", CalculatorConstants.Text.AllCharges);

			Assert(true);
		}

		void TestPercentageCalculatorApplyToChargeGroup(string assertionMessage, string chargeGroup, string expectedConvertedApplyTo)
		{
			var surchargeChargeCode = Helper.ChargeCodes["BAF"];

			var response = ValidSEARatesSearchResponse;
			var firstRate = response.Rates[0];
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = surchargeChargeCode.AC_Code, PercentageAppliesTo = chargeGroup, Percentage = 10, Currency = Constants.CurrencyCodes.Australia }
			};

			var entry = ConvertSingleEntry(response);

			var rateLine = entry.ChildRateLines.Single();
			AssertEquals("Line calculator should be Percentage", PercentageCalculator.Code, rateLine.TL_RateCalculator);

			var rateLineItems = rateLine.ChildRateLineItems;
			var lineItemApplyTo = rateLineItems.Single(i => i.TM_Type == CalculatorConstants.Type.ApplyTo);
			AssertEquals(ZString.Empty, lineItemApplyTo.CalculationOrderOrPercentOf);
			AssertEquals(assertionMessage, expectedConvertedApplyTo, lineItemApplyTo.TM_Text);
		}

		public void TestConvert_ShouldCreateCombinedCalculator_ForPerUnit_WhenProviderIsCargoSphereAndContainerModeIsLCL()
		{
			var response = ValidSEARatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var firstRate = response.Rates[0];
			firstRate.ContainerMode = Constants.RateMode.LCL;
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", PerUnitRate = 6M , UnitMultiplier = 1000M , Unit = "M3", ActualPercentage = 100 }
			};

			AssertEquals("Provider should be CargoSphere.", WRConstants.RateProviders.CargoSphere, firstRate.Provider);
			AssertEquals("Container mode should be LCL.", Constants.RateMode.LCL, firstRate.ContainerMode);

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals("TL_AC should match the charge code PK.", chargeCodeFRT.PK, line.TL_AC);
			AssertEquals("TL_RateCalculator should be CombinedCalculator.Code.", CombinedCalculator.Code, line.TL_RateCalculator);
			AssertEquals("TL_WeightVolumeMultiple should be 1000.", 1000M, line.TL_WeightVolumeMultiple);
			AssertEquals("TL_ActualPercentage should be 100.", (byte)100, line.TL_ActualPercentage);

			var rateLineItems = line.ChildRateLineItems.ToList();

			var unitCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.UNT);
			AssertEquals("TM_Value should be 6.", 6M, unitCharge.TM_Value);
		}

		public void TestConvert_ShouldCreateFlatCalculator_ForFlat_WhenProviderIsCargoSphereAndContainerModeIsLCL()
		{
			var response = ValidSEARatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var firstRate = response.Rates[0];
			firstRate.ContainerMode = Constants.RateMode.LCL;
			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 12M }
			};

			AssertEquals("The provider should match the expected CargoSphere provider.", WRConstants.RateProviders.CargoSphere, firstRate.Provider);
			AssertEquals("The container mode should be LCL.", Constants.RateMode.LCL, firstRate.ContainerMode);

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals("The TL_AC should match the charge code PK.", chargeCodeFRT.PK, line.TL_AC);
			AssertEquals("The TL_RateCalculator should be FlatCalculator.Code.", FlatCalculator.Code, line.TL_RateCalculator);

			var rateLineItems = line.ChildRateLineItems.ToList();

			var flatCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.BAS);
			AssertEquals("The TM_Value of the flat charge should be 12M.", 12M, flatCharge.TM_Value);
		}

		public void TestConvert_ShouldCreateCombinedCalculator_ForFlatRateWithMinAndMaxWithBreaksHaveFlatRate()
		{
			var response = ValidSEARatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var firstRate = response.Rates[0];
			firstRate.ContainerMode = Constants.RateMode.LCL;

			firstRate.Charges = new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 150 , MinRate = 150, MaxRate = 10000 },
				new Charge { ChargeCode = "FRT", Currency = "AUD", Break = 1, BreakOperator = ">", PerUnitRate = 7.4M, FlatRate = 48M } ,
				new Charge { ChargeCode = "FRT", Currency = "AUD", Break = 50, BreakOperator = ">", PerUnitRate = 6M, FlatRate = 31.5M }
			};

			AssertEquals("First rate provider should match", WRConstants.RateProviders.CargoSphere, firstRate.Provider);
			AssertEquals("First rate container mode should match", Constants.RateMode.LCL, firstRate.ContainerMode);

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.First();

			AssertEquals("Charge code should match", chargeCodeFRT.PK, line.TL_AC);
			AssertEquals("Rate calculator should be combined calculator", CombinedCalculator.Code, line.TL_RateCalculator);

			var rateLineItems = line.ChildRateLineItems.ToList();

			var flatCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.BAS);
			AssertEquals("Flat charge value should match", 150M, flatCharge.TM_Value);

			var breaks = rateLineItems.Where(i => i.TM_Type == Calculator.Items.Operator.Plus).Select(x => x.TM_Value);
			AssertContainsExactElementsInAnyOrder(
				"Break rates should match",
				new[] { (ZDecimal)6M, (ZDecimal)7.4M },
				breaks
			);

			var flatForBreaks = rateLineItems.Where(i => i.TM_Type == Calculator.Items.Operator.Plus).Select(x => x.TM_FlatAmount);
			AssertContainsExactElementsInAnyOrder(
				"Flat amounts for breaks should match",
				new[] { (ZDecimal)48M, (ZDecimal)31.5M },
				flatForBreaks
			);

			var minCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.MIN);
			AssertEquals("Minimum charge should match", (ZDecimal)150M, minCharge.TM_Value);

			var maxCharge = rateLineItems.Single(i => i.TM_Type == Calculator.Items.Operator.MAX);
			AssertEquals("Maximum charge should match", (ZDecimal)10000M, maxCharge.TM_Value);
		}

		#region Highest Rate Calculator

		//object ToAssertionItem(IRateLineItem item) =>
		//	NewAssertionItem(tm_type: item.TM_Type, tm_relevantValue: item.TM_RelevantValue,
		//		tm_breakWeightVolume: item.TM_BreakWeightVolume, tm_unitMultiple: item.TM_UnitMultiple);

		//object NewAssertionItem(string tm_type, decimal tm_relevantValue = 0, string tm_breakWeightVolume = "", int tm_unitMultiple = 0, ZGuid chargeComparisonGroupID = default) =>
		//	new
		//	{
		//		TM_Type = tm_type,
		//		TM_RelevantValue = tm_relevantValue,
		//		TM_BreakWeightVolume = tm_breakWeightVolume,
		//		TM_UnitMultiple = tm_unitMultiple,
		//		ChargeComparisonGroupID = chargeComparisonGroupID,
		//	};

		public void TestConvert_TwoChargesSameRateLineID_ShouldCreateHRCCalculatorWithWeightAndVolumePerUnitItems()
		{
			var response = ValidSEARatesSearchResponse;
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var rate = response.Rates[0];
			rate.ContainerMode = Constants.RateMode.LCL;

			rate.Charges = new[]
			{
				new Charge
				{
					RateLineID = 1,
					ChargeCode = "FRT", Currency = "AUD", PerUnitRate = 10, Unit = "KG", UnitMultiplier = 1000,
				},
				new Charge
				{
					RateLineID = 1,
					ChargeCode = "FRT", Currency = "AUD", PerUnitRate = 15, Unit = "M3", UnitMultiplier = 4,
				},
			};

			AssertEquals("precondition", WRConstants.RateProviders.CargoSphere, rate.Provider);
			AssertEquals("precondition", Constants.RateMode.LCL, rate.ContainerMode);

			var entry = ConvertSingleEntry(response);
			var line = (WiseLine)entry.ChildRateLines.Single();

			AssertEquals(chargeCodeFRT.PK, line.TL_AC);
			AssertEquals(HighestRateCalculator.Code, line.TL_RateCalculator);
			AssertEquals(ZDecimal.Zero, line.TL_WeightVolumeMultiple);

			var rateLineItems = line.ChildRateLineItems
				.Select(i => $"{(i).TM_Type}|{(i).TM_RelevantValue}|{(i).TM_BreakWeightVolume}|{(i).TM_UnitMultiple}")
				.ToList();

			var expectedRateLineItems = new[]
			{
				"UNT|10|KG|1000",
				"UNT|15|M3|4",
				"RPR|0||0"
			};

			AssertContainsExactElementsInAnyOrder(expectedRateLineItems, rateLineItems);
		}

		public void TestConvert_TwoChargesDifferentRateLineIDs_ShouldNotResolveAsHighestRateCalculator()
		{
			var response = ValidSEARatesSearchResponse;
			var rate = response.Rates[0];
			rate.ContainerMode = Constants.RateMode.LCL;
			rate.Charges = new[]
			{
				new Charge
				{
					RateLineID = 1,
					ChargeCode = "FRT", Currency = "AUD", PerUnitRate = 10, Unit = "KG", UnitMultiplier = 1000,
					ConversionFactor = 1000, ConversionFactorUnit = "KG", ConversionFactorDenominatorUnit = "M3"
				},
				new Charge
				{
					RateLineID = 2,
					ChargeCode = "FRT", Currency = "AUD", PerUnitRate = 15, Unit = "CF", UnitMultiplier = 4,
					ConversionFactor = 100, ConversionFactorUnit = "LB", ConversionFactorDenominatorUnit = "M3"
				},
			};

			AssertEquals("precondition", WRConstants.RateProviders.CargoSphere, rate.Provider);
			AssertEquals("precondition", Constants.RateMode.LCL, rate.ContainerMode);

			var entry = ConvertSingleEntry(response);
			var lines = entry.ChildRateLines.OfType<WiseLine>().ToList();

			AssertEquals(2, lines.Count);
			foreach (var line in lines)
			{
				AssertNotEquals(HighestRateCalculator.Code, line.TL_RateCalculator);
			}
		}

		#endregion

		#endregion

		#region CG HBLR

		public void TestConvert_CargoguideRate_CombinedCalculatorWithHBLR()
		{
			var response = ValidAIRRatesSearchResponse;
			var charges = response.Rates.SelectMany(r => r.Charges);
			foreach (var charge in charges)
			{
				charge.IsHigherBreakLowerRate = true;
			}

			var entry = ConvertSingleEntry(response, ValidCriteria);

			Assert(
				"All child rate lines should have calculators of type BaseCombinedCalculator.",
				entry
					.ChildRateLines
					.All(l => l.Calculator is BaseCombinedCalculator)
			);

			Assert(
				"All calculators should use the higher chargeable lower rate rule.",
				entry
					.ChildRateLines
					.Select(l => l.Calculator as BaseCombinedCalculator)
					.All(c => c.UseHigherChargeableLowerRateRule)
			);
		}

		#endregion

		#region Included Lines

		public void TestConvert_PopulateIncludedLines()
		{
			var chargeCodes = new[]
			{
				CreateChargeCode("L_FRT", "FRT", Env.CurrentCompanyPK),
				CreateChargeCode("L_FRT2", "FRT2", Env.CurrentCompanyPK),
				CreateChargeCode("L_BAF", "BAF", Env.CurrentCompanyPK),
				CreateChargeCode("L_BAF2", "BAF2", Env.CurrentCompanyPK),
				CreateChargeCode("L_WAR", "WAR", Env.CurrentCompanyPK),
				CreateChargeCode("L_FUL", "FUL", Env.CurrentCompanyPK),
				CreateChargeCode("L_CAF", "CAF", Env.CurrentCompanyPK),
			};

			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].Charges = new List<Charge>(new[]
			{
				new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 100m },
				new Charge { ChargeCode = "FRT2", Currency = "AUD", FlatRate = 200m },
				new Charge
				{
					ChargeCode = "BAF",
					Currency = "AUD",
					ChargeType = ChargeType.Included,
					FreightInclusiveCarriageCharge = "FRT"
				},
				new Charge
				{
					ChargeCode = "BAF2",
					Currency = "AUD",
					ChargeType = ChargeType.Included,
					FreightInclusiveCarriageCharge = "FRT2"
				},
				new Charge
				{
					ChargeCode = "CAF",
					Currency = "AUD",
					ChargeType = ChargeType.SubjectTo,
					FreightInclusiveCarriageCharge = "FRT",
				},
				new Charge { ChargeCode = "WAR", Currency = "AUD", FlatRate = 200 },
				new Charge { ChargeCode = "FUL", Currency = "AUD", FlatRate = 200, ChargeType = ChargeType.Optional },
			});

			response.ChargeCodes = new[]
			{
				new RefChargeCode { Code = "FRT", Description = "FRT Description" },
				new RefChargeCode { Code = "FRT2", Description = "FRT2 Description" },
				new RefChargeCode { Code = "BAF", Description = "BAF Description" },
				new RefChargeCode { Code = "XXX", Description = "XXX Description" },
				new RefChargeCode { Code = "WAR", Description = "WAR Description" },
				new RefChargeCode { Code = "FUL", Description = "FUL Description" },
			};

			var rate = ConvertSingleEntry(response);
			var frtCharge = rate.ChildRateLines.First(r => r.ChargeCode.AC_Code == "L_FRT");
			var actualFrtIncludedLines = frtCharge.IncludedLines.Select(c => (string)c.ChargeCode.AC_Code).ToArray();
			var expectedFrtIncludedLines = new[] { "L_BAF", "L_CAF" };
			AssertContainsExactElementsInAnyOrder(
				"Expected included lines for L_FRT",
				expectedFrtIncludedLines,
				actualFrtIncludedLines
			);

			var frt2Charge = rate.ChildRateLines.First(r => r.ChargeCode.AC_Code == "L_FRT2");
			var actualFrt2IncludedLines = frt2Charge.IncludedLines.Select(c => (string)c.ChargeCode.AC_Code).ToArray();
			var expectedFrt2IncludedLines = new[] { "L_BAF2" };
			AssertContainsExactElementsInAnyOrder(
				"Expected included lines for L_FRT2",
				expectedFrt2IncludedLines,
				actualFrt2IncludedLines
			);
		}

		#endregion

		#region Charge Code Custom Rounding

		public void TestConvert_SetsCustomRoundingInLineFromCharge()
		{
			var response = ValidAIRRatesSearchResponse;
			var precision = 0.5;
			response.Rates[0].Charges[0].ProviderCustomFields =
				new[] { new CustomField { Code = Rate.CustomFields.Common.Precision, Value = precision, Description = "Precision" } };

			var rate = ConvertSingleEntry(response);
			var rateLine = rate.ChildRateLines.First();

			AssertEquals("TL_Rounding should be set to Custom.", new ZString(RatingRoundingTypes.Custom), rateLine.TL_Rounding);
			AssertEquals("TL_RoundingFactor should match the precision.", new ZDecimal(precision), rateLine.TL_RoundingFactor);
		}

		public void TestConvert_SetsZeroCustomRoundingInLineFromCharge()
		{
			var response = ValidAIRRatesSearchResponse;
			var precision = 0.0;
			response.Rates[0].Charges[0].ProviderCustomFields =
				new[] { new CustomField { Code = Rate.CustomFields.Common.Precision, Value = precision, Description = "Precision" } };

			var rate = ConvertSingleEntry(response);
			var rateLine = rate.ChildRateLines.First();
			AssertEquals("Rounding type should be set to custom.", RatingRoundingTypes.Custom, rateLine.TL_Rounding);
			AssertEquals("Rounding factor should match the precision value.", new ZDecimal(precision), rateLine.TL_RoundingFactor);
		}

		public void TestConvert_SetsNoCustomRoundingInLineFromCharge()
		{
			var response = ValidAIRRatesSearchResponse;
			Assert(response.Rates[0].Charges[0].ProviderCustomFields.IsNullOrEmpty());

			var rate = ConvertSingleEntry(response);
			var rateLine = rate.ChildRateLines.First();
			AssertEquals("Expected TL_Rounding to be empty.", ZString.Empty, rateLine.TL_Rounding);
			AssertEquals("Expected TL_RoundingFactor to be zero.", ZDecimal.Zero, rateLine.TL_RoundingFactor);
		}
		#endregion

		public void TestConvert_IncludeCGReference()
		{
			var response = ValidAIRRatesSearchResponse;
			response.Rates[0].ProviderCustomFields =
				new[] { new CustomField { Code = Rate.CustomFields.Cargoguide.Reference, Value = "REF1", Description = "Reference" } };

			var entry = ConvertSingleEntry(response, ValidCriteria);
			var actualValues = entry.CustomFields.Select(c => c.Value);

			AssertCollectionContains("Expected 'REF1' to be present in custom field values.", "REF1", actualValues);
		}

		public void TestConvert_EmptyCurrency_TraceIDPresentOnDevException()
		{
			var response = ValidSEARatesSearchResponse;
			var charge = response.Rates[0].Charges[0];
			charge.Currency = string.Empty;

			var entry = ConvertSingleEntry(response);

			AssertEquals(0, Logger.GetErrorsAndWarnings().Count());
			AssertEquals(
				"[TraceID: traceid] Charge from the Rates Service has no Currency",
				ErrorReporter.LastMessageReported
			);
			ErrorReporter.Clear();
		}

		public void TestConvert_ValidSEARate_ShouldBeConvertedWithoutErrors()
		{
			var entry = ConvertSingleEntry(ValidSEARatesSearchResponse, ValidCriteria);

			AssertEquals("Entry should be a valid rate.", true, entry.IsValidRate());
			AssertNullOrEmpty("Entry should not have an invalid reason.", entry.InvalidReason);
		}

		public void TestConvert_ValidAIRRate_ShouldBeConvertedWithoutErrors()
		{
			var entry = ConvertSingleEntry(ValidAIRRatesSearchResponse, ValidCriteria);

			AssertEquals("The rate should be valid after conversion.", true, entry.IsValidRate());
			AssertNullOrEmpty("The invalid reason should be null or empty for a valid rate.", entry.InvalidReason);
		}

		public void TestConvertChargeCode_NoWiseChargeCode_ShouldReturnError()
		{
			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode(string.Empty, Factory);

			Assert(error == "The charge code has NO mapping with any Universal Charge Code.");
		}

		public void TestConvertChargeCode_LocalCarrierChargeCodeWithoutOrgLink_ShouldBeConvertedWithoutErrors()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var (accChargeCode, universalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "CAR", true, "AIR", null);
			Factory.Save();

			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode("TEST", Factory, "TEST", orgHeader, "AIR");

			Assert("The charge code should select the Local Carrier Charge Code with no errors", accChargeCode == chargeCode);
		}

		public void TestConvertChargeCode_LocalCarrierChargeCodeWithOrgLink_ShouldBeConvertedWithoutErrors()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var (accChargeCode, universalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "CAR", true, "SEA", orgHeader);
			Factory.Save();

			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode("TEST", Factory, "TEST", orgHeader, "SEA");

			Assert("The charge code should select the Local Carrier Charge Code with no errors", accChargeCode == chargeCode);
		}

		public void TestConvertChargeCode_GlobalCarrierChargeCode_ShouldBeConvertedWithoutErrors()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var (accChargeCode, universalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "CAR", false, "SEA", orgHeader);
			Factory.Save();

			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode("TEST", Factory, "TEST", orgHeader, "SEA");

			Assert("The charge code should select the Global Carrier Charge Code with no errors", accChargeCode == chargeCode);
		}

		public void TestConvertChargeCode_LocalUniversalChargeCode_ShouldBeConvertedWithoutErrors()
		{
			var (accChargeCode, universalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "UCC", true);
			Factory.Save();

			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode("TEST", Factory, "TEST");

			Assert("The charge code should select the Local Universal Charge Code with no errors", accChargeCode == chargeCode);
		}

		public void TestConvertChargeCode_GlobalUniversalChargeCode_ShouldBeConvertedWithoutErrors()
		{
			var (accChargeCode, universalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "UCC", false);
			Factory.Save();

			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode("TEST", Factory, "TEST");

			Assert("The charge code should select the Global Universal Charge Code with no errors", accChargeCode == chargeCode);
		}

		public void TestConvertChargeCode_LocalCarrierAndUniversalChargeCode_ShouldBeConvertedWithoutErrors()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var (localCarrierChargeCode, localCarrierUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "CAR", true, "SEA", orgHeader);
			var (localUniversalChargeCode, localUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "UCC", true);
			Factory.Save();

			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode("TEST", Factory, "TEST", orgHeader, "SEA");

			Assert("The charge code should select the Local Carrier Charge Code first.", localCarrierChargeCode == chargeCode);
		}

		public void TestConvertChargeCode_GlobalCarrierAndUnviersalChargeCode_ShouldBeConvertedWithoutErrors()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var (globalCarrierChargeCode, globalCarrierUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "CAR", false, "AIR", orgHeader);
			var (globalUniversalChargeCode, globalUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "UCC", false);
			Factory.Save();

			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode("TEST", Factory, "TEST", orgHeader, "AIR");

			Assert("The charge code should select the Global Carrier Charge Code first.", globalCarrierChargeCode == chargeCode);
		}

		public void TestConvertChargeCode_LocalUniversalAndGlobalCarrierChargeCode_ShouldBeConvertedWithoutErrors()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var (globalCarrierChargeCode, globalCarrierUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "CAR", false, "SEA", orgHeader);
			var (localUniversalChargeCode, localUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "UCC", true);
			Factory.Save();

			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode("TEST", Factory, "TEST", orgHeader, "SEA");

			Assert("The charge code should select the Local Universal Charge Code first.", localUniversalChargeCode == chargeCode);
		}

		public void TestConvertChargeCode_AllChargeCodesFilled_ShouldBeConvertedWithoutErrors()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var (localCarrierChargeCode, localCarrierUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "CAR", true, "SEA", orgHeader);
			var (localUniversalChargeCode, localUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "UCC", true);
			var (globalCarrierChargeCode, globalCarrierUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "CAR", false, "SEA", orgHeader);
			var (globalUniversalChargeCode, globalUniversalChargeCodeMapping) = CreateChargeCodeUniversalChargeCodeMapping("TEST", "UCC", false);
			Factory.Save();

			var (chargeCode, error) = WiseRatesConverter.ConvertChargeCode("TEST", Factory, "TEST", orgHeader, "SEA");

			Assert("The charge code should select the Local Carrier Charge Code first.", localCarrierChargeCode == chargeCode);
		}

		(AccChargeCode, AccChargeCodeUniversalCodeMapping) CreateChargeCodeUniversalChargeCodeMapping(string code, string type, bool local, string transportMode = "", OrgHeader carrier = null)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = local ? Env.CurrentCompanyPK : ZGuid.Empty;
			chargeCode.AC_ChargeType = "MRG";
			chargeCode.AC_ChargeGroup = "FRT";
			chargeCode.AC_Desc = "Test Charge Code";

			var universalChargeCodeMapping = Factory.NewWithValidTestData<AccChargeCodeUniversalCodeMapping>();
			universalChargeCodeMapping.AUP_AC = chargeCode.PK;
			universalChargeCodeMapping.AUP_Code = code;
			universalChargeCodeMapping.AUP_Type = type;
			universalChargeCodeMapping.AUP_TransportMode = transportMode;
			if (carrier != null)
			{
				universalChargeCodeMapping.AUP_OH_Carrier = carrier.PK;
			}

			if (!local)
			{
				Factory.Save();
				var query = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode.AC_Code);
				query.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);

				var localVersionChargeCode = Factory.Load<AccChargeCode>(query).Single();

				return (localVersionChargeCode, universalChargeCodeMapping);
			}

			return (chargeCode, universalChargeCodeMapping);
		}

		WiseEntry ConvertSingleEntry(RatesSearchResponse response)
		{
			return ConvertSingleEntry(response, ValidCriteria);
		}

		WiseEntry ConvertSingleEntry(RatesSearchResponse response, RatingCriteria criteria)
		{
			return ConvertAllEntries(response, criteria).Single();
		}

		IList<WiseEntry> ConvertAllEntries(RatesSearchResponse response, RatingCriteria criteria)
		{
			var converter = new WiseRatesConverter(Factory, Logger);

			ConversionOptions conversionOptions;
			conversionOptions.AddRateModeAndCategoryValidation = true;

			var entries = converter.Convert(response, criteria, conversionOptions, searchTraceID: "traceid");

			return entries;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var billTo = Factory.NewWithValidTestData<OrgHeader>();

			var oceanCarrier = Factory.NewWithValidTestData<OrgHeader>();
			oceanCarrier.OH_FullName = "Qantas Ocean";
			oceanCarrier.OH_IsShippingProvider = true;
			AddShippingLineToCarrier(oceanCarrier, "EMIR");

			NewCarrier(carrierCode: "AIRC", fullName: "Air Carrier", iataCode: "AC", airlineAccountingCode: "7xx");

			ValidCriteria = new TestRatingCriteria("AUSYD", "UAIEV", 2, container, billTo);
			ValidCriteria.AdapterType = AdapterType.Consolidation;
			ValidCriteria.JobDatesProvider = new TestJobDatesProvider(Factory.NewWithValidTestData<DummyBusinessObject>(), new ZDateTime(2016, 08, 15), new ZDateTime(2016, 08, 04));
			ValidCriteria.OperationalJobCode = "S000234202";

			CodeMappingsOrg = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			Logger = new TestLogger();

			ValidAIRRatesSearchResponse = new RatesSearchResponse
			{
				Rates = new[]
				{
					new Rate
					{
						Carrier = "RAC",
						Origin = "UAIEV",
						Destination = "AUSYD",
						TransportMode = "AIR",
						ContainerMode = "LCL",
						Provider = WRConstants.RateProviders.CargoGuide,
						StartDate = new DateTime(2020, 01, 01),
						ExpiryDate = new DateTime(2025, 01, 01),
						Charges = new[]
						{
							new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 100m },
						}
					}
				},
				Carriers = new[]
				{
					new RefCarrier
					{
						Code = "RAC", Name = "Ref Airlines", IATACode = "AC"
					}
				},
				ChargeCodes = new[]
				{
					new RefChargeCode
					{
						Code = "FRT", Group = "FRT"
					}
				}
			};

			ValidSEARatesSearchResponse = new RatesSearchResponse
			{
				Rates = new[]
				{
					new Rate
					{
						Carrier = "EMR",
						Origin = "UAIEV",
						Destination = "AUSYD",
						TransportMode = "SEA",
						ContainerMode = "FCL",
						Provider = WRConstants.RateProviders.CargoSphere,
						StartDate = new DateTime(2020, 01, 01),
						ExpiryDate = new DateTime(2025, 01, 01),
						Charges = new[]
						{
							new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 100m },
						},
					}
				},
				Carriers = new[]
				{
					new RefCarrier
					{
						Code = "EMR", Name = "Emirates", SCACCode = "EMIR"
					}
				},
				ChargeCodes = new[]
				{
					new RefChargeCode
					{
						Code = "FRT", Group = "FRT"
					}
				}
			};
		}

		void AddShippingLineToCarrier(OrgHeader carrier, string scac)
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = scac;
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
		}

		protected RatesSearchResponse ValidAIRRatesSearchResponse { get; private set; }
		protected RatesSearchResponse ValidSEARatesSearchResponse { get; private set; }
		protected RatingCriteria ValidCriteria { get; private set; }
		protected OrgHeader CodeMappingsOrg { get; private set; }
		protected TestLogger Logger { get; private set; }
	}
}
