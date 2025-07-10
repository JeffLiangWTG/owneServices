using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public static class CO2eTestHelper
	{
		public static UniversalShipment GetSampleCO2eResponseDataObjectForQuotedBooking()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.DataProviderForCodeMapping = "WTG Greenhouse Gas Emission";
			dataObject.TotalWeight = 1m;
			dataObject.TotalWeightUnit = new UnitOfWeight { Code = "T" };
			dataObject.TransportMode = new CodeDescriptionPair { Code = "SEA" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "VNVNH" };
			dataObject.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = 10000m,
				CO2eUnit = new UnitOfWeight { Code = "KG" },
				CO2ePerTonne = 10,
				CO2ePerTonneUnit = new UnitOfWeight { Code = "T" }
			};
			return dataObject;
		}

		public static QuotedBooking CreateQuotedBooking(BusinessObjectFactory factory, string origin = "AUSYD", string destination = "VNVNH", string loadPort = "", string dischargePort = "", string via = "", string sailingLoad = "", string sailingDischarge = "")
		{
			var quotePK = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK;
			var bookingPK = QuotedBooking.CreateNewBooking(factory).PK;
			var quotedBooking = QuotedBooking.New(quotePK, bookingPK, factory);
			quotedBooking.Mode = "FCL";
			quotedBooking.Origin = origin;
			quotedBooking.Destination = destination;
			quotedBooking.LoadPort = loadPort;
			quotedBooking.DischargePort = dischargePort;
			quotedBooking.Via = via;
			var pack = quotedBooking.Booking.OuterPackLines.AddNew();
			pack.JL_ActualWeight = 1m;
			pack.JL_ActualWeightUQ = "T";
			quotedBooking.Booking.JS_UnitOfWeight = "T";
			quotedBooking.Booking.JS_ActualWeight = 1m;

			if (!string.IsNullOrEmpty(sailingLoad) && !string.IsNullOrEmpty(sailingDischarge))
			{
				var sailing = factory.NewWithValidTestData<JobSailing>();
				var jobVoyage = factory.NewWithValidTestData<JobVoyage>();
				jobVoyage.JV_AirSeaRoad = "SEA";

				var voyageOrigin = factory.New<VoyageOrigin>();
				voyageOrigin.JA_RL_NKPortOfLoading = sailingLoad;
				voyageOrigin.JA_JV = jobVoyage.PK;

				var voyageDestination = factory.New<VoyageDestination>();
				voyageDestination.JB_RL_NKPortOfDischarge = sailingDischarge;
				voyageDestination.JB_JV = jobVoyage.PK;

				sailing.JX_JA = voyageOrigin.PK;
				sailing.JX_JB = voyageDestination.PK;

				((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;
			}

			return quotedBooking;
		}

		public static QuotedBooking CreateQuotedBookingWithLoadAndDischarge(BusinessObjectFactory factory)
		{
			var quotePK = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK;
			var bookingPK = QuotedBooking.CreateNewBooking(factory).PK;
			var quotedBooking = QuotedBooking.New(quotePK, bookingPK, factory);
			quotedBooking.Mode = "FCL";
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "VNVNH";
			var pack = quotedBooking.Booking.OuterPackLines.AddNew();
			pack.JL_ActualWeight = 1m;
			pack.JL_ActualWeightUQ = "T";
			return quotedBooking;
		}

		public static QuotedBooking CreateQuotedBooking(BusinessObjectFactory factory, string mode, string paymentTerms, OrgHeader client, OrgHeader consignor, OrgHeader consignee, OrgHeader carrier, string origin, string destination, decimal weight, decimal volume, QuotedBookingState quotedBookingState = QuotedBookingState.AcceptedBookingWithQuote)
		{
			var quotePK = quotedBookingState == QuotedBookingState.QuoteOnly || quotedBookingState == QuotedBookingState.AcceptedBookingWithQuote
				? QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK
			: Guid.Empty;
			var bookingPK = quotedBookingState == QuotedBookingState.BookingOnly || quotedBookingState == QuotedBookingState.AcceptedBookingWithQuote
				? QuotedBooking.CreateNewBooking(factory).PK
			: Guid.Empty;

			var quotedBooking = QuotedBooking.New(quotePK, bookingPK, factory);
			if (quotedBooking.ClientDocAddress != null && client?.MainAddress != null)
			{
				quotedBooking.ClientDocAddress.E2_OA_Address = client.MainAddress.PK;
			}

			if (consignor != null)
			{
				if (!string.IsNullOrEmpty(origin))
				{
					consignor.OH_RL_NKClosestPort = origin;
				}

				if (quotedBooking.Booking != null)
				{
					quotedBooking.Booking.ConsignorPickupAddress.E2_OA_Address = consignor.MainAddress.PK;
				}

				quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			}

			if (consignee != null)
			{
				if (!string.IsNullOrEmpty(destination))
				{
					consignee.OH_RL_NKClosestPort = destination;
				}

				if (quotedBooking.Booking != null)
				{
					quotedBooking.Booking.ConsigneeDeliveryAddress.E2_OA_Address = consignee.MainAddress.PK;
				}

				quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			}

			if (carrier != null)
			{
				quotedBooking.OH_Carrier = carrier.PK;
			}

			quotedBooking.Mode = mode;
			quotedBooking.PaymentTerms = paymentTerms;
			quotedBooking.Origin = origin;
			quotedBooking.Destination = destination;
			quotedBooking.Weight = weight;
			quotedBooking.Volume = volume;

			return quotedBooking;
		}
	}
}
