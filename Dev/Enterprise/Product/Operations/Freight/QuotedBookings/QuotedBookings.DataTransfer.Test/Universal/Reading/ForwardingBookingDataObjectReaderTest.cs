using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using LegType = Enterprise.UniversalDataBuss.DataObjects.Universal.LegType;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalOrder = Enterprise.UniversalDataBuss.DataObjects.Universal.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public class ForwardingBookingDataObjectReaderTest : ShipmentDataObjectReadingHelperTest
	{
		public void TestCompanyTariffLevelOverride()
		{
			shipmentDataObject.CompanyTariffLevelOverride = 2;
			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			AssertEquals("CompanyTariffLevelOverride should be imported", "2", bookingBO.CompanyTariffLevel);
		}

		public void TestShipmentStatusForUniversalXML()
		{
			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();
			AssertEquals(ShipmentStatusList.Codes.Booked, bookingBO.ShipmentStatus);
		}

		public void TestContainerModeOverride()
		{
			const string containerModeOverride = Enterprise.Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR;
			shipmentDataObject.HBLContainerPackModeOverride = containerModeOverride;

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals("ContainerPackModeOverride", containerModeOverride, bookingBO.ContainerPackModeOverride);
		}

		public void TestBookingPartyDocumentaryAddress()
		{
			var bookingPartyAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.BookingPartyDocumentaryAddress));
			var orgAddress = new OrganisationDataObjectReader(bookingPartyAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(bookingPartyAddress);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			AssertNotNull(shipmentBO.BookingParty.MainAddress);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.BookingParty.MainAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'BookingPartyDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});
			#endregion
		}

		public void TestDoesNotImportSailing()
		{
			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			Factory.SaveForTesting();
			AssertEquals("The sailing should not be created", ZGuid.Empty, shipmentBO.SailingPK);
		}

		public void TestTransportLegCollectionSailingLinked()
		{
			var today = ZDateTime.Today;
			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBB", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var sailing = SetupSailing("AUBNE", "JPOSA", "VESSEL1", "A1234A", "SEA", carrier.PK, 1);

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
				LegType = LegType.Main,

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
				sailingLeg
			});

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			Factory.SaveForTesting();
			AssertEquals("Sailing should be linked", sailing.PK, shipmentBO.SailingPK);
			AssertEquals(@"Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Finding Schedule for Transport Leg: Origin: AUBNE Destination: JPOSA to link to QuotedBooking
Information - Matching 'Carrier':- Matched to address '#1' on 'XVBQP68SIYXQ' by registration detail (Country/Region='US', Type='CCC', Number='BBB').
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - Schedule with Origin: AUBNE Destination: JPOSA has been found and linked to QuotedBooking.
Information - Added Quick Booking from UniversalShipment.", Logger.Logs);
		}

		public void TestTransportLegCollectionSailingIsLinked_MultipleLegs()
		{
			var today = ZDateTime.Today;

			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBB", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var sailing1 = SetupSailing("AUBNE", "JPOSA", "VESSEL1", "A1234A", "SEA", carrier.PK, 1);
			SetupSailing("NZAKL", "USLAX", "VESSEL2", "B1234B", "SEA", carrier.PK, 3);

			Factory.SaveForTesting();

			var sailingLeg1 = new TransportLeg
			{
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" },
				PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" },
				VesselName = "VESSEL1",
				VoyageFlightNo = "A1234A",
				EstimatedArrival = today.AddDays(2),
				EstimatedDeparture = today.AddDays(1),
				LegType = LegType.Main
			};

			sailingLeg1.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "Carrier"
			};
			sailingLeg1.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
					Value = "BBB",
					CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.UnitedStates } }
			});

			var sailingLeg2 = new TransportLeg
			{
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "NZAKL", Name = "Auckland" },
				PortOfDischarge = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" },
				VesselName = "VESSEL2",
				VoyageFlightNo = "B1234B",
				EstimatedArrival = today.AddDays(4),
				EstimatedDeparture = today.AddDays(3),
				LegType = LegType.Other
			};

			sailingLeg2.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "Carrier"
			};
			sailingLeg2.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
					Value = "BBB",
					CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.UnitedStates } }
			});

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
			{
				sailingLeg1,
				sailingLeg2
			});

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			Factory.SaveForTesting();
			AssertEquals("Main leg should have been linked", sailing1.PK, shipmentBO.SailingPK);
			AssertEquals(@"Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - There are multiple compatible legs. The Main leg has been selected to try link to QuotedBooking.
Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Finding Schedule for Transport Leg: Origin: AUBNE Destination: JPOSA to link to QuotedBooking
Information - Matching 'Carrier':- Matched to address '#1' on 'XVBQP68SIYXQ' by registration detail (Country/Region='US', Type='CCC', Number='BBB').
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - Schedule with Origin: AUBNE Destination: JPOSA has been found and linked to QuotedBooking.
Information - Added Quick Booking from UniversalShipment.", Logger.Logs);
		}

		public void TestIncompatibleLegIsNotLinked()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBB", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			SetupSailing("AUBNE", "JPOSA", "VESSEL1", "A1234A", "AIR", carrier.PK, 1);
			var today = ZDateTime.Today;

			var sailingLeg = new TransportLeg
			{
				TransportMode = TransportMode.Air,
				PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" },
				PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" },
				VesselName = "VESSEL1",
				VoyageFlightNo = "A1234A",
				EstimatedArrival = today.AddDays(2),
				EstimatedDeparture = today.AddDays(1),
				LegType = LegType.Main
			};

			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "SEA", Description = "Sea" };

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
			{
				sailingLeg
			});

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertEquals("Should not be linked - incompatible transport mode", ZGuid.Empty, bookingBO.Booking.SailingPK);
			AssertEquals(@"Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No leg is compatible to link to QuotedBooking.
Information - Added Quick Booking from UniversalShipment.", Logger.Logs);
		}

		public void TestTransportLegCollectionNoScheduleFoundLoadAndDischargePopulated()
		{
			var today = ZDateTime.Today;

			var sailingLeg = new TransportLeg
			{
				TransportMode = TransportMode.Sea,
				PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" },
				PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" },
				VesselName = "VESSEL1",
				VoyageFlightNo = "A1234A",
				EstimatedArrival = today.AddDays(2),
				EstimatedDeparture = today.AddDays(1),
				LegType = LegType.Main
			};

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
			{
				sailingLeg
			});

			shipmentDataObject.PortOfLoading = new UNLOCO() { Code = "", Name = "" };
			shipmentDataObject.PortOfDischarge = new UNLOCO() { Code = "", Name = "" };

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertEquals("Should not be linked - sailing does not exist", ZGuid.Empty, bookingBO.Booking.SailingPK);
			AssertEquals("QuotedBooking load populated from transport leg details", bookingBO.LoadPort, "AUBNE");
			AssertEquals("QuotedBooking discharge populated from transport leg details", bookingBO.DischargePort, "JPOSA");
			AssertEquals(@"Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Finding Schedule for Transport Leg: Origin: AUBNE Destination: JPOSA to link to QuotedBooking
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The QuotedBooking will not be linked
Information - Added Quick Booking from UniversalShipment.", Logger.Logs);
		}

		JobSailing SetupSailing(ZString load, ZString discharge, ZString vessel, ZString voyageFlight, ZString transportMode, ZGuid carrierPK, int seed)
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
			voyage.JV_RV_NKVessel = vessel;
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

		public void TestPickupLocalTransportAddress_OldKey()
		{
			var pickupLocalTransportAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.LocalCartagePickupFromAddress));
			var orgAddress = new OrganisationDataObjectReader(pickupLocalTransportAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(pickupLocalTransportAddress);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			AssertNotNull(shipmentBO.DocsAndCartage.PickupCartageCoAddr);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.DocsAndCartage.PickupCartageCoAddr);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'LocalCartagePickupFromAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'LocalCartagePickupFromAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestPickupLocalTransportAddress()
		{
			var pickupLocalTransportAddress = GetNewAddressData_INTHEMSYD(AddressTypes.PickupLocalCartage);
			var orgAddress = new OrganisationDataObjectReader(pickupLocalTransportAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(pickupLocalTransportAddress);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			AssertNotNull(shipmentBO.DocsAndCartage.PickupCartageCoAddr);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.DocsAndCartage.PickupCartageCoAddr);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'PickupLocalCartage':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'PickupLocalCartage':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestDeliveryAgentAddress()
		{
			var deliveryAgentAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.DeliveryAgent));
			var orgAddress = new OrganisationDataObjectReader(deliveryAgentAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(deliveryAgentAddress);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			AssertNotNull(shipmentBO.DeliveryAgent);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.DeliveryAgent.MainAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'DeliveryAgent':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'DeliveryAgent':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestPickupCFSAddress()
		{
			var pickupCFSAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.DepartureCFSAddress));
			var orgAddress = new OrganisationDataObjectReader(pickupCFSAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(pickupCFSAddress);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			AssertNotNull(shipmentBO.ExportReceivingDepot);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.ExportReceivingDepot);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'DepartureCFSAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'DepartureCFSAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestExportBrokerAddress()
		{
			var exportBrokerAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ExportBroker));
			var orgAddress = new OrganisationDataObjectReader(exportBrokerAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				exportBrokerAddress
			});

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();
			var shipmentBO = bookingBO.Booking;

			AssertNotNull(bookingBO);
			AssertNotNull(shipmentBO.ExportBroker);

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.ExportBroker.MainAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'ExportBroker':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'ExportBroker':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestImportBrokerAddress()
		{
			var importBrokerAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ImportBroker));
			var orgAddress = new OrganisationDataObjectReader(importBrokerAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				importBrokerAddress
			});

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();
			var shipmentBO = bookingBO.Booking;

			AssertNotNull(bookingBO);
			AssertNotNull(shipmentBO.ImportBroker);

			CombineAssertions(delegate
			{
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.ImportBroker.MainAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'ImportBroker':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'ImportBroker':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestLoadingBookingThroughAdditionalReferences()
		{
			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var cusEntryNumber1 = shipmentBOToLoad.Numbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "CE00001";
			cusEntryNumber1.CE_EntryType = "AMS";
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber2 = shipmentBOToLoad.Numbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "CE00002";
			cusEntryNumber2.CE_EntryType = "COC";
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber3 = shipmentBOToLoad.Numbers.AddNew();
			cusEntryNumber3.CE_EntryNum = "CE00003";
			cusEntryNumber3.CE_EntryType = "UBR";
			cusEntryNumber3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var shipmentPK = shipmentBOToLoad.PK;

			Factory.SaveForTesting();

			Thread.Sleep(200);

			var dummyBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);

			var dummyShipmentBOToLoad = dummyBooking.Booking;
			dummyShipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var dummyCusEntryNumber1 = dummyShipmentBOToLoad.Numbers.AddNew();
			dummyCusEntryNumber1.CE_EntryNum = "CE00001";
			dummyCusEntryNumber1.CE_EntryType = "AMS";
			dummyCusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var dummyCusEntryNumber2 = dummyShipmentBOToLoad.Numbers.AddNew();
			dummyCusEntryNumber2.CE_EntryNum = "CE00002";
			dummyCusEntryNumber2.CE_EntryType = "COC";
			dummyCusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var additionalReference1 = new AdditionalReference { Type = new EntryType { Code = "AMS", Description = "AMS Number" }, ReferenceNumber = "CE00001" };
			var additionalReference2 = new AdditionalReference { Type = new EntryType { Code = "COC", Description = "Customs Office Code (Override)" }, ReferenceNumber = "CE00002" };
			var additionalReference3 = new AdditionalReference { Type = new EntryType { Code = "UBR", Description = "Under Bond Approval Reference Number" }, ReferenceNumber = "CE00003" };

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReference1);
			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReference2);
			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReference3);
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.CodesMappedToTarget = true;

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				var shipmentBO = bookingBO.Booking;
				AssertEquals("shipmentBO.PK", shipmentPK, shipmentBO.PK);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Quick Booking - Booking (S00001000) from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLoadsBookingFromHouseBill()
		{
			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var shipmentPk = shipmentBOToLoad.PK;

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.GoodsDescription = "I SWEAR I'VE CHANGED!";

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.PK", shipmentPk, shipmentBO.PK);
				AssertEquals("shipmentBO.JS_GoodsDescription", "I SWEAR I'VE CHANGED!", shipmentBO.JS_GoodsDescription);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Updated Quick Booking - Booking (S00001000) from UniversalShipment.				".Trim(), Logger.Logs);
			});
		}

		public void TestLoadsBestMatchedBooking()
		{
			shipmentDataObject.CFSReference = "NO CFS FOR YOU";
			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			Factory.SaveForTesting();

			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_BookingReference = "BOOK ME";
			shipmentBOToLoad.JS_InterimReceipt = "IR Text";
			shipmentBOToLoad.JS_CFSReference = "CFS Book Ref";
			shipmentBOToLoad.JS_RL_NKOrigin = "NZDUD";
			shipmentBOToLoad.JS_RL_NKDestination = "AUBDG";

			var shipmentPk = shipmentBOToLoad.PK;
			Factory.SaveForTesting();

			var newLogger = new TestErrorLogger();
			shipmentDataObject.GoodsDescription = "I SWEAR I'VE CHANGED!";
			shipmentDataObject.CFSReference = "CFS Book Ref";
			reader = new ForwardingBookingDataObjectReader(shipmentDataObject, newLogger, Factory);
			bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.PK", shipmentPk, shipmentBO.PK);
				AssertEquals("shipmentBO.JS_GoodsDescription", "I SWEAR I'VE CHANGED!", shipmentBO.JS_GoodsDescription);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Updated Quick Booking - Booking (S00001001) from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		public void TestLoadsLatestBookingIfMoreThanOneMatch()
		{
			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			Factory.SaveForTesting();

			Thread.Sleep(200);

			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_BookingReference = "BOOK ME";
			shipmentBOToLoad.JS_InterimReceipt = "IR Text";
			shipmentBOToLoad.JS_CFSReference = "CFS Book Ref";
			shipmentBOToLoad.JS_RL_NKOrigin = "NZDUD";
			shipmentBOToLoad.JS_RL_NKDestination = "AUBDG";

			var shipmentPk = shipmentBOToLoad.PK;

			Factory.SaveForTesting();

			var newLogger = new TestErrorLogger();
			shipmentDataObject.GoodsDescription = "I SWEAR I'VE CHANGED!";
			reader = new ForwardingBookingDataObjectReader(shipmentDataObject, newLogger, Factory);
			bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.PK", shipmentPk, shipmentBO.PK);
				AssertEquals("shipmentBO.JS_GoodsDescription", "I SWEAR I'VE CHANGED!", shipmentBO.JS_GoodsDescription);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Updated Quick Booking - Booking (S00001001) from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		public void TestLoadOfBookings()
		{
			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentPK = bookingBO.Booking.PK;

			Factory.SaveForTesting();

			var newLogger = new TestErrorLogger();
			shipmentDataObject.GoodsDescription = "I SWEAR I'VE CHANGED!";
			reader = new ForwardingBookingDataObjectReader(shipmentDataObject, newLogger, Factory);
			bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.PK", shipmentPK, shipmentBO.PK);
				AssertEquals("shipmentBO.JS_GoodsDescription", "I SWEAR I'VE CHANGED!", shipmentBO.JS_GoodsDescription);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Updated Quick Booking - Booking (S00001000) from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		public void TestWorkflowCustomFieldsOnBookingAreImported()
		{
			#region Setup Template

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "QBK";
			processTaskTemplate.P0_IsActive = true;
			processTaskTemplate.P0_SubType2 = "QBN";

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnBool2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool2.XC_Name = "Flag This!";
			genCustomColumnBool2.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool2);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			Factory.SaveForTesting();

			#endregion

			shipmentDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString("HELLO")));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Customs are customary", new ZString("GOODBYE")));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(2011, 1, 1)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Last Date", new ZDateTime(2011, 1, 2)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal(0.3)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("+ 1 point zero", new ZDecimal(1.3)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", ZBool.True));
			var incorrectCustomField = new CustomizedField { Key = "Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean };
			shipmentDataObject.CustomizedFieldCollection.Add(incorrectCustomField);
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integer Mate", new ZInt(42)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integraler", ZInt.Zero));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Bogus Custom Field", new ZString("I am BOGUS")));

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var customFields = bookingBO.GetUserDefinedValues();
				var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();

				Assert("Custom Field 1 not found", customFieldsString.Contains("Deci Deca - 0.3"));
				Assert("Custom Field 2 not found", customFieldsString.Contains("First Date - 01-Jan-11 00:00:00"));
				Assert("Custom Field 3 not found", customFieldsString.Contains("Flagger - Y"));
				Assert("Custom Field 4 not found", customFieldsString.Contains("Integer Mate - 42"));
				Assert("Custom Field 5 not found", customFieldsString.Contains("Textual context - HELLO"));

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestLocalProcessing()
		{
			var orgAddress = GetNewAddressData_INTHEMSYD("FOO");
			new OrganisationDataObjectReader(orgAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var localProcessing = CreateLocalProcessing(orgAddress);

			var additionalService = new AdditionalService()
			{
				ServiceCode = new CodeDescriptionPair { Code = "TAI", Description = "Tailgate" },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = orgAddress,
			};

			localProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			localProcessing.AdditionalServiceCollection.Add(additionalService);

			shipmentDataObject.LocalProcessing = localProcessing;

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			CombineAssertions(delegate
			{
				var docsBO = bookingBO.Booking.DocsAndCartage;

				AssertEquals("docsBO.JP_InsuranceRequired", true, docsBO.JP_InsuranceRequired);
				AssertEquals("docsBO.JP_FCLPickupEquipmentNeeded", "TRL", docsBO.JP_FCLPickupEquipmentNeeded);
				AssertEquals("docsBO.JP_FCLDeliveryEquipmentNeeded", "WUP", docsBO.JP_FCLDeliveryEquipmentNeeded);
				AssertEquals("docsBO.JP_EstimatedDelivery", new ZDateTime(2011, 1, 1), docsBO.JP_EstimatedDelivery);
				AssertEquals("docsBO.JP_DeliveryRequiredBy", new ZDateTime(2011, 1, 2), docsBO.JP_DeliveryRequiredBy);
				AssertEquals("docsBO.JP_EstimatedPickup", new ZDateTime(2011, 1, 3), docsBO.JP_EstimatedPickup);
				AssertEquals("docsBO.JP_PickupRequiredBy", new ZDateTime(2011, 1, 4), docsBO.JP_PickupRequiredBy);
				AssertEquals("docsBO.JP_OrderItemsAsString", "REFEREME,DONTREFEREME", docsBO.JP_OrderItemsAsString);

				AssertEquals("docsBO.Services.Count", 1, docsBO.Services.Count);

				var service = docsBO.Services[0];
				AssertEquals("service.ES_Booked", new ZDateTime(2011, 6, 1), service.ES_Booked);
				AssertEquals("service.ES_Completed", new ZDateTime(2011, 6, 2), service.ES_Completed);
				AssertEquals("service.ES_ServiceCode", "TAI", service.ES_ServiceCode);
				AssertAddressContentMatches_INTHEMSYD(service.Contractor.MainAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'FOO':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestLocalProcessing_AdditionalServicesComplete()
		{
			var newBookingBO = PrepareAndReadBookingObject(addCollectionContent: true, CollectionContent.Complete);

			AssertEquals(2, newBookingBO.Booking.DocsAndCartage.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, newBookingBO.Booking.DocsAndCartage.Services.Cast<JobService>().ToArray()[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, newBookingBO.Booking.DocsAndCartage.Services.Cast<JobService>().ToArray()[1].ES_ServiceCode);
		}

		public void TestLocalProcessing_AdditionalServicesPartial()
		{
			var newBookingBO = PrepareAndReadBookingObject(addCollectionContent: true, CollectionContent.Partial);

			AssertEquals(3, newBookingBO.Booking.DocsAndCartage.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, newBookingBO.Booking.DocsAndCartage.Services.Cast<JobService>().ToArray()[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Cleaning, newBookingBO.Booking.DocsAndCartage.Services.Cast<JobService>().ToArray()[1].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, newBookingBO.Booking.DocsAndCartage.Services.Cast<JobService>().ToArray()[2].ES_ServiceCode);
		}

		public void TestLocalProcessing_AdditionalServicesWithoutContentType_ShouldWorkLikePartial()
		{
			var newBookingBO = PrepareAndReadBookingObject(addCollectionContent: false);

			AssertEquals(3, newBookingBO.Booking.DocsAndCartage.Services.Count);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Fumigation, newBookingBO.Booking.DocsAndCartage.Services.Cast<JobService>().ToArray()[0].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Cleaning, newBookingBO.Booking.DocsAndCartage.Services.Cast<JobService>().ToArray()[1].ES_ServiceCode);
			AssertEquals(Core.Constants.FreightServiceType.Codes.Washing, newBookingBO.Booking.DocsAndCartage.Services.Cast<JobService>().ToArray()[2].ES_ServiceCode);
		}

		LocalProcessing CreateLocalProcessing(OrganizationAddress orgAddress)
		{
			var localProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);

			localProcessing.InsuranceRequired = true;
			localProcessing.FCLPickupEquipmentNeeded = new CodeDescriptionPair { Code = "TRL", Description = "Drop Trailer" };
			localProcessing.FCLDeliveryEquipmentNeeded = new CodeDescriptionPair { Code = "WUP", Description = "Wait for Pack/Unpack" };
			localProcessing.EstimatedDelivery = new ZDateTime(2011, 1, 1);
			localProcessing.DeliveryRequiredBy = new ZDateTime(2011, 1, 2);
			localProcessing.EstimatedPickup = new ZDateTime(2011, 1, 3);
			localProcessing.PickupRequiredBy = new ZDateTime(2011, 1, 4);

			var orderNumber1 = new OrderNumber { Sequence = 2, OrderReference = "REFEREME" };
			var orderNumber2 = new OrderNumber { Sequence = 1, OrderReference = "DONTREFEREME" };
			localProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>());
			localProcessing.OrderNumberCollection.Add(orderNumber1);
			localProcessing.OrderNumberCollection.Add(orderNumber2);

			return localProcessing;
		}

		QuotedBooking PrepareAndReadBookingObject(bool addCollectionContent, CollectionContent collectionContent = CollectionContent.Complete)
		{
			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			bookingBO.Booking.DocsAndCartage.Services.RemoveAndDeleteAll();
			var fumService = bookingBO.Booking.DocsAndCartage.Services.AddNew();
			fumService.ShouldPopulateServiceId = false;
			fumService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			var clnService = bookingBO.Booking.DocsAndCartage.Services.AddNew();
			clnService.ShouldPopulateServiceId = false;
			clnService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Cleaning;

			Factory.SaveForTesting();

			var fumAdditionalService = new AdditionalService
			{
				ServiceCode = new CodeDescriptionPair { Code = Core.Constants.FreightServiceType.Codes.Fumigation, Description = Core.Constants.FreightServiceType.Descriptions.Fumigation },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = null,
			};

			var wshAdditionalService = new AdditionalService
			{
				ServiceCode = new CodeDescriptionPair { Code = Core.Constants.FreightServiceType.Codes.Washing, Description = Core.Constants.FreightServiceType.Descriptions.Washing },
				Booked = new ZDateTime(2011, 6, 1),
				Completed = new ZDateTime(2011, 6, 2),
				Contractor = null,
			};

			var additionalServicesDataObject = new DataObjectList<AdditionalService>();
			additionalServicesDataObject.Add(fumAdditionalService);
			additionalServicesDataObject.Add(wshAdditionalService);

			if (addCollectionContent)
			{
				additionalServicesDataObject.Content = collectionContent;
			}

			shipmentDataObject.LocalProcessing = CreateLocalProcessing(GetNewAddressData_INTHEMSYD("FOO"));
			shipmentDataObject.LocalProcessing.SetAdditionalServiceCollection(() => additionalServicesDataObject);

			var newLogger = new TestErrorLogger();
			reader = new ForwardingBookingDataObjectReader(shipmentDataObject, newLogger, Factory);
			var newBookingBO = reader.ReadIntoBusinessObject();
			return newBookingBO;
		}

		public void TestCusEntryNumbers()
		{
			shipmentDataObject.SetEntryNumberCollection(() => new List<EntryNumber>());

			var entryNumberDataObject = SetupEntryNumber();

			shipmentDataObject.EntryNumberCollection.Add(entryNumberDataObject);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.CusEntryNumbersForAllCountries.Count", 1, shipmentBO.CusEntryNumbersForAllCountries.Count);

				var entryNumberBO = shipmentBO.CusEntryNumbersForAllCountries[0];
				AssertContents(entryNumberBO, shipmentBO);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestBasicShipmentLevelFieldMappings()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				shipmentDataObject.WayBillNumber = "MYHOUSE";
				shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

				shipmentDataObject.SetDateCollection(() => new List<Date>());
				shipmentDataObject.DateCollection.Add(Date.New(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2010, 1, 1)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.Received, ZBool.False, new ZDateTime(2010, 1, 2)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.Departure, ZBool.True, new ZDateTime(2010, 1, 3)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2010, 1, 4)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.ClientRequestedETA, ZBool.False, new ZDateTime(2010, 1, 5)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.DeliveryDueDate, ZBool.True, new ZDateTime(2016, 4, 25)));

				shipmentDataObject.ReleaseType = new CodeDescriptionPair() { Code = "EBL", Description = "Express Bill Of Lading" };
				shipmentDataObject.HBLAWBChargesDisplay = new CodeDescriptionPair() { Code = "SHW", Description = "Show Collect Charges" };
				shipmentDataObject.ShippedOnBoard = new CodeDescriptionPair() { Code = "SHP", Description = "Shipped" };
				shipmentDataObject.CarrierServiceLevel = new ServiceLevel() { Code = "STD", Description = "Standard" };

				var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
				var bookingBO = reader.ReadIntoBusinessObject();

				Factory.SaveForTesting();

				AssertNotNull(bookingBO);
				var shipmentBO = bookingBO.Booking;

				#region Check Contents of shipment Business Object

				var logs = shipmentBO.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.DeliveryDateUpdated);
				var originalLog = logs.FirstOrDefault(log => log.Parameters[Params.Type] == "Original");
				var revisedLog = logs.FirstOrDefault(log => log.Parameters[Params.Type] == "Revised");

				CombineAssertions(() =>
				{
					AssertNotNull(originalLog);
					AssertNotNull(originalLog.Parameters[Params.New]);
					AssertEquals("Changed by Data Import", "Changed by Data Import", originalLog.Parameters[Params.Reason]);
				});

				AssertNull(revisedLog);

				CombineAssertions(delegate
				{
					AssertContents(bookingBO);
					AssertEquals("shipmentBO.JS_A_BKD", new ZDateTime(2010, 1, 1), shipmentBO.JS_A_BKD);
					AssertEquals("shipmentBO.JS_A_RCV", new ZDateTime(2010, 1, 2), shipmentBO.JS_A_RCV);
					AssertEquals("shipmentBO.JS_E_DEP", new ZDateTime(2010, 1, 3), shipmentBO.JS_E_DEP);
					AssertEquals("shipmentBO.JS_E_ARV", new ZDateTime(2010, 1, 4), shipmentBO.JS_E_ARV);
					AssertEquals("shipmentBO.JS_ClientRequestedETA", new ZDateTime(2010, 1, 5), shipmentBO.JS_ClientRequestedETA);
					AssertEquals("shipmentBO.JS_ReleaseType", "EBL", shipmentBO.JS_ReleaseType);
					AssertEquals("shipmentBO.JS_HBLAWBChargesDisplay", "SHW", shipmentBO.JS_HBLAWBChargesDisplay);
					AssertEquals("shipmentBO.JS_ShippedOnBoard", "SHP", shipmentBO.JS_ShippedOnBoard);
					AssertEquals("shipmentBO.JS_PL_NKCarrierServiceLevel", "STD", shipmentBO.JS_PL_NKCarrierServiceLevel);
					AssertEquals("shipmentBO.JS_DeliveryDueDate", new ZDateTime(2016, 4, 25), shipmentBO.JS_DeliveryDueDate);
					AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
				});

				#endregion
			}
		}

		public void TestShipmentLevelFieldMappingsWithNVOCCRecipientRole()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			shipmentDataObject.AgentsReference = "BOOKS";
			shipmentDataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
			shipmentDataObject.CoLoadBookingConfirmationReference = "S00005000";

			Factory.SaveForTesting();

			var dataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			dataContext.RecipientRoleCollection = new List<RecipientRole>();
			dataContext.RecipientRoleCollection.Add(new RecipientRole { Code = RecipientRoleType.NVO, ServiceCode = ServiceCodeType.BRQ });
			shipmentDataObject.DataContext = dataContext;

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var bookingPartyOrgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
				Address1 = "Booking Party Address 1",
				OrganizationCode = "BKGPARTY"
			};
			shipmentDataObject.OrganizationAddressCollection.Add(bookingPartyOrgAddress);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			#region Check Contents of shipment Business Object

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.JS_BookingReference", "BOOKS", shipmentBO.JS_BookingReference);
				AssertEquals("shipmentBO.JS_ShipmentStatus", ShipmentStatusList.Codes.ElectronicBooking, shipmentBO.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestShipmentStatusDefaultValueWithNVOCCRecipientRole()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			shipmentDataObject.AgentsReference = "BOOKS";
			shipmentDataObject.ShipmentStatus = null;
			shipmentDataObject.CoLoadBookingConfirmationReference = "S00005000";

			Factory.SaveForTesting();

			var dataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			dataContext.RecipientRoleCollection = new List<RecipientRole>();
			dataContext.RecipientRoleCollection.Add(new RecipientRole { Code = RecipientRoleType.NVO, ServiceCode = ServiceCodeType.BRQ });
			shipmentDataObject.DataContext = dataContext;

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var bookingPartyOrgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
				Address1 = "Booking Party Address 1",
				OrganizationCode = "BKGPARTY"
			};
			shipmentDataObject.OrganizationAddressCollection.Add(bookingPartyOrgAddress);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			#region Check Contents of shipment Business Object

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.JS_BookingReference", "BOOKS", shipmentBO.JS_BookingReference);
				AssertEquals("shipmentBO.JS_ShipmentStatus with default value", ShipmentStatusList.Codes.ElectronicBooking, shipmentBO.JS_ShipmentStatus);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestShipmentLevelFieldMappingsWithoutNVOCCRecipientRole()
		{
			shipmentDataObject.BookingConfirmationReference = ZString.Empty;
			shipmentDataObject.AgentsReference = "BOOKS";
			shipmentDataObject.CoLoadBookingConfirmationReference = "S00005000";

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			#region Check Contents of shipment Business Object

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.JS_BookingReference", ZString.Empty, shipmentBO.JS_BookingReference);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestGetVesselNameFallingBackToLloydNumberLookup()
		{
			shipmentDataObject.VesselName = null;
			shipmentDataObject.LloydsIMO = "8907993";

			TestBasicShipmentLevelFieldMappings();
		}

		public void TestWithFMCTariffID()
		{
			shipmentDataObject.FMCTariffID = "abcd";

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			AssertEquals("booking.FMCTariffID", (ZString)"abcd", bookingBO.FMCTariffID);
		}

		public void TestWithInspectionType()
		{
			shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "APP" };
			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			AssertEquals("Booking.JS_InspectionTypeCode", "UNK", bookingBO.Booking.JS_InspectionTypeCode);

			shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "PHS" };
			reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			AssertEquals("Booking.JS_InspectionTypeCode", "PHS", bookingBO.Booking.JS_InspectionTypeCode);
		}

		public void TestWithCommodity()
		{
			shipmentDataObject.RateCommodity = new Commodity { Code = "COM", Description = "Common" };

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			AssertEquals("booking.Commodity", (ZString)"COM", bookingBO.Commodity);
		}

		public void TestWithPackLines()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var packingLineDataObject = SetupPackingLine(Factory);

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			shipmentDataObject.PackingLineCollection.Add(packingLineDataObject);
			SetupCFSAddresses(shipmentDataObject);

			Factory.SaveForTesting();

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			#region Check Contents of shipment Business object

			AssertEquals("shipmentBO.OuterPackLines.Count", 1, shipmentBO.OuterPackLines.Count);

			CombineAssertions(delegate
			{
				AssertContents(bookingBO);
				AssertContents(shipmentBO.OuterPackLines[0]);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'DepartureCFSAddress':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - Matching 'ArrivalCFSAddress':- Matched to 'OH1' by code, address 'AD1' by short code.
Warning - Unknown Address Type [ArrivalCFSAddress] found. Job Document Address not imported.
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Matching 'LastKnownCFSFacility':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - There is no contact found in database with name 'Telepuzik' and phone '' for dangerous goods substance code '3000c' (IMO Class = ''). Make sure that name and phone is not empty. If not create contact first.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - There is no substance with code '3001' found. Please use standard dangerous goods substance code.
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestWithContainers()
		{
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var containerDataObject = new Container
			{
				AirVentFlow = 12.3m,
				Commodity = new Commodity { Code = "COM", Description = "Common" },
				ContainerNumber = "C10001000",
				ContainerType = new ContainerType { Code = "ZW0W", Description = "Container Type WOW!!", ISOCode = "21G5" },
				DepartureEstimatedPickup = new ZDateTime(2011, 1, 1),
				DepartureSlotDateTime = new ZDateTime(2011, 1, 2),
				DepartureSlotReference = "REF REF",
				EmptyRequired = new ZDateTime(2011, 1, 3),
				FCL_LCL_AIR = new ContainerMode { Code = "LCL", Description = "Less Container Load" },
				ReleaseNum = "R10003333",
				SetPointTemp = 14.1m,
				TempRecorderSerialNo = "SSTT1111",
			};

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			shipmentDataObject.ContainerCollection.Add(containerDataObject);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			#region Check Contents of shipment Business Object

			CombineAssertions(delegate
			{
				AssertContents(bookingBO);
				AssertEquals("bookingBO.QuotedBookingContainers.Count", 1, bookingBO.QuotedBookingContainers.Count);

				var containerBO = bookingBO.QuotedBookingContainers[0];
				AssertEquals("containerBO.JC_AirVentFlow", 12.3m, containerBO.JC_AirVentFlow);
				AssertEquals("containerBO.JC_RH_NKContainerCommodityCode", "COM", containerBO.JC_RH_NKContainerCommodityCode);
				AssertEquals("containerBO.JC_ContainerNum", "C10001000", containerBO.JC_ContainerNum);
				AssertNull("containerBO.RefContainer", containerBO.RefContainer);
				AssertEquals("containerBO.JC_DepartureEstimatedPickup", new ZDateTime(2011, 1, 1), containerBO.JC_DepartureEstimatedPickup);
				AssertEquals("containerBO.JC_DepartureSlotDateTime", new ZDateTime(2011, 1, 2), containerBO.JC_DepartureSlotDateTime);
				AssertEquals("containerBO.JC_DepartureSlotReference", "REF REF", containerBO.JC_DepartureSlotReference);
				AssertEquals("containerBO.JC_EmptyRequired", new ZDateTime(2011, 1, 3), containerBO.JC_EmptyRequired);
				AssertEquals("containerBO.JC_ContainerMode", "LCL", containerBO.JC_ContainerMode);
				AssertEquals("containerBO.JC_ReleaseNum", "R10003333", containerBO.JC_ReleaseNum);
				AssertEquals("containerBO.JC_SetPointTemp", 14.1m, containerBO.JC_SetPointTemp);
				AssertEquals("containerBO.JC_TempRecorderSerialNo", "SSTT1111", containerBO.JC_TempRecorderSerialNo);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestOrdersPopulated()
		{
			var orderData = GetNewOrderDataObject();
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var bookingBO = reader.ReadIntoBusinessObject();
			var shipmentBO = bookingBO.Booking;

			AssertNotNull(shipmentBO);
			AssertEquals(1, shipmentBO.AttachedOrders.Count);
			var order = shipmentBO.AttachedOrders[0];
			AssertEquals("ORDER ME", order.JD_OrderNumber);
			AssertEquals(new ZByte(2), order.JD_OrderNumberSplit);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		public void TestEmptyContainerNumber()
		{
			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var shipmentPk = shipmentBOToLoad.PK;

			var containerBOToLoad = Factory.New<ForwardingContainer>();
			containerBOToLoad.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad.JC_ReleaseNum = "R11111111";
			containerBOToLoad.JC_ContainerCount = 1;

			bookingBOToLoad.QuotedBookingContainers.Add(containerBOToLoad);

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var containerDataObject1 = new Container
			{
				ContainerCount = 3,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10003333",
			};

			var containerDataObject2 = new Container
			{
				ContainerCount = 5,
				ContainerType = new ContainerType { Code = "40HC", Description = "40HC Container", ISOCode = "40HC" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10005555",
			};

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			shipmentDataObject.ContainerCollection.Add(containerDataObject1);
			shipmentDataObject.ContainerCollection.Add(containerDataObject2);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertContents(bookingBO);
				AssertEquals("shipmentBO.PK", shipmentPk, shipmentBO.PK);
				AssertEquals("bookingBO.QuotedBookingContainers.Count", 2, bookingBO.QuotedBookingContainers.Count);

				var containerBO1 = bookingBO.QuotedBookingContainers[0];
				AssertEquals("containerBO1.JC_ContainerNum", "", containerBO1.JC_ContainerNum);
				AssertEquals("containerBO1.JC_ContainerCount", new ZShort(3), containerBO1.JC_ContainerCount);
				AssertNotNull("containerBO1.RefContainer", containerBO1.RefContainer);
				AssertEquals("containerBO1.RefContainer.RC_Code", "20GP", containerBO1.RefContainer.RC_Code);
				AssertEquals("containerBO1.JC_ContainerMode", "FCL", containerBO1.JC_ContainerMode);
				AssertEquals("containerBO1.JC_ReleaseNum", "R10003333", containerBO1.JC_ReleaseNum);

				var containerBO2 = bookingBO.QuotedBookingContainers[1];
				AssertEquals("containerBO2.JC_ContainerNum", "", containerBO2.JC_ContainerNum);
				AssertEquals("containerBO2.JC_ContainerCount", new ZShort(5), containerBO2.JC_ContainerCount);
				AssertNotNull("containerBO2.RefContainer", containerBO2.RefContainer);
				AssertEquals("containerBO2.RefContainer.RC_Code", "40HC", containerBO2.RefContainer.RC_Code);
				AssertEquals("containerBO2.JC_ContainerMode", "FCL", containerBO2.JC_ContainerMode);
				AssertEquals("containerBO2.JC_ReleaseNum", "R10005555", containerBO2.JC_ReleaseNum);

				AssertContains("20GP container should be successfully matched", "Information - Successfully loaded matching ForwardingContainer.", Logger.Logs);
				AssertContains("40HC container should be added", "Information - No matching ForwardingContainer found, creating new ForwardingContainer.", Logger.Logs);
			});
		}

		public void TestEmptyContainerNumber_MultipleMatchingContainersInXML()
		{
			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var shipmentPk = shipmentBOToLoad.PK;

			var containerBOToLoad1 = bookingBOToLoad.QuotedBookingContainers.AddNew();
			containerBOToLoad1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad1.JC_ReleaseNum = "R11111111";
			containerBOToLoad1.JC_ContainerCount = 1;

			var containerBOToLoad2 = bookingBOToLoad.QuotedBookingContainers.AddNew();
			containerBOToLoad2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad2.JC_ReleaseNum = "R22222222";
			containerBOToLoad2.JC_ContainerCount = 15;

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var containerDataObject1 = new Container
			{
				ContainerCount = 5,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10003333",
			};

			var containerDataObject2 = new Container
			{
				ContainerCount = 3,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10005555",
			};

			var containerDataObject3 = new Container
			{
				ContainerCount = 9,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10007777",
			};

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			shipmentDataObject.ContainerCollection.Add(containerDataObject1);
			shipmentDataObject.ContainerCollection.Add(containerDataObject2);
			shipmentDataObject.ContainerCollection.Add(containerDataObject3);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertContents(bookingBO);
				AssertEquals("shipmentBO.PK", shipmentPk, shipmentBO.PK);
				AssertEquals("bookingBO.QuotedBookingContainers.Count", 3, bookingBO.QuotedBookingContainers.Count);

				var containerBO1 = bookingBO.QuotedBookingContainers[0];
				AssertEquals("containerBO1.JC_ContainerNum", "", containerBO1.JC_ContainerNum);
				AssertEquals("containerBO1.JC_ContainerCount", new ZShort(5), containerBO1.JC_ContainerCount);
				AssertNotNull("containerBO1.RefContainer", containerBO1.RefContainer);
				AssertEquals("containerBO1.RefContainer.RC_Code", "20GP", containerBO1.RefContainer.RC_Code);
				AssertEquals("containerBO1.JC_ContainerMode", "FCL", containerBO1.JC_ContainerMode);
				AssertEquals("containerBO1.JC_ReleaseNum", "R10003333", containerBO1.JC_ReleaseNum);

				var containerBO2 = bookingBO.QuotedBookingContainers[1];
				AssertEquals("containerBO2.JC_ContainerNum", "", containerBO2.JC_ContainerNum);
				AssertEquals("containerBO2.JC_ContainerCount", new ZShort(3), containerBO2.JC_ContainerCount);
				AssertNotNull("containerBO2.RefContainer", containerBO2.RefContainer);
				AssertEquals("containerBO2.RefContainer.RC_Code", "20GP", containerBO2.RefContainer.RC_Code);
				AssertEquals("containerBO2.JC_ContainerMode", "FCL", containerBO2.JC_ContainerMode);
				AssertEquals("containerBO2.JC_ReleaseNum", "R10005555", containerBO2.JC_ReleaseNum);

				var containerBO3 = bookingBO.QuotedBookingContainers[2];
				AssertEquals("containerBO3.JC_ContainerNum", "", containerBO3.JC_ContainerNum);
				AssertEquals("containerBO3.JC_ContainerCount", new ZShort(9), containerBO3.JC_ContainerCount);
				AssertNotNull("containerBO3.RefContainer", containerBO3.RefContainer);
				AssertEquals("containerBO3.RefContainer.RC_Code", "20GP", containerBO3.RefContainer.RC_Code);
				AssertEquals("containerBO3.JC_ContainerMode", "FCL", containerBO3.JC_ContainerMode);
				AssertEquals("containerBO3.JC_ReleaseNum", "R10007777", containerBO3.JC_ReleaseNum);

				AssertContains("20GP container 1 should be successfully matched", "Information - Successfully loaded matching ForwardingContainer.", Logger.Logs);
				AssertContains("20GP container 2 should be successfully matched", "Information - Successfully loaded matching ForwardingContainer.", Logger.Logs);
				AssertContains("20GP container 3 should be added", "Information - No matching ForwardingContainer found, creating new ForwardingContainer.", Logger.Logs);
			});
		}

		public void TestWithContainerNumber_MultipleMatchingContainersInXML()
		{
			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var shipmentPk = shipmentBOToLoad.PK;

			var containerBOToLoad = Factory.New<ForwardingContainer>();
			containerBOToLoad.JC_ContainerNum = "CNT123456";
			containerBOToLoad.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad.JC_ReleaseNum = "R11111111";
			containerBOToLoad.JC_ContainerCount = 1;

			bookingBOToLoad.QuotedBookingContainers.Add(containerBOToLoad);
			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var containerDataObject1 = new Container
			{
				ContainerNumber = "CNT123456",
				ContainerCount = 5,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10003333",
			};

			var containerDataObject2 = new Container
			{
				ContainerNumber = "CNT123456",
				ContainerCount = 3,
				ContainerType = new ContainerType { Code = "20GP", Description = "20GP Container", ISOCode = "20GP" },
				FCL_LCL_AIR = new ContainerMode { Code = "FCL", Description = "Full Container Load" },
				ReleaseNum = "R10005555",
			};

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			shipmentDataObject.ContainerCollection.Add(containerDataObject1);
			shipmentDataObject.ContainerCollection.Add(containerDataObject2);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertContents(bookingBO);
				AssertEquals("shipmentBO.PK", shipmentPk, shipmentBO.PK);
				AssertEquals("bookingBO.QuotedBookingContainers.Count", 2, bookingBO.QuotedBookingContainers.Count);

				var containerBO1 = bookingBO.QuotedBookingContainers[0];
				AssertEquals("containerBO1.JC_ContainerNum", "CNT123456", containerBO1.JC_ContainerNum);
				AssertEquals("containerBO1.JC_ContainerCount", new ZShort(5), containerBO1.JC_ContainerCount);
				AssertNotNull("containerBO1.RefContainer", containerBO1.RefContainer);
				AssertEquals("containerBO1.RefContainer.RC_Code", "20GP", containerBO1.RefContainer.RC_Code);
				AssertEquals("containerBO1.JC_ContainerMode", "FCL", containerBO1.JC_ContainerMode);
				AssertEquals("containerBO1.JC_ReleaseNum", "R10003333", containerBO1.JC_ReleaseNum);

				var containerBO2 = bookingBO.QuotedBookingContainers[1];
				AssertEquals("containerBO2.JC_ContainerNum", "CNT123456", containerBO2.JC_ContainerNum);
				AssertEquals("containerBO2.JC_ContainerCount", new ZShort(3), containerBO2.JC_ContainerCount);
				AssertNotNull("containerBO2.RefContainer", containerBO2.RefContainer);
				AssertEquals("containerBO2.RefContainer.RC_Code", "20GP", containerBO2.RefContainer.RC_Code);
				AssertEquals("containerBO2.JC_ContainerMode", "FCL", containerBO2.JC_ContainerMode);
				AssertEquals("containerBO2.JC_ReleaseNum", "R10005555", containerBO2.JC_ReleaseNum);

				AssertContains("20GP container CNT123456 should be successfully matched", "Information - Successfully loaded matching ForwardingContainer.", Logger.Logs);
				AssertContains("One more 20GP container CNT123456 should be added", "Information - No matching ForwardingContainer found, creating new ForwardingContainer.", Logger.Logs);
			});
		}

		public void TestWithContainerNumber_CompleteCollection()
		{
			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var shipmentPk = shipmentBOToLoad.PK;

			var containerBOToLoad = Factory.New<ForwardingContainer>();
			containerBOToLoad.JC_ContainerNum = "CNT123456";
			containerBOToLoad.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad.JC_ReleaseNum = "R11111111";
			containerBOToLoad.JC_ContainerCount = 1;
			var containerBOToLoad1 = Factory.New<ForwardingContainer>();
			containerBOToLoad1.JC_ContainerNum = "CNT654321";
			containerBOToLoad1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad1.JC_ReleaseNum = "R2222222";
			containerBOToLoad1.JC_ContainerCount = 1;

			bookingBOToLoad.QuotedBookingContainers.Add(containerBOToLoad);
			bookingBOToLoad.QuotedBookingContainers.Add(containerBOToLoad1);
			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var containerDataObject1 = new Container
			{
				ContainerNumber = "CNT123456",
				ReleaseNum = "R10003333",
			};

			var containerDataObject2 = new Container
			{
				ContainerNumber = "CNT666666",
				ReleaseNum = "R10005555",
			};

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			shipmentDataObject.ContainerCollection.Content = CollectionContent.Complete;
			shipmentDataObject.ContainerCollection.Add(containerDataObject1);
			shipmentDataObject.ContainerCollection.Add(containerDataObject2);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertContents(bookingBO);
				AssertEquals("shipmentBO.PK", shipmentPk, shipmentBO.PK);
				bookingBO.QuotedBookingContainers.Load();
				AssertEquals("bookingBO.QuotedBookingContainers.Count", 2, bookingBO.QuotedBookingContainers.Count);

				var containerBO1 = bookingBO.QuotedBookingContainers[0];
				AssertEquals("containerBO1.JC_ContainerNum", "CNT123456", containerBO1.JC_ContainerNum);
				AssertEquals("containerBO1.JC_ReleaseNum", "R10003333", containerBO1.JC_ReleaseNum);

				var containerBO2 = bookingBO.QuotedBookingContainers[1];
				AssertEquals("containerBO2.JC_ContainerNum", "CNT666666", containerBO2.JC_ContainerNum);
				AssertEquals("containerBO2.JC_ReleaseNum", "R10005555", containerBO2.JC_ReleaseNum);
			});
		}

		public void TestWithContainerNumber_PartialCollection()
		{
			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var shipmentPk = shipmentBOToLoad.PK;

			var containerBOToLoad = Factory.New<ForwardingContainer>();
			containerBOToLoad.JC_ContainerNum = "CNT123456";
			containerBOToLoad.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad.JC_ReleaseNum = "R11111111";
			containerBOToLoad.JC_ContainerCount = 1;
			var containerBOToLoad1 = Factory.New<ForwardingContainer>();
			containerBOToLoad1.JC_ContainerNum = "CNT654321";
			containerBOToLoad1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			containerBOToLoad1.JC_ReleaseNum = "R2222222";
			containerBOToLoad1.JC_ContainerCount = 1;

			bookingBOToLoad.QuotedBookingContainers.Add(containerBOToLoad);
			bookingBOToLoad.QuotedBookingContainers.Add(containerBOToLoad1);
			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var containerDataObject1 = new Container
			{
				ContainerNumber = "CNT123456",
				ReleaseNum = "R10003333",
			};

			var containerDataObject2 = new Container
			{
				ContainerNumber = "CNT666666",
				ReleaseNum = "R10005555",
			};

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			shipmentDataObject.ContainerCollection.Content = CollectionContent.Partial;
			shipmentDataObject.ContainerCollection.Add(containerDataObject1);
			shipmentDataObject.ContainerCollection.Add(containerDataObject2);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertContents(bookingBO);
				AssertEquals("shipmentBO.PK", shipmentPk, shipmentBO.PK);
				bookingBO.QuotedBookingContainers.Load();
				AssertEquals("bookingBO.QuotedBookingContainers.Count", 3, bookingBO.QuotedBookingContainers.Count);

				var containerBO1 = bookingBO.QuotedBookingContainers[0];
				AssertEquals("containerBO1.JC_ContainerNum", "CNT123456", containerBO1.JC_ContainerNum);
				AssertEquals("containerBO1.JC_ReleaseNum", "R10003333", containerBO1.JC_ReleaseNum);

				var containerBO2 = bookingBO.QuotedBookingContainers[1];
				AssertEquals("containerBO2.JC_ContainerNum", "CNT654321", containerBO2.JC_ContainerNum);
				AssertEquals("containerBO2.JC_ReleaseNum", "R2222222", containerBO2.JC_ReleaseNum);

				var containerBO3 = bookingBO.QuotedBookingContainers[2];
				AssertEquals("containerBO3.JC_ContainerNum", "CNT666666", containerBO3.JC_ContainerNum);
				AssertEquals("containerBO3.JC_ReleaseNum", "R10005555", containerBO3.JC_ReleaseNum);
			});
		}

		public void TestLinkOrderLineWithPackProduct()
		{
			var packedItemDataObject1 = new PackedItem
			{
				PackedQuantity = 1,
				OrderLineLink = 1,
				UnitOfQuantity = new PackageType { Code = "CNT" },
				Product = new Product { Code = "Camel Poodles" }
			};

			var packedItemDataObject2 = new PackedItem
			{
				PackedQuantity = 1,
				OrderLineLink = 2,
				UnitOfQuantity = new PackageType { Code = "CNT" },
				Product = new Product { Code = "Panda Chow Chows" }
			};

			var packedItemDataObject3 = new PackedItem
			{
				PackedQuantity = 1,
				OrderLineLink = 3,
				UnitOfQuantity = new PackageType { Code = "CNT" },
				Product = new Product { Code = "Dog Wow" }
			};

			var packingLineDataObject = SetupPackingLine(Factory);
			packingLineDataObject.PackQty = 3;

			packingLineDataObject.SetPackedItemCollection(() => new List<PackedItem>
			{
				packedItemDataObject1,
				packedItemDataObject2,
				packedItemDataObject3
			});

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			shipmentDataObject.PackingLineCollection.Add(packingLineDataObject);

			var orderLineDataObject1 = new OrderLine
			{
				LineNumber = 1,
				Link = 1,
				Volume = 0.1m,
				OrderedQty = 1.1m
			};

			var orderLineDataObject2 = new OrderLine
			{
				LineNumber = 2,
				Link = 2,
				Volume = 2.1m,
				OrderedQty = 2.2m
			};

			var orderLineDataObject3 = new OrderLine
			{
				LineNumber = 3,
				Link = 3,
				Volume = 3.3m,
				OrderedQty = 3.4m
			};

			var orderData = GetNewOrderDataObject();
			orderData.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance);
			orderData.Order.OrderNumberSplit = new ZByte(2);

			orderData.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>
			{
				orderLineDataObject1,
				orderLineDataObject2,
				orderLineDataObject3
			});

			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			SetupCFSAddresses(shipmentDataObject);
			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			var shipmentBO = bookingBO.Booking;

			AssertEquals(1, shipmentBO.OuterPackLines.Count);
			AssertEquals(3, shipmentBO.OuterPackLines[0].Products.Count);
			var product1 = shipmentBO.OuterPackLines[0].Products[0];
			AssertEquals("Camel Poodles", product1.D2_ProductCode);
			var product2 = shipmentBO.OuterPackLines[0].Products[1];
			AssertEquals("Panda Chow Chows", product2.D2_ProductCode);
			var product3 = shipmentBO.OuterPackLines[0].Products[2];
			AssertEquals("Dog Wow", product3.D2_ProductCode);

			AssertEquals(1, shipmentBO.AttachedOrders.Count);
			AssertEquals(3, shipmentBO.AttachedOrders[0].OrderLines.Count);
			AssertEquals(0.1m, shipmentBO.AttachedOrders[0].OrderLines[0].JO_ActualVolume);
			AssertEquals(shipmentBO.AttachedOrders[0].OrderLines[0].PK, product1.D2_JO);
			AssertEquals(2.1m, shipmentBO.AttachedOrders[0].OrderLines[1].JO_ActualVolume);
			AssertEquals(shipmentBO.AttachedOrders[0].OrderLines[1].PK, product2.D2_JO);
			AssertEquals(3.3m, shipmentBO.AttachedOrders[0].OrderLines[2].JO_ActualVolume);
			AssertEquals(shipmentBO.AttachedOrders[0].OrderLines[2].PK, product3.D2_JO);
		}

		public void TestWithOrgAddresses()
		{
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var matchingOrgAddressDataObjectCRD = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var newOrgAddressDataObjectNPP = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.NotifyParty));
			var newOrgAddressDataObjectN2D = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.NotifyParty2));
			var newOrgAddressDataObjectN3D = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.NotifyParty3));
			var unknownOrgAddressDataObjectSTP = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ShipToParty));
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(matchingOrgAddressDataObjectCRD);
			shipmentDataObject.OrganizationAddressCollection.Add(newOrgAddressDataObjectNPP);
			shipmentDataObject.OrganizationAddressCollection.Add(newOrgAddressDataObjectN2D);
			shipmentDataObject.OrganizationAddressCollection.Add(newOrgAddressDataObjectN3D);
			shipmentDataObject.OrganizationAddressCollection.Add(unknownOrgAddressDataObjectSTP);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			#region Check Contents of shipment Business object

			var jobDocAddressBOCRD = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			var jobDocAddressBONPP = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);
			var jobDocAddressBON2D = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty2);
			var jobDocAddressBON3D = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty3);
			var jobDocAddressBOSTP = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.ShipToParty);

			AssertEquals("shipmentBO.DocAddresses.Count", 7, shipmentBO.DocAddresses.Count);

			CombineAssertions(delegate
			{
				AssertContents(bookingBO);
				AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBOCRD);
				AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBONPP);
				AssertNull(jobDocAddressBOSTP);
				AssertNull("Organisation Collection without the localclient address does not add it in on the shipment business object", shipmentBO.ShipmentJobHeader);
				AssertEquals("jobDocAddressBOCRD.E2_AddressType", "CRD", jobDocAddressBOCRD.E2_AddressType);
				AssertEquals("jobDocAddressBONPP.E2_AddressType", "NPP", jobDocAddressBONPP.E2_AddressType);
				AssertEquals("jobDocAddressBON2D.E2_AddressType", "N2D", jobDocAddressBON2D.E2_AddressType);
				AssertEquals("jobDocAddressBON3D.E2_AddressType", "N3D", jobDocAddressBON3D.E2_AddressType);
				AssertEquals("jobDocAddressBOCRD.E2_AddressOverride", true, jobDocAddressBOCRD.E2_AddressOverride);
				AssertEquals("jobDocAddressBONPP.E2_AddressOverride", true, jobDocAddressBONPP.E2_AddressOverride);
				AssertEquals("jobDocAddressBOCRD.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBOCRD.E2_OA_Address);
				AssertEquals("jobDocAddressBONPP.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBONPP.E2_OA_Address);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'NotifyParty':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'NotifyParty2':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'NotifyParty3':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ShipToParty':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Unknown Address Type [ShipToParty] found. Job Document Address not imported.
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLocalClientAddress()
		{
			var localClientAddress = GetLocalClientAddress();
			var addressBO = new OrganisationDataObjectReader(localClientAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(localClientAddress);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertLocalClientAddress(shipmentBO.ShipmentJobHeader.LocalChargesAddr);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'LocalClient':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Quick Booking from UniversalShipment.
Information - Successfully saved Quick Booking - Booking (S00001000).
".Trim(), Logger.Logs);
			});
		}

		public void TestWithNotes()
		{
			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = false;

			shipmentDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			shipmentDataObject.NoteCollection.Add(noteDataObject);

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			#region Check Contents of shipment Business object

			StmNote[] note = bookingBO.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);

			CombineAssertions(delegate
			{
				AssertNoteContents(note[0]);
				AssertEquals("noteBO.ST_IsCustomDescription", true, note[0].ST_IsCustomDescription);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestImportNotesWithQuotedBookingNoteTypes()
		{
			var quotedBookingCustomNotes = new CustomNoteModuleAndCountry();
			quotedBookingCustomNotes.ModuleIDName = ModuleIDs.QuotedBookings.Name;
			quotedBookingCustomNotes.CountryCode = "ALL";

			var quotedBookingCustomNote = quotedBookingCustomNotes.CustomNoteTypesList.AddNew();
			quotedBookingCustomNote.NoteName = "Custom Quoted Booking Note";
			quotedBookingCustomNote.IsTextOnly = ZBool.True;
			quotedBookingCustomNote.IsAppendingNote = ZBool.True;
			quotedBookingCustomNote.IsReadOnlyAfterAdd = ZBool.False;
			quotedBookingCustomNote.ForceRead = ZBool.False;
			quotedBookingCustomNote.DefaultVisibility = nameof(StmNoteVisibility.PRV);

			var shipmentCustomNotes = new CustomNoteModuleAndCountry();
			shipmentCustomNotes.ModuleIDName = ModuleIDs.JobShipment.Name;
			shipmentCustomNotes.CountryCode = "ALL";

			var shipmentCustomNote = shipmentCustomNotes.CustomNoteTypesList.AddNew();
			shipmentCustomNote.NoteName = "Custom Shipment Note";
			shipmentCustomNote.IsTextOnly = ZBool.True;
			shipmentCustomNote.IsAppendingNote = ZBool.True;
			shipmentCustomNote.IsReadOnlyAfterAdd = ZBool.False;
			shipmentCustomNote.ForceRead = ZBool.False;
			shipmentCustomNote.DefaultVisibility = nameof(StmNoteVisibility.PRV);

			var customNoteTypes = new CustomNoteTypes();
			customNoteTypes.NoteModuleAndCountryList.Add(quotedBookingCustomNotes);
			customNoteTypes.NoteModuleAndCountryList.Add(shipmentCustomNotes);

			var factory = new BusinessObjectFactory();
			var booking = factory.NewWithValidTestData<ForwardingShipment>();
			booking.JS_IsBooking = true;
			booking.JS_HouseBill = "ForNoteTypes";
			booking.JS_InterimReceipt = "ForNoteTypes";
			booking.JS_CFSReference = "ForNoteTypes";
			booking.JS_BookingReference = "ForNoteTypes";

			var quotedBookingPredefinedNoteTypes = QuotedBooking.New(ZGuid.Empty, booking.PK, factory).NoteTypes;

			using (Registry.Business.SystemDataRegistry.Instance.CustomNotes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customNoteTypes))
			{
				int incrementCount = 0;
				foreach (PredefinedNoteType noteType in quotedBookingPredefinedNoteTypes)
				{
					AssertImportNoteWithQuotedBookingNoteType(noteType.Code, "2023Test" + incrementCount++);
				}

				AssertImportNoteWithQuotedBookingNoteType("Custom Quoted Booking Note", "2023Test" + incrementCount++);
				AssertImportNoteWithQuotedBookingNoteType("Custom Shipment Note", "2023Test" + incrementCount++, false);
			}
		}

		void AssertImportNoteWithQuotedBookingNoteType(ZString noteDescription, ZString uniqueNumber, bool isCustomDescriptionInDataObject = true)
		{
			shipmentDataObject.WayBillNumber = uniqueNumber;
			shipmentDataObject.BookingConfirmationReference = uniqueNumber;
			shipmentDataObject.InterimReceiptNumber = uniqueNumber;
			shipmentDataObject.CFSReference = uniqueNumber;

			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = isCustomDescriptionInDataObject;
			noteDataObject.Description = noteDescription;

			shipmentDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			shipmentDataObject.NoteCollection.Add(noteDataObject);

			Logger.ClearLogs();

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);
			AssertMultilineASCIIEquals("Logger.Logs", $@"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: {noteDescription}) is {(isCustomDescriptionInDataObject ? "NOT " : "")}a custom note. IsCustomDescription(value: {(isCustomDescriptionInDataObject ? "True" : "False/empty")}) was ignored.
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);

			var note = bookingBO.Notes.FindByDescription(noteDescription).First();
			AssertEquals(!isCustomDescriptionInDataObject, note.ST_IsCustomDescription);
		}

		public void TestDuplicates()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quickBooking.Booking.JS_HouseBill = "MYHOUSE";

			var orderItem1 = quickBooking.Booking.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "TOM";
			orderItem1.JT_Sequence = new ZShort(1);

			var orderItem2 = quickBooking.Booking.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "DICK";
			orderItem2.JT_Sequence = new ZShort(2);

			var orderItem3 = quickBooking.Booking.DocsAndCartage.OrderItems.AddNew();
			orderItem3.JT_OrderReference = "HARRY";
			orderItem3.JT_Sequence = new ZShort(3);

			Factory.SaveForTesting();

			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>());
			var orderNumbers = shipmentDataObject.LocalProcessing.OrderNumberCollection;

			orderNumbers.Add(new OrderNumber { OrderReference = "TOM", Sequence = 1 });
			orderNumbers.Add(new OrderNumber { OrderReference = "HARRY", Sequence = 2 });
			orderNumbers.Add(new OrderNumber { OrderReference = "BILL", Sequence = 3 });
			orderNumbers.Add(new OrderNumber { OrderReference = "ED", Sequence = 4 });

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var quickBookingUpdated = reader.ReadIntoBusinessObject();
			var docsBO = quickBookingUpdated.Booking.DocsAndCartage;

			AssertNotNull(quickBookingUpdated);
			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.DocsAndCartage.OrderItems.Count", 4, docsBO.OrderItems.Count);
				AssertEquals("TOM,HARRY,BILL,ED", docsBO.JP_OrderItemsAsString);
				AssertNotEquals(quickBooking.Booking, quickBookingUpdated.Booking);
				var builder = new ZStringBuilder();

				foreach (OrderItem orderItem in quickBookingUpdated.Booking.DocsAndCartage.OrderItems)
				{
					builder.Append(orderItem.JT_Sequence.ToString() + ": " + orderItem.JT_OrderReference);
				}
				AssertMultilineASCIIEquals("", @"
1: TOM
2: HARRY
3: BILL
4: ED".Trim(), builder.ToStringWithNewLineBetweenAppends());
			});
		}

		public void TestWithBuyerSupplierLinks()
		{
			var consignor = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var consignee = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);

			shipmentDataObject.WayBillNumber = "MYHOUSE1";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var addressCNR = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var addressCNE = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(addressCNR);
			shipmentDataObject.OrganizationAddressCollection.Add(addressCNE);

			Factory.SaveForTesting();

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertEquals("Precondition - Origin", "NZDUD", bookingBO.Origin);
			AssertEquals("Precondition - Destination", "AUBDG", bookingBO.Destination);

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_OH_Supplier = consignor.PK;
			OrgSupBuyLinkTrnMode linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			linkTrnMode.PF_RL_NKPlaceOfReceivalPort = "NZCHC";
			linkTrnMode.PF_RL_NKPlaceOfDeliveryPort = "AUMEL";

			shipmentDataObject.WayBillNumber = "MYHOUSE2";
			shipmentDataObject.BookingConfirmationReference = "MYBOOKING2";
			shipmentDataObject.InterimReceiptNumber = "MYRECEIPT2";
			shipmentDataObject.CFSReference = "MYCFSREF2";

			Factory.SaveForTesting();

			bookingBO = reader.ReadIntoBusinessObject();

			AssertEquals("Origin is taken from XML", "NZDUD", bookingBO.Origin);
			AssertEquals("Destination is taken from XML", "AUBDG", bookingBO.Destination);

			shipmentDataObject.PortOfOrigin = new UNLOCO();
			shipmentDataObject.PortOfDestination = new UNLOCO();

			shipmentDataObject.WayBillNumber = "MYHOUSE3";
			shipmentDataObject.BookingConfirmationReference = "MYBOOKING3";
			shipmentDataObject.InterimReceiptNumber = "MYRECEIPT3";
			shipmentDataObject.CFSReference = "MYCFSREF3";

			Factory.SaveForTesting();

			bookingBO = reader.ReadIntoBusinessObject();

			AssertEquals("Origin falls back to Buyer / Supplier link", "NZCHC", bookingBO.Origin);
			AssertEquals("Destination falls back to Buyer / Supplier link", "AUMEL", bookingBO.Destination);
		}

		public void TestPopulateBusinessObjectDoesntUpdateActualsFromChargeable()
		{
			var bookingBOToLoad = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBOToLoad.Booking;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";
			shipmentBOToLoad.JS_UnitOfVolume = Core.Constants.Volume.CubicCentimeters;
			shipmentBOToLoad.JS_ActualVolume = 999999m;

			Factory.SaveForTesting();

			AssertEquals(1m, shipmentBOToLoad.JS_ActualChargeable);

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.TotalWeight = 0m;
			shipmentDataObject.TotalVolume = 999999m;
			shipmentDataObject.TotalVolumeUnit.Code = Core.Constants.Volume.CubicCentimeters;
			shipmentDataObject.TotalWeightUnit.Code = Core.Constants.Weight.Kilograms;
			shipmentDataObject.ActualChargeable = 959595m;

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			QuotedBooking booking;
			AssertNoExceptionThrown("Should not throw developer notification exception", () =>
			{
				booking = reader.ReadIntoBusinessObject();
				Assert(!((ISupportDataImporting)booking.Booking).IsImportingData);
			}
			);
		}

		public void TestReadIntoBusinessObject_ChargeableIsTooBig_ReplaceWithInvalidValue()
		{
			shipmentDataObject.ActualChargeable = 100000000m;

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertEquals("The value should be corrected", 999999m, bookingBO.Chargeable);
		}

		public void TestReadIntoBusinessObject_CarrierContractNumber()
		{
			shipmentDataObject.CarrierContractNumber = "ABC123";

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertEquals("The value should be updated", "ABC123", bookingBO.CarrierContractNumber);
		}

		public void TestReadIntoBusinessObject_CarrierContractNumberFilledFromNumbers()
		{
			var conNumber = new AdditionalReference
			{
				Type = new EntryType
				{
					Code = "CON",
					Description = "Carrier Contract Number"
				},
				ReferenceNumber = "ABC123"
			};

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { conNumber });

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertEquals("The value should be updated", "ABC123", bookingBO.CarrierContractNumber);
		}

		public void TestChargeables_ImportedValueIsOutOfRange_ToMaxValue()
		{
			shipmentDataObject.ActualChargeable = 100000000m;
			shipmentDataObject.DocumentedChargeable = 100000000m;
			shipmentDataObject.ManifestedChargeable = 100000000m;
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertEquals("JS_ActualChargeable value should be corrected", 999999m, shipmentBO.JS_ActualChargeable);
				AssertEquals("JS_DocumentedChargeable value should be corrected", 999999m,
					shipmentBO.JS_DocumentedChargeable);
				AssertEquals("JS_ManifestedChargeable value should be corrected", 999999m,
					shipmentBO.JS_ManifestedChargeable);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Warning - Attempted to insert '100000000' into Field [JS_ActualChargeable] which has a maximum numeric value of '999999'. Field was truncated to the max value.
Warning - Attempted to insert '100000000' into Field [JS_ActualChargeable] which has a maximum numeric value of '999999'. Field was truncated to the max value.
Warning - Attempted to insert '100000000' into Field [JS_DocumentedChargeable] which has a maximum numeric value of '999999'. Field was truncated to the max value.
Warning - Attempted to insert '100000000' into Field [JS_ManifestedChargeable] which has a maximum numeric value of '999999'. Field was truncated to the max value.
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestChargeables_CalculatedValueIsOutOfRange_ToMaxValue()
		{
			shipmentDataObject.TotalWeight = 77777777m;
			shipmentDataObject.TotalVolume = null;
			shipmentDataObject.ActualChargeable = null;
			shipmentDataObject.DocumentedChargeable = null;
			shipmentDataObject.ManifestedChargeable = null;
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingBO);

			var shipmentBO = bookingBO.Booking;

			CombineAssertions(delegate
			{
				AssertEquals("JS_ActualChargeable value should be corrected", 999999m, shipmentBO.JS_ActualChargeable);
				AssertEquals("JS_DocumentedChargeable value should be corrected", 999999m,
					shipmentBO.JS_DocumentedChargeable);
				AssertEquals("JS_ManifestedChargeable value should be corrected", 999999m,
					shipmentBO.JS_ManifestedChargeable);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching QuotedBooking found, creating new QuotedBooking.
Information - Populating QuotedBooking...
Warning - Attempted to insert '77777777' into Field [JS_ActualWeight] which has a maximum numeric value of '999999'. Field was truncated to the max value.
Warning - Attempted to insert '999999000000' into Field [JS_ActualChargeable] which has a maximum numeric value of '999999'. Field was truncated to the max value.
Information - Added Quick Booking from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestReaderRejectsWhenTargetingConvertedQuickBooking()
		{
			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quickBooking.Booking.JS_IsForwardRegistered = false;

			var dummyObjectReader = new DummyForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var expectedErrorMessage = "[*XML Targeted a converted Booking. Target Type should be ForwardingShipment for updating converted Bookings.*]";

			AssertNotEquals(expectedErrorMessage, dummyObjectReader.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(quickBooking));

			quickBooking.Booking.JS_IsForwardRegistered = true;
			AssertEquals(
				"When message targets a non-NVOCC quick booking that has been converted, it should be rejected.",
				expectedErrorMessage,
				dummyObjectReader.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(quickBooking)
			);
		}

		public void TestNoExceptionWhenImportingAttachedDocumentForConvertedNVOCCQuotedBooking()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory.BOFactory);
			quickBooking.LoadPort = "AUSYD";
			quickBooking.DischargePort = "SGSIN";
			quickBooking.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			Factory.SaveForTesting();

			var converter = new QuotedBookingToShipmentConverter(quickBooking, QuotedBookingToShipmentConverter.BookingToShipmentConversionSource.Form);

			if (converter.HasAnyErrors(out var errors))
			{
				Fail(errors);
			}

			converter.ConvertBookingToShipment(quickBooking.Booking);

			Factory.SaveForTesting();

			var dataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			dataContext.RecipientRoleCollection = new List<RecipientRole>();
			dataContext.RecipientRoleCollection.Add(new RecipientRole
			{
				Code = RecipientRoleType.NVO,
				ServiceCode = ServiceCodeType.BRQ
			});

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = dataContext;
			dataObject.DataContext.AddDataTarget(DataContextType.ForwardingBooking, quickBooking.Booking.JS_UniqueConsignRef);
			dataObject.TotalWeight = 1m;
			dataObject.TransportMode = new CodeDescriptionPair { Code = "SEA" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "SGSIN" };

			var attachedDocuments = new List<AttachedDocument>
			{
				AttachedDocumentCreator.Create(
					fileName: "CIV-Commercial_Invoice-DOUCMENT_NUMBER.pdf",
					base64ImageData: "SGVsbG8sIFdvcmxkIQ==",
					documentType: "MSC")
			};

			dataObject.SetAttachedDocumentCollection(() => attachedDocuments);

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
						Address1 = "Booking Party Address 1",
						OrganizationCode = "BKGPARTY"
					}
				});

			using (Factory.BOFactory.AddDisposableService())
			{
				var reader = new ForwardingBookingDataObjectReader(dataObject, logger, Factory);

				AssertNoExceptionThrown("No ErrorReporter exception for importing AttachedDocument to a converted Quick Booking",
					codeToRun: () => reader.ReadIntoBusinessObject());

				AssertContains("Quick Booking was matched",
					expected: "Information - Successfully loaded matching QuotedBooking",
					actualContainingExpected: logger.Logs);

				AssertContains("Quick Booking was updated",
					expected: "Information - Updated Quick Booking",
					actualContainingExpected: logger.Logs);
			}
		}

		public void TestCO2eCalculation_GHGUpdatedEventsLogged()
		{
			// Arrange
			var (quotedBooking, dataObject) = CreateBookingWithSailing("S000001");
			AssertEquals("Pre-condtion", 0m, quotedBooking.Booking.Sailing.GetCO2ePerTonneInKg());
			AssertEquals("Pre-condtion", CO2eStatusList.Codes.NotCalculated, quotedBooking.Booking.Sailing.GetCO2eStatus());

			// Act
			var reader = new ForwardingBookingDataObjectReader(dataObject, logger, Factory);
			var quotedBookingBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);

			// Assert
			AssertEquals(10000m, quotedBookingBO.GetTotalCO2e());
			AssertEquals(CO2eStatusList.Codes.Current, quotedBookingBO.GetCO2eStatus());
			AssertGHGEvent(quotedBookingBO.Logs, "|NEW=10000|OLD=NA|TYP=Updated");

			var sailing = quotedBookingBO.Booking.Sailing;
			AssertEquals(10000m, sailing.GetCO2ePerTonneInKg());
			AssertEquals(CO2eStatusList.Codes.Current, sailing.GetCO2eStatus());
			AssertGHGEvent(sailing.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
		}

		public void TestCO2eCalculation_ImportTEU()
		{
			// Arrange
			var (quotedBooking, dataObject) = CreateBookingWithSailing("S000001");
			var containers = quotedBooking.QuotedBookingContainers;

			containers.RemoveAll();
			var container1 = containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("20GP111", "22G0", 1m, 2280m);
			container1.JC_RC = refContainer1.PK;

			var container2 = containers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("40REHC111", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;

			dataObject.GreenhouseGasEmission.CO2e = 530.8608m;
			dataObject.GreenhouseGasEmission.CO2eUnit = new UnitOfWeight { Code = "KG" };
			dataObject.GreenhouseGasEmission.CO2ePerTEU = 123.456m;
			dataObject.GreenhouseGasEmission.CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" };

			dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2ePerTEU = 23.45m;
			dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" };

			// Act
			var reader = new ForwardingBookingDataObjectReader(dataObject, logger, Factory);
			var quotedBookingBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);

			// Assert
			AssertEquals(530.8608m, quotedBookingBO.GetTotalCO2e());
			AssertEquals(CO2eStatusList.Codes.Current, quotedBookingBO.GetCO2eStatus());
			AssertGHGEvent(quotedBookingBO.Logs, "|NEW=530.8608|OLD=NA|TYP=Updated");
			AssertEquals("530.861", quotedBooking.TotalCO2eForBinding);

			var sailing = quotedBookingBO.Booking.Sailing;
			AssertEquals(10000m, sailing.GetCO2ePerTonneInKg());
			AssertEquals(23.45m, sailing.GetCO2ePerTEUInKg());
			AssertEquals(CO2eStatusList.Codes.Current, sailing.GetCO2eStatus());
			AssertGHGEvent(sailing.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
		}

		RefContainer NewRefContainer(ZString code, ZString isoType, decimal teu, decimal tareWeight)
		{
			var refContainer = RefContainer.New(Factory.BOFactory);
			refContainer.RC_Code = code;
			refContainer.RC_ISOType = isoType;
			refContainer.RC_TEU = teu;
			refContainer.RC_TareWeight = tareWeight;

			return refContainer;
		}

		public void TestCO2eCalculation_TriggerByWorkflow_ChangeParamBeforeReceivingResponse_GHGRejectedEventsLogged()
		{
			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory.BOFactory, sailingLoad: "AUSYD", sailingDischarge: "VNVNH");
			quotedBooking.Booking.JS_UniqueConsignRef = "S00001";
			var trigger = quotedBooking.WorkflowItems.Triggers.AddNew();

			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 1;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCalculateCO2EmissionRequest;
			quotedBooking.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.SaveForTesting();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: Customizable Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);
			var workflowDescriptor = new QuotedBookingWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);

			quotedBooking.Origin = "USLAX";
			var loggerprocess = new NotificationBuffer();
			using (Factory.BOFactory.AddDisposableService())
			{
				processor.Process(loggerprocess);
				Factory.SaveForTesting();
			}

			AssertMultilineASCIIEquals("loggger results from processor.Process()", string.Empty, loggerprocess.AsString);
			var newFactory = new BusinessObjectFactory();
			var messages = newFactory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			var message = messages[0];
			Assert(message.IsInDatabase);
			CombineAssertions("EDI Message sent", () =>
			{
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_ReceiveTransmit", EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertContains("message.EM_MessageText", "UniversalShipment", message.EM_MessageText);
			});

			var interchange = newFactory.Load<EDIInterchange>(message.EM_EI);
			Assert(interchange.IsInDatabase);
			CombineAssertions("EDI Interchange", () =>
			{
				AssertNotNull(interchange);
				AssertEquals("EMISSION_CALCULATOR", interchange.EI_To);
			});

			AssertEquals(CO2eStatusList.Codes.Pending, quotedBooking.GetCO2eStatus());

			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectForQuotedBooking();
			dataObject.DataContext.AddDataTarget(DataContextType.ForwardingBooking, "S00001");
			Factory.SaveForTesting();

			var reader = new ForwardingBookingDataObjectReader(dataObject, logger, Factory);
			var quotedBookingBO = reader.ReadIntoBusinessObject();

			AssertEquals(CO2eStatusList.Codes.NotCurrent, quotedBookingBO.GetCO2eStatus());

			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Warning - Cannot populate CO2e for QuotedBooking because CO2e Calculation input parameters have been changed
Information - Updated Booking with Quote - Quote (00001000) - Booking (S00001) from UniversalShipment."
			, logger.Logs);

			var transportGHGEvent = quotedBookingBO.Logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			AssertEquals("GHG event Reference", "|RES=Input value(s) have changed|TYP=Rejected", transportGHGEvent.SL_Reference);
		}

		void AssertGHGEvent(Logs logs, string reference)
		{
			AssertEquals("New GHG event created", 1, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			var transportGHGEvent = logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			AssertEquals("GHG event Reference", reference, transportGHGEvent.SL_Reference);
		}

		public void TestReaderDoesNotPopulateCO2eWhenQuotedBookingDoesNotMatchCO2eCalculationParameters()
		{
			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory.BOFactory);
			quotedBooking.Booking.JS_UniqueConsignRef = "S00001";
			quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectForQuotedBooking();
			dataObject.DataContext.AddDataTarget(DataContextType.ForwardingBooking, "S00001");

			quotedBooking.Origin = "USLAX";
			var reader = new ForwardingBookingDataObjectReader(dataObject, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Warning - Cannot populate CO2e for QuotedBooking because CO2e Calculation input parameters have been changed
Information - Updated Booking with Quote - Quote (00001000) - Booking (S00001) from UniversalShipment.", logger.Logs);

			logger.ClearLogs();
			quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory.BOFactory);
			quotedBooking.Booking.JS_UniqueConsignRef = "S00002";

			Factory.SaveForTesting();

			dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectForQuotedBooking();
			dataObject.DataContext.AddDataTarget(DataContextType.ForwardingBooking, "S00002");

			reader = new ForwardingBookingDataObjectReader(dataObject, logger, Factory);
			reader.ReadIntoBusinessObject();
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Information - Importing greenhouse gas emissions calculation result.
Information - CO2e is calculated for QuotedBooking: Booking with Quote - Quote (00001001) - Booking (S00002)
Information - Updated Booking with Quote - Quote (00001001) - Booking (S00002) from UniversalShipment.", logger.Logs);
		}

		public void TestReaderDoesNotPopulateSailingCO2eWhenQuotedBookingLinkedSailingDetailChanged()
		{
			void AssertReadRejectedAfterSailingChanged(Action<QuotedBooking> change, string quote, string booking)
			{
				// Arrange
				var (quotedBooking, dataObject) = CreateBookingWithSailing(booking);

				// Act
				change.Invoke(quotedBooking);
				quotedBooking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
				var reader = new ForwardingBookingDataObjectReader(dataObject, logger, Factory);
				var quotedBookingBO = reader.ReadIntoBusinessObject();

				// Assert
				AssertMultilineASCIIEquals($@"Information - Successfully loaded matching QuotedBooking.
Information - Populating QuotedBooking...
Warning - Cannot populate CO2e for QuotedBooking because CO2e Calculation input parameters have been changed
Information - Updated Booking with Quote - Quote ({quote}) - Booking ({booking}) from UniversalShipment."
				, logger.Logs);
				AssertGHGEvent(quotedBooking.Logs, "|RES=Input value(s) have changed|TYP=Rejected");
				logger.ClearLogs();
			}

			CombineAssertions(() =>
			{
				AssertReadRejectedAfterSailingChanged((quotedBooking) => quotedBooking.Booking.Sailing.Voyage.JV_VoyageFlight = "2303N", "00001000", "S00001");
				AssertReadRejectedAfterSailingChanged((quotedBooking) => quotedBooking.Booking.Sailing.Voyage.JV_AircraftType = "AAA", "00001001", "S00002");
				AssertReadRejectedAfterSailingChanged((quotedBooking) => quotedBooking.Booking.Sailing.Origin.JA_RL_NKPortOfLoading = "CNSHA", "00001002", "S00003");
				AssertReadRejectedAfterSailingChanged((quotedBooking) => quotedBooking.Booking.Sailing.Destination.JB_RL_NKPortOfDischarge = "CNSHA", "00001003", "S00004");

				var vessel = Factory.New<RefVessel>();
				vessel.RV_LloydsNumber = "1234";
				vessel.RV_Name = "MSC TINA";
				AssertReadRejectedAfterSailingChanged((quotedBooking) => quotedBooking.Booking.Sailing.Voyage.JV_RV_NKVessel = vessel.RV_FK, "00001004", "S00005");
			});
		}

		(QuotedBooking, UniversalShipment) CreateBookingWithSailing(string uniqueConsignRef)
		{
			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory.BOFactory, sailingLoad: "AUSYD", sailingDischarge: "VNVNH");
			quotedBooking.Booking.JS_UniqueConsignRef = uniqueConsignRef;
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectForQuotedBooking();
			dataObject.DataContext.AddDataTarget(DataContextType.ForwardingBooking, uniqueConsignRef);
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>()
			{
				new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "AUSYD" },
					PortOfDischarge = new UNLOCO { Code = "VNVNH" },
					TransportMode = TransportMode.Sea,
					GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2ePerTonne = 10m, CO2ePerTonneUnit = new UnitOfWeight { Code = "T" }
					}
				}
			});
			return (quotedBooking, dataObject);
		}

		public void TestDefaultNumberOfBills_WithoutReleaseType()
		{
			AssertDefaultNumberOfBills(string.Empty, string.Empty, 5, 6);
		}

		public void TestDefaultNumberOfBills_WithReleaseType()
		{
			AssertDefaultNumberOfBills(Core.Constants.ShipmentReleaseTypes.ExpressBofL, "Express Bill of Lading", 0, 4);
		}

		void AssertDefaultNumberOfBills(string releaseTypeCode, string releaseTypeDescription, int expectedOriginals, int expectedCopies)
		{
			var releaseTypes = FreightDataRegistry.Instance.ReleaseTypes.DefaultValue;
			releaseTypes.OriginalsNumber = 5;
			releaseTypes.CopiesNumber = 6;

			var expressReleaseType = releaseTypes.Types.FindByCode(Core.Constants.ShipmentReleaseTypes.ExpressBofL);
			expressReleaseType.OriginalsNumber = 0;
			expressReleaseType.CopiesNumber = 4;

			FreightDataRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, releaseTypes);

			shipmentDataObject.ReleaseType = new CodeDescriptionPair
			{
				Code = releaseTypeCode,
				Description = releaseTypeDescription
			};

			var reader = new ForwardingBookingDataObjectReader(shipmentDataObject, Logger, Factory);
			var bookingBO = reader.ReadIntoBusinessObject();
			AssertNotNull("Precondition", bookingBO);
			AssertEquals("Originals", (ZByte)expectedOriginals, bookingBO.Booking.JS_NoOriginalBills);
			AssertEquals("Copies", (ZByte)expectedCopies, bookingBO.Booking.JS_NoCopyBills);
		}

		#region Implementation

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipmentDataObject.ContainerMode = new ContainerMode() { Code = "FCL", Description = "Full Container Load" };
			shipmentDataObject.AdditionalTerms = "Add Me Some Terms";
		}

		static void AssertContents(QuotedBooking bookingBO)
		{
			var shipmentBO = bookingBO.Booking;

			AssertEquals("shipmentBO.JS_AdditionalTerms", "Add Me Some Terms", shipmentBO.JS_AdditionalTerms);

			AssertEquals("shipmentBO.JS_TransportMode", "SEA", shipmentBO.JS_TransportMode);
			AssertEquals("shipmentBO.JS_PackingMode", "FCL", shipmentBO.JS_PackingMode);

			AssertEquals("shipmentBO.JS_AWBServiceLevel", "ELG", shipmentBO.JS_AWBServiceLevel);
			AssertEquals("shipmentBO.JS_BookingReference", "BOOK ME", shipmentBO.JS_BookingReference);
			AssertEquals("shipmentBO.JS_CFSReference", "CFS Book Ref", shipmentBO.JS_CFSReference);

			AssertEquals("shipmentBO.JS_UnitFreightRate", 67.89m, shipmentBO.JS_UnitFreightRate);
			AssertEquals("shipmentBO.JS_RX_NKFrtRateCurrency", Enterprise.Core.Constants.CurrencyCodes.CzechRepublic, shipmentBO.JS_RX_NKFrtRateCurrency);
			AssertEquals("shipmentBO.JS_GoodsDescription", "RAT HATS", shipmentBO.JS_GoodsDescription);
			AssertEquals("shipmentBO.JS_GoodsValue", 5.67m, shipmentBO.JS_GoodsValue);
			AssertEquals("shipmentBO.JS_RX_NKGoodsValueCurr", Enterprise.Core.Constants.CurrencyCodes.Ghana, shipmentBO.JS_RX_NKGoodsValueCurr);
			AssertEquals("shipmentBO.JS_InsuranceValue", 6.78m, shipmentBO.JS_InsuranceValue);
			AssertEquals("shipmentBO.JS_RX_NKInsuranceCurrency", Enterprise.Core.Constants.CurrencyCodes.Kenya, shipmentBO.JS_RX_NKInsuranceCurrency);
			AssertEquals("shipmentBO.JS_InterimReceipt", "IR Text", shipmentBO.JS_InterimReceipt);

			AssertEquals("shipmentBO.JS_IsDirectBooking", false, shipmentBO.JS_IsDirectBooking);
			AssertEquals("shipmentBO.JS_IsForwardRegistered", false, shipmentBO.JS_IsForwardRegistered);

			AssertEquals("shipmentBO.JS_OuterPacks", 44, shipmentBO.JS_OuterPacks);
			AssertEquals("shipmentBO.JS_F3_NKPackType", "VF", shipmentBO.JS_F3_NKPackType);

			AssertEquals("shipmentBO.JS_PackingOrder", 1, shipmentBO.JS_PackingOrder);
			AssertEquals("shipmentBO.JS_RS_NKServiceLevel", "PFT", shipmentBO.JS_RS_NKServiceLevel);
			AssertEquals("shipmentBO.JS_INCO", "CIF", shipmentBO.JS_INCO);

			AssertEquals("shipmentBO.JS_ActualVolume", 23.45m, shipmentBO.JS_ActualVolume);
			AssertEquals("shipmentBO.JS_UnitOfVolume", "CF", shipmentBO.JS_UnitOfVolume);
			AssertEquals("shipmentBO.JS_ActualWeight", 34.56m, shipmentBO.JS_ActualWeight);
			AssertEquals("shipmentBO.JS_UnitOfWeight", "KT", shipmentBO.JS_UnitOfWeight);

			AssertEquals("shipmentBO.JS_RL_NKOrigin", "NZDUD", shipmentBO.JS_RL_NKOrigin);
			AssertEquals("shipmentBO.JS_RL_NKLoadPort", "NZCHC", shipmentBO.JS_RL_NKLoadPort);
			AssertEquals("shipmentBO.JS_RL_NKDischargePort", "AUSYD", shipmentBO.JS_RL_NKDischargePort);
			AssertEquals("shipmentBO.JS_RL_NKDestination", "AUBDG", shipmentBO.JS_RL_NKDestination);

			AssertEquals("shipmentBO.JS_ActualChargeable", 123.45m, shipmentBO.JS_ActualChargeable);
			AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
		}

		class DummyForwardingBookingDataObjectReader : ForwardingBookingDataObjectReader
		{
			public DummyForwardingBookingDataObjectReader(UniversalShipment bookingDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(bookingDataObject, logger, factory)
			{
			}

			public new ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(QuotedBooking targetBO)
				=> base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		#endregion
	}
}
