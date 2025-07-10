using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingScheduleUpdateSubscriberTest : ScheduleUpdateSubscriberTest<ForwardingScheduleUpdateSubscriber>
	{
		public void TestUpdateAllRelatedShipmentsETDAndETA_ForDeclaration()
		{
			var dec1 = DeclarationWithRelatedVoyage;
			var dec2 = MessagingDeclarationWithRelatedVoyage;
			var voyage = VoyageWithRelatedDeclaration;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			dec1 = (BusinessObject)newFactory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(dec1.PK);
			dec2 = (BusinessObject)newFactory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(dec2.PK);
			voyage = newFactory.Load<JobVoyage>(voyage.PK);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			voyage.Origins[0].JA_E_DEP = new ZDateTime(2001, 2, 2);

			AssertContains("The following declarations were processed:", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("\r\nDEC2 has not been updated as messages have been sent", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("\r\nDEC1 ETD updated", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			voyage.Destinations[0].JB_E_ARV = new ZDateTime(2002, 2, 2);
			AssertContains("The following declarations were processed:", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("\r\nDEC2 has not been updated as messages have been sent", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("\r\nDEC1 ETA updated", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("ETD updated", new ZDateTime(2001, 2, 2), dec1[JobDeclarationSchema.JE_DateAtOrigin]);
			AssertEquals("ETA updated", new ZDateTime(2002, 2, 2), dec1[JobDeclarationSchema.JE_DateAtFinalDestination]);
			AssertEquals("ETD not updated", new ZDateTime(2001, 1, 1), dec2[JobDeclarationSchema.JE_DateAtOrigin]);
			AssertEquals("ETA not updated", new ZDateTime(2002, 1, 1), dec2[JobDeclarationSchema.JE_DateAtFinalDestination]);
		}

		public void TestLoadingDeclarationMatchingOldDatesOnlyToAvoidPerformanceIssues()
		{
			var currentDate = ZDateTime.Today;

			var declaration1 = NewDeclarationWithRelatedVoyage("AIR", "", "HJ2", "NZAKL", "NZWEL", "AUMEL", "AUADL", currentDate.AddDays(-14), currentDate.AddDays(-13), "DEC1");
			var declaration2 = NewDeclarationWithRelatedVoyage("AIR", "", "HJ2", "NZAKL", "NZWEL", "AUMEL", "AUADL", currentDate.AddDays(-12), currentDate.AddDays(-12), "DEC2");
			var declaration3 = NewDeclarationWithRelatedVoyage("AIR", "", "HJ2", "NZAKL", "NZWEL", "AUMEL", "AUADL", currentDate.AddDays(1), currentDate.AddDays(1), "DEC3");
			declaration3.Factory.Save();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_FlightDate = currentDate.AddDays(3);
			voyage.JV_VoyageFlight = "HJ2";
			voyage.JV_AirSeaRoad = "AIR";

			var declarationsReturned = ForwardingScheduleUpdateSubscriber.LoadDeclarationsForLoading(voyage, new ZString("NZAKL"), voyage.JV_FlightDate, currentDate.AddDays(-12).AddMinutes(10));
			AssertEquals("count returned", 1, declarationsReturned.Length);
			AssertEquals("DeclarationReference", "DEC2", declarationsReturned[0][JobDeclarationSchema.JE_DeclarationReference]);

			declarationsReturned = ForwardingScheduleUpdateSubscriber.LoadDeclarationsForDischarge(voyage, new ZString("AUMEL"), voyage.JV_FlightDate, currentDate.AddDays(-13).AddMinutes(10));
			AssertEquals("count returned", 1, declarationsReturned.Length);
			AssertEquals("DeclarationReference", "DEC1", declarationsReturned[0][JobDeclarationSchema.JE_DeclarationReference]);
		}

		public void TestLoadDeclarationsWouldAcknowledgeCarrier()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();
			var carrier3 = Factory.New<OrgHeader>();

			var mainVoyage = Factory.New<JobVoyage>();
			mainVoyage.JV_VoyageType = Constants.VoyageType.MainVoyage;
			mainVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			mainVoyage.JV_RV_NKVessel = "Visund";
			mainVoyage.JV_VoyageFlight = "123";
			mainVoyage.JV_OH_Line = carrier1.PK;

			mainVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			mainVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

			var slotVoyage = Factory.New<JobVoyage>();
			slotVoyage.JV_VoyageType = Constants.VoyageType.SlotVoyage;
			slotVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			slotVoyage.JV_RV_NKVessel = "Visund";
			slotVoyage.JV_VoyageFlight = "123";
			slotVoyage.JV_OH_Line = carrier2.PK;

			slotVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			slotVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";

			var currentDate = ZDateTime.Today;

			Func<ZGuid, BusinessObject> createDeclaration = (carrierPK) =>
			{
				var declaration = NewDeclarationWithRelatedVoyage("SEA", "Visund", "123", "NZAKL", "NZWEL", "AUMEL", "AUADL", currentDate, currentDate, "");
				declaration[JobDeclarationSchema.JE_OH_ShippingLine] = carrierPK;

				return declaration;
			};

			var declaration1 = createDeclaration(carrier1.PK);
			var declaration2 = createDeclaration(carrier2.PK);
			var declaration3 = createDeclaration(carrier3.PK);

			var declarationsReturned = ForwardingScheduleUpdateSubscriber.LoadDeclarationsForLoading(mainVoyage, "NZAKL", currentDate.AddDays(1), currentDate);
			AssertContainsExactElementsInAnyOrder("Main voyage: Declarations with carriers with their own SLOT voyage are ignored",
				new[] { declaration1, declaration3 }, declarationsReturned);

			declarationsReturned = ForwardingScheduleUpdateSubscriber.LoadDeclarationsForLoading(slotVoyage, "NZAKL", currentDate.AddDays(1), currentDate);
			AssertContainsExactElementsInAnyOrder("Slot voyage: Returning declarations with selected carrier only",
				new[] { declaration2 }, declarationsReturned);
		}

		public void TestUpdateAllRelatedShipmentsETDAndETA_ForDeclaration_NotSaved()
		{
			DeclarationWithRelatedVoyage.Factory.Save();

			VoyageWithRelatedDeclaration.Origins[0].JA_E_DEP = new ZDateTime(2001, 5, 2);
			VoyageWithRelatedDeclaration.Destinations[0].JB_E_ARV = new ZDateTime(2002, 5, 2);

			VoyageWithRelatedDeclaration.Origins[0].JA_E_DEP = new ZDateTime(2001, 2, 2);
			VoyageWithRelatedDeclaration.Destinations[0].JB_E_ARV = new ZDateTime(2002, 2, 2);

			Factory.Save();

			AssertNotEquals("ETD NOT updated", new ZDateTime(2001, 2, 2), DeclarationWithRelatedVoyage[JobDeclarationSchema.JE_DateAtOrigin]);
			AssertNotEquals("ETA NOT updated", new ZDateTime(2002, 2, 2), DeclarationWithRelatedVoyage[JobDeclarationSchema.JE_DateAtFinalDestination]);
		}

		public void TestUpdateAllRelatedShipmentsETDAndETA_SingleShipmentOnConsol()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();

			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2004, 04, 14);
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2004, 04, 16);
			voyage.GenerateSailings();
			Assert("Sailings.Count > 0", voyage.Sailings.Count > 0);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;
			consol.Voyage.ParentConsol = consol; //TODO: Refer to comments in UpdateAllRelatedShipments

			CommonShipment shipment = consol.Shipments.AddNew();
			AssertEquals("CommonShipment ETD", origin.JA_E_DEP, shipment.JS_E_DEP);
			AssertEquals("CommonShipment ETA", destination.JB_E_ARV, shipment.JS_E_ARV);

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = voyage.Sailings[0].PK;

			//Consol2.Voyage.ParentConsol = Consol2; //TODO: Refer to comments in UpdateAllRelatedShipments
			CommonShipment shipment2 = consol2.Shipments.AddNew();
			AssertEquals("Shipment2 ETD", origin.JA_E_DEP, shipment2.JS_E_DEP);
			AssertEquals("Shipment2 ETA", destination.JB_E_ARV, shipment2.JS_E_ARV);

			shipment.JS_UniqueConsignRef = "Shipment1";
			shipment2.JS_UniqueConsignRef = "Shipment2";

			Factory.Save();

			shipment.JS_E_DEP = shipment.JS_E_DEP.AddDays(1);
			shipment.JS_E_ARV = shipment.JS_E_ARV.AddDays(1);
			AssertEquals("CommonShipment ETD", origin.JA_E_DEP.AddDays(1), shipment.JS_E_DEP);
			AssertEquals("CommonShipment ETA", destination.JB_E_ARV.AddDays(1), shipment.JS_E_ARV);
			AssertEquals("Shipment2 ETD", origin.JA_E_DEP, shipment2.JS_E_DEP);
			AssertEquals("Shipment2 ETA", destination.JB_E_ARV, shipment2.JS_E_ARV);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			origin.JA_E_DEP = origin.JA_E_DEP.AddDays(2);
			destination.JB_E_ARV = destination.JB_E_ARV.AddDays(3);
			AssertEquals("The following shipments were processed:\r\nShipment1 ETA updated\r\nShipment2 ETA updated", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("CommonShipment ETD", origin.JA_E_DEP.AddDays(1), shipment.JS_E_DEP);
			AssertEquals("CommonShipment ETA", destination.JB_E_ARV.AddDays(1), shipment.JS_E_ARV);
			AssertEquals("Shipment2 ETD", origin.JA_E_DEP, shipment2.JS_E_DEP);
			AssertEquals("Shipment2 ETA", destination.JB_E_ARV, shipment2.JS_E_ARV);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			origin.JA_E_DEP = origin.JA_E_DEP.AddDays(-3);
			destination.JB_E_ARV = destination.JB_E_ARV.AddDays(-2);
			AssertEquals("The following shipments were processed:\r\nShipment1 ETA updated\r\nShipment2 ETA updated", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("CommonShipment ETD", origin.JA_E_DEP.AddDays(1), shipment.JS_E_DEP);
			AssertEquals("CommonShipment ETA", destination.JB_E_ARV.AddDays(1), shipment.JS_E_ARV);
			AssertEquals("Shipment2 ETD", origin.JA_E_DEP, shipment2.JS_E_DEP);
			AssertEquals("Shipment2 ETA", destination.JB_E_ARV, shipment2.JS_E_ARV);
		}

		public void TestUpdateTranshipETA_MultipleConsols()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
			voyage1.JV_VoyageFlight = "1321";

			VoyageOrigin origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = new ZDateTime(2004, 8, 5);
			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = new ZDateTime(2004, 8, 18);

			AssertEquals("Precondition: Expecting voyage1 to have 1 sailing.", 1, voyage1.Sailings.Count);

			JobSailing sailing1 = voyage1.Sailings[0];

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = voyage1.JV_RV_NKVessel;
			voyage2.JV_VoyageFlight = "1325";
			VoyageOrigin origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = new ZDateTime(2004, 8, 21);
			VoyageDestination destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "JPOSA";
			destination2.JB_E_ARV = new ZDateTime(2004, 9, 4);

			AssertEquals("Precondition: Expecting voyage2 to have 1 sailing.", 1, voyage2.Sailings.Count);

			JobSailing sailing2 = voyage2.Sailings[0];

			Factory.Save();

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = sailing1.PK;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = sailing2.PK;

			Factory.Save();

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.Consols.Add(consol1);
			shipment1.Consols.Add(consol2);
			shipment1.JS_RL_NKOrigin = "GBLON";
			shipment1.JS_RL_NKDestination = "JPTYO";
			shipment1.JS_E_DEP = new ZDateTime(2004, 8, 1);
			shipment1.JS_E_ARV = new ZDateTime(2004, 9, 9);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.Consols.Add(consol1);
			shipment2.JS_RL_NKOrigin = "GBLON";
			shipment2.JS_RL_NKDestination = "SGSIN";
			shipment2.JS_E_DEP = new ZDateTime(2004, 8, 4);
			shipment2.JS_E_ARV = new ZDateTime(2004, 8, 20);

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.Consols.Add(consol2);
			shipment3.JS_RL_NKOrigin = "SGSIN";
			shipment3.JS_RL_NKDestination = "JPOSA";
			shipment3.JS_E_DEP = new ZDateTime(2004, 8, 20);
			shipment3.JS_E_ARV = new ZDateTime(2004, 9, 6);

			Factory.Save();

			transport1.JW_ETA = ZDateTime.Invalid;

			//Set eta date on consol 1 back, to see if shipments are updated correctly
			transport1.JW_ETA = new ZDateTime(2004, 8, 15);

			AssertEquals("Expecting Shipment2's eta not to change just because its ship is arriving 3 days earlier.", new ZDateTime(2004, 8, 20), shipment2.JS_E_ARV);

			AssertEquals("Not expecting Shipment3's eta to change - it's not affected by this sailing.", new ZDateTime(2004, 9, 6), shipment3.JS_E_ARV);

			AssertEquals("Not Expecting Shipment1's eta to change, it's first consol is arriving in sgsin 3 days earlier,"
				+ "but it will not get to jptyo any sooner, as it is arriving there by a second ship.", new ZDateTime(2004, 9, 9), shipment1.JS_E_ARV);
		}

		public void TestUpdateLastETA_MultipleConsols()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
			voyage1.JV_VoyageFlight = "1321";
			VoyageOrigin origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = new ZDateTime(2004, 8, 5);
			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = new ZDateTime(2004, 8, 18);

			AssertEquals("Precondition: Expecting voyage1 to have 1 sailing.", 1, voyage1.Sailings.Count);

			JobSailing sailing1 = voyage1.Sailings[0];

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = voyage1.JV_RV_NKVessel;
			voyage2.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage2.JV_VoyageFlight = "1323";
			VoyageOrigin origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = new ZDateTime(2004, 8, 21);
			VoyageDestination destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "JPOSA";
			destination2.JB_E_ARV = new ZDateTime(2004, 9, 4);

			AssertEquals("Precondition: Expecting voyage2 to have 1 sailing.", 1, voyage2.Sailings.Count);

			JobSailing sailing2 = voyage2.Sailings[0];

			Factory.Save();

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = sailing1.PK;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_UniqueConsignRef = "Consol2";
			consol2.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = sailing2.PK;

			Factory.Save();

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.Consols.Add(consol1);
			shipment1.Consols.Add(consol2);
			shipment1.JS_RL_NKOrigin = "GBLON";
			shipment1.JS_RL_NKDestination = "JPTYO";
			shipment1.JS_E_DEP = new ZDateTime(2004, 8, 1);
			shipment1.JS_E_ARV = new ZDateTime(2004, 9, 9);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_UniqueConsignRef = "Shipment2";
			shipment2.Consols.Add(consol1);
			shipment2.JS_RL_NKOrigin = "GBLON";
			shipment2.JS_RL_NKDestination = "SGSIN";
			shipment2.JS_E_DEP = new ZDateTime(2004, 8, 4);
			shipment2.JS_E_ARV = new ZDateTime(2004, 8, 20);

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_UniqueConsignRef = "Shipment3";
			shipment3.Consols.Add(consol2);
			shipment3.JS_RL_NKOrigin = "SGSIN";
			shipment3.JS_RL_NKDestination = "JPOSA";
			shipment3.JS_E_DEP = new ZDateTime(2004, 8, 20);
			shipment3.JS_E_ARV = new ZDateTime(2004, 9, 6);

			Factory.Save();
			AssertNotNull("Consol Sailing", transport2.Sailing);
			//Set eta date on consol 2 back, to see if shipments are updated correctly
			transport2.JW_ETA = new ZDateTime(2004, 9, 9);

			AssertEquals("Not Expecting Shipment1's eta to change, even though it is 5 days later it is on the original arrival date", new ZDateTime(2004, 9, 14), shipment1.JS_E_ARV);
			AssertEquals("Not expecting Shipment2's eta to change - it's not affected by this sailing.", new ZDateTime(2004, 8, 20), shipment2.JS_E_ARV);
			AssertEquals("Expecting Shipment3's eta to change, It consol is arriving later than its original Arrival Date", new ZDateTime(2004, 9, 11), shipment3.JS_E_ARV);

			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			filter.AddToFilter(StmALogSchema.SL_IsCancelled, "N");
			filter.AddToFilter(StmALogSchema.SL_Parent, voyage2.PK);
			filter.OrderBy = StmALog.Schema.SL_EventTime + OrderByClause.Descending;

			var dateChangeLog = Factory.LoadTop1<StmALog>(filter);
			AssertNotNull(dateChangeLog);
			AssertEquals("Expecting date change to be logged.", "JPOSA ETA edited from " + new ZDateTime(2004, 9, 4) + " to " + new ZDateTime(2004, 9, 9) + ". Updated related shipment's ETAs.", dateChangeLog.SL_Reference);
		}

		public void TestUpdateFirstETD_MultipleConsols()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
			voyage1.JV_VoyageFlight = "1321";
			VoyageOrigin origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = new ZDateTime(2004, 8, 5);
			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = new ZDateTime(2004, 8, 18);

			AssertEquals("Precondition: Expecting voyage1 to have 1 sailing.", 1, voyage1.Sailings.Count);

			JobSailing sailing1 = voyage1.Sailings[0];

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = voyage1.JV_RV_NKVessel;
			voyage2.JV_VoyageFlight = "1325";
			VoyageOrigin origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = new ZDateTime(2004, 8, 21);
			VoyageDestination destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "JPOSA";
			destination2.JB_E_ARV = new ZDateTime(2004, 9, 4);

			AssertEquals("Precondition: Expecting voyage2 to have 1 sailing.", 1, voyage2.Sailings.Count);

			JobSailing sailing2 = voyage2.Sailings[0];

			Factory.Save();

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = sailing1.PK;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = sailing2.PK;

			AssertEquals("Consol 1 Sailing", sailing1.PK, transport1.JW_JX);
			AssertEquals("Consol 2 Sailing", sailing2.PK, transport2.JW_JX);

			Factory.Save();

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.Consols.Add(consol1);
			shipment1.Consols.Add(consol2);
			shipment1.JS_RL_NKOrigin = "GBLON";
			shipment1.JS_RL_NKDestination = "JPTYO";
			shipment1.JS_E_DEP = new ZDateTime(2004, 8, 1);
			shipment1.JS_E_ARV = new ZDateTime(2004, 9, 9);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.Consols.Add(consol1);
			shipment2.JS_RL_NKOrigin = "GBLON";
			shipment2.JS_RL_NKDestination = "SGSIN";
			shipment2.JS_E_DEP = new ZDateTime(2004, 8, 4);
			shipment2.JS_E_ARV = new ZDateTime(2004, 8, 20);

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.Consols.Add(consol2);
			shipment3.JS_RL_NKOrigin = "SGSIN";
			shipment3.JS_RL_NKDestination = "JPOSA";
			shipment3.JS_E_DEP = new ZDateTime(2004, 8, 20);
			shipment3.JS_E_ARV = new ZDateTime(2004, 9, 6);

			Factory.Save();

			//Set etd date on consol 1 forward, to see if shipments are updated correctly
			transport1.JW_ETD = new ZDateTime(2004, 8, 6);

			AssertEquals("Not Expecting Sailing Object To Change", sailing1.PK, transport1.Sailing.PK);

			AssertEquals("Expecting Shipment1's etd to change, it is leaving 1 day later ", new ZDateTime(2004, 8, 2), shipment1.JS_E_DEP);
			AssertEquals("Expecting Shipment2's etd to change, it is leaving 1 day later ", new ZDateTime(2004, 8, 5), shipment2.JS_E_DEP);
			AssertEquals("Not expecting Shipment3's etd to change - it's not affected by this sailing.", new ZDateTime(2004, 8, 20), shipment3.JS_E_DEP);

			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			filter.AddToFilter(StmALogSchema.SL_IsCancelled, "N");
			filter.AddToFilter(StmALogSchema.SL_Parent, voyage1.PK);
			filter.OrderBy = StmALog.Schema.SL_EventTime + OrderByClause.Descending;

			var dateChangeLog = Factory.LoadTop1<StmALog>(filter);
			AssertNotNull(dateChangeLog);
			AssertEquals("Expecting date change to be logged.", "GBLON ETD edited from " + new ZDateTime(2004, 8, 5) + " to " + new ZDateTime(2004, 8, 6) + ". Updated related shipment's ETDs.", dateChangeLog.SL_Reference);
		}

		public void TestUpdateNonForwardingJobs()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
			voyage1.JV_VoyageType = Enterprise.Core.Constants.TransportModes.Sea;
			voyage1.JV_VoyageFlight = "1321";
			VoyageOrigin origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = new ZDateTime(2004, 8, 5);
			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = new ZDateTime(2004, 8, 18);
			Factory.Save();

			AssertEquals("Precondition: Expecting voyage1 to have 1 sailing.", 1, voyage1.Sailings.Count);

			JobSailing sailing1 = voyage1.Sailings[0];
			CommonConsol testConsol = Factory.New<CommonConsol>();
			testConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport testTransport = testConsol.Transports[0];
			testTransport.JW_JX = sailing1.PK;

			CommonShipment testShipment = (CommonShipment)testConsol.Shipments.AddNew(typeof(CommonShipment));

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = sailing1.PK;
			transport1.JW_ETA = new ZDateTime(2004, 8, 19);
			AssertEquals("Test Base CommonShipment should have been updated correctly through the voyage", consol1.JK_JX_JB_E_ARV, testShipment.JS_E_ARV);
		}

		public void TestUpdateShipmentDates_MultipleConsolsPerLoadPort_OneConsolPerShipment()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
			voyage1.JV_VoyageFlight = "1321";

			VoyageOrigin origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = new ZDateTime(2004, 8, 5);

			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = new ZDateTime(2004, 8, 18);

			VoyageDestination destination2 = voyage1.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "MYPKG";
			destination2.JB_E_ARV = new ZDateTime(2004, 8, 21);

			AssertEquals("Precondition: Expecting voyage1 to have 2 sailing.", 2, voyage1.Sailings.Count);

			JobSailing sailing1 = voyage1.Sailings.GetSailingFromLoadAndDischarge("GBLON", "SGSIN");

			JobSailing sailing2 = voyage1.Sailings.GetSailingFromLoadAndDischarge("GBLON", "MYPKG");

			Factory.Save();

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = sailing1.PK;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = sailing2.PK;

			Factory.Save();

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.Consols.Add(consol1);
			shipment1.JS_HouseBill = "Ship 1";
			shipment1.JS_RL_NKOrigin = "GBLON";
			shipment1.JS_RL_NKDestination = "SGSIN";
			shipment1.JS_E_DEP = new ZDateTime(2004, 8, 1);
			shipment1.JS_E_ARV = new ZDateTime(2004, 8, 20);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.Consols.Add(consol1);
			shipment2.JS_HouseBill = "Ship 2";
			shipment2.JS_RL_NKOrigin = "GBLON";
			shipment2.JS_RL_NKDestination = "SGSIN";
			shipment2.JS_E_DEP = new ZDateTime(2004, 8, 4);
			shipment2.JS_E_ARV = new ZDateTime(2004, 8, 21);

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.Consols.Add(consol2);
			shipment3.JS_HouseBill = "Ship 3";
			shipment3.JS_RL_NKOrigin = "GBLON";
			shipment3.JS_RL_NKDestination = "MYPKG";
			shipment3.JS_E_DEP = new ZDateTime(2004, 8, 3);
			shipment3.JS_E_ARV = new ZDateTime(2004, 8, 24);

			Factory.Save();

			//Set eta date on consol 1 back, to see if shipments are updated correctly
			transport1.JW_ETA = new ZDateTime(2004, 8, 15);

			AssertEquals("Expecting Shipment1's eta to be updated.", new ZDateTime(2004, 8, 17), shipment1.JS_E_ARV);
			AssertEquals("Expecting Shipment2's eta to be updated.", new ZDateTime(2004, 8, 18), shipment2.JS_E_ARV);
			AssertEquals("Not Expecting Shipment3's eta to change", new ZDateTime(2004, 8, 24), shipment3.JS_E_ARV);

			transport2.JW_ETD = new ZDateTime(2004, 8, 2);
			AssertEquals("Expecting Shipment1's etd to be unchanged - its ship is leaving.", new ZDateTime(2004, 7, 29), shipment1.JS_E_DEP);
			AssertEquals("Expecting Shipment2's etd to be 2004/8/1 - When this ship leaves", new ZDateTime(2004, 8, 1), shipment2.JS_E_DEP);
			AssertEquals("Expecting Shipment3's etd to be 2004/7/31 - When its ship is leaving.", new ZDateTime(2004, 7, 31), shipment3.JS_E_DEP);
		}

		public void TestUpdateTranshipETD_MultipleConsols()
		{
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_VoyageType = Enterprise.Core.Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
			voyage1.JV_VoyageFlight = "1321";
			VoyageOrigin origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = new ZDateTime(2004, 8, 5);
			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = new ZDateTime(2004, 8, 18);

			AssertEquals("Precondition: Expecting voyage1 to have 1 sailing.", 1, voyage1.Sailings.Count);

			JobSailing sailing1 = voyage1.Sailings[0];

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = voyage1.JV_RV_NKVessel;
			voyage2.JV_VoyageFlight = "1325";
			VoyageOrigin origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = new ZDateTime(2004, 8, 21);
			VoyageDestination destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "JPOSA";
			destination2.JB_E_ARV = new ZDateTime(2004, 9, 4);

			AssertEquals("Precondition: Expecting voyage2 to have 1 sailing.", 1, voyage2.Sailings.Count);

			JobSailing sailing2 = voyage2.Sailings[0];

			Factory.Save();

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = sailing1.PK;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = sailing2.PK;

			Factory.Save();

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.Consols.Add(consol1);
			shipment1.Consols.Add(consol2);
			shipment1.JS_RL_NKOrigin = "GBLON";
			shipment1.JS_RL_NKDestination = "JPTYO";
			shipment1.JS_E_DEP = new ZDateTime(2004, 8, 1);
			shipment1.JS_E_ARV = new ZDateTime(2004, 9, 9);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.Consols.Add(consol1);
			shipment2.JS_RL_NKOrigin = "GBLON";
			shipment2.JS_RL_NKDestination = "SGSIN";
			shipment2.JS_E_DEP = new ZDateTime(2004, 8, 4);
			shipment2.JS_E_ARV = new ZDateTime(2004, 8, 20);

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.Consols.Add(consol2);
			shipment3.JS_RL_NKOrigin = "SGSIN";
			shipment3.JS_RL_NKDestination = "JPOSA";
			shipment3.JS_E_DEP = new ZDateTime(2004, 8, 20);
			shipment3.JS_E_ARV = new ZDateTime(2004, 9, 6);

			Factory.Save();

			//Set etd date on consol 2 forward, to see if shipments are updated correctly
			transport2.JW_ETD = new ZDateTime(2004, 8, 23);

			AssertEquals("Expecting Shipment3's etd to changed because its ship is leaving 2 days later.", new ZDateTime(2004, 8, 22), shipment3.JS_E_DEP);

			AssertEquals("Not expecting Shipment2's etd to change - it's not affected by this sailing.", new ZDateTime(2004, 8, 4), shipment2.JS_E_DEP);

			AssertEquals("Not expecting Shipment1's etd to change, it's first consol is leaving sgsin 2 days later,"
				+ "but it cannot leave gblon any sooner, as it is leaving there by a different ship.", new ZDateTime(2004, 8, 1), shipment1.JS_E_DEP);
		}

		public void TestOriginDestinationDateTimeOutOfRangeValuesException()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;
			voyage.JV_VoyageType = Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "1321";
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(1999, 04, 14, 10, 38, 00);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2000, 04, 16, 11, 39, 00);

			Factory.Save();

			origin.JA_E_DEP = new ZDateTime(1889, 04, 14, 10, 38, 00);
			destination.JB_E_ARV = new ZDateTime(2080, 04, 16, 11, 39, 00);

			AssertExceptionThrown("Out of range small date time should throw ArgumentOutOfRangeException", typeof(ArgumentOutOfRangeException), () => { Factory.Save(); });
		}

		[ExpectNoExceptions]
		public void TestLoadDeclarationsForLoadingAndDischargeDateTimeOutOfRangeValuesException()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;
			voyage.JV_VoyageType = Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "4567";
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUMEL";
			origin.JA_E_DEP = new ZDateTime(1999, 08, 14, 10, 38, 00);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2000, 04, 15, 11, 39, 00);

			Factory.Save();

			origin.JA_E_DEP = new ZDateTime(2079, 6, 6, 23, 59, 29);
			destination.JB_E_ARV = new ZDateTime(2079, 6, 6, 23, 59, 29);
		}

		public void TestDoNotUpdateShipmentETA_WhenConsolIntermediateLegETAIsUpdated()
		{
			var today = ZDateTime.Today;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "Vessel Test 1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "Vessel Test 2";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "1321";
			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = today;
			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = today.AddDays(13);

			voyage1.GenerateSailings();
			AssertEquals("Precondition: Expecting voyage1 to have 1 sailing.", 1, voyage1.Sailings.Count);

			var sailing1 = voyage1.Sailings[0];

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage2.JV_VoyageFlight = "1323";
			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = today.AddDays(16);
			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "JPOSA";
			destination2.JB_E_ARV = today.AddDays(30);

			voyage2.GenerateSailings();
			AssertEquals("Precondition: Expecting voyage2 to have 1 sailing.", 1, voyage2.Sailings.Count);

			var sailing2 = voyage2.Sailings[0];

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var transport1 = consol1.Transports[0];
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;

			var transport2 = consol1.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.Consols.Add(consol1);
			shipment1.JS_RL_NKOrigin = "GBLON";
			shipment1.JS_RL_NKDestination = "JPTYO";
			shipment1.JS_E_DEP = today.AddDays(-4);
			shipment1.JS_E_ARV = today.AddDays(35);

			Factory.Save();

			transport1.JW_ETA = today.AddDays(15);
			AssertEquals("Shipment1's ETA should not change", today.AddDays(35), shipment1.JS_E_ARV);
		}

		public void TestDoNotUpdateShipmentETD_WhenConsolIntermediateLegETDIsUpdated()
		{
			var today = ZDateTime.Today;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "Vessel Test 1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "Vessel Test 2";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "1321";
			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = today;
			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = today.AddDays(13);

			voyage1.GenerateSailings();
			AssertEquals("Precondition: Expecting voyage1 to have 1 sailing.", 1, voyage1.Sailings.Count);

			var sailing1 = voyage1.Sailings[0];

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage2.JV_VoyageFlight = "1323";
			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = today.AddDays(16);
			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "JPOSA";
			destination2.JB_E_ARV = today.AddDays(30);

			voyage2.GenerateSailings();
			AssertEquals("Precondition: Expecting voyage2 to have 1 sailing.", 1, voyage2.Sailings.Count);

			var sailing2 = voyage2.Sailings[0];

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var transport1 = consol1.Transports[0];
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;

			var transport2 = consol1.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.Consols.Add(consol1);
			shipment1.JS_RL_NKOrigin = "GBLON";
			shipment1.JS_RL_NKDestination = "JPTYO";
			shipment1.JS_E_DEP = today.AddDays(-4);
			shipment1.JS_E_ARV = today.AddDays(35);

			Factory.Save();

			transport2.JW_ETD = today.AddDays(18);
			AssertEquals("Shipment1's ETD should not change", today.AddDays(-4), shipment1.JS_E_DEP);
		}

		public void TestDoNotUpdateShipmentETA_WhenConsolIntermediateLegETAIsUpdated_MultipleConsols()
		{
			var today = ZDateTime.Today;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "Vessel Test 1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "Vessel Test 2";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "1321";
			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = today;
			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = today.AddDays(13);

			voyage1.GenerateSailings();
			AssertEquals("Precondition: Expecting voyage1 to have 1 sailing.", 1, voyage1.Sailings.Count);

			var sailing1 = voyage1.Sailings[0];

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage2.JV_VoyageFlight = "1323";
			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = today.AddDays(16);
			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "JPOSA";
			destination2.JB_E_ARV = today.AddDays(30);

			voyage2.GenerateSailings();
			AssertEquals("Precondition: Expecting voyage2 to have 1 sailing.", 1, voyage2.Sailings.Count);

			var sailing2 = voyage2.Sailings[0];

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var transport1 = consol1.Transports[0];
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;

			var transport2 = consol1.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.Consols.Add(consol1);
			shipment1.JS_RL_NKOrigin = "GBLON";
			shipment1.JS_RL_NKDestination = "JPTYO";
			shipment1.JS_E_DEP = today.AddDays(-4);
			shipment1.JS_E_ARV = today.AddDays(35);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_UniqueConsignRef = "Consol2";
			consol2.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var transport3 = consol2.Transports[0];
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport3.JW_IsLinked = true;
			transport3.JW_JX = sailing1.PK;

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_UniqueConsignRef = "Shipment2";
			shipment2.Consols.Add(consol2);
			shipment2.JS_RL_NKOrigin = "GBLON";
			shipment2.JS_RL_NKDestination = "SGSIN";
			shipment2.JS_E_DEP = today.AddDays(-4);
			shipment2.JS_E_ARV = today.AddDays(35);

			Factory.Save();

			transport1.JW_ETA = today.AddDays(15);
			AssertEquals("Shipment2's ETA should change", today.AddDays(37), shipment2.JS_E_ARV);
			AssertEquals("Shipment1's ETA should not change", today.AddDays(35), shipment1.JS_E_ARV);
		}

		public void TestDoNotUpdateShipmentETD_WhenConsolIntermediateLegETDIsUpdated_MultipleConsols()
		{
			var today = ZDateTime.Today;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "Vessel Test 1";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "Vessel Test 2";

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "1321";
			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = today;
			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = today.AddDays(13);

			voyage1.GenerateSailings();
			AssertEquals("Precondition: Expecting voyage1 to have 1 sailing.", 1, voyage1.Sailings.Count);

			var sailing1 = voyage1.Sailings[0];

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage2.JV_VoyageFlight = "1323";
			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = today.AddDays(16);
			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "JPOSA";
			destination2.JB_E_ARV = today.AddDays(30);

			voyage2.GenerateSailings();
			AssertEquals("Precondition: Expecting voyage2 to have 1 sailing.", 1, voyage2.Sailings.Count);

			var sailing2 = voyage2.Sailings[0];

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var transport1 = consol1.Transports[0];
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;

			var transport2 = consol1.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.Consols.Add(consol1);
			shipment1.JS_RL_NKOrigin = "GBLON";
			shipment1.JS_RL_NKDestination = "JPTYO";
			shipment1.JS_E_DEP = today.AddDays(-4);
			shipment1.JS_E_ARV = today.AddDays(35);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_UniqueConsignRef = "Consol2";
			consol2.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var transport3 = consol2.Transports[0];
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_IsLinked = true;
			transport3.JW_JX = sailing2.PK;

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_UniqueConsignRef = "Shipment2";
			shipment2.Consols.Add(consol2);
			shipment2.JS_RL_NKOrigin = "SGSIN";
			shipment2.JS_RL_NKDestination = "JPTYO";
			shipment2.JS_E_DEP = today.AddDays(-4);
			shipment2.JS_E_ARV = today.AddDays(35);

			Factory.Save();

			transport2.JW_ETD = today.AddDays(18);
			AssertEquals("Shipment2's ETD should change", today.AddDays(-2), shipment2.JS_E_DEP);
			AssertEquals("Shipment1's ETD should not change", today.AddDays(-4), shipment1.JS_E_DEP);
		}

		#region Implementation

		BusinessObject NewDeclarationWithRelatedVoyage(string jE_TransportMode, string jE_VesselName, string jE_VoyageFlightNo, string jE_RL_NKPortOfLoading, string jE_RL_NKOrigin, string jE_RL_NKPortOfArrival, string jE_RL_NKFinalDestination, ZDateTime jE_DateAtOrigin, ZDateTime jE_DateAtFinalDestination, string jE_DeclarationReference)
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_TransportMode] = jE_TransportMode;
			declaration[JobDeclarationSchema.JE_VesselName] = jE_VesselName;
			declaration[JobDeclarationSchema.JE_VoyageFlightNo] = jE_VoyageFlightNo;

			declaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = jE_RL_NKPortOfLoading;
			declaration[JobDeclarationSchema.JE_RL_NKOrigin] = jE_RL_NKOrigin;
			declaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = jE_RL_NKPortOfArrival;
			declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = jE_RL_NKFinalDestination;

			declaration[JobDeclarationSchema.JE_DateAtOrigin] = jE_DateAtOrigin;
			declaration[JobDeclarationSchema.JE_DateAtFinalDestination] = jE_DateAtFinalDestination;
			declaration[JobDeclarationSchema.JE_DeclarationReference] = jE_DeclarationReference;

			return declaration;
		}

		BusinessObject DeclarationWithRelatedVoyage
		{
			get
			{
				if (fDeclarationWithRelatedVoyage == null)
				{
					fDeclarationWithRelatedVoyage = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_TransportMode] = "SEA";
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_VesselName] = "Vessel";
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_VoyageFlightNo] = "Voyage";
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "NZAKL";
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_RL_NKOrigin] = "NZWEL";
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_RL_NKPortOfArrival] = "AUMEL";
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_RL_NKFinalDestination] = "AUADL";
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_VoyageFlightNo] = "Voyage";
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_DateAtOrigin] = new ZDateTime(2001, 1, 1);
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_DateAtFinalDestination] = new ZDateTime(2002, 1, 1);
					fDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_DeclarationReference] = "DEC1";
				}
				return fDeclarationWithRelatedVoyage;
			}
		}
		BusinessObject fDeclarationWithRelatedVoyage;

		BusinessObject MessagingDeclarationWithRelatedVoyage
		{
			get
			{
				if (fMessagingDeclarationWithRelatedVoyage == null)
				{
					fMessagingDeclarationWithRelatedVoyage = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_TransportMode] = "SEA";
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_VesselName] = "Vessel";
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_VoyageFlightNo] = "Voyage";
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "NZAKL";
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_RL_NKOrigin] = "NZWEL";
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_RL_NKPortOfArrival] = "AUMEL";
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_RL_NKFinalDestination] = "AUADL";
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_VoyageFlightNo] = "Voyage";
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_DateAtOrigin] = new ZDateTime(2001, 1, 1);
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_DateAtFinalDestination] = new ZDateTime(2002, 1, 1);
					fMessagingDeclarationWithRelatedVoyage[JobDeclarationSchema.JE_DeclarationReference] = "DEC2";

					EDIMessage message = EDIMessageTestFactory.New(Factory);
					message.EM_LinkedObject = fMessagingDeclarationWithRelatedVoyage;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message.EM_ApplicationCode = "CMR";
				}
				return fMessagingDeclarationWithRelatedVoyage;
			}
		}
		BusinessObject fMessagingDeclarationWithRelatedVoyage;

		JobVoyage VoyageWithRelatedDeclaration
		{
			get
			{
				if (fVoyageWithRelatedDeclaration == null)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "Vessel";

					fVoyageWithRelatedDeclaration = Factory.New<JobVoyage>();
					fVoyageWithRelatedDeclaration.JV_RV_NKVessel = vessel.RV_FK;
					fVoyageWithRelatedDeclaration.JV_VoyageFlight = "Voyage";
					fVoyageWithRelatedDeclaration.JV_AirSeaRoad = "SEA";

					VoyageOrigin origin = fVoyageWithRelatedDeclaration.Origins.AddNew();
					VoyageDestination destination = fVoyageWithRelatedDeclaration.Destinations.AddNew();
					JobSailing sailing = fVoyageWithRelatedDeclaration.Sailings.AddNew();
					sailing.JX_JA = origin.PK;
					sailing.JX_JB = destination.PK;

					origin.JA_RL_NKPortOfLoading = "NZAKL";
					destination.JB_RL_NKPortOfDischarge = "AUMEL";
					origin.JA_E_DEP = new ZDateTime(2001, 1, 1);
					destination.JB_E_ARV = new ZDateTime(2002, 1, 1);
				}
				return fVoyageWithRelatedDeclaration;
			}
		}
		JobVoyage fVoyageWithRelatedDeclaration;

		#endregion
	}
}
