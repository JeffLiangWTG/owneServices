using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.CarrierConnect;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;
using Constants = Enterprise.Core.Constants;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.GUI.Testing
{
	public class GlowRateSelectorJobUpdaterTest : RatingTestCase
	{
		UrsHelper UrsHelper => new (Factory, Helper);

		public void TestGetAndApplyAllConfirmations_Consol()
		{
			var consol = CreateForwardingConsol().consol;
			var results = GetAllConfirmations(consol.RatingAdapter, out var rateResult);

			AssertAndApplyAllConfirmations(results, new Dictionary<JobConfirmationType, string>
			{
				{ JobConfirmationType.Origin, "AUMEL" },
				{ JobConfirmationType.Destination, "USLAX" },
				{ JobConfirmationType.Carrier, "Transport Provider 1" },
				{ JobConfirmationType.CarrierContractNumber, "Test123" },
				{ JobConfirmationType.PaymentTerm, "PPD" },
				{ JobConfirmationType.ServiceLevel, "STD" },
				{ JobConfirmationType.AutoratingDate, null }
			});

			AssertEquals("Origin", rateResult.Origin, consol.JK_RL_NKLoadPort);
			AssertEquals("Destination", rateResult.Destination, consol.JK_RL_NKDischargePort);
			AssertEquals("Carrier", TransportProvider2.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			AssertEquals("Payment term", rateResult.PaymentTerm, consol.JK_PrepaidCollect);
			AssertEquals("Service level", rateResult.CarrierServiceLevel, consol.JK_AWBServiceLevel);
			AssertEquals("Contract Number", rateResult.CarrierContractNumber, consol.JK_CarrierContractNumber);
			AssertEquals("Autorate Date", ZDate.BrettsBirthday, consol.AutoratingDate);
		}

		public void TestGetAndApplyAllConfirmations_Spot()
		{
			var consol = CreateForwardingConsol().consol;
			var results = GetUpdatesForSpot(consol.RatingAdapter, out var rateResult);

			AssertAndApplyAllConfirmations(results.JobUpdates, new Dictionary<JobConfirmationType, string>
			{
				{ JobConfirmationType.Origin, "AUMEL" },
				{ JobConfirmationType.Destination, "USLAX" },
				{ JobConfirmationType.Carrier, "Transport Provider 1" },
				{ JobConfirmationType.CarrierContractNumber, "Test123" },
				{ JobConfirmationType.PaymentTerm, "PPD" },
				{ JobConfirmationType.ServiceLevel, "STD" },
				{ JobConfirmationType.AutoratingDate, null },
				{ JobConfirmationType.Schedule, null },
				{ JobConfirmationType.SpotBookingTerms, null },
				{ JobConfirmationType.SpotPenalties, null },
				{ JobConfirmationType.SendSpotBooking, null },
			});

			AssertEquals("Origin", rateResult.Origin, consol.JK_RL_NKLoadPort);
			AssertEquals("Destination", rateResult.Destination, consol.JK_RL_NKDischargePort);
			AssertEquals("Carrier", TransportProvider2.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			AssertEquals("Payment term", rateResult.PaymentTerm, consol.JK_PrepaidCollect);
			AssertEquals("Service level", rateResult.CarrierServiceLevel, consol.JK_AWBServiceLevel);
			AssertEquals("Contract Number", rateResult.CarrierContractNumber, consol.JK_CarrierContractNumber);
			AssertEquals("Autorate Date", ZDate.BrettsBirthday, consol.AutoratingDate);
		}

		public void TestGetAndApplyAllConfirmations_Shipment()
		{
			var (consol, shipment) = CreateForwardingConsol();
			var testShipmentAdapter = new ShipmentAdapterForTest(shipment);
			var results = GetAllConfirmations(testShipmentAdapter, out var rateResult);

			AssertAndApplyAllConfirmations(results, new Dictionary<JobConfirmationType, string>
			{
				{ JobConfirmationType.Origin, "AUMEL" },
				{ JobConfirmationType.Destination, "USLAX" },
				{ JobConfirmationType.Carrier, "Transport Provider 1" },
				{ JobConfirmationType.CarrierContractNumber, "Test123" },
				{ JobConfirmationType.PaymentTerm, "PPD" },
				{ JobConfirmationType.ServiceLevel, "STD" }
			});

			AssertEquals("Origin", rateResult.Origin, consol.JK_RL_NKLoadPort);
			AssertEquals("Destination", rateResult.Destination, consol.JK_RL_NKDischargePort);
			AssertEquals("Carrier", TransportProvider2.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			AssertEquals("Payment term", rateResult.PaymentTerm, consol.JK_PrepaidCollect);
			AssertEquals("Service level", rateResult.CarrierServiceLevel, consol.JK_AWBServiceLevel);
			AssertEquals("Contract Number", rateResult.CarrierContractNumber, consol.JK_CarrierContractNumber);
		}

		public void TestGetAndApplyAllConfirmations_BookingWithQuote()
		{
			var bookingWithQuote = CreateBookingWithQuote();
			var results = GetAllConfirmations(bookingWithQuote.GetFirstAdapter(), out var rateResult);

			AssertAndApplyAllConfirmations(results, new Dictionary<JobConfirmationType, string>
			{
				{ JobConfirmationType.Origin, "AUMEL" },
				{ JobConfirmationType.Destination, "USLAX" },
				{ JobConfirmationType.Carrier, null },
				{ JobConfirmationType.CarrierContractNumber, "Test123" },
				{ JobConfirmationType.ServiceLevel, "STD" }
			});

			AssertQuotedBookingUpdated(bookingWithQuote, rateResult);
		}

		public void TestGetAndApplyAllConfirmations_OneOffQuote()
		{
			var oneOffQuote = CreateOneOffQuote();
			var results = GetAllConfirmations(oneOffQuote.GetFirstAdapter(), out var rateResult);

			AssertAndApplyAllConfirmations(results, new Dictionary<JobConfirmationType, string>
			{
				{ JobConfirmationType.Origin, "AUMEL" },
				{ JobConfirmationType.Destination, "USLAX" },
				{ JobConfirmationType.Carrier, null },
				{ JobConfirmationType.CarrierContractNumber, "Test123" },
				{ JobConfirmationType.ServiceLevel, "STD" }
			});

			var contractNumber = oneOffQuote.Quote.CurrentOneOffQuote.Numbers.ListContractNumbers().First().CE_EntryNum;

			AssertEquals("Origin", rateResult.Origin, oneOffQuote.Origin);
			AssertEquals("Destination", rateResult.Destination, oneOffQuote.Destination);
			AssertEquals("Carrier", TransportProvider2.PK, oneOffQuote.Carrier.PK);
			AssertEquals("Service level", rateResult.CarrierServiceLevel, oneOffQuote.CarrierServiceLevel);
			AssertEquals("Contract number", rateResult.CarrierContractNumber, contractNumber);
		}

		public void TestGetAndApplyAllConfirmations_QuickBooking()
		{
			var quickBooking = CreateQuickBooking();
			var results = GetAllConfirmations(quickBooking.GetFirstAdapter(), out var rateResult);

			AssertAndApplyAllConfirmations(results, new Dictionary<JobConfirmationType, string>
			{
				{ JobConfirmationType.Origin, "AUMEL" },
				{ JobConfirmationType.Destination, "USLAX" },
				{ JobConfirmationType.Carrier, null },
				{ JobConfirmationType.CarrierContractNumber, "Test123" },
				{ JobConfirmationType.ServiceLevel, "STD" }
			});

			AssertQuotedBookingUpdated(quickBooking, rateResult);
		}

		void AssertQuotedBookingUpdated(QuotedBooking booking, RateResultDto rateResult)
		{
			AssertEquals("Origin", rateResult.Origin, booking.LoadPort);
			AssertEquals("Destination", rateResult.Destination, booking.DischargePort);
			AssertEquals("Carrier", TransportProvider2.PK, booking.Carrier.PK);
			AssertEquals("Service level", rateResult.CarrierServiceLevel, booking.CarrierServiceLevel);
			AssertEquals("Contract number", rateResult.CarrierContractNumber, booking.CarrierContractNumber);
		}

		void AssertAndApplyAllConfirmations(List<JobUpdateDto> confirmations, Dictionary<JobConfirmationType, string> expectedConfirmations, ApplyRateRequestDto applyRatesRequest = null)
		{
			var confirmationsToCheck = confirmations.Where(c => c.CommitUpdate != null).ToList();
			var actualTypes = confirmationsToCheck.Select(c => c.Type);
			AssertContainsExactElementsInAnyOrder("Should only contain expected confirmations.", expectedConfirmations.Keys, actualTypes);

			foreach (var confirmation in confirmationsToCheck)
			{
				Assert($"Unexpected confirmation type: {confirmation.Type}", expectedConfirmations.TryGetValue(confirmation.Type, out var expectedValue));
				AssertEquals($"Unexpected current value for confirmation type {confirmation.Type}", expectedValue, confirmation.CurrentValue);
			}

			applyRatesRequest ??= new ApplyRateRequestDto();
			confirmationsToCheck.ForEach(c => c.CommitUpdate!(applyRatesRequest));
		}

		List<JobUpdateDto> GetAllConfirmations(IAutoRating job, out RateResultDto rateResult)
		{
			var criteria = new RatingCriteria(job, Factory);

			var costing = Helper.NewCosting(TransportProvider2);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUMEL",
				"USLAX", ZString.Empty, "20GP", removeLines: true);
			var frtLine = costEntry.AddFlatRateLine("FRT", 20m, "AUD");

			var autoRateInfo = new AutoRateInfo(Factory, frtLine);
			var autoRateInfoCollection = new AutoRateInfoCollection(Factory) { autoRateInfo };

			rateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "HKHKG",
				CarrierContractNumber = "AnotherValue",
				PaymentTerm = "CCX",
				CarrierServiceLevel = "EXP"
			};

			return GlowRateSelectorJobUpdater.GetUpdateConfirmations(criteria, null!, autoRateInfoCollection, rateResult, ZDate.BrettsBirthday.ToZDateTime()).JobUpdates;
		}

		JobUpdateCollectionDto GetUpdatesForSpot(IAutoRating job, out RateResultDto rateResult)
		{
			var ursLine = UrsHelper.CreateUrsLine("FRT", "CN", "AUD", 5m, "UNT");
			var info = new UrsBookingInfo
			{
				BookingTerms = new BookingTerms
				{
					Items = [
						new BookingTermItem
						{
							Name = "Description",
							Currency = "USD",
							Fee = 100,
						}
					]
				},
				Schedule = new WiseRates.Api.Model.Schedule
				{
					ScheduleDetails = [
						new ScheduleDetail
						{
							DateInfos = [
								new ScheduleDateInfo
								{
									Code = "CY",
									Name = "Commercial cargo cutoff",
									Type = "Documentation",
									Date = new DateTime(2020, 12, 12)
								},
								new ScheduleDateInfo
								{
									Code = "XX",
									Name = "Origin demurrage",
									Type = "STO",
									Date = new DateTime(2020, 12, 15)
								}
							]
						}
					]
				},
				Penalties = [
					new Penalty
					{
						Direction = "IMP",
						Name = "Detention",
						Currency = "AUD",
						StartDay = 3
					}
				]
			};
			var ursEntry = UrsHelper.CreateUrsEntry(
				mode: "SEA",
				category: "FCL",
				origin: "AUSYD",
				destination: "UAIEV",
				commodityCode: "",
				rateProvider: "URS",
				startDate: new ZDate(2020, 06, 06),
				endDate: new ZDate(2030, 06, 06),
				bookingInfo: info,
				rateLines: ursLine
			);
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			ursEntry.TI_RC = container.PK;
			ursEntry.ChildRateLines = new[] { ursLine };
			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider2));
			ursServiceProvider.ChildRateEntries = new[] { ursEntry };

			var autoRateInfo = new AutoRateInfo(Factory, ursLine);
			var autoRateInfoCollection = new AutoRateInfoCollection(Factory) { autoRateInfo };

			rateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "HKHKG",
				CarrierContractNumber = "AnotherValue",
				PaymentTerm = "CCX",
				CarrierServiceLevel = "EXP"
			};

			return GlowRateSelectorJobUpdater.GetUpdateConfirmations(new RatingCriteria(job, Factory), null!, autoRateInfoCollection, rateResult, ZDate.BrettsBirthday.ToZDateTime());
		}

		(ForwardingConsol consol, ForwardingShipment shipment) CreateForwardingConsol(string origin = "AUMEL", string destination = "USLAX")
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			consol.JK_CarrierContractNumber = "Test123";
			consol.JK_PrepaidCollect = "PPD";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1234";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ActualVolume = 0.5M;
			shipment.JS_ActualWeight = 55M;
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			Factory.Save();
			return (consol, shipment);
		}

		QuotedBooking CreateBookingWithQuote() => CreateQuotedBooking(QuotedBookingState.AcceptedBookingWithQuote);

		QuotedBooking CreateOneOffQuote()
		{
			var qb = CreateQuotedBooking(QuotedBookingState.QuoteOnly);
			qb.Quote.CurrentOneOffQuote.Numbers.AddOrSkipContractNumber("Test123");
			return qb;
		}

		QuotedBooking CreateQuickBooking() => CreateQuotedBooking(QuotedBookingState.BookingOnly);

		QuotedBooking CreateQuotedBooking(QuotedBookingState state)
		{
			var booking = BaseRatingIntegrationTest.CreateQuotedBooking(Factory, "SEA", "FCL", "PPD", null, null, null, null, "AUMEL", "USLAX", 10m, 1m, state);
			booking.CarrierContractNumber = "Test123";
			return booking;
		}

		class ShipmentAdapterForTest : ForwardingShipmentRateSelectorEnabledRatingAdapter
		{
			public ShipmentAdapterForTest(ForwardingShipment shipment) : base(shipment) { }
		}
	}
}
