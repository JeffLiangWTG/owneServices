using System;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowTriggerNotificationTest : TestCaseWithFactory
	{
		Lazy<MessageProcessorCommunicationModesResult> GetModes(IEDICommunicationsMode[] modes)
		{
			return Lazy.Create(() => new MessageProcessorCommunicationModesResult(modes, (NoResString)string.Empty));
		}

		public void TestActionDescriptionIsSetInDeliveryContext()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.ProcessTask.P9_Description = "A Test Trigger";
			var context = new WorkflowTriggerNotification(GetModes(Array.Empty<IEDICommunicationsMode>()), notification, dummy, null).ConvertToContext(notification);
			Assert(context.ActionDescription.Contains("A Test Trigger"));
			AssertEquals(context.ActionDescription, notification.Description);
		}

		public void TestProcessUser()
		{
			string notesExpected;
			var trigger = GetTask(out notesExpected, false);

			AssertProcess(notesExpected, trigger.Item1.ProcessTaskNotifications[0], trigger.Item2);
		}

		public void TestProcessGroup()
		{
			string notesExpected;
			var trigger = GetTask(out notesExpected, true);

			AssertProcess(notesExpected, trigger.Item1.ProcessTaskNotifications[0], trigger.Item2);
		}

		public void TestProcessNullUserNullGroup()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_EmailText = "This notes should end up in an email. Description:(*Description*)+AssignedTo:(*AssignedTo*)";
			trigger.P9_Description = "UNDESCRIBABLEBAL";

			string expectedNotes = "This notes should end up in an email. Description:UNDESCRIBABLEBAL+AssignedTo:Unassigned";

			AssertProcess(expectedNotes, notification, dummy);
		}

		public void TestProcessJobNumberShipment()
		{
			IWorkflowProvider shipment = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
			ProcessTask trigger = shipment.WorkflowItems.Triggers.AddNew();
			((BusinessObject)shipment)[JobShipmentSchema.JS_UniqueConsignRef] = "Int.Max";
			trigger.P9_ParentID = ((BusinessObject)shipment).PK;

			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_EmailText = "This notes should end up in an email. Description:(*Description*)+AssignedTo:(*AssignedTo*)+JobNumber:(*JobNumber*)";
			trigger.P9_Description = "UNDESCRIBABLEBAL";

			string expectedNotes = "This notes should end up in an email. Description:UNDESCRIBABLEBAL+AssignedTo:Unassigned+JobNumber:Int.Max";
			AssertProcess(expectedNotes, notification, (BusinessObject)shipment);
		}

		public void TestProcessJobNumberTransportBooking()
		{
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S0101";

			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_JobType = "BKG";
			consolidation.KB_ParentTableCode = shipment.TablePrefix;
			consolidation.KB_ParentID = shipment.PK;

			var booking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
			booking.KM_KB_Booking = consolidation.PK;
			booking.KM_JobID = "TB0101";
			var iBooking = (IWorkflowProvider)booking;

			var trigger = iBooking.WorkflowItems.Triggers.AddNew();
			trigger.P9_ParentID = booking.PK;

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_EmailText = "This notes should end up in an email. Description:(*Description*)+AssignedTo:(*AssignedTo*)+JobNumber:(*JobNumber*)";
			trigger.P9_Description = "UNDESCRIBABLEBAL";

			string expectedNotes = "This notes should end up in an email. Description:UNDESCRIBABLEBAL+AssignedTo:Unassigned+JobNumber:TB0101";
			AssertProcess(expectedNotes, notification, (BusinessObject)iBooking);
		}

		public void TestProcessJobNumberConsol()
		{
			IWorkflowProvider consol = (IWorkflowProvider)Factory.New<Forwarding.IForwardingConsol>();
			ProcessTask trigger = consol.WorkflowItems.Triggers.AddNew();
			((BusinessObject)consol)[JobConsolSchema.JK_UniqueConsignRef] = "clint has huge feet";
			trigger.P9_ParentID = ((BusinessObject)consol).PK;

			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_EmailText = "This notes should end up in an email. Description:(*Description*)+AssignedTo:(*AssignedTo*)+JobNumber:(*JobNumber*)";
			trigger.P9_Description = "UNDESCRIBABLEBAL";

			string expectedNotes = "This notes should end up in an email. Description:UNDESCRIBABLEBAL+AssignedTo:Unassigned+JobNumber:clint has huge feet";
			AssertProcess(expectedNotes, notification, (BusinessObject)consol);
		}

		public void TestProcessJobNumberWithoutParent()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_EmailText = "This notes should end up in an email. Description:(*Description*)+AssignedTo:(*AssignedTo*)+JobNumber:(*JobNumber*)";
			trigger.P9_Description = "UNDESCRIBABLEBAL";

			string expectedNotes = "This notes should end up in an email. Description:UNDESCRIBABLEBAL+AssignedTo:Unassigned+JobNumber:";
			AssertProcess(expectedNotes, notification, dummy);
		}

		public void TestProcessJobNumberOnParentWithoutJob()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Description = "JOB00004321";
			ProcessTask trigger = dummy.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_EmailText = "This notes should end up in an email. Description:(*Description*)+AssignedTo:(*AssignedTo*)+JobNumber:(*JobNumber*)";
			trigger.P9_Description = "UNDESCRIBABLEBAL";
			string expectedNotes = "This notes should end up in an email. Description:UNDESCRIBABLEBAL+AssignedTo:Unassigned+JobNumber:JOB00004321";
			AssertProcess(expectedNotes, notification, dummy);
		}

		public void TestOriginDestinationWebTrackerUrlShipment()
		{
			string oldUrl = WebDataRegistry.Instance.WebTrackerUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			try
			{
				WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.somesite.com");

				IWorkflowProvider shipment = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
				ProcessTask trigger = shipment.WorkflowItems.Triggers.AddNew();
				((BusinessObject)shipment)[JobShipmentSchema.JS_UniqueConsignRef] = "Int.Max";
				((BusinessObject)shipment)[JobShipmentSchema.JS_RL_NKOrigin] = "UAIEV";
				((BusinessObject)shipment)[JobShipmentSchema.JS_RL_NKDestination] = "AUSYD";

				trigger.P9_ParentID = ((BusinessObject)shipment).PK;

				ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_EmailText = "This notes should end up in an email. Shipment from (*ORIGIN*) to (*dEstination*) has arrived. Can be viewed at: (*webtrackerurl*)";
				trigger.P9_Description = "UNDESCRIBABLEBAL";

				string expectedNotes = "This notes should end up in an email. Shipment from Kiev to Sydney has arrived. Can be viewed at: <a href=http://www.somesite.com/AutoLoginRequestHandler.axd?AutoLogin=";
				AssertProcess(expectedNotes, notification, (BusinessObject)shipment);
			}
			finally
			{
				WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldUrl);
			}
		}

		public void TestFallbackForEmptyEmailSubject()
		{
			void TestCase(string notificationType)
			{
				var dummy = Factory.New<DummyWithWorkflow>();
				var trigger = dummy.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Task Desc";
				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_EmailText = "This notes should end up in an email.";

				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				mode.EK_FileFormat = notificationType;
				mode.EK_Destination = "test@test.com";
				mode.EK_Filename = "test_filename";
				mode.EK_ServerAddressSubject = "";

				var workflowTriggerNotification = new WorkflowTriggerNotification(GetModes(new[] { mode }), action, dummy, null);

				((IProcessor)workflowTriggerNotification).Process(null);
				Factory.Save();
				AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContains("Email body", action.PQ_EmailText, email.Body);
				AssertEquals("Email recipient", true, email.Recipients.Contains("test@test.com"));
				AssertEquals("Expecting empty email subject to fallback to WorkflowDescriptor.GetSubjectForEmailParty", $"{DummyWorkflowDescriptor.Instance.Description} - {trigger.P9_Description}", email.Subject);
				Env.ClearAllEmailsCreated();
			}

			TestCase(EDICommunicationsModeFileFormatList.Codes.NotificationEmail);
			TestCase(EDICommunicationsModeFileFormatList.Codes.NotificationBodyEmail);
		}

		public void TestExtraDataSubstitution()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Description = "JOB00001234";
			ProcessTask trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "UNDESCRIBABLEBAL-(*HelloWorld*)";
			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_EmailText = "This notes should end up in an email. Description:(*Description*)+AssignedTo:(*AssignedTo*)+JobNumber:(*JobNumber*)+HelloWorld:(*HelloWorld*)";
			string expectedNotes = "This notes should end up in an email. Description:UNDESCRIBABLEBAL-+AssignedTo:Unassigned+JobNumber:JOB00001234+HelloWorld:";
			WorkflowTriggerNotification workflowTriggerNotification = new WorkflowTriggerNotification(GetModes(new[] { CommunicationsMode }), action, dummy, null);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			((IProcessor)workflowTriggerNotification).Process(null);
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Wrong body without extra substituting and removed unprocessed templates", expectedNotes, Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals("Should send it to correct recepient", true, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Contains("test@test.com"));

			workflowTriggerNotification.ExtraDataSubstitution = ExtraDataSubstitution;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			expectedNotes = "This notes should end up in an email. Description:UNDESCRIBABLEBAL-HI PEOPLE+AssignedTo:Unassigned+JobNumber:JOB00001234+HelloWorld:HI PEOPLE";
			((IProcessor)workflowTriggerNotification).Process(null);
			Factory.Save();
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("subbjectt-HI PEOPLE", email.Subject);
			AssertContains("Wrong body", expectedNotes, email.Body);
			AssertEquals("Should send it to correct recepient", true, email.Recipients.Contains("test@test.com"));
		}

		ZString ExtraDataSubstitution(ProcessTaskNotification action, BusinessObject bizo, ZString data)
		{
			return data.ReplaceIgnoringCase("(*HelloWorld*)", "HI PEOPLE");
		}

		public void TestSubstituteExtendedMacroTemplate()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_VarCharMax = "Smart Dummy Bizo";
			ProcessTask trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Super Duper Trigger";
			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_EmailAddr = "dummy@free.net";
			action.PQ_EmailText = "Bizo: <B>(*Z0_VarCharMax*)</B><BR />Trigger: (*P9_Description*)<BR />Action: (*PQ_EmailAddr*)";

			const string expectedText = "Bizo: <B>Smart Dummy Bizo</B><BR />Trigger: Super Duper Trigger<BR />Action: dummy@free.net";

			WorkflowTriggerNotification workflowTriggerNotification = new WorkflowTriggerNotification(GetModes(new[] { CommunicationsMode }), action, dummy, null);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			((IProcessor)workflowTriggerNotification).Process(null);
			Factory.Save();
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains(expectedText, Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestSubstituteWithNonLatinCharacters()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_VarCharMax = "敏";
			ProcessTask trigger = dummy.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_EmailAddr = "dummy@free.net";
			action.PQ_EmailText = "Bizo: <B>(*Z0_VarCharMax*)</B>";

			const string expectedText = "Bizo: <B>敏</B>";

			WorkflowTriggerNotification workflowTriggerNotification = new WorkflowTriggerNotification(GetModes(new[] { CommunicationsMode }), action, dummy, null);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			((IProcessor)workflowTriggerNotification).Process(null);
			Factory.Save();
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains(expectedText, Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestEventReference()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.IncidentClosed, "Confirmed");
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_EmailAddr = "mail@is.down";
			action.PQ_EmailText = "Reference: (*EventReference*)";
			var workflowTriggerNotification = new WorkflowTriggerNotification(GetModes(new[] { CommunicationsMode }), action, dummy, Lazy.Create(() => (IStmALog)log));
			Env.OutgoingMailManager.EmailsCreated.Clear();
			((IProcessor)workflowTriggerNotification).Process(null);
			Factory.Save();
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Reference: Confirmed", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestGetUserEmailWhoRaisedEvent()
		{
			// Arrange
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "TST";
			user.GS_EmailAddress = "tst@tst.com";
			var dummy = Factory.New<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.IncidentClosed, "Confirmed Stone!");
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_GS_NKUser = user.GS_Code;
			}
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_EmailAddr = "tst@tst.com";
			action.PQ_EmailTextFallbackToTemplate = "Hello (*GetUserEmailWhoRaisedEvent(\"ICL\",\"Confirmed*\")*)!";
			CommunicationsMode.EK_Destination = "tst@tst.com";
			var workflowTriggerNotification = new WorkflowTriggerNotification(GetModes(new[] { CommunicationsMode }), action, dummy, Lazy.Create(() => (IStmALog)log));
			Env.OutgoingMailManager.EmailsCreated.Clear();
			// Act
			((IProcessor)workflowTriggerNotification).Process(null);
			Factory.Save();
			// Assert
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Should substitute the user email in body.", user.GS_EmailAddress, Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestGetUserEmailWhoRaisedEventWithPK()
		{
			// Arrange
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "TST";
			user.GS_EmailAddress = "tst@tst.com";
			var dummy = Factory.New<DummyWithWorkflow>();
			var child = dummy.Collection.AddNew();
			var log = child.Logs.AddNew(Events.IncidentClosed, "Confirmed Stone!");
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_GS_NKUser = user.GS_Code;
			}
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_EmailAddr = "tst@tst.com";
			action.PQ_EmailTextFallbackToTemplate = "Hello (*GetUserEmailWhoRaisedEvent(\"ICL\",\"Confirmed*\", \"(*Collection.PK*)\")*)!";
			CommunicationsMode.EK_Destination = "tst@tst.com";
			var workflowTriggerNotification = new WorkflowTriggerNotification(GetModes(new[] { CommunicationsMode }), action, dummy, Lazy.Create(() => (IStmALog)log));
			Env.OutgoingMailManager.EmailsCreated.Clear();
			// Act
			((IProcessor)workflowTriggerNotification).Process(null);
			Factory.Save();
			// Assert
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Should substitute the user email in body.", user.GS_EmailAddress, Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		void CreateEmailPopulatedByEventContextDataForTest(string emailText)
		{
			var interchange = Factory.New<IEDIInterchange>();
			interchange.EI_From = "The Death Star";
			interchange.EI_To = "Yavin 4";
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageNum = "99999999999";
			message.EM_MessageSubType = "XUS";
			message.EM_MessageType = "XDC";
			((BusinessObject)message)[EDIMessageSchema.EM_EI] = interchange.PK;
			message.Content = XElement.Parse(@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>StarDestroyer</Type>
          <Key>S00001043</Key>
        </DataSource>
      </DataSourceCollection>

      <DataTargetCollection>
        <DataTarget>
          <Type>StarDestroyer</Type>
          <Key>S00001043</Key>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>DST</Code>
        <Description>Destruction</Description>
      </ActionPurpose>
      <Company>
        <Code>GAL</Code>
        <Name>Galactic Empire</Name>
      </Company>
      <EnterpriseID>GAL</EnterpriseID>
      <EventType>
        <Code>IMP</Code>
        <Description>Imperial</Description>
      </EventType>
      <EventUser>
        <Code>DRV</Code>
        <Name>Darth Vader</Name>
      </EventUser>
      <ServerID>GAL</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2011-04-21T12:06:00</TriggerDate>
      <TriggerDescription>Surrender or Die</TriggerDescription>
      <TriggerReference>Darth Vader</TriggerReference>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <WayBillNumber>IAMTHEMASTER</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>".Trim());

			var dummy = Factory.New<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_EmailAddr = "mail@is.down";
			action.PQ_EmailText = emailText;
			var pivot = Factory.New<IGenPivot>();
			pivot.XX_Relation1ID = log.PK;
			pivot.XX_Relation1TableCode = "SL";
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = "EM";
			pivot.XX_RelationType = Constants.GenPivotTypes.XmlEdiMessage;

			var workflowTriggerNotification = new WorkflowTriggerNotification(GetModes(new[] { CommunicationsMode }), action, dummy, Lazy.Create(() => (IStmALog)log));
			Env.OutgoingMailManager.EmailsCreated.Clear();
			((IProcessor)workflowTriggerNotification).Process(null);
			Factory.Save();
		}

		public void TestGetEventContextByKey()
		{
			var emailText = @"Data Source Trigger Reference: (*GetEventContextByKey(Data Source Trigger Reference)*),
Inexisting context key: (*GetEventContextByKey(Invalid key)*),
Data Source Trigger Description: (*GetEventContextByKey(Data Source Trigger Description)*),
Empty context key: (*GetEventContextByKey()*).";
			CreateEmailPopulatedByEventContextDataForTest(emailText);

			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains(@"Data Source Trigger Reference: Darth Vader,
Inexisting context key: ,
Data Source Trigger Description: Surrender or Die,
Empty context key: .", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestNestedGetEventContextByKey()
		{
			var emailText = @"Nested event context key in true condition: (*If(""(*GetEventContextByKey(Data Source Trigger Description)*)""==""Surrender or Die"", ""TRUE"", ""FALSE"")*),
Nested event context key in false condition: (*If(""(*GetEventContextByKey(Data Source Trigger Description)*)""==""Darth Vader"", ""TRUE"", ""FALSE"")*),
Two nested event context keys in true condition: (*If(""(*GetEventContextByKey(Data Source Trigger Description)*)""==""(*GetEventContextByKey(Data Source Trigger Description)*)"", ""TRUE"", ""FALSE"")*),
Event Context Macro: (*GetEventContextByKey(Data Source Trigger Description)*)";
			CreateEmailPopulatedByEventContextDataForTest(emailText);

			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains(@"Nested event context key in true condition: TRUE,
Nested event context key in false condition: FALSE,
Two nested event context keys in true condition: TRUE,
Event Context Macro: Surrender or Die", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestGetEventContextByKey_NoContext()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_EmailAddr = "mail@is.down";
			action.PQ_EmailText = "Data Source Trigger Reference: (*GetEventContextByKey(Data Source Trigger Reference)*).";
			var workflowTriggerNotification = new WorkflowTriggerNotification(GetModes(new[] { CommunicationsMode }), action, dummy, Lazy.Create(() => (IStmALog)log));
			Env.OutgoingMailManager.EmailsCreated.Clear();
			((IProcessor)workflowTriggerNotification).Process(null);
			Factory.Save();
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Data Source Trigger Reference: .", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestSendErrorNotificationWhenMessagePurposeNotFound()
		{
			string notesExpected;
			var (trigger, dummy) = GetTask(out notesExpected, false);
			var action = trigger.ProcessTaskNotifications[0];
			action.PQ_MessagePurpose = "P0";
			CommunicationsMode.EK_MessagePurpose = "P1";

			AssertProcess(notesExpected, action, dummy);
			AssertEquals("Trigger/Milestone action message purpose [P0] is not part of EDICommunicationsMode's message purpose of [P1]. There are 1 modes currently present.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestDoNotSendErrorNotificationWhenEmptyCommunicationModes()
		{
			string notesExpected;
			var (trigger, dummy) = GetTask(out notesExpected, false);
			var action = trigger.ProcessTaskNotifications[0];
			action.PQ_MessagePurpose = "P0";

			ErrorReporter.Clear();

			var notification = new WorkflowTriggerNotification(GetModes(Array.Empty<EDICommunicationsMode>()), action, dummy, null);
			((IProcessor)notification).Process(null);
			Factory.Save();
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestSendErrorNotificationIncludesCountWhenMessagePurposeNotFound()
		{
			string notesExpected;
			var (trigger, dummy) = GetTask(out notesExpected, false);
			var action = trigger.ProcessTaskNotifications[0];
			action.PQ_MessagePurpose = "P0";
			CommunicationsMode.EK_MessagePurpose = "P1";
			AssertProcess(notesExpected, action, dummy);
			AssertEquals("Trigger/Milestone action message purpose [P0] is not part of EDICommunicationsMode's message purpose of [P1]. There are 1 modes currently present.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSendNotification_For_MatchedAndEmptyCommunicationModes()
		{
			string notesExpected;
			var (trigger, dummy) = GetTask(out notesExpected, false);
			var action = trigger.ProcessTaskNotifications[0];
			action.PQ_MessagePurpose = "P0";
			var communicationsMode1 = GetNewCommunicationsMode();
			CommunicationsMode.EK_MessagePurpose = string.Empty;
			communicationsMode1.EK_MessagePurpose = "P0";
			var notification = new WorkflowTriggerNotification(GetModes(new EDICommunicationsMode[] { CommunicationsMode, communicationsMode1 }), action, dummy, null);
			((IProcessor)notification).Process(null);
			Factory.Save();
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSendNotification_For_NotMatchedAndEmptyCommunicationModes()
		{
			string notesExpected;
			var (trigger, dummy) = GetTask(out notesExpected, false);
			var action = trigger.ProcessTaskNotifications[0];
			action.PQ_MessagePurpose = "P0";
			var communicationsMode1 = GetNewCommunicationsMode();
			CommunicationsMode.EK_MessagePurpose = string.Empty;
			communicationsMode1.EK_MessagePurpose = "P1";
			var notification = new WorkflowTriggerNotification(GetModes(new EDICommunicationsMode[] { CommunicationsMode, communicationsMode1 }), action, dummy, null);
			((IProcessor)notification).Process(null);
			Factory.Save();
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSendNotification_For_MatchedAndNotEmptyCommunicationModes()
		{
			string notesExpected;
			var (trigger, dummy) = GetTask(out notesExpected, false);
			var action = trigger.ProcessTaskNotifications[0];
			action.PQ_MessagePurpose = "P0";
			var communicationsMode1 = GetNewCommunicationsMode();
			CommunicationsMode.EK_MessagePurpose = "P0";
			communicationsMode1.EK_MessagePurpose = "P1";
			var notification = new WorkflowTriggerNotification(GetModes(new EDICommunicationsMode[] { CommunicationsMode, communicationsMode1 }), action, dummy, null);
			((IProcessor)notification).Process(null);
			Factory.Save();
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSendNotification_For_NotMatchedAndNotEmptyCommunicationModes()
		{
			string notesExpected;
			var (trigger, dummy) = GetTask(out notesExpected, false);
			var action = trigger.ProcessTaskNotifications[0];
			action.PQ_MessagePurpose = "P0";
			var communicationsMode1 = GetNewCommunicationsMode();
			CommunicationsMode.EK_MessagePurpose = "P1";
			communicationsMode1.EK_MessagePurpose = "P2";
			var notification = new WorkflowTriggerNotification(GetModes(new EDICommunicationsMode[] { CommunicationsMode, communicationsMode1 }), action, dummy, null);
			((IProcessor)notification).Process(null);
			Factory.Save();
			AssertEquals("Trigger/Milestone action message purpose [P0] is not part of EDICommunicationsMode's message purpose of [P1, P2]. There are 2 modes currently present.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		#region Implementation

		public EDICommunicationsMode GetNewCommunicationsMode()
		{
			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			communicationsMode.EK_Destination = "test1@test.com";
			communicationsMode.EK_Filename = "test1_filename";
			communicationsMode.EK_ServerAddressSubject = "subbjectt1-(*HelloWorld*)";
			return communicationsMode;
		}

		EDICommunicationsMode CommunicationsMode
		{
			get
			{
				if (communicationsMode == null)
				{
					communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
					communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
					communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
					communicationsMode.EK_Destination = "test@test.com";
					communicationsMode.EK_Filename = "test_filename";
					communicationsMode.EK_ServerAddressSubject = "subbjectt-(*HelloWorld*)";
				}
				return communicationsMode;
			}
		}
		EDICommunicationsMode communicationsMode;

		(ProcessTask, BusinessObject) GetTask(out string notesExpected, bool useGroup)
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Triggers.AddNew();
			string notes = "This notes should end up in an email. Description:(*Description*)+AssignedTo:(*AssignedTo*)";
			ProcessTaskNotification notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_EmailText = notes;
			if (useGroup)
			{
				GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Desc = "AA Group";
				task.P9_GG_AssignedGroup = group.PK;
				task.P9_Description = "GROUP_USE";
				notesExpected = "This notes should end up in an email. Description:GROUP_USE+AssignedTo:AA Group";
			}
			else
			{
				GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_FullName = "Zorro's Undies";
				staff.GS_Code = "ZRU";
				task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				task.P9_Description = "UNDESCRIBABLEBAL";
				notesExpected = "This notes should end up in an email. Description:UNDESCRIBABLEBAL+AssignedTo:Zorro's Undies";
			}

			return (task, dummy);
		}

		void AssertProcess(string notesExpected, ProcessTaskNotification action, BusinessObject parent)
		{
			WorkflowTriggerNotification notification = new WorkflowTriggerNotification(GetModes(new EDICommunicationsMode[] { CommunicationsMode }), action, parent, null);
			((IProcessor)notification).Process(null);
			action.Factory.Save();
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Wrong body", notesExpected, Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals("Should send it to correct recepient", true, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Contains("test@test.com"));
		}

		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}

		#endregion Implementation
	}
}
