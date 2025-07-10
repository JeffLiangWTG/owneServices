using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(AgencyShipmentEventLogSubscriber))]
	class AgencyShipmentEventLogSubscriberTest : ShipmentEventLogSubscriberTest
	{
		public void TestProcessQueueLogsShipmentEventsWithoutSBR()
		{
			var shipment = CreateAgencyShipment();

			AddTriggerEvent(shipment, AutoEvents.PickupCartageCompleteFinalisedCode, "");

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();

				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				AssertNotNull(ediMessages);
				AssertEquals(0, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWithSBR()
		{
			var shipment = CreateAgencyShipment();
			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());

			AddTriggerEvent(shipment, AutoEvents.PickupCartageCompleteFinalisedCode, "");

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();

				var ediMessage = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging)).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).FirstOrDefault();
				var interchange = Factory.Load<EDIInterchange>(ediMessage.EM_EI);

				AssertEquals(
					_eHubIdForCA,
					interchange.EI_To);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWithSBR_BillOfLading()
		{
			var shipment = CreateBillOfLading();
			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());

			AddTriggerEvent(shipment, AutoEvents.PickupCartageCompleteFinalisedCode, "");

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();

				var ediMessage = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging)).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).FirstOrDefault();
				var interchange = Factory.Load<EDIInterchange>(ediMessage.EM_EI);

				AssertEquals(
					_eHubIdForCA,
					interchange.EI_To);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_MultipleSBR_SameITN()
		{
			var shipment = CreateAgencyShipment();
			var referenceITN = ZGuid.NewZGuid();
			CreateShipmentVisibilitySubscription(shipment, referenceITN);
			CreateShipmentVisibilitySubscription(shipment, referenceITN);

			AddTriggerEvent(shipment, AutoEvents.PickupCartageCompleteFinalisedCode, "");

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();

				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				AssertNotNull(ediMessages);
				AssertEquals(1, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_MultipleSBR_MultipleITN()
		{
			var shipment = CreateAgencyShipment();

			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());
			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());

			AddTriggerEvent(shipment, AutoEvents.PickupCartageCompleteFinalisedCode, "");

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();

				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				AssertNotNull(ediMessages);
				AssertEquals(2, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_MatchedParameters_NoEventsSetInRegistry()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());

			AddTriggerEvent(shipment, AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent(shipment, AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(shipment, AutoEvents.UnpackingCompletedCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(shipment, AutoEvents.PackingCompletedCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(shipment, AutoEvents.PickupCartageCompleteFinalisedCode, "");
			AddTriggerEvent(shipment, AutoEvents.DeliveryCartageCompleteFinalisedCode, "");
			AddTriggerEvent(shipment, AutoEvents.CustomsClearedCode, "");
			AddTriggerEvent(shipment, AutoEvents.ReleasedCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(shipment, AutoEvents.HeldCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(shipment, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent(shipment, AutoEvents.StorageCommencedCode, "|LOC=TestLOC|FAC=CTO");

			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (WebDataRegistry.Instance.EventVisibilityOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EventVisibilityOverrideCollection()))
			{
				RunLogWalkerCycleForTest();
				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));

				AssertNotNull(ediMessages);
				AssertEquals(0, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_MatchedParameters_Shipment()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());

			AddTriggerEvent(shipment, AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent(shipment, AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(shipment, AutoEvents.UnpackingCompletedCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(shipment, AutoEvents.PackingCompletedCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(shipment, AutoEvents.PickupCartageCompleteFinalisedCode, "");
			AddTriggerEvent(shipment, AutoEvents.DeliveryCartageCompleteFinalisedCode, "");
			AddTriggerEvent(shipment, AutoEvents.CustomsClearedCode, "");
			AddTriggerEvent(shipment, AutoEvents.ReleasedCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(shipment, AutoEvents.HeldCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(shipment, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent(shipment, AutoEvents.StorageCommencedCode, "|LOC=TestLOC|FAC=CTO");

			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));

				AssertNotNull(ediMessages);
				AssertEquals(11, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_MatchedParameters_Container()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_ContainerNum = _containerNumber;
			Factory.Save();

			var packLine = shipment.OuterPackLines.AddNew();
			container.AddPackLine(packLine);
			Factory.Save();

			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());
			RunLogWalkerCycleForTest();

			AddTriggerEvent_Container(container, AutoEvents.FreightLoadedCode, "|LOC=TestLOC|FAC=CTO|MOD=SEA");
			AddTriggerEvent_Container(container, AutoEvents.FreightUnloadedCode, "|LOC=TestLOC|FAC=CTO|MOD=SEA");
			AddTriggerEvent_Container(container, AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CY");
			AddTriggerEvent_Container(container, AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CY|");
			AddTriggerEvent_Container(container, AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent_Container(container, AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CTO|");
			AddTriggerEvent_Container(container, AutoEvents.DehireCode, "|LOC=TestLOC|FAC=CY");

			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));

				AssertNotNull(ediMessages);
				AssertEquals(7, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_MatchedParameters_ShipmentThenContainer_TriggeredFromShipment_MultipleMessagesNotSent()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_ContainerNum = _containerNumber;
			Factory.Save();

			var packLine = shipment.OuterPackLines.AddNew();
			container.AddPackLine(packLine);
			Factory.Save();

			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());
			RunLogWalkerCycleForTest();

			AddTriggerEvent(shipment, AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent(shipment, AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(shipment, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent(shipment, AutoEvents.CustomsClearedCode, "");
			AddTriggerEvent(shipment, AutoEvents.ReleasedCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(shipment, AutoEvents.HeldCode, "|LOC=TestLOC|DEP=TestDEP");

			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));

				AssertNotNull(ediMessages);
				AssertEquals(6, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_MatchedParameters_ShipmentThenContainer_TriggeredFromContainer_MultipleMessagesNotSent()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_ContainerNum = _containerNumber;
			Factory.Save();

			var packLine = shipment.OuterPackLines.AddNew();
			container.AddPackLine(packLine);
			Factory.Save();

			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());
			RunLogWalkerCycleForTest();

			AddTriggerEvent_Container(container, AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent_Container(container, AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent_Container(container, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent_Container(container, AutoEvents.CustomsClearedCode, "");
			AddTriggerEvent_Container(container, AutoEvents.ReleasedCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent_Container(container, AutoEvents.HeldCode, "|LOC=TestLOC|DEP=TestDEP");
			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));

				AssertNotNull(ediMessages);
				AssertEquals(6, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_MatchedParameters_Transport()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());
			Factory.Save();

			AddTriggerEvent(shipment, AutoEvents.ArrivalCode, "|LOC=TestLOC|FAC=CTO|MOD=SEA");
			AddTriggerEvent(shipment, AutoEvents.DepartureCode, "|LOC=TestLOC|FAC=CTO|MOD=SEA");
			AddTriggerEvent(shipment, AutoEvents.CutOffDateCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent(shipment, AutoEvents.ReceiptCommencedCode, "|LOC=TestLOC|FAC=CTO");

			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));

				AssertNotNull(ediMessages);
				AssertEquals(0, ediMessages.Length);
			}

			var transport = shipment.Transports.AddNew();

			AddTriggerEvent_Transport(transport, AutoEvents.ArrivalCode, "|LOC=TestLOC|FAC=CTO|MOD=SEA");
			AddTriggerEvent_Transport(transport, AutoEvents.DepartureCode, "|LOC=TestLOC|FAC=CTO|MOD=SEA");
			AddTriggerEvent_Transport(transport, AutoEvents.CutOffDateCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent_Transport(transport, AutoEvents.ReceiptCommencedCode, "|LOC=TestLOC|FAC=CTO");

			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));

				AssertNotNull(ediMessages);
				AssertEquals(4, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_MatchedParameters_PackingmodeLCL()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());
			var transport = shipment.Transports.AddNew();

			AddTriggerEvent_Transport(transport, AutoEvents.CutOffDateCode, " |LOC=TestLOC|FAC=CFS");
			AddTriggerEvent(shipment, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent_Transport(transport, AutoEvents.ReceiptCommencedCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent(shipment, AutoEvents.StorageCommencedCode, "|LOC=TestLOC|FAC=CFS");
			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));

				AssertNotNull(ediMessages);
				AssertEquals(4, ediMessages.Length);
			}
		}

		public void TestProcessQueueLogsShipmentEventsWith_UnMatchedParameters()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = "";
			CreateShipmentVisibilitySubscription(shipment, ZGuid.NewZGuid());

			AddTriggerEvent(shipment, AutoEvents.GateOutCode, "");
			AddTriggerEvent(shipment, AutoEvents.GateInCode, "");
			AddTriggerEvent(shipment, AutoEvents.UnpackingCompletedCode, "");
			AddTriggerEvent(shipment, AutoEvents.PackingCompletedCode, "");
			AddTriggerEvent(shipment, AutoEvents.ReleasedCode, "");
			AddTriggerEvent(shipment, AutoEvents.HeldCode, "");
			AddTriggerEvent(shipment, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent(shipment, AutoEvents.StorageCommencedCode, "|LOC=TestLOC|FAC=CTO");
			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));

				AssertNotNull(ediMessages);
				AssertEquals(0, ediMessages.Length);
			}
		}

		#region XmlMappingTests

		public void TestProcessQueueLogsShipment_XML_HasEventContextParametersPopulated()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var referenceITN = ZGuid.NewZGuid();
			CreateShipmentVisibilitySubscription(shipment, referenceITN);

			Factory.Save();
			RunLogWalkerCycleForTest();

			TestXMLMapping_EventSourceShipment(AutoEvents.UnpackingCompletedCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceShipment(AutoEvents.PackingCompletedCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceShipment(AutoEvents.CustomsClearedCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceShipment(AutoEvents.ReleasedCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceShipment(AutoEvents.HeldCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceShipment(AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceShipment(AutoEvents.StorageCommencedCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceShipment(AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceShipment(AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");

			void TestXMLMapping_EventSourceShipment(string eventCode, string eventReference)
			{
				AddTriggerEvent(shipment, eventCode, eventReference);

				var expectedEventParameters = StmALog.GetParametersFromReference(eventReference);

				var expectedContextParameters = new Dictionary<string, string>();
				AddExpectedCommonContextParamaters(expectedContextParameters);

				AssertProcessQueueLogs_XML_HasCommonEventAndContextParametersPopulated(referenceITN.ToString(), expectedEventParameters, expectedContextParameters);
			}
		}

		public void TestProcessQueueLogs_SourceTransport_XML_HasVesselVoyagePopulated()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var referenceITN = ZGuid.NewZGuid();

			CreateShipmentVisibilitySubscription(shipment, referenceITN);

			var transport = shipment.Transports.AddNew();
			var vessel = Factory.New<RefVessel>();

			vessel.RV_Name = _vesselName;
			vessel.RV_LloydsNumber = _lloydNumber;
			vessel.RV_Code = _vesselName;
			Factory.Save();

			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = _voyageNumber;
			transport.JW_RL_NKLoadPort = _originPort;
			transport.JW_RL_NKDiscPort = _destinationPort;

			Factory.Save();
			RunLogWalkerCycleForTest();

			TestXMLMapping_EventSourceTransport(AutoEvents.ArrivalCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceTransport(AutoEvents.DepartureCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceTransport(AutoEvents.CutOffDateCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceTransport(AutoEvents.ReceiptCommencedCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");

			void TestXMLMapping_EventSourceTransport(string eventCode, string eventReference)
			{
				AddTriggerEvent_Transport(transport, eventCode, eventReference);

				var expectedEventParameters = StmALog.GetParametersFromReference(eventReference);

				var expectedContextParameters = new Dictionary<string, string>();
				AddExpectedCommonContextParamaters(expectedContextParameters);
				AddExpectedVesselVoyageParamaters(expectedContextParameters);

				AssertProcessQueueLogs_XML_HasVesselVoyageParametersPopulated(referenceITN.ToString(), expectedEventParameters, expectedContextParameters, false);
			}
		}

		public void TestProcessQueueLogs_EventSourceContainer_XML_HasContainerDetailsPopulated()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var referenceITN = ZGuid.NewZGuid();

			CreateShipmentVisibilitySubscription(shipment, referenceITN);
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_ContainerNum = _containerNumber;
			Factory.Save();

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-7");
			refContainer.RC_ContainerType = Constants.ContainerTypes.Refrigerated;
			refContainer.RC_ISOType = _containerIsoType;
			container.JC_RC = refContainer.PK;

			var packLine = shipment.OuterPackLines.AddNew();
			container.AddPackLine(packLine);
			Factory.Save();
			RunLogWalkerCycleForTest();

			TestXMLMapping_EventSourceContainer(AutoEvents.FreightLoadedCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceContainer(AutoEvents.FreightUnloadedCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceContainer(AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceContainer(AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");

			void TestXMLMapping_EventSourceContainer(string eventCode, string eventReference)
			{
				AddTriggerEvent_Container(container, eventCode, eventReference);

				var expectedEventParameters = StmALog.GetParametersFromReference(eventReference);

				var expectedContextParameters = new Dictionary<string, string>();
				AddExpectedCommonContextParamaters(expectedContextParameters);
				AddExpectedContainerParamaters(expectedContextParameters);

				AssertProcessQueueLogs_XML_HasContainerDetailsPopulated(referenceITN.ToString(), expectedEventParameters, expectedContextParameters);
			}
		}

		public void TestProcessQueueLogsShipment_XML_HasEventContextParametersPopulated_FromEDIMessage()
		{
			var shipment = CreateAgencyShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var referenceITN = ZGuid.NewZGuid();
			CreateShipmentVisibilitySubscription(shipment, referenceITN);

			Factory.Save();
			RunLogWalkerCycleForTest();

			TestXMLMapping_EventSourceShipment(AutoEvents.UnpackingCompletedCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			
			void TestXMLMapping_EventSourceShipment(string eventCode, string eventReference)
			{
				AddTriggerEventWithEDIMessage(shipment, eventCode, eventReference);

				var expectedEventParameters = StmALog.GetParametersFromReference(eventReference);

				var expectedContextParameters = new Dictionary<string, string>();
				AddExpectedCommonContextParamaters(expectedContextParameters);
				AddExpectedContainerParamaters(expectedContextParameters);
				AddExpectedVesselVoyageParamaters(expectedContextParameters);

				AssertProcessQueueLogs_XML_HasAllContextParametersPopulated(referenceITN.ToString(), expectedEventParameters, expectedContextParameters);
			}
		}

		void AddExpectedCommonContextParamaters(Dictionary<string, string> expectedContextParameters)
		{
			expectedContextParameters.Add("CarrierC1CCode", _organisation);
			expectedContextParameters.Add("CarriersBookingReference", _shipmentReference);
			expectedContextParameters.Add("MBOLNumber", _houseBillNumber);
			expectedContextParameters.Add("MBOLOriginUNLOCO", _originPort);
			expectedContextParameters.Add("MBOLDestinationUNLOCO", _destinationPort);
		}

		void AddExpectedContainerParamaters(Dictionary<string, string> expectedContextParameters)
		{
			expectedContextParameters.Add("ContainerNumber", _containerNumber);
			expectedContextParameters.Add("ContainerISOCode", _containerIsoType);
		}

		void AddExpectedVesselVoyageParamaters(Dictionary<string, string> expectedContextParameters)
		{
			expectedContextParameters.Add("VesselName", _vesselName);
			expectedContextParameters.Add("LloydsNumber", _lloydNumber);
			expectedContextParameters.Add("VoyageNumber", _voyageNumber);
			expectedContextParameters.Add("LegOriginUNLOCO", _originPort);
			expectedContextParameters.Add("LegDestinationUNLOCO", _destinationPort);
		}

		#endregion

		#region Implementations

		readonly string _shipmentReference = "S000128";
		readonly string _houseBillNumber = "TEST001";
		readonly string _originPort = "AUSYD";
		readonly string _destinationPort = "USLAX";
		readonly string _vesselName = "TestVessel";
		readonly string _lloydNumber = "TLloyds";
		readonly string _voyageNumber = "Q123";
		readonly string _containerNumber = "C00009999";
		readonly string _containerIsoType = "22R1";
		readonly string _organisation = "TestORG";

		AgencyShipment CreateAgencyShipment()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_RL_NKOrigin = _originPort;
			shipment.JS_RL_NKDestination = _destinationPort;
			shipment.JS_HouseBill = _houseBillNumber;
			shipment.JS_UniqueConsignRef = _shipmentReference;
			return shipment;
		}

		BillOfLading CreateBillOfLading()
		{
			BillOfLading shipment = Factory.NewWithValidTestData<BillOfLading>();
			shipment.JS_UniqueConsignRef = "S00000100";
			return shipment;
		}

		void CreateShipmentVisibilitySubscription(AgencyShipment shipment, ZGuid referenceITN)
		{
			var log = shipment.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = AutoEvents.SubscriptionRequested.Code;
				log.SL_Reference = $"|ITN={referenceITN}|TYP=Shipment Visibility|ORG=TestORG|SER=CA";
				log.SL_EventTime = DateTime.Now;
				log.SL_IsEstimate = false;
			}
			Factory.Save();
		}

		void AddTriggerEvent(AgencyShipment shipment, string eventCode, string eventReference)
		{
			var triggeringEvent = shipment.GetLogs().AddNew();
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_SE_NKEvent = eventCode;
				triggeringEvent.SL_Table = JobShipmentSchema.Constants.TableName;
				triggeringEvent.SL_Reference = eventReference;
			}

			Factory.Save();
		}

		void AddTriggerEvent_Transport(Transport transport, string eventCode, string eventReference)
		{
			var triggeringEvent = transport.GetLogs().AddNew();
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_SE_NKEvent = eventCode;
				triggeringEvent.SL_Table = JobConsolTransportSchema.Constants.TableName;
				triggeringEvent.SL_Reference = eventReference;
			}

			Factory.Save();
		}

		void AddTriggerEvent_Container(CommonContainer container, string eventCode, string eventReference)
		{
			var triggeringEvent = container.GetLogs().AddNew();
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_SE_NKEvent = eventCode;
				triggeringEvent.SL_Table = JobContainerSchema.Constants.TableName;
				triggeringEvent.SL_Reference = eventReference;
			}

			Factory.Save();
		}

		void AddTriggerEventWithEDIMessage(AgencyShipment shipment, string eventCode, string eventReference)
		{
			var triggeringEvent = shipment.GetLogs().AddNew();
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_SE_NKEvent = eventCode;
				triggeringEvent.SL_Table = JobShipmentSchema.Constants.TableName;
				triggeringEvent.SL_Reference = eventReference;
			}

			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse($@"
			<UniversalEvent>
				<Event>
					<EventType>{triggeringEvent.SL_SE_NKEvent}</EventType>
					<EventTime>{triggeringEvent.SL_EventTime}</EventTime>
					<ContextCollection>
						<Context>
							<Type>CarriersBookingReference</Type>
							<Value>{_shipmentReference}</Value>
						</Context>
						<Context>
							<Type>MBOLNumber</Type>
							<Value>{_houseBillNumber}</Value>
						</Context>
						<Context>
							<Type>ContainerNumber</Type>
							<Value>{_containerNumber}</Value>
						</Context>
						<Context>
							<Type>ContainerISOCode</Type>
							<Value>{_containerIsoType}</Value>
						</Context>
						<Context>
							<Type>VesselName</Type>
							<Value>{_vesselName}</Value>
						</Context>
						<Context>
							<Type>LloydsNumber</Type>
							<Value>{_lloydNumber}</Value>
						</Context>
						<Context>
							<Type>VoyageNumber</Type>
							<Value>{_voyageNumber}</Value>
						</Context>
						<Context>
							<Type>LegOriginUNLOCO</Type>
							<Value>{_originPort}</Value>
						</Context>
						<Context>
							<Type>LegDestinationUNLOCO</Type>
							<Value>{_destinationPort}</Value>
						</Context>
						<Context>
							<Type>MBOLOriginUNLOCO</Type>
							<Value>{_originPort}</Value>
						</Context>
						<Context>
							<Type>MBOLDestinationUNLOCO</Type>
							<Value>{_destinationPort}</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>");

			var firstPivot = Factory.New<IGenPivot>();
			firstPivot.XX_Relation1ID = triggeringEvent.PK;
			firstPivot.XX_Relation1TableCode = "SL";
			firstPivot.XX_Relation2ID = message.PK;
			firstPivot.XX_Relation2TableCode = "EM";
			firstPivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
		}
		#endregion
	}
}
