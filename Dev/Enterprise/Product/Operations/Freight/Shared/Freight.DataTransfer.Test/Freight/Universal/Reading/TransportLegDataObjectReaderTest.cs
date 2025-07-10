using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Agency;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public class TransportLegDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateBusinessObjectWithoutSailingDataAndTransportModeSea()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Sea,
				new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4),
				new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" },
				new ZDateTime(2011, 3, 8), new ZDateTime(2011, 3, 3));
			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "SEA", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);
			AssertEquals("transportBO.JW_STD", new ZDateTime(2011, 3, 3), transportBO.JW_STD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);
			AssertEquals("transportBO.JW_STA", new ZDateTime(2011, 3, 8), transportBO.JW_STA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs",
					@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataAndTransportModeRail()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Rail, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });
			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "RAI", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs",
					@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataPortOfLoadingAndTransportModeAir()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Air, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), null, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });
			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "AIR", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", ZString.Empty, transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin:  Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataPortOfDepartureAndTransportModeAir()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Air, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, null);
			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "AIR", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", ZString.Empty, transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: 
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataWithVoyageFlightAndTransportModeSea()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Sea, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });
			transportLegDataObject.VoyageFlightNo = "343L";

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", "343L", transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "SEA", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs",
					@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataWithVoyageFlightAndTransportModeRail()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Rail, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });
			transportLegDataObject.VoyageFlightNo = "343L";

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", "343L", transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "RAI", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs",
					@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataEstimatedDepartureTransportModeAir()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Air, new ZDateTime(2011, 3, 6), null, new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "AIR", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", ZString.Empty, transportBO.JW_ETD.ToString());
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataEstimatedArrivalAndTransportModeAir()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Air, null, new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "AIR", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", ZString.Empty, transportBO.JW_ETA.ToString());
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataEstimatedArrivalAndEstimatedDepartureAndTransportModeAir()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Air, null, null, new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "AIR", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", ZString.Empty, transportBO.JW_ETD.ToString());
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", ZString.Empty, transportBO.JW_ETA.ToString());
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataWithVesselAndTransportModeSea()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Sea, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });
			transportLegDataObject.VesselName = "ROTOITI";

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", "ROTOITI", transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "SEA", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs",
					@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataWithVesselAndTransportModeRail()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Rail, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });
			transportLegDataObject.VesselName = "ROTOITI";

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", "ROTOITI", transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "RAI", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs",
					@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithoutSailingDataWithVesselAndTransportModeAir()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Air, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });
			transportLegDataObject.VesselName = "ROTOITI";

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", ZString.Empty, transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "AIR", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestPopulateBusinessObjectWithVoyageFlightAndVesselAndTransportModeInlandWaterwayTransport()
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.InlandWaterway, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });
			transportLegDataObject.VoyageFlightNo = "343L";
			transportLegDataObject.VesselName = "ROTOITI";
			transportLegDataObject.LegType = Enterprise.UniversalDataBuss.DataObjects.Universal.LegType.PreCarriage;

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertEquals("transportBO.JW_VoyageFlight", "343L", transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", "ROTOITI", transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "IWT", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs",
					@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - No schedule for Inland Waterway Transport. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestBizObjectProvider()
		{
			var dataObject = new TransportLeg
			{
				PortOfLoading = new UNLOCO { Code = "AUSYD" },
				PortOfDischarge = new UNLOCO { Code = "NZAKL" },
				VoyageFlightNo = "XXX"
			};

			var shipment = GetNewShipmentBO();
			var transports = GetTransportCollection(shipment);
			var transportLeg1 = transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";
			transportLeg1.JW_VoyageFlight = "001";

			var transportLeg2 = transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "NZAKL";
			transportLeg2.JW_RL_NKDiscPort = "USCHI";
			transportLeg2.JW_VoyageFlight = "002";

			var reader = new TransportLegDataObjectReader(dataObject, logger, Factory, shipment, dataObj => transportLeg2);
			reader.ReadIntoBusinessObject();

			AssertEquals("XXX", transportLeg2.JW_VoyageFlight);
		}

		public void TestBasicNoteLevelFieldMappings()
		{
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "BUNGA DELIMA";

			var carrierAddressForSetup = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("Fooey");
			var carrierReaderForSetup = new OrganisationDataObjectReader(carrierAddressForSetup, logger, Factory);
			var carrierAddressBoToLoad = carrierReaderForSetup.GetMatchedOrNewForTesting();

			var creditorAddressForSetup = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("CreditorSYD");
			var creditorReaderForSetup = new OrganisationDataObjectReader(creditorAddressForSetup, logger, Factory);
			var creditorAddressBOToLoad = creditorReaderForSetup.GetMatchedOrNewForTesting();

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);
			AssertNotNull(transportBO.Carrier);
			AssertNotNull(transportBO.CreditorAddress.Header);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertContents(transportBO);
				AssertEquals("transportBO.JW_Vessel", "BUNGA DELIMA", transportBO.JW_Vessel);
				AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
				AssertEquals("transportBO.CarrierAddress", carrierAddressBoToLoad.PK, transportBO.CarrierAddress.PK);
				AssertEquals("transportBO.CreditorAddress", creditorAddressBOToLoad.PK, transportBO.CreditorAddress.PK);
				AssertOrgHeaderContents(transportBO.Carrier);
				AssertOrgHeaderContents(transportBO.CreditorAddress.Header);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Matching 'CreditorSYD':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Fooey':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - A Schedule has been found and linked to the Transport Leg.
Information - Matching 'CreditorSYD':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestUnMatchingCarrierAndCreditorDoesNotGetAdded()
		{
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "ROTOITI";

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertNotNull(transportBO);
				AssertContents(transportBO);
				AssertEquals("transportBO.JW_Vessel", "ROTOITI", transportBO.JW_Vessel);
				AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
				AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
				AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);
				AssertMultilineASCIIEquals("logger.Logs", @"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestMatchingTransportLegAgainstCarrier_ValidVessel()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			Factory.SaveForTesting();

			var transportLegDataObject = SetupTransportLegToTestLinkedSailing();
			transportLegDataObject.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "Carrier"
			};
			transportLegDataObject.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
					Value = "AAA",
					CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.UnitedStates }
				}
			});

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertEquals(@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to address '#1' on 'H5ZX52PAMCOI' by registration detail (Country/Region='US', Type='CCC', Number='AAA').
Information - A Schedule has been found and linked to the Transport Leg.
Information - Transport Leg updated.", logger.Logs);
		}

		public void TestMatchingTransportLegAgainstCarrier_InvalidVessel()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			Factory.SaveForTesting();

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "FAKE";
			transportLegDataObject.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "Carrier"
			};
			transportLegDataObject.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
					Value = "AAA",
					CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.UnitedStates } }
			});

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			reader.ReadIntoBusinessObject();

			AssertEquals(@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Matching 'Carrier':- Matched to address '#1' on 'H5ZX52PAMCOI' by registration detail (Country/Region='US', Type='CCC', Number='AAA').
Information - An existing vessel could not be found. A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Matching 'Carrier':- Matched to address '#1' on 'H5ZX52PAMCOI' by registration detail (Country/Region='US', Type='CCC', Number='AAA').
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.", logger.Logs);
		}

		public void TestMatchingTransportLegAgainstCarrier_InvalidVessel_Not_Sea()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			Factory.SaveForTesting();

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.TransportMode = TransportMode.Air;
			transportLegDataObject.VesselName = "FAKE";
			transportLegDataObject.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "Carrier"
			};
			transportLegDataObject.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
					Value = "AAA",
					CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.UnitedStates } }
			});

			var agencyShipment = Factory.BOFactory.New<IAgencyShipment>();
			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, agencyShipment as ITransportParentCommon);
			reader.ReadIntoBusinessObject();

			AssertEquals(@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule will not be created. Attempting to find an existing Schedule...
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Information - Matching 'Carrier':- Matched to address '#1' on 'H5ZX52PAMCOI' by registration detail (Country/Region='US', Type='CCC', Number='AAA').
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.", logger.Logs);
		}

		public void TestImportingFlightScheduleRegistryThreshold()
		{
			SystemDataRegistry.Instance.UpdateSchedulesDuringAutomaticImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);

			var existingVoyage = Factory.New<JobVoyage>();
			existingVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			existingVoyage.JV_VoyageFlight = "QF409";
			existingVoyage.JV_IsCargoOnly = true;
			existingVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			existingVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			existingVoyage.Destinations[0].JB_E_ARV = new ZDateTime(2011, 3, 6).AddHours(7);
			existingVoyage.GenerateSailings();

			var existingSailing = existingVoyage.Sailings[0];
			var shipment = GetNewShipmentBO();
			SetTransportMode(shipment, Core.Constants.TransportModes.Air);

			Factory.SaveForTesting();

			Assert("Pre-condition", existingVoyage.IsInDatabase);

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.TransportMode = TransportMode.Air;
			transportLegDataObject.VoyageFlightNo = "QF409";
			transportLegDataObject.VesselName = "ROTOITI";

			//transport parent needs to be importing data
			((ISupportDataImporting)shipment).IsImportingData = true;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);

			AssertNotNull(transportBO);
			AssertNotEquals("Transport should have a sailing", ZGuid.Empty, transportBO.JW_JX);
			AssertNotEquals("Transport should have a voyage", ZGuid.Empty, transportBO.Voyage);

			Assert(transportBO.Voyage.IsInDatabase);

			AssertNotEquals("Voyage should not be matched to the existing voyage as the difference in time between the ETA/ETD is greater than the registry threshold",
				existingVoyage.PK, transportBO.Voyage.PK);
			AssertNotEquals("Voyage should not be matched to the existing sailing as a new flight schedule should have been generated", existingSailing.PK, transportBO.JW_JX);
		}

		public void TestFallbackLloydsNumberToFindVesselName()
		{
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselLloydsIMO = "8907993";

			var shipment = GetNewShipmentBO();

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);
			AssertEquals("transportBO.JW_Vessel", "BUNGA DELIMA", transportBO.JW_Vessel);
		}

		public void TestLoadOfTransportLegOffParentWhenPortsAreEmpty()
		{
			var transportBO = Factory.New<Transport>();
			var shipment = GetNewShipmentBO();
			GetTransportCollection(shipment).Add(transportBO);
			transportBO.JW_RL_NKDiscPort = "";
			transportBO.JW_RL_NKLoadPort = "";
			transportBO.JW_Vessel = "THIS FIELD WILL NOT BE TOUCHED";

			Factory.SaveForTesting();

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.PortOfDischarge = null;
			transportLegDataObject.PortOfLoading = null;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertEquals("transportBO.JW_VoyageFlight", "343L", transportBO.JW_VoyageFlight);
				AssertEquals("transportBO.JW_TransportMode", "SEA", transportBO.JW_TransportMode);
				AssertEquals("transportBO.JW_Vessel", "THIS FIELD WILL NOT BE TOUCHED", transportBO.JW_Vessel);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin:  Destination: 
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadOfTransportLegOffShipment()
		{
			var transportBO = Factory.New<Transport>();
			var shipment = GetNewShipmentBO();
			GetTransportCollection(shipment).Add(transportBO);
			transportBO.JW_RL_NKDiscPort = "AUMEL";
			transportBO.JW_RL_NKLoadPort = "NZAKL";
			transportBO.JW_Vessel = "THIS FIELD WILL NOT BE TOUCHED";

			Factory.SaveForTesting();

			var transportLegDataObject = SetupTransportLeg();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertContents(transportBO);
				AssertEquals("transportBO.JW_Vessel", "THIS FIELD WILL NOT BE TOUCHED", transportBO.JW_Vessel);
				AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestLoadOfTransportLegOffConsolOnAShipment()
		{
			var transportBO = Factory.New<Transport>();
			var shipmentBO = Factory.New<CommonShipment>();
			var consolBO = Factory.New<CommonConsol>();

			consolBO.JK_RL_NKDischargePort = "AUMEL";
			consolBO.JK_RL_NKLoadPort = "NZAKL";
			consolBO.Transports.Add(transportBO);
			transportBO.JW_RL_NKDiscPort = "AUMEL";
			transportBO.JW_RL_NKLoadPort = "NZAKL";
			transportBO.JW_Vessel = "THIS FIELD WILL NOT BE TOUCHED";

			shipmentBO.Consols.Add(consolBO);

			Factory.SaveForTesting();

			var transportLegDataObject = SetupTransportLeg();

			Func<TransportLeg, Transport> provider = dataObj =>
			{
				var finder = new TransportLegBusinessObjectFinder(dataObj, shipmentBO);
				return finder.Find(shipmentBO.TransportsIncludingRelated.Cast<Transport>());
			};

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipmentBO, provider);
			transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertContents(transportBO);
				AssertEquals("transportBO.JW_Vessel", "THIS FIELD WILL NOT BE TOUCHED", transportBO.JW_Vessel);
				AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestSeaTransportIsNotLinkedToSchedule_MissingVessel()
		{
			var shipment = GetNewShipmentBO();
			SetTransportMode(shipment, Core.Constants.TransportModes.Sea);

			var transportLegDataObject = SetupTransportLeg();

			Assert("Pre-condition", !transportLegDataObject.VesselName.HasValue);

			int sailings = Factory.BOFactory.GetDatabaseCount(typeof(JobSailing));

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			Assert("Sea transport cannot be linked without a vessel", !transportBO.JW_IsLinked);
			AssertEquals("no new sailing created", sailings, Factory.BOFactory.GetDatabaseCount(typeof(JobSailing)));
		}

		public void TestSeaTransportIsNotLinkedToSchedule_MissingCarrier()
		{
			var shipment = GetNewShipmentBO();
			SetTransportMode(shipment, Core.Constants.TransportModes.Sea);

			var vesselWithoutCarrier = Factory.BOFactory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_OH, null));
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.Carrier = null;
			transportLegDataObject.VesselName = vesselWithoutCarrier.RV_Name;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transport = reader.ReadIntoBusinessObject();

			Assert("Expected not to be linked as we cannot determine the carrier", !transport.JW_IsLinked);
			Assert("Expected no sailing to be created and linked to the transport", transport.JW_JX.IsEmpty);

			var carrier = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;

			var vesselWithCarrier = Factory.BOFactory.NewWithValidTestData<RefVessel>();
			vesselWithCarrier.RV_Name = "Milano";
			vesselWithCarrier.RV_OH = carrier.PK;

			transportLegDataObject.VesselName = vesselWithCarrier.RV_Name;
			reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			transport = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to default carrier from the vessel", transport.Carrier, carrier);
			Assert("Expected to be linked as this vessel has a default carrier", transport.JW_IsLinked);
			Assert("Expected a sailing to be created from linked transport", !transport.JW_JX.IsEmpty);
		}

		public void TestGetLinkedToScheduleIfThereIsEnoughInformation()
		{
			var shipment = GetNewShipmentBO();
			SetTransportMode(shipment, Core.Constants.TransportModes.Sea);

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "ROTOITI";
			transportLegDataObject.Carrier = null;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();
			Assert(transportBO.JW_IsLinked);
			AssertTransportSailingCorrectlyImported(transportLegDataObject, transportBO);

			SetTransportMode(shipment, Core.Constants.TransportModes.Rail);

			transportLegDataObject.TransportMode = TransportMode.Rail;
			transportLegDataObject.VesselName = "LostGhosts";
			transportLegDataObject.VoyageFlightNo = "Hea734";

			reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			transportBO = reader.ReadIntoBusinessObject();
			Assert(transportBO.JW_IsLinked);
			AssertTransportSailingCorrectlyImported(transportLegDataObject, transportBO);

			SetTransportMode(shipment, Core.Constants.TransportModes.Air);

			transportLegDataObject.TransportMode = TransportMode.Air;
			transportLegDataObject.VoyageFlightNo = "D1234";
			transportLegDataObject.VesselName = "";

			reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			transportBO = reader.ReadIntoBusinessObject();
			Assert(transportBO.JW_IsLinked);

			SetTransportMode(shipment, Core.Constants.TransportModes.Road);

			transportLegDataObject.TransportMode = TransportMode.Road;
			transportLegDataObject.VesselName = "";
			transportLegDataObject.VoyageFlightNo = "T108";

			Assert(transportBO.JW_IsLinked);
		}

		#region TestStopPopulateArchivedScheduleIfNoSailingDataChanged

		public void TestStopPopulateArchivedScheduleIfNoSailingDataChanged()
		{
			var etd = new ZDateTime(2011, 3, 4);
			var eta = new ZDateTime(2011, 3, 6);
			var consol = SetupConsolWithVoyage("NZAKL", "AUMEL", etd, eta);
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "ROTOITI";
			transportLegDataObject.Carrier = null;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();
			AssertNotNull("New sailing should be created", transportBO.Sailing);
			AssertEquals("New sailing should match consol.JK_JX_Sailing", consol.JK_JX_Sailing, transportBO.Sailing.PK);
			AssertEquals("New Sailing should match consol.Transports[0].JW_JX", consol.Transports[0].JW_JX, transportBO.Sailing.PK);
			AssertEquals("TransportBO sailing should not be archived", false, transportBO.Sailing.Voyage.IsArchived);

			consol.Transports[0].Sailing.Voyage.JV_IsActive = false;
			reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO2 = reader.ReadIntoBusinessObject();
			AssertNotNull("Old sailing should be kept unchanged", transportBO2.Sailing);
			AssertEquals("Old sailing should match consol.JK_JX_Sailing", consol.JK_JX_Sailing, transportBO2.Sailing.PK);
			AssertEquals("Old Sailing should match consol.Transports[0].JW_JX", consol.Transports[0].JW_JX, transportBO2.Sailing.PK);
			AssertEquals("TransportBO2 sailing should be archived", true, transportBO2.Sailing.Voyage.IsArchived);
		}

		public static TransportLeg SetupTransportLegWithoutVoyageAndVessel(TransportMode transportMode, ZDateTime? estimatedArrival, ZDateTime? estimatedDeparture, UNLOCO portOfLoading, UNLOCO portOfDischarge,
			ZDateTime? scheduleArrival = null, ZDateTime? scheduleDeparture = null)
		{
			var transportLegDataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);

			transportLegDataObject.LegOrder = 1;
			transportLegDataObject.TransportMode = transportMode;
			transportLegDataObject.ActualArrival = new ZDateTime(2011, 3, 7);
			transportLegDataObject.ActualDeparture = new ZDateTime(2011, 3, 5);
			transportLegDataObject.CarrierBookingReference = "BOOKMEUP";
			transportLegDataObject.CarrierServiceLevel = new ServiceLevel() { Code = "STD", Description = "Standard" };
			transportLegDataObject.EstimatedArrival = estimatedArrival;
			transportLegDataObject.EstimatedDeparture = estimatedDeparture;
			transportLegDataObject.LegType = Enterprise.UniversalDataBuss.DataObjects.Universal.LegType.Main;
			transportLegDataObject.LegNotes = "Test Leg Notes";
			transportLegDataObject.PortOfDischarge = portOfDischarge;
			transportLegDataObject.PortOfLoading = portOfLoading;
			transportLegDataObject.VoyageFlightNo = "";
			transportLegDataObject.AircraftType = new CodeDescriptionPair() { Code = "E90" };
			transportLegDataObject.Carrier = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("Fooey");
			transportLegDataObject.Creditor = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("CreditorSYD");
			transportLegDataObject.IsCargoOnly = true;
			transportLegDataObject.VesselName = "";
			transportLegDataObject.ScheduledDeparture = scheduleDeparture;
			transportLegDataObject.ScheduledArrival = scheduleArrival;

			return transportLegDataObject;
		}

		CommonConsol SetupConsolWithVoyage(string origin1, string destination1, ZDateTime etd, ZDateTime eta)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "SEA";
			var vessel = RefVessel.LookupVesselByName("ROTOITI", voyage.Factory).First();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "343L";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = origin1;
			origin.JA_E_DEP = etd;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = destination1;
			destination.JB_E_ARV = eta;

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			Factory.SaveForTesting();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = sailing.PK;

			return consol;
		}

		#endregion

		#region TestUnlinkArchivedScheduleIfSailingDataChanged

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_EstimatedDeparture()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) => leg.EstimatedDeparture = ((ZDateTime)leg.EstimatedDeparture).AddHours(1));
		}

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_ActualDeparture()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) => leg.ActualDeparture = ((ZDateTime)leg.ActualDeparture).AddHours(1));
		}

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_EstimatedArrival()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) => leg.EstimatedArrival = ((ZDateTime)leg.EstimatedArrival).AddHours(1));
		}

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_ActualArrival()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) => leg.ActualArrival = ((ZDateTime)leg.ActualArrival).AddHours(1));
		}

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_LCLAvailability()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) => leg.LCLAvailability = ZDateTime.Now);
		}

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_LCLCutOff()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) => leg.LCLCutOff = ZDateTime.Now);
		}

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_LCLReceivalCommences()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) => leg.LCLReceivalCommences = ZDateTime.Now);
		}

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_LCLStorageDate()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) => leg.LCLStorageDate = ZDateTime.Now);
		}

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_DepartureFrom()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) =>
			{
				leg.DepartureFrom = CreateOrganizationAddress("1 DEPARTUREFROM ST", "DEPART");
				Factory.SaveForTesting();
			});
		}

		public void TestUnlinkArchivedScheduleIfSailingDataChanged_ArrivalAt()
		{
			AssertUnlinkArchivedScheduleIfSailingDataChanged((leg) =>
			{
				leg.ArrivalAt = CreateOrganizationAddress("2 ARRIVALAT ST", "ARRIVE");
				Factory.SaveForTesting();
			});
		}

		void AssertUnlinkArchivedScheduleIfSailingDataChanged(Action<TransportLeg> action)
		{
			AssertNotNull("Pre condition: action should not be null", action);

			var shipment = GetNewShipmentBO();
			SetTransportMode(shipment, Core.Constants.TransportModes.Sea);

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "ROTOITI";
			transportLegDataObject.Carrier = null;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();
			AssertNotNull("An active sailing should be created", transportBO.Sailing);
			Assert("Transport should be linked to the sailing", transportBO.JW_IsLinked);
			AssertTransportSailingCorrectlyImported(transportLegDataObject, transportBO);

			transportBO.Sailing.Voyage.JV_IsActive = false;
			action(transportLegDataObject);

			reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO2 = reader.ReadIntoBusinessObject();
			AssertNull("No sailing is linked to the transport", transportBO2.Sailing);
			AssertEquals("Transport JW_IsLinked should be cleared", false, transportBO2.JW_IsLinked);
		}

		#endregion

		public void TestNotGetLinkedToScheduleIfSavedTransportLegIsNotLinked()
		{
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "ROTOITI";

			var shipment = GetNewShipmentBO();
			SetTransportMode(shipment, Core.Constants.TransportModes.Sea);

			var attachedTransport = GetTransportCollection(shipment).AddNew();
			attachedTransport.JW_IsLinked = false;
			attachedTransport.JW_LegOrder = (ZByte)transportLegDataObject.LegOrder;
			attachedTransport.JW_TransportMode = shipment is CommonShipment ? (shipment as CommonShipment).JS_TransportMode : null;
			attachedTransport.JW_ATA = (ZDateTime)transportLegDataObject.ActualArrival;
			attachedTransport.JW_ATD = (ZDateTime)transportLegDataObject.ActualDeparture;
			attachedTransport.JW_CarrierBookingReference = (ZString)transportLegDataObject.CarrierBookingReference;
			attachedTransport.JW_ETA = (ZDateTime)transportLegDataObject.EstimatedArrival;
			attachedTransport.JW_ETD = (ZDateTime)transportLegDataObject.EstimatedDeparture;
			attachedTransport.JW_TransportType = new LegTypeConverter().FromEnumValue(transportLegDataObject.LegType);
			attachedTransport.JW_RL_NKDiscPort = "AUMEL";
			attachedTransport.JW_RL_NKLoadPort = "NZAKL";
			attachedTransport.JW_VoyageFlight = (ZString)transportLegDataObject.VoyageFlightNo;

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			Assert(!transportBO.JW_IsLinked);
		}

		public void TestKeepLinkedToScheduleIfSavedTransportLegIsLinked()
		{
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "ROTOITI";
			transportLegDataObject.Carrier = null;

			var shipmentBO = GetNewShipmentBO();
			SetTransportMode(shipmentBO, Core.Constants.TransportModes.Sea);
			CommonConsol consolBO = null;
			if (shipmentBO is CommonShipment)
			{
				consolBO = (shipmentBO as CommonShipment).Consols.AddNew();
				consolBO.JK_RL_NKDischargePort = transportLegDataObject.PortOfDischarge.Code.Value;
				consolBO.JK_RL_NKLoadPort = transportLegDataObject.PortOfLoading.Code.Value;
			}

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipmentBO);
			var transportBO = reader.ReadIntoBusinessObject();

			if (consolBO != null)
			{
				consolBO.Transports.Add(transportBO);
				(shipmentBO as CommonShipment).Consols.Add(consolBO);
			}

			Factory.SaveForTesting();

			Assert(transportBO.JW_IsLinked);
			AssertTransportSailingCorrectlyImported(transportLegDataObject, transportBO);
		}

		public void TestUpdateSailingSchedulesDuringAutomaticImport()
		{
			var shipmentBO = GetNewShipmentBO();
			SetTransportMode(shipmentBO, Core.Constants.TransportModes.Sea);

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "ROTOITI";
			transportLegDataObject.Carrier = null;

			CommonConsol consolBO = null;
			if (shipmentBO is CommonShipment)
			{
				consolBO = (shipmentBO as CommonShipment).Consols.AddNew();
				consolBO.JK_RL_NKDischargePort = transportLegDataObject.PortOfDischarge.Code.Value;
				consolBO.JK_RL_NKLoadPort = transportLegDataObject.PortOfLoading.Code.Value;
			}

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipmentBO);
			var transportBO = reader.ReadIntoBusinessObject();

			if (consolBO != null)
			{
				consolBO.Transports.Add(transportBO);
			}

			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			var sailing1 = factory.Load<JobSailing>(transportBO.Sailing.PK);

			Assert(transportBO.JW_IsLinked);
			AssertTransportSailingCorrectlyImported(transportLegDataObject, transportBO);

			Enterprise.Registry.Business.SystemDataRegistry.Instance.UpdateSailingSchedulesDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var transportLegDataObject2 = SetupTransportLegToTestLinkedSailing();
			reader = new TransportLegDataObjectReader(transportLegDataObject2, logger, Factory, shipmentBO);
			var transportBO2 = reader.ReadIntoBusinessObject();

			var sailing2 = transportBO2.Sailing;

			AssertNotEquals(sailing1, sailing2);
			AssertEquals(sailing2.Origin.JA_E_DEP, sailing1.Origin.JA_E_DEP);
			AssertEquals(sailing2.Origin.JA_A_DEP, sailing1.Origin.JA_A_DEP);
			AssertEquals(sailing2.Origin.JA_RL_NKPortOfLoading, sailing1.Origin.JA_RL_NKPortOfLoading);
			AssertEquals(sailing2.Destination.JB_E_ARV, sailing1.Destination.JB_E_ARV);
			AssertEquals(sailing2.Destination.JB_A_ARV, sailing1.Destination.JB_A_ARV);
			AssertEquals(sailing2.Destination.JB_RL_NKPortOfDischarge, sailing1.Destination.JB_RL_NKPortOfDischarge);
			AssertEquals(sailing2.JX_DepotAvailabilityDate, sailing1.JX_DepotAvailabilityDate);
			AssertEquals(sailing2.JX_DepotStorageDate, sailing1.JX_DepotStorageDate);
			AssertEquals(sailing2.Origin.JA_Berth, sailing1.Origin.JA_Berth);
			AssertEquals(sailing2.Origin.JA_DepartReference, sailing1.Origin.JA_DepartReference);
			AssertEquals(sailing2.Origin.JA_DocumentaryCutoff, sailing1.Origin.JA_DocumentaryCutoff);
			AssertEquals(sailing2.Origin.JA_VGMCutOff, sailing1.Origin.JA_VGMCutOff);
			AssertEquals(sailing2.Origin.JA_CutOff, sailing1.Origin.JA_CutOff);
			AssertEquals(sailing2.Origin.JA_ReceivalCommences, sailing1.Origin.JA_ReceivalCommences);
			AssertEquals(sailing2.Destination.JB_Berth, sailing1.Destination.JB_Berth);
			AssertEquals(sailing2.Destination.JB_ArrivalReference, sailing1.Destination.JB_ArrivalReference);
			AssertEquals(sailing2.Destination.JB_AvailabilityDate, sailing1.Destination.JB_AvailabilityDate);
			AssertEquals(sailing2.Destination.JB_StorageDate, sailing1.Destination.JB_StorageDate);
			AssertEquals(sailing2.Origin.JA_EmptyReceivalCommences, sailing1.Origin.JA_EmptyReceivalCommences);
			AssertEquals(sailing2.Origin.JA_EmptyCutOff, sailing1.Origin.JA_EmptyCutOff);
			AssertEquals(sailing2.Origin.JA_ReeferReceivalCommences, sailing1.Origin.JA_ReeferReceivalCommences);
			AssertEquals(sailing2.Origin.JA_ReeferCutOff, sailing1.Origin.JA_ReeferCutOff);

			Enterprise.Registry.Business.SystemDataRegistry.Instance.UpdateSailingSchedulesDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			reader = new TransportLegDataObjectReader(transportLegDataObject2, logger, Factory, shipmentBO);
			transportBO2 = reader.ReadIntoBusinessObject();

			sailing2 = transportBO2.Sailing;
			AssertTransportSailingCorrectlyImported(transportLegDataObject2, transportBO2);
			AssertEquals(sailing2.JX_DepotAvailabilityDate, transportLegDataObject2.LCLAvailability);
			AssertEquals(sailing2.JX_DepotStorageDate, transportLegDataObject2.LCLStorageDate);
			AssertEquals(sailing2.Origin.JA_Berth, transportLegDataObject2.DepartureBerth);
			AssertEquals(sailing2.Origin.JA_DepartReference, transportLegDataObject2.DepartureReference);
			AssertEquals(sailing2.Origin.JA_DocumentaryCutoff, transportLegDataObject2.DocumentCutOff);
			AssertEquals(sailing2.Origin.JA_VGMCutOff, transportLegDataObject2.VGMCutOff);
			AssertEquals(sailing2.Origin.JA_CutOff, transportLegDataObject2.FCLCutOff);
			AssertEquals(sailing2.Origin.JA_ReceivalCommences, transportLegDataObject2.FCLReceivalCommences);
			AssertEquals(sailing2.Destination.JB_Berth, transportLegDataObject2.ArrivalBerth);
			AssertEquals(sailing2.Destination.JB_ArrivalReference, transportLegDataObject2.ArrivalReference);
			AssertEquals(sailing2.Destination.JB_AvailabilityDate, transportLegDataObject2.FCLAvailability);
			AssertEquals(sailing2.Destination.JB_StorageDate, transportLegDataObject2.FCLStorage);
			AssertEquals(sailing2.Origin.JA_EmptyReceivalCommences, transportLegDataObject2.EmptyReceivalCommences);
			AssertEquals(sailing2.Origin.JA_EmptyCutOff, transportLegDataObject2.EmptyCutOff);
			AssertEquals(sailing2.Origin.JA_ReeferReceivalCommences, transportLegDataObject2.ReeferReceivalCommences);
			AssertEquals(sailing2.Origin.JA_ReeferCutOff, transportLegDataObject2.ReeferCutOff);
		}

		public void TestShouldNotUpdateSailingScheduleInShipping()
		{
			var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var booking = (CommonShipment)Factory.New(ObjectFactory.GetType<IAgencyBooking>());

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "RAINBOW";
			transportLegDataObject.Carrier.OrganizationCode = carrier.OH_Code;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, booking);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNull("No sailing is created for booking transport legs", transportBO.Sailing);

			var billOfLading = (CommonShipment)Factory.New(ObjectFactory.GetType<IBillOfLading>());

			transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "BLUESTAR";
			transportLegDataObject.Carrier.OrganizationCode = carrier.OH_Code;

			reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, billOfLading);
			transportBO = reader.ReadIntoBusinessObject();

			AssertNull("No sailing is created for bill of lading transport legs", transportBO.Sailing);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "ROTOITI";
			transportLegDataObject.Carrier.OrganizationCode = carrier.OH_Code;

			reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull("Sailing is created for forwarding transport legs", transportBO.Sailing);
			AssertEquals("Carrier is as per XML", carrier.OH_Code, transportBO.Sailing.Line.OH_Code);
		}

		void AssertTransportSailingCorrectlyImported(TransportLeg dataObject, Transport transport)
		{
			var sailing = transport.Sailing;

			AssertNotNull("transport is linked to a sailing", sailing);

			AssertEquals(sailing.Origin.JA_E_DEP, dataObject.EstimatedDeparture);
			AssertEquals(sailing.Origin.JA_A_DEP, dataObject.ActualDeparture);
			AssertEquals(sailing.Origin.JA_EmptyReceivalCommences, dataObject.EmptyReceivalCommences);
			AssertEquals(sailing.Origin.JA_EmptyCutOff, dataObject.EmptyCutOff);
			AssertEquals(sailing.Origin.JA_ReeferReceivalCommences, dataObject.ReeferReceivalCommences);
			AssertEquals(sailing.Origin.JA_ReeferCutOff, dataObject.ReeferCutOff);
			AssertEquals(sailing.Origin.JA_RL_NKPortOfLoading, dataObject.PortOfLoading.Code);
			AssertEquals(sailing.Destination.JB_E_ARV, dataObject.EstimatedArrival);
			AssertEquals(sailing.Destination.JB_A_ARV, dataObject.ActualArrival);
			AssertEquals(sailing.Destination.JB_RL_NKPortOfDischarge, dataObject.PortOfDischarge.Code);
		}

		public void TestArrivalAndDepartureLocation_Linked()
		{
			var shipment1 = GetNewShipmentBO();
			var transportLegDataObject = SetupTransportLegToTestLinkedSailing();
			transportLegDataObject.DepartureCTO = CreateOrganizationAddress("1 DEPARTURECTO ST", "DEPCTO");
			transportLegDataObject.ArrivalCTO = CreateOrganizationAddress("2 ARRIVALCTO ST", "ARVCTO");
			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment1);
			var transport1 = reader.ReadIntoBusinessObject();

			AssertNull(transport1.DepartureLocation);
			AssertNull(transport1.ArrivalLocation);
			AssertEquals("1 DEPARTURECTO ST", transport1.Sailing.Origin.DepartureCTOAddress.OA_Address1);
			AssertEquals("2 ARRIVALCTO ST", transport1.Sailing.Destination.ArrivalCTOAddress.OA_Address1);

			var shipment2 = GetNewShipmentBO();
			transportLegDataObject.DepartureFrom = CreateOrganizationAddress("3 DEPARTUREFROM ST", "DEPART");
			transportLegDataObject.ArrivalAt = CreateOrganizationAddress("4 ARRIVALAT ST", "ARRIVE");
			Factory.SaveForTesting();

			var reader2 = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment2);
			var transport2 = reader2.ReadIntoBusinessObject();

			AssertEquals("3 DEPARTUREFROM ST", transport2.DepartureLocation.OA_Address1);
			AssertEquals("4 ARRIVALAT ST", transport2.ArrivalLocation.OA_Address1);
			AssertEquals("3 DEPARTUREFROM ST", transport2.Sailing.Origin.DepartureCTOAddress.OA_Address1);
			AssertEquals("4 ARRIVALAT ST", transport2.Sailing.Destination.ArrivalCTOAddress.OA_Address1);
		}

		public void TestArrivalAndDepartureLocation_NotLinked()
		{
			var shipment = GetNewShipmentBO();
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.DepartureFrom = CreateOrganizationAddress("1 DEPARTUREFROM ST", "DEPART");
			transportLegDataObject.ArrivalAt = CreateOrganizationAddress("2 ARRIVALAT ST", "ARRIVE");
			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transport = reader.ReadIntoBusinessObject();

			AssertEquals("1 DEPARTUREFROM ST", transport.DepartureLocation.OA_Address1);
			AssertEquals("2 ARRIVALAT ST", transport.ArrivalLocation.OA_Address1);
			AssertNull(transport.Sailing);
		}

		public void TestCarrierFallbackToShippingLineOrgFromParentTransportSupporter()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var vesselWithoutCarrier = Factory.BOFactory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_OH, null));
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.Carrier = null;
			transportLegDataObject.VesselName = vesselWithoutCarrier.RV_Name;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transport = reader.ReadIntoBusinessObject();

			Assert("Expected not to be linked as we cannot determine the carrier", !transport.JW_IsLinked);
			Assert("Expected no sailing to be created and linked to the transport", transport.JW_JX.IsEmpty);

			var carrier = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			transport = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to default carrier from the consol's ShippingLine", transport.Carrier, carrier);
			Assert("Expected to be linked as this vessel has a default carrier", transport.JW_IsLinked);
			Assert("Expected a sailing to be created from linked transport", !transport.JW_JX.IsEmpty);
		}

		public void TestMatchCarrierUsingCarrierCodeRegistration()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var carrier = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER1";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("CCC", "NUM1", "US");

			Factory.SaveForTesting();

			var vesselWithoutCarrier = Factory.BOFactory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_OH, null));
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = vesselWithoutCarrier.RV_Code;
			transportLegDataObject.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType() { Code = "CCC" },
					CountryOfIssue = new Country() { Code = "US" },
					Value = "NUM1"
				}
			});

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transport = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to default carrier from the consol's ShippingLine", transport.Carrier, carrier);
			Assert("Expected to be linked as this vessel has a default carrier", transport.JW_IsLinked);
			Assert("Expected a sailing to be created from linked transport", !transport.JW_JX.IsEmpty);
		}

		public void TestSeaTransport_DoNotCreateSailing_WhenRefVesselDoesNotExist()
		{
			var etd = ZDateTime.Today;
			var eta = etd.AddDays(2);
			var consol = SetupConsolWithVoyage("NZAKL", "AUMEL", etd, eta);

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "DUUUUUUUMB";
			transportLegDataObject.VoyageFlightNo = "DOODOO";

			var carrierAddressForSetup = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("Fooey");
			var carrierAddressBoToLoad = new OrganisationDataObjectReader(carrierAddressForSetup, logger, Factory).GetMatchedOrNewForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);
			AssertEquals("transportBO.CarrierAddress", carrierAddressBoToLoad.PK, transportBO.CarrierAddress.PK);
			AssertNull("No new sailing is created", transportBO.Sailing);
			AssertEquals("JW_IsLinked should be false", false, transportBO.JW_IsLinked);
		}

		public void TestDoNotImportVesselForAirTransport()
		{
			var consol = Factory.New<CommonConsol>();
			SetTransportMode(consol, Core.Constants.TransportModes.Air);

			var existingVoyage = Factory.New<JobVoyage>();
			existingVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			existingVoyage.JV_VoyageFlight = "QF409";
			existingVoyage.JV_IsCargoOnly = true;
			existingVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			existingVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			existingVoyage.Destinations[0].JB_E_ARV = new ZDateTime(2011, 3, 6).AddHours(7);
			existingVoyage.GenerateSailings();

			Factory.SaveForTesting();

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "AIRCRAFT NAME";
			transportLegDataObject.TransportMode = TransportMode.Air;
			transportLegDataObject.VoyageFlightNo = "QF409";

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();
			Assert(transportBO.JW_IsLinked);
			AssertEquals("Linked to existing sailing", existingVoyage.Sailings[0].PK, transportBO.JW_JX);
			AssertEquals("Do not import vessel name for AIR", "", transportBO.JW_Vessel);
			AssertEquals("Not imported to voyage either", "", existingVoyage.JV_RV_NKVessel);
		}

		public void TestVoyageIsNotLinkedForUnmatchedCarrier()
		{
			var etd = ZDateTime.Today;
			var eta = etd.AddDays(2);
			var consol = SetupConsolWithVoyage("NZAKL", "AUMEL", etd, eta);

			var existingSailingPK = consol.JK_JX_Sailing;

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.Carrier.OrganizationCode = "ZAZAZAZA";
			transportLegDataObject.VesselName = "ROTOITI";

			AssertNull("Precondition: carrier org doesn't exist", Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, transportLegDataObject.Carrier.OrganizationCode.Value));

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);
			AssertEquals("JW_IsLinked should be false", false, transportBO.JW_IsLinked);
		}

		public void TestNewVoyageIsCreatedWhereCarrierDoesntMatchExistingVoyageCarrier()
		{
			var etd = ZDateTime.Today;
			var eta = etd.AddDays(2);
			var consol = SetupConsolWithVoyage("NZAKL", "AUMEL", etd, eta);

			var existingSailingPK = consol.JK_JX_Sailing;

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.Carrier.OrganizationCode = "SEASHI";
			transportLegDataObject.VesselName = "ROTOITI";

			AssertNotNull("Precondition: carrier org exists", Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, transportLegDataObject.Carrier.OrganizationCode.Value));
			AssertNotEquals("Precondition: the carrier org is not the one on the existing voyage", transportLegDataObject.Carrier.OrganizationCode, consol.Transports[0].Voyage.Line.OH_Code);

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);
			AssertNotNull("Sailing is created", transportBO.Sailing);
			AssertEquals("JW_IsLinked should be true", true, transportBO.JW_IsLinked);
			AssertNotEquals("Sailing is not the existing one", existingSailingPK, transportBO.JW_JX);
			AssertEquals("Carrier from XML is used", "SEASHI", transportBO.Voyage.Line.OH_Code);
		}

		public void TestNewlyCreatedTransportIsLinkedToNewScheduleIfCarrierDoesntMatch()
		{
			var etdAKL = ZDateTime.Today;
			var etaMEL = etdAKL.AddDays(2);
			var etdMEL = etdAKL.AddDays(3);
			var etaSIN = etdAKL.AddDays(4);
			var consol = SetupConsolWithVoyage("NZAKL", "AUMEL", etdAKL, etaMEL);
			consol.JK_RL_NKDischargePort = "SGSIN";

			var existingVoyageCarrier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "NEPORI");

			var existingVoyage = Factory.New<JobVoyage>();
			existingVoyage.JV_AirSeaRoad = "SEA";
			var vessel = RefVessel.LookupVesselByName("ARAFURA", existingVoyage.Factory).First();
			existingVoyage.JV_RV_NKVessel = vessel.RV_FK;
			existingVoyage.JV_VoyageFlight = "444S";
			existingVoyage.JV_OH_Line = existingVoyageCarrier.PK;

			var origin = existingVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUMEL";
			origin.JA_E_DEP = etdMEL;

			var destination = existingVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = etaSIN;

			existingVoyage.GenerateSailings();
			var existingSailing = existingVoyage.Sailings[0];
			Factory.SaveForTesting();

			var existingSailingPK = existingSailing.PK;

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.Carrier.OrganizationCode = "SEASHI";
			transportLegDataObject.VesselName = "ARAFURA";
			transportLegDataObject.VoyageFlightNo = "444S";
			transportLegDataObject.PortOfLoading.Code = "AUMEL";
			transportLegDataObject.PortOfDischarge.Code = "SGSIN";
			transportLegDataObject.EstimatedDeparture = etdMEL;
			transportLegDataObject.EstimatedArrival = etaSIN;

			AssertNotNull("Precondition: carrier org exists", Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, transportLegDataObject.Carrier.OrganizationCode.Value));
			AssertNotEquals("Precondition: the carrier org is not the one on the existing voyage", transportLegDataObject.Carrier.OrganizationCode, existingVoyage.Line.OH_Code);
			AssertEquals("Precondition: the Vessel matches existing voyage", existingVoyage.JV_RV_NKVessel, transportLegDataObject.VesselName);
			AssertEquals("Precondition: the Voyage Number matches existing voyage", existingVoyage.JV_VoyageFlight, transportLegDataObject.VoyageFlightNo);
			AssertEquals("Precondition: the Load ports match", existingVoyage.Origins[0].JA_RL_NKPortOfLoading, transportLegDataObject.PortOfLoading.Code);
			AssertEquals("Precondition: the Discharge ports match", existingVoyage.Destinations[0].JB_RL_NKPortOfDischarge, transportLegDataObject.PortOfDischarge.Code);
			AssertEquals("Precondition: the ETDs match", existingVoyage.Origins[0].JA_E_DEP, transportLegDataObject.EstimatedDeparture);
			AssertEquals("Precondition: the ETAs match", existingVoyage.Destinations[0].JB_E_ARV, transportLegDataObject.EstimatedArrival);

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);

			AssertNotNull("Sailing is created", transportBO.Sailing);
			AssertEquals("JW_IsLinked should be true", true, transportBO.JW_IsLinked);
			AssertNotEquals("Sailing is not the existing one", existingSailingPK, transportBO.JW_JX);
			AssertEquals("Carrier from XML is used", "SEASHI", transportBO.Voyage.Line.OH_Code);
		}

		public void TestGetDatesIfSavedTransportLegIsNotLinked()
		{
			var transportLegDataObject = SetupTransportLeg();

			var shipment = GetNewShipmentBO();
			SetTransportMode(shipment, Core.Constants.TransportModes.Sea);

			var attachedTransport = GetTransportCollection(shipment).AddNew();
			attachedTransport.JW_IsLinked = false;
			attachedTransport.JW_LegOrder = (ZByte)transportLegDataObject.LegOrder;
			attachedTransport.JW_TransportMode = null;
			attachedTransport.JW_ATA = (ZDateTime)transportLegDataObject.ActualArrival;
			attachedTransport.JW_ATD = (ZDateTime)transportLegDataObject.ActualDeparture;
			attachedTransport.JW_CarrierBookingReference = (ZString)transportLegDataObject.CarrierBookingReference;
			attachedTransport.JW_ETA = (ZDateTime)transportLegDataObject.EstimatedArrival;
			attachedTransport.JW_ETD = (ZDateTime)transportLegDataObject.EstimatedDeparture;
			attachedTransport.JW_TransportType = new LegTypeConverter().FromEnumValue(transportLegDataObject.LegType);
			attachedTransport.JW_RL_NKDiscPort = "AUMEL";
			attachedTransport.JW_RL_NKLoadPort = "NZAKL";
			attachedTransport.JW_VoyageFlight = (ZString)transportLegDataObject.VoyageFlightNo;

			transportLegDataObject.VesselName = "ROTOITI";
			transportLegDataObject.Carrier = null;
			transportLegDataObject.LCLAvailability = new ZDateTime(2011, 2, 3);
			transportLegDataObject.LCLCutOff = new ZDateTime(2011, 2, 2);
			transportLegDataObject.LCLReceivalCommences = new ZDateTime(2011, 2, 1);
			transportLegDataObject.LCLStorageDate = new ZDateTime(2011, 2, 4);

			transportLegDataObject.DocumentCutOff = new ZDateTime(2011, 2, 9);
			transportLegDataObject.VGMCutOff = new ZDateTime(2011, 2, 10);

			transportLegDataObject.FCLAvailability = new ZDateTime(2011, 2, 7);
			transportLegDataObject.FCLCutOff = new ZDateTime(2011, 2, 6);
			transportLegDataObject.FCLReceivalCommences = new ZDateTime(2011, 2, 5);
			transportLegDataObject.FCLStorage = new ZDateTime(2011, 2, 8);

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertEquals("", false, transportBO.JW_IsLinked);

			AssertEquals("transportBO.JW_TerminalReceivalCommences", new ZDateTime(2011, 2, 5), transportBO.JW_TerminalReceivalCommences);
			AssertEquals("transportBO.JW_TerminalCutOff", new ZDateTime(2011, 2, 6), transportBO.JW_TerminalCutOff);
			AssertEquals("transportBO.JW_TerminalAvailabilityDate", new ZDateTime(2011, 2, 7), transportBO.JW_TerminalAvailabilityDate);
			AssertEquals("transportBO.JW_TerminalStorageDate", new ZDateTime(2011, 2, 8), transportBO.JW_TerminalStorageDate);

			AssertEquals("transportBO.JW_DocumentaryCutOff", new ZDateTime(2011, 2, 9), transportBO.JW_DocumentaryCutOff);
			AssertEquals("transportBO.JW_VGMCutOff", new ZDateTime(2011, 2, 10), transportBO.JW_VGMCutOff);

			AssertEquals("transportBO.JW_DepotReceivalCommences", new ZDateTime(2011, 2, 1), transportBO.JW_DepotReceivalCommences);
			AssertEquals("transportBO.JW_DepotCutOff", new ZDateTime(2011, 2, 2), transportBO.JW_DepotCutOff);
			AssertEquals("transportBO.JW_DepotAvailabilityDate", new ZDateTime(2011, 2, 3), transportBO.JW_DepotAvailabilityDate);
			AssertEquals("transportBO.JW_DepotStorageDate", new ZDateTime(2011, 2, 4), transportBO.JW_DepotStorageDate);
		}

		public void TestBookingStatus()
		{
			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.TransportMode = TransportMode.Air;
			transportLegDataObject.VesselName = "FAKE";
			transportLegDataObject.BookingStatus = new CodeDescriptionPair() { Code = "QUE" };

			var agencyShipment = Factory.BOFactory.New<IAgencyShipment>();
			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, agencyShipment as ITransportParentCommon);
			reader.ReadIntoBusinessObject();

			var transportBO = reader.ReadIntoBusinessObject();
			AssertEquals(transportLegDataObject.BookingStatus.Code, transportBO.JW_Status);
		}

		public void TestSaveBusinessObjectWhenSailingNotGenerated()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.EnableOrphanedVoyageReportingForTests = true;
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_FlightDate = new ZDateTime(2011, 3, 4);
			voyage.JV_VoyageFlight = "343L";

			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(TransportMode.Air, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "NZAKL", Name = "Auckland" });
			transportLegDataObject.VoyageFlightNo = "343L";

			var shipment = GetNewShipmentBO();
			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);
			AssertNull(transportBO.Sailing);

			AssertEquals("transportBO.JW_VoyageFlight", "343L", transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_Vessel", ZString.Empty, transportBO.JW_Vessel);

			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "AIR", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "NZAKL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_IsLinked", false, transportBO.JW_IsLinked);
			AssertEquals("transportBO.JW_IsCargoOnly", true, transportBO.JW_IsCargoOnly);
			AssertEquals("transportBO.JW_OA_CreditorAddress", ZGuid.Empty, transportBO.JW_OA_CreditorAddress);

			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("logger.Logs", @"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: NZAKL
Information - Attempting to get Schedule for the Transport Leg
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Sailing is not generated. The Load (NZAKL) and Discharge (NZAKL) cannot be the same for AIR voyage.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.", logger.Logs);
			});
		}

		public void TestJW_OA_CreditorAddressShouldNotBeRepopulatedByJW_OA_CarrierAddress()
		{
			var carrier = Factory.NewWithValidTestData<OrgAddress>();
			carrier.Header.OH_IsShippingLine = true;
			carrier.Header.OH_IsShippingProvider = true;
			carrier.Header.OH_IsCreditor = true;
			carrier.Header.OH_Code = "YOUAUS_AU";

			var creditor = Factory.NewWithValidTestData<OrgAddress>();
			creditor.Header.OH_IsShippingLine = true;
			creditor.Header.OH_IsShippingProvider = true;
			creditor.Header.OH_IsCreditor = true;
			creditor.Header.OH_Code = "AUAGENSYD";

			var consol = SetupConsolWithVoyage("NZAKL", "AUMEL", new ZDateTime(2011, 3, 4), new ZDateTime(2011, 3, 6));
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = "ROTOITI";

			transportLegDataObject.Carrier = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("Carrier");
			transportLegDataObject.Carrier.OrganizationCode = "YOUAUS_AU";

			transportLegDataObject.Creditor = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("Creditor");
			transportLegDataObject.Creditor.OrganizationCode = "AUAGENSYD";

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertEquals("TransportBO JW_OA_CarrierAddress should be consistent with USXML.", "YOUAUS_AU", transportBO.Carrier.OH_Code);
				AssertEquals("TransportBO JW_OA_CreditorAddress should be consistent with USXML", "AUAGENSYD", transportBO.Creditor.OH_Code);

				AssertMultilineASCIIEquals("logger.Logs",
					@"Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Matching 'Carrier':- Matched to 'YOUAUS_AU' by code, main address used.
Information - Matching 'Creditor':- Matched to 'AUAGENSYD' by code, main address used.
Information - Transport Leg updated.
".Trim(), logger.Logs);
			});
		}

		public void TestVoyageIsNotLinkedIfDateComninationIsInvalid()
		{
			var today = ZDateTime.Today;
			var etd = today.AddDays(1);
			var eta = today.AddDays(3);
			var consol = SetupConsolWithVoyage("NZAKL", "AUMEL", etd, eta);
			var voyage = consol.Voyage;

			var transportLegDataObject = SetupTransportLeg();
			transportLegDataObject.VesselName = consol.Voyage.Vessel.RV_Code;
			transportLegDataObject.VoyageFlightNo = consol.Voyage.JV_VoyageFlight;
			transportLegDataObject.PortOfLoading.Code = "NZAKL";
			transportLegDataObject.PortOfDischarge.Code = "AUMEL";
			transportLegDataObject.EstimatedDeparture = today;
			transportLegDataObject.EstimatedArrival = today.AddDays(-5);
			transportLegDataObject.Carrier = null;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);
			AssertEquals("JW_IsLinked should be false", false, transportBO.JW_IsLinked);
			AssertNull("Voyage should be null", consol.Voyage);
			AssertEquals("JA_E_DEP should has no change", voyage.Origins[0].JA_E_DEP, today.AddDays(1));
			AssertEquals("JB_E_ARV should has no change", voyage.Destinations[0].JB_E_ARV, today.AddDays(3));
		}

		public void TestJWTransportTypeWillNotChangeIfLegtypeISNULL()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var transportLegDataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transportLegDataObject.LegType = UniversalDataBuss.DataObjects.Universal.LegType.Flight3;

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, consol);
			var transportBO = reader.ReadIntoBusinessObject();

			AssertNotNull(transportBO);
			AssertEquals("JW_TransportType is FL3 when LegType is Flight3", "FL3", transportBO.JW_TransportType);

			transportLegDataObject.LegType = null;
			AssertEquals("JW_TransportType will not change when LegType is null", "FL3", transportBO.JW_TransportType);
		}

		OrganizationAddress CreateOrganizationAddress(ZString address, ZString orgCode)
		{
			var result = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			result.Address1 = address;
			result.OrganizationCode = orgCode;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.MainAddress.OA_Address1 = address;

			return result;
		}

		#region AdditionalTransportModes

		public void TestPopulateAdditionalTransportModes_RailWithRoad()
		{
			AssertPopulateAdditionalTransportModes(TransportMode.Rail, TransportMode.Road, new TransportModeConverter().FromEnumValue(TransportMode.Road),
				@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.");
		}
		public void TestPopulateAdditionalTransportModes_InlandWaterwayTransportWithRoad()
		{
			AssertPopulateAdditionalTransportModes(TransportMode.InlandWaterway, TransportMode.Road, new TransportModeConverter().FromEnumValue(TransportMode.Road),
				@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - No schedule for Inland Waterway Transport. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.");
		}

		public void TestPopulateAdditionalTransportModes_InlandWaterwayTransportWithRail()
		{
			AssertPopulateAdditionalTransportModes(TransportMode.InlandWaterway, TransportMode.Rail, new TransportModeConverter().FromEnumValue(TransportMode.Rail),
				@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - No schedule for Inland Waterway Transport. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
");
		}

		public void TestPopulateAdditionalTransportModes_Invalid()
		{
			AssertPopulateAdditionalTransportModes(TransportMode.Road, TransportMode.Sea, ZString.Empty,
				@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Invalid additional transport mode SEA is found.
Information - Transport Leg updated.");
		}

		public void TestPopulateAdditionalTransportModes_NotCapable()
		{
			AssertPopulateAdditionalTransportModes(TransportMode.Rail, TransportMode.Rail, ZString.Empty,
				@"Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Additional transport mode RAI is not applicable for RAI transport mode.
Information - Transport Leg updated.");
		}

		public void TestPopulateAdditionalTransportModes_Multiple()
		{
			AssertPopulateAdditionalTransportModes(TransportMode.Sea, TransportMode.Air, ZString.Empty, ZString.Empty, true);
		}

		void AssertPopulateAdditionalTransportModes(TransportMode transportMode, TransportMode additionalTransportMode, ZString expectedMode, ZString expectedLog, bool multipleAdditionalModes = false)
		{
			var transportLegDataObject = SetupTransportLegWithoutVoyageAndVessel(transportMode, new ZDateTime(2011, 3, 6), new ZDateTime(2011, 3, 4), new UNLOCO() { Code = "NZAKL", Name = "Auckland" }, new UNLOCO() { Code = "AUMEL", Name = "Melbourne" });
			var shipment = GetNewShipmentBO();

			transportLegDataObject.LegType = Enterprise.UniversalDataBuss.DataObjects.Universal.LegType.OnForwarding;
			var additionalTransportModes = new List<AdditionalTransportMode>();
			additionalTransportModes.Add(new AdditionalTransportMode { TransportMode = additionalTransportMode });
			if (multipleAdditionalModes)
			{
				additionalTransportModes.Add(new AdditionalTransportMode { TransportMode = additionalTransportMode });
			}
			transportLegDataObject.SetAdditionalTransportModeCollection(() => additionalTransportModes);

			Factory.SaveForTesting();

			var reader = new TransportLegDataObjectReader(transportLegDataObject, logger, Factory, shipment);

			if (multipleAdditionalModes)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("", () => reader.ReadIntoBusinessObject());
			}
			else
			{
				var transportBO = reader.ReadIntoBusinessObject();

				CombineAssertions(delegate
				{
					AssertNotNull(transportBO);
					AssertEquals(expectedMode, transportBO.JW_AdditionalTransportMode);
					AssertMultilineASCIIEquals(expectedLog.Trim(), logger.Logs);
				});
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			SetUpDefaultCarrierForVessel();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		void SetUpDefaultCarrierForVessel()
		{
			var carrier = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;

			var vessel = RefVessel.LookupVesselByName("ROTOITI", Factory.BOFactory).FirstOrDefault();
			vessel.RV_OH = carrier.PK;
		}

		public static TransportLeg SetupTransportLeg()
		{
			var transportLegDataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);

			transportLegDataObject.LegOrder = 1;
			transportLegDataObject.TransportMode = TransportMode.Sea;
			transportLegDataObject.ActualArrival = new ZDateTime(2011, 3, 7);
			transportLegDataObject.ActualDeparture = new ZDateTime(2011, 3, 5);
			transportLegDataObject.CarrierBookingReference = "BOOKMEUP";
			transportLegDataObject.CarrierServiceLevel = new ServiceLevel() { Code = "STD", Description = "Standard" };
			transportLegDataObject.EstimatedArrival = new ZDateTime(2011, 3, 6);
			transportLegDataObject.EstimatedDeparture = new ZDateTime(2011, 3, 4);
			transportLegDataObject.LegType = Enterprise.UniversalDataBuss.DataObjects.Universal.LegType.Main;
			transportLegDataObject.LegNotes = "Test Leg Notes";
			transportLegDataObject.PortOfDischarge = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };
			transportLegDataObject.PortOfLoading = new UNLOCO() { Code = "NZAKL", Name = "Auckland" };
			transportLegDataObject.VoyageFlightNo = "343L";
			transportLegDataObject.AircraftType = new CodeDescriptionPair() { Code = "E90" };
			transportLegDataObject.Carrier = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("Fooey");
			transportLegDataObject.Creditor = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD("CreditorSYD");
			transportLegDataObject.IsCargoOnly = true;
			transportLegDataObject.EmptyReceivalCommences = new ZDateTime(1952, 1, 25);
			transportLegDataObject.EmptyCutOff = new ZDateTime(1952, 1, 26);
			transportLegDataObject.ReeferReceivalCommences = new ZDateTime(1952, 1, 27);
			transportLegDataObject.ReeferCutOff = new ZDateTime(1952, 1, 28);
			transportLegDataObject.HazzardReceivalCommences = new ZDateTime(1952, 1, 29);
			transportLegDataObject.HazzardCutOffDate = new ZDateTime(1952, 1, 30);

			return transportLegDataObject;
		}

		public static TransportLeg SetupTransportLegToTestLinkedSailing()
		{
			var transportLegDataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);

			transportLegDataObject.LegOrder = 1;
			transportLegDataObject.TransportMode = TransportMode.Sea;
			transportLegDataObject.CarrierBookingReference = "BOOKMEUP";
			transportLegDataObject.CarrierServiceLevel = new ServiceLevel() { Code = "STD", Description = "Standard" };
			transportLegDataObject.LegType = Enterprise.UniversalDataBuss.DataObjects.Universal.LegType.Main;
			transportLegDataObject.PortOfDischarge = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" };
			transportLegDataObject.PortOfLoading = new UNLOCO() { Code = "CAGLT", Name = "Galt" };
			transportLegDataObject.VoyageFlightNo = "343L";
			transportLegDataObject.VesselName = "ROTOITI";
			transportLegDataObject.PortOfDischarge = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };
			transportLegDataObject.PortOfLoading = new UNLOCO() { Code = "NZAKL", Name = "Auckland" };
			transportLegDataObject.ActualArrival = new ZDateTime(2011, 4, 7);
			transportLegDataObject.ActualDeparture = new ZDateTime(2011, 4, 5);
			transportLegDataObject.EstimatedArrival = new ZDateTime(2011, 4, 6);
			transportLegDataObject.EstimatedDeparture = new ZDateTime(2011, 4, 4);
			transportLegDataObject.LCLAvailability = new ZDateTime(1948, 1, 30);
			transportLegDataObject.LCLStorageDate = new ZDateTime(1949, 1, 30);
			transportLegDataObject.DepartureBerth = "DB";
			transportLegDataObject.DepartureReference = "D_REF";
			transportLegDataObject.DocumentCutOff = new ZDateTime(1950, 1, 30);
			transportLegDataObject.FCLCutOff = new ZDateTime(1951, 1, 30);
			transportLegDataObject.FCLReceivalCommences = new ZDateTime(1952, 1, 30);
			transportLegDataObject.EmptyReceivalCommences = new ZDateTime(1952, 1, 25);
			transportLegDataObject.EmptyCutOff = new ZDateTime(1952, 1, 26);
			transportLegDataObject.ReeferReceivalCommences = new ZDateTime(1952, 1, 27);
			transportLegDataObject.ReeferCutOff = new ZDateTime(1952, 1, 28);
			transportLegDataObject.VGMCutOff = new ZDateTime(1953, 1, 30);
			transportLegDataObject.ArrivalBerth = "AB";
			transportLegDataObject.ArrivalReference = "A_REF";
			transportLegDataObject.FCLAvailability = new ZDateTime(1953, 1, 30);
			transportLegDataObject.FCLStorage = new ZDateTime(1954, 1, 30);
			transportLegDataObject.IsCargoOnly = true;

			return transportLegDataObject;
		}

		public static void AssertContents(Transport transportBO)
		{
			AssertEquals("transportBO.JW_VoyageFlight", "343L", transportBO.JW_VoyageFlight);
			AssertEquals("transportBO.JW_AircraftType", "E90", transportBO.JW_AircraftType);
			AssertEquals("transportBO.JW_TransportMode", "SEA", transportBO.JW_TransportMode);
			AssertEquals("transportBO.JW_LegNotes", "Test Leg Notes", transportBO.JW_LegNotes);

			AssertEquals("transportBO.JW_RL_NKLoadPort", "NZAKL", transportBO.JW_RL_NKLoadPort);
			AssertEquals("transportBO.JW_ETD", new ZDateTime(2011, 3, 4), transportBO.JW_ETD);
			AssertEquals("transportBO.JW_ATD", new ZDateTime(2011, 3, 5), transportBO.JW_ATD);

			AssertEquals("transportBO.JW_RL_NKDiscPort", "AUMEL", transportBO.JW_RL_NKDiscPort);
			AssertEquals("transportBO.JW_ETA", new ZDateTime(2011, 3, 6), transportBO.JW_ETA);
			AssertEquals("transportBO.JW_ATA", new ZDateTime(2011, 3, 7), transportBO.JW_ATA);

			AssertEquals("transportBO.JW_CarrierBookingReference", "BOOKMEUP", transportBO.JW_CarrierBookingReference);
			AssertEquals("transportBO.JW_PL_NKCarrierServiceLevel", "STD", transportBO.JW_PL_NKCarrierServiceLevel);

			AssertEquals("transportBO.JW_EmptyReceivalCommences", new ZDateTime(1952, 1, 25), transportBO.JW_EmptyReceivalCommences);
			AssertEquals("transportBO.JW_EmptyCutOff", new ZDateTime(1952, 1, 26), transportBO.JW_EmptyCutOff);
			AssertEquals("transportBO.JW_ReeferReceivalCommences", new ZDateTime(1952, 1, 27), transportBO.JW_ReeferReceivalCommences);
			AssertEquals("transportBO.JW_ReeferCutOff", new ZDateTime(1952, 1, 28), transportBO.JW_ReeferCutOff);
			AssertEquals("transportBO.JW_DGReceivalCommences", new ZDateTime(1952, 1, 29), transportBO.JW_DGReceivalCommences);
			AssertEquals("transportBO.JW_DGCutOff", new ZDateTime(1952, 1, 30), transportBO.JW_DGCutOff);
		}

		static void AssertAddressContentMatches_INTHEMSYD(OrgAddress addressBO)
		{
			AssertEquals("addressBO.OA_Address1", "Unit 12, Level 3", addressBO.OA_Address1);
			AssertEquals("addressBO.OA_Address2", "233 Here St", addressBO.OA_Address2);
			AssertEquals("addressBO.OA_City", "ThereVille", addressBO.OA_City);
			AssertEquals("addressBO.OA_State", "OfBliss", addressBO.OA_State);
			AssertEquals("addressBO.OA_PostCode", "1233", addressBO.OA_PostCode);
			AssertEquals("addressBO.OA_RL_NKRelatedPortCode", "AU", addressBO.OA_RL_NKRelatedPortCode);
			AssertEquals("addressBO.OA_Phone", "1239813209", addressBO.OA_Phone);
			AssertEquals("addressBO.OA_Fax", "234098234", addressBO.OA_Fax);
			AssertEquals("addressBO.OA_Email", "s.m@moment.com.au", addressBO.OA_Email);
			AssertEquals("addressBO.OA_Mobile", "234098293", addressBO.OA_Mobile);
		}

		static void AssertOrgHeaderContents(OrgHeader organizationBO)
		{
			AssertAddressContentMatches_INTHEMSYD(organizationBO.MainAddress);
			AssertEquals("organizationBO.OH_Code", "INTHEMSYD", organizationBO.OH_Code);
			AssertEquals("organizationBO.OH_FullName", "In The Moment", organizationBO.OH_FullName);

			organizationBO.Contacts.Sort(OrgContact.Schema.OC_ContactName);
			var contacts = new List<string>();
			foreach (OrgContact contact in organizationBO.Contacts)
			{
				contacts.Add(string.Format("Name[{0}] Email[{1}] Phone[{2}] Fax[{3}] Mobile[{4}]", contact.OC_ContactName, contact.OC_Email, contact.OC_Phone, contact.OC_Fax, contact.OC_Mobile));
			}
			AssertMultilineASCIIEquals("organizationBO.Contacts", @"
Name[Starshine Moonbeam] Email[s.m@moment.com.au] Phone[1239813209] Fax[234098234] Mobile[234098293]
				".Trim(), string.Join("\r\n", contacts.ToArray()));

			organizationBO.CustomsCodes.Sort(OrgCusCode.Schema.OK_CodeType);
			var registrationNumbers = new List<string>();
			foreach (OrgCusCode customsCode in organizationBO.CustomsCodes)
			{
				registrationNumbers.Add(string.Format("Country[{0}] Type[{1}] Number[{2}] Address[{3}]", customsCode.OK_RN_NKCodeCountry, customsCode.OK_CodeType, customsCode.OK_CustomsRegNo, customsCode.PremisesAddress == null ? ZString.Empty : customsCode.PremisesAddress.OA_Address2));
			}
			AssertMultilineASCIIEquals("organizationBO.CustomsCodes", @"
Country[NZ] Type[ATF] Number[1234F] Address[233 Here St]
Country[AU] Type[GST] Number[55555] Address[]
Country[AU] Type[UNC] Number[545] Address[]
Country[AU] Type[UOC] Number[454] Address[]
				".Trim(), string.Join("\r\n", registrationNumbers.ToArray()));
		}

		protected virtual ITransportParentCommon GetNewShipmentBO()
		{
			return Factory.New<CommonShipment>();
		}

		TransportCollection GetTransportCollection(ITransportParentCommon shipmentBO)
		{
			return shipmentBO is CommonShipment ? (shipmentBO as CommonShipment).Transports : new TransportCollection(shipmentBO);
		}

		void SetTransportMode(ITransportParentCommon shipmentBO, string mode)
		{
			if (shipmentBO is CommonShipment)
			{
				var shipment = shipmentBO as CommonShipment;
				shipment.JS_TransportMode = mode;
			}
		}

		#endregion
	}
}
