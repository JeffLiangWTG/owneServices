using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business.Public.Interfaces;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class JobVoyageShipmentDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestRailSchedulesCannotBeImported()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("RAI", (ICodeDescriptionPairList)null);

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var voyage = readerToTest.ReadIntoBusinessObject();

			AssertContains("Rail Schedules cannot be imported", logger.Logs);
			AssertNull(voyage);
		}

		public void TestSailingsAreUnpublishedIfNotInTransportLegCollection()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("SEA", (ICodeDescriptionPairList)null);
			dataObject.VoyageFlightNo = "AAA1";

			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg1.TransportMode = TransportMode.Sea;
			leg1.PortOfLoading = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			leg1.PortOfDischarge = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" };

			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg2.TransportMode = TransportMode.Sea;
			leg2.PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" };
			leg2.PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" };

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			dataObject.TransportLegCollection.Add(leg1);
			dataObject.TransportLegCollection.Add(leg2);

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var voyage = readerToTest.ReadIntoBusinessObject();

			AssertEquals(3, voyage.Sailings.Count);

			var sailing1 = voyage.Sailings[0];
			AssertEquals("AUSYD->AUBNE", string.Format("{0}->{1}", sailing1.Origin.JA_RL_NKPortOfLoading, sailing1.Destination.JB_RL_NKPortOfDischarge));
			Assert("Is Published", sailing1.JX_IsPublished);

			var sailing2 = voyage.Sailings[1];
			AssertEquals("AUBNE->JPOSA", string.Format("{0}->{1}", sailing2.Origin.JA_RL_NKPortOfLoading, sailing2.Destination.JB_RL_NKPortOfDischarge));
			Assert("Is Published", sailing2.JX_IsPublished);

			var sailing3 = voyage.Sailings[2];
			AssertEquals("AUSYD->JPOSA", string.Format("{0}->{1}", sailing3.Origin.JA_RL_NKPortOfLoading, sailing3.Destination.JB_RL_NKPortOfDischarge));
			Assert("Not Published as not explicitly in TransportLegCollection", !sailing3.JX_IsPublished);
		}

		public void TestSailingsNotInCollectionAreMarkedAsUnpublishedIfReferenced()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "SEA";
			voyage.JV_VoyageFlight = "K48";

			var referencedSailing = voyage.Sailings.AddNew();
			var origin1 = Factory.New<VoyageOrigin>();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_JV = voyage.PK;
			var dest1 = Factory.New<VoyageDestination>();
			dest1.JB_RL_NKPortOfDischarge = "AUBNE";
			dest1.JB_JV = voyage.PK;
			referencedSailing.JX_JA = origin1.PK;
			referencedSailing.JX_JB = dest1.PK;

			var unreferencedSailing = voyage.Sailings.AddNew();
			var origin2 = Factory.New<VoyageOrigin>();
			origin2.JA_RL_NKPortOfLoading = "AUBNE";
			origin2.JA_JV = voyage.PK;
			var dest2 = Factory.New<VoyageDestination>();
			dest2.JB_RL_NKPortOfDischarge = "AUPER";
			dest2.JB_JV = voyage.PK;
			unreferencedSailing.JX_JA = origin2.PK;
			unreferencedSailing.JX_JB = dest2.PK;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_JX = referencedSailing.PK;

			Factory.SaveForTesting();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("SEA", (ICodeDescriptionPairList)null);
			dataObject.VoyageFlightNo = "K48";

			var leg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg.TransportMode = TransportMode.Sea;
			leg.PortOfLoading = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };
			leg.PortOfDischarge = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" };

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { leg }));

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var importedVoyage = readerToTest.ReadIntoBusinessObject();

			AssertEquals("Voyage is matched", voyage.PK, importedVoyage.PK);

			var sailing1 = importedVoyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "AUMEL" && x.JX_JB_RL_NKPortOfDischarge == "AUBNE");
			Assert("Sailing is published", sailing1.JX_IsPublished);

			var sailing2 = importedVoyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "AUBNE");
			Assert("Sailing is not published", !sailing2.JX_IsPublished);

			var sailing3 = importedVoyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "AUBNE" && x.JX_JB_RL_NKPortOfDischarge == "AUPER");
			AssertNull("Sailing is removed as it is not in the collection and is not referenced", sailing3);

			Assert("Factory should not be read-only", !(importedVoyage.Factory is ReadOnlyBusinessObjectFactory));
		}

		public void TestVoyageLevelFieldsMustMatchCorrespondingFieldsAtShipmentLevel_TransportMode()
		{
			var expectedError = "The Transport Modes of all Transport Legs must match the Transport Mode 'AIR' of the Schedule.";

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("AIR", (ICodeDescriptionPairList)null);

			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg1.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg1.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg2.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg2.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var leg3 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg3.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg3.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			dataObject.TransportLegCollection.Add(leg1);
			dataObject.TransportLegCollection.Add(leg2);
			dataObject.TransportLegCollection.Add(leg3);

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Transport legs do not define Transport Mode", expectedError, logger.Logs);
			AssertEquals("AIR", voyage.JV_AirSeaRoad);

			logger.ClearLogs();

			leg1.TransportMode = TransportMode.Air;

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Transport legs match Transport Mode where defined", expectedError, logger.Logs);
			AssertEquals("AIR", voyage.JV_AirSeaRoad);

			logger.ClearLogs();

			leg2.TransportMode = TransportMode.Air;
			leg3.TransportMode = TransportMode.Air;

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("All Transport legs Transport Modes match", expectedError, logger.Logs);
			AssertEquals("AIR", voyage.JV_AirSeaRoad);

			logger.ClearLogs();

			leg3.TransportMode = TransportMode.Sea;

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertContains("At least one Transport leg's Transport Mode does not match Shipment transport mode", expectedError, logger.Logs);
			AssertNull(voyage);

			logger.ClearLogs();

			leg3.TransportMode = null;

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Transport mode of legs matches where it has been entered", expectedError, logger.Logs);
			AssertEquals("AIR", voyage.JV_AirSeaRoad);
		}

		public void TestVoyageLevelFieldsMustMatchCorrespondingFieldsAtShipmentLevel_Vessel()
		{
			var expectedVesselNameError = "The Vessel Name of all Transport Legs must match the Vessel Name 'Boaty McBoatface' of the Schedule.";
			var expectedLloydsIMOError = "The Lloyds IMO of all Transport Legs must match the Lloyds IMO 'BOATY' of the Schedule.";
			var expectedVoyageFlightNoError = "The Voyage / Flight No of all Transport Legs must match the Voyage / Flight No 'B01' of the Schedule.";

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("SEA", (ICodeDescriptionPairList)null);
			dataObject.VesselName = "Boaty McBoatface";
			dataObject.LloydsIMO = "BOATY";
			dataObject.VoyageFlightNo = "B01";

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotNull(voyage);
			AssertEquals("Data object contains no transport legs", 0, voyage.Sailings.Count);

			logger.ClearLogs();

			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg1.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg1.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg2.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg2.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var leg3 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg3.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg3.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			dataObject.TransportLegCollection.Add(leg1);
			dataObject.TransportLegCollection.Add(leg2);
			dataObject.TransportLegCollection.Add(leg3);

			readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Transport legs do not define Vessel fields", expectedVesselNameError, logger.Logs);
			AssertNotContains("Transport legs do not define Vessel fields", expectedLloydsIMOError, logger.Logs);
			AssertNotContains("Transport legs do not define Vessel fields", expectedVoyageFlightNoError, logger.Logs);
			AssertNotNull(voyage);

			logger.ClearLogs();

			leg1.VesselName = "Boaty McBoatface";
			leg2.VesselLloydsIMO = "BOATY";
			leg3.VoyageFlightNo = "B01";

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Transport leg Vessel fields match where defined", expectedVesselNameError, logger.Logs);
			AssertNotContains("Transport leg Vessel fields match where defined", expectedLloydsIMOError, logger.Logs);
			AssertNotContains("Transport leg Vessel fields match where defined", expectedVoyageFlightNoError, logger.Logs);
			AssertNotNull(voyage);

			logger.ClearLogs();

			leg2.VesselName = "Ferry McFerryface";

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertContains(expectedVesselNameError, logger.Logs);
			AssertNull(voyage);

			logger.ClearLogs();

			leg2.VesselName = "Boaty McBoatface";
			leg2.VoyageFlightNo = "F01";

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertContains(expectedVoyageFlightNoError, logger.Logs);
			AssertNull(voyage);

			logger.ClearLogs();

			leg2.VoyageFlightNo = "B01";
			leg3.VesselLloydsIMO = "FERRY";

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertContains(expectedLloydsIMOError, logger.Logs);
			AssertNull(voyage);
		}

		public void TestVoyageLevelFieldsMustMatchCorrespondingFieldsAtShipmentLevel_Carrier()
		{
			var expectedError = "The Carrier of all Transport Legs must match the Carrier 'SWISHP - Swiss Shipping' of the Schedule.";

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("SEA", (ICodeDescriptionPairList)null);

			var carrier1Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			carrier1Address.OrganizationCode = "SWISHP";
			carrier1Address.CompanyName = "Swiss Shipping";
			carrier1Address.AddressType = nameof(DocAddressType.Carrier);

			var carrier2Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			carrier2Address.OrganizationCode = "AUSSHP";
			carrier2Address.CompanyName = "Austrian Shipping";
			carrier2Address.AddressType = nameof(DocAddressType.Carrier);

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			dataObject.OrganizationAddressCollection.Add(carrier1Address);

			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg1.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg1.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg2.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg2.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var leg3 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg3.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg3.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			dataObject.TransportLegCollection.Add(leg1);
			dataObject.TransportLegCollection.Add(leg2);
			dataObject.TransportLegCollection.Add(leg3);

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Transport legs do not define Carrier", expectedError, logger.Logs);
			AssertNotNull(voyage);

			logger.ClearLogs();

			leg2.Carrier = carrier1Address;

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Carrier matches", expectedError, logger.Logs);
			AssertNotNull(voyage);

			logger.ClearLogs();

			leg3.Carrier = carrier2Address;

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertContains("Carrier does not match", expectedError, logger.Logs);
			AssertNull(voyage);
		}

		public void TestCarrierMustMatchOnAllTransportLegs_OrgCode()
		{
			var expectedError = "The Carrier of all Transport Legs must match the Carrier 'SWISHP - Swiss Shipping' of the Schedule.";

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "SWISHP";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "AUSTSHP";

			Factory.SaveForTesting();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("SEA", (ICodeDescriptionPairList)null);

			var carrier1Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			carrier1Address.OrganizationCode = "SWISHP";
			carrier1Address.CompanyName = "Swiss Shipping";
			carrier1Address.AddressType = nameof(DocAddressType.Carrier);

			var carrier2Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			carrier2Address.OrganizationCode = "AUSTSHP";
			carrier2Address.CompanyName = "Austrian Shipping";
			carrier2Address.AddressType = nameof(DocAddressType.Carrier);

			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg1.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg1.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			leg1.Carrier = carrier1Address;

			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg2.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg2.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			leg2.Carrier = carrier1Address;

			var leg3 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg3.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg3.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			dataObject.TransportLegCollection.Add(leg1);
			dataObject.TransportLegCollection.Add(leg2);
			dataObject.TransportLegCollection.Add(leg3);

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Carrier matches where defined", expectedError, logger.Logs);
			AssertNotNull(voyage);
			AssertEquals("Carrier code matches", "SWISHP", voyage.Line.OH_Code);

			leg3.Carrier = carrier2Address;

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertContains("Carrier does not match", expectedError, logger.Logs);
			AssertNull(voyage);
		}

		public void TestCarrierMustMatchOnAllTransportLegs_SCAC()
		{
			var expectedError = "The Carrier SCAC of all Transport Legs must match the Carrier SCAC '123' of the Schedule.";

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var code1 = carrier1.CustomsCodes.AddNew();
			code1.OK_CodeType = "CCC";
			code1.OK_RN_NKCodeCountry = "US";
			code1.OK_CustomsRegNo = "123";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var code2 = carrier2.CustomsCodes.AddNew();
			code2.OK_CodeType = "CCC";
			code2.OK_RN_NKCodeCountry = "US";
			code2.OK_CustomsRegNo = "456";

			Factory.SaveForTesting();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("SEA", (ICodeDescriptionPairList)null);

			var carrier1Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			carrier1Address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = "CCC" },
					CountryOfIssue = new Country { Code = "US" },
					Value = "123"
				}
			});

			var carrier2Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			carrier2Address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType { Code = "CCC" },
					CountryOfIssue = new Country { Code = "US" },
					Value = "456"
				}
			});

			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg1.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg1.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			leg1.Carrier = carrier1Address;

			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg2.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg2.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };
			leg2.Carrier = carrier1Address;

			var leg3 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg3.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg3.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			dataObject.TransportLegCollection.Add(leg1);
			dataObject.TransportLegCollection.Add(leg2);
			dataObject.TransportLegCollection.Add(leg3);

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Carrier matches where defined", expectedError, logger.Logs);
			AssertNotNull(voyage);
			AssertEquals("Carrier matches", carrier1.PK, voyage.JV_OH_Line);

			leg3.Carrier = carrier2Address;

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertContains("Carrier does not match", expectedError, logger.Logs);
			AssertNull(voyage);
		}

		public void TestVoyageLevelFieldsMustMatchCorrespondingFieldsAtShipmentLevel_FlightDetails()
		{
			var expectedIsCargoOnlyError = "The Is Cargo Only flag on all Transport Legs must be the same.";
			var expectedAircraftTypeError = "The Aircraft Type on all Transport Legs must be the same.";

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("AIR", (ICodeDescriptionPairList)null);

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotNull(voyage);
			AssertEquals("Data object does not define transport legs", 0, voyage.Sailings.Count);

			logger.ClearLogs();

			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg1.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg1.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg2.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg2.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			var leg3 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg3.PortOfDischarge = new UNLOCO { Code = "NZAKL", Name = "Auckland" };
			leg3.PortOfLoading = new UNLOCO { Code = "AUSYD", Name = "Sydney" };

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			dataObject.TransportLegCollection.Add(leg1);
			dataObject.TransportLegCollection.Add(leg2);
			dataObject.TransportLegCollection.Add(leg3);

			readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Transport legs do not define Flight detail fields", expectedIsCargoOnlyError, logger.Logs);
			AssertNotContains("Transport legs do not define Flight detail fields", expectedAircraftTypeError, logger.Logs);
			AssertNotNull(voyage);

			logger.ClearLogs();

			leg1.IsCargoOnly = true;
			leg2.AircraftType = ListHelper.GetWithDescription<CodeDescriptionPair>("747", (ICodeDescriptionPairList)null);
			leg2.AircraftType = ListHelper.GetWithDescription<CodeDescriptionPair>("747", (ICodeDescriptionPairList)null);

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertNotContains("Transport leg Flight Details fields match where defined", expectedIsCargoOnlyError, logger.Logs);
			AssertNotContains("Transport leg Flight Details fields match where defined", expectedIsCargoOnlyError, logger.Logs);
			AssertNotNull(voyage);

			logger.ClearLogs();

			leg2.IsCargoOnly = false;

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertContains(expectedIsCargoOnlyError, logger.Logs);
			AssertNull(voyage);

			logger.ClearLogs();

			leg2.IsCargoOnly = true;
			leg3.AircraftType = ListHelper.GetWithDescription<CodeDescriptionPair>("787", (ICodeDescriptionPairList)null);

			voyage = readerToTest.ReadIntoBusinessObject();
			AssertContains(expectedAircraftTypeError, logger.Logs);
			AssertNull(voyage);
		}

		public void TestImportWithMultipleTransportLegsCreatesMultipleSailings()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("SEA", (ICodeDescriptionPairList)null);
			dataObject.VesselName = "Boaty McBoatface";
			dataObject.LloydsIMO = "BOATY";
			dataObject.VoyageFlightNo = "B01";

			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			var leg3 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			var leg4 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);

			leg1.PortOfLoading = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			leg1.PortOfDischarge = new UNLOCO() { Code = "FJSUV", Name = "Suva" };
			leg1.EstimatedDeparture = ZDate.Today.AddDays(1);

			leg2.PortOfLoading = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			leg2.PortOfDischarge = new UNLOCO() { Code = "NZAKL", Name = "Aukland" };
			leg2.EstimatedDeparture = ZDate.Today.AddDays(1);

			leg3.PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" };
			leg3.PortOfDischarge = new UNLOCO() { Code = "FJSUV", Name = "Suva" };
			leg3.EstimatedDeparture = ZDate.Today.AddDays(3);

			leg4.PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" };
			leg4.PortOfDischarge = new UNLOCO() { Code = "NZAKL", Name = "Aukland" };
			leg4.EstimatedDeparture = ZDate.Today.AddDays(3);

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			dataObject.TransportLegCollection.Add(leg1);
			dataObject.TransportLegCollection.Add(leg2);
			dataObject.TransportLegCollection.Add(leg3);
			dataObject.TransportLegCollection.Add(leg4);

			var readerToTest = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			var voyage = readerToTest.ReadIntoBusinessObject();

			AssertContainsExactElementsInAnyOrder("2 origins expected", new[] { "AUSYD", "AUBNE" }, voyage.Origins.Cast<VoyageOrigin>().Select(x => x.JA_RL_NKPortOfLoading));
			AssertContainsExactElementsInAnyOrder("2 destinations expected", new[] { "FJSUV", "NZAKL" }, voyage.Destinations.Cast<VoyageDestination>().Select(x => x.JB_RL_NKPortOfDischarge));
			AssertContainsExactElementsInAnyOrder("4 sailings expected", new[] { "AUSYD->FJSUV", "AUSYD->NZAKL", "AUBNE->FJSUV", "AUBNE->NZAKL" }, voyage.Sailings.Cast<JobSailing>().Select(x => x.JX_JA_RL_NKPortOfLoading + "->" + x.JX_JB_RL_NKPortOfDischarge));
		}

		public void TestWhenImportingVoyageThenSailingsAreRegeneratedOnce()
		{
			var messageText = GetMessageFromEmbeddedResource(
				"Enterprise.Freight.DataTransfer.Test.Freight.Universal.JobVoyage.TestFiles.JobVoyage_UPS_UniversalShipment.xml");

			var message = GetQueuedUniversalShipmentMessage(messageText);
			var factory = new UniversalObjectFactory();
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(factory, serviceTaskLog);
			manager.Process(message);

			var importedVoyageQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "32");
			importedVoyageQuery.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, "ROA");
			importedVoyageQuery.FetchOnlyFromLocalCache = true;

			var voyage = factory.BOFactory.LoadTop1<JobVoyage>(importedVoyageQuery);

			AssertEquals($"{nameof(JobVoyage.GenerateSailings)} was called once during import",
				1, voyage.GenerateSailingsCountForTesting);
		}

		public void TestWhenImportingSameVoyageXmlTwiceThenVoyageOriginDestinationSailingsAreNotSavedTheSecondTime()
		{
			var collector = new SavedObjectsCollector();

			using (ObjectFactory.Substitute<IFactoryProcessingExtension>(collector))
			{
				var messageText = GetMessageFromEmbeddedResource(
					"Enterprise.Freight.DataTransfer.Test.Freight.Universal.JobVoyage.TestFiles.JobVoyage_UPS_UniversalShipment.xml");

				void ProcessUPSMessage(UniversalObjectFactory factory)
				{
					var message = GetQueuedUniversalShipmentMessage(messageText);
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var manager = new UniversalMessageProcessingManager(factory, serviceTaskLog);
					manager.Process(message);
				}

				var factory1 = new UniversalObjectFactory();
				ProcessUPSMessage(factory1);

				var savedVoyageBizObjs = GetSavedVoyageBusinessObjects(collector, factory1.BOFactory);
				AssertEquals("Saved: JobVoyage x1, JobVoyOrigin x 1, JobVoyDestination x 1, JobSailing x 1",
					4, savedVoyageBizObjs.Length);

				var factory2 = new UniversalObjectFactory();
				ProcessUPSMessage(factory2);

				var savedVoyageBizObjs2 = GetSavedVoyageBusinessObjects(collector, factory2.BOFactory);
				AssertEquals("No JobVoyage, JobVoyOrigin, JobVoyDestination and JobSailing needed to be saved",
					0, savedVoyageBizObjs2.Length);
			}
		}

		sealed class SavedObjectsCollector : IFactoryProcessingExtension
		{
			public IReadOnlyDictionary<long, BusinessObject[]> SavedBusinessObjects => savedBusinessObjects;
			readonly Dictionary<long, BusinessObject[]> savedBusinessObjects = new Dictionary<long, BusinessObject[]>();

			public void OnFactoryBusinessObjectsSaved(BusinessObjectFactory factory, List<BusinessObject> businessObjects)
			{
				if (factory != null
					&& businessObjects != null)
				{
					savedBusinessObjects[factory._Instance] = businessObjects.ToArray();
				}
			}
		}

		BusinessObject[] GetSavedVoyageBusinessObjects(SavedObjectsCollector collector, BusinessObjectFactory factory)
		{
			if (collector.SavedBusinessObjects.TryGetValue(factory._Instance, out var bizObjs))
			{
				return bizObjs.Where(bizObj => bizObj is JobVoyage
					|| bizObj is VoyageOrigin
					|| bizObj is VoyageDestination
					|| bizObj is JobSailing)
					.ToArray();
			}

			return Array.Empty<BusinessObject>();
		}

		string GetMessageFromEmbeddedResource(string resourceName)
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				return resourceRetriever.GetString(resourceName);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		#endregion
	}
}
