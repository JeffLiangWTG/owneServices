using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ConsolCargoImpPhase2MessageDeliveryTest : TestCaseWithFactory
	{
		public void TestRouteMapIsNotAllowed()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);

			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			var processTask = consol.WorkflowItems.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			action.Parent.TriggerConditions_ForBinding.TriggerEventCode = Events.Attached.Code;
			NotificationBuffer notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.Attached.Code, SJ_Reference = string.Empty };
			var processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(@"", notifications.AsString);
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment1);
			AssertEquals(@"CargoIMP Phase 2 integration is only available for Air Shipments.
", note.LoadFromNote());
			note = new CargoIMPPhase2MessageNote(shipment2);
			AssertEquals(@"CargoIMP Phase 2 integration is only available for Air Shipments.
", note.LoadFromNote());
		}

		public void TestStatusUpdateIsNotAllowed()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);

			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S1111";
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S1112";
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			var processTask = consol.WorkflowItems.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			action.Parent.TriggerConditions_ForBinding.TriggerEventCode = Events.CargoReceivedAtDepot.Code;
			NotificationBuffer notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.CargoReceivedAtDepot.Code, SJ_Reference = string.Empty };
			var processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(@"CargoIMP Phase 2 MSU Message for Shipment S1111
CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.
CargoIMP Phase 2 MSU Message for Shipment S1112
CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.
", notifications.AsString);
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment1);
			AssertEquals(@"CargoIMP Phase 2 integration is only available for Air Shipments.
CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.
", note.LoadFromNote());
			note = new CargoIMPPhase2MessageNote(shipment2);
			AssertEquals(@"CargoIMP Phase 2 integration is only available for Air Shipments.
CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.
", note.LoadFromNote());
		}

		public void TestRouteMapCreationError()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			var relatedParty = org1.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			relatedParty = org2.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			transport.JW_VoyageFlight = "QF1234";

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S0001000";
			shipment.Consols.Add(consol);
			var processTask = consol.WorkflowItems.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			action.Parent.TriggerConditions_ForBinding.TriggerEventCode = Events.Attached.Code;
			NotificationBuffer notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.Attached.Code, SJ_Reference = string.Empty };
			CargoImpPhase2MessageDelivery processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals(@"CargoIMP Phase 2 RMI Message for Shipment S0001000
Error: Required field empty (House Bill Number)
Error: Required field empty (Registered Date)
Error: Required field empty (Origin)
Error: Required field empty (Destination)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
Registry item 'Sender Identification (PIMA)' must be set
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", notifications.AsString);
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			AssertEquals(@"Error: Required field empty (House Bill Number)
Error: Required field empty (Registered Date)
Error: Required field empty (Origin)
Error: Required field empty (Destination)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
Registry item 'Sender Identification (PIMA)' must be set
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", note.LoadFromNote());
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(@"Error: Required field empty (House Bill Number)
Error: Required field empty (Registered Date)
Error: Required field empty (Origin)
Error: Required field empty (Destination)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
Registry item 'Sender Identification (PIMA)' must be set
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals(@"CargoIMP Phase 2 RMI Message for S0001000 Shipment Creation Error", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			GlbStaff.CurrentUser.GS_EmailAddress = "hello2@example.com";
			processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);

			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			AssertEquals(0, manager.Messages.Count);
		}

		public void TestRouteMapCreationErrorBeforeStatusUpdate()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			var relatedParty = org1.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			relatedParty = org2.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			transport.JW_VoyageFlight = "QF1234";

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S0001000";
			shipment.Consols.Add(consol);
			var processTask = consol.WorkflowItems.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			action.Parent.TriggerConditions_ForBinding.TriggerEventCode = AutoEvents.CargoReceivedAtDepot.Code;
			NotificationBuffer notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.CargoReceivedAtDepot.Code, SJ_Reference = string.Empty };
			var processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals(@"CargoIMP Phase 2 MSU Message for Shipment S0001000
CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.
Error: Required field empty (House Bill Number)
Error: Required field empty (Registered Date)
Error: Required field empty (Origin)
Error: Required field empty (Destination)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
Registry item 'Sender Identification (PIMA)' must be set
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", notifications.AsString);
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			AssertEquals(@"CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.
Error: Required field empty (House Bill Number)
Error: Required field empty (Registered Date)
Error: Required field empty (Origin)
Error: Required field empty (Destination)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
Registry item 'Sender Identification (PIMA)' must be set
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", note.LoadFromNote());
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(@"CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.
Error: Required field empty (House Bill Number)
Error: Required field empty (Registered Date)
Error: Required field empty (Origin)
Error: Required field empty (Destination)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
Registry item 'Sender Identification (PIMA)' must be set
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals(@"CargoIMP Phase 2 RMI Message for S0001000 Shipment Creation Error", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);

			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			AssertEquals(0, manager.Messages.Count);
		}

		public void TestStatusUpdateCreationError()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			transport.JW_VoyageFlight = "QF1234";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			var relatedParty = org1.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			relatedParty = org2.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S0001000";
			shipment.Consols.Add(consol);

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");
			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			var message = manager.Messages.AddNew();
			message.EM_MessageText = "MESSAGE1";
			message.EM_MessageType = "RMI";

			var processTask = consol.WorkflowItems.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			action.Parent.TriggerConditions_ForBinding.TriggerEventCode = Events.CargoReceivedAtDepot.Code;
			NotificationBuffer notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.CargoReceivedAtDepot.Code, SJ_Reference = string.Empty };
			var processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals(@"CargoIMP Phase 2 MSU Message for Shipment S0001000
Error: Required field empty (Registered Date)
Error: Required field empty (House Bill Number)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
Error: Required field empty (Milestone Actual Date)
Error: Required field empty (Origin)
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", notifications.AsString);
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			AssertEquals(@"Error: Required field empty (Registered Date)
Error: Required field empty (House Bill Number)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
Error: Required field empty (Milestone Actual Date)
Error: Required field empty (Origin)
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", note.LoadFromNote());
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(@"Error: Required field empty (Registered Date)
Error: Required field empty (House Bill Number)
Error: Required field empty (Packs)
Error: Required field empty (Weight)
Error: Required field empty (Milestone Actual Date)
Error: Required field empty (Origin)
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals(@"CargoIMP Phase 2 MSU Message for S0001000 Shipment Creation Error", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			GlbStaff.CurrentUser.GS_EmailAddress = "hello2@example.com";
			processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals(1, manager.Messages.Count);
		}

		public void TestRouteMapWithWarnings()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 1300.345M;
			Factory.Save();

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			transport.JW_VoyageFlight = "QF1234";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			var relatedParty = org1.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			relatedParty = org2.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "COD3;";
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org3.MainAddress.PK;

			shipment.Consols.Add(consol);
			var processTask = consol.WorkflowItems.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			action.Parent.TriggerConditions_ForBinding.TriggerEventCode = Events.Attached.Code;
			NotificationBuffer notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.Attached.Code, SJ_Reference = string.Empty };
			var processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals("", notifications.AsString);
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			AssertEquals("", note.LoadFromNote());
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			AssertEquals(1, manager.Messages.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			notifications.Clear();

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2AllowToSendMilestoneMessagesWithWarnings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals("", notifications.AsString);
			note = new CargoIMPPhase2MessageNote(shipment);
			AssertEquals("", note.LoadFromNote());
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(2, manager.Messages.Count);
		}

		public void TestStatusUpdateWithWarnings()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 1300.345M;
			Factory.Save();

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			org1.OH_RL_NKClosestPort = "AUALX";
			shipment.JS_OA_ExportReceivingDepot = org1.MainAddress.PK;

			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			var message = manager.Messages.AddNew();
			message.EM_MessageText = "MESSAGE1";
			message.EM_MessageType = "RMI";

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			transport.JW_VoyageFlight = "QF1234";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			var relatedParty = org2.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "CODE3";
			relatedParty = org3.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			consol.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org3.MainAddress.PK;

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "COD4;";
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = org4.MainAddress.PK;

			shipment.Consols.Add(consol);
			var processTask = consol.WorkflowItems.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			action.Parent.TriggerConditions_ForBinding.TriggerEventCode = Events.CargoReceivedAtDepot.Code;
			((ProcessTask)action.Parent).SetMilestoneActualDateForTest(ZDateTime.Now);
			NotificationBuffer notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.CargoReceivedAtDepot.Code, SJ_Reference = string.Empty };
			var processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals("", notifications.AsString);
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			AssertEquals("", note.LoadFromNote());
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(2, manager.Messages.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			notifications.Clear();

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2AllowToSendMilestoneMessagesWithWarnings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals("", notifications.AsString);
			note = new CargoIMPPhase2MessageNote(shipment);
			AssertEquals("", note.LoadFromNote());
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(3, manager.Messages.Count);
		}

		public void TestRouteMapCreationSuccess()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "NZAKL";
			shipment1.JS_HouseBill = "HOUSE BILL";
			shipment1.JS_OuterPacks = 10;
			shipment1.JS_ActualWeight = 1300.345M;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "NZAKL";
			shipment2.JS_HouseBill = "HOUSE BILL 2";
			shipment2.JS_OuterPacks = 20;
			shipment2.JS_ActualWeight = 2300.345M;
			Factory.Save();

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			transport.JW_VoyageFlight = "QF1234";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			var relatedParty = org1.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			relatedParty = org2.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			shipment1.Consols.Add(consol);
			shipment2.Consols.Add(consol);
			var processTask = consol.WorkflowItems.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			action.Parent.TriggerConditions_ForBinding.TriggerEventCode = Events.Attached.Code;
			NotificationBuffer notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.Attached.Code, SJ_Reference = string.Empty };
			var processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals("", notifications.AsString);
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment1);
			AssertEquals("", note.LoadFromNote());
			note = new CargoIMPPhase2MessageNote(shipment2);
			AssertEquals("", note.LoadFromNote());

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			CargoIMPPhase2MessageManager manager1 = CargoIMPPhase2MessageManager.New(shipment1);
			AssertEquals(1, manager1.Messages.Count);

			CargoIMPPhase2MessageManager manager2 = CargoIMPPhase2MessageManager.New(shipment2);
			AssertEquals(1, manager2.Messages.Count);
		}

		public void TestStatusUpdateCreationSuccess()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 1300.345M;
			Factory.Save();

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			org1.OH_RL_NKClosestPort = "AUALX";
			shipment.JS_OA_ExportReceivingDepot = org1.MainAddress.PK;

			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2GroupToSendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Groups.PostMastersGroupPK);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2SendMessageErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMemberAndNominatedGroup);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			transport.JW_VoyageFlight = "QF1234";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			var relatedParty = org2.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "CODE3";
			relatedParty = org3.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			consol.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org3.MainAddress.PK;

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			shipment.Consols.Add(consol);
			var processTask = consol.WorkflowItems.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;
			action.Parent.TriggerConditions_ForBinding.TriggerEventCode = Events.CargoReceivedAtDepot.Code;
			((ProcessTask)action.Parent).SetMilestoneActualDateForTest(ZDateTime.Now);
			NotificationBuffer notifications = new NotificationBuffer();

			var queuedLog = new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = AutoEvents.CargoReceivedAtDepot.Code, SJ_Reference = string.Empty };
			var processor = new ConsolCargoImpPhase2MessageDelivery(consol, action, queuedLog);
			processor.Process(notifications);
			Factory.Save();

			AssertEquals(@"CargoIMP Phase 2 MSU Message for Shipment S00001000
CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.
", notifications.AsString);
			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			AssertEquals(@"CargoIMP Phase 2 Route Map Information message must be sent before first Status Update message.
", note.LoadFromNote());
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(2, manager.Messages.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var postMastersGroup = new BusinessObjectFactory().Load<GlbGroup>(Constants.Groups.PostMastersGroupPK);
			var postMaster = postMastersGroup.Staff.AddNew();
			postMaster.GS_Code = "_PM";
			postMaster.GS_EmailAddress = "righteousbastard@utopianexistence.com";
			postMastersGroup.Factory.Save();
		}
	}
}
