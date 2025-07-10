using System;
using System.Drawing;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector.Models;
using Moq;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class BookingEngineRateViewModelTest : RatingTestCase
	{
		public void TestPopulateAirlineIcon_WhenAirlineLogoExistsInDB()
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();

			const string iataCode = "LH";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = iataCode;
			airline.RM_TwoCharacterCode = iataCode;

			var converter = new ImageConverter();
			using (var bitmap = new Bitmap(1, 1))
			{
				airline.RM_AirlineLogo = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
			}

			Factory.Save();

			var rate = ModelsTestingHelper.GetMockedBookingRate(
				2000,
				Constants.CurrencyCodes.EuropeanUnion,
				"STANDARD",
				"BOOKABLE",
				iataCode
			);
			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);

			var cache = MemoryCache.Default;
			var key = $"RateSelector.Airline.{iataCode}.Logo";
			AssertEquals($"Cache should contain the key {key}", true, cache.Contains(key));
			var item = (Lazy<Bitmap>)cache[key];
			AssertSame($"Cache item should match the Lazy<Bitmap> instance for key {key}", item, cache.Remove(key));
			var cachedAirlineIconBitmap = item.Value;
			AssertNotNull("cachedAirlineIconBitmap", cachedAirlineIconBitmap);

			AssertSame(
				"Airline icon bitmap in the view model should match the cached airline icon bitmap",
				cachedAirlineIconBitmap,
				bookingRateViewModel.AirlineIconBitmap
			);
			AssertEquals(
				"Carrier code in the booking rate view model should match the IATA code",
				iataCode,
				bookingRateViewModel.CarrierCode
			);
		}

		public void TestPopulateAirlineIcon_WhenAirlineLogoDoesNotExist()
		{
			const string iataCode = "LH";
			var rate = ModelsTestingHelper.GetMockedBookingRate(
				2000,
				Constants.CurrencyCodes.EuropeanUnion,
				"STANDARD",
				"BOOKABLE",
				iataCode
			);

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("Expected carrier code to be '??' when airline logo does not exist", "??", bookingRateViewModel.CarrierCode);

			var cache = MemoryCache.Default;
			var key = $"RateSelector.Airline.{iataCode}.Logo";
			AssertEquals($"Expected cache not to contain key '{key}'", false, cache.Contains(key));
		}

		public void TestPopulateFromBookingRate()
		{
			var transportLeg = new Mock<IBookingTransportLeg>(MockBehavior.Loose);

			var rate = ModelsTestingHelper.GetMockedBookingRate(
				2000,
				Constants.CurrencyCodes.EuropeanUnion,
				"STANDARD",
				"BOOKABLE",
				"074",
				"remarks",
				new[] { transportLeg.Object }
			);

			var rateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);

			AssertEquals("The main charge amount should be correct.", 2000m, rateViewModel.MainCharge.Amount);
			AssertEquals("The currency should match the rate.", Constants.CurrencyCodes.EuropeanUnion, rateViewModel.MainCharge.Currency);
			AssertEquals("The charge code should be correct.", "STANDARD", rateViewModel.MainCharge.ChargeCode);
			AssertEquals("The charge code description should match.", "BOOKABLE", rateViewModel.MainCharge.ChargeCodeDescription);

			AssertEquals("The carrier code should match.", "KL", rateViewModel.CarrierCode);
			AssertEquals("The carrier name should match.", "KLM Royal Dutch Airlines", rateViewModel.CarrierName);
			AssertEquals("The remarks should be properly set.", "remarks", rateViewModel.Remarks);

			AssertEquals("The transport legs count should be 1.", 1, rateViewModel.TransportLegs.Count);
			AssertSame("The rate should match the mocked rate object.", rate.Object, rateViewModel.Rate);
		}

		public void TestChargeAddedToFreightCharges()
		{
			var rate = ModelsTestingHelper.GetMockedBookingRate(GetCharge(), "000");

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("Freight charges should contain exactly one charge", 1, bookingRateViewModel.FreightCharges.Charges.Count());
			AssertEquals("Total price should be correctly calculated", 10m, bookingRateViewModel.TotalPrice);
			AssertEquals("Total price string should have correct format", "10.00 USD", bookingRateViewModel.TotalPriceString);
		}

		public void TestOriginalCurrencyIsUsedWhenThereIsNoExchangeRate()
		{
			var rate = ModelsTestingHelper.GetMockedBookingRate(GetCharge("LKR"), "000");

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);

			AssertEquals("Expected one charge in FreightCharges", 1, bookingRateViewModel.FreightCharges.Charges.Count());
			AssertEquals("Total price should be 0 for the given scenario", 0m, bookingRateViewModel.TotalPrice);
			AssertEquals("Expected total price string to match the formatted value", "10.00 LKR", bookingRateViewModel.TotalPriceString);
			AssertEquals("Expected error message for missing exchange rate", "No valid exchange rate between LKR and USD is found for Charge Test", bookingRateViewModel.TotalPriceError);
		}

		public void TestDisplayOnlyPriceIfInvalidCurrencyProvided()
		{
			var rate = ModelsTestingHelper.GetMockedBookingRate(GetCharge("XXX"), "000");

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);

			AssertEquals(1, bookingRateViewModel.FreightCharges.Charges.Count());
			AssertEquals(0m, bookingRateViewModel.TotalPrice);
			AssertEquals("10.00", bookingRateViewModel.TotalPriceString);
			AssertEquals(
				"XXX currency for Charge Test is invalid",
				bookingRateViewModel.TotalPriceError
			);
		}

		public void TestPopulateCarrierFromRefAirline()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_AirlinePrefix = "XYZ";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XYZ";
			airline.RM_AirlineName1 = "WiseTech Air Service";
			airline.RM_TwoCharacterCode = "WT";
			airline.RM_IsActive = true;
			Factory.Save();

			var rate = ModelsTestingHelper.GetMockedBookingRate(
				2000,
				Constants.CurrencyCodes.EuropeanUnion,
				"STANDARD",
				"BOOKABLE",
				"XYZ"
			);

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("Carrier code mismatch", "WT", bookingRateViewModel.CarrierCode);
			AssertEquals("Carrier name mismatch", "WiseTech Air Service", bookingRateViewModel.CarrierName);
		}

		public void TestGetCarrierCodeFromTransportLegs()
		{
			var rate = ModelsTestingHelper.GetMockedBookingRate(
				charge: GetCharge(),
				carrierPrefix: "XXX",
				transportLegs: GetLegs()
			);

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("Expected carrier code does not match.", "QF", bookingRateViewModel.CarrierCode);
			AssertEquals("Expected carrier name does not match.", "The rate has no carrier", bookingRateViewModel.CarrierName);
		}

		public void TestSetCarrierToUnknownWhenCarrierDoesNotExist()
		{
			var rate = ModelsTestingHelper.GetMockedBookingRate(GetCharge(), "XXX");

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("Expected carrier code to be '??' for unknown carriers.", "??", bookingRateViewModel.CarrierCode);
			AssertEquals("Expected carrier name to be 'The rate has no carrier' for unknown carriers.", "The rate has no carrier", bookingRateViewModel.CarrierName);
		}

		public void TestSetCarrierCodeEvenIfVoyageNumberIsNotValid()
		{
			var leg = new DummyBookingTransportLeg()
			{
				LegOrder = 1,
				VoyageNumber = "X",
			};

			var rate = ModelsTestingHelper.GetMockedBookingRate(
				charge: GetCharge(),
				carrierPrefix: "XXX",
				transportLegs: new[] { leg }
			);

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("Expected CarrierCode to match the provided VoyageNumber", "X", bookingRateViewModel.CarrierCode);
			AssertEquals("Expected CarrierName to indicate no carrier is present in the rate", "The rate has no carrier", bookingRateViewModel.CarrierName);
		}

		public void TestPopulateVoyageInformationFromLegs()
		{
			var rate = ModelsTestingHelper.GetMockedBookingRate(charge: GetCharge(), transportLegs: GetLegs());

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("Booking rate carrier code should match the expected value.", "??", bookingRateViewModel.CarrierCode);
			AssertEquals("Booking rate origin should match the expected value.", "SYD", bookingRateViewModel.Origin);
			AssertEquals("Booking rate destination should match the expected value.", "JFK", bookingRateViewModel.Destination);
			AssertEquals("Departure time should match the expected value.", new DateTime(2020, 2, 2), bookingRateViewModel.DepartureTime);
			AssertEquals("Arrival time should match the expected value.", new DateTime(2020, 2, 2, 18, 15, 0), bookingRateViewModel.ArrivalTime);
		}

		public void TestPopulateAdditionalDetailsInformationFromAdditionalDetails()
		{
			var additionalDetails = new[]
			{
				new DummyCodeDescription("CO2 Emission", "1200"),
				new DummyCodeDescription("CO2 Emission", "5400"),
				new DummyCodeDescription("Additional Fee", string.Empty)
			};

			var rate = ModelsTestingHelper.GetMockedBookingRate(charge: GetCharge(), transportLegs: GetLegs(), additionalDetails: additionalDetails);

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);

			var actual = bookingRateViewModel.AdditionalDetails
				.Select(additionalDetail => $"{additionalDetail.Code}|{additionalDetail.Description}")
				.ToArray();

			var expected = new[]
			{
				"CO2 Emission|1200",
				"CO2 Emission|5400",
				"Additional Fee|"
			};

			AssertContainsExactElementsInAnyOrder(
				"The additional details should match the expected collection",
				expected,
				actual
			);
		}

		public void TestShowAdditionalDetailsShouldBeFalseWhenAdditionalDetailsIsEmpty()
		{
			var additionalDetails = new[]
			{
				new DummyCodeDescription("CO2 Emission", "1200")
			};

			var rate = ModelsTestingHelper.GetMockedBookingRate(charge: GetCharge(), transportLegs: GetLegs(), additionalDetails: additionalDetails);

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("ShowAdditionalDetails should be true when additionalDetails is provided.", true, bookingRateViewModel.ShowAdditionalDetails);

			rate = ModelsTestingHelper.GetMockedBookingRate(charge: GetCharge(), transportLegs: GetLegs(), additionalDetails: null);

			bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("ShowAdditionalDetails should be false when additionalDetails is null.", false, bookingRateViewModel.ShowAdditionalDetails);
		}

		public void TestPopulateCostBreakdown()
		{
			var costBreakdownCharges = new[]
			{
				new DummyCostBreakdownCharge
				{
					ChargeCode = "FRC",
					ChargeCodeDescription = "Freight Charge",
					Amount = 100m,
					Currency = "USD"
				},
				new DummyCostBreakdownCharge
				{
					ChargeCode = "VAL",
					ChargeCodeDescription = "Valuation",
					Amount = 0.0m,
					Currency = "USD"
				},
				new DummyCostBreakdownCharge
				{
					ChargeCode = "XX",
					ChargeCodeDescription = "Other (XX)",
					Amount = 0.40m,
					Currency = "USD"
				}
			};

			var rate = ModelsTestingHelper.GetMockedBookingRate(charge: GetCharge(), transportLegs: GetLegs(), costBreakdownCharges: costBreakdownCharges);

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);

			var actual = bookingRateViewModel.CostBreakdownCharges
				.Select(c => $"{c.ChargeCode}|{c.ChargeCodeDescription}|{c.Amount}|{c.Currency}|{c.CostString}")
				.ToArray();

			var expected = new[]
			{
				"FRC|Freight Charge|100|USD|100.00 USD",
				"VAL|Valuation|0.0|USD|0.00 USD",
				"XX|Other (XX)|0.40|USD|0.40 USD"
			};

			AssertContainsExactElementsInAnyOrder(
				"Cost breakdown charges should match the expected format and values",
				expected,
				actual
			);
		}

		public void TestShowCostBreakdownChargesShouldBeFalseWhenCostBreakdownChargesIsEmpty()
		{
			var costBreakdownCharges = new[]
			{
				new DummyCostBreakdownCharge
				{
					ChargeCode = "FRC",
					ChargeCodeDescription = "Freight Charge",
					Amount = 100m,
					Currency = "USD"
				}
			};

			var rate = ModelsTestingHelper.GetMockedBookingRate(charge: GetCharge(), transportLegs: GetLegs(), costBreakdownCharges: costBreakdownCharges);

			var bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("ShowCostBreakdownCharges should be true when costBreakdownCharges is not null", true, bookingRateViewModel.ShowCostBreakdownCharges);

			rate = ModelsTestingHelper.GetMockedBookingRate(charge: GetCharge(), transportLegs: GetLegs(), costBreakdownCharges: null);

			bookingRateViewModel = BookingEngineRateViewModel.New(Factory, rate.Object);
			AssertEquals("ShowCostBreakdownCharges should be false when costBreakdownCharges is null", false, bookingRateViewModel.ShowCostBreakdownCharges);
		}

		BookingEngineChargeViewModel GetCharge(string currency = "USD")
		{
			var rate = new Mock<IBookingRate>();
			rate.SetupGet(r => r.Amount).Returns(10);
			rate.SetupGet(r => r.Currency).Returns(currency);
			rate.SetupGet(r => r.ChargeCode).Returns("Test");

			return BookingEngineChargeViewModel.New(Factory, rate.Object);
		}

		DummyBookingTransportLeg[] GetLegs()
		{
			var baseDate = new DateTime(2020, 2, 2);

			return new[]
			{
				new DummyBookingTransportLeg()
				{
					LegOrder = 3,
					TransportMode = Constants.TransportModes.Air,
					VesselType = "74F",
					VesselName = "Boeing 747-400 Freighter",
					VoyageNumber = "AA001",
					PortOfLoadingCode = "LHR",
					PortOfDischargeCode = "JFK",
					PortOfDischargeName = "New York",
					EstimatedDeparture = baseDate.AddHours(12),
					EstimatedArrival = baseDate.AddHours(18).AddMinutes(15)
				},
				new DummyBookingTransportLeg()
				{
					LegOrder = 1,
					TransportMode = Constants.TransportModes.Air,
					VesselType = "388",
					VesselName = "Airbus A380-800",
					VoyageNumber = "QF123",
					PortOfLoadingCode = "SYD",
					PortOfDischargeCode = "SIN",
					EstimatedDeparture = baseDate,
					EstimatedArrival = baseDate.AddHours(8).AddMinutes(30)
				},
				new DummyBookingTransportLeg()
				{
					LegOrder = 2,
					TransportMode = Constants.TransportModes.Sea,
					VesselType = "74F",
					VesselName = "Boeing 747-400 Freighter",
					VoyageNumber = "SQ008",
					PortOfLoadingCode = "SIN",
					PortOfLoadingName = "Singapore",
					PortOfDischargeCode = "LHR",
					PortOfDischargeName = "London Heathrow xxxx",
					EstimatedDeparture = baseDate.AddHours(12),
					EstimatedArrival = baseDate.AddHours(18).AddMinutes(15)
				}
			};
		}

		#region Implementation

		class DummyBookingTransportLeg : IBookingTransportLeg
		{
			public string TransportMode { get; set; }

			public int LegOrder { get; set; }

			public string VoyageNumber { get; set; }

			public string VesselType { get; set; }

			public string VesselName { get; set; }

			public DateTime EstimatedDeparture { get; set; }

			public DateTime EstimatedArrival { get; set; }

			public string PortOfLoadingCode { get; set; }

			public string PortOfLoadingName { get; set; }

			public string PortOfDischargeCode { get; set; }

			public string PortOfDischargeName { get; set; }
		}

		class DummyCodeDescription : ICodeDescription
		{
			public DummyCodeDescription(string code, string description)
			{
				Code = code;
				Description = description;
			}

			public object PK { get; set; }

			public string Code { get; set; }

			public string Description { get; set; }
		}

		class DummyCostBreakdownCharge : IBookingCostBreakdownCharge
		{
			public decimal Amount { get; set; }

			public string Currency { get; set; }

			public string ChargeCode { get; set; }

			public string ChargeCodeDescription { get; set; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			originalCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			GlbCompany.CurrentCompany.SetCurrency("USD");
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbCompany.CurrentCompany.SetCurrency(originalCurrency.Code);
		}

		RefCurrency originalCurrency;

		#endregion
	}
}
