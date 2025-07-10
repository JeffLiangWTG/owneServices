using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class BookingToTransportJobProcessorTest : DtbBookingTestCaseWithFactory
	{
		public void TestCreateDtbBookingQueueRecord_FromWorkflowAction_RegistryPort()
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.PortTransport;
			serviceTaskOption.IsSystemDefined = false;

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			{
				var recipientCode = MessageRecipientPartyTypeList.Codes.TransportJobRegistry;
				var expectedTargetModule = AutoCreatorTargetModules.Codes.PortTransport;
				AssertCreateDtbBookingQueueRecordFromWorkflowActionForRecipient(recipientCode, expectedTargetModule);
			}
		}

		public void TestCreateDtbBookingQueueRecord_FromWorkflowAction_RegistryLand()
		{
			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			serviceTaskOption.IsSystemDefined = false;

			using (TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions))
			{
				var recipientCode = MessageRecipientPartyTypeList.Codes.TransportJobRegistry;
				var expectedTargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
				AssertCreateDtbBookingQueueRecordFromWorkflowActionForRecipient(recipientCode, expectedTargetModule);
			}
		}

		public void AssertCreateDtbBookingQueueRecordFromWorkflowActionForRecipient(string triggerPartyType, string expectedTargetModule)
		{
			var dummyBooking = CreateDummyDtbBooking();

			Factory.Save();

			// Create a workflow trigger and action to generate a booking queue record, which will eventually create a Transport Job from the Transport Booking
			var dummyTrigger = CreateTrigger(dummyBooking, Constants.Workflow.WorkflowTriggerType, AutoEvents.ServiceRequestedCode);
			var dummyAction = CreateTriggerAction(dummyTrigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob, triggerPartyType);

			Factory.Save();

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;
				var logger = new NotificationBuffer();

				var query = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummyBooking.PK);
				var preProcessCheck = Factory.Load<DtbBookingQueue>(query);

				AssertEquals("Precondition: DtbBookingQueue record not created yet", 0, preProcessCheck.Length);

				IProcessor processor = Processor(dummyBooking, dummyAction);
				processor.Process(logger);

				var queueRecord = Factory.Load<DtbBookingQueue>(query).FirstOrDefault();

				AssertQueueRecordMatchesExpectedValues(
					queueRecord,
					dummyBooking.TablePrefix,
					Env.CurrentBranch.Code,
					0,
					expectedTargetModule,
					"Processor should have created TB queue record for booking",
					"TB Queue record details should match expected");
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		public void TestThreeTriggersCreateThreeDtbBookingQueueRecords_FromWorkflowAction()
		{
			var dummy = CreateDummyDtbBooking();
			var dummy2 = CreateDummyDtbBooking();
			var dummy3 = CreateDummyDtbBooking();
			Factory.Save();

			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.LandTransportConsignment;
			serviceTaskOption.IsSystemDefined = false;

			using var registryValue = TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions);

			var dummy1Trigger = CreateTrigger(dummy, Constants.Workflow.WorkflowTriggerType, AutoEvents.ServiceRequestedCode);
			var dummy1Action = CreateTriggerAction(dummy1Trigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob, MessageRecipientPartyTypeList.Codes.TransportJobRegistry);

			var dummy2Trigger = CreateTrigger(dummy2, Constants.Workflow.WorkflowTriggerType, AutoEvents.ServiceCommencedCode);
			var dummy2Action = CreateTriggerAction(dummy2Trigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob, MessageRecipientPartyTypeList.Codes.TransportJobRegistry);

			var dummy3Trigger = CreateTrigger(dummy3, Constants.Workflow.WorkflowTriggerType, AutoEvents.ServiceCommencedCode);
			var dummy3Action = CreateTriggerAction(dummy3Trigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob, MessageRecipientPartyTypeList.Codes.TransportJobRegistry);
			Factory.Save();

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;
				var logger = new NotificationBuffer();

				var queryAll = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, new ZGuid[] { dummy.PK, dummy2.PK, dummy3.PK });
				var preDtbBookingQueueCheck = Factory.Load<DtbBookingQueue>(queryAll);
				AssertEquals("Precondition: no DtbBookingQueue records exist with parents dummy/dummy2/dummy3", 0, preDtbBookingQueueCheck.Length);

				var processor1 = (IProcessor)Processor(dummy, dummy1Action);
				processor1.Process(logger);
				var queryDummy1Only = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummy.PK);
				var dummy1TBQueueRecord = Factory.Load<DtbBookingQueue>(queryDummy1Only).FirstOrDefault();
				AssertQueueRecordMatchesExpectedValues(
					dummy1TBQueueRecord,
					dummy.TablePrefix,
					Env.CurrentBranch.Code,
					0,
					AutoCreatorTargetModules.Codes.LandTransportConsignment,
					"Processor1 should have created DtbBookingQueue record with dummy as parent",
					"Details for TB Queue record created by Processor1 with dummy as parent should match expected");

				var processor2 = (IProcessor)Processor(dummy2, dummy2Action);
				processor2.Process(logger);
				var queryDummy2Only = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummy2.PK);
				var dummy2TBQueueRecord = Factory.Load<DtbBookingQueue>(queryDummy2Only).FirstOrDefault();
				AssertQueueRecordMatchesExpectedValues(
					dummy2TBQueueRecord,
					dummy2.TablePrefix,
					Env.CurrentBranch.Code,
					0,
					AutoCreatorTargetModules.Codes.LandTransportConsignment,
					"Processor2 should have created DtbBookingQueue record with dummy2 as parent",
					"Details for TB Queue record created by Processor2 with dummy2 as parent should match expected");

				var processor3 = (IProcessor)Processor(dummy3, dummy3Action);
				processor3.Process(logger);
				var queryDummy3Only = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummy3.PK);
				var dummy3TBQueueRecord = Factory.Load<DtbBookingQueue>(queryDummy3Only).FirstOrDefault();
				AssertQueueRecordMatchesExpectedValues(
					dummy3TBQueueRecord,
					dummy3.TablePrefix,
					Env.CurrentBranch.Code,
					0,
					AutoCreatorTargetModules.Codes.LandTransportConsignment,
					"Processor3 should have created DtbBookingQueue record with dummy3 as parent",
					"Details for TB Queue record created by Processor3 with dummy3 as parent should match expected");
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
				registryValue.Dispose();
			}
		}

		public void TestLogsException()
		{
			var dummyBooking = CreateDummyDtbBooking();

			Factory.Save();

			var triggerPartyType = MessageRecipientPartyTypeList.Codes.TransportJobRegistry;

			// Create a workflow trigger and action to generate a booking queue record, which will eventually create a Transport Job from the Transport Booking
			var dummyTrigger = CreateTrigger(dummyBooking, Constants.Workflow.WorkflowTriggerType, AutoEvents.ServiceRequestedCode);
			var dummyAction = CreateTriggerAction(dummyTrigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob, triggerPartyType);

			Factory.Save();

			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.PortTransport;
			serviceTaskOption.IsSystemDefined = false;

			using var registryValue = TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions);

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				var problemFactory = new NewThrowsExceptionBusinessObjectFactory();
				var dummyParentProblemFactory = problemFactory.Load<DtbBooking>(dummyBooking.PK);

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
				registryValue.Dispose();
			}
		}

		public void TestDoesNotCreateDtbBookingQueueRecordIfParentInactive()
		{
			var dummyBooking = CreateDummyDtbBooking();
			dummyBooking.IsCancelled = true;

			Factory.Save();

			var triggerPartyType = MessageRecipientPartyTypeList.Codes.TransportJobRegistry;

			// Create a workflow trigger and action to generate a booking queue record with CombineContainers = false, will eventually create a separate Transport Booking for each of the 2 containers specified in the dummy parent.
			var dummyTrigger = CreateTrigger(dummyBooking, Constants.Workflow.WorkflowTriggerType, AutoEvents.ServiceRequestedCode);
			var dummyAction = CreateTriggerAction(dummyTrigger, WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob, triggerPartyType);

			Factory.Save();

			var serviceTaskOptions = new ServiceTaskCreatorOptionCollection();
			var serviceTaskOption = serviceTaskOptions.AddNew();
			serviceTaskOption.ContainerMode = AutoCreatorContainerModes.Codes.Loose;
			serviceTaskOption.TargetModule = AutoCreatorTargetModules.Codes.PortTransport;
			serviceTaskOption.IsSystemDefined = false;

			using var registryValue = TransportRegistry.Instance.ServiceTaskCreatorOption.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceTaskOptions);

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;
				var logger = new NotificationBuffer();

				var query = new ZQuery(DtbBookingQueueSchema.KMQ_ParentID, dummyBooking.PK);
				var preProcessCheck = Factory.Load<DtbBookingQueue>(query);

				AssertEquals("Precondition: DtbBookingQueue record not created yet", 0, preProcessCheck.Length);

				IProcessor processor = Processor(dummyBooking, dummyAction);
				processor.Process(logger);

				var postProcessCheck = Factory.Load<DtbBookingQueue>(query);

				AssertEquals("Should not create DtbBookingQueue record if the parent is cancelled", 0, postProcessCheck.Length);

				var message = logger.GetEventsByType(CargoWise.ComponentModel.NotificationType.Warning).Single().Message;
				AssertEquals("Should log warning message", $"Transport Booking {dummyBooking.KM_JobID} is deactivated and cannot create new Transport Jobs.".Trim(), message.Trim());
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
				registryValue.Dispose();
			}
		}

		DtbBooking CreateDummyDtbBooking()
		{
			var dummy = Factory.NewWithValidTestData<DtbBooking>();
			return dummy;
		}

		BookingToTransportJobProcessor Processor(DtbBooking parent, ProcessTaskNotification action)
		{
			return new BookingToTransportJobProcessor(parent, action);
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

		void AssertQueueRecordMatchesExpectedValues(DtbBookingQueue queueRecord, string expectedParentTableCode, string expectedBranchCode, short expectedIteration, string expectedTargetModule, string recordExistsAssertionMessage, string recordMatchesAssertionMessage)
		{
			AssertNotNull(recordExistsAssertionMessage, queueRecord);

			CombineAssertions(recordMatchesAssertionMessage,
				() =>
				{
					AssertEquals(expectedParentTableCode, queueRecord.KMQ_ParentTableCode);
					AssertEquals(expectedBranchCode, queueRecord.KMQ_GB_NKBranch);
					AssertEquals(expectedIteration, queueRecord.Iteration);
					AssertEquals(expectedTargetModule, queueRecord.KMQ_TargetModule);
				});
		}
	}
}
