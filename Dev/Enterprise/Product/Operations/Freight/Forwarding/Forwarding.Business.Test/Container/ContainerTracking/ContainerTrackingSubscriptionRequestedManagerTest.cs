using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ContainerTrackingSubscriptionRequestedManagerTest : TestCaseWithFactory
	{
		public void TestSubscriptionRequestedEventReference()
		{
			AssertEquals("|TYP=Container Tracking", ContainerTrackingSubscriptionRequestedManager.SubscriptionRequestedEventReference);

			var eventParameters = new Dictionary<string, string>
			{
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Constants.EventReferenceParameterTypes.ContainerTracking
			};

			var generatedEventReference = StmALog.GenerateEventReference("", eventParameters);
			AssertEquals("Sanity check", generatedEventReference, ContainerTrackingSubscriptionRequestedManager.SubscriptionRequestedEventReference);
		}

		public void TestCtors_NullParametersAreNotAllowed_ThrowsException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ContainerTrackingSubscriptionRequestedManager(null));
			AssertExceptionThrown<ArgumentNullException>(() => new ContainerTrackingSubscriptionRequestedManager(null, null));
		}

		public void TestGlobalContainerTrackingDisabled_DoNothing()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();

			var manager = new ContainerTrackingSubscriptionRequestedManager(consol);
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				manager.UpdateIfNecessary();
				AssertNull("Event on consol", consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested));
			}

			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				manager.UpdateIfNecessary();
				AssertNotNull("Event on consol", consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested));
			}
		}

		public void TestUpdateContainerTrackingProvider_CancelExistingEventsOnContainers()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.Containers[0].Logs.AddNew(Events.SubscriptionRequested);

			RunAndAssert("Cancel existing events on containers", consol, eventExpected: true);
		}

		public void TestIsApplicable_TransportMode()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			RunAndAssert("Not applicable for AIR", consol, eventExpected: false);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			RunAndAssert("Applicable for SEA", consol, eventExpected: true);

			void RunAndAssertRailOrRoad(string mode, bool isShippingLine, bool isNVOCC, bool eventExpected)
			{
				consol.JK_TransportMode = mode;
				consol.ShippingLine.OH_IsShippingLine = isShippingLine;
				consol.ShippingLine.OH_IsSeaWholesaler = isNVOCC;
				RunAndAssert($"SBR for {mode} when isShippingLine={isShippingLine}, isNVOCC={isNVOCC}", consol, eventExpected);
			}

			RunAndAssertRailOrRoad(Constants.TransportModes.Rail, true, false, true);
			RunAndAssertRailOrRoad(Constants.TransportModes.Rail, false, true, true);
			RunAndAssertRailOrRoad(Constants.TransportModes.Rail, false, false, false);
			RunAndAssertRailOrRoad(Constants.TransportModes.Road, true, false, true);
			RunAndAssertRailOrRoad(Constants.TransportModes.Road, false, true, true);
			RunAndAssertRailOrRoad(Constants.TransportModes.Road, false, false, false);
		}

		public void TestIsApplicable_ShippingLine()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;

			RunAndAssert("Missing shipping line", consol, eventExpected: false);
		}

		public void TestIsApplicable_CarrierCode()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();

			var carrierCode = consol.ShippingLine.CustomsCodes
				.Cast<OrgCusCode>()
				.Single(cc => cc.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode);

			carrierCode.OK_CustomsRegNo = "WRONGFORMAT";
			RunAndAssert("Carrier code is in wrong format", consol, eventExpected: true);

			carrierCode.OK_CustomsRegNo = "";
			RunAndAssert("Carrier code is empty", consol, eventExpected: true);

			carrierCode.OK_CustomsRegNo = "YEAH";
			RunAndAssert("Carrier code checked", consol, eventExpected: true);
		}

		public void TestIsApplicable_ShouldHaveEitherBookingReferenceOrMasterbillNumber()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_BookingReference = "";
			consol.JK_MasterBillNum = "";
			RunAndAssert("Both essential numbers are empty", consol, eventExpected: false);

			consol.JK_BookingReference = "12345678";
			consol.JK_MasterBillNum = "";
			RunAndAssert("BookingReference checked", consol, eventExpected: true);

			consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_BookingReference = "";
			consol.JK_MasterBillNum = "12345678";
			RunAndAssert("MasterBillNum checked", consol, eventExpected: true);
		}

		public void TestIsApplicable_BookingReference()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_BookingReference = "SHORT";
			RunAndAssert("BookingReference is invalid", consol, eventExpected: true);

			consol.JK_BookingReference = "12345678*&";
			RunAndAssert("BookingReference is invalid", consol, eventExpected: true);

			consol.JK_BookingReference = "12345678";
			RunAndAssert("BookingReference is valid", consol, eventExpected: true);
		}

		public void TestIsApplicable_MasterBillNum()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_MasterBillNum = "SHORT";
			RunAndAssert("MasterBillNum is invalid", consol, eventExpected: true);

			consol.JK_MasterBillNum = "12345678*&";
			RunAndAssert("MasterBillNum is invalid", consol, eventExpected: true);

			consol.JK_MasterBillNum = "12345678";
			RunAndAssert("MasterBillNum is valid", consol, eventExpected: true);
		}

		public void TestIsApplicable_NoExistingLog_NoArrivalDate_NoDepartureDates_NewLogCreated()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			RunAndAssert("Original event log created", consol, eventExpected: true);

			Factory.Save();

			var log = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested);
			log.Cancel();

			consol.Transports[0].JW_ETA = ZDateTime.Empty;
			consol.Transports[0].JW_ETD = ZDateTime.Empty;
			RunAndAssert("Dates are not entered, but log should be created", consol, eventExpected: true);
		}

		public void TestIsApplicable_HasExistingLog_OldArrivalDate_WouldNotCancelExistingLog()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			RunAndAssert("Original event log created", consol, eventExpected: true);

			Factory.Save();

			consol.Transports[0].JW_ETA = ZDateTime.Today.AddMonths(-3);
			RunAndAssert("Existing event log not cancelled", consol, eventExpected: true);

			consol.JK_MasterBillNum = "IHAVECHANGED";
			RunAndAssert("Existing event log not cancelled", consol, eventExpected: true);
		}

		public void TestIsApplicable_NoExistingLog_DehireEvent()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			RunAndAssert("Original event log created", consol, eventExpected: true);

			Factory.Save();

			var log = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested);
			log.Cancel();

			var dehire = consol.Logs.AddNew(Events.Dehire);
			using (dehire.LockForUpdatingKeyFieldsForTesting())
			{
				dehire.SL_IsEstimate = true;
			}
			RunAndAssert("Dehire event found, but it's estimate - applicable for update", consol, eventExpected: true);

			log = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested);
			log.Cancel();

			using (dehire.LockForUpdatingKeyFieldsForTesting())
			{
				dehire.SL_IsEstimate = false;
			}
			RunAndAssert("Dehire event found - not applicable for update", consol, eventExpected: false);

			dehire.Cancel();
			RunAndAssert("No dehire event found - applicable for update", consol, eventExpected: true);
		}

		public void TestIsApplicable_HasExistingLog_DehireEvent_WouldNotCancelExistingLog()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			RunAndAssert("Original event log created", consol, eventExpected: true);

			Factory.Save();

			consol.Logs.AddNew(Events.Dehire);
			RunAndAssert("Existing event log not cancelled", consol, eventExpected: true);

			consol.JK_MasterBillNum = "IHAVECHANGED";
			RunAndAssert("Existing event log not cancelled", consol, eventExpected: true);
		}

		public void TestHasSubscriptionDetailsChanged_BookingReference()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.Factory.Save();
			Assert("Nothing should change", consol, eventExpected: true);

			var originalEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;

			consol.JK_BookingReference = "1234567890";
			RunAndAssert("BookingReference has changed", consol, eventExpected: true);

			var updatedEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;
			Assert("Event log updated", updatedEventTime > originalEventTime);
		}

		public void TestHasSubscriptionDetailsChanged_MasterBillNum()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.Factory.Save();
			Assert("Expect event to be created.", consol, eventExpected: true);

			var originalEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;

			consol.JK_MasterBillNum = "1234567890";
			RunAndAssert("MasterBillNum has changed", consol, eventExpected: true);

			var updatedEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;
			Assert("Event log updated", updatedEventTime > originalEventTime);
		}

		public void TestHasSubscriptionDetailsChanged_CoLoadMasterBillNum()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.Factory.Save();
			Assert("Nothing should change", consol, eventExpected: true);

			var originalEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;

			consol.JK_CoLoadMasterBill = "1234567890";
			RunAndAssert("CoLoadMasterBillNum has changed", consol, eventExpected: true);

			var updatedEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;
			Assert("Event log updated", updatedEventTime > originalEventTime);
		}

		public void TestHasSubscriptionDetailsChanged_CoLoadBookingReference()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.Factory.Save();
			Assert("Nothing should change", consol, eventExpected: true);

			var originalEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;

			consol.JK_CoLoadBookingReference = "1234567890";
			RunAndAssert("CoLoadBookingReference has changed", consol, eventExpected: true);

			var updatedEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;
			Assert("Event log updated", updatedEventTime > originalEventTime);
		}

		public void TestHasSubscriptionDetailsChanged_ContainerNumber()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.Factory.Save();
			RunAndAssert("Expect event to be created", consol, eventExpected: true);

			var originalEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;

			consol.Containers[0].JC_ContainerNum = "NEWNUMBER";
			RunAndAssert("Any container number has changed", consol, eventExpected: true);

			var updatedEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;
			Assert("Event log updated", updatedEventTime > originalEventTime);
		}

		public void TestHasSubscriptionDetailsChanged_CoLoadCarrier()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.Factory.Save();
			RunAndAssert("Expect event to be created", consol, eventExpected: true);

			var originalEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;

			consol.JK_OA_CreditorAddress = ZGuid.NewZGuid();
			RunAndAssert("ColoadCarrier has changed", consol, eventExpected: true);

			var updatedEventTime = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested).SL_EventTime;
			Assert("Event log updated", updatedEventTime > originalEventTime);
		}

		public void TestGatewayCoload_CreatesSubscription()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var container = consol.Containers[0];
			var manager = new ContainerTrackingSubscriptionRequestedManager(consol);

			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				manager.UpdateIfNecessary();
			}

			var eventReference = ContainerTrackingSubscriptionRequestedManager.SubscriptionRequestedEventReference;

			var consolEvent = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
			AssertNotNull("Event on coload consol", consolEvent);

			var container1Event = container.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
			AssertNull("Gateway Coload consol - Event should NOT be created directly on container", container1Event);
		}

		public void TestSubscribeToContainersOnly_GatewayCoLoad_NoSBREventForCBROnly()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_MasterBillNum = "";

			RunAndAssert("Gateway Coload consol - Event should NOT be created when CBR number only", consol, eventExpected: false);
		}

		public void TestSubscribeToContainersOnly_Coload_NoSBREventForCBROnly()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_MasterBillNum = "";

			RunAndAssert("Coload consol - Event should NOT be created when CBR number only", consol, eventExpected: false);
		}

		public void TestCreateSBRWhenContainerChanges()
		{
			// Arrange
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("Prerequisite", true, (consol as IContainerTrackingProvider).SubscribeToContainersOnly);

			var container1 = consol.Containers[0];
			AssertEquals("Prerequisite", true, ContainerNumberValidation.IsValidContainerNumber(container1.JC_ContainerNum));

			var container2 = consol.Containers[1];
			AssertEquals("Prerequisite", true, ContainerNumberValidation.IsValidContainerNumber(container2.JC_ContainerNum));
			Factory.Save();

			AssertEquals(
				"Prerequisite: 1 event on consol",
				1,
				consol.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code));

			// Act
			container2.JC_ContainerNum = "AAAA0000007";
			Factory.Save();

			// Assert
			AssertEquals(
				"2 events on consol",
				2,
				consol.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code));
		}

		public void TestCreateSBR_WhenAutomaticContainerCreationRegistryIsNeverCreate()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();

			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("Prerequisite", false, (consol as IContainerTrackingProvider).SubscribeToContainersOnly);

			var container1 = consol.Containers[0];
			AssertEquals("Prerequisite", true, ContainerNumberValidation.IsValidContainerNumber(container1.JC_ContainerNum));

			var container2 = consol.Containers[1];
			AssertEquals("Prerequisite", true, ContainerNumberValidation.IsValidContainerNumber(container2.JC_ContainerNum));

			var automaticContainerCreation = new AutomaticContainerCreation();
			automaticContainerCreation.IsNeverCreate = true;

			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				RunAndAssert("Event on consol when AutomaticContainerCreation is true in registry", consol, eventExpected: true);
			}
		}

		public void TestCreateSBRWhenRoutingIsUpdated()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			var transport = consol.Transports.AddNew("DEFRA", "AUBNE");
			Factory.Save();
			AssertEventNumber("1 uncancelled SBR event on consol, 0 cancelled SBR event on consol", consol, 1, 0);

			transport.JW_RL_NKDiscPort = "AUMEL";
			Factory.Save();
			AssertEventNumber("1 uncancelled SBR event on consol, 1 cancelled SBR event on consol", consol, 1, 1);

			var factory = new BusinessObjectFactory();
			var consol2 = factory.Load<ForwardingConsol>(consol.PK);
			consol2.Transports.RemoveAll();
			factory.Save();
			AssertEventNumber("1 uncancelled SBR event on consol, 2 cancelled SBR events on consol", consol2, 1, 2);
		}

		public void TestCreateSBRWhenContainerISNotRecent()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			var transport = consol.Transports.AddNew("DEFRA", "AUBNE");
			transport.JW_ETA = ZDateTime.Today.AddMonths(-10);
			transport.JW_ETD = ZDateTime.Today.AddMonths(-9);

			Factory.Save();
			AssertEventNumber("1 uncancelled SBR event on consol, 0 cancelled SBR event on consol", consol, 1, 0);
		}

		public void TestCreateSBRWhenConsolTypeIsUpdated()
		{
			var consol = CreateApplicableForSubscriptionConsolWithTwoContainers();
			consol.JK_AgentType = "AGT";
			Factory.Save();
			AssertEventNumber("1 uncancelled SBR event on consol, 0 cancelled SBR event on consol", consol, 1, 0);

			consol.JK_AgentType = "DRT";
			Factory.Save();
			AssertEventNumber("1 uncancelled SBR event on consol, 1 cancelled SBR event on consol", consol, 1, 1);

			consol.JK_AgentType = "DRT";
			Factory.Save();
			AssertEventNumber("1 uncancelled SBR event on consol, 1 cancelled SBR event on consol", consol, 1, 1);

			consol.JK_AgentType = "CLD";
			Factory.Save();
			AssertEventNumber("1 uncancelled SBR event on consol, 1 cancelled SBR event on consol", consol, 1, 2);
		}

		#region Implementation

		void RunAndAssert(string message, ForwardingConsol consol, bool eventExpected)
		{
			Run(consol);
			Assert(message, consol, eventExpected);
		}

		static void Run(ForwardingConsol consol)
		{
			var manager = new ContainerTrackingSubscriptionRequestedManager(consol);
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				manager.UpdateIfNecessary();
			}
		}

		static void Assert(string message, ForwardingConsol consol, bool eventExpected)
		{
			CombineAssertions(message, () =>
			{
				var eventReference = ContainerTrackingSubscriptionRequestedManager.SubscriptionRequestedEventReference;

				var log = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertEquals("Event on consol", eventExpected, log != null);

				foreach (ForwardingContainer container in consol.Containers)
				{
					log = container.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
					AssertEquals("Event on container", false, log != null);
				}
			});
		}

		static void AssertEventNumber(string message, ForwardingConsol consol, int expectedUncancelledEventNumber, int expectedCancelledEventNumber)
		{
			AssertEquals(
				message,
				expectedUncancelledEventNumber,
				consol.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled));
			AssertEquals(
				message,
				expectedCancelledEventNumber,
				consol.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && x.IsCancelled));
		}

		ForwardingConsol CreateApplicableForSubscriptionConsolWithTwoContainers(string transportMode = Constants.TransportModes.Sea)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_BookingReference = "BOOKING1234";
			consol.JK_MasterBillNum = "MASTER1234";
			consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US").MainAddress.PK;

			consol.Transports[0].JW_ETA = ZDateTime.Today;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA1234566";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "AAAA2222220";

			Assert("Prerequisite", consol is IContainerTrackingProvider);
			Assert("Prerequisite", container1 is ITrackableContainer);

			return consol;
		}

		#endregion
	}
}
