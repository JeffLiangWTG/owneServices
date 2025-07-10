using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationTransportSupporterTest : TransportSupporterTestCase<BaseJobDeclarationTransportSupporter<BaseJobDeclaration>>
	{
		public void TestUpdateFromSailing()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ITransportParent routingParent = declaration;
			TransportSupporter supporter = routingParent.TransportSupporter;

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF202";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);
			Transport transport = declaration.Transports[0];
			AssertEquals("Precondition: Air transport.JW_ETD", new ZDateTime(2009, 1, 2), transport.JW_ETD);
			AssertEquals("Precondition: Air transport.JW_ETA", new ZDateTime(2009, 1, 3), transport.JW_ETA);

			supporter.ETDSetFromSailing(transport, new ZDateTime(2009, 1, 2), new ZDateTime(2009, 1, 4));
			AssertEquals("1 declaration.JE_ExportDate", new ZDateTime(2009, 1, 4), declaration.JE_ExportDate);
			AssertEquals("1 declaration.JE_DateOfArrival", new ZDateTime(2009, 1, 3), declaration.JE_DateOfArrival);

			supporter.ETASetFromSailing(transport, new ZDateTime(2009, 1, 3), new ZDateTime(2009, 1, 5));
			AssertEquals("2 declaration.JE_ExportDate", new ZDateTime(2009, 1, 4), declaration.JE_ExportDate);
			AssertEquals("2 declaration.JE_DateOfArrival", new ZDateTime(2009, 1, 5), declaration.JE_DateOfArrival);

			supporter.ETDSetFromSailing(transport, new ZDateTime(2009, 1, 2), new ZDateTime(2009, 2, 4));
			supporter.ETASetFromSailing(transport, new ZDateTime(2009, 1, 3), new ZDateTime(2009, 2, 5));

			AssertEquals("3 declaration.JE_ExportDate", new ZDateTime(2009, 1, 4), declaration.JE_ExportDate);
			AssertEquals("3 declaration.JE_DateOfArrival", new ZDateTime(2009, 1, 5), declaration.JE_DateOfArrival);

			supporter.ETDSetFromSailing(transport, new ZDateTime(2009, 1, 4), new ZDateTime(2009, 2, 4));
			supporter.ETASetFromSailing(transport, new ZDateTime(2009, 1, 5), new ZDateTime(2009, 2, 5));

			AssertEquals("4 declaration.JE_ExportDate", new ZDateTime(2009, 2, 4), declaration.JE_ExportDate);
			AssertEquals("4 declaration.JE_DateOfArrival", new ZDateTime(2009, 2, 5), declaration.JE_DateOfArrival);

			declaration.CustomsEntryHeaders.AddNew().Messages.AddNew();

			supporter.ETDSetFromSailing(transport, new ZDateTime(2009, 2, 4), new ZDateTime(2009, 3, 4));
			supporter.ETASetFromSailing(transport, new ZDateTime(2009, 2, 5), new ZDateTime(2009, 3, 5));

			AssertEquals("5 declaration.JE_ExportDate", new ZDateTime(2009, 2, 4), declaration.JE_ExportDate);
			AssertEquals("5 declaration.JE_DateOfArrival", new ZDateTime(2009, 2, 5), declaration.JE_DateOfArrival);
		}

		public void TestDoNotUpdateFromSailingIfWrongLeg()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ITransportParent routingParent = declaration;
			TransportSupporter supporter = routingParent.TransportSupporter;

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF202";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);
			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);
			Transport transport = declaration.Transports[0];
			AssertEquals("Precondition: Air transport.JW_ETD", new ZDateTime(2009, 1, 2), transport.JW_ETD);
			AssertEquals("Precondition: Air transport.JW_ETA", new ZDateTime(2009, 1, 3), transport.JW_ETA);

			Transport transportOther = Factory.New<Transport>();
			transportOther.ParentType = declaration.GetType();
			transportOther.JW_ParentGUID = declaration.PK;
			transportOther.JW_ETD = new ZDateTime(2009, 1, 2);
			transportOther.JW_ETA = new ZDateTime(2009, 1, 3);

			AssertNull("Precondition: transportOther should not be part of the declaration.Transports", declaration.Transports.FindByPK(transportOther.PK));
			AssertEquals("Precondition: transportOther should not be part of the declaration.Transports", 1, declaration.Transports.Count);

			supporter.ETDSetFromSailing(transportOther, new ZDateTime(2009, 1, 2), new ZDateTime(2009, 1, 4));
			AssertEquals("1 declaration.JE_ExportDate", new ZDateTime(2009, 1, 2), declaration.JE_ExportDate);
			AssertEquals("1 declaration.JE_DateOfArrival", new ZDateTime(2009, 1, 3), declaration.JE_DateOfArrival);

			supporter.ETDSetFromSailing(transport, new ZDateTime(2009, 1, 2), new ZDateTime(2009, 1, 4));
			AssertEquals("1 declaration.JE_ExportDate", new ZDateTime(2009, 1, 4), declaration.JE_ExportDate);
			AssertEquals("1 declaration.JE_DateOfArrival", new ZDateTime(2009, 1, 3), declaration.JE_DateOfArrival);

			supporter.ETASetFromSailing(transportOther, new ZDateTime(2009, 1, 3), new ZDateTime(2009, 1, 5));
			AssertEquals("2 declaration.JE_ExportDate", new ZDateTime(2009, 1, 4), declaration.JE_ExportDate);
			AssertEquals("2 declaration.JE_DateOfArrival", new ZDateTime(2009, 1, 3), declaration.JE_DateOfArrival);

			supporter.ETASetFromSailing(transport, new ZDateTime(2009, 1, 3), new ZDateTime(2009, 1, 5));
			AssertEquals("2 declaration.JE_ExportDate", new ZDateTime(2009, 1, 4), declaration.JE_ExportDate);
			AssertEquals("2 declaration.JE_DateOfArrival", new ZDateTime(2009, 1, 5), declaration.JE_DateOfArrival);
		}

		public void TestUpdateFromGlobalSailingSchedule()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "ABC123";

			var shippingLine1 = Factory.New<OrgHeader>();
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_OH_Line = shippingLine1.PK;
			voyage1.JV_AirSeaRoad = TransportTypeList.Codes.Sea;
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "NZAKL";
			origin1.JA_A_DEP = new ZDateTime(2019, 2, 3);
			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			destination1.JB_A_ARV = new ZDateTime(2019, 2, 4);
			var sailing1 = voyage1.Sailings.AddNew();
			var declaration = Factory.New<BaseJobDeclaration>();
			var transport1 = declaration.Transports.AddNew();

			AssertEquals("Precondition: Carrier is empty", ZGuid.Empty, declaration.JE_OH_ShippingLine);
			AssertEquals("Precondition: Port of arrival is empty", ZString.Empty, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Precondition: Arrival date is empty", ZDateTime.Empty, declaration.JE_DateOfArrival);
			AssertEquals("Precondition: Port of loading is empty", ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Precondition: Export date is empty", ZDateTime.Empty, declaration.JE_ExportDate);
			AssertEquals("Precondition: Transport mode is empty", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("Precondition: Vessel is empty", ZString.Empty, declaration.JE_VesselName);
			AssertEquals("Precondition: Voyage Flight No. is empty", ZString.Empty, declaration.JE_VoyageFlightNo);

			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;

			AssertEquals(transport1.CarrierPK, declaration.JE_OH_ShippingLine);
			AssertEquals(transport1.JW_RL_NKDiscPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals(transport1.JW_ATA, declaration.JE_DateOfArrival);
			AssertEquals(transport1.JW_RL_NKLoadPort, declaration.JE_RL_NKPortOfLoading);
			AssertEquals(transport1.JW_ATD, declaration.JE_ExportDate);
			AssertEquals(transport1.JW_TransportMode, declaration.JE_TransportMode);
			AssertEquals(transport1.JW_Vessel, declaration.JE_VesselName);
			AssertEquals(transport1.JW_VoyageFlight, declaration.JE_VoyageFlightNo);

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "DEF456";

			var shippingLine2 = Factory.New<OrgHeader>();
			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_OH_Line = shippingLine2.PK;
			voyage2.JV_AirSeaRoad = TransportTypeList.Codes.Air;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "USLAX";
			origin2.JA_A_DEP = new ZDateTime(2019, 5, 3);
			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUMEL";
			destination2.JB_A_ARV = new ZDateTime(2019, 5, 4);
			var sailing2 = voyage2.Sailings.AddNew();
			var transport2 = declaration.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;

			AssertEquals(transport1.CarrierPK, declaration.JE_OH_ShippingLine);
			AssertEquals(transport1.JW_RL_NKDiscPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals(transport1.JW_ATA, declaration.JE_DateOfArrival);
			AssertEquals(transport1.JW_RL_NKLoadPort, declaration.JE_RL_NKPortOfLoading);
			AssertEquals(transport1.JW_ATD, declaration.JE_ExportDate);
			AssertEquals(transport1.JW_TransportMode, declaration.JE_TransportMode);
			AssertEquals(transport1.JW_Vessel, declaration.JE_VesselName);
			AssertEquals(transport1.JW_VoyageFlight, declaration.JE_VoyageFlightNo);
		}

		public void TestUpdateAllDeclarationTransportData()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback; // stop synchronisation
			ITransportParent routingParent = declaration;
			BaseJobDeclarationTransportSupporter<BaseJobDeclaration> supporter = (BaseJobDeclarationTransportSupporter<BaseJobDeclaration>)routingParent.TransportSupporter;
			AssertEquals("Precondition: declaration.Transports.Count", 0, declaration.Transports.Count);

			Transport transport = declaration.Transports.AddNew();
			transport.JW_ETD = new ZDateTime(2009, 1, 2);
			transport.JW_ETA = new ZDateTime(2009, 1, 3);
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.FillWithValidTestData();
			transport.CarrierPK = carrier.PK;
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_TransportMode = TransportTypeList.Codes.Sea;
			transport.JW_Vessel = "VESSEL";
			transport.JW_VoyageFlight = "SW1";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("export date should be empty", ZDateTime.Empty, declaration.JE_ExportDate);
			AssertEquals("arrival date should be empty", ZDateTime.Empty, declaration.JE_DateOfArrival);
			AssertEquals("Shipping Line should be empty", ZGuid.Empty, declaration.JE_OH_ShippingLine);
			AssertEquals("Port Of Arrival should be empty", ZString.Empty, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Port Of Loading should be empty", ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Transport Mode should be empty", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("Vessel should be empty", ZString.Empty, declaration.JE_VesselName);
			AssertEquals("Voyage Flight No should be empty", ZString.Empty, declaration.JE_VoyageFlightNo);

			supporter.UpdateAllDeclarationTransportDataIfEmpty(transport);

			AssertEquals("export date should be updated", new ZDateTime(2009, 1, 2), declaration.JE_ExportDate);
			AssertEquals("arrival date should be updated", new ZDateTime(2009, 1, 3), declaration.JE_DateOfArrival);
			AssertEquals("Shipping Line should be updated", carrier.PK, declaration.JE_OH_ShippingLine);
			AssertEquals("Port Of Arrival should be updated", "USLAX", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Port Of Loading should be updated", "NZAKL", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Transport Mode should be updated", TransportTypeList.Codes.Sea, declaration.JE_TransportMode);
			AssertEquals("Vessel should be updated", "VESSEL", declaration.JE_VesselName);
			AssertEquals("Voyage Flight No should be updated", "SW1", declaration.JE_VoyageFlightNo);
		}

		public void TestDoNotUpdateDeclarationTransportDataIfWrongLeg()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback; // stop synchronisation
			ITransportParent routingParent = declaration;
			BaseJobDeclarationTransportSupporter<BaseJobDeclaration> supporter = (BaseJobDeclarationTransportSupporter<BaseJobDeclaration>)routingParent.TransportSupporter;
			AssertEquals("Precondition: declaration.Transports.Count", 0, declaration.Transports.Count);

			Transport transport = declaration.Transports.AddNew();
			transport.JW_ETD = new ZDateTime(2009, 1, 2);
			transport.JW_ETA = new ZDateTime(2009, 1, 3);
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.FillWithValidTestData();
			transport.CarrierPK = carrier.PK;
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_TransportMode = TransportTypeList.Codes.Sea;
			transport.JW_Vessel = "VESSEL";
			transport.JW_VoyageFlight = "SW1";

			Transport transportOther = Factory.New<Transport>();
			transportOther.ParentType = declaration.GetType();
			transportOther.JW_ParentGUID = declaration.PK;
			AssertNull("Precondition: transportOther should not be part of the declaration.Transports", declaration.Transports.FindByPK(transportOther.PK));
			AssertEquals("Precondition: transportOther should not be part of the declaration.Transports", 1, declaration.Transports.Count);
			transportOther.JW_ETD = new ZDateTime(2009, 2, 2);
			transportOther.JW_ETA = new ZDateTime(2009, 2, 3);
			OrgHeader carrier2 = Factory.New<OrgHeader>();
			carrier2.FillWithValidTestData();
			transportOther.CarrierPK = carrier2.PK;
			transportOther.JW_RL_NKDiscPort = "USCHI";
			transportOther.JW_RL_NKLoadPort = "NZCHC";
			transportOther.JW_TransportMode = TransportTypeList.Codes.Air;
			transportOther.JW_Vessel = "QANTAS";
			transportOther.JW_VoyageFlight = "QF123";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("export date should be empty", ZDateTime.Empty, declaration.JE_ExportDate);
			AssertEquals("arrival date should be empty", ZDateTime.Empty, declaration.JE_DateOfArrival);
			AssertEquals("Shipping Line should be empty", ZGuid.Empty, declaration.JE_OH_ShippingLine);
			AssertEquals("Port Of Arrival should be empty", ZString.Empty, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Port Of Loading should be empty", ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Transport Mode should be empty", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("Vessel should be empty", ZString.Empty, declaration.JE_VesselName);
			AssertEquals("Voyage Flight No should be empty", ZString.Empty, declaration.JE_VoyageFlightNo);

			supporter.UpdateAllDeclarationTransportDataIfEmpty(transportOther);

			AssertEquals("export date should not be updated", ZDateTime.Empty, declaration.JE_ExportDate);
			AssertEquals("arrival date should not be updated", ZDateTime.Empty, declaration.JE_DateOfArrival);
			AssertEquals("Shipping Line should not be updated", ZGuid.Empty, declaration.JE_OH_ShippingLine);
			AssertEquals("Port Of Arrival should not be updated", ZString.Empty, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Port Of Loading should not be updated", ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Transport Mode should not be updated", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("Vessel should not be updated", ZString.Empty, declaration.JE_VesselName);
			AssertEquals("Voyage Flight No should not be updated", ZString.Empty, declaration.JE_VoyageFlightNo);

			supporter.UpdateAllDeclarationTransportDataIfEmpty(transport);

			AssertEquals("export date should be updated", new ZDateTime(2009, 1, 2), declaration.JE_ExportDate);
			AssertEquals("arrival date should be updated", new ZDateTime(2009, 1, 3), declaration.JE_DateOfArrival);
			AssertEquals("Shipping Line should be updated", carrier.PK, declaration.JE_OH_ShippingLine);
			AssertEquals("Port Of Arrival should be updated", "USLAX", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Port Of Loading should be updated", "NZAKL", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Transport Mode should be updated", TransportTypeList.Codes.Sea, declaration.JE_TransportMode);
			AssertEquals("Vessel should be updated", "VESSEL", declaration.JE_VesselName);
			AssertEquals("Voyage Flight No should be updated", "SW1", declaration.JE_VoyageFlightNo);
		}

		public void TestGetNewTransportValidation()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ITransportParent routingParent = declaration;
			TransportSupporter supporter = routingParent.TransportSupporter;

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF202";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);

			Transport transport = declaration.Transports[0];

			AssertEquals(typeof(BaseJobDeclarationTransportValidation), supporter.GetNewTransportValidator(transport).GetType());
		}

		public void TestNotifyVoyageUpdated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback; // stop synchronisation
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			var msDEP = declaration.WorkflowItems.Milestones.AddNew();
			msDEP.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			AssertEquals("Precondition: DEP Milestone Actual Date", ZDateTime.Empty, msDEP.P9_ActualDate);
			var msARV = declaration.WorkflowItems.Milestones.AddNew();
			msARV.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			AssertEquals("Precondition: ARV Milestone Actual Date", ZDateTime.Empty, msARV.P9_ActualDate);
			var msCAV = declaration.WorkflowItems.Milestones.AddNew();
			msCAV.TriggerConditions.TriggerEventCode = Events.CargoAvailableCode;
			AssertEquals("Precondition: CAV Milestone Actual Date", ZDateTime.Empty, msCAV.P9_ActualDate);
			ITransportParent routingParent = declaration;
			var supporter = (BaseJobDeclarationTransportSupporter<BaseJobDeclaration>)routingParent.TransportSupporter;
			AssertEquals("Precondition: declaration.Transports.Count", 0, declaration.Transports.Count);

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_ATD = new ZDateTime(2016, 1, 1);
			transport1.JW_ATA = new ZDateTime(2016, 1, 2);
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.FillWithValidTestData();
			transport1.CarrierPK = carrier1.PK;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_TransportMode = TransportTypeList.Codes.Sea;
			transport1.JW_Vessel = "VESSEL1";
			transport1.JW_VoyageFlight = "SW1";
			transport1.JW_LegOrder = 1;

			var transport2 = declaration.Transports.AddNew();
			var carrier2 = Factory.New<OrgHeader>();
			carrier2.FillWithValidTestData();
			transport2.CarrierPK = carrier2.PK;
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_TransportMode = TransportTypeList.Codes.Sea;
			transport2.JW_Vessel = "VESSEL2";
			transport2.JW_VoyageFlight = "SW2";
			transport2.JW_LegOrder = 2;

			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = "VESSEL2";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "SW2";
			var origin = voyage.Origins.AddNew();
			origin.JA_A_DEP = new ZDateTime(2016, 1, 3);
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			var destination = voyage.Destinations.AddNew();
			destination.JB_A_ARV = new ZDateTime(2016, 1, 4);
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_AvailabilityDate = new ZDate(2016, 1, 5);

			transport2.JW_IsLinked = true;
			transport2.JW_JX = voyage.Sailings[0].PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("export date should be empty", ZDateTime.Empty, declaration.JE_ExportDate);
			AssertEquals("arrival date should be empty", ZDateTime.Empty, declaration.JE_DateOfArrival);
			AssertEquals("Precondition: DEP Milestone Actual Date", ZDateTime.Empty, msDEP.P9_ActualDate);
			AssertEquals("Precondition: ARV Milestone Actual Date", ZDateTime.Empty, msARV.P9_ActualDate);
			AssertEquals("Precondition: CAV Milestone Actual Date", ZDateTime.Empty, msCAV.P9_ActualDate);
			AssertEquals(0, declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DepartureCode).Count());
			AssertEquals(0, declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.ArrivalCode).Count());
			AssertEquals(0, declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.CargoAvailableCode).Count());

			supporter.NotifyVoyageUpdated(transport1);
			supporter.NotifyVoyageUpdated(transport2);

			AssertEquals("export date should be updated", new ZDateTime(2016, 1, 1), declaration.JE_ExportDate);
			AssertEquals("arrival date should be updated", new ZDateTime(2016, 1, 2), declaration.JE_DateOfArrival);
			AssertEquals("DEP Event Time", new ZDateTime(2016, 1, 1), declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DepartureCode).First().SL_EventTime);
			AssertEquals("ARV Event Time", new ZDateTime(2016, 1, 4), declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.ArrivalCode).First().SL_EventTime);
			AssertEquals("CAV Event Time", new ZDateTime(2016, 1, 5), declaration.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.CargoAvailableCode).First().SL_EventTime);
			AssertEquals("DEP Milestone Actual Date", new ZDateTime(2016, 1, 1), msDEP.P9_ActualDate);
			AssertEquals("ARV Milestone Actual Date", new ZDateTime(2016, 1, 4), msARV.P9_ActualDate);
			AssertEquals("CAV Milestone Actual Date", new ZDateTime(2016, 1, 5), msCAV.P9_ActualDate);
		}

		public void TestNotifyVoyageUpdated_DoesNotTriggerApportionment_WhenThereAreNoChanges()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ITransportParent routingParent = declaration;
			TransportSupporter supporter = routingParent.TransportSupporter;

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF202";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_ExportDate = new ZDateTime(2018, 1, 1);
			declaration.JE_DateOfArrival = new ZDateTime(2018, 2, 1);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1.0m;

			AssertEquals("Precondition: declaration.Transports.Count", 1, declaration.Transports.Count);
			Transport transport = declaration.Transports[0];
			transport.JW_IsLinked = true;

			AssertEquals("Precondition: JW_IsLinked", true, transport.JW_IsLinked);
			AssertEquals("Precondition: JW_ETD", new ZDateTime(2018, 1, 1), transport.JW_ETD);
			AssertEquals("Precondition: JW_ETA", new ZDateTime(2018, 2, 1), transport.JW_ETA);
			AssertEquals("Precondition: JW_ATD", ZDateTime.Empty, transport.JW_ATD);
			AssertEquals("Precondition: JW_ATA", ZDateTime.Empty, transport.JW_ATA);
			AssertEquals("Precondition: JE_ExportDate", new ZDateTime(2018, 1, 1), declaration.JE_ExportDate);
			AssertEquals("Precondition: JE_DateOfArrival", new ZDateTime(2018, 2, 1), declaration.JE_DateOfArrival);

			Factory.Save();

			AssertEquals("Precondition: ApportionmentDirty", false, declaration.ApportionmentDirty);

			supporter.NotifyVoyageUpdated(transport);

			AssertEquals("ApportionmentDirty", false, declaration.ApportionmentDirty);

			declaration.JE_ExportDate = new ZDateTime(2018, 3, 3);
			AssertEquals("This test assumes that changing JE_ExportDate triggers apportionment. If it is not true, than this test does not test anything.", true, declaration.ApportionmentDirty);
		}

		public void TestUpdateRoutingDefaultIfAllowed()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var transport = declaration.Transports.AddNew();

			declaration.UpdateRoutingDefaultIfAllowedAndSingleLeg(() =>
			{
				declaration.UpdateRoutingDefaultIfAllowed(declaration.JE_RL_NKPortOfLoadingInfo, (ZString)"NZAKL");
				AssertEquals("Should blocked the update with RoutingDefaultLock engaged", ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			}, transport);

			declaration.UpdateRoutingDefaultIfAllowed(declaration.JE_RL_NKPortOfLoadingInfo, (ZString)"NZAKL");
			AssertEquals("Should allowed the update with RoutingDefaultLock released", "NZAKL", declaration.JE_RL_NKPortOfLoading);
		}

		public void TestUpdateRoutingDefaultIfAllowedAndSingleLeg()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var transport1 = declaration.Transports.AddNew();

			declaration.UpdateRoutingDefaultIfAllowedAndSingleLeg(() =>
			{
				declaration.UpdateRoutingDefaultIfAllowedAndSingleLeg(declaration.JE_RL_NKPortOfLoadingInfo, (ZString)"NZAKL", transport1);
				AssertEquals("Should blocked the update with RoutingDefaultLock engaged", ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			}, transport1);

			declaration.UpdateRoutingDefaultIfAllowedAndSingleLeg(declaration.JE_RL_NKPortOfLoadingInfo, (ZString)"NZAKL", transport1);
			AssertEquals("Should allowed the update with RoutingDefaultLock released", "NZAKL", declaration.JE_RL_NKPortOfLoading);

			var transport2 = declaration.Transports.AddNew();
			declaration.UpdateRoutingDefaultIfAllowedAndSingleLeg(declaration.JE_RL_NKPortOfLoadingInfo, (ZString)"NZBUW", transport2);
			AssertEquals("Should blocked the update due to multiple legs", "NZAKL", declaration.JE_RL_NKPortOfLoading);
		}

		public void TestNotifyATAChangedCore()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			AssertNull("Precondition: declaration doesn't have event ARV", declaration.Logs.MostRecentLogByEventTime(Events.Arrival));

			var transport = declaration.Transports.AddNew();
			var carrier = Factory.New<OrgHeader>();
			carrier.FillWithValidTestData();
			transport.CarrierPK = carrier.PK;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_TransportMode = TransportTypeList.Codes.Air;
			transport.JW_Vessel = "QANTAS";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_LegOrder = 1;
			transport.JW_ETA = new ZDateTime(2016, 1, 2);
			transport.JW_ATA = new ZDateTime(2016, 1, 2);

			var arrivalEvent = declaration.Logs.MostRecentLogByEventTime(Events.Arrival, new ZQuery(StmALogSchema.SL_Reference, "|FAC=CTO|FDT=2016-01-02|LOC=USLAX|VFL=QF123"));
			CombineAssertions(() =>
			{
				AssertEquals("SL_EventTime", new ZDateTime(2016, 1, 2), arrivalEvent.SL_EventTime);
				AssertEquals("SL_IsEstimate", false, arrivalEvent.SL_IsEstimate);
			});
		}

		public void TestNotifyATDChangedCore()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			AssertNull("Precondition: declaration doesn't have event DEP", declaration.Logs.MostRecentLogByEventTime(Events.Departure));

			var transport = declaration.Transports.AddNew();
			var carrier = Factory.New<OrgHeader>();
			carrier.FillWithValidTestData();
			transport.CarrierPK = carrier.PK;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_TransportMode = TransportTypeList.Codes.Air;
			transport.JW_Vessel = "QANTAS";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_LegOrder = 1;
			transport.JW_ETD = new ZDateTime(2016, 1, 2);
			transport.JW_ATD = new ZDateTime(2016, 1, 2);

			var departureEvent = declaration.Logs.MostRecentLogByEventTime(Events.Departure, new ZQuery(StmALogSchema.SL_Reference, "|FAC=CTO|FDT=2016-01-02|LOC=AUSYD|VFL=QF123"));
			CombineAssertions(() =>
			{
				AssertEquals("SL_EventTime", new ZDateTime(2016, 1, 2), departureEvent.SL_EventTime);
				AssertEquals("SL_IsEstimate", false, departureEvent.SL_IsEstimate);
			});
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<BaseJobDeclaration>();
			return parent.TransportSupporter;
		}

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}
	}
}
