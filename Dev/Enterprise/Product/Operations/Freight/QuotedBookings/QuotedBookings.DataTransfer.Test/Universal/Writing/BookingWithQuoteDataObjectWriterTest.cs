using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public class BookingWithQuoteDataObjectWriterTest : BaseShipmentDataObjectWriterTest
	{
		#region General Fields

		public void TestQuoteDetails()
		{
			BookingWithQuote.Quote.TH_QuoteNumber = "1234";
			BookingWithQuote.CompanyTariffLevel = "1";
			BookingWithQuote.OneOffQuoteApprovalStatus = true;
			var shipmentData = GetShipmentData(BookingWithQuote);

			AssertEquals("QuoteNumber", "1234", shipmentData.QuoteNumber);
			AssertEquals("CompanyTariffLevelOverride", (ZByte)1, shipmentData.CompanyTariffLevelOverride);
			AssertEquals("OneOffQuoteApprovalStatus", ZBool.True, shipmentData.IsQuoteApprovedByManager);
		}

		public void TestPaymentTermAndShipmentIncoTerm()
		{
			AssertPaymentTerm(true, "PPD", "Prepaid", "domestic prepaid");
			AssertPaymentTerm(true, "C3P", "Collect 3rd Party", "");

			AssertPaymentTerm(false, "CIF", "Cost, Insurance And Freight", "international incoterm: CIF");
			AssertPaymentTerm(false, "CPT", "Carriage Paid To", "");

			void AssertPaymentTerm(bool isDomestic, string code, string desc, string additionalTerms)
			{
				BookingWithQuote.IsDomesticFreight = isDomestic;
				BookingWithQuote.PaymentTerms = code;
				BookingWithQuote.AdditionalTerms = additionalTerms;

				var shipmentData = GetShipmentData(BookingWithQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions(() =>
				{
					AssertEquals("IsDomesticFreight", isDomestic, shipmentData.IsDomesticFreight);
					AssertEquals("ShipmentIncoTerm.Code", code, shipmentData.ShipmentIncoTerm.Code);
					AssertEquals("ShipmentIncoTerm.Description", desc, shipmentData.ShipmentIncoTerm.Description);
					AssertEquals("AdditionalTerms", additionalTerms, shipmentData.AdditionalTerms);
				});
			}
		}

		public void TestCarrier()
		{
			testHelper.AssertCarrier(QuoteBookingType.BookingWithQuote, (quotedBooking) =>
			{
				var carrier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
				carrier.OH_IsShippingProvider = true;

				quotedBooking.OH_Carrier = carrier.PK;
			});
		}

		public void TestCreditor()
		{
			testHelper.AssertCreditor(QuoteBookingType.BookingWithQuote, (quotedBooking) =>
			{
				var creditor = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
				creditor.OH_IsCreditor = true;

				quotedBooking.Booking.JS_OH_Creditor = creditor.PK;
			});
		}

		public void TestCarrierServiceLevel()
		{
			BookingWithQuote.CarrierServiceLevel = "STD";

			var shipmentData = GetShipmentData(BookingWithQuote);

			AssertEquals("ServiceLevel.Code", "STD", shipmentData.CarrierServiceLevel.Code);
			AssertEquals("ServiceLevel.Description", "Standard", shipmentData.CarrierServiceLevel.Description);
		}

		public void TestServiceLevel()
		{
			AssertServiceLevel("STD", "Standard");
			AssertServiceLevel("TSP", "Transhipment");

			void AssertServiceLevel(string code, string description)
			{
				BookingWithQuote.ServiceLevel = code;

				var shipmentData = GetShipmentData(BookingWithQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("ServiceLevel.Code", code, shipmentData.ServiceLevel.Code);
					AssertEquals("ServiceLevel.Description", description, shipmentData.ServiceLevel.Description);
				});
			}
		}

		public void TestQuotationClientAddress()
		{
			BookingWithQuote.Quote.QuotationClientAddress.E2_OA_Address = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress.PK;
			var shipmentData = GetShipmentData(BookingWithQuote);
			AssertOrganizationBO_WUFSHIJNB("Quotation Client Address", shipmentData.OrganizationAddressCollection.Single(x => x.AddressType.Value == "QuotationClientAddress"), "QuotationClientAddress");
		}

		public void TestVia()
		{
			AssertVia("INIXE", "Mangalore");
			AssertVia("AUSYD", "Sydney");

			void AssertVia(string code, string name)
			{
				BookingWithQuote.Via = code;

				var shipmentData = GetShipmentData(BookingWithQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("PortFirstForeign.Code", code, shipmentData.PortFirstForeign.Code);
					AssertEquals("PortFirstForeign.Name", name, shipmentData.PortFirstForeign.Name);
				});
			}
		}

		public void TestTransitTime()
		{
			AssertTransitTime("1", "1 day");
			AssertTransitTime("3", "3 days");
			AssertTransitTime("12", "12 days");

			void AssertTransitTime(string code, string name)
			{
				BookingWithQuote.TransitTime = code;

				var shipmentData = GetShipmentData(BookingWithQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {code}", () =>
				{
					AssertEquals("TransitTime.Code", code, shipmentData.TransitTime.Code);
					AssertEquals("TransitTime.Description", name, shipmentData.TransitTime.Description);
				});
			}
		}

		public void TestFrequency()
		{
			AssertFrequency(1, "DAYS", "Every X days");
			AssertFrequency(3, "FORTNIGHT", "X per fortnight");

			void AssertFrequency(int frequency, string frequencyUnitCode, string frequencyUnitDesc)
			{
				BookingWithQuote.Frequency = frequency;
				BookingWithQuote.FrequencyUnit = frequencyUnitCode;

				var shipmentData = GetShipmentData(BookingWithQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"Failed for {frequencyUnitCode}", () =>
				{
					AssertEquals("Frequency", frequency, shipmentData.Frequency);
					AssertEquals("FrequencyUnit.Code", frequencyUnitCode, shipmentData.FrequencyUnit.Code);
					AssertEquals("FrequencyUnit.Description", frequencyUnitDesc, shipmentData.FrequencyUnit.Description);
				});
			}
		}

		public void TestQuoteStatistics()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("EEE", "DesEEE");

			using (DataRegistryRating.Instance.OneOffQuoteKPISettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteSourceSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (DataRegistryRating.Instance.OneOffQuoteRevisionReasonSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				BookingWithQuote.OneOffQuoteStatistics.OneOffQuoteKPI = "EEE";
				BookingWithQuote.OneOffQuoteStatistics.OneOffQuoteSource = "EEE";
				BookingWithQuote.OneOffQuoteStatistics.OneOffQuoteRevisionReason = "EEE";

				var shipmentData = GetShipmentData(BookingWithQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions($"All three fields of OOQ statistics should be populated into XML", () =>
				{
					AssertEquals("QuoteKPI.Code", "EEE", shipmentData.QuoteKPI.Code);
					AssertEquals("QuoteKPI.Description", "DesEEE", shipmentData.QuoteKPI.Description);
					AssertEquals("QuoteSource.Code", "EEE", shipmentData.QuoteSource.Code);
					AssertEquals("QuoteSource.Description", "DesEEE", shipmentData.QuoteSource.Description);
					AssertEquals("QuoteRevisionReason.Code", "EEE", shipmentData.QuoteRevisionReason.Code);
					AssertEquals("QuoteRevisionReason.Description", "DesEEE", shipmentData.QuoteRevisionReason.Description);
				});
			}
		}

		#endregion

		#region Dates

		public void TestDates()
		{
			var startDate = new ZDate(2022, 10, 11);
			var endDate = new ZDate(2022, 10, 21);
			var acceptedDate = new ZDate(2022, 10, 8);
			var clientAcceptedDate = new ZDate(2022, 10, 9);
			var followUpDate = new ZDate(2022, 10, 10);

			// Quote
			BookingWithQuote.StartDate = startDate;
			BookingWithQuote.EndDate = endDate;
			BookingWithQuote.Quote.TH_Accepted = acceptedDate;
			BookingWithQuote.Quote.TH_ClientAccepted = clientAcceptedDate;
			BookingWithQuote.Quote.TH_FollowUpDate = followUpDate;

			// Booking
			BookingWithQuote.Booking.JS_TransportMode = "SEA"; // Sea Freight
			BookingWithQuote.Booking.JS_PackingMode = "LCL";
			BookingWithQuote.Booking.JS_ShipmentType = "STD"; // Standard House
			BookingWithQuote.Booking.JS_A_BKD = new ZDateTime(2011, 3, 11);
			BookingWithQuote.Booking.JS_A_RCV = new ZDateTime(2011, 3, 12);
			BookingWithQuote.Booking.JS_E_DEP = new ZDateTime(2011, 3, 13);
			BookingWithQuote.Booking.JS_E_ARV = new ZDateTime(2011, 3, 14);
			BookingWithQuote.Booking.JS_ClientRequestedETA = new ZDateTime(2011, 3, 15);
			BookingWithQuote.Booking.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "For Test";
			BookingWithQuote.Booking.JS_DeliveryDueDate = new ZDateTime(2012, 4, 15);
			BookingWithQuote.Booking.JS_RevisedDeliveryDueDate = new ZDateTimeOffset(2012, 4, 16);

			var shipmentData = GetShipmentData(BookingWithQuote);

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.DateCollection", shipmentData.DateCollection);

			var actualShipmentDateCollection = shipmentData.DateCollection
				.Select(date => new { Type = (DateType)date.Type!, IsEstimate = (ZBool)date.IsEstimate!, Value = (ZDate)date.Value! })
				.OrderBy(s => s.Value)
				.ToList();
			var expectedShipmentDateCollection = new[]
			{
				new { Type = DateType.Start, IsEstimate = ZBool.False, Value = startDate },
				new { Type = DateType.End, IsEstimate = ZBool.False, Value = endDate },
				new { Type = DateType.Accepted, IsEstimate = ZBool.False, Value = acceptedDate },
				new { Type = DateType.ClientAccepted, IsEstimate = ZBool.False, Value = clientAcceptedDate },
				new { Type = DateType.FollowUp, IsEstimate = ZBool.False, Value = followUpDate },
				new { Type = DateType.BookingConfirmed, IsEstimate = ZBool.False, Value = new ZDate(2011, 3, 11) },
				new { Type = DateType.Received, IsEstimate = ZBool.False, Value = new ZDate(2011, 3, 12) },
				new { Type = DateType.Departure, IsEstimate = ZBool.True, Value = new ZDate(2011, 3, 13) },
				new { Type = DateType.Arrival, IsEstimate = ZBool.True, Value = new ZDate(2011, 3, 14) },
				new { Type = DateType.ClientRequestedETA, IsEstimate = ZBool.True, Value = new ZDate(2011, 3, 15) },
				new { Type = DateType.DeliveryDueDate, IsEstimate = ZBool.False, Value = new ZDate(2012, 4, 15) },
				new { Type = DateType.RevisedDeliveryDueDate, IsEstimate = ZBool.False, Value = new ZDate(2012, 4, 16) },
			}.OrderBy(s => s.Value).ToList();

			for (int i = 0; i < actualShipmentDateCollection.Count; i++)
			{
				AssertEquals(expectedShipmentDateCollection[i].Type, actualShipmentDateCollection[i].Type);
				AssertEquals(expectedShipmentDateCollection[i].IsEstimate, actualShipmentDateCollection[i].IsEstimate);
				AssertEquals(expectedShipmentDateCollection[i].Value, actualShipmentDateCollection[i].Value);
			}
		}

		public void TestDateCollectionIsNull()
		{
			using (SetDataObjectWriterStrategy())
			{
				var startDate = new ZDate(2022, 10, 11);
				var endDate = new ZDate(2022, 10, 21);
				var acceptedDate = new ZDate(2022, 10, 8);
				var clientAcceptedDate = new ZDate(2022, 10, 9);
				var followUpDate = new ZDate(2022, 10, 10);

				// Quote
				BookingWithQuote.StartDate = startDate;
				BookingWithQuote.EndDate = endDate;
				BookingWithQuote.Quote.TH_Accepted = acceptedDate;
				BookingWithQuote.Quote.TH_ClientAccepted = clientAcceptedDate;
				BookingWithQuote.Quote.TH_FollowUpDate = followUpDate;

				// Booking
				BookingWithQuote.Booking.JS_TransportMode = "SEA"; // Sea Freight
				BookingWithQuote.Booking.JS_PackingMode = "LCL";
				BookingWithQuote.Booking.JS_ShipmentType = "STD"; // Standard House
				BookingWithQuote.Booking.JS_A_BKD = new ZDateTime(2011, 3, 11);
				BookingWithQuote.Booking.JS_A_RCV = new ZDateTime(2011, 3, 12);
				BookingWithQuote.Booking.JS_E_DEP = new ZDateTime(2011, 3, 13);
				BookingWithQuote.Booking.JS_E_ARV = new ZDateTime(2011, 3, 14);
				BookingWithQuote.Booking.JS_ClientRequestedETA = new ZDateTime(2011, 3, 15);
				BookingWithQuote.Booking.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "For Test";
				BookingWithQuote.Booking.JS_DeliveryDueDate = new ZDateTime(2012, 4, 15);
				BookingWithQuote.Booking.JS_RevisedDeliveryDueDate = new ZDateTimeOffset(2012, 4, 16);

				var shipmentData = GetShipmentData(BookingWithQuote);

				AssertNotNull("shipmentData", shipmentData);
				AssertNull("shipmentData.DateCollection", shipmentData.DateCollection);
			}
		}

		#endregion

		#region Brokerage Details

		public void TestBrokerageDetails()
		{
			AssertBrokerageDetails(1, 12);
			AssertBrokerageDetails(3, 33);

			void AssertBrokerageDetails(short numOfEntries, short numOfLines)
			{
				BookingWithQuote.QuoteNumberOfEntries = numOfEntries;
				BookingWithQuote.QuoteNumberOfEntryLines = numOfLines;

				var shipmentData = GetShipmentData(BookingWithQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions(() =>
				{
					AssertEquals("QuoteNumberOfEntries", numOfEntries, shipmentData.QuoteNumberOfEntries);
					AssertEquals("QuoteNumberOfEntryLines", numOfLines, shipmentData.QuoteNumberOfEntryLines);
				});
			}
		}

		#endregion

		#region Collections

		public void TestAdditionalReferenceNumbers()
		{
			BookingWithQuote.Quote.CurrentOneOffQuote.Numbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory.BOFactory, "CQN", "C0Q0N1"));
			var shipmentData = GetShipmentData(BookingWithQuote);

			AssertNotNull("Precondition: shipmentData", shipmentData);
			AssertNotNull("Precondition: shipmentData.AdditionalReferenceCollection", shipmentData.AdditionalReferenceCollection);
			CombineAssertions(() =>
			{
				AssertEquals("shipmentData.AdditionalReferenceCollection.Count", 1, shipmentData.AdditionalReferenceCollection.Count);
				AdditionalReferenceDataObjectWriterTest.AssertContents(shipmentData.AdditionalReferenceCollection[0], "CQN", "Carrier Quote Number", "C0Q0N1");
			});
		}

		#endregion

		#region Goods Details & Monetory Values

		public void TestGoodsDetails_LSE()
		{
			BookingWithQuote.Mode = "LSE";

			AssertGeneralGoodsDetails(5, "KG", "Kilograms", 0.3m, "M3", "Cubic Meters", 30);
			AssertGeneralGoodsDetails(50, "MG", "Milligrams", 0.5m, "CI", "Cubic Inches", 95);

			AssertEquipmentAndCommodity("HUL", "Hand Unload/Load by Premise", "HWL", "Hand Unload/Load by Haulier", "HAZ", "HAZARDOUS GOODS");
			AssertEquipmentAndCommodity("HSL", "Haulier Supplies Lift", "PSL", "Premise Supplies Lift", "SUGR", "SUGAR");

			void AssertGeneralGoodsDetails(decimal weight, string weightUnitCode, string weightUnitDesc,
				decimal volume, string volumeUnitCode, string volumeUnitDesc,
				decimal chargeable)
			{
				BookingWithQuote.Weight = weight;
				BookingWithQuote.WeightUnit = weightUnitCode;

				BookingWithQuote.Volume = volume;
				BookingWithQuote.VolumeUnit = volumeUnitCode;

				BookingWithQuote.Chargeable = chargeable;

				var shipmentData = GetShipmentData(BookingWithQuote);
				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions(() =>
				{
					AssertEquals("TotalWeight", weight, shipmentData.TotalWeight);
					AssertEquals("TotalWeightUnit.Code", weightUnitCode, shipmentData.TotalWeightUnit.Code);
					AssertEquals("TotalWeightUnit.Description", weightUnitDesc, shipmentData.TotalWeightUnit.Description);

					AssertEquals("TotalVolume", volume, shipmentData.TotalVolume);
					AssertEquals("TotalVolumeUnit.Code", volumeUnitCode, shipmentData.TotalVolumeUnit.Code);
					AssertEquals("TotalVolumeUnit.Description", volumeUnitDesc, shipmentData.TotalVolumeUnit.Description);

					AssertEquals("ActualChargeable", chargeable, shipmentData.ActualChargeable);
				});
			}
		}

		public void TestGoodsDetails_FCL()
		{
			BookingWithQuote.Mode = "FCL";

			AssertEquipmentAndCommodity("LOF", "Drop Container - Premise supplies Lift", "SDL", "Drop Container with Sideloader", "HAZ", "HAZARDOUS GOODS");
			AssertEquipmentAndCommodity("TRL", "Drop Trailer", "WUP", "Wait for Pack/Unpack", "SUGR", "SUGAR");
		}

		void AssertEquipmentAndCommodity(string pickupEquipmentCode, string pickupEquipmentDesc,
				string deliveryEquipmentCode, string deliveryEquipmentDesc,
				string commodityCode, string commodityDesc)
		{
			BookingWithQuote.PickupEquipment = pickupEquipmentCode;
			BookingWithQuote.DeliveryEquipment = deliveryEquipmentCode;
			BookingWithQuote.Commodity = commodityCode;

			var shipmentData = GetShipmentData(BookingWithQuote);
			AssertNotNull("shipmentData", shipmentData);

			var goodsDetails = shipmentData.LocalProcessing;
			AssertNotNull("goodsDetails", goodsDetails);

			CombineAssertions(() =>
			{
				AssertEquals("PickupEquipmentNeeded.Code", pickupEquipmentCode, goodsDetails.PickupEquipmentNeeded.Code);
				AssertEquals("PickupEquipmentNeeded.Description", pickupEquipmentDesc, goodsDetails.PickupEquipmentNeeded.Description);

				AssertEquals("DeliveryEquipmentNeeded.Code", deliveryEquipmentCode, goodsDetails.DeliveryEquipmentNeeded.Code);
				AssertEquals("DeliveryEquipmentNeeded.Description", deliveryEquipmentDesc, goodsDetails.DeliveryEquipmentNeeded.Description);

				AssertEquals("Commodity.Code", commodityCode, goodsDetails.Commodity.Code);
				AssertEquals("Commodity.Description", commodityDesc, goodsDetails.Commodity.Description);
			});
		}

		public void TestLocalProcessing()
		{
			var shipmentBO = BookingWithQuote.Booking;
			#region Setup shipmentBO
			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "FCL";
			shipmentBO.JS_ShipmentType = "STD";
			var docsBO = shipmentBO.DocsAndCartage;
			docsBO.JP_FCLPickupEquipmentNeeded = "TRL";
			docsBO.JP_EstimatedPickup = new ZDateTime(2011, 5, 1);
			docsBO.JP_PickupRequiredBy = new ZDateTime(2011, 5, 2);
			docsBO.JP_PickupCartageAdvised = new ZDateTime(2011, 5, 3);
			docsBO.JP_ArrivalCartageRef = "ARRCARTREF";
			docsBO.JP_PickupCartageCompleted = new ZDateTime(2011, 5, 4);
			docsBO.JP_PickupLabourTime = new ZDateTime(2011, 5, 5);
			docsBO.JP_PickupLabourCharge = 1.11m;
			docsBO.JP_PickupTruckWaitTime = new ZDateTime(2011, 5, 6);
			docsBO.JP_PickupTruckWaitCharge = 2.22m;
			docsBO.JP_PrintOptionForPackagesOnAWB = "ALL";
			docsBO.JP_FCLDeliveryEquipmentNeeded = "WUP";
			docsBO.JP_FCLAvailable = new ZDateTime(2011, 5, 7);
			docsBO.JP_FCLStorageCommences = new ZDateTime(2011, 5, 8);
			docsBO.JP_LCLAvailable = new ZDateTime(2011, 5, 9);
			docsBO.JP_LCLStorageCommences = new ZDateTime(2011, 5, 10);
			docsBO.JP_LCLAirStorageDaysOrHours = new ZByte(12);
			docsBO.JP_LCLAirStorageCharge = 3.33m;
			docsBO.JP_EstimatedDelivery = new ZDateTime(2011, 5, 11);
			docsBO.JP_DeliveryRequiredBy = new ZDateTime(2011, 5, 12);
			docsBO.JP_DeliveryCartageAdvised = new ZDateTime(2011, 5, 13);
			docsBO.JP_DeliveryCartageCompleted = new ZDateTime(2011, 5, 14);
			docsBO.JP_DeliveryLabourTime = new ZDateTime(2011, 5, 15);
			docsBO.JP_DeliveryLabourCharge = 4.44m;
			docsBO.JP_DeliveryTruckWaitTime = new ZDateTime(2011, 5, 16);
			docsBO.JP_DeliveryTruckWaitCharge = 5.55m;
			docsBO.JP_HasProhibitedPackaging = ZBool.True;
			docsBO.JP_InsuranceRequired = ZBool.False;
			docsBO.JP_IsContingencyRelease = ZBool.True;
			docsBO.JP_LCLDatesOverrideConsol = ZBool.True;
			docsBO.JP_ExportStatement = "DEF";
			var orderBO = docsBO.OrderItems.AddNew();
			orderBO.JT_Sequence = 1;
			orderBO.JT_OrderReference = "ORDER_FF";
			var serviceBO = docsBO.Services.AddNew();
			serviceBO.ES_ServiceCode = "TAI";
			serviceBO.ES_Booked = new ZDateTime(2011, 6, 1);
			serviceBO.ES_Completed = new ZDateTime(2011, 6, 2);
			serviceBO.ES_Duration = new ZDateTime(2011, 6, 3);
			serviceBO.ES_ServiceCount = 1.23m;
			serviceBO.ES_ServiceNote = "BLAH BLAH BLAH BLAH Hotdog BLAH BLAH BLAH.";
			serviceBO.ES_References = "GREAT!";
			serviceBO.ES_OH_Contractor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			#endregion
			var shipmentData = GetShipmentData(BookingWithQuote);
			#region Check Contents of shipmentData object
			AssertNotNull("shipmentData", shipmentData);
			var localProcessing = shipmentData.LocalProcessing;
			AssertNotNull("shipmentData.LocalProcessing", localProcessing);
			CombineAssertions("Checking all fields on LocalProcessing", () =>
			{
				AssertEquals("localProcessing.FCLPickupEquipmentNeeded.Code", "TRL", localProcessing.FCLPickupEquipmentNeeded.Code);
				AssertEquals("localProcessing.FCLPickupEquipmentNeeded.Description", "Drop Trailer", localProcessing.FCLPickupEquipmentNeeded.Description);
				AssertEquals("localProcessing.EstimatedPickup", new ZDateTime(2011, 5, 1), localProcessing.EstimatedPickup);
				AssertEquals("localProcessing.PickupRequiredBy", new ZDateTime(2011, 5, 2), localProcessing.PickupRequiredBy);
				AssertEquals("localProcessing.PickupCartageAdvised", null, localProcessing.PickupCartageAdvised);
				AssertEquals("localProcessing.ArrivalCartageRef", null, localProcessing.ArrivalCartageRef);
				AssertEquals("localProcessing.PickupCartageCompleted", null, localProcessing.PickupCartageCompleted);
				AssertEquals("localProcessing.PickupLabourTime", null, localProcessing.PickupLabourTime);
				AssertEquals("localProcessing.PickupLabourCharge", null, localProcessing.PickupLabourCharge);
				AssertEquals("localProcessing.DemurrageOnPickupTime", null, localProcessing.DemurrageOnPickupTime);
				AssertEquals("localProcessing.PickupTruckWaitTime", null, localProcessing.PickupTruckWaitTime);
				AssertEquals("localProcessing.DemurrageOnPickupCharge", null, localProcessing.DemurrageOnPickupCharge);
				AssertEquals("localProcessing.PickupTruckWaitCharge", null, localProcessing.PickupTruckWaitCharge);
				AssertEquals("localProcessing.PrintOptionForPackagesOnAWB", null, localProcessing.PrintOptionForPackagesOnAWB);
				AssertEquals("localProcessing.FCLDeliveryEquipmentNeeded.Code", "WUP", localProcessing.FCLDeliveryEquipmentNeeded.Code);
				AssertEquals("localProcessing.FCLDeliveryEquipmentNeeded.Description", "Wait for Pack/Unpack", localProcessing.FCLDeliveryEquipmentNeeded.Description);
				AssertEquals("localProcessing.FCLAvailable", null, localProcessing.FCLAvailable);
				AssertEquals("localProcessing.FCLStorageCommences", null, localProcessing.FCLStorageCommences);
				AssertEquals("localProcessing.LCLAvailable", null, localProcessing.LCLAvailable);
				AssertEquals("localProcessing.LCLStorageCommences", null, localProcessing.LCLStorageCommences);
				AssertEquals("localProcessing.LCLAirStorageDaysOrHours", null, localProcessing.LCLAirStorageDaysOrHours);
				AssertEquals("localProcessing.LCLAirStorageCharge", null, localProcessing.LCLAirStorageCharge);
				AssertEquals("localProcessing.EstimatedDelivery", new ZDateTime(2011, 5, 11), localProcessing.EstimatedDelivery);
				AssertEquals("localProcessing.DeliveryRequiredBy", new ZDateTime(2011, 5, 12), localProcessing.DeliveryRequiredBy);
				AssertEquals("localProcessing.DeliveryCartageAdvised", null, localProcessing.DeliveryCartageAdvised);
				AssertEquals("localProcessing.DeliveryCartageCompleted", null, localProcessing.DeliveryCartageCompleted);
				AssertEquals("localProcessing.DeliveryLabourTime", null, localProcessing.DeliveryLabourTime);
				AssertEquals("localProcessing.DeliveryLabourCharge", null, localProcessing.DeliveryLabourCharge);
				AssertEquals("localProcessing.DemurrageOnDeliveryTime", null, localProcessing.DemurrageOnDeliveryTime);
				AssertEquals("localProcessing.DeliveryTruckWaitTime", null, localProcessing.DeliveryTruckWaitTime);
				AssertEquals("localProcessing.DemurrageOnDeliveryCharge", null, localProcessing.DemurrageOnDeliveryCharge);
				AssertEquals("localProcessing.DeliveryTruckWaitCharge", null, localProcessing.DeliveryTruckWaitCharge);
				AssertEquals("localProcessing.HasProhibitedPackaging", null, localProcessing.HasProhibitedPackaging);
				AssertEquals("localProcessing.InsuranceRequired", ZBool.False, localProcessing.InsuranceRequired);
				AssertEquals("localProcessing.IsContingencyRelease", null, localProcessing.IsContingencyRelease);
				AssertEquals("localProcessing.LCLDatesOverrideConsol", null, localProcessing.LCLDatesOverrideConsol);
				AssertEquals("localProcessing.ExportStatement", null, localProcessing.ExportStatement);
				AssertNotNull("localProcessing.OrderNumberCollection", localProcessing.OrderNumberCollection);
				AssertNotNull("localProcessing.AdditionalServiceCollection", localProcessing.AdditionalServiceCollection);
			}

			);
			AssertEquals("localProcessing.OrderNumberCollection.Count", 1, localProcessing.OrderNumberCollection.Count);
			var orderNumber = localProcessing.OrderNumberCollection[0];
			AssertEquals("orderNumber.Sequence", new ZShort(1), orderNumber.Sequence);
			AssertEquals("orderNumber.OrderReference", "ORDER_FF", orderNumber.OrderReference);
			AssertEquals("localProcessing.AdditionalServiceCollection.Count", 1, localProcessing.AdditionalServiceCollection.Count);
			var additionalService = localProcessing.AdditionalServiceCollection[0];
			CombineAssertions("Checking all fields on AdditionalService", () =>
			{
				AssertEquals("additionalService.ServiceCode.Code", "TAI", additionalService.ServiceCode.Code);
				AssertEquals("additionalService.ServiceCode.Description", "Tailgate", additionalService.ServiceCode.Description);
				AssertEquals("additionalService.Booked", new ZDateTime(2011, 6, 1), additionalService.Booked);
				AssertEquals("additionalService.Completed", new ZDateTime(2011, 6, 2), additionalService.Completed);
				AssertEquals("additionalService.Duration", null, additionalService.Duration);
				AssertEquals("additionalService.ServiceCount", null, additionalService.ServiceCount);
				AssertEquals("additionalService.ServiceNote", null, additionalService.ServiceNote);
				AssertEquals("additionalService.References", null, additionalService.References);
			}

			);
			AssertOrganizationBO_WUFSHIJNB("additionalService.Contractor", additionalService.Contractor, "Contractor");
			#endregion
		}

		public void TestMonetaryValues()
		{
			AssertMonetaryValues(10000, "AUD", "Australian Dollar", 200, "USD", "United States Dollar");
			AssertMonetaryValues(20000, "USD", "United States Dollar", 50, "AUD", "Australian Dollar");

			void AssertMonetaryValues(decimal expectedGoodsValue, string expectedGoodsCurrencyCode, string expectedGoodsCurrencyDescription,
				decimal expectedInsuranceValue, string expectedInsuranceCurrencyCode, string expectedInsuranceCurrencyDescription)
			{
				BookingWithQuote.GoodsValue = expectedGoodsValue;
				BookingWithQuote.GoodsCurrency = expectedGoodsCurrencyCode;

				BookingWithQuote.InsuranceValue = expectedInsuranceValue;
				BookingWithQuote.InsuranceCurrency = expectedInsuranceCurrencyCode;

				var shipmentData = GetShipmentData(BookingWithQuote);

				AssertNotNull("shipmentData", shipmentData);

				CombineAssertions(() =>
				{
					AssertEquals("GoodsValue", expectedGoodsValue, shipmentData.GoodsValue);
					AssertEquals("GoodsValueCurrency.Code", expectedGoodsCurrencyCode, shipmentData.GoodsValueCurrency.Code);
					AssertEquals("GoodsValueCurrency.Description", expectedGoodsCurrencyDescription, shipmentData.GoodsValueCurrency.Description);

					AssertEquals("InsuranceValue", expectedInsuranceValue, shipmentData.InsuranceValue);
					AssertEquals("InsuranceValueCurrency.Code", expectedInsuranceCurrencyCode, shipmentData.InsuranceValueCurrency.Code);
					AssertEquals("InsuranceValueCurrency.Description", expectedInsuranceCurrencyDescription, shipmentData.InsuranceValueCurrency.Description);
				});
			}
		}

		#endregion

		#region Workflow Custom Fields

		public void TestWorkflowCustomFields()
			=> testHelper.AssertWorkflowCustomFields(QuoteBookingType.BookingWithQuote);

		public void TestWorkflowCustomFields_FromMatchedWorkflow()
			=> testHelper.AssertWorkflowCustomFields_FromMatchedWorkflow(QuoteBookingType.BookingWithQuote, QuotedBooking.BookingWithQuoteCode, (quotedBooking) => quotedBooking.Booking);

		#endregion

		#region Quote Charges

		public void TestQuoteCharges_ShouldIncludeCostDataIfRecipientTypeIsORP()
			=> testHelper.AssertQuoteCharges_ShouldIncludeCostDataIfRecipientTypeIsORP(BookingWithQuote, GetShipmentData);

		#endregion

		#region Notes

		public void TestNotes_BookingWithQuote()
		{
			testHelper.AssertNotes(BookingWithQuote);
		}

		#endregion

		QuotedBooking BookingWithQuote => bookingWithQuote ?? (bookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory));
		QuotedBooking bookingWithQuote;

		static Shipment GetShipmentData(QuotedBooking quotedBooking)
			=> GetShipmentData(quotedBooking, RecipientRoleType.ORP);

		static Shipment GetShipmentData(QuotedBooking quotedBooking, RecipientRoleType recipientRoleType)
		{
			var writer = new BookingWithQuoteDataObjectWriter(new DataWritingManager(new ActionInfo(recipientRoleType, quotedBooking), writerStrategy: dataObjectWriterStrategy), quotedBooking);
			return writer.GetDataObject(quotedBooking.Booking);
		}

		protected override BaseShipmentDataObjectWriter GetNewShipmentDataObjectWriter(BusinessObject topLevelBO)
		{
			var bookingBO = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory.BOFactory);
			return new BookingWithQuoteDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO)), bookingBO);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new QuotedBookingDataObjectWriterTestHelper(Factory, GetShipmentData);
		}

		QuotedBookingDataObjectWriterTestHelper testHelper;

		public static IDisposable SetDataObjectWriterStrategy()
		{
			dataObjectWriterStrategy = new DateCollectionNotAllowedToSetWriterStrategy();
			return new DisposableAction(() => dataObjectWriterStrategy = null);
		}

		static IDataObjectWriterStrategy dataObjectWriterStrategy;

		#region Implementation

		public class DateCollectionNotAllowedToSetWriterStrategy : IDataObjectWriterStrategy
		{
			public bool IsAllowSet(string fieldName)
			{
				return "DateCollection" != fieldName;
			}
		}

		#endregion
	}
}
