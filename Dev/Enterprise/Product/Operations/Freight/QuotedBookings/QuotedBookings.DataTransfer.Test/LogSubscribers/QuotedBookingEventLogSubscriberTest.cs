using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Test
{
	[TestedType(typeof(QuotedBookingEventLogSubscriber))]
	class QuotedBookingEventLogSubscriberTest : ShipmentEventLogSubscriberTest
	{
		public void TestProcessQueueLogsShipmentEventsWithoutSBR()
		{
			var booking = CreateQuotedBooking();

			AddTriggerEvent(booking, AutoEvents.PickupCartageCompleteFinalisedCode, "");

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

		public void TestProcessQueueLogsShipmentEventsWithSBR_RecipientContainerTracking()
		{
			var booking = CreateQuotedBooking();
			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());

			AddTriggerEvent(booking, AutoEvents.PickupCartageCompleteFinalisedCode, "");

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
			var booking = CreateQuotedBooking();
			var referenceITN = ZGuid.NewZGuid();
			CreateShipmentVisibilitySubscription(booking, referenceITN);
			CreateShipmentVisibilitySubscription(booking, referenceITN);

			AddTriggerEvent(booking, AutoEvents.PickupCartageCompleteFinalisedCode, "");

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
			var booking = CreateQuotedBooking();

			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());
			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());

			AddTriggerEvent(booking, AutoEvents.PickupCartageCompleteFinalisedCode, "");

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
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());

			AddTriggerEvent(booking, AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent(booking, AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(booking, AutoEvents.UnpackingCompletedCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(booking, AutoEvents.PackingCompletedCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(booking, AutoEvents.PickupCartageCompleteFinalisedCode, "");
			AddTriggerEvent(booking, AutoEvents.DeliveryCartageCompleteFinalisedCode, "");
			AddTriggerEvent(booking, AutoEvents.CustomsClearedCode, "");
			AddTriggerEvent(booking, AutoEvents.ReleasedCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(booking, AutoEvents.HeldCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(booking, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent(booking, AutoEvents.StorageCommencedCode, "|LOC=TestLOC|FAC=CTO");

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
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());

			AddTriggerEvent(booking, AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent(booking, AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(booking, AutoEvents.UnpackingCompletedCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(booking, AutoEvents.PackingCompletedCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(booking, AutoEvents.PickupCartageCompleteFinalisedCode, "");
			AddTriggerEvent(booking, AutoEvents.DeliveryCartageCompleteFinalisedCode, "");
			AddTriggerEvent(booking, AutoEvents.CustomsClearedCode, "");
			AddTriggerEvent(booking, AutoEvents.ReleasedCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(booking, AutoEvents.HeldCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(booking, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent(booking, AutoEvents.StorageCommencedCode, "|LOC=TestLOC|FAC=CTO");

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
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = _containerNumber;
			booking.QuotedBookingContainers.Add(container);
			Factory.Save();

			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());
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
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = _containerNumber;
			booking.QuotedBookingContainers.Add(container);
			Factory.Save();

			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());
			RunLogWalkerCycleForTest();

			AddTriggerEvent(booking, AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent(booking, AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CFS|");
			AddTriggerEvent(booking, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent(booking, AutoEvents.CustomsClearedCode, "");
			AddTriggerEvent(booking, AutoEvents.ReleasedCode, "|LOC=TestLOC|DEP=TestDEP");
			AddTriggerEvent(booking, AutoEvents.HeldCode, "|LOC=TestLOC|DEP=TestDEP");
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
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = _containerNumber;
			booking.QuotedBookingContainers.Add(container);
			Factory.Save();

			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());
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

		public void TestProcessQueueLogsShipmentEventsWith_MatchedParameters_PackingmodeLCL()
		{
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());

			AddTriggerEvent(booking, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CFS");
			AddTriggerEvent(booking, AutoEvents.StorageCommencedCode, "|LOC=TestLOC|FAC=CFS");
			Factory.Save();

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

		public void TestProcessQueueLogsShipmentEventsWith_UnMatchedParameters()
		{
			var booking = CreateQuotedBooking();
			CreateShipmentVisibilitySubscription(booking, ZGuid.NewZGuid());

			AddTriggerEvent(booking, AutoEvents.GateOutCode, "");
			AddTriggerEvent(booking, AutoEvents.GateInCode, "");
			AddTriggerEvent(booking, AutoEvents.UnpackingCompletedCode, "");
			AddTriggerEvent(booking, AutoEvents.PackingCompletedCode, "");
			AddTriggerEvent(booking, AutoEvents.ReleasedCode, "");
			AddTriggerEvent(booking, AutoEvents.HeldCode, "");
			AddTriggerEvent(booking, AutoEvents.CargoAvailableCode, "|LOC=TestLOC|FAC=CTO");
			AddTriggerEvent(booking, AutoEvents.StorageCommencedCode, "|LOC=TestLOC|FAC=CTO");
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
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			var referenceITN = ZGuid.NewZGuid();
			CreateShipmentVisibilitySubscription(booking, referenceITN);
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
				AddTriggerEvent(booking, eventCode, eventReference);

				var expectedEventParameters = StmALog.GetParametersFromReference(eventReference);

				var expectedContextParameters = new Dictionary<string, string>();
				AddExpectedCommonContextParamaters(expectedContextParameters);

				AssertProcessQueueLogs_XML_HasCommonEventAndContextParametersPopulated(referenceITN.ToString(), expectedEventParameters, expectedContextParameters);
			}
		}

		public void TestProcessQueueLogs_EventSourceContainer_XML_HasEventContextParametersPopulated()
		{
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var referenceITN = ZGuid.NewZGuid();

			CreateShipmentVisibilitySubscription(booking, referenceITN);

			var container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = _containerNumber;
			booking.QuotedBookingContainers.Add(container);
			Factory.Save();

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-7");
			refContainer.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			refContainer.RC_ISOType = _containerIsoType;
			container.JC_RC = refContainer.PK;
			Factory.Save();

			RunLogWalkerCycleForTest();

			TestXMLMapping_EventSourceContainer(AutoEvents.FreightLoadedCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceContainer(AutoEvents.FreightUnloadedCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceContainer(AutoEvents.GateInCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			TestXMLMapping_EventSourceContainer(AutoEvents.GateOutCode, "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");

			void TestXMLMapping_EventSourceContainer(string eventCode, string eventReference)
			{
				AddTriggerEvent_Container(container, eventCode, eventReference);

				var expectedEventParameters =  StmALog.GetParametersFromReference(eventReference);

				var expectedContextParameters = new Dictionary<string, string>();
				AddExpectedCommonContextParamaters(expectedContextParameters);
				AddExpectedContainerParamaters(expectedContextParameters);

				AssertProcessQueueLogs_XML_HasContainerDetailsPopulated(referenceITN.ToString(), expectedEventParameters, expectedContextParameters);
			}
		}

		public void TestProcessQueueLogs_XML_HasVesselVoyageDetails()
		{
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var referenceITN = ZGuid.NewZGuid();

			CreateShipmentVisibilitySubscription(booking, referenceITN);

			var container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = _containerNumber;
			booking.QuotedBookingContainers.Add(container);
			Factory.Save();
			
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = _vesselName;
			vessel.RV_LloydsNumber = _lloydNumber;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();
			jobVoyage.JV_VoyageFlight = _voyageNumber;
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = _originPort;
			origin.JA_JV = jobVoyage.PK;
			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = _destinationPort;
			destination.JB_JV = jobVoyage.PK;
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			((ISailingChooserParent)booking).SailingJX = sailing.PK;

			RunLogWalkerCycleForTest();

			var eventReference = "|LOC=TestLOC|FAC=CTO|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|";
			AddTriggerEvent_Container(container, AutoEvents.FreightLoadedCode, eventReference);

			var expectedEventParameters = StmALog.GetParametersFromReference(eventReference);

			var expectedContextParameters = new Dictionary<string, string>();
			AddExpectedCommonContextParamaters(expectedContextParameters);
			AddExpectedVesselVoyageParamaters(expectedContextParameters);

			AssertProcessQueueLogs_XML_HasVesselVoyageParametersPopulated(referenceITN.ToString(), expectedEventParameters, expectedContextParameters, false);
		}

		public void TestProcessQueueLogsShipment_XML_HasEventContextParametersPopulated_FromEDIMessage()
		{
			var booking = CreateQuotedBooking();
			booking.Booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			var referenceITN = ZGuid.NewZGuid();
			CreateShipmentVisibilitySubscription(booking, referenceITN);
			Factory.Save();
			RunLogWalkerCycleForTest();

			TestXMLMapping_EventSourceShipment(AutoEvents.UnpackingCompletedCode, "|LOC=TestLOC|FAC=CFS|DEP=TestDEP|TYP=TestTYP|MOD=TestMOD|");
			
			void TestXMLMapping_EventSourceShipment(string eventCode, string eventReference)
			{
				AddTriggerEventWithEDIMessage(booking, eventCode, eventReference);

				var expectedEventParameters = StmALog.GetParametersFromReference(eventReference);

				var expectedContextParameters = new Dictionary<string, string>();
				AddExpectedCommonContextParamaters(expectedContextParameters);
				AddExpectedContainerParamaters(expectedContextParameters);
				AddExpectedVesselVoyageParamaters(expectedContextParameters);

				AssertProcessQueueLogs_XML_HasContainerDetailsPopulated(referenceITN.ToString(), expectedEventParameters, expectedContextParameters);
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

		void AddTriggerEvent(QuotedBooking booking, string eventCode, string eventReference)
		{
			var triggeringEvent = booking.GetLogs().AddNew();
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_SE_NKEvent = eventCode;
				triggeringEvent.SL_Table = ViewQuotedBookingSchema.Constants.TableName;
				triggeringEvent.SL_Reference = eventReference;
			}

			Factory.Save();
		}

		QuotedBooking CreateQuotedBooking()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var bookingJsUniqueConsignRef = _shipmentReference;
			quotedBooking.Booking.JS_UniqueConsignRef = bookingJsUniqueConsignRef;
			quotedBooking.Booking.JS_HouseBill = _houseBillNumber;
			quotedBooking.Booking.JS_RL_NKOrigin = _originPort;
			quotedBooking.Booking.JS_RL_NKDestination = _destinationPort;
			return quotedBooking;
		}

		void CreateShipmentVisibilitySubscription(QuotedBooking booking, ZGuid referenceITN)
		{
			var log = booking.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = AutoEvents.SubscriptionRequested.Code;
				log.SL_Reference = $"|ITN={referenceITN}|TYP=Shipment Visibility|ORG={_organisation}|SER=CA";
				log.SL_EventTime = DateTime.Now;
				log.SL_IsEstimate = false;
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

		void AddTriggerEventWithEDIMessage(QuotedBooking booking, string eventCode, string eventReference)
		{
			var triggeringEvent = booking.GetLogs().AddNew();
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_SE_NKEvent = eventCode;
				triggeringEvent.SL_Table = ViewQuotedBookingSchema.Constants.TableName;
				triggeringEvent.SL_Reference = eventReference;
			}

			var message = Factory.New<Messaging.Integration.IXmlEDIMessage>();
			message.EM_MessageType = Messaging.Integration.EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = Messaging.Integration.EDIMessageSubTypeList.Codes.XmlUniversalEvent;
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
			firstPivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
		}

		#endregion
	}
}
