using System;
using System.IO;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class FreightValueObjectDataAdapterTest : BaseFreightTest
	{
		#region TestImportNewDestinationDontOverwriteExistingOrigin

		public void TestImportNewDestinationDontOverwriteExistingOrigin()
		{
			ZDateTime now = ZDateTime.Now;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "06660";
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;

			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = HomePort;
			origin1.JA_E_DEP = now.AddDays(10);

			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = OverseasPort;
			destination1.JB_E_ARV = now.AddDays(20);

			voyage.GenerateSailings();
			Factory.Save();

			ConsolTransportCollection transports = Factory.New<CommonConsol>().Transports;

			Xsd.PlannedLegCollection plannedLegs = new Xsd.PlannedLegCollection();

			Xsd.PlannedLeg leg = plannedLegs.AddNew();
			leg.TransportType = Xsd.PlannedLegTransportType.MainVessel;
			leg.TransportMode = Xsd.TransportMode.SEA;
			leg.Item = CreateSailing(voyage.JV_RV_NKVessel, voyage.JV_VoyageFlight);
			leg.PortOfLoading.Port = Xsd.UNLOCO.FromPort(origin1.PortOfLoading);
			leg.PortOfLoading.EstimatedDateTime = ZDateTime.MinSmallDateTimeValue;
			leg.PortOfDischarge.Port = Xsd.UNLOCO.FromPort(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, OverseasPort2));
			leg.PortOfDischarge.EstimatedDateTime = ZDateTime.MaxSmallDateTime;

			ImportPlannedLegs(transports, plannedLegs, new PsudoNotificationSubscriber());
			Transport transport = transports[0];

			AssertNotNull("Transport should have a sailing", transport.Sailing);
			AssertEquals("Should be using the existing voyage", voyage.PK, transport.Sailing.Voyage.PK);
			AssertEquals("Should be using the existing origin", origin1.PK, transport.Sailing.JX_JA);
			AssertNotEquals("Should not be using the existing destination", destination1.PK, transport.Sailing.JX_JB);

			AssertEquals("Should not have overwritten the ETD", now.AddDays(10), transport.Sailing.Origin.JA_E_DEP);
			AssertEquals("Should have set the ETA", ZDateTime.MaxSmallDateTime, transport.Sailing.Destination.JB_E_ARV);
		}

		#endregion

		#region TestImportNewOriginDontOverwriteExistingDestination

		public void TestImportNewOriginDontOverwriteExistingDestination()
		{
			ZDateTime now = ZDateTime.Now;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "06660";
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;

			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = HomePort;
			origin1.JA_E_DEP = now.AddDays(10);

			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = OverseasPort;
			destination1.JB_E_ARV = now.AddDays(20);

			voyage.GenerateSailings();
			Factory.Save();

			ConsolTransportCollection transports = Factory.New<CommonConsol>().Transports;

			Xsd.PlannedLegCollection plannedLegs = new Xsd.PlannedLegCollection();

			Xsd.PlannedLeg leg = plannedLegs.AddNew();
			leg.TransportType = Xsd.PlannedLegTransportType.MainVessel;
			leg.TransportMode = Xsd.TransportMode.SEA;
			leg.Item = CreateSailing(voyage.JV_RV_NKVessel, voyage.JV_VoyageFlight);
			leg.PortOfLoading.Port = Xsd.UNLOCO.FromPort(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, AlternateHomePort));
			leg.PortOfLoading.EstimatedDateTime = ZDateTime.MinSmallDateTimeValue;
			leg.PortOfDischarge.Port = Xsd.UNLOCO.FromPort(destination1.PortOfDischarge);
			leg.PortOfDischarge.EstimatedDateTime = ZDateTime.MaxSmallDateTime;

			ImportPlannedLegs(transports, plannedLegs, new PsudoNotificationSubscriber());
			Transport transport = transports[0];

			AssertNotNull("Transport should have a sailing", transport.Sailing);
			AssertEquals("Should be using the existing voyage", voyage.PK, transport.Sailing.Voyage.PK);
			AssertNotEquals("Should not be using the existing origin", origin1.PK, transport.Sailing.JX_JA);
			AssertEquals("Should be using the existing destination", destination1.PK, transport.Sailing.JX_JB);

			AssertEquals("Should have set the ETD", ZDateTime.MinSmallDateTimeValue, transport.Sailing.Origin.JA_E_DEP);
			AssertEquals("Should not have overwritten the ETA", now.AddDays(20), transport.Sailing.Destination.JB_E_ARV);
		}

		#endregion

		#region TestShouldMatchSailingsUsingMappedCodes

		public void TestShouldMatchSailingsUsingMappedCodes()
		{
			ZDateTime now = ZDateTime.Now;
			string fakePortCode1 = "TORONTO";
			string fakePortCode2 = "SYDNEY";
			string realPortCode1 = "CATOR";
			string realPortCode2 = "AUSYD";

			OrgHeader orgWithMappings = Factory.NewWithValidTestData<OrgHeader>();

			OrgPatternMatchOverride override1 = orgWithMappings.CreatePatternMatchOverrideForTest();
			override1.OO_ForeignCode = fakePortCode1;
			override1.OO_Relationship = "PTC";
			override1.OO_LocalGuid = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, realPortCode1).PK;

			OrgPatternMatchOverride override2 = orgWithMappings.CreatePatternMatchOverrideForTest();
			override2.OO_ForeignCode = fakePortCode2;
			override2.OO_Relationship = "PTC";
			override2.OO_LocalGuid = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, realPortCode2).PK;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "Random";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = realPortCode1;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = realPortCode2;
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			Factory.Save();

			Xsd.PlannedLegCollection collection = new Xsd.PlannedLegCollection();

			Xsd.PlannedLeg leg = collection.AddNew();
			leg.Item = CreateSailing(voyage.JV_RV_NKVessel, voyage.JV_VoyageFlight);
			leg.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(Factory, fakePortCode1, ZDateTime.Empty, ZDateTime.Empty);
			leg.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(Factory, fakePortCode2, ZDateTime.Empty, ZDateTime.Empty);
			((Xsd.Sailing)leg.Item).DocCutOffDate = now;

			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.EDICode = orgWithMappings.OH_Code;

			INotifications notify = new NotificationBuffer();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, notify);

			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = sailing.JX_JA_RL_NKPortOfLoading;
			transport.JW_RL_NKDiscPort = sailing.JX_JB_RL_NKPortOfDischarge;

			FreightValueObjectDataAdapterTestClass adapter = new FreightValueObjectDataAdapterTestClass();
			adapter.ImportPlannedLegs(consol.Transports, collection, context, "blah");

			AssertEquals("consol should have 1 transport", 1, consol.Transports.Count);
			AssertEquals("docs cut off", now, sailing.JX_JA_DocumentaryCutoff);
			AssertEquals("existing transport matches, should not be deleted", false, transport.IsDeleted);
			AssertEquals("consol should still have the same transport", transport, consol.Transports[0]);
		}

		#endregion

		#region TestMaximumLengthOfVoyageFlightAndVesselName

		public void TestMaximumLengthOfVoyageFlight()
		{
			TestMaximumLength(Xsd.TransportMode.AIR);
		}
		public void TestMaximumLengthOfVesselName()
		{
			TestMaximumLength(Xsd.TransportMode.SEA);
		}

		void TestMaximumLength(Xsd.TransportMode transportMode)
		{
			Xsd.PlannedLegCollection plannedLegs = new Xsd.PlannedLegCollection();
			Xsd.PlannedLeg plannedLeg = plannedLegs.AddNew();
			plannedLeg.TransportMode = transportMode;
			plannedLeg.TransportType = Xsd.PlannedLegTransportType.Flight1;
			plannedLeg.TransportTypeSpecified = true;
			ZDateTime eTA = new ZDateTime(2005, 11, 22, 10, 58, 21);
			ZDateTime aTA = new ZDateTime(2005, 11, 22, 10, 59, 33);
			ZDateTime eTD = new ZDateTime(2005, 11, 22, 11, 00, 00);
			ZDateTime aTD = new ZDateTime(2005, 11, 22, 11, 01, 12);

			plannedLeg.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(Factory, "USLAX", eTD, aTD);
			plannedLeg.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(Factory, "AUSYD", eTA, aTA);
			if (transportMode == Xsd.TransportMode.AIR)
			{
				plannedLeg.Item = CreateFlight("1234567890123456789012345678901234567890");
			}
			else if (transportMode == Xsd.TransportMode.SEA)
			{
				plannedLeg.Item = CreateSailing("1234567890123456789012345678901234567890", "235425", "3987253");
			}

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			FreightValueObjectDataAdapterTestClass adapater = new FreightValueObjectDataAdapterTestClass();

			adapater.ImportPlannedLegs(Consol.Transports, plannedLegs, context, "");
			AssertEquals("Should be 1 Transport in the Transport Colection", 1, Consol.Transports.Count);
			Transport transport = Consol.Transports[0];
			if (transportMode == Xsd.TransportMode.AIR)
			{
				AssertEquals("Flight Number", "1234567890", transport.JW_VoyageFlight);
			}
			else if (transportMode == Xsd.TransportMode.SEA)
			{
				AssertEquals("vessel Name", "12345678901234567890123456789012345", transport.JW_Vessel);
			}
		}

		#endregion

		#region TestUpdateTransports

		public void TestUpdateConsolTransports()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			GenericUpdateTransportsTest(consol.Transports);
			GenericUpdatePartialTransportTest(consol.Transports);
			GenericUpdatePartialPlanningTest(consol.Transports);
		}

		public void TestUpdateShipmentTransports()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			GenericUpdateTransportsTest(shipment.Transports);
			GenericUpdatePartialTransportTest(shipment.Transports);
			GenericUpdatePartialPlanningTest(shipment.Transports);
		}

		void GenericUpdatePartialTransportTest(TransportCollection transports)
		{
			transports.RemoveAndDeleteAll();

			Transport transport1 = (transports.Count == 0 ? transports.AddNew() : transports[0]);
			transport1.JW_IsLinked = false;
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "";
			transport1.JW_RL_NKDiscPort = "NLAMS";

			Transport transport2 = transports.AddNew();
			transport2.JW_IsLinked = false;
			transport2.JW_RL_NKLoadPort = "";
			transport2.JW_RL_NKDiscPort = "";

			Transport transport3 = transports.AddNew();
			transport3.JW_IsLinked = false;
			transport3.JW_RL_NKLoadPort = "AUBNE";
			transport3.JW_RL_NKDiscPort = "";

			Xsd.PlannedLegCollection plannedLegs = new Xsd.PlannedLegCollection();

			Xsd.PlannedLeg plannedLeg1 = plannedLegs.AddNew();
			plannedLeg1.TransportType = Xsd.PlannedLegTransportType.MainVessel;
			plannedLeg1.TransportMode = Xsd.TransportMode.SEA;
			plannedLeg1.Item = CreateSailing(TestVessel1.RV_Name, "Blah");
			plannedLeg1.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUBNE");
			plannedLeg1.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "SGSIN");

			Xsd.PlannedLeg plannedLeg2 = plannedLegs.AddNew();
			plannedLeg2.TransportType = Xsd.PlannedLegTransportType.OnForwarding;
			plannedLeg2.TransportMode = Xsd.TransportMode.SEA;
			plannedLeg2.Item = CreateSailing(TestVessel2.RV_Name, "Blaticus");
			plannedLeg2.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "SGSIN");
			plannedLeg2.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "GBLON");

			Xsd.PlannedLeg plannedLeg3 = plannedLegs.AddNew();
			plannedLeg3.TransportType = Xsd.PlannedLegTransportType.Other;
			plannedLeg3.TransportMode = Xsd.TransportMode.SEA;
			plannedLeg3.Item = CreateSailing(TestVessel2.RV_Name, "SNTH");
			plannedLeg3.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "GBLON");
			plannedLeg3.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "NLAMS");

			ImportPlannedLegs(transports, plannedLegs, new NotificationBuffer());

			AssertEquals("transport1: load port", "GBLON", transport1.JW_RL_NKLoadPort);
			AssertEquals("transport1: discharge port", "NLAMS", transport1.JW_RL_NKDiscPort);

			AssertEquals("transport2: had no ports and so could not be matched", true, transport2.IsDeleted);

			AssertEquals("transport3: load port", "AUBNE", transport3.JW_RL_NKLoadPort);
			AssertEquals("transport3: discharge port", "SGSIN", transport3.JW_RL_NKDiscPort);

			Transport transport4 = FindTransport(transports, "SGSIN", "GBLON");
			AssertEquals("transport4: load port", "SGSIN", transport4.JW_RL_NKLoadPort);
			AssertEquals("transport4: discharge port", "GBLON", transport4.JW_RL_NKDiscPort);
		}

		void GenericUpdatePartialPlanningTest(TransportCollection transports)
		{
			transports.RemoveAndDeleteAll();

			Transport transport1 = (transports.Count == 0 ? transports.AddNew() : transports[0]);
			transport1.JW_IsLinked = false;
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "SGSIN";

			Transport transport2 = transports.AddNew();
			transport2.JW_IsLinked = false;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "GBLON";

			Transport transport3 = transports.AddNew();
			transport3.JW_IsLinked = false;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_RL_NKLoadPort = "GBLON";
			transport3.JW_RL_NKDiscPort = "NLAMS";

			Xsd.PlannedLegCollection plannedLegs = new Xsd.PlannedLegCollection();

			Xsd.PlannedLeg plannedLeg2 = plannedLegs.AddNew();
			plannedLeg2.TransportType = Xsd.PlannedLegTransportType.OnForwarding;
			plannedLeg2.TransportMode = Xsd.TransportMode.SEA;
			plannedLeg2.Item = CreateSailing(TestVessel2.RV_Name, "Blaticus");
			plannedLeg2.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "");
			plannedLeg2.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "");

			Xsd.PlannedLeg plannedLeg1 = plannedLegs.AddNew();
			plannedLeg1.TransportType = Xsd.PlannedLegTransportType.MainVessel;
			plannedLeg1.TransportMode = Xsd.TransportMode.SEA;
			plannedLeg1.Item = CreateSailing(TestVessel1.RV_Name, "Blah");
			plannedLeg1.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUBNE");
			plannedLeg1.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "");

			Xsd.PlannedLeg plannedLeg3 = plannedLegs.AddNew();
			plannedLeg3.TransportType = Xsd.PlannedLegTransportType.Other;
			plannedLeg3.TransportMode = Xsd.TransportMode.SEA;
			plannedLeg3.Item = CreateSailing(TestVessel2.RV_Name, "SNTH");
			plannedLeg3.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "");
			plannedLeg3.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "NLAMS");

			ImportPlannedLegs(transports, plannedLegs, new NotificationBuffer());

			AssertContainsExactElementsInAnyOrder("No new legs should have been added, No existing legs should have been removed",
				new BusinessObject[] { transport1, transport2, transport3 },
				transports);

			AssertEquals("transport1: load port", "AUBNE", transport1.JW_RL_NKLoadPort);
			AssertEquals("transport1: discharge port", "SGSIN", transport1.JW_RL_NKDiscPort);

			AssertEquals("transport2: load port", "SGSIN", transport2.JW_RL_NKLoadPort);
			AssertEquals("transport2: discharge port", "GBLON", transport2.JW_RL_NKDiscPort);

			AssertEquals("transport3: load port", "GBLON", transport3.JW_RL_NKLoadPort);
			AssertEquals("transport3: discharge port", "NLAMS", transport3.JW_RL_NKDiscPort);
		}

		void GenericUpdateTransportsTest(TransportCollection transports)
		{
			transports.RemoveAndDeleteAll();

			Transport transport1 = (transports.Count == 0 ? transports.AddNew() : transports[0]);
			transport1.JW_IsLinked = false;
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_VoyageFlight = "QF1234";
			transport1.JW_RL_NKLoadPort = "AUCNS";
			transport1.JW_RL_NKDiscPort = "AUBNE";
			transport1.JW_ETD = ZDateTime.Now;

			Transport transport2 = transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_Vessel = TestVessel1.RV_FK;
			transport2.JW_VoyageFlight = "Blah";
			transport2.CarrierPK = Factory.New<OrgHeader>().PK;
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_CarrierBookingReference = "BookingRef";

			Transport transport3 = transports.AddNew();
			transport3.JW_IsLinked = false;
			transport3.JW_TransportMode = Constants.TransportModes.Rail;
			transport3.JW_RL_NKLoadPort = "SGSIN";
			transport3.JW_RL_NKDiscPort = "GBLON";

			var carrier = Factory.New<OrgHeader>();
			var existingVoyageToBeMatchedByPlannedLeg4 = new VoyageTestHelper(Factory).CreateSeaVoyage(TestVessel2.RV_Name, "voyage2", carrier.PK);

			Xsd.PlannedLegCollection plannedLegs = new Xsd.PlannedLegCollection();

			Xsd.PlannedLeg plannedLeg2 = plannedLegs.AddNew();
			plannedLeg2.TransportType = Xsd.PlannedLegTransportType.MainVessel;
			plannedLeg2.TransportMode = Xsd.TransportMode.SEA;
			plannedLeg2.Item = CreateSailing(TestVessel1.RV_Name, "Blah");
			plannedLeg2.PortOfLoading.Port = Xsd.UNLOCO.FromPort(transport2.LoadPort);
			plannedLeg2.PortOfDischarge.Port = Xsd.UNLOCO.FromPort(transport2.DiscPort);

			Xsd.PlannedLeg plannedLeg3 = plannedLegs.AddNew();
			plannedLeg3.TransportMode = Xsd.TransportMode.RAI;
			plannedLeg3.Item = CreateFlight("JourneyNo");
			plannedLeg3.PortOfLoading.Port = Xsd.UNLOCO.FromPort(transport3.LoadPort);
			plannedLeg3.PortOfDischarge.Port = Xsd.UNLOCO.FromPort(transport3.DiscPort);

			Xsd.PlannedLeg plannedLeg4 = plannedLegs.AddNew();
			plannedLeg4.TransportMode = Xsd.TransportMode.SEA;
			plannedLeg4.Item = CreateSailing(TestVessel2.RV_Name, "voyage2");
			plannedLeg4.PortOfLoading.Port = Xsd.UNLOCO.FromPort(transport3.DiscPort);
			plannedLeg4.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "NLAMS");

			Xsd.PlannedLeg plannedLeg5 = plannedLegs.AddNew();
			plannedLeg5.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "NLAMS");
			plannedLeg5.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "MYBAG");

			ImportPlannedLegs(transports, plannedLegs, new NotificationBuffer());

			AssertEquals("transport1 should have been deleted since it was not matched", true, transport1.IsDeleted);

			AssertEquals("transport2 should remain linked", true, transport2.JW_IsLinked);
			AssertEquals("transport2 should be sea", Constants.TransportModes.Sea, transport2.JW_TransportMode);
			AssertEquals("transport2 should have the same vessel", TestVessel1.RV_FK, transport2.JW_Vessel);
			AssertEquals("transport2 should have the same voyage no", "Blah", transport2.JW_VoyageFlight);
			AssertEquals("transport2 should have the same load port", "AUBNE", transport2.JW_RL_NKLoadPort);
			AssertEquals("transport2 should have the same discharge port", "SGSIN", transport2.JW_RL_NKDiscPort);
			AssertEquals("transport2 should have the same booking ref", "BookingRef", transport2.JW_CarrierBookingReference);

			AssertEquals("transprot3 should remain un-linked", false, transport3.JW_IsLinked);
			AssertEquals("transport3 should still be rail", Constants.TransportModes.Rail, transport3.JW_TransportMode);
			AssertEquals("transport3 should have a new journey number", "JourneyNo", transport3.JW_VoyageFlight);
			AssertEquals("transport3 should have the same load port", "SGSIN", transport3.JW_RL_NKLoadPort);
			AssertEquals("transport3 should have the same discharge port", "GBLON", transport3.JW_RL_NKDiscPort);

			Transport transport4 = FindTransport(transports, "GBLON", "NLAMS");
			AssertEquals("transport4 should be linked as it did have enough information", true, transport4.JW_IsLinked);
			AssertEquals("transport4 should be sea", Constants.TransportModes.Sea, transport4.JW_TransportMode);
			AssertEquals("transport4 vessel", TestVessel2.RV_FK, transport4.JW_Vessel);
			AssertEquals("transport4 voyage", "voyage2", transport4.JW_VoyageFlight);
			AssertEquals("transport4 load port", "GBLON", transport4.JW_RL_NKLoadPort);
			AssertEquals("transport4 discharge port", "NLAMS", transport4.JW_RL_NKDiscPort);

			Transport transport5 = FindTransport(transports, "NLAMS", "MYBAG");
			AssertEquals("transport5 should not be linked as it didnt have enough information", false, transport5.JW_IsLinked);
			AssertEquals("transport5 load port", "NLAMS", transport5.JW_RL_NKLoadPort);
			AssertEquals("transport5 discharge port", "MYBAG", transport5.JW_RL_NKDiscPort);
		}

		Transport FindTransport(TransportCollection collection, ZString load, ZString discharge)
		{
			foreach (Transport transport in collection)
			{
				if (transport.JW_RL_NKLoadPort == load && transport.JW_RL_NKDiscPort == discharge)
				{
					return transport;
				}
			}
			return null;
		}

		#endregion

		#region Tests

		public void TestDataImportsCorrectlyWithRegistryDefaultedToFalse()
		{
			AssertDataImportsCorrectlyWithRespectoToRegistryValue(false);
		}

		public void TestDataImportsCorrectlyWithRegistryDefaultedToTrue()
		{
			AssertDataImportsCorrectlyWithRespectoToRegistryValue(true);
		}

		public void TestImport_CreateNewPlannedLegAir()
		{
			AssertImportPlannedLeg(Xsd.TransportMode.AIR);
		}

		public void TestImport_UpdatePlannedLegAir()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_IsLinked = false;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "AUSYD";

			AssertImportPlannedLeg(Xsd.TransportMode.AIR);
		}

		public void TestImportPlannedLegSea()
		{
			AssertImportPlannedLeg(Xsd.TransportMode.SEA);
		}

		public void TestExportPlannedLegSeaThenAir()
		{
			Transport transport = Consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2005, 11, 21, 16, 40, 00);
			transport.JW_ATD = new ZDateTime(2005, 11, 21, 16, 41, 00);
			transport.JW_ETA = new ZDateTime(2005, 11, 21, 16, 43, 00);
			transport.JW_ATA = new ZDateTime(2005, 11, 21, 16, 44, 00);

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Vessel Name";
			Factory.Save();

			transport.JW_Vessel = "Vessel Name";
			transport.JW_VoyageFlight = "98765432";
			transport.Vessel.RV_LloydsNumber = "1234564";

			transport.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;

			FreightValueObjectDataAdapterTestClass adapter = new FreightValueObjectDataAdapterTestClass();

			Xsd.PlannedLeg plannedLeg = new Xsd.PlannedLeg();
			adapter.ExportPlannedLeg(plannedLeg, transport, new ValueObjectExportContext(new NotificationBuffer()), "");
			AssertEquals("TransportType OnForwarding/ONF", Xsd.PlannedLegTransportType.OnForwarding, plannedLeg.TransportType);
			AssertEquals("TransportType Specified", true, plannedLeg.TransportTypeSpecified);
			AssertEquals("TransportMode should be Sea", Xsd.TransportMode.SEA, plannedLeg.TransportMode);
			AssertEquals("Port of Loading should be AUSYD", "AUSYD", plannedLeg.PortOfLoading.Port.Value);
			AssertEquals("Port of Discharge should be USLAX", "USLAX", plannedLeg.PortOfDischarge.Port.Value);
			AssertEquals("Estimated time of Departure", new ZDateTime(2005, 11, 21, 16, 40, 00), plannedLeg.PortOfLoading.EstimatedDateTime);
			AssertEquals("Actual time of Departure", new ZDateTime(2005, 11, 21, 16, 41, 00), plannedLeg.PortOfLoading.ActualDateTime);
			AssertEquals("Estimated time of Arrival", new ZDateTime(2005, 11, 21, 16, 43, 00), plannedLeg.PortOfDischarge.EstimatedDateTime);
			AssertEquals("Acutal time of Arrival", new ZDateTime(2005, 11, 21, 16, 44, 00), plannedLeg.PortOfDischarge.ActualDateTime);
			Xsd.SailingForPlannedLegs plannedLegVessel = plannedLeg.Item as Xsd.SailingForPlannedLegs;
			AssertNotNull("PlannedLeg.Item Should be a PlannedLegVessel", vessel);
			AssertEquals("Vessel Name", "Vessel Name", plannedLegVessel.VesselName);
			AssertEquals("Loydes Number", "1234564", plannedLegVessel.LloydsNo);
			AssertEquals("Voyage Number", "98765432", plannedLegVessel.VoyageNo);

			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "49468468";
			transport.JW_TransportType = ZString.Empty;

			adapter.ExportPlannedLeg(plannedLeg, transport, new ValueObjectExportContext(new NotificationBuffer()), "");

			AssertEquals("TransportType Specified", false, plannedLeg.TransportTypeSpecified);
			Xsd.FlightWithFlightNumber flight = plannedLeg.Item as Xsd.FlightWithFlightNumber;
			AssertNotNull("PlannedLeg.Item Should be a PlannedLegRoadRailFlight", flight);
			AssertEquals("Flight Number", "49468468", flight.FlightNoJourneyNoTruckRegNo);
		}

		public void TestCreateStorageDocsDataAdapter()
		{
			FreightValueObjectDataAdapterTestClass freightAdapter = new FreightValueObjectDataAdapterTestClass();
			IValueObjectDataAdapter storageDocsAdapter = freightAdapter.CreateStorateDocsDataAdapter();
			AssertEquals("Should be a StorageDocs Data Adapter", "StorageDocsValueObjectDataAdapter", storageDocsAdapter.GetType().Name);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImporteDocs()
		{
			FreightValueObjectDataAdapterTestClass freightAdapter = new FreightValueObjectDataAdapterTestClass();
			Xsd.DocumentCollection documents = new Xsd.DocumentCollection();
			Xsd.Document doc1 = documents.AddNew();
			doc1.Data = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Small.tif"));
			doc1.DataType = "TIF";
			doc1.Date = new ZDateTime(2005, 12, 6);
			doc1.Description = "Masterbill";
			doc1.DocumentType = "MBL";

			Xsd.Document doc2 = documents.AddNew();
			doc2.Data = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Sample.PDF"));
			doc2.DataType = "PDF";
			doc2.Date = new ZDateTime(2005, 12, 6);
			doc2.Description = "Masterbill 2";
			doc2.DocumentType = "MBL";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			freightAdapter.ImporteDocs(Consol, documents, importContext);

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IStorageMainForPK documentFactory = (IStorageMainForPK)documentFactoryProvider.GetFactory(Factory);
			IDocumentsView storageMain = documentFactory.GetStorageMain(Consol.PK);
			AssertNotNull("Consol should have a StorageMain", storageMain);
			AssertEquals("StorageMain should have 1 StorageDocs", 1, storageMain.DocumentCollectionView.Count);
			AssertEquals("StorageMain should have 1 pdf StorageFile", 1, storageMain.PDFFilesCollectionView.Count);

			BusinessObject storageDoc1 = ((IBusinessObjectCollectionView)storageMain.DocumentCollectionView).ToArray()[0];
			AssertEquals("Date Type", "TIF", storageDoc1[StorageDocsSchema.SC_DataType.Name]);
			AssertEquals("Data", doc1.Data, storageDoc1[StorageDocsSchema.SC_ImageData.Name]);
			AssertEquals("Date", new ZDateTime(2005, 12, 6), storageDoc1[StorageDocsSchema.SC_Date.Name]);
			AssertEquals("Description", "Masterbill", storageDoc1[StorageDocsSchema.SC_Desc.Name]);
			AssertEquals("Document Type", "MBL", storageDoc1[StorageDocsSchema.SC_DocType.Name]);

			BusinessObject storageDoc2 = ((IBusinessObjectCollectionView)storageMain.PDFFilesCollectionView).ToArray()[0];
			AssertEquals("Date Type", "PDF", storageDoc2[StorageDocsSchema.SC_DataType.Name]);
			AssertEquals("Data", doc2.Data, storageDoc2[StorageDocsSchema.SC_ImageData.Name]);
			AssertEquals("Date", new ZDateTime(2005, 12, 6), storageDoc2[StorageDocsSchema.SC_Date.Name]);
			AssertEquals("Description", "Masterbill 2", storageDoc2[StorageDocsSchema.SC_Desc.Name]);
			AssertEquals("Document Type", "MBL", storageDoc2[StorageDocsSchema.SC_DocType.Name]);
		}

		public void TestGetNewOrganisationValueObjectDataAdapter()
		{
			FreightValueObjectDataAdapterTestClass adapter1 = new FreightValueObjectDataAdapterTestClass();
			OrganisationValueObjectDataAdapter orgAdapter = adapter1.GetNewOrganisationValueObjectDataAdapter(Consol);
			AssertEquals(Consol, orgAdapter.DocAddressesParent);

			FreightValueObjectDataAdapterNotIDocAddressesTestClass adapter2 = new FreightValueObjectDataAdapterNotIDocAddressesTestClass();
			OrgHeader org = Factory.New<OrgHeader>();
			orgAdapter = adapter2.GetNewOrganisationValueObjectDataAdapter(org);
			AssertEquals(org, orgAdapter.DocAddressesParent);
		}

		class FreightValueObjectDataAdapterNotIDocAddressesTestClass : FreightValueObjectDataAdapter<OrgHeader, Xsd.Organisation>
		{
			#region empty implementation

			protected override bool RegistryDefaultForImporting
			{
				get { throw new NotImplementedException(); }
			}

			public override XmlSchema CollectionSchema
			{
				get { throw new NotImplementedException(); }
			}

			protected override void ExportToValueObjectCore(OrgHeader bizObj, Xsd.Organisation constructedValueObject, IValueObjectExportContext context)
			{
				throw new NotImplementedException();
			}

			protected override void ImportFromValueObjectCore(OrgHeader bizObj, Xsd.Organisation value, IValueObjectImportContext context)
			{
				throw new NotImplementedException();
			}

			public override string RootCollectionElementName
			{
				get { throw new NotImplementedException(); }
			}

			public override string RootElementName
			{
				get { throw new NotImplementedException(); }
			}

			public override XmlSchema Schema
			{
				get { throw new NotImplementedException(); }
			}

			#endregion
		}

		#endregion

		#region implementation

		Xsd.FlightWithFlightNumber CreateFlight(ZString flightNumber)
		{
			Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
			flight.FlightNoJourneyNoTruckRegNo = flightNumber;
			return flight;
		}

		Xsd.SailingForPlannedLegs CreateSailing(ZString vessel, ZString voyage)
		{
			Xsd.SailingForPlannedLegs sailing = new Xsd.SailingForPlannedLegs();
			sailing.VesselName = vessel;
			sailing.VoyageNo = voyage;
			return sailing;
		}

		Xsd.SailingForPlannedLegs CreateSailing(ZString vessel, ZString voyage, ZString lloyds)
		{
			Xsd.SailingForPlannedLegs sailing = CreateSailing(vessel, voyage);
			sailing.LloydsNo = lloyds;
			return sailing;
		}

		void ImportPlannedLegs(TransportCollection transports, Xsd.PlannedLegCollection plannedLegs, INotifications notify)
		{
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notify);
			FreightValueObjectDataAdapterTestClass adapter = new FreightValueObjectDataAdapterTestClass();
			adapter.ImportPlannedLegs(transports, plannedLegs, importContext, "Error Context");
		}

		void AssertImportPlannedLeg(Xsd.TransportMode transportMode)
		{
			Xsd.PlannedLegCollection plannedLegs = new Xsd.PlannedLegCollection();
			Xsd.PlannedLeg plannedLeg = plannedLegs.AddNew();
			plannedLeg.TransportMode = transportMode;
			plannedLeg.TransportType = Xsd.PlannedLegTransportType.Flight1;
			plannedLeg.TransportTypeSpecified = true;
			ZDateTime eTA = new ZDateTime(2005, 11, 22, 10, 58, 00);
			ZDateTime aTA = new ZDateTime(2005, 11, 22, 11, 00, 00);
			ZDateTime eTD = new ZDateTime(2005, 11, 22, 11, 01, 00);
			ZDateTime aTD = new ZDateTime(2005, 11, 22, 11, 02, 00);

			plannedLeg.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(Factory, "USLAX", eTD, aTD);
			plannedLeg.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(Factory, "AUSYD", eTA, aTA);

			if (transportMode == Xsd.TransportMode.AIR)
			{
				plannedLeg.Item = CreateFlight("QF1234");
			}
			else
			{
				plannedLeg.Item = CreateSailing("America Star", "235425", "3987253");
			}

			FreightValueObjectDataAdapterTestClass adapater = new FreightValueObjectDataAdapterTestClass();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapater.ImportPlannedLegs(Consol.Transports, plannedLegs, context, "");

			AssertEquals("Should be 1 Transport in the Transport Colection", 1, Consol.Transports.Count);
			Transport transport = Consol.Transports[0];
			AssertEquals("Transports Mode", (transportMode == Xsd.TransportMode.AIR) ? Core.Constants.TransportModes.Air : Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Transport Type should be Flight1", Core.Constants.TransportPlanningType.Flight1, transport.JW_TransportType);
			AssertEquals("Port of Discharge", "AUSYD", transport.JW_RL_NKDiscPort);
			AssertEquals("Port of Loading", "USLAX", transport.JW_RL_NKLoadPort);
			AssertEquals("ETA", eTA, transport.JW_ETA);
			AssertEquals("ATA", aTA, transport.JW_ATA);
			AssertEquals("ETD", eTD, transport.JW_ETD);
			AssertEquals("ATD", aTD, transport.JW_ATD);

			if (transportMode == Xsd.TransportMode.AIR)
			{
				AssertEquals("Flight Number", "QF1234", transport.JW_VoyageFlight);
			}
			else if (transportMode == Xsd.TransportMode.SEA)
			{
				AssertEquals("Voyage Number", "235425", transport.JW_VoyageFlight);
				AssertEquals("vessel Name", "AMERICA STAR", transport.JW_Vessel);
			}
		}

		void AssertDataImportsCorrectlyWithRespectoToRegistryValue(bool registryValue)
		{
			SetRegistryItem(registryValue);
			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Precondition: IsBatchProcessor", true, Env.CurrentUser.IsBatchProcessor);

				CommonContainer original = GetNewFullyPopulatedBizO();
				original.Factory.Save();
				IValueObject valueObj = ExportToValueObj(FreightValueObjectDataAdapterToTest, original, new ValueObjectExportContext(new NotificationBuffer()));
				UpdateBizoValue(original);
				original.Factory.Save();

				ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				BusinessObject @new = ImportFromValueObj(FreightValueObjectDataAdapterToTest, valueObj, context);
				if (registryValue)
				{
					AssertValueOnBizOUpdated(original, @new);
				}
				else
				{
					AssertValueOnBizODidNotUpdate(original, @new);
				}
			}
		}

		void AssertValueOnBizOUpdated(BusinessObject original, BusinessObject @new)
		{
			AssertEquals("Test should find existing object", original.PK, @new.PK);
			AssertEquals("New container should have updated so it should have original value", ContainerModeOriginal, ((CommonContainer)@new).JC_ContainerMode);
		}

		void AssertValueOnBizODidNotUpdate(BusinessObject original, BusinessObject @new)
		{
			AssertEquals("Test should find existing object", original.PK, @new.PK);
			AssertEquals("New container should not have udpated so it should have new value", ContainerModeNew, ((CommonContainer)@new).JC_ContainerMode);
		}

		void SetRegistryItem(bool value)
		{
			RegistryItemToCheck.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		IValueObject ExportToValueObj(ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter, CommonContainer bizO, IValueObjectExportContext context)
		{
			return adapter.ExportToValueObject(bizO, context);
		}

		ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> FreightValueObjectDataAdapterToTest
		{
			get
			{
				if (freightValueObjectDataAdapterToTest == null)
				{
					freightValueObjectDataAdapterToTest = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(Consol);
				}
				return freightValueObjectDataAdapterToTest;
			}
		}
		ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> freightValueObjectDataAdapterToTest;

		CommonContainer GetNewFullyPopulatedBizO()
		{
			var newContainer = Factory.NewWithValidTestData<CommonContainer>();
			Consol.Containers.Add(newContainer);
			newContainer.JC_ContainerMode = ContainerModeOriginal;
			newContainer.JC_ContainerNum = "1";
			return newContainer;
		}

		BusinessObject ImportFromValueObj(ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container> adapter, IValueObject valueObj, ValueObjectImportContext context)
		{
			Xsd.Container containerValue = (Xsd.Container)valueObj;
			return adapter.CreateOrUpdateFromValueObject(containerValue, context);
		}

		BooleanRegistryItem RegistryItemToCheck
		{
			get { return SystemRegistry.UpdateConsolContainersDuringAutomaticImport; }
		}

		SystemDataRegistry SystemRegistry
		{
			get { return SystemDataRegistry.Instance; }
		}

		void UpdateBizoValue(BusinessObject bizO)
		{
			((CommonContainer)bizO).JC_ContainerMode = ContainerModeNew;
		}

		CommonConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<CommonConsol>();
				}
				return fConsol;
			}
		}
		CommonConsol fConsol;
		const string ContainerModeOriginal = Constants.ContainerModes.AgentConsol;
		const string ContainerModeNew = Constants.ContainerModes.AIR;

		class FreightValueObjectDataAdapterTestClass : FreightValueObjectDataAdapter<CommonConsol, Xsd.Consol>
		{
			#region Abstract Overrides

			public override string RootCollectionElementName
			{
				get { return "Consols"; }
			}

			public override string RootElementName
			{
				get { return "Consol"; }
			}

			public override XmlSchema Schema
			{
				get { return FreightXmlSchemaDefinitions.Instance.SingleConsolSchema; }
			}

			public override XmlSchema CollectionSchema
			{
				get { return FreightXmlSchemaDefinitions.Instance.ConsolsSchema; }
			}

			protected override void ExportToValueObjectCore(CommonConsol bizObj, Xsd.Consol constructedValueObject, IValueObjectExportContext context)
			{
				throw new NotImplementedException("Test class only");
			}

			protected override void ImportFromValueObjectCore(CommonConsol bizObj, Xsd.Consol value, IValueObjectImportContext context)
			{
				throw new NotImplementedException("Test class only");
			}

			protected override bool RegistryDefaultForImporting
			{
				get { return false; }
			}

			#endregion

			public void ImportPlannedLegs(TransportCollection transports, Xsd.PlannedLegCollection plannedLegs, IValueObjectImportContext context, string errorContext)
			{
				XsdPlannedLegObjectHelper.ImportPlannedLegs(transports, plannedLegs, context, errorContext);
			}

			public void ExportPlannedLeg(Xsd.PlannedLeg plannedLegValue, Transport transport, IValueObjectExportContext context, string errorContext)
			{
				XsdPlannedLegObjectHelper.ExportPlannedLeg(plannedLegValue, transport, context, errorContext);
			}

			public new IValueObjectDataAdapter CreateStorateDocsDataAdapter()
			{
				return base.CreateStorateDocsDataAdapter();
			}

			public new void ImporteDocs(BusinessObject bizObj, Xsd.DocumentCollection documents, IValueObjectImportContext context)
			{
				base.ImporteDocs(bizObj, documents, context);
			}
		}

		class PsudoNotificationSubscriber : INotifications, INotificationSubscriberQueryUser
		{
			#region INotifications Members

			public void Add(INotification @event)
			{
			}

			#endregion

			#region INotificationSubscriberQueryUser Members

			public void QueryUser(IQueryUserEventArgs e)
			{
				QueryUserYesNoYesAllNoAllEventArgs args = e as QueryUserYesNoYesAllNoAllEventArgs;
				if (args != null)
				{
					args.Response = false;
				}
			}

			#endregion
		}

		#endregion
	}
}
