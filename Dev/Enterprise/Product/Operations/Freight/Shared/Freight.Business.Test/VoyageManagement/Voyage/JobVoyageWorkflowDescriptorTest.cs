using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobVoyageWorkflowDescriptor))]
	sealed class JobVoyageWorkflowDescriptorTest : WorkflowDescriptorTestCase<JobVoyageWorkflowDescriptor>
	{
		#region WorkflowDescriptor Tests

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.SailingScheduleWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Sailing Schedule", WorkflowDescriptor.Description);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSubTypes()
		{
			var allSubTypes = WorkflowDescriptor.SubTypeInformation.SelectMany(subType => subType.List.Cast<CodeDescriptionPair>());

			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, "AIR", "SEA", "ROA", "RAI" },
				allSubTypes.Select(codeDescPair => codeDescPair.Code));
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.ArrivalCTO |
					MessageRecipientPartyType.DepartureCTO |
					MessageRecipientPartyType.Carrier |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email;
			}
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					JobVoyOriginSchema.JA_E_DEP,
					JobVoyOriginSchema.JA_CutOff,
					JobVoyOriginSchema.JA_ReceivalCommences,
					JobVoyOriginSchema.JA_DGCutOff,
					JobVoyOriginSchema.JA_DGReceivalCommences,
					JobVoyDestinationSchema.JB_E_ARV
				};
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new[] { VoyageWithConfiguredOrganisationParties };
		}

		#endregion

		#region Arrival / Departure CTOs triggered by event

		public void TestTriggerActionToRelevantDepartureCTO_TriggeredByEvent()
		{
			using (Factory.AddDisposableService())
			{
				Action createDepartureEvent = () =>
				{
					VoyageWithConfiguredOrganisationParties.Logs.AddNew(
						Events.Departure,
						new ZDateTimeOffset(2012, 7, 25),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, Origin2.JA_RL_NKPortOfLoading));
				};

				SendUniversalEvent(MessageRecipientPartyTypeList.Codes.DepartureCTO,
					Events.Departure,
					createDepartureEvent);

				var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

				AssertContainsExactElementsInAnyOrder("triggering event contains Origin 2 location",
					new[] { DepartureCTO_Origin2.OH_Code },
					messages.Select(message => message.Interchange.EI_To));
			}
		}

		public void TestTriggerActionToAllDepartureCTO_TriggeredByEvent()
		{
			using (Factory.AddDisposableService())
			{
				Action createDepartureEvent = () =>
				{
					VoyageWithConfiguredOrganisationParties.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2012, 7, 25));
				};

				SendUniversalEvent(MessageRecipientPartyTypeList.Codes.DepartureCTO,
					Events.Departure,
					createDepartureEvent);

				var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

				AssertContainsExactElementsInAnyOrder("triggering event contains none of the origins location",
					new[] { DepartureCTO_Origin1.OH_Code, DepartureCTO_Origin2.OH_Code },
					messages.Select(message => message.Interchange.EI_To));
			}
		}

		public void TestTriggerActionToRelevantArrivalCTO_TriggeredByEvent()
		{
			using (Factory.AddDisposableService())
			{
				Action createArrivalEvent = () =>
				{
					VoyageWithConfiguredOrganisationParties.Logs.AddNew(
						Events.Arrival,
						new ZDateTimeOffset(2012, 7, 25),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, Destination1.JB_RL_NKPortOfDischarge));
				};

				SendUniversalEvent(MessageRecipientPartyTypeList.Codes.ArrivalCTO,
					Events.Arrival,
					createArrivalEvent);

				var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

				AssertContainsExactElementsInAnyOrder("triggering event contains Destination 1 location",
					new[] { ArrivalCTO_Destination1.OH_Code },
					messages.Select(message => message.Interchange.EI_To));
			}
		}

		public void TestTriggerActionToAllArrivalCTO_TriggeredByEvent()
		{
			using (Factory.AddDisposableService())
			{
				Action createArrivalEvent = () =>
				{
					VoyageWithConfiguredOrganisationParties.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2012, 7, 25));
				};

				SendUniversalEvent(MessageRecipientPartyTypeList.Codes.ArrivalCTO,
					Events.Arrival,
					createArrivalEvent);

				var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

				AssertContainsExactElementsInAnyOrder("triggering event contains none of the destinations location",
					new[] { ArrivalCTO_Destination1.OH_Code, ArrivalCTO_Destination2.OH_Code },
					messages.Select(message => message.Interchange.EI_To));
			}
		}

		public void TestTriggerActionToRelevantDepartureCTO_OriginWithNoCTO_TriggeredByEvent()
		{
			using (Factory.AddDisposableService())
			{
				Origin2.JA_OA_DepartureCTOAddress = ZGuid.Empty;

				Action createDepartureEvent = () =>
				{
					VoyageWithConfiguredOrganisationParties.Logs.AddNew(Events.Departure, Origin2.JA_RL_NKPortOfLoading, new ZDateTimeOffset(2012, 7, 25));
				};

				SendUniversalEvent(MessageRecipientPartyTypeList.Codes.DepartureCTO,
					Events.Departure,
					createDepartureEvent);

				var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

				AssertContainsExactElementsInAnyOrder("no messages sent as related origin has no departure CTO set",
					Array.Empty<EDIMessage>(), messages);
			}
		}

		public void TestTriggerActionToRelevantArrivalCTO_DestinationWithNoCTO_TriggeredByEvent()
		{
			using (Factory.AddDisposableService())
			{
				Destination1.JB_OA_ArrivalCTOAddress = ZGuid.Empty;

				Action createArrivalEvent = () =>
				{
					VoyageWithConfiguredOrganisationParties.Logs.AddNew(Events.Arrival, Destination1.JB_RL_NKPortOfDischarge, new ZDateTimeOffset(2012, 7, 25));
				};

				SendUniversalEvent(MessageRecipientPartyTypeList.Codes.ArrivalCTO,
					Events.Arrival,
					createArrivalEvent);

				var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

				AssertContainsExactElementsInAnyOrder("no messages sent as related destination has no arrival CTO set",
					Array.Empty<EDIMessage>(), messages);
			}
		}

		#endregion

		#region Arrival / Departure CTOs triggered by change log

		[TestDate(2012, 1, 1)]
		public void TestTriggerActionToRelevantDepartureCTO_TriggeredByChangeLog()
		{
			Action setDepartureDate = () =>
			{
				Origin2.JA_E_DEP = new ZDateTime(2012, 7, 25);
			};

			SendUniversalSchedule(MessageRecipientPartyTypeList.Codes.DepartureCTO,
				JobVoyOriginSchema.JA_E_DEP,
				setDepartureDate);

			var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

			AssertContainsExactElementsInAnyOrder("triggering event contains change log with origin 2 location",
				new[] { DepartureCTO_Origin2.OH_Code },
				messages.Select(message => message.Interchange.EI_To));
		}

		[TestDate(2012, 1, 1)]
		public void TestTriggerActionToRelevantDepartureCTO_TriggeredByMultipleChangeLog()
		{
			Action setDepartureDate = () =>
			{
				Origin1.JA_E_DEP = new ZDateTime(2012, 7, 20);
				Origin2.JA_E_DEP = new ZDateTime(2012, 7, 25);
			};

			SendUniversalSchedule(MessageRecipientPartyTypeList.Codes.DepartureCTO,
				JobVoyOriginSchema.JA_E_DEP,
				setDepartureDate);

			var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

			AssertContainsExactElementsInAnyOrder("triggering event contains 2 change logs with both origins locations",
				new[] { DepartureCTO_Origin1.OH_Code, DepartureCTO_Origin2.OH_Code },
				messages.Select(message => message.Interchange.EI_To));
		}

		[TestDate(2012, 1, 1)]
		public void TestTriggerActionToRelevantArrivalCTO_TriggeredByChangeLog()
		{
			Action setArrivalDate = () =>
			{
				Destination2.JB_E_ARV = new ZDateTime(2012, 7, 25);
			};

			SendUniversalSchedule(MessageRecipientPartyTypeList.Codes.ArrivalCTO,
				JobVoyDestinationSchema.JB_E_ARV,
				setArrivalDate);

			var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

			AssertContainsExactElementsInAnyOrder("triggering event contains change log with departure 2 location",
				new[] { ArrivalCTO_Destination2.OH_Code },
				messages.Select(message => message.Interchange.EI_To));
		}

		[TestDate(2012, 1, 1)]
		public void TestTriggerActionToRelevantArrivalCTO_TriggeredByMiltipleChangeLog()
		{
			Action setArrivalDate = () =>
			{
				Destination1.JB_E_ARV = new ZDateTime(2012, 7, 20);
				Destination2.JB_E_ARV = new ZDateTime(2012, 7, 25);
			};

			SendUniversalSchedule(MessageRecipientPartyTypeList.Codes.ArrivalCTO,
				JobVoyDestinationSchema.JB_E_ARV,
				setArrivalDate);

			var messages = GetMessages(VoyageWithConfiguredOrganisationParties);

			AssertContainsExactElementsInAnyOrder("triggering event contains 2 change logs with both departures locations",
				new[] { ArrivalCTO_Destination1.OH_Code, ArrivalCTO_Destination2.OH_Code },
				messages.Select(message => message.Interchange.EI_To));
		}

		public void TestGetWorkflowTriggerAction_EmailAddressSupportsMacro()
		{
			// Arrange
			// Prepare users.
			var user1 = Factory.NewWithValidTestData<GlbStaff>();
			user1.GS_Code = "US1";
			user1.GS_EmailAddress = "user1@test.com";
			var user2 = Factory.NewWithValidTestData<GlbStaff>();
			user2.GS_Code = "US2";
			user2.GS_EmailAddress = "user2@test.com";
			// Prepare the Business Object.
			var job = (IWorkflowProvider)Factory.New<JobVoyage>();
			// Prepare logs.
			var logForUser1 = Factory.New<StmALog>();
			using (logForUser1.LockForUpdatingKeyFieldsForTesting())
			{
				logForUser1.SL_Parent = job.PK;
				logForUser1.SL_SE_NKEvent = AutoEvents.IncidentClosedCode;
				logForUser1.SL_Reference = "Confirmed User1!";
				logForUser1.SL_GS_NKUser = user1.GS_Code;
				logForUser1.SL_EventTime = ZDateTime.UtcNow;
			}
			var logForUser2 = Factory.New<StmALog>();
			using (logForUser2.LockForUpdatingKeyFieldsForTesting())
			{
				logForUser2.SL_Parent = job.PK;
				logForUser2.SL_SE_NKEvent = AutoEvents.IncidentClosedCode;
				logForUser2.SL_Reference = "Confirmed User2!";
				logForUser2.SL_GS_NKUser = user2.GS_Code;
				logForUser2.SL_EventTime = ZDateTime.UtcNow.AddSeconds(1);
			}
			// Prepare the notification.
			var trigger = job.WorkflowItems.Triggers.AddNew();
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "(*GetUserEmailWhoRaisedEvent(\"ICL\",\"Confirmed*\")*)";
			// Act
			var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)) as WorkflowTriggerNotification;
			// Assert
			AssertEquals(user2.GS_EmailAddress, notificationAction.Modes.CommunicationModes[0].EK_Destination);
		}

		public void TestGetWorkflowTriggerAction_NoExceptionThrown()
		{
			var job = (IWorkflowProvider)Factory.New<JobVoyage>();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "(*GetUserEmailWhoRaisedEvent(\"ICL\",\"Confirmed*\")*)";

			AssertNoExceptionThrown(() => WorkflowDescriptor.GetWorkflowTriggerAction(notification, null));
		}

		#endregion

		#region Implementation

		OrgHeader DepartureCTO_Origin1
		{
			get { return DepartureCTOOrg; }
		}

		OrgHeader DepartureCTO_Origin2
		{
			get { return departureCTOOrg2 ?? (departureCTOOrg2 = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader departureCTOOrg2;

		OrgHeader ArrivalCTO_Destination1
		{
			get { return ArrivalCTOOrg; }
		}

		OrgHeader ArrivalCTO_Destination2
		{
			get { return arrivalCTOOrg2 ?? (arrivalCTOOrg2 = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader arrivalCTOOrg2;

		IProcessor WorkflowServiceTaskTester
		{
			get { return workflowServiceTaskTester ?? (workflowServiceTaskTester = ObjectFactory.Get<IProcessorTest>("WorkflowServiceTaskTester")); }
		}

		IProcessor workflowServiceTaskTester;

		void SetupOrgCommunicationMode(OrgHeader orgHeader, ZString universalData)
		{
			var communicationsMode = orgHeader.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = WorkflowDescriptor.Code;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_FileFormat = universalData;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = orgHeader.OH_Code;

			Factory.Save();
		}

		JobVoyage VoyageWithConfiguredOrganisationParties
		{
			get { return voyage ?? (voyage = CreateVoyage()); }
		}

		JobVoyage voyage;

		VoyageOrigin Origin1
		{
			get { return VoyageWithConfiguredOrganisationParties.Origins[0]; }
		}

		VoyageOrigin Origin2
		{
			get { return VoyageWithConfiguredOrganisationParties.Origins[1]; }
		}

		VoyageDestination Destination1
		{
			get { return VoyageWithConfiguredOrganisationParties.Destinations[0]; }
		}

		VoyageDestination Destination2
		{
			get { return VoyageWithConfiguredOrganisationParties.Destinations[1]; }
		}

		JobVoyage CreateVoyage()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			voyage.JV_OH_Line = CarrierOrg.PK;

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_OA_DepartureCTOAddress = DepartureCTO_Origin1.MainAddress.PK;

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";
			destination1.JB_OA_ArrivalCTOAddress = ArrivalCTO_Destination1.MainAddress.PK;

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "NZAKL";
			origin2.JA_OA_DepartureCTOAddress = DepartureCTO_Origin2.MainAddress.PK;

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";
			destination2.JB_OA_ArrivalCTOAddress = ArrivalCTO_Destination2.MainAddress.PK;

			SetupOrgCommunicationMode(DepartureCTO_Origin1, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);
			SetupOrgCommunicationMode(DepartureCTO_Origin2, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);

			SetupOrgCommunicationMode(DepartureCTO_Origin1, EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule);
			SetupOrgCommunicationMode(DepartureCTO_Origin2, EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule);

			SetupOrgCommunicationMode(ArrivalCTO_Destination1, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);
			SetupOrgCommunicationMode(ArrivalCTO_Destination2, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);

			SetupOrgCommunicationMode(ArrivalCTO_Destination1, EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule);
			SetupOrgCommunicationMode(ArrivalCTO_Destination2, EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule);

			Factory.Save();

			return voyage;
		}

		void SendUniversalEvent(ZString triggerParty, Event ediEvent, Action logEntryCreator)
		{
			IWorkflowProvider workflowProvider = VoyageWithConfiguredOrganisationParties;

			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = ediEvent.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_TriggerParty = triggerParty;

			logEntryCreator();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggeringLog = Factory.LoadTop1<StmALog>(query);

			AssertNotNull("triggering log found", triggeringLog);

			var queuedLog = new QueuedLogForTesting(triggeringLog, trigger);

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);
			processor.Process(new Mock<INotifications>().Object);
			Factory.Save();
		}

		void SendUniversalSchedule(ZString triggerParty, SchemaColumn triggeringProperty, Action triggeringValueSetter)
		{
			IWorkflowProvider workflowProvider = VoyageWithConfiguredOrganisationParties;

			workflowProvider.WorkflowItems.RemoveAndDeleteAll();
			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerFieldName = triggeringProperty.Name;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML;
			action.PQ_TriggerParty = triggerParty;

			triggeringValueSetter();

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));
			var notificationsMock = new Mock<INotifications>();
			WorkflowServiceTaskTester.Process(notificationsMock.Object);
		}

		EDIMessage[] GetMessages(JobVoyage voyage)
		{
			return voyage.Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, voyage.PK));
		}

		#endregion
	}
}
