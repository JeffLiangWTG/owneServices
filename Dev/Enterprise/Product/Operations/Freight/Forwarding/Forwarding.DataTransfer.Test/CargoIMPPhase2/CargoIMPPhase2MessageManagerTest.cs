using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(CargoIMPPhase2MessageManager))]
	public class CargoIMPPhase2MessageManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsRouteMapInformationMessageCanBeCreated()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);

			NotificationBuffer notifications = new NotificationBuffer();
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"CargoIMP Phase 2 integration is only available for Air Shipments.
", notifications.AsString);

			notifications.Clear();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"'Forwarder Identification' registry item must be set
", notifications.AsString);

			notifications.Clear();
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "YAS");
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy.
", notifications.AsString);

			notifications.Clear();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			shipment.Consols.Add(consol);
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy.
", notifications.AsString);

			notifications.Clear();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy.
", notifications.AsString);

			notifications.Clear();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy.
", notifications.AsString);

			notifications.Clear();
			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy.
", notifications.AsString);

			notifications.Clear();

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy.
", notifications.AsString);

			notifications.Clear();
			var relatedParty1 = org1.AllRelatedParties.AddNew();
			relatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty1.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy.
", notifications.AsString);

			notifications.Clear();
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy.
", notifications.AsString);

			notifications.Clear();
			var relatedParty2 = org2.AllRelatedParties.AddNew();
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty2.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"No records found in 'Supported Routes' registry item corresponding to any Flights in consolidation
", notifications.AsString);

			CargoIMPPhase2RouteMapCollection routeMapColl = new CargoIMPPhase2RouteMapCollection();
			var routeMap = routeMapColl.AddNew();
			routeMap.Origin = "AUSYD";
			routeMap.Destination = "NZAKL";
			routeMap.AirlineTwoCharacterCode = "QF";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2RouteMap.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, routeMapColl);

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "NZAKL";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "123455";
			transport.JW_VoyageFlight = "QF1234";

			notifications.Clear();
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"No records found in 'Supported Routes' registry item corresponding to any Flights in consolidation
", notifications.AsString);

			notifications.Clear();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "UA";
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"No records found in 'Supported Routes' registry item corresponding to any Flights in consolidation
", notifications.AsString);

			notifications.Clear();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZLYT";
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"No records found in 'Supported Routes' registry item corresponding to any Flights in consolidation
", notifications.AsString);

			notifications.Clear();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_VoyageFlight = "QA1234";
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"No records found in 'Supported Routes' registry item corresponding to any Flights in consolidation
", notifications.AsString);

			notifications.Clear();
			transport.JW_VoyageFlight = "QF1234";
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			routeMap.Origin = "AU";
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			routeMap.Destination = "NZ";
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			var newProxy = Factory.New<OrgHeader>();
			newProxy.OH_Code = "PROXY1";
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = newProxy.PK;
			Factory.Save();

			GlbBranch.CurrentBranch.Factory.ReloadAll<GlbBranch>();

			relatedParty1.PR_OH_RelatedParty = newProxy.PK;
			relatedParty2.PR_OH_RelatedParty = newProxy.PK;
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			var message = manager.Messages.AddNew();
			message.EM_MessageText = "MESSAGE1";
			message.EM_MessageType = CargoIMPPhase2MessageManager.RouteMapInformationType;

			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_MessageSubType = CargoIMPPhase2MSUEventCodeList.Codes.POD;
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_MessageType = CargoIMPPhase2MessageManager.MilestoneStatusUpdateType;
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-3);
			message.ResetMessageDateTimeForTesting();
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-18);
			message.ResetMessageDateTimeForTesting();
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"No CargoIMP Phase 2 message can be sent after previously sent POD Status Update message.
", notifications.AsString);

			notifications.Clear();
			message.EM_MessageSubType = CargoIMPPhase2MSUEventCodeList.Codes.DIW;
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_MessageSubType = CargoIMPPhase2MSUEventCodeList.Codes.POD;

			var message1 = manager.Messages.AddNew();
			message1.EM_MessageText = "MESSAGE2";
			message1.EM_MessageType = CargoIMPPhase2MessageManager.RouteMapCancellationType;
			message1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-10);
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message1.EM_MessageType = CargoIMPPhase2MessageManager.RouteMapInformationType;
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = Guid.Empty;
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"The consol must be gateway-enabled and the Sending and Receiving agent must be in the same Management grouping as the Current Company/Branch Organization proxy.
", notifications.AsString);
		}

		public void TestIsRouteMapCancellationMessageCanBeCreated()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);

			NotificationBuffer notifications = new NotificationBuffer();
			AssertEquals(false, manager.IsRouteMapCancellationMessageCanBeCreated(notifications));
			AssertEquals(@"CargoIMP Phase 2 Cancellation message can be sent only after previously sent Route Map or Status Update message.
", notifications.AsString);

			var message = manager.Messages.AddNew();
			message.EM_MessageText = "MESSAGE1";
			message.EM_MessageType = "RMX";

			notifications.Clear();
			AssertEquals(false, manager.IsRouteMapCancellationMessageCanBeCreated(notifications));
			AssertEquals(@"CargoIMP Phase 2 Cancellation message can be sent only after previously sent Route Map or Status Update message.
", notifications.AsString);

			notifications.Clear();
			message.EM_MessageType = "RMI";
			AssertEquals(true, manager.IsRouteMapCancellationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);
		}

		public void TestIsMilestoneStatusUpdateCanBeCreated()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);

			NotificationBuffer notifications = new NotificationBuffer();
			var message = manager.Messages.AddNew();
			message.EM_MessageText = "MESSAGE1";
			message.EM_MessageType = "RMX";

			notifications.Clear();
			AssertEquals(false, manager.IsMilestoneStatusUpdateCanBeCreated(notifications));
			AssertEquals(@"CargoIMP Phase 2 Status Update message can be sent only after previously sent Route Map or Status Update message.
", notifications.AsString);

			notifications.Clear();
			message.EM_MessageType = "RMI";
			AssertEquals(false, manager.IsMilestoneStatusUpdateCanBeCreated(notifications));
			AssertEquals(@"CargoIMP Phase 2 integration is only available for Air Shipments.
", notifications.AsString);

			notifications.Clear();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals(false, manager.IsMilestoneStatusUpdateCanBeCreated(notifications));
			AssertEquals(@"'Forwarder Identification' registry item must be set
", notifications.AsString);

			notifications.Clear();
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

			shipment.Consols.Add(consol);

			AssertEquals(true, manager.IsMilestoneStatusUpdateCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_MessageSubType = CargoIMPPhase2MSUEventCodeList.Codes.POD;
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_MessageType = CargoIMPPhase2MessageManager.MilestoneStatusUpdateType;
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-3);
			message.ResetMessageDateTimeForTesting();
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-17);
			message.ResetMessageDateTimeForTesting();
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"No CargoIMP Phase 2 message can be sent after previously sent POD Status Update message.
", notifications.AsString);
			notifications.Clear();

			message.EM_MessageSubType = CargoIMPPhase2MSUEventCodeList.Codes.DIW;
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message.EM_MessageSubType = CargoIMPPhase2MSUEventCodeList.Codes.POD;

			var message1 = manager.Messages.AddNew();
			message1.EM_MessageText = "MESSAGE2";
			message1.EM_MessageType = CargoIMPPhase2MessageManager.RouteMapCancellationType;
			message1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-10);
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message1.EM_MessageType = CargoIMPPhase2MessageManager.RouteMapInformationType;
			AssertEquals(true, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals("", notifications.AsString);

			message1.EM_MessageType = CargoIMPPhase2MessageManager.MilestoneStatusUpdateType;
			message1.EM_MessageSubType = CargoIMPPhase2MSUEventCodeList.Codes.DIW;
			AssertEquals(false, manager.IsRouteMapInformationMessageCanBeCreated(notifications));
			AssertEquals(@"No CargoIMP Phase 2 message can be sent after previously sent POD Status Update message.
", notifications.AsString);
			notifications.Clear();
		}

		public void TestIsShipmentAttachedToCorrectConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);

			NotificationBuffer notifications = new NotificationBuffer();
			AssertEquals(false, manager.IsShipmentAttachedToCorrectConsol());

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			shipment.Consols.Add(consol);
			AssertEquals(false, manager.IsShipmentAttachedToCorrectConsol());

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(false, manager.IsShipmentAttachedToCorrectConsol());

			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals(false, manager.IsShipmentAttachedToCorrectConsol());

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			AssertEquals(false, manager.IsShipmentAttachedToCorrectConsol());

			var relatedParty1 = org1.AllRelatedParties.AddNew();
			relatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty1.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			AssertEquals(false, manager.IsShipmentAttachedToCorrectConsol());

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			AssertEquals(false, manager.IsShipmentAttachedToCorrectConsol());

			var relatedParty2 = org2.AllRelatedParties.AddNew();
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty2.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;
			AssertEquals(true, manager.IsShipmentAttachedToCorrectConsol());
		}

		public void TestNullOrgProxyDoesntThrowException()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			shipment.Consols.Add(consol);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE1";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CODE2";
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			var relatedParty1 = org1.AllRelatedParties.AddNew();
			relatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty1.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			var relatedParty2 = org2.AllRelatedParties.AddNew();
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relatedParty2.PR_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals(false, manager.IsShipmentAttachedToCorrectConsol());
		}

		#region Creation

		public void TestCreateRouteMapInformationMessage()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			Factory.Save();

			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			NotificationBufferTestClass notificationBuffer = new NotificationBufferTestClass();
			manager.CreateRouteMapInformationMessage(notificationBuffer);
			AssertEquals(0, manager.Messages.Count);
			AssertEquals(@"Error: Required field empty (Weight)
Message was created with errors. Sending this message with errors is not allowed as it will be rejected.
", notificationBuffer.AsString);
			Assert("No question was asked", string.IsNullOrEmpty(notificationBuffer.LastQueryUserMessage));
			notificationBuffer.Clear();

			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2AllowToSendMessagesWithErrors.SetValue(Guid.Empty, Guid.Empty,
					Guid.Empty, true);
			notificationBuffer.AllowToSaveWithErrors = false;
			manager.CreateRouteMapInformationMessage(notificationBuffer);
			AssertEquals(0, manager.Messages.Count);
			AssertEquals(@"Message was created with errors.
Sending this message with errors is not advised
as it will most likely be rejected and cause delays in your process.
Do you still wish to send it?", notificationBuffer.LastQueryUserMessage);
			notificationBuffer.LastQueryUserMessage = "";
			notificationBuffer.Clear();

			notificationBuffer.AllowToSaveWithErrors = true;
			manager.CreateRouteMapInformationMessage(notificationBuffer);
			AssertEquals(1, manager.Messages.Count);
			EDIMessage message = manager.Messages[0];
			AssertEquals(shipment.PK, message.EM_LinkUniqueID);
			AssertEquals(JobShipmentSchema.Constants.TableName, message.EM_LinkTable);
			AssertEquals(ApplicationCodeList.Codes.CargoIMPPhase2, message.EM_ApplicationCode);
			AssertEquals(CargoIMPPhase2MessageManager.RouteMapInformationType, message.EM_MessageType);
			AssertEquals(ZString.Empty, message.EM_MessageSubType);
			AssertContains(@"RMI,3
MSG,", message.EM_MessageText);
			AssertEquals(@"Message was created with errors.
Sending this message with errors is not advised
as it will most likely be rejected and cause delays in your process.
Do you still wish to send it?", notificationBuffer.LastQueryUserMessage);
			notificationBuffer.LastQueryUserMessage = "";
			notificationBuffer.Clear();
			manager.Messages.RemoveAndDeleteAllFromTest();

			shipment.JS_ActualWeight = 1300.345M;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1;";
			shipment.JS_UniqueConsignRef = "AAA_VV11";
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org1.MainAddress.PK;
			manager.CreateRouteMapInformationMessage(notificationBuffer);
			AssertEquals(1, manager.Messages.Count);
			message = manager.Messages[0];
			AssertEquals(shipment.PK, message.EM_LinkUniqueID);
			AssertEquals(@"Warning: Shipment ID: value can't be formatted, Input data must consist of 'Text character - only comprising upper case alphabetic, numeric, hyphen, full stop, forward slash and space' with length from 0 to 20
", notificationBuffer.AsString);
			AssertEquals(@"Message was created with warnings.
Sending this message with warnings can cause delays in your process.
Do you still wish to send it?", notificationBuffer.LastQueryUserMessage);
			notificationBuffer.LastQueryUserMessage = "";
			notificationBuffer.Clear();
			manager.Messages.RemoveAndDeleteAllFromTest();

			org1.OH_Code = "COD1";
			shipment.JS_UniqueConsignRef = "AAA VV11";
			manager.CreateRouteMapInformationMessage(notificationBuffer);
			AssertEquals(1, manager.Messages.Count);
			message = manager.Messages[0];
			AssertEquals(shipment.PK, message.EM_LinkUniqueID);
			AssertEquals("", notificationBuffer.LastQueryUserMessage);
			AssertEquals(0, notificationBuffer.Events.Length);
		}

		public void TestCreateMilestoneStatusUpdateMessage()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "12345678901234567";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 1200.467M;
			Factory.Save();

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "COD1";
			org1.OH_RL_NKClosestPort = "AUALX";
			shipment.JS_OA_ExportReceivingDepot = org1.MainAddress.PK;

			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			NotificationBufferTestClass notificationBuffer = new NotificationBufferTestClass();
			manager.CreateMilestoneStatusUpdateMessage(CargoIMPPhase2MSUEventCodeList.Codes.REW, ZDateTime.Now, notificationBuffer);
			AssertEquals(1, manager.Messages.Count);
			EDIMessage message = manager.Messages[0];
			AssertEquals(shipment.PK, message.EM_LinkUniqueID);
			AssertEquals(CargoIMPPhase2MessageManager.MilestoneStatusUpdateType, message.EM_MessageType);
			AssertEquals(CargoIMPPhase2MSUEventCodeList.Codes.REW, message.EM_MessageSubType);
			AssertEquals(null, notificationBuffer.LastQueryUserMessage);
			AssertEquals(0, notificationBuffer.Events.Length);
		}

		public void TestCreateRouteMapCancellationMessage()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "12345678901234567";
			Factory.Save();

			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			NotificationBufferTestClass notificationBuffer = new NotificationBufferTestClass();
			manager.CreateRouteMapCancellationMessage(notificationBuffer);
			AssertEquals(1, manager.Messages.Count);
			EDIMessage message = manager.Messages[0];
			AssertEquals(shipment.PK, message.EM_LinkUniqueID);
			AssertEquals(CargoIMPPhase2MessageManager.RouteMapCancellationType, message.EM_MessageType);
			AssertEquals(ZString.Empty, message.EM_MessageSubType);
			AssertEquals(null, notificationBuffer.LastQueryUserMessage);
			AssertEquals(0, notificationBuffer.Events.Length);
		}

		#endregion

		#region Saving/Message Number

		[ExpectNoExceptions]
		public void TestSaving()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2AllowToSendMessagesWithErrors.SetValue(Guid.Empty, Guid.Empty,
					Guid.Empty, true);
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			Factory.Save();

			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			NotificationBufferTestClass notificationBuffer = new NotificationBufferTestClass();
			manager.CreateRouteMapInformationMessage(notificationBuffer);
			EDIMessage message = manager.Messages[0];
			Factory.Save();

			long nextMessageNumber = Env.NumberFountains.EDIFACTNumberFountain("M", "PIMA", "CARGOIMPPHASE2").PeekPreliminary(Db.Connection);
			long justUsedNumber = nextMessageNumber - 1;

			AssertEquals(justUsedNumber.ToString(), message.EM_MessageNum);
		}

		#endregion

		public void TestCurrentStatus()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");

			AssertEquals("No messages have been sent", Manager.CurrentStatus);
			AssertStatus("Message Queued for Sending", EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, CargoIMPPhase2MessageManager.RouteMapInformationType, string.Empty);
			AssertStatus("Message Failed", EDIMessage.Direction.Transmit, EDIMessage.Status.Failed, CargoIMPPhase2MessageManager.RouteMapInformationType, string.Empty);
			AssertStatus("Route Map Information Sent", EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, CargoIMPPhase2MessageManager.RouteMapInformationType, string.Empty);
			AssertStatus("Message Canceled", EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, CargoIMPPhase2MessageManager.RouteMapCancellationType, string.Empty);
			AssertStatus("Milestone Status Update Sent", EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, CargoIMPPhase2MessageManager.MilestoneStatusUpdateType, string.Empty);
			AssertStatus("Milestone Status Update (POD) Sent", EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, CargoIMPPhase2MessageManager.MilestoneStatusUpdateType, "POD");
			AssertStatus("Milestone Status Update (DEW) Sent", EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, CargoIMPPhase2MessageManager.MilestoneStatusUpdateType, "DEW");
		}

		public void TestLastMessageCreationLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CargoIMPPhase2MessageManager manager = CargoIMPPhase2MessageManager.New(shipment);
			AssertEquals("", manager.LastMessageCreationLog);

			CargoIMPPhase2MessageNote note = new CargoIMPPhase2MessageNote(shipment);
			note.WriteToNote("text");
			manager = CargoIMPPhase2MessageManager.New(shipment);
			AssertEquals("text", manager.LastMessageCreationLog);
		}

		CargoIMPPhase2MessageManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = CargoIMPPhase2MessageManager.New(Factory.NewWithValidTestData<ForwardingShipment>());
				}

				return fManager;
			}
		}
		CargoIMPPhase2MessageManager fManager;

		protected override BusinessObject GetNewBusinessObject()
		{
			return CargoIMPPhase2MessageManager.New(Factory.NewWithValidTestData<ForwardingShipment>());
		}

		[ExpectNoExceptions]
		public void TestCriticalError()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2AllowToSendMessagesWithErrors.SetValue(Guid.Empty, Guid.Empty,
					Guid.Empty, true);
			NotificationBufferTestClass notificationBuffer = new NotificationBufferTestClass();
			notificationBuffer.AllowToSaveWithErrors = true;
			Manager.CreateRouteMapCancellationMessage(notificationBuffer);
			AssertEquals(0, Manager.Messages.Count);
			AssertContains("Message was created with errors." +
				" Sending this message with errors is not allowed as it will be rejected.", notificationBuffer.AsString);
			AssertContains(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.Caption, notificationBuffer.AsString);
			notificationBuffer.Clear();
		}

		protected void AssertStatus(string expectedStatus, string direction, string status,
					string messageType, string messageSubType)
		{
			EDIMessage message = Manager.Messages.AddNew();
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			AssertEquals(expectedStatus, Manager.CurrentStatus);
		}
	}
}
