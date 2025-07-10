using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public class ForwardingBookingTransportLegDataObjectReaderTest : ShipmentDataObjectReadingHelperTest
	{
		public void TestValidSailingIsLinkedToBooking()
		{
			var today = ZDateTime.Today;

			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBB", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL1";
			var sailing = SetupSailing("AUBNE", "JPOSA", vessel, "A1234A", "SEA", carrier.PK, 1);

			Factory.SaveForTesting();

			var sailingLeg = new TransportLeg
			{
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" },
				PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" },
				VesselName = "VESSEL1",
				VoyageFlightNo = "A1234A",
				EstimatedArrival = today.AddDays(2),
				EstimatedDeparture = today.AddDays(1),
				LegType = UniversalDataBuss.DataObjects.Universal.LegType.Main
			};

			sailingLeg.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "Carrier"
			};
			sailingLeg.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
						Value = "BBB",
						CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.UnitedStates } }
				});

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
			{
				sailingLeg,
			});

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var reader = new ForwardingBookingTransportLegDataObjectReader(sailingLeg, logger, Factory, booking, shipmentDataObject);
			var bookingBO = reader.ReadIntoBusinessObject();
			AssertEquals("Sailing is found and linked", sailing.PK, bookingBO.Booking.SailingPK);
			AssertEquals(@"Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Finding Schedule for Transport Leg: Origin: AUBNE Destination: JPOSA to link to QuotedBooking
Information - Matching 'Carrier':- Matched to address '#1' on 'XVBQP68SIYXQ' by registration detail (Country/Region='US', Type='CCC', Number='BBB').
Information - Schedule with Origin: AUBNE Destination: JPOSA has been found and linked to QuotedBooking.", logger.Logs);
		}

		public void TestSaveBusinessObjectWhenSailingNotGenerated()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBB", Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_RV_NKVessel = "VESSEL1";
			voyage.JV_VoyageFlight = "A1234A";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL1";

			var today = ZDateTime.Today;
			var sailingLeg = new TransportLeg
			{
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" },
				PortOfDischarge = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" },
				VesselName = "VESSEL1",
				VoyageFlightNo = "A1234A",
				EstimatedArrival = today.AddDays(2),
				EstimatedDeparture = today.AddDays(1),
				LegType = LegType.Main
			};
			sailingLeg.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "Carrier"
			};
			sailingLeg.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
					Value = "BBB",
					CountryOfIssue = new Country { Code = Constants.CountryCodes.UnitedStates }
				}
			});

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { sailingLeg });

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			booking.Booking.JS_TransportMode = Constants.TransportModes.Sea;

			var reader = new ForwardingBookingTransportLegDataObjectReader(sailingLeg, logger, Factory, booking, shipmentDataObject);
			var bookingBO = reader.ReadIntoBusinessObject();
			AssertNull("Sailing is not generated", bookingBO.Booking.Sailing);

			Factory.SaveForTesting();

			AssertEquals(@"Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Finding Schedule for Transport Leg: Origin: AUBNE Destination: AUBNE to link to QuotedBooking
Information - Matching 'Carrier':- Matched to address '#1' on 'XVBQP68SIYXQ' by registration detail (Country/Region='US', Type='CCC', Number='BBB').
Information - An existing Schedule could not be found. The QuotedBooking will not be linked
Warning - Sailing is not generated. The Load (AUBNE) and Discharge (AUBNE) cannot be the same for SEA voyage.", logger.Logs);
		}

		public void TestCarrierUpdatesOnBooking_TransportLegLevel()
		{
			var today = ZDateTime.Today;

			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBB", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var otherCarrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "CCC", Core.Constants.CountryCodes.UnitedStates);
			otherCarrier.OH_IsShippingLine = true;
			otherCarrier.OH_IsShippingProvider = true;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL1";
			var sailing = SetupSailing("AUBNE", "JPOSA", vessel, "A1234A", "SEA", carrier.PK, 1);

			Factory.SaveForTesting();

			var sailingLeg = new TransportLeg
			{
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" },
				PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" },
				VesselName = "VESSEL1",
				VoyageFlightNo = "A1234A",
				EstimatedArrival = today.AddDays(2),
				EstimatedDeparture = today.AddDays(1),
				LegType = UniversalDataBuss.DataObjects.Universal.LegType.Main,
				Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = "Carrier"
				}
			};
			sailingLeg.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
					Value = "BBB",
					CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.UnitedStates } }
			});

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
			{
				sailingLeg,
			});

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			booking.Booking.JS_OA_BookedShippingLineAddress = otherCarrier.MainAddress.PK;

			var reader = new ForwardingBookingTransportLegDataObjectReader(sailingLeg, logger, Factory, booking, shipmentDataObject);
			var bookingBO = reader.ReadIntoBusinessObject();
			AssertEquals("Sailing is found and linked", sailing.PK, bookingBO.Booking.SailingPK);
			AssertEquals("Booking carrier has been updated", bookingBO.Booking.JS_OA_BookedShippingLineAddress, carrier.MainAddress.PK);
		}

		public void TestCarrierUpdatesOnBooking_VesselLevel()
		{
			var today = ZDateTime.Today;

			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBB", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var otherCarrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "CCC", Core.Constants.CountryCodes.UnitedStates);
			otherCarrier.OH_IsShippingLine = true;
			otherCarrier.OH_IsShippingProvider = true;

			var refVessel = Factory.NewWithValidTestData<RefVessel>();
			refVessel.RV_IsActive = true;
			refVessel.RV_OH = carrier.PK;

			var sailing = SetupSailing("AUBNE", "JPOSA", refVessel, "A1234A", "SEA", carrier.PK, 1);

			Factory.SaveForTesting();

			var sailingLeg = new TransportLeg
			{
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" },
				PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" },
				VesselName = refVessel.RV_Name,
				VoyageFlightNo = "A1234A",
				EstimatedArrival = today.AddDays(2),
				EstimatedDeparture = today.AddDays(1),
				LegType = UniversalDataBuss.DataObjects.Universal.LegType.Main
			};

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
			{
				sailingLeg,
			});

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;
			booking.Booking.JS_OA_BookedShippingLineAddress = otherCarrier.MainAddress.PK;

			var reader = new ForwardingBookingTransportLegDataObjectReader(sailingLeg, logger, Factory, booking, shipmentDataObject);
			var bookingBO = reader.ReadIntoBusinessObject();
			AssertEquals("Sailing is found and linked", sailing.PK, bookingBO.Booking.SailingPK);
			AssertEquals("Booking carrier has been updated", bookingBO.Booking.JS_OA_BookedShippingLineAddress, refVessel.Header.MainAddress.PK);
		}

		public void TestImportSailingDetails()
		{
			var today = ZDateTime.Today;

			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBB", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL1";
			var sailing = SetupSailing("AUBNE", "JPOSA", vessel, "A1234A", "SEA", carrier.PK, 1);

			Factory.SaveForTesting();

			var sailingLeg = new TransportLeg
			{
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" },
				PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" },
				VesselName = "VESSEL1",
				VoyageFlightNo = "A1234A",
				EstimatedArrival = today.AddDays(2),
				EstimatedDeparture = today.AddDays(1),
				LegType = UniversalDataBuss.DataObjects.Universal.LegType.Main,
				DepartureReference = "DepartureReference1",
				ArrivalReference = "ArrivalReference1"
			};

			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			booking.Booking.JS_TransportMode = Core.Constants.TransportModes.Sea;

			new ForwardingBookingTransportLegDataObjectReader(sailingLeg, logger, Factory, booking, shipmentDataObject).ReadIntoBusinessObject();
			AssertEquals("DepartureReference1", sailing.JX_DeparturePortRouteId);
			AssertEquals("ArrivalReference1", sailing.JX_ArrivalPortRouteId);
		}

		JobSailing SetupSailing(ZString load, ZString discharge, RefVessel vessel, ZString voyageFlight, ZString transportMode, ZGuid carrierPK, int seed)
		{
			var today = ZDateTime.Today;

			var sailing = Factory.New<JobSailing>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = today.AddDays(seed);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = today.AddDays(seed + 1);

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageFlight;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_OH_Line = carrierPK;
			voyage.JV_AirSeaRoad = transportMode;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			return sailing;
		}
	}
}
