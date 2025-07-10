using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Agency;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingProcessorTest : DtbBookingTestCaseWithFactory
	{
		public void TestCreateDtbBookingQueueRecordDoNotCombineContainers_FromWorkflowAction_Pickup()
		{
			AssertCreateDtbBookingQueueRecordDoNotCombineContainersFromWorkflowActionForDirection(isPickup: true);
		}

		public void TestCreateDtbBookingQueueRecordDoNotCombineContainers_FromWorkflowAction_Delivery()
		{
			AssertCreateDtbBookingQueueRecordDoNotCombineContainersFromWorkflowActionForDirection(isPickup: false);
		}

		public void AssertCreateDtbBookingQueueRecordDoNotCombineContainersFromWorkflowActionForDirection(bool isPickup)
		{
			var dummyParent = CreateDummyIDtbBookingParent("DUM456");

			Factory.Save();

			var triggerPartyType = isPickup ? MessageRecipientPartyTypeList.Codes.PickupCartage : MessageRecipientPartyTypeList.Codes.DeliveryCartage;
			var expectedDirection = isPickup ? "PIC" : "DLV";

			// Create a workflow trigger and action to generate a booking queue record with CombineContainers = false, will eventually create a separate Transport Booking for each of the 2 containers specified in the dummy parent.
			var dummyTrigger = CreateTrigger(dummyParent, Constants.Workflow.WorkflowTriggerType, Events.ServiceRequestedCode);
			var dummyAction = CreateTriggerAction(dummyTrigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer, triggerPartyType);

			Factory.Save();

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;
				var logger = new NotificationBuffer();

				var query = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummyParent.PK);
				var preProcessCheck = Factory.Load<DtbBookingQueue>(query);

				AssertEquals("Precondition: DtbBookingQueue record not created yet", 0, preProcessCheck.Length);

				IProcessor processor = Processor(dummyParent, dummyAction);
				processor.Process(logger);

				var queueRecord = Factory.Load<DtbBookingQueue>(query).FirstOrDefault();

				AssertQueueRecordMatchesExpectedValues(
					queueRecord,
					dummyParent.TablePrefix,
					expectedDirection,
					Env.CurrentBranch.Code,
					false,
					0,
					"Processor should have created TB queue record for dummyParent",
					"TB Queue record details should match expected");
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		public void TestCreateDtbBookingQueueRecordWithCombineContainers_FromWorkflowAction_Pickup()
		{
			AssertCreateDtbBookingQueueRecordWithCombineContainersFromWorkflowActionForDirection(isPickup: true);
		}

		public void TestCreateDtbBookingQueueRecordWithCombineContainers_FromWorkflowAction_Delivery()
		{
			AssertCreateDtbBookingQueueRecordWithCombineContainersFromWorkflowActionForDirection(isPickup: false);
		}

		public void AssertCreateDtbBookingQueueRecordWithCombineContainersFromWorkflowActionForDirection(bool isPickup)
		{
			var dummyParent = CreateDummyIDtbBookingParent("DUM456");
			Factory.Save();

			var triggerPartyType = isPickup ? MessageRecipientPartyTypeList.Codes.PickupCartage : MessageRecipientPartyTypeList.Codes.DeliveryCartage;
			var expectedDirection = isPickup ? "PIC" : "DLV";

			// Create a workflow trigger and action to generate a DtbBookingQueue record with CombineContainers = true, that will eventually become a single booking containing both of the 2 containers specified in the dummy parent.
			var dummy1Trigger = CreateTrigger(dummyParent, Constants.Workflow.WorkflowTriggerType, Events.ServiceRequestedCode);
			var dummy1Action = CreateTriggerAction(dummy1Trigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking, triggerPartyType);

			Factory.Save();

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;
				var logger = new NotificationBuffer();

				var query = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummyParent.PK);
				var preProcessCheck = Factory.Load<DtbBookingQueue>(query);

				AssertEquals("Precondition: DtbBookingQueue record not created yet", 0, preProcessCheck.Length);

				IProcessor processor1 = Processor(dummyParent, dummy1Action);
				processor1.Process(logger);

				var queueRecord = Factory.Load<DtbBookingQueue>(query).FirstOrDefault();

				AssertQueueRecordMatchesExpectedValues(
					queueRecord,
					dummyParent.TablePrefix,
					expectedDirection,
					Env.CurrentBranch.Code,
					true,
					0,
					"Processor should have created TB queue record for dummyParent",
					"TB Queue record details should match expected");
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		public void TestTwoTriggersCreateTwoDtbBookingQueueRecords_FromWorkflowAction()
		{
			var dummy = CreateDummyIDtbBookingParent("DUM456");
			var dummy2 = CreateDummyIDtbBookingParent("DUM123");
			Factory.Save();

			var dummy1Trigger = CreateTrigger(dummy, Constants.Workflow.WorkflowTriggerType, Events.ServiceRequestedCode);
			var dummy1Action = CreateTriggerAction(dummy1Trigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking, MessageRecipientPartyTypeList.Codes.DeliveryCartage);

			var dummy2Trigger = CreateTrigger(dummy2, Constants.Workflow.WorkflowTriggerType, Events.ServiceCommencedCode);
			var dummy2Action = CreateTriggerAction(dummy2Trigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking, MessageRecipientPartyTypeList.Codes.PickupCartage);
			Factory.Save();

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;
				var logger = new NotificationBuffer();

				var queryAll = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, new ZGuid[] { dummy.PK, dummy2.PK });
				var preDtbBookingQueueCheck = Factory.Load<DtbBookingQueue>(queryAll);
				AssertEquals("Precondition: no DtbBookingQueue records exist with parents dummy/dummy2", 0, preDtbBookingQueueCheck.Length);

				var processor1 = (IProcessor)Processor(dummy, dummy1Action);
				processor1.Process(logger);
				var queryDummy1Only = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummy.PK);
				var dummy1TBQueueRecord = Factory.Load<DtbBookingQueue>(queryDummy1Only).FirstOrDefault();
				AssertQueueRecordMatchesExpectedValues(
					dummy1TBQueueRecord,
					dummy.TablePrefix,
					"DLV",
					Env.CurrentBranch.Code,
					true,
					0,
					"Processor1 should have created DtbBookingQueue record with dummy as parent",
					"Details for TB Queue record created by Processor1 with dummy as parent should match expected");

				var processor2 = (IProcessor)Processor(dummy2, dummy2Action);
				processor2.Process(logger);
				var queryDummy2Only = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummy2.PK);
				var dummy2TBQueueRecord = Factory.Load<DtbBookingQueue>(queryDummy2Only).FirstOrDefault();
				AssertQueueRecordMatchesExpectedValues(
					dummy2TBQueueRecord,
					dummy2.TablePrefix,
					"PIC",
					Env.CurrentBranch.Code,
					true,
					0,
					"Processor2 should have created DtbBookingQueue record with dummy2 as parent",
					"Details for TB Queue record created by Processor2 with dummy2 as parent should match expected");
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		public void TestCreateDtbBookingQueueRecordFromForwardingShipmentHasCorrectParentTableCodeValue()
		{
			CreateDtbBookingQueueRecordFromSpecifiedParentAndCheckRecordValuesCore(CreateForwardingShipmentParent, "Forwarding Shipment", MessageRecipientPartyTypeList.Codes.PickupCartage, WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer, "JS", "PIC", false);
		}

		public void TestCreateDtbBookingQueueRecordFromAgencyShipmentHasCorrectParentTableCodeValue()
		{
			CreateDtbBookingQueueRecordFromSpecifiedParentAndCheckRecordValuesCore(CreateAgencyShipmentParent, "Agency Shipment", MessageRecipientPartyTypeList.Codes.PickupCartage, WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer, "JS", "PIC", false);
		}

		public void TestCreateDtbBookingQueueRecordFromBillOfLadingHasCorrectParentTableCodeValue()
		{
			CreateDtbBookingQueueRecordFromSpecifiedParentAndCheckRecordValuesCore(CreateBillOfLadingParent, "Bill of Lading", MessageRecipientPartyTypeList.Codes.PickupCartage, WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer, "JS", "PIC", false);
		}

		public void CreateDtbBookingQueueRecordFromSpecifiedParentAndCheckRecordValuesCore(Func<BusinessObject> parentCreator, string parentTypeDescription, string triggerParty, string triggerType, string expectedParentTableCode, string expectedDirection, bool expectedCombineContainers)
		{
			var parent = parentCreator();
			Factory.Save();

			var triggerPartyType = triggerParty;

			// Create a workflow trigger and action to generate a DtbBookingQueue record with CombineContainers = true, that will eventually become a single booking containing both of the 2 containers specified in the dummy parent.
			var trigger = CreateTrigger((IWorkflowProvider)parent, Constants.Workflow.WorkflowTriggerType, Events.ServiceRequestedCode);
			var action = CreateTriggerAction(trigger, triggerType, triggerPartyType);

			Factory.Save();

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;
				var logger = new NotificationBuffer();

				var query = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, parent.PK);
				var preProcessCheck = Factory.Load<DtbBookingQueue>(query);

				AssertEquals("Precondition: DtbBookingQueue record not created yet", 0, preProcessCheck.Length);

				IProcessor processor1 = Processor((IDtbBookingParent)parent, action);
				processor1.Process(logger);
				Factory.Save();

				var queueRecord = Factory.Load<DtbBookingQueue>(query).FirstOrDefault();

				AssertQueueRecordMatchesExpectedValues(
					queueRecord,
					expectedParentTableCode,
					expectedDirection,
					Env.CurrentBranch.Code,
					expectedCombineContainers,
					0,
					"Processor should have created TB queue record for parent " + parentTypeDescription,
					"TB Queue record details should match expected");
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		public void TestLogsException()
		{
			var dummyParent = CreateDummyIDtbBookingParent("DUM456");

			Factory.Save();

			var triggerPartyType = MessageRecipientPartyTypeList.Codes.PickupCartage;

			// Create a workflow trigger and action to generate a booking queue record with CombineContainers = false, will eventually create a separate Transport Booking for each of the 2 containers specified in the dummy parent.
			var dummyTrigger = CreateTrigger(dummyParent, Constants.Workflow.WorkflowTriggerType, Events.ServiceRequestedCode);
			var dummyAction = CreateTriggerAction(dummyTrigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer, triggerPartyType);

			Factory.Save();

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				var problemFactory = new NewThrowsExceptionBusinessObjectFactory();
				var dummyParentProblemFactory = problemFactory.Load<DummyWithDtbBooking>(dummyParent.PK);

				Globals.IsUserInteractive = false;
				var logger = new NotificationBuffer();

				IProcessor processor = Processor(dummyParentProblemFactory, dummyAction);
				AssertExceptionThrown("Should not swallow exception", typeof(NotImplementedException), "No new objects allowed!", () =>
				{
					processor.Process(logger);
				});

				var message = logger.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Single().Message;
				AssertEquals("Should log message", "No new objects allowed!", message);
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		public void TestDoesNotCreateDtbBookingQueueRecordIfParentInactive()
		{
			var dummyParent = CreateDummyIDtbBookingParent("DUM456");
			dummyParent.IsCancelled = true;

			Factory.Save();

			var triggerPartyType = MessageRecipientPartyTypeList.Codes.PickupCartage;

			// Create a workflow trigger and action to generate a booking queue record with CombineContainers = false, will eventually create a separate Transport Booking for each of the 2 containers specified in the dummy parent.
			var dummyTrigger = CreateTrigger(dummyParent, Constants.Workflow.WorkflowTriggerType, Events.ServiceRequestedCode);
			var dummyAction = CreateTriggerAction(dummyTrigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer, triggerPartyType);

			Factory.Save();

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;
				var logger = new NotificationBuffer();

				var query = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummyParent.PK);
				var preProcessCheck = Factory.Load<DtbBookingQueue>(query);

				AssertEquals("Precondition: DtbBookingQueue record not created yet", 0, preProcessCheck.Length);

				IProcessor processor = Processor(dummyParent, dummyAction);
				processor.Process(logger);

				var postProcessCheck = Factory.Load<DtbBookingQueue>(query);

				AssertEquals("Should not create DtbBookingQueue record if the parent is cancelled", 0, postProcessCheck.Length);

				var message = logger.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Single().Message;
				AssertEquals("Should log warning message", "Dummy Business Object DUM456 is deactivated and cannot create new Transport Bookings.", message);
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		DummyWithDtbBooking CreateDummyIDtbBookingParent(string description)
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = description;
			return dummy;
		}

		DtbBookingProcessor Processor(IDtbBookingParent parent, ProcessTaskNotification action)
		{
			return new DtbBookingProcessor(parent, action);
		}

		static ProcessTask CreateTrigger(IWorkflowProvider parent, string workflowTriggerType, string eventCode)
		{
			var trigger = parent.WorkflowItems.AddNew();
			trigger.P9_Type = workflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = eventCode;

			return trigger;
		}

		static ProcessTaskNotification CreateTriggerAction(ProcessTask trigger, string triggerType, string triggerParty)
		{
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = triggerType;
			action.PQ_TriggerParty = triggerParty;

			return action;
		}

		void AssertQueueRecordMatchesExpectedValues(DtbBookingQueue queueRecord, string expectedParentTableCode, string expectedDirection, string expectedBranchCode, bool expectedCombineContainers, short expectedIteration, string recordExistsAssertionMessage, string recordMatchesAssertionMessage)
		{
			AssertNotNull(recordExistsAssertionMessage, queueRecord);

			CombineAssertions(recordMatchesAssertionMessage,
				() =>
				{
					AssertEquals(expectedParentTableCode, queueRecord.KMQ_ParentTableCode);
					AssertEquals(expectedDirection, queueRecord.KMQ_Direction);
					AssertEquals(expectedBranchCode, queueRecord.KMQ_GB_NKBranch);
					AssertEquals(expectedCombineContainers, queueRecord.KMQ_CombineContainers);
					AssertEquals(expectedIteration, queueRecord.Iteration);
				});
		}

		BusinessObject CreateForwardingShipmentParent()
		{
			var forwardingShipment = Factory.New<IForwardingShipment>();
			var forwardingShipmentAsBusinessObject = ((BusinessObject)forwardingShipment);
			forwardingShipmentAsBusinessObject.FillWithValidTestData();
			forwardingShipmentAsBusinessObject[JobShipmentSchema.JS_UniqueConsignRef] = "VX999999";
			Factory.Save();
			return forwardingShipmentAsBusinessObject;
		}

		BusinessObject CreateAgencyShipmentParent()
		{
			var agencyShipment = Factory.New<IAgencyShipment>();
			var agencyShipmentAsBusinessObject = ((BusinessObject)agencyShipment);
			agencyShipmentAsBusinessObject.FillWithValidTestData();
			agencyShipmentAsBusinessObject[JobShipmentSchema.JS_UniqueConsignRef] = "VX999999";
			Factory.Save();
			return agencyShipmentAsBusinessObject;
		}

		BusinessObject CreateBillOfLadingParent()
		{
			var billOfLading = Factory.New<IBillOfLading>();
			var billOfLadingAsBusinessObject = ((BusinessObject)billOfLading);
			billOfLadingAsBusinessObject.FillWithValidTestData();
			billOfLadingAsBusinessObject[JobShipmentSchema.JS_UniqueConsignRef] = "BOL99999";
			Factory.Save();
			return billOfLadingAsBusinessObject;
		}
	}

	class NewThrowsExceptionBusinessObjectFactory : BusinessObjectFactory
	{
		public override BusinessObject New(Type bizOType)
		{
			throw new NotImplementedException("No new objects allowed!");
		}
	}
}
