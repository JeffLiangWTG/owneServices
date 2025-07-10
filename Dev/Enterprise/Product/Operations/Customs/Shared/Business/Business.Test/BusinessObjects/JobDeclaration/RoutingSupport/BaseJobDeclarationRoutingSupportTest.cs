using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationRoutingSupportTest : TestCaseWithFactory
	{
		public void TestTransportNotSetDuringDeclarationCreate()
		{
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);
		}

		public void TestTransportNotCreatedUntilKeyValuesNotEnteredForSea()
		{
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_ExportDate = new ZDateTime(2009, 4, 5);
			declaration.JE_DateOfArrival = new ZDateTime(2009, 5, 6);
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			string validVesselName = "WANA BHUM";
			var vessel = RefVessel.LookupVesselByName(validVesselName, Factory).FirstOrDefault();
			AssertNotNull("Precondition: Make Sure Vesssel '" + validVesselName + "' exists.", vessel);
			declaration.JE_VesselName = validVesselName;
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_VoyageFlightNo = "4389";
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();

			declaration.JE_OH_ShippingLine = shippingLine.PK;
			AssertEquals("Routing created", 1, declaration.Transports.Count);
			AssertEquals("JW_ETA should be set", new ZDateTime(2009, 5, 6), declaration.Transports[0].JW_ETA);

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "V123";
			voyage.JV_OH_Line = shippingLine.PK;

			var origin = voyage.Origins.AddNew();
			origin.JA_A_DEP = new ZDateTime(2011, 01, 20);
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var origin2 = voyage.Origins.AddNew();
			origin.JA_A_DEP = new ZDateTime(2011, 01, 21);
			origin.JA_RL_NKPortOfLoading = "AUMEL";

			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();

			var destination = voyage.Destinations.AddNew();
			destination.JB_E_ARV = new ZDateTime(2011, 01, 22);
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

			declaration.JE_VoyageFlightNo = "V123";
			declaration.JE_RL_NKPortOfLoading = "AUMEL";
			AssertEquals(new ZDateTime(2011, 01, 21), declaration.Transports[0].JW_ATD);
			AssertEquals(new ZDateTime(2011, 01, 21), declaration.JE_ExportDate);
		}

		public void TestTransportNotCreatedUntilKeyValuesNotEnteredForAir()
		{
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_ExportDate = new ZDateTime(2009, 4, 5);
			declaration.JE_DateOfArrival = new ZDateTime(2009, 5, 6);
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_VoyageFlightNo = "4389";
			AssertEquals("Routing created", 1, declaration.Transports.Count);
			AssertEquals("JW_ETA should be set", new ZDateTime(2009, 5, 6), declaration.Transports[0].JW_ETA);
		}

		void SetRoutingIntegrationOptions(bool alwaysLink, bool neverLink, bool conditionLink)
		{
			var options = new RoutingIntegrationOptions();

			if (alwaysLink)
			{
				options.AlwaysLink = alwaysLink;
			}

			if (neverLink)
			{
				options.NeverLink = neverLink;
			}

			if (conditionLink)
			{
				options.ConditionalLink = conditionLink;
			}

			Customs.DataRegistry.Business.CustomsDataRegistry.Instance.RoutingIntegrationOptions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);
		}

		public void TestTransportNotLinkedUntilMandatoryValuesEnteredForAirAlwaysLink()
		{
			SetRoutingIntegrationOptions(true, false, false);

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_ExportDate = ZDateTime.Today;
			AssertEquals("created", 1, declaration.Transports.Count);
			AssertEquals("linked", true, declaration.Transports[0].JW_IsLinked);
		}

		public void TestTransportNotLinkedUntilMandatoryValuesEnteredForAirNeverLinkOption()
		{
			SetRoutingIntegrationOptions(false, true, false);

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_ExportDate = ZDateTime.Today;
			AssertEquals("created", 1, declaration.Transports.Count);
			AssertEquals("not linked", false, declaration.Transports[0].JW_IsLinked);

			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Still one", 1, declaration.Transports.Count);
			AssertEquals("not linked", false, declaration.Transports[0].JW_IsLinked);

			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("still one", 1, declaration.Transports.Count);
			AssertEquals("still not linked", false, declaration.Transports[0].JW_IsLinked);
		}

		public void TestTransportNotLinkedUntilMandatoryValuesEnteredForSeaAlwaysLink()
		{
			SetRoutingIntegrationOptions(true, false, false);

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			string validVesselName = "WANA BHUM";
			AssertNotNull("Precondition: Make Sure Vesssel '" + validVesselName + "' exists.", RefVessel.LookupVesselByCode(validVesselName, Factory));
			declaration.JE_VesselName = validVesselName;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_OH_ShippingLine = Factory.New<OrgHeader>().PK;

			AssertEquals("created", 1, declaration.Transports.Count);
			AssertEquals("linked", true, declaration.Transports[0].JW_IsLinked);
		}

		public void TestTransportNotLinkedUntilMandatoryValuesEnteredForSeaMandatory()
		{
			SetRoutingIntegrationOptions(false, false, true);

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			string validVesselName = "WANA BHUM";
			AssertNotNull("Precondition: Make Sure Vesssel '" + validVesselName + "' exists.", RefVessel.LookupVesselByCode(validVesselName, Factory));
			declaration.JE_VesselName = validVesselName;

			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertEquals("not created", 0, declaration.Transports.Count);

			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			AssertEquals("not created", 0, declaration.Transports.Count);

			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("not created", 0, declaration.Transports.Count);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			declaration.JE_OH_ShippingLine = carrier.PK;
			AssertEquals("Created", 1, declaration.Transports.Count);
			AssertEquals("linked", true, declaration.Transports[0].JW_IsLinked);
			AssertNoErrors("If this fails and you have introduced another mandatory fields, you should change this synchronisation. Talk to Customs Team.", declaration.Transports[0]);
		}

		public void TestTransportNotLinkedUntilMandatoryValuesEnteredForSeaNeverLinkOption()
		{
			SetRoutingIntegrationOptions(false, true, false);

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VoyageFlightNo = "4389";

			string validVesselName = "WANA BHUM";
			AssertNotNull("Precondition: Make Sure Vesssel '" + validVesselName + "' exists.", RefVessel.LookupVesselByCode(validVesselName, Factory));
			declaration.JE_VesselName = validVesselName;

			AssertEquals("created: carrier is not needed for 'Never Link' option", 1, declaration.Transports.Count);
			AssertEquals("not linked", false, declaration.Transports[0].JW_IsLinked);

			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Still one", 1, declaration.Transports.Count);
			AssertEquals("not linked", false, declaration.Transports[0].JW_IsLinked);

			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("still one", 1, declaration.Transports.Count);
			AssertEquals("still not linked", false, declaration.Transports[0].JW_IsLinked);
		}

		public void TestVesselNotInRefVesselIsSynchronisedIfNotLinked()
		{
			SetRoutingIntegrationOptions(false, true, false);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			string invalidVesselName = "mltjiadct";
			declaration.JE_VesselName = invalidVesselName;
			AssertNull(declaration.Vessel);

			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("Created", 1, declaration.Transports.Count);
			AssertEquals("linked", false, declaration.Transports[0].JW_IsLinked);
			AssertEquals(invalidVesselName, declaration.Transports[0].JW_Vessel);

			declaration.JE_VesselName = "";
			declaration.Transports[0].JW_Vessel = "";
			declaration.Transports[0].JW_IsLinked = true;
			declaration.JE_VesselName = invalidVesselName;
			AssertEquals("", declaration.Transports[0].JW_Vessel);

			string validVesselName = "WANA BHUM";
			declaration.JE_VesselName = validVesselName;
			AssertNotNull(declaration.Vessel);
			AssertEquals(validVesselName, declaration.Transports[0].JW_Vessel);
		}

		public void TestTransportNotLinkedUntilMandatoryValuesEnteredForRoad()
		{
			SetRoutingIntegrationOptions(true, false, false);
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_ExportDate = new ZDateTime(2009, 4, 5);
			AssertEquals("created", 1, declaration.Transports.Count);
			AssertEquals("linked", true, declaration.Transports[0].JW_IsLinked);

			SetRoutingIntegrationOptions(false, false, true);
			declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertEquals("not created", 0, declaration.Transports.Count);

			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			AssertEquals("not created", 0, declaration.Transports.Count);

			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("Created", 1, declaration.Transports.Count);
			AssertEquals("linked", true, declaration.Transports[0].JW_IsLinked);
			AssertNoErrors("If this fails and you have introduced another mandatory fields, you should change this synchronisation. Talk to Customs Team.", declaration.Transports[0]);
		}

		public void TestTransportNotLinkedUntilMandatoryValuesEnteredForRail()
		{
			SetRoutingIntegrationOptions(true, false, false);
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_ExportDate = ZDateTime.Today;
			AssertEquals("created", 1, declaration.Transports.Count);
			AssertEquals("linked", true, declaration.Transports[0].JW_IsLinked);

			SetRoutingIntegrationOptions(false, false, true);
			declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No routing created", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertEquals("not created", 0, declaration.Transports.Count);

			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			AssertEquals("not created", 0, declaration.Transports.Count);

			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("Created", 1, declaration.Transports.Count);
			AssertEquals("linked", true, declaration.Transports[0].JW_IsLinked);
			AssertNoErrors("If this fails and you have introduced another mandatory fields, you should change this synchronisation. Talk to Customs Team.", declaration.Transports[0]);
		}

		public void TestLegNoGetsFilledInOnAutomaticallyCreatedLeg()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 4, 5);
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);
			AssertEquals("declaration.Transports[0].JW_LegOrder", (ZByte)1, declaration.Transports[0].JW_LegOrder);
		}

		public void TestNewTransportRowDoesntPickUpInvalidTransportMode()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			declaration.JE_TransportMode = "!_!";
			Transport transport = declaration.Transports.AddNew();
			AssertEquals("transport.JW_TransportMode", ZString.Empty, transport.JW_TransportMode);

			declaration.JE_TransportMode = "SEA";
			Transport transport2 = declaration.Transports.AddNew();
			AssertEquals("transport2.JW_TransportMode", "SEA", transport2.JW_TransportMode);
		}

		public void TestUnknownTransportModesDontGetCopied()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var routingParent = (ITransportParent)declaration;

			declaration.JE_TransportMode = "!1!";
			AssertEquals("Precondition: declaration.Transports.Count", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF101";
			declaration.JE_ExportDate = new ZDateTime(2009, 4, 5);

			var transport = declaration.Transports[0];
			AssertEquals("transport.JW_TransportMode", TransportTypeList.Codes.Air, transport.JW_TransportMode);

			declaration.JE_TransportMode = "*3*";
			AssertEquals("transport.JW_TransportMode", TransportTypeList.Codes.Air, transport.JW_TransportMode);
		}

		public void TestInvalidPortsDontGetCopied()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var routingParent = (ITransportParent)declaration;

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_RL_NKPortOfLoading = "_!1!_";
			declaration.JE_RL_NKPortOfArrival = "_!2!_";
			declaration.JE_ExportDate = ZDateTime.Invalid;
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);
			Transport transport = declaration.Transports[0];
			AssertEquals("transport.JW_RL_NKLoadPort", ZString.Empty, transport.JW_RL_NKLoadPort);
			AssertEquals("transport.JW_RL_NKDiscPort", ZString.Empty, transport.JW_RL_NKDiscPort);

			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("transport.JW_RL_NKLoadPort", "AUSYD", transport.JW_RL_NKLoadPort);
			AssertEquals("transport.JW_RL_NKDiscPort", "NZAKL", transport.JW_RL_NKDiscPort);
		}

		public void TestInvalidDatesDontGetCopied()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var routingParent = (ITransportParent)declaration;

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_ExportDate = ZDateTime.Invalid;
			declaration.JE_DateOfArrival = ZDateTime.Invalid;
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);
			var transport = declaration.Transports[0];
			AssertEquals("transport.JW_ETD", ZDateTime.Empty, transport.JW_ETD);
			AssertEquals("transport.JW_ETA", ZDateTime.Empty, transport.JW_ETA);

			declaration.JE_ExportDate = new ZDateTime(2009, 4, 5);
			declaration.JE_DateOfArrival = new ZDateTime(2009, 5, 6);
			AssertEquals("transport.JW_ETD", new ZDateTime(2009, 4, 5), transport.JW_ETD);
			AssertEquals("transport.JW_ETA", new ZDateTime(2009, 5, 6), transport.JW_ETA);
		}

		public void TestUpdateFromRoutingTabOnLoadIfNoMessagesSentYet()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ITransportParent routingParent = declaration;

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);
			declaration.JE_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);

			Transport transport = declaration.Transports[0];
			AssertEquals("Precondition: transport.JW_ETD", new ZDateTime(2009, 1, 2), transport.JW_ETD);
			AssertEquals("Precondition: transport.JW_ETA", new ZDateTime(2009, 1, 3), transport.JW_ETA);
			transport.JW_IsLinked = true;

			JobSailing sailing = transport.Sailing;
			Factory.Save();

			BusinessObjectFactory updateFactory = new BusinessObjectFactory();
			JobSailing sailingToUpdate = updateFactory.Load<JobSailing>(sailing.PK);
			sailingToUpdate.Origin.JA_E_DEP = new ZDateTime(2009, 1, 4);
			sailingToUpdate.Destination.JB_E_ARV = new ZDateTime(2009, 1, 5);
			updateFactory.Save();

			BusinessObjectFactory reloadFactory = new BusinessObjectFactory();
			BaseJobDeclaration declarationReloaded = reloadFactory.Load<BaseJobDeclaration>(declaration.PK);
			declarationReloaded.UpdateFromRoutingTabOnLoadIfNoMessagesSentYet();
			ITransportParent routingParentReloaded = declaration;
			AssertEquals("Precondition: declarationReloaded.Transports.Count", 1, declarationReloaded.Transports.Count);
			Transport transportReloaded = declarationReloaded.Transports[0];
			AssertEquals("Precondition: transportReloaded.JW_ETD", new ZDateTime(2009, 1, 4), transportReloaded.JW_ETD);
			AssertEquals("Precondition: transportReloaded.JW_ETA", new ZDateTime(2009, 1, 5), transportReloaded.JW_ETA);

			AssertEquals("declarationReloaded.JE_ExportDate", new ZDateTime(2009, 1, 4), declarationReloaded.JE_ExportDate);
			AssertEquals("declarationReloaded.JE_DateOfArrival", new ZDateTime(2009, 1, 5), declarationReloaded.JE_DateOfArrival);

			declarationReloaded.Messages.AddNew().EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			reloadFactory.Save();

			BusinessObjectFactory updateFactory2 = new BusinessObjectFactory();
			JobSailing sailingToUpdate2 = updateFactory2.Load<JobSailing>(sailing.PK);
			sailingToUpdate2.Origin.JA_E_DEP = new ZDateTime(2009, 1, 6);
			sailingToUpdate2.Destination.JB_E_ARV = new ZDateTime(2009, 1, 7);
			updateFactory2.Save();

			BusinessObjectFactory reloadFactory2 = new BusinessObjectFactory();
			BaseJobDeclaration declarationReloaded2 = reloadFactory2.Load<BaseJobDeclaration>(declaration.PK);
			declarationReloaded2.UpdateFromRoutingTabOnLoadIfNoMessagesSentYet();

			AssertEquals("declarationReloaded2.JE_ExportDate", new ZDateTime(2009, 1, 4), declarationReloaded2.JE_ExportDate);
			AssertEquals("declarationReloaded2.JE_DateOfArrival", new ZDateTime(2009, 1, 5), declarationReloaded2.JE_DateOfArrival);
		}

		public void TestUpdateFromRoutingTabOnLoadIfNoMessagesSentYet2()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ITransportParent routingParent = declaration;

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);
			declaration.JE_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);

			Transport transport = declaration.Transports[0];
			AssertEquals("Precondition: transport.JW_ETD", new ZDateTime(2009, 1, 2), transport.JW_ETD);
			AssertEquals("Precondition: transport.JW_ETA", new ZDateTime(2009, 1, 3), transport.JW_ETA);
			transport.JW_IsLinked = true;

			JobSailing sailing = transport.Sailing;
			Factory.Save();

			BusinessObjectFactory updateFactory = new BusinessObjectFactory();
			JobSailing sailingToUpdate = updateFactory.Load<JobSailing>(sailing.PK);
			sailingToUpdate.Origin.JA_RL_NKPortOfLoading = "NZABY";
			sailingToUpdate.Origin.JA_E_DEP = new ZDateTime(2009, 1, 6);
			sailingToUpdate.Destination.JB_E_ARV = new ZDateTime(2009, 1, 7);
			updateFactory.Save();

			BusinessObjectFactory reloadFactory = new BusinessObjectFactory();
			BaseJobDeclaration declarationReloaded = reloadFactory.Load<BaseJobDeclaration>(declaration.PK);
			ITransportParent routingParentReloaded = declaration;
			AssertEquals("Precondition: declarationReloaded3.Transports.Count", 1, declarationReloaded.Transports.Count);
			Transport transportReloaded = declarationReloaded.Transports[0];
			AssertEquals("Precondition: transportReloaded3.JW_RL_NKLoadPort", "NZABY", transportReloaded.JW_RL_NKLoadPort);
			AssertEquals("Precondition: transportReloaded3.JW_ETD", new ZDateTime(2009, 1, 6), transportReloaded.JW_ETD);
			AssertEquals("Precondition: transportReloaded3.JW_ETA", new ZDateTime(2009, 1, 7), transportReloaded.JW_ETA);

			declarationReloaded.UpdateFromRoutingTabOnLoadIfNoMessagesSentYet();
			AssertEquals("declarationReloaded3.JE_RL_NKPortOfLoading", "NZAKL", declarationReloaded.JE_RL_NKPortOfLoading);
			AssertEquals("declarationReloaded3.JE_ExportDate", new ZDateTime(2009, 1, 2), declarationReloaded.JE_ExportDate);
			AssertEquals("declarationReloaded3.JE_DateOfArrival", new ZDateTime(2009, 1, 7), declarationReloaded.JE_DateOfArrival);
		}

		public void TestUpdateRoutingWhenActualTimeExist()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 5);
			declaration.JE_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().PK;

			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);

			var transport = declaration.Transports[0];
			AssertEquals("Precondition: transport.JW_ETD", new ZDateTime(2009, 1, 2), transport.JW_ETD);
			AssertEquals("Precondition: transport.JW_ETA", new ZDateTime(2009, 1, 5), transport.JW_ETA);

			transport.JW_ATD = new ZDateTime(2009, 1, 3);
			transport.JW_ATA = new ZDateTime(2009, 1, 6);
			AssertEquals("declaration ATD", new ZDateTime(2009, 1, 3), declaration.JE_ExportDate);
			AssertEquals("declaration ATA", new ZDateTime(2009, 1, 6), declaration.JE_DateOfArrival);

			declaration.JE_ExportDate = new ZDateTime(2009, 1, 4);
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 7);
			AssertEquals("transport.JW_ETD", new ZDateTime(2009, 1, 2), transport.JW_ETD);
			AssertEquals("transport.JW_ATD", new ZDateTime(2009, 1, 4), transport.JW_ATD);
			AssertEquals("transport.JW_ETA", new ZDateTime(2009, 1, 5), transport.JW_ETA);
			AssertEquals("transport.JW_ATA", new ZDateTime(2009, 1, 7), transport.JW_ATA);
		}

		public void TestTemplateCopyCopiesRoutings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF101";
			declaration.JE_ExportDate = new ZDateTime(2009, 4, 5);
			declaration.Transports.AddNew().JW_VoyageFlight = "QF102";
			AssertEquals("Precondition: declaration.Transports.Count", 2, declaration.Transports.Count);

			declaration.Transports.Sort(JobConsolTransportSchema.Constants.JW_VoyageFlight);
			AssertEquals("declaration.Transports[0].JW_VoyageFlight", "QF101", declaration.Transports[0].JW_VoyageFlight);
			AssertEquals("declaration.Transports[1].JW_VoyageFlight", "QF102", declaration.Transports[1].JW_VoyageFlight);

			Factory.Save();

			var copiedDeclaration = declaration.GetNewRelatedDeclaration(Factory);
			AssertEquals("copiedDeclaration.Transports.Count", 2, copiedDeclaration.Transports.Count);
			copiedDeclaration.Transports.Sort(JobConsolTransportSchema.Constants.JW_VoyageFlight);
			AssertEquals("copiedDeclaration.Transports[0].JW_VoyageFlight", "QF101", copiedDeclaration.Transports[0].JW_VoyageFlight);
			AssertEquals("copiedDeclaration.Transports[1].JW_VoyageFlight", "QF102", copiedDeclaration.Transports[1].JW_VoyageFlight);
		}

		public void TestUpdatingFromDeclarationToRoutings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Precondition: declaration.Transports.Count", 0, declaration.Transports.Count);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF101";
			declaration.JE_ExportDate = new ZDateTime(2009, 4, 5);
			AssertEquals("declaration.Transports.Count", 1, declaration.Transports.Count);

			var transport = declaration.Transports[0];
			AssertEquals(TransportTypeList.Codes.Air, transport.JW_TransportMode);
			declaration.JE_VoyageFlightNo = "QF253";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);
			var airline = Factory.New<OrgHeader>();
			declaration.JE_OH_ShippingLine = airline.PK;

			AssertEquals("Air transport.JW_LegOrder", (ZByte)1, transport.JW_LegOrder);
			AssertEquals("Air transport.JW_TransportMode", "AIR", transport.JW_TransportMode);
			AssertEquals("Air transport.JW_TransportType", "FL1", transport.JW_TransportType);
			AssertEquals("Air transport.JW_Vessel", "", transport.JW_Vessel);
			AssertEquals("Air transport.JW_VoyageFlight", "QF253", transport.JW_VoyageFlight);
			AssertEquals("Air transport.JW_RL_NKLoadPort", "NZAKL", transport.JW_RL_NKLoadPort);
			AssertEquals("Air transport.JW_RL_NKDiscPort", "AUSYD", transport.JW_RL_NKDiscPort);
			AssertEquals("Air transport.JW_ATD", ZDateTime.Empty, transport.JW_ATD);
			AssertEquals("Air transport.JW_ATA", ZDateTime.Empty, transport.JW_ATA);
			AssertEquals("Air transport.JW_ETD", new ZDateTime(2009, 1, 2), transport.JW_ETD);
			AssertEquals("Air transport.JW_ETA", new ZDateTime(2009, 1, 3), transport.JW_ETA);
			AssertEquals("Air transport.CarrierPK", airline.PK, transport.CarrierPK);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "BUNGA BIDARA";
			declaration.JE_VoyageFlightNo = "V109";
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2009, 2, 2);
			declaration.JE_RL_NKPortOfArrival = "AUMEL";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 2, 3);
			var shippingLine = Factory.New<OrgHeader>();
			declaration.JE_OH_ShippingLine = shippingLine.PK;

			AssertEquals("Sea transport.JW_LegOrder", (ZByte)1, transport.JW_LegOrder);
			AssertEquals("Sea transport.JW_TransportMode", "SEA", transport.JW_TransportMode);
			AssertEquals("Sea transport.JW_TransportType", "MAI", transport.JW_TransportType);
			AssertEquals("Sea transport.JW_Vessel", "BUNGA BIDARA", transport.JW_Vessel);
			AssertEquals("Sea transport.JW_VoyageFlight", "V109", transport.JW_VoyageFlight);
			AssertEquals("Sea transport.JW_RL_NKLoadPort", "USLAX", transport.JW_RL_NKLoadPort);
			AssertEquals("Sea transport.JW_RL_NKDiscPort", "AUMEL", transport.JW_RL_NKDiscPort);
			AssertEquals("Sea transport.JW_ATD", ZDateTime.Empty, transport.JW_ATD);
			AssertEquals("Sea transport.JW_ATA", ZDateTime.Empty, transport.JW_ATA);
			AssertEquals("Sea transport.JW_ETD", new ZDateTime(2009, 2, 2), transport.JW_ETD);
			AssertEquals("Sea transport.JW_ETA", new ZDateTime(2009, 2, 3), transport.JW_ETA);
			AssertEquals("Sea transport.CarrierPK", shippingLine.PK, transport.CarrierPK);

			//Test update when second Transport is present.
			var secondTransport = declaration.Transports.AddNew();
			secondTransport.JW_VoyageFlight = "222";
			secondTransport.JW_ETD = new ZDateTime(2009, 2, 3);
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_ETA = new ZDateTime(2009, 2, 4);
			secondTransport.JW_RL_NKDiscPort = "AUSYD";
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_ExportDate = new ZDateTime(2009, 2, 1);
			declaration.JE_RL_NKPortOfLoading = "USCHI";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 2, 5);
			declaration.JE_RL_NKPortOfArrival = "AUPER";
			AssertEquals("Unchanged transport.JW_Vessel", "BUNGA BIDARA", transport.JW_Vessel);
			AssertEquals("First Transport.JW_ETD", new ZDateTime(2009, 2, 1), transport.JW_ETD);
			AssertEquals("First Transport.JW_ETA", new ZDateTime(2009, 2, 5), transport.JW_ETA);
		}

		public void TestUpdatingFromRoutingToDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Precondition: declaration.Transports.Count", 0, declaration.Transports.Count);

			Transport transport = declaration.Transports.AddNew();

			transport.JW_TransportMode = "AIR";
			AssertEquals("Precondition: transport.JW_TransportType", "FL1", transport.JW_TransportType);
			transport.JW_Vessel = "";
			transport.JW_VoyageFlight = "QF253";
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2009, 1, 2);
			transport.JW_ETA = new ZDateTime(2009, 1, 3);
			transport.JW_ATD = new ZDateTime(2009, 1, 2);
			transport.JW_ATA = new ZDateTime(2009, 1, 3);
			OrgHeader airline = Factory.New<OrgHeader>();
			transport.CarrierPK = airline.PK;

			AssertEquals("Air declaration.JE_TransportMode", TransportTypeList.Codes.Air, declaration.JE_TransportMode);
			AssertEquals("Air declaration.JE_VoyageFlightNo", "QF253", declaration.JE_VoyageFlightNo);
			AssertEquals("Air declaration.JE_RL_NKPortOfLoading", "NZAKL", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Air declaration.JE_ExportDate", new ZDateTime(2009, 1, 2), declaration.JE_ExportDate);
			AssertEquals("Air declaration.JE_RL_NKPortOfArrival", "AUSYD", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Air declaration.JE_DateOfArrival", new ZDateTime(2009, 1, 3), declaration.JE_DateOfArrival);
			AssertEquals("Air declaration.JE_OH_ShippingLine", airline.PK, declaration.JE_OH_ShippingLine);

			transport.JW_TransportMode = "SEA";
			AssertEquals("Precondition: transport.JW_TransportType", "MAI", transport.JW_TransportType);
			transport.JW_Vessel = "BUNGA BIDARA";
			transport.JW_VoyageFlight = "V109";
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_ATD = new ZDateTime(2009, 2, 2);
			transport.JW_ATA = new ZDateTime(2009, 2, 3);
			transport.JW_ETD = new ZDateTime(2009, 4, 2);
			transport.JW_ETA = new ZDateTime(2009, 4, 3);
			OrgHeader shippingLine = Factory.New<OrgHeader>();
			transport.CarrierPK = shippingLine.PK;

			AssertEquals("Sea declaration.JE_TransportMode", TransportTypeList.Codes.Sea, declaration.JE_TransportMode);
			AssertEquals("Sea declaration.JE_VesselName", "BUNGA BIDARA", declaration.JE_VesselName);
			AssertEquals("Sea declaration.JE_VoyageFlightNo", "V109", declaration.JE_VoyageFlightNo);
			AssertEquals("Sea declaration.JE_RL_NKPortOfLoading", "USLAX", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Sea declaration.JE_ExportDate", new ZDateTime(2009, 2, 2), declaration.JE_ExportDate);
			AssertEquals("Sea declaration.JE_RL_NKPortOfArrival", "AUMEL", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Sea declaration.JE_DateOfArrival", new ZDateTime(2009, 2, 3), declaration.JE_DateOfArrival);
			AssertEquals("Sea declaration.JE_OH_ShippingLine", shippingLine.PK, declaration.JE_OH_ShippingLine);

			//Test update when second Transport is present.
			transport.JW_ATD = ZDateTime.Empty;
			transport.JW_ATA = ZDateTime.Empty;
			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ETA = ZDateTime.Empty;
			Transport secondTransport = declaration.Transports.AddNew();
			secondTransport.JW_VoyageFlight = "222";
			transport.JW_Vessel = "BUNGA DELIMA";
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_ETD = new ZDateTime(2009, 2, 1);
			secondTransport.JW_RL_NKDiscPort = "AUSYD";
			secondTransport.JW_ETA = new ZDateTime(2009, 2, 5);
			AssertEquals("Unchanged declaration.JE_VesselName", "BUNGA BIDARA", declaration.JE_VesselName);
			AssertEquals("Sea declaration.JE_RL_NKPortOfLoading", "USLAX", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Sea declaration.JE_ExportDate", ZDateTime.Empty, declaration.JE_ExportDate);
			AssertEquals("Sea declaration.JE_RL_NKPortOfArrival", "AUMEL", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Sea declaration.JE_DateOfArrival", ZDateTime.Empty, declaration.JE_DateOfArrival);

			transport.JW_ETD = new ZDateTime(2009, 2, 3);
			transport.JW_ETA = new ZDateTime(2009, 2, 4);
			AssertEquals("Sea declaration.JE_ExportDate", new ZDateTime(2009, 2, 3), declaration.JE_ExportDate);
			AssertEquals("Sea declaration.JE_RL_NKPortOfArrival", "AUMEL", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Sea declaration.JE_DateOfArrival", new ZDateTime(2009, 2, 4), declaration.JE_DateOfArrival);
		}

		public void TestRoutingSupportFromStandalone()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			IRoutingSupport routingParent = declaration;

			AssertEquals("Transport Mode", Enterprise.Core.Constants.TransportModes.Sea, routingParent.TransportMode);
			AssertEquals("declaration.HasRoutingSupportDirectOnDeclaration", true, declaration.HasRoutingSupportDirectOnDeclaration);

			Transport transport1 = routingParent.Transports.AddNew();
			AssertEquals("Transports.AddNew().Parent", declaration, transport1.Parent);

			Transport transport2 = routingParent.TransportsIncludingRelated.AddNew();
			AssertEquals("TransportsIncludingRelated.AddNew().Parent", declaration, transport2.Parent);
		}

		public void TestRoutingSupportFromShipmentLinked()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.AirSea;
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			AssertEquals("declaration.HasRoutingSupportDirectOnDeclaration", false, declaration.HasRoutingSupportDirectOnDeclaration);

			IRoutingSupport routingParentDeclaration = declaration;
			IRoutingSupport routingParentShipment = shipment;
			AssertEquals("dec transport Mode", Enterprise.Core.Constants.TransportModes.AirSea, routingParentDeclaration.TransportMode);
			AssertEquals("shipment transport Mode", Enterprise.Core.Constants.TransportModes.AirSea, routingParentShipment.TransportMode);

			Transport transport1 = routingParentDeclaration.Transports.AddNew();
			AssertEquals("Transports.AddNew().Parent", routingParentShipment, transport1.Parent);

			Transport transport2 = routingParentDeclaration.TransportsIncludingRelated.AddNew();
			AssertEquals("TransportsIncludingRelated.AddNew().Parent", routingParentShipment, transport2.Parent);
		}

		public void TestUpdateDetailsToExistingSailingSchedules()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var declaration = Factory.New<BaseJobDeclaration>();

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);
			declaration.JE_OH_ShippingLine = carrier.PK;
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);

			var transport = declaration.Transports[0];
			AssertEquals("Precondition: transport.JW_ETD", new ZDateTime(2009, 1, 2), transport.JW_ETD);
			AssertEquals("Precondition: transport.JW_ETA", new ZDateTime(2009, 1, 3), transport.JW_ETA);
			AssertNotNull("SailingSchedule has been created", transport.Sailing);
			Factory.Save();

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration2.JE_VesselName = "BUNGA DELIMA";
			declaration2.JE_VoyageFlightNo = "4389";
			declaration2.JE_RL_NKPortOfLoading = "NZAKL";
			declaration2.JE_RL_NKPortOfArrival = "AUSYD";
			declaration2.JE_OH_ShippingLine = carrier.PK;

			var transport2 = declaration2.Transports[0];
			AssertEquals("Precondition: transport2.JW_ETD", new ZDateTime(2009, 1, 2), transport2.JW_ETD);
			AssertEquals("Precondition: transport2.JW_ETA", new ZDateTime(2009, 1, 3), transport2.JW_ETA);
			AssertEquals("date should be defaulted", new ZDateTime(2009, 1, 2), declaration2.JE_ExportDate);
			AssertEquals("date should be defaulted", new ZDateTime(2009, 1, 3), declaration2.JE_DateOfArrival);
		}

		public void TestITransportParentBasics()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ITransportParent routingParent = declaration;
			TransportSupporter supporter = routingParent.TransportSupporter;

			declaration.JE_MasterBill = "MASTER BILL";
			declaration.JE_DeclarationReference = "FELLOWSHIP";
			declaration.JE_TransportMode = "XXX";

			AssertEquals("routingParent.BillOfLading", "MASTER BILL", supporter.BillOfLading);
			AssertEquals("routingParent.ConsignmentRef", "FELLOWSHIP", supporter.ConsignmentRef);
			AssertEquals("routingParent.Description", "FELLOWSHIP", supporter.Description);
			AssertEquals("routingParent.TransportMode", "XXX", supporter.TransportMode);
			AssertEquals("routingParent.TypeCode", Constants.TransportParentTypes.Declaration, routingParent.TypeCode);

			OrgHeader shippingLine = Factory.New<OrgHeader>();
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			AssertEquals("routingParent.ShippingLine", shippingLine.PK, supporter.ShippingLine);
			supporter.ShippingLine = ZGuid.Empty;
			AssertEquals("routingParent.ShippingLine", shippingLine.PK, supporter.ShippingLine);
			AssertEquals("declaration.JE_OH_ShippingLine", shippingLine.PK, declaration.JE_OH_ShippingLine);

			declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertEquals("routingParent.ShippingLine", ZGuid.Empty, supporter.ShippingLine);
			AssertEquals("declaration.JE_OH_ShippingLine", ZGuid.Empty, declaration.JE_OH_ShippingLine);
			AssertEquals("routingParent.Transports", declaration.Transports, routingParent.Transports);
		}
	}
}
