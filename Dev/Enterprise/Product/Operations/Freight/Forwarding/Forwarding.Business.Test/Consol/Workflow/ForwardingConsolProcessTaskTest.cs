using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolProcessTask))]
	sealed class ForwardingConsolProcessTaskTest : RoutingSupportProcessTaskTest<ForwardingConsol>
	{
		public void TestProxiedFieldsWithoutParent()
		{
			DepartureMilestone.P9_ParentID = ZGuid.Empty;
			AssertNotNull("should not throw exception", DepartureMilestone.ETA);
			AssertNotNull("should not throw exception", DepartureMilestone.ETD);
			AssertNotNull("should not throw exception", DepartureMilestone.LoadOrOriginPort);
			AssertNotNull("should not thorw exception", DepartureMilestone.DischargeOrDestinationPort);
		}

		public void TestMatchesTriggerAction_ForXMFTrigger()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			JobShipmentPreplanning preplanning = Factory.New<JobShipmentPreplanning>();
			preplanning.EF_JS = shipment.PK;

			ProcessTask consolTrigger = ((IWorkflowProvider)consol).WorkflowItems.Triggers.AddNew();
			ProcessTask shipmentTrigger = shipment.WorkflowItems.Triggers.AddNew();
			ProcessTask preplanningTrigger = preplanning.WorkflowItems.Triggers.AddNew();

			AssertEquals(false, consolTrigger.MatchesTriggerAction(preplanningTrigger));
			AssertEquals(false, preplanningTrigger.MatchesTriggerAction(consolTrigger));
			AssertEquals(false, consolTrigger.MatchesTriggerAction(shipmentTrigger));
			AssertEquals(false, shipmentTrigger.MatchesTriggerAction(consolTrigger));

			ProcessTaskNotification consolNotification = consolTrigger.ProcessTaskNotifications.AddNew();
			consolNotification.PQ_TriggerType = "XMF";
			ProcessTaskNotification shipmentNotification = shipmentTrigger.ProcessTaskNotifications.AddNew();
			shipmentNotification.PQ_TriggerType = "XMF";
			ProcessTaskNotification preplanningNotification = preplanningTrigger.ProcessTaskNotifications.AddNew();
			preplanningNotification.PQ_TriggerType = "XMF";

			AssertEquals("Consol matches consol", true, consolTrigger.MatchesTriggerAction(consolTrigger));
			AssertEquals("Consol matches attached preadvice", true, consolTrigger.MatchesTriggerAction(preplanningTrigger));
			AssertEquals("Preadvice matches attached consol", true, preplanningTrigger.MatchesTriggerAction(consolTrigger));
			AssertEquals("Consol matches attached shipment", true, consolTrigger.MatchesTriggerAction(shipmentTrigger));
			AssertEquals("Shipment matches attached consol", true, shipmentTrigger.MatchesTriggerAction(consolTrigger));
		}

		public void TestIsMatchForItemTemplateMerge_ArrivalEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dummy = Factory.New<DummyWithWorkflow>();

			var milestone1 = consol.WorkflowItems.AddNew();
			milestone1.IsMilestone = true;
			milestone1.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;

			var milestone2 = dummy.WorkflowItems.AddNew();
			milestone2.IsMilestone = true;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FirstLeg.Destination>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<SecondLeg.Destination>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<ThirdLeg.Destination>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<LastLeg.Destination>,FAC=CTO";
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));
		}

		public void TestIsMatchForItemTemplateMerge_DepartureEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dummy = Factory.New<DummyWithWorkflow>();

			var milestone1 = consol.WorkflowItems.AddNew();
			milestone1.IsMilestone = true;
			milestone1.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;

			var milestone2 = dummy.WorkflowItems.AddNew();
			milestone2.IsMilestone = true;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			milestone2.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<SecondLeg.Origin>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<ThirdLeg.Origin>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FourthLeg.Origin>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FirstLeg.Origin>,FAC=CTO";
			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));
		}

		public void TestIsMatchForItemTemplateMerge_CutOffDateEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dummy = Factory.New<DummyWithWorkflow>();

			var milestone1 = consol.WorkflowItems.AddNew();
			milestone1.IsMilestone = true;
			milestone1.TriggerConditions.TriggerEventCode = Events.CutOffDate.Code;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;

			var milestone2 = dummy.WorkflowItems.AddNew();
			milestone2.IsMilestone = true;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			milestone2.TriggerConditions.TriggerEventCode = Events.CutOffDate.Code;

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<SecondLeg.Origin>";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<ThirdLeg.Origin>";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FourthLeg.Origin>";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FirstLeg.Origin>";
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));
		}

		public void TestIsMatchForItemTemplateMerge_StorageCommencedEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dummy = Factory.New<DummyWithWorkflow>();

			var milestone1 = consol.WorkflowItems.AddNew();
			milestone1.IsMilestone = true;
			milestone1.TriggerConditions.TriggerEventCode = Events.StorageCommenced.Code;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;

			var milestone2 = dummy.WorkflowItems.AddNew();
			milestone2.IsMilestone = true;
			milestone2.TriggerConditions.TriggerEventCode = Events.StorageCommenced.Code;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FirstLeg.Destination>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<SecondLeg.Destination>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<ThirdLeg.Destination>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<LastLeg.Destination>,FAC=CTO";
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));
		}

		public void TestisMatchForItemTemplateMerge_ReceiptCommencedEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var dummy = Factory.New<DummyWithWorkflow>();

			var milestone1 = consol.WorkflowItems.AddNew();
			milestone1.IsMilestone = true;
			milestone1.TriggerConditions.TriggerEventCode = Events.ReceiptCommenced.Code;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;

			var milestone2 = dummy.WorkflowItems.AddNew();
			milestone2.IsMilestone = true;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			milestone2.TriggerConditions.TriggerEventCode = Events.ReceiptCommenced.Code;

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<SecondLeg.Origin>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<ThirdLeg.Origin>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FourthLeg.Origin>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FirstLeg.Origin>,FAC=CTO";
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));
		}

		#region Transport Legs

		public void TestP9_ReferencedID_PopulatedAutomatically()
		{
			AssertEquals("Departure", OriginTransport.PK, DepartureMilestone.P9_ReferencedID);
			AssertEquals("Arrival", DestinationTransport.PK, ArrivalMilestone.P9_ReferencedID);
			AssertEquals("Departure for Leg 2", IntermediateTransport.PK, DepartureMilestoneForIntermediateTransport.P9_ReferencedID);
			AssertEquals("Arrival for Leg 2", OriginTransport.PK, ArrivalMilestoneForIntermediateTransport.P9_ReferencedID);
			AssertEquals("Departure for Leg 3", DestinationTransport.PK, DepartureMilestoneForDestinationTransport.P9_ReferencedID);
			AssertEquals("Arrival for Leg 3", IntermediateTransport.PK, ArrivalMilestoneForDestinationTransport.P9_ReferencedID);
			AssertEquals("Departure for Export", OriginTransport.PK, DepartureExportMilestoneForOriginTransport.P9_ReferencedID);
			AssertEquals("Departure for Import", OriginTransport.PK, DepartureImportMilestoneForOriginTransport.P9_ReferencedID);
			AssertEquals("Arrival for Export", DestinationTransport.PK, ArrivalExportMilestoneForDestinationTransport.P9_ReferencedID);
			AssertEquals("Arrival for Import", DestinationTransport.PK, ArrivalImportMilestoneForDestinationTransport.P9_ReferencedID);

			AssertNotEquals("prerequisite (main transport not set)", OriginTransport.JW_TransportType, Core.Constants.TransportPlanningType.MainVessel);
			AssertNotEquals("prerequisite (main transport not set)", IntermediateTransport.JW_TransportType, Core.Constants.TransportPlanningType.MainVessel);
			AssertNotEquals("prerequisite (main transport not set)", DestinationTransport.JW_TransportType, Core.Constants.TransportPlanningType.MainVessel);

			AssertEquals("Departure for main", ZGuid.Empty, DepartureMilestoneForMainTransport.P9_ReferencedID);
			AssertEquals("Arrival for main", ZGuid.Empty, ArrivalMilestoneForMainTransport.P9_ReferencedID);

			OriginTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			departureMilestoneForMainTransport = null;
			arrivalMilestoneForMainTransport = null;

			AssertEquals("Departure for main", OriginTransport.PK, DepartureMilestoneForMainTransport.P9_ReferencedID);
			AssertEquals("Arrival for main", OriginTransport.PK, ArrivalMilestoneForMainTransport.P9_ReferencedID);
		}

		public void TestP9_ReferencedID_PopulatedAutomatically_ForTriggers()
		{
			AssertEquals("Departure", OriginTransport.PK, DepartureTrigger.P9_ReferencedID);
			AssertEquals("Arrival", DestinationTransport.PK, ArrivalTrigger.P9_ReferencedID);
			AssertEquals("Departure for Leg 2", IntermediateTransport.PK, DepartureTriggerForIntermediateTransport.P9_ReferencedID);
			AssertEquals("Arrival for Leg 2", OriginTransport.PK, ArrivalTriggerForIntermediateTransport.P9_ReferencedID);
			AssertEquals("Departure for Leg 3", DestinationTransport.PK, DepartureTriggerForDestinationTransport.P9_ReferencedID);
			AssertEquals("Arrival for Leg 3", IntermediateTransport.PK, ArrivalTriggerForDestinationTransport.P9_ReferencedID);
			AssertEquals("Departure for Export", OriginTransport.PK, DepartureExportTriggerForOriginTransport.P9_ReferencedID);
			AssertEquals("Departure for Import", OriginTransport.PK, DepartureImportTriggerForOriginTransport.P9_ReferencedID);
			AssertEquals("Arrival for Export", DestinationTransport.PK, ArrivalExportTriggerForDestinationTransport.P9_ReferencedID);
			AssertEquals("Arrival for Import", DestinationTransport.PK, ArrivalImportTriggerForDestinationTransport.P9_ReferencedID);

			AssertEquals("Departure", OriginTransport.PK, EstimatedDepartureTrigger.P9_ReferencedID);
			AssertEquals("Arrival", DestinationTransport.PK, EstimatedArrivalTrigger.P9_ReferencedID);
			AssertEquals("Departure for Leg 2", IntermediateTransport.PK, EstimatedDepartureTriggerForIntermediateTransport.P9_ReferencedID);
			AssertEquals("Arrival for Leg 2", OriginTransport.PK, EstimatedArrivalTriggerForIntermediateTransport.P9_ReferencedID);
			AssertEquals("Departure for Leg 3", DestinationTransport.PK, EstimatedDepartureTriggerForDestinationTransport.P9_ReferencedID);
			AssertEquals("Arrival for Leg 3", IntermediateTransport.PK, EstimatedArrivalTriggerForDestinationTransport.P9_ReferencedID);
			AssertEquals("Departure for Export", OriginTransport.PK, EstimatedDepartureExportTriggerForOriginTransport.P9_ReferencedID);
			AssertEquals("Departure for Import", OriginTransport.PK, EstimatedDepartureImportTriggerForOriginTransport.P9_ReferencedID);
			AssertEquals("Arrival for Export", DestinationTransport.PK, EstimatedArrivalExportTriggerForDestinationTransport.P9_ReferencedID);
			AssertEquals("Arrival for Import", DestinationTransport.PK, EstimatedArrivalImportTriggerForDestinationTransport.P9_ReferencedID);
		}

		public void TestDepartureArrivalTransports()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USSFO";
			consol.JK_RL_NKDischargePort = "HKHKG";

			Transport transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "USSFO";
			transport1.JW_RL_NKDiscPort = "SGSIN";

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "AUSYD";

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "AUSYD";
			transport3.JW_RL_NKDiscPort = "NZAKL";

			Transport transport4 = consol.Transports.AddNew();
			transport4.JW_RL_NKLoadPort = "NZAKL";
			transport4.JW_RL_NKDiscPort = "HKHKG";

			ForwardingConsolProcessTask departureMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			departureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			ForwardingConsolProcessTask departureTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			departureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
			ForwardingConsolProcessTask estimatedDepartureTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			estimatedDepartureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETD;

			ForwardingConsolProcessTask secondLegArrivalMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			secondLegArrivalMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			secondLegArrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			ForwardingConsolProcessTask secondLegArrivalTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			secondLegArrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
			secondLegArrivalTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			ForwardingConsolProcessTask secondLegEstimatedArrivalTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			secondLegEstimatedArrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETA;
			secondLegEstimatedArrivalTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;

			ForwardingConsolProcessTask secondLegDepartureMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			secondLegDepartureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			secondLegDepartureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			ForwardingConsolProcessTask secondLegDepartureTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			secondLegDepartureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
			secondLegDepartureTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			ForwardingConsolProcessTask secondLegEstimatedDepartureTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			secondLegEstimatedDepartureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETD;
			secondLegEstimatedDepartureTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;

			ForwardingConsolProcessTask thirdLegArrivalMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			thirdLegArrivalMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			thirdLegArrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			ForwardingConsolProcessTask thirdLegArrivalTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			thirdLegArrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
			thirdLegArrivalTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			ForwardingConsolProcessTask thirdLegEstimatedArrivalTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			thirdLegEstimatedArrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETA;
			thirdLegEstimatedArrivalTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;

			ForwardingConsolProcessTask thirdLegDepartureMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			thirdLegDepartureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			thirdLegDepartureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			ForwardingConsolProcessTask thirdLegDepartureTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			thirdLegDepartureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
			thirdLegDepartureTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			ForwardingConsolProcessTask thirdLegEstimatedDepartureTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			thirdLegEstimatedDepartureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETD;
			thirdLegEstimatedDepartureTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;

			ForwardingConsolProcessTask fourthLegArrivalMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			fourthLegArrivalMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			fourthLegArrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			ForwardingConsolProcessTask fourthLegArrivalTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			fourthLegArrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
			fourthLegArrivalTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			ForwardingConsolProcessTask fourthLegEstimatedArrivalTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			fourthLegEstimatedArrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETA;
			fourthLegEstimatedArrivalTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;

			ForwardingConsolProcessTask fourthLegDepartureMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			fourthLegDepartureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			fourthLegDepartureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			ForwardingConsolProcessTask fourthLegDepartureTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			fourthLegDepartureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
			fourthLegDepartureTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			ForwardingConsolProcessTask fourthLegEstimatedDepartureTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			fourthLegEstimatedDepartureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETD;
			fourthLegEstimatedDepartureTrigger.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;

			ForwardingConsolProcessTask arrivalMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			arrivalMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			ForwardingConsolProcessTask arrivalTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			arrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
			ForwardingConsolProcessTask estimatedArrivalTrigger = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			estimatedArrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETA;

			AssertEquals(transport1.PK, departureMilestone.P9_ReferencedID);
			AssertEquals(transport1.PK, departureTrigger.P9_ReferencedID);
			AssertEquals(transport1.PK, estimatedDepartureTrigger.P9_ReferencedID);

			AssertEquals(transport1.PK, secondLegArrivalMilestone.P9_ReferencedID);
			AssertEquals(transport1.PK, secondLegArrivalTrigger.P9_ReferencedID);
			AssertEquals(transport1.PK, secondLegEstimatedArrivalTrigger.P9_ReferencedID);

			AssertEquals(transport2.PK, secondLegDepartureMilestone.P9_ReferencedID);
			AssertEquals(transport2.PK, secondLegDepartureTrigger.P9_ReferencedID);
			AssertEquals(transport2.PK, secondLegEstimatedDepartureTrigger.P9_ReferencedID);

			AssertEquals(transport2.PK, thirdLegArrivalMilestone.P9_ReferencedID);
			AssertEquals(transport2.PK, thirdLegArrivalTrigger.P9_ReferencedID);
			AssertEquals(transport2.PK, thirdLegEstimatedArrivalTrigger.P9_ReferencedID);

			AssertEquals(transport3.PK, thirdLegDepartureMilestone.P9_ReferencedID);
			AssertEquals(transport3.PK, thirdLegDepartureTrigger.P9_ReferencedID);
			AssertEquals(transport3.PK, thirdLegEstimatedDepartureTrigger.P9_ReferencedID);

			AssertEquals(transport3.PK, fourthLegArrivalMilestone.P9_ReferencedID);
			AssertEquals(transport3.PK, fourthLegArrivalTrigger.P9_ReferencedID);
			AssertEquals(transport3.PK, fourthLegEstimatedArrivalTrigger.P9_ReferencedID);

			AssertEquals(transport4.PK, fourthLegDepartureMilestone.P9_ReferencedID);
			AssertEquals(transport4.PK, fourthLegDepartureTrigger.P9_ReferencedID);
			AssertEquals(transport4.PK, fourthLegEstimatedDepartureTrigger.P9_ReferencedID);

			AssertEquals(transport4.PK, arrivalMilestone.P9_ReferencedID);
			AssertEquals(transport4.PK, arrivalTrigger.P9_ReferencedID);
			AssertEquals(transport4.PK, estimatedArrivalTrigger.P9_ReferencedID);
		}

		public void TestMainVessel()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USSFO";

			Transport transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "USSFO";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;

			ForwardingConsolProcessTask mainTransportDepartureMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			mainTransportDepartureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			mainTransportDepartureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;

			AssertEquals(ZGuid.Empty, mainTransportDepartureMilestone.P9_ReferencedID);

			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;

			mainTransportDepartureMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			mainTransportDepartureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			mainTransportDepartureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;

			AssertEquals(transport1.PK, mainTransportDepartureMilestone.P9_ReferencedID);

			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;

			mainTransportDepartureMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			mainTransportDepartureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			mainTransportDepartureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;

			AssertEquals(transport2.PK, mainTransportDepartureMilestone.P9_ReferencedID);
		}

		[ExpectNoExceptions]
		public void TestDepartureArrivalTransportsDoesNotThrowException()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USSFO";

			Transport transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "USSFO";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;

			ForwardingConsolProcessTask arrivalMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			arrivalMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			arrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			arrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			arrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			arrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;

			ForwardingConsolProcessTask departureMilestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
			departureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			departureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			departureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			departureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			departureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "AUSYD";

			arrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			arrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			arrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			arrivalMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;

			departureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			departureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			departureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			departureMilestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;
		}

		public void TestP9_ReferencedID_PopulatedAutomaticallyForMainTransportCondition()
		{
			Transport decoyTransport = Consol.Transports[0];
			decoyTransport.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			Transport transport = Consol.Transports.AddNew();
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			Transport decoyTransport2 = Consol.Transports.AddNew();
			decoyTransport2.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;

			RoutingSupportProcessTask milestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;
			AssertEquals("Milestone linked to main transport", Core.Constants.TransportPlanningType.MainVessel, milestone.Transport.JW_TransportType);
		}

		public void TestActualDateIsUpdatedInDb()
		{
			ForwardingConsol job = this.Job;
			AssertEquals("Departure for Leg 1", OriginTransport.PK, DepartureMilestone.P9_ReferencedID);
			DepartureMilestone.P9_Type = Core.Constants.Workflow.MilestoneType;
			DepartureMilestone.P9_RespondToCascadedEvents = true;
			AssertEquals(true, OriginTransport.JW_IsLinked);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ForwardingConsol loadedConsol = newFactory.Load<ForwardingConsol>(job.PK);
			Transport originTransport = loadedConsol.Transports.DepartureTransport;
			originTransport.JW_ATD = ZDateTime.Now;
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			departureMilestone = newFactory.Load<ForwardingConsolProcessTask>(departureMilestone.PK);
			AssertEquals(false, departureMilestone.P9_ActualDateInfo.OriginalValue.IsEmpty);
		}

		public void TestGetTransportLegToAttach_VesselVoyageFlightProcessTask()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USSFO";
			consol.JK_RL_NKLoadPort = "SGSIN";

			consol.JK_RL_NKLoadPort = "USSFO";
			consol.JK_RL_NKDischargePort = "HKHKG";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "USSFO";
			transport1.JW_RL_NKDiscPort = "SGSIN";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "AUSYD";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "AUSYD";
			transport3.JW_RL_NKDiscPort = "NZAKL";

			var transport4 = consol.Transports.AddNew();
			transport4.JW_RL_NKLoadPort = "NZAKL";
			transport4.JW_RL_NKDiscPort = "HKHKG";

			void AssertGetTransportLegToAttach(string triggerFeildName, string templateCondition1, ZGuid transportPK)
			{
				var milestone = (ForwardingConsolProcessTask)consol.WorkflowItems.Milestones.AddNew();
				milestone.TriggerConditions.TriggerFieldName = triggerFeildName;
				milestone.TemplateConditions.TemplateCondition1 = templateCondition1;

				AssertEquals(transportPK, milestone.P9_ReferencedID);
				AssertEquals(ZArchitecture.Schema.JobConsolTransportSchema.Constants.Prefix, milestone.P9_ReferencedTableCode);
			}

			AssertGetTransportLegToAttach(Transport.Schema.JW_Vessel, string.Empty, transport1.PK);
			AssertGetTransportLegToAttach(Transport.Schema.JW_VoyageFlight, string.Empty, transport1.PK);

			AssertGetTransportLegToAttach(Transport.Schema.JW_Vessel, JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg, transport2.PK);
			AssertGetTransportLegToAttach(Transport.Schema.JW_VoyageFlight, JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg, transport2.PK);

			AssertGetTransportLegToAttach(Transport.Schema.JW_Vessel, JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg, transport3.PK);
			AssertGetTransportLegToAttach(Transport.Schema.JW_VoyageFlight, JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg, transport3.PK);

			AssertGetTransportLegToAttach(Transport.Schema.JW_Vessel, JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg, transport4.PK);
			AssertGetTransportLegToAttach(Transport.Schema.JW_VoyageFlight, JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg, transport4.PK);
		}

		#endregion

		#region Defaulting Estimate from Available / Storage

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromLoadingETD()
		{
			TestEstimateDefaultedFrom(ForwardingConsolEstimateDefaultedFromList.Codes.LoadingETD, Job.Transports.DepartureTransport, "JW_ETD");
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromDischargeETA()
		{
			TestEstimateDefaultedFrom(ForwardingConsolEstimateDefaultedFromList.Codes.DischargeETA, Job.Transports.ArrivalTransport, "JW_ETA");
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromFCLAvailable()
		{
			TestEstimateDefaultedFrom(ForwardingConsolEstimateDefaultedFromList.Codes.FCLAvailable, Job.Transports.ArrivalTransport, "JW_TerminalAvailabilityDate");
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromFCLStorage()
		{
			TestEstimateDefaultedFrom(ForwardingConsolEstimateDefaultedFromList.Codes.FCLStorage, Job.Transports.ArrivalTransport, "JW_TerminalStorageDate");
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromLCLAvailable()
		{
			TestEstimateDefaultedFrom(ForwardingConsolEstimateDefaultedFromList.Codes.LCLAvailable, Job.Transports.ArrivalTransport, "JW_DepotAvailabilityDate");
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromLCLStorage()
		{
			TestEstimateDefaultedFrom(ForwardingConsolEstimateDefaultedFromList.Codes.LCLStorage, Job.Transports.ArrivalTransport, "JW_DepotStorageDate");
		}

		void TestEstimateDefaultedFrom(ZString estimateDefaultedFrom, Transport transport, ZString transportProperty)
		{
			var milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = estimateDefaultedFrom;
			milestone.P9_EstimatedDefaultTimeDelta = new ZDateTime(2000, 1, 1).AddDays(1);

			transport[transportProperty] = new ZDateTime(2005, 10, 1);
			Factory.Save();
			AssertEquals("Estimated date populated", new ZDateTime(2005, 10, 2), milestone.P9_ScheduledDate.ToZDateTime());
		}

		#endregion

		#region Transport Objects

		#region Arrival/Departure Milestones

		new ForwardingConsolProcessTask DepartureMilestone
		{
			get
			{
				if (departureMilestone == null)
				{
					departureMilestone = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
				}
				return departureMilestone;
			}
		}
		ForwardingConsolProcessTask departureMilestone;

		new ForwardingConsolProcessTask ArrivalMilestone
		{
			get
			{
				if (arrivalMilestone == null)
				{
					arrivalMilestone = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
				}
				return arrivalMilestone;
			}
		}
		ForwardingConsolProcessTask arrivalMilestone;

		ForwardingConsolProcessTask DepartureMilestoneForMainTransport
		{
			get
			{
				if (departureMilestoneForMainTransport == null)
				{
					departureMilestoneForMainTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureMilestoneForMainTransport.TriggerConditions.TriggerEventCode = Events.Departure.Code;
					departureMilestoneForMainTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;
				}
				return departureMilestoneForMainTransport;
			}
		}
		ForwardingConsolProcessTask departureMilestoneForMainTransport;

		ForwardingConsolProcessTask ArrivalMilestoneForMainTransport
		{
			get
			{
				if (arrivalMilestoneForMainTransport == null)
				{
					arrivalMilestoneForMainTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalMilestoneForMainTransport.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
					arrivalMilestoneForMainTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.MainTransport;
				}
				return arrivalMilestoneForMainTransport;
			}
		}
		ForwardingConsolProcessTask arrivalMilestoneForMainTransport;

		#endregion

		#region IntermediateTransport Milestones

		ForwardingConsolProcessTask DepartureMilestoneForIntermediateTransport
		{
			get
			{
				if (departureMilestoneForIntermediateTransport == null)
				{
					departureMilestoneForIntermediateTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureMilestoneForIntermediateTransport.TriggerConditions.TriggerEventCode = Events.Departure.Code;
					departureMilestoneForIntermediateTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
				}
				return departureMilestoneForIntermediateTransport;
			}
		}
		ForwardingConsolProcessTask departureMilestoneForIntermediateTransport;

		ForwardingConsolProcessTask ArrivalMilestoneForIntermediateTransport
		{
			get
			{
				if (arrivalMilestoneForIntermediateTransport == null)
				{
					arrivalMilestoneForIntermediateTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalMilestoneForIntermediateTransport.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
					arrivalMilestoneForIntermediateTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
				}
				return arrivalMilestoneForIntermediateTransport;
			}
		}
		ForwardingConsolProcessTask arrivalMilestoneForIntermediateTransport;

		#endregion

		#region DestinationTransport Milestones

		ForwardingConsolProcessTask DepartureMilestoneForDestinationTransport
		{
			get
			{
				if (departureMilestoneForDestinationTransport == null)
				{
					departureMilestoneForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureMilestoneForDestinationTransport.TriggerConditions.TriggerEventCode = Events.Departure.Code;
					departureMilestoneForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
				}
				return departureMilestoneForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask departureMilestoneForDestinationTransport;

		ForwardingConsolProcessTask ArrivalMilestoneForDestinationTransport
		{
			get
			{
				if (arrivalMilestoneForDestinationTransport == null)
				{
					arrivalMilestoneForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalMilestoneForDestinationTransport.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
					arrivalMilestoneForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
				}
				return arrivalMilestoneForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask arrivalMilestoneForDestinationTransport;

		ForwardingConsolProcessTask DepartureExportMilestoneForOriginTransport
		{
			get
			{
				if (departureExportMilestoneForOriginTransport == null)
				{
					departureExportMilestoneForOriginTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureExportMilestoneForOriginTransport.TriggerConditions.TriggerEventCode = Events.Departure.Code;
					departureExportMilestoneForOriginTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Export;
				}
				return departureExportMilestoneForOriginTransport;
			}
		}
		ForwardingConsolProcessTask departureExportMilestoneForOriginTransport;

		ForwardingConsolProcessTask DepartureImportMilestoneForOriginTransport
		{
			get
			{
				if (departureImportMilestoneForOriginTransport == null)
				{
					departureImportMilestoneForOriginTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureImportMilestoneForOriginTransport.TriggerConditions.TriggerEventCode = Events.Departure.Code;
					departureImportMilestoneForOriginTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Import;
				}
				return departureImportMilestoneForOriginTransport;
			}
		}
		ForwardingConsolProcessTask departureImportMilestoneForOriginTransport;

		ForwardingConsolProcessTask ArrivalImportMilestoneForDestinationTransport
		{
			get
			{
				if (arrivalImportMilestoneForDestinationTransport == null)
				{
					arrivalImportMilestoneForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalImportMilestoneForDestinationTransport.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
					arrivalImportMilestoneForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Import;
				}
				return arrivalImportMilestoneForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask arrivalImportMilestoneForDestinationTransport;

		ForwardingConsolProcessTask ArrivalExportMilestoneForDestinationTransport
		{
			get
			{
				if (arrivalExportMilestoneForDestinationTransport == null)
				{
					arrivalExportMilestoneForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalExportMilestoneForDestinationTransport.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
					arrivalExportMilestoneForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Export;
				}
				return arrivalExportMilestoneForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask arrivalExportMilestoneForDestinationTransport;

		#endregion

		#region Arrival/Departure Triggers

		ForwardingConsolProcessTask EstimatedDepartureTrigger
		{
			get
			{
				if (estimatedDepartureTrigger == null)
				{
					estimatedDepartureTrigger = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					estimatedDepartureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETD;
				}
				return estimatedDepartureTrigger;
			}
		}
		ForwardingConsolProcessTask estimatedDepartureTrigger;

		ForwardingConsolProcessTask DepartureTrigger
		{
			get
			{
				if (departureTrigger == null)
				{
					departureTrigger = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
				}
				return departureTrigger;
			}
		}
		ForwardingConsolProcessTask departureTrigger;

		ForwardingConsolProcessTask EstimatedArrivalTrigger
		{
			get
			{
				if (estimatedArrivalTrigger == null)
				{
					estimatedArrivalTrigger = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					estimatedArrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETA;
				}
				return estimatedArrivalTrigger;
			}
		}
		ForwardingConsolProcessTask estimatedArrivalTrigger;

		ForwardingConsolProcessTask ArrivalTrigger
		{
			get
			{
				if (arrivalTrigger == null)
				{
					arrivalTrigger = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalTrigger.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
				}
				return arrivalTrigger;
			}
		}
		ForwardingConsolProcessTask arrivalTrigger;

		#endregion

		#region IntermediateTransport Triggers

		ForwardingConsolProcessTask EstimatedDepartureTriggerForIntermediateTransport
		{
			get
			{
				if (estimatedDepartureTriggerForIntermediateTransport == null)
				{
					estimatedDepartureTriggerForIntermediateTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					estimatedDepartureTriggerForIntermediateTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETD;
					estimatedDepartureTriggerForIntermediateTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
				}
				return estimatedDepartureTriggerForIntermediateTransport;
			}
		}
		ForwardingConsolProcessTask estimatedDepartureTriggerForIntermediateTransport;

		ForwardingConsolProcessTask DepartureTriggerForIntermediateTransport
		{
			get
			{
				if (departureTriggerForIntermediateTransport == null)
				{
					departureTriggerForIntermediateTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureTriggerForIntermediateTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
					departureTriggerForIntermediateTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
				}
				return departureTriggerForIntermediateTransport;
			}
		}
		ForwardingConsolProcessTask departureTriggerForIntermediateTransport;

		ForwardingConsolProcessTask EstimatedArrivalTriggerForIntermediateTransport
		{
			get
			{
				if (estimatedArrivalTriggerForIntermediateTransport == null)
				{
					estimatedArrivalTriggerForIntermediateTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					estimatedArrivalTriggerForIntermediateTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETA;
					estimatedArrivalTriggerForIntermediateTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
				}
				return estimatedArrivalTriggerForIntermediateTransport;
			}
		}
		ForwardingConsolProcessTask estimatedArrivalTriggerForIntermediateTransport;

		ForwardingConsolProcessTask ArrivalTriggerForIntermediateTransport
		{
			get
			{
				if (arrivalTriggerForIntermediateTransport == null)
				{
					arrivalTriggerForIntermediateTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalTriggerForIntermediateTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
					arrivalTriggerForIntermediateTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
				}
				return arrivalTriggerForIntermediateTransport;
			}
		}
		ForwardingConsolProcessTask arrivalTriggerForIntermediateTransport;

		#endregion

		#region DestinationTransport Triggers

		ForwardingConsolProcessTask EstimatedDepartureTriggerForDestinationTransport
		{
			get
			{
				if (estimatedDepartureTriggerForDestinationTransport == null)
				{
					estimatedDepartureTriggerForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					estimatedDepartureTriggerForDestinationTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETD;
					estimatedDepartureTriggerForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
				}
				return estimatedDepartureTriggerForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask estimatedDepartureTriggerForDestinationTransport;

		ForwardingConsolProcessTask DepartureTriggerForDestinationTransport
		{
			get
			{
				if (departureTriggerForDestinationTransport == null)
				{
					departureTriggerForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureTriggerForDestinationTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
					departureTriggerForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
				}
				return departureTriggerForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask departureTriggerForDestinationTransport;

		ForwardingConsolProcessTask EstimatedArrivalTriggerForDestinationTransport
		{
			get
			{
				if (estimatedArrivalTriggerForDestinationTransport == null)
				{
					estimatedArrivalTriggerForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					estimatedArrivalTriggerForDestinationTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETA;
					estimatedArrivalTriggerForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
				}
				return estimatedArrivalTriggerForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask estimatedArrivalTriggerForDestinationTransport;

		ForwardingConsolProcessTask ArrivalTriggerForDestinationTransport
		{
			get
			{
				if (arrivalTriggerForDestinationTransport == null)
				{
					arrivalTriggerForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalTriggerForDestinationTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
					arrivalTriggerForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
				}
				return arrivalTriggerForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask arrivalTriggerForDestinationTransport;

		ForwardingConsolProcessTask EstimatedDepartureExportTriggerForOriginTransport
		{
			get
			{
				if (estimatedDepartureExportTriggerForOriginTransport == null)
				{
					estimatedDepartureExportTriggerForOriginTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					estimatedDepartureExportTriggerForOriginTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETD;
					estimatedDepartureExportTriggerForOriginTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Export;
				}
				return estimatedDepartureExportTriggerForOriginTransport;
			}
		}
		ForwardingConsolProcessTask estimatedDepartureExportTriggerForOriginTransport;

		ForwardingConsolProcessTask DepartureExportTriggerForOriginTransport
		{
			get
			{
				if (departureExportTriggerForOriginTransport == null)
				{
					departureExportTriggerForOriginTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureExportTriggerForOriginTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
					departureExportTriggerForOriginTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Export;
				}
				return departureExportTriggerForOriginTransport;
			}
		}
		ForwardingConsolProcessTask departureExportTriggerForOriginTransport;

		ForwardingConsolProcessTask EstimatedDepartureImportTriggerForOriginTransport
		{
			get
			{
				if (estimatedDepartureImportTriggerForOriginTransport == null)
				{
					estimatedDepartureImportTriggerForOriginTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Triggers.AddNew();
					estimatedDepartureImportTriggerForOriginTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETD;
					estimatedDepartureImportTriggerForOriginTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Import;
				}
				return estimatedDepartureImportTriggerForOriginTransport;
			}
		}
		ForwardingConsolProcessTask estimatedDepartureImportTriggerForOriginTransport;

		ForwardingConsolProcessTask DepartureImportTriggerForOriginTransport
		{
			get
			{
				if (departureImportTriggerForOriginTransport == null)
				{
					departureImportTriggerForOriginTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Triggers.AddNew();
					departureImportTriggerForOriginTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
					departureImportTriggerForOriginTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Import;
				}
				return departureImportTriggerForOriginTransport;
			}
		}
		ForwardingConsolProcessTask departureImportTriggerForOriginTransport;

		ForwardingConsolProcessTask EstimatedArrivalImportTriggerForDestinationTransport
		{
			get
			{
				if (estimatedArrivalImportTriggerForDestinationTransport == null)
				{
					estimatedArrivalImportTriggerForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Triggers.AddNew();
					estimatedArrivalImportTriggerForDestinationTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETA;
					estimatedArrivalImportTriggerForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Import;
				}
				return estimatedArrivalImportTriggerForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask estimatedArrivalImportTriggerForDestinationTransport;

		ForwardingConsolProcessTask ArrivalImportTriggerForDestinationTransport
		{
			get
			{
				if (arrivalImportTriggerForDestinationTransport == null)
				{
					arrivalImportTriggerForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Triggers.AddNew();
					arrivalImportTriggerForDestinationTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
					arrivalImportTriggerForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Import;
				}
				return arrivalImportTriggerForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask arrivalImportTriggerForDestinationTransport;

		ForwardingConsolProcessTask EstimatedArrivalExportTriggerForDestinationTransport
		{
			get
			{
				if (estimatedArrivalExportTriggerForDestinationTransport == null)
				{
					estimatedArrivalExportTriggerForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Triggers.AddNew();
					estimatedArrivalExportTriggerForDestinationTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ETA;
					estimatedArrivalExportTriggerForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Export;
				}
				return estimatedArrivalExportTriggerForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask estimatedArrivalExportTriggerForDestinationTransport;

		ForwardingConsolProcessTask ArrivalExportTriggerForDestinationTransport
		{
			get
			{
				if (arrivalExportTriggerForDestinationTransport == null)
				{
					arrivalExportTriggerForDestinationTransport = (ForwardingConsolProcessTask)WorkflowProvider.WorkflowItems.Triggers.AddNew();
					arrivalExportTriggerForDestinationTransport.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
					arrivalExportTriggerForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Export;
				}
				return arrivalExportTriggerForDestinationTransport;
			}
		}
		ForwardingConsolProcessTask arrivalExportTriggerForDestinationTransport;

		#endregion

		#endregion

		#region ProcessTask Property Overrides

		public void TestETA_EmptyInBase()
		{
			ForwardingConsolProcessTask task = Factory.NewWithValidTestData<ForwardingConsolProcessTask>();
			task.P9_ParentID = Consol.PK;
			task.P9_ParentTableCode = "JK";

			Factory.Save();

			AssertEquals("ETA field should return ForwardingConsol.MostInterestingTransportForBinding[0].JW_ETA for current Exception if its ParentTableCode is JK", Consol.MostInterestingTransportForBinding[0].JW_ETA, task.ETA);
		}

		public void TestETD_EmptyInBase()
		{
			ForwardingConsolProcessTask task = Factory.NewWithValidTestData<ForwardingConsolProcessTask>();
			task.P9_ParentID = Consol.PK;
			task.P9_ParentTableCode = "JK";

			Factory.Save();

			AssertEquals("ETD field should return ForwardingConsol.MostInterestingTransportForBinding[0].JW_ETD for current Exception if its ParentTableCode is JK", Consol.MostInterestingTransportForBinding[0].JW_ETD, task.ETD);
		}

		[ExpectNoExceptions]
		public void TestETA_WhenMostInterestingTransportForBindingIsEmpty_NoException()
		{
			ForwardingConsolProcessTask task = Factory.NewWithValidTestData<ForwardingConsolProcessTask>();

			AssertEquals(ZDateTime.Empty, task.ETA);
		}

		[ExpectNoExceptions]
		public void TestETD_WhenMostInterestingTransportForBindingIsEmpty_NoException()
		{
			ForwardingConsolProcessTask task = Factory.NewWithValidTestData<ForwardingConsolProcessTask>();

			AssertEquals(ZDateTime.Empty, task.ETD);
		}

		public void TestLoadOrOriginPort_EmptyInBase()
		{
			ForwardingConsolProcessTask task = Factory.NewWithValidTestData<ForwardingConsolProcessTask>();
			task.P9_ParentID = Consol.PK;
			task.P9_ParentTableCode = "JK";

			Factory.Save();

			AssertEquals("LoadOrOriginPort field should return ForwardingConsol.JK_RL_NKLoadPort for current Exception if its ParentTableCode is JK", Consol.JK_RL_NKLoadPort, task.LoadOrOriginPort);
		}

		public void TestDischargeOrDestinationPort_EmptyInBase()
		{
			ForwardingConsolProcessTask task = Factory.NewWithValidTestData<ForwardingConsolProcessTask>();
			task.P9_ParentID = Consol.PK;
			task.P9_ParentTableCode = "JK";

			Factory.Save();

			AssertEquals("DischargeOrDestinationPort field should return ForwardingConsol.JK_RL_NKDischargePort for current Exception if its ParentTableCode is JK", Consol.JK_RL_NKDischargePort, task.DischargeOrDestinationPort);
		}

		#endregion

		#region Implementation

		ForwardingConsol Consol
		{
			get { return (ForwardingConsol)WorkflowProvider; }
		}

		protected override ZString ParentOrigin
		{
			get { return Job.JK_RL_NKLoadPort; }
			set { Job.JK_RL_NKLoadPort = value; }
		}

		protected override ZString ParentDestination
		{
			get { return Job.JK_RL_NKDischargePort; }
			set { Job.JK_RL_NKDischargePort = value; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return WorkflowProvider.WorkflowItems.AddNew();
		}

		#endregion
	}
}
