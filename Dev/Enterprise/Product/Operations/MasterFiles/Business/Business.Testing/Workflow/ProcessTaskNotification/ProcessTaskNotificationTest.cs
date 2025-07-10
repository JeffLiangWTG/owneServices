using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskNotification))]
	sealed class ProcessTaskNotificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRunLocation()
		{
			foreach (CodeDescriptionPair actionType in new WorkflowTriggerActionTypeConstants())
			{
				var notification = Factory.New<ProcessTaskNotification>();
				notification.PQ_TriggerType = actionType.Code;

				if (actionType.Code == WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange)
				{
					AssertEquals(TriggerRunLocation.Client, (notification as ITriggerAction).RunLocation);
				}
				else
				{
					AssertEquals("Note To Developer: If you have added a new Action Type and the RunLocation is set to Client, please review the BusinessObjectPropertyChangeTracker class to check if it is applicable to the new Action Type and if any modifications are needed for error messages.", TriggerRunLocation.Server, (notification as ITriggerAction).RunLocation);
				}
			}
		}

		public void TestRootTypes()
		{
			var task = Factory.New<ProcessTask>();
			var notification = Factory.New<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			var rootTypeProvider = notification as IRootTypeProvider;
			AssertSequencesEqual("RootTypes", new[] { task.GetType(), notification.GetType(), typeof(StmALog) }, rootTypeProvider.RootTypes);

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var shipmentTask = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			notification.PQ_P9 = shipmentTask.PK;
			AssertSequencesEqual("Notification's Parent Job is shipment", new[] { shipment.GetType(), shipmentTask.GetType(), notification.GetType(), typeof(StmALog) }, rootTypeProvider.RootTypes);

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			AssertSequencesEqual("Notification's Parent Job is shipment and shipment has attached declaration which company is the same as the context, has extra macro type(type of declaration)",
				new[] { shipment.GetType(), shipmentTask.GetType(), declaration.GetType(), notification.GetType(), typeof(StmALog) }, rootTypeProvider.RootTypes);
		}

		public void TestStaffCode_ReadOnly()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var notification = person.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			Assert(!notification.StaffCode_ReadOnly);

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			Assert(notification.StaffCode_ReadOnly);
		}

		public void TestPQ_EmailAddr_ReadOnly()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var notification = person.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			Assert(!notification.PQ_EmailAddr_ReadOnly);

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail;
			Assert(!notification.PQ_EmailAddr_ReadOnly);

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail;
			Assert(!notification.PQ_EmailAddr_ReadOnly);

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			Assert(notification.PQ_EmailAddr_ReadOnly);
		}

		public void TestEDIMessageDeliveryContextSelector_ReadOnly()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			var trigger = template.WorkflowItems.Triggers.AddNew();
			var action = trigger.CompletionTriggerActionsCollection().AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertEquals(true, action.PQ_ECS_MessageDeliveryContextSelectorInfo.ReadOnly);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			AssertEquals(false, action.PQ_ECS_MessageDeliveryContextSelectorInfo.ReadOnly);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;
			AssertEquals(false, action.PQ_ECS_MessageDeliveryContextSelectorInfo.ReadOnly);

			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
			action = ((ProcessTask)dummy.WorkflowItems.Triggers.First()).ProcessTaskNotifications.First();
			AssertEquals("New actions can be added to triggers from templates so it doesn't make sense to make this field read only (unless we had some extra over complicated read only logic)"
				, false, action.PQ_ECS_MessageDeliveryContextSelectorInfo.ReadOnly);

			action = dummy.WorkflowItems.Triggers.AddNew().CompletionTriggerActionsCollection().AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;
			AssertEquals(false, action.PQ_ECS_MessageDeliveryContextSelectorInfo.ReadOnly);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertEquals(true, action.PQ_ECS_MessageDeliveryContextSelectorInfo.ReadOnly);
		}

		public void TestAddDocumentToEDocsActionWhenDocumentNull()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;

			shipmentWithWorkflow.Logs.AddNew(AutoEvents.CustomisableEvent00);

			Factory.Save();

			string logs = "";
			AssertNoExceptionThrown(() => { logs = MasterFilesTestHelper.RunLogWalker(); });
			AssertContains("EDC action does not contain a document", logs);
		}

		public void TestStaffCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@email.com";
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var notification = person.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;

			Assert(notification.StaffCode_ReadOnly);

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			Assert(!notification.StaffCode_ReadOnly);

			notification.StaffCode = staff.GS_Code;
			AssertEquals("staff@email.com", notification.PQ_EmailAddr);
		}

		public void TestSetEmailAddressIfNeededOnLoaded()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_EmailAddress = "personal@email.com";
			var notification = person.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			notification.PQ_EmailAddr = "";
			Factory.Save();

			AssertNullOrEmpty(notification.PQ_EmailAddr);

			var notificationInAnotherFactory = new BusinessObjectFactory().Load<ProcessTaskNotification>(notification.PK);
			AssertEquals("personal@email.com", notificationInAnotherFactory.PQ_EmailAddr);
		}

		public void TestSetEmailAddressIfNeeded()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var person = GlbPerson.CreateFromStaff(Factory, staff);
			person.PER_EmailAddress = "personal@email.com";
			staff.GS_EmailAddress = "work@email.com";
			var notification = person.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			AssertEquals("personal@email.com", notification.PQ_EmailAddr);

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail;
			AssertEquals("work@email.com", notification.PQ_EmailAddr);

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail;
			AssertEquals("personal@email.com", notification.PQ_EmailAddr);
		}

		public void TestIsEmailAddressRetrievable()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var notification = person.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();

			Assert(!notification.IsEmailAddressRetrievable(person));

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			Assert(notification.IsEmailAddressRetrievable(person));
			Assert(notification.IsEmailRelatedTriggerParty());

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail;
			Assert(notification.IsEmailAddressRetrievable(person));
			Assert(notification.IsEmailRelatedTriggerParty());

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail;
			Assert(notification.IsEmailAddressRetrievable(person));
			Assert(notification.IsEmailRelatedTriggerParty());

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			Assert(!notification.IsEmailAddressRetrievable(person));
			Assert(notification.IsEmailRelatedTriggerParty());

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery;
			Assert(!notification.IsEmailAddressRetrievable(person));
			Assert(!notification.IsEmailRelatedTriggerParty());
		}

		[ExpectException(typeof(ZSaveException))]
		public void TestNullReferenceForParent()
		{
			var notification = Factory.New<ProcessTaskNotification>();
			notification.PQ_P9 = ZGuid.NewZGuid();
			Factory.Save();
		}

		public void TestDeletedParent()
		{
			var task = Factory.New<ProcessTask>();
			var notification = Factory.New<ProcessTaskNotification>();
			notification.PQ_P9 = task.PK;
			AssertNotNull(notification.Parent);
			task.Delete();
			AssertNull(notification.Parent);
		}

		public void TestMacro_EvaluateNestedMacrosSeparately()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "test1";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification.PQ_FieldName = "<Z0_Description>";

			notification.PQ_FieldValue = "<Z0_Code> <Z0_Code>";
			AssertNoErrors("GIVEN nested macro WHEN running macro THEN should not error", notification.PQ_FieldValueInfo);

			notification.PQ_FieldValue = "<Z0_Code> <WorkflowItems.First(\"<P9_Description>\"==\"test1\").FH_Status>";

			AssertNoErrors("GIVEN invalid nested macro (property cannot be found) WHEN running macro THEN should not error", notification.PQ_FieldValueInfo);

			Factory.Save();

			Dummy.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			var logwalker = MasterFilesTestHelper.RunLogWalker();

			CombineAssertions("WHEN processing trigger action THEN should show error", () =>
			{
				AssertContains("Warning: [WorkflowEventTrigger] [Default] Cannot find property FH_Status on Enterprise.MasterFiles.Business.Testing.DummyProcessTask.", logwalker);
				AssertContains("Cannot find property WorkflowItems on Enterprise.MasterFiles.Business.Testing.DummyProcessTask.", logwalker);
				AssertContains("Cannot find property WorkflowItems on Enterprise.MasterFiles.Business.ProcessTaskNotification.", logwalker);
				AssertContains("Warning: [WorkflowEventTrigger] [Default] Set field (FLD) macro evaluated to null. Macro: [<Z0_Code> <WorkflowItems.First(\"<P9_Description>\"==\"test1\").FH_Status>] Source: [Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, ", logwalker);
			});

			notification.PQ_FieldValue = "<Z0_Code> <WorkflowItems.First(\"<P9_Description>\"==\"test1\").P9_Description>";
			AssertNoErrors("GIVEN nested macro WHEN running macro THEN should not error", notification.PQ_FieldValueInfo);
		}

		public void TestMacro_EvaluateRoot_FieldName()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var task1 = org.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "task1";
			task1.P9_GS_NKAssignedStaffMember = "GAN";

			var task2 = org.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "task2";
			task2.P9_GS_NKAssignedStaffMember = "FRO";

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("task1 status", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
				AssertEquals("task2 status", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			});

			var trigger = org.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<WorkflowItems.Where(\"<P9_GS_NKAssignedStaffMember>\" == \"<TriggeringEvent.SL_GS_NKUser>\").P9_Status>";
			action.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var triggeringEvent = org.GetLogs().AddNew(Events.Authorised);
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_GS_NKUser = "FRO";
			}

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			task1.Reload();
			task2.Reload();

			CombineAssertions("GIVEN macro in action.PQ_FieldName, WHEN RunLogWalker THEN it should match 'task2' and update its status to 'CLS'", () =>
			{
				AssertEquals("task1", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
				AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
			});
		}

		public void TestMacro_EvaluateRoot_FieldValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "AAA";

			var task1 = org.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "task1";
			task1.P9_GS_NKAssignedStaffMember = "GAN";

			var task2 = org.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "task2";
			task2.P9_GS_NKAssignedStaffMember = "FRO";

			var trigger = org.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<OH_FullName>";
			action.PQ_FieldValue = "<WorkflowItems.FirstOrDefault(\"<P9_GS_NKAssignedStaffMember>\" == \"<TriggeringEvent.SL_GS_NKUser>\").P9_GS_NKAssignedStaffMember>";

			var triggeringEvent = org.GetLogs().AddNew(Events.Authorised);
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_GS_NKUser = "FRO";
			}

			Factory.Save();

			AssertEquals("Precondition: OrgHeader.OH_FullName", "AAA", org.OH_FullName);

			MasterFilesTestHelper.RunLogWalker();

			org.Reload();

			AssertEquals("GIVEN macro in action.PQ_FieldValue, WHEN RunLogWalker THEN it should match 'task2' and update its OH_FullName to 'FRO'",
				"FRO",
				org.OH_FullName);
		}

		public void TestMacro_EvaluatePropertyInfoMessages()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "test1";

			// First Property Clause

			var notification1 = milestone.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			notification1.PQ_FieldName = "<WorkflowItems.First(\"<P9_Description>\" == \"test\").P9_Status>";
			notification1.PQ_FieldValue = "CLS";

			AssertHasWarnings("<First> function is not supported.", notification1.PQ_FieldNameInfo);
			AssertNoErrors(notification1.PQ_FieldNameInfo);

			AssertHasWarnings("Field Name has errors.", notification1.PQ_FieldValueInfo);
			AssertNoErrors(notification1.PQ_FieldValueInfo);

			// FirstOrDefault Property Clause

			var notification2 = milestone.ProcessTaskNotifications.AddNew();
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			notification2.PQ_FieldName = "<WorkflowItems.FirstOrDefault(\"<P9_Description>\" == \"test\").P9_Status>";
			notification2.PQ_FieldValue = "CLS";

			AssertHasWarnings("<FirstOrDefault> function is not supported.", notification2.PQ_FieldNameInfo);
			AssertNoErrors(notification2.PQ_FieldNameInfo);

			AssertHasWarnings("Field Name has errors.", notification2.PQ_FieldValueInfo);
			AssertNoErrors(notification2.PQ_FieldValueInfo);

			// Where Property Clause

			var notification3 = milestone.ProcessTaskNotifications.AddNew();
			notification3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			notification3.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Description>\" == \"test\").P9_Status>";
			notification3.PQ_FieldValue = "CLS";

			AssertNoNotifications(notification3.PQ_FieldNameInfo);
			AssertNoNotifications(notification3.PQ_FieldValueInfo);

			// Unknown Property Clause

			var notification4 = milestone.ProcessTaskNotifications.AddNew();
			notification4.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			notification4.PQ_FieldName = "<WorkflowItems.test(\"<P9_Description>\" == \"test\").P9_Status>";
			notification4.PQ_FieldValue = "CLS";

			AssertHasWarnings("Cannot find property test(\"<P9_Description>\" == \"test\") on Enterprise.MasterFiles.Business.Testing.DummyProcessTask.", notification4.PQ_FieldNameInfo);
			AssertNoErrors(notification4.PQ_FieldNameInfo);

			AssertNoErrors(notification4.PQ_FieldValueInfo);
		}

		public void TestMacroClauseProcessor_FieldName_First()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			var shipmentWorkflow = (IWorkflowProvider)shipment;

			var task1 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "a";

			var task2 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "a";

			var task3 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task3.P9_Description = "b";

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("task1 status", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
				AssertEquals("task2 status", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
				AssertEquals("task3 status", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
			});

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<WorkflowItems.First(\"<P9_Description>\" == \"a\").P9_Status>";
			action.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var triggeringEvent = shipment.GetLogs().AddNew(Events.CustomisableEvent00);

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			task1.Reload();
			task2.Reload();
			task3.Reload();

			CombineAssertions("GIVEN macro in action.PQ_FieldName, WHEN RunLogWalker THEN task1 and task2 should update to Closed.", () =>
			{
				AssertEquals("task1", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
				AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
				AssertEquals("task3", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
			});
		}

		public void TestMacroClauseProcessor_FieldName_FirstOrDefault()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			var shipmentWorkflow = (IWorkflowProvider)shipment;

			var task1 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "a";

			var task2 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "a";

			var task3 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task3.P9_Description = "b";

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("task1 status", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
				AssertEquals("task2 status", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
				AssertEquals("task3 status", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
			});

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<WorkflowItems.FirstOrDefault(\"<P9_Description>\" == \"a\").P9_Status>";
			action.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var triggeringEvent = shipment.GetLogs().AddNew(Events.CustomisableEvent00);

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			task1.Reload();
			task2.Reload();
			task3.Reload();

			CombineAssertions("GIVEN macro in action.PQ_FieldName, WHEN RunLogWalker THEN task1 and task2 should update to Closed.", () =>
			{
				AssertEquals("task1", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
				AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
				AssertEquals("task3", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
			});
		}

		public void TestMacroClauseProcessor_FieldName_Where()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			var shipmentWorkflow = (IWorkflowProvider)shipment;

			var task1 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "a";

			var task2 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "a";

			var task3 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task3.P9_Description = "b";

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("task1 status", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
				AssertEquals("task2 status", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
				AssertEquals("task3 status", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
			});

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Description>\" == \"a\").P9_Status>";
			action1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var triggeringEvent = shipment.GetLogs().AddNew(Events.CustomisableEvent00);

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			task1.Reload();
			task2.Reload();
			task3.Reload();

			CombineAssertions("GIVEN macro in action.PQ_FieldName, WHEN RunLogWalker THEN it should match both 'task1' and 'task2' and update its status to 'CLS'", () =>
			{
				AssertEquals("task1", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
				AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
				AssertEquals("task3", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
			});
		}

		public void TestMacroClauseProcessor_WhenThereIsWorkingTask_ShouldNotAllowSettingAnotherStaffTaskToWorking()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;

			var task1 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "a";
			task1.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var task2 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "b";
			task2.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Description>\" == \"b\").P9_Status>";
			action1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Working;

			var triggeringEvent = shipment.GetLogs().AddNew(Events.CustomisableEvent00);

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			task1.Reload();
			task2.Reload();

			CombineAssertions("GIVEN macro in action.PQ_FieldName and 'task1' already 'WRK', WHEN RunLogWalker THEN it should set 'task2' to 'SUS'", () =>
			{
				AssertEquals("task1", ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
				AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Suspended, task2.P9_Status);
			});
		}

		public void TestMacroClauseProcessor_WhenThereIsWorkingTask_ShouldAllowSettingAnotherGroupTaskToWorking()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var task1 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "a";
			task1.P9_GG_AssignedGroup = group.PK;
			task1.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var task2 = shipmentWorkflow.WorkflowItems.Tasks.AddNew();
			task2.P9_GG_AssignedGroup = group.PK;
			task2.P9_Description = "b";
			task2.P9_GS_NKAssignedStaffMember = ZString.Empty;

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Description>\" == \"b\").P9_Status>";
			action1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Working;

			var triggeringEvent = shipment.GetLogs().AddNew(Events.CustomisableEvent00);

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			task1.Reload();
			task2.Reload();

			CombineAssertions("EVEN macro in action.PQ_FieldName and 'task1' is 'WRK', WHEN RunLogWalker it should set 'task2' to 'WRK' becase same group CAN have different task with status working!", () =>
			{
				AssertEquals("task1", ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
				AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Working, task2.P9_Status);
			});
		}

		public void TestMacro_EvaluateCollectionFromProcessTaskNotification()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var milestone = org.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			var notification = milestone.ProcessTaskNotifications.AddNew();

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification.PQ_FieldName = "<OH_Code>";

			notification.PQ_FieldValue = "<ExtraResources.First(\"<PE_GS>\"==\"test1\").PE_GS>";
			AssertNoErrors("GIVEN collection from process-task-notification WHEN validating THEN should not error", notification.PQ_FieldValueInfo);

			notification.PQ_FieldValue = "<ExtraResources.First(\"<???>\"==\"test1\").PE_GS>";
			AssertNoErrors("GIVEN collection from process-task-notification WHEN validating THEN should not error because ExtraResources is empty hence not setting FieldValue and <???> wasn't evaluated", notification.PQ_FieldValueInfo);

			notification.PQ_FieldValue = "<ExtraResources2.First(\"<PE_GS>\"==\"test1\").PE_GS>";
			AssertNoErrors("GIVEN collection from process-task-notification with ExtraResources2 doesn't exist WHEN validating THEN should not error", notification.PQ_FieldValueInfo);

			Factory.Save();

			((IWorkflowProvider)org).Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			var logwalker = MasterFilesTestHelper.RunLogWalker();

			CombineAssertions("WHEN processing trigger action THEN should show error", () =>
			{
				AssertContains("Cannot find property ExtraResources2 on Enterprise.MasterFiles.Business.OrgHeader.", logwalker);
				AssertContains("Cannot find property ExtraResources2 on Enterprise.MasterFiles.Business.OrgHeaderProcessTask.", logwalker);
				AssertContains("Cannot find property ExtraResources2 on Enterprise.MasterFiles.Business.ProcessTaskNotification.", logwalker);
				AssertContains("Set field (FLD) macro evaluated to null. Macro: [<ExtraResources2.First(\"<PE_GS>\"==\"test1\").PE_GS>] Source:", logwalker);
			});
		}

		public void TestMacro_RandomExpression()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "AAA";
			org.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;
			var milestone = org.WorkflowItems.Milestones.AddNew();
			var action = milestone.ProcessTaskNotifications.AddNew();

			var extraResource = milestone.ExtraResources.AddNew();
			extraResource.PE_GS_NKStaffOrResource = "abc";

			Factory.Save();

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "OH_FullName";
			action.PQ_FieldValue = "<ExtraResources.FirstOrDefault(	\"<PE_GS_NKStaffOrResource>\" == \"abc\").Task.ExtraResources.FirstOrDefault(\"<PE_GS_NKStaffOrResource>\" == \"abc\").Task.Parent.OH_Language>";
			AssertNoErrors("GIVEN collection from process-task-notification WHEN running marco THEN should not throw error", action.PQ_FieldValueInfo);

			AssertEquals("pre-condition", "AAA", org.OH_FullName);

			FireTriggerAction(org, milestone, action);
			AssertEquals("GIVEN collection only, WHEN processing macro THEN should return macro-result", Core.SharedConstants.Languages.EnglishAmerican, org.OH_FullName);
		}

		public void TestMacro_ValueFoundOnLastBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "AAA";
			var milestone = org.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "ZZZ";
			var action1 = milestone.ProcessTaskNotifications.AddNew();
			var action2 = milestone.ProcessTaskNotifications.AddNew();

			Factory.Save();

			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1.PQ_FieldName = "OH_FullName";
			action1.PQ_FieldValue = "<ProcessTaskNotifications.FirstOrDefault(\"<PQ_TriggerType>\" == \"FLD\").ProcessTask.P9_Description>";
			AssertNoErrors("GIVEN collection from process-task-notification WHEN validating THEN should not throw error", action1.PQ_FieldValueInfo);

			AssertEquals("pre-condition", "AAA", org.OH_FullName);

			FireTriggerAction(org, milestone, action1);
			AssertEquals("GIVEN collection only, WHEN processing macro THEN should return macro-result", "ZZZ", org.OH_FullName);

			action1.PQ_FieldValue = "<ProcessTaskNotifications.FirstOrDefault(\"<PQ_TriggerType>\" == \"FLD\").ProcessTask.XXX>";

			AssertNoErrors("GIVEN collection from process-task-notification with invalid macro (field cannot be found) WHEN validating THEN should not throw error", action1.PQ_FieldValueInfo);

			var notifications = new NotificationsForTest();
			FireTriggerAction(org, milestone, action1, notifications);

			AssertContains("WHEN running macro THEN should throw error", @"Set field (FLD) macro evaluated to null. Macro: [<ProcessTaskNotifications.FirstOrDefault(""<PQ_TriggerType>"" == ""FLD"").ProcessTask.XXX>] Source:",
				notifications.ToString().Trim());
		}

		public void TestMacro_Evaluate_Workflow()
		{
			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			var jobHeader = BMTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var parent = (DummyWithWorkflow)jobHeader.Parent;

			var firstWorkflow = jobHeader.ProcessHeaders[0];
			firstWorkflow.FH_CompletionStatement = "FirstWorkflow";
			var secondWorkflow = jobHeader.ProcessHeaders.AddNew();
			secondWorkflow.FH_CompletionStatement = "SecondWorkflow";

			var trigger = parent.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<Workflows.Where(\"<FH_CompletionStatement>\" == \"FirstWorkflow\").FH_CompletionStatement>";
			action.PQ_FieldValue = "MainWorkflow";

			var triggeringEvent = parent.GetLogs().AddNew(Events.Authorised);
			using (triggeringEvent.LockForUpdatingKeyFieldsForTesting())
			{
				triggeringEvent.SL_GS_NKUser = "FRO";
			}

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			var firstWorkflowInNewFactory = newFactory.Load<IProcessHeader>(firstWorkflow.PK);
			var secondWorkflowInNewFactory = newFactory.Load<IProcessHeader>(secondWorkflow.PK);

			CombineAssertions("The macro should match the first workflow and thus cause its completion statement to be modified and the second workflow to be unchanged", () =>
			{
				AssertEquals("FirstWorkflow status", "MainWorkflow", firstWorkflowInNewFactory.FH_CompletionStatement);
				AssertEquals("SecondWorkflow status", "SecondWorkflow", secondWorkflowInNewFactory.FH_CompletionStatement);
			});
		}

		public void TestPQ_RelatedEntityID_ReadOnlyness()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = ((IWorkflowProvider)dummy).WorkflowItems.Tasks.AddNew();
			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			task.TriggerConditions.TriggerEventCode = "ATH";

			var triggerAction = task.ProcessTaskNotifications.AddNew();

			Type type = typeof(WorkflowTriggerActionTypeConstants.Codes);
			var fieldInfos = type.GetFields(BindingFlags.Static | BindingFlags.Public);

			foreach (var info in fieldInfos)
			{
				var triggerType = info.GetValue(null) as string;
				triggerAction.PQ_TriggerType = triggerType;
				if (triggerType == WorkflowTriggerActionTypeConstants.Codes.ApplyTag)
				{
					Assert($"if TriggerType = {triggerType}, PQ_RelatedEntityId should be editable", !triggerAction.PQ_RelatedEntityIdInfo.ReadOnly);
				}
				else
				{
					Assert($"if TriggerType = {triggerType}, PQ_RelatedEntityId should be readonly", triggerAction.PQ_RelatedEntityIdInfo.ReadOnly);
				}
			}
		}

		public void TestPQ_RelatedEntityTableCode()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "A1", usageScope: "ALL", scope: "ALL");
			var tag = BMSTestHelper.CreateTagMagnitude(tagDefinition, "TG1", "TAG 1");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			var task = ((IWorkflowProvider)dummy).WorkflowItems.Tasks.AddNew();
			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			task.TriggerConditions.TriggerEventCode = "ATH";

			var triggerAction = task.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			triggerAction.PQ_RelatedEntityId = tag.PK;

			AssertEquals($"PQ_RelatedEntityTableCode is set to {TagMagnitudeSchema.Constants.Prefix}", TagMagnitudeSchema.Constants.Prefix, triggerAction.PQ_RelatedEntityTableCode);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;
			AssertEquals($"PQ_RelatedEntityId set to empty when PQ_TriggerType != {WorkflowTriggerActionTypeConstants.Codes.ApplyTag}", ZGuid.Empty, triggerAction.PQ_RelatedEntityId);
			AssertEquals($"PQ_RelatedEntityTableCode set to empty when PQ_TriggerType != {WorkflowTriggerActionTypeConstants.Codes.ApplyTag}", ZString.Empty, triggerAction.PQ_RelatedEntityTableCode);
		}

		public void TestRelatedEntity()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "A1", usageScope: "ALL", scope: "ALL");
			var tag = BMSTestHelper.CreateTagMagnitude(tagDefinition, "TG1", "TAG 1");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			var task = ((IWorkflowProvider)dummy).WorkflowItems.Tasks.AddNew();
			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			task.TriggerConditions.TriggerEventCode = "ATH";

			var triggerAction = task.ProcessTaskNotifications.AddNew();

			AssertNull(triggerAction.RelatedEntity);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			triggerAction.PQ_RelatedEntityId = tag.PK;

			AssertNotNull(triggerAction.RelatedEntity);
			AssertEquals(tag.PK, triggerAction.RelatedEntity.PK);
			AssertEquals(tag.TGM_Code, triggerAction.RelatedEntity.TGM_Code);
			AssertEquals(tag.TGM_Description, triggerAction.RelatedEntity.TGM_Description);
		}

		#region Property Overrides

		#region ReadOnly

		public void TestReadOnlyMatchesParentTaskTrue()
		{
			AssertReadOnlyMatchesTask(true);
		}

		public void TestReadOnlyMatchesParentTaskFalse()
		{
			AssertReadOnlyMatchesTask(false);
		}

		public void AssertReadOnlyMatchesTask(bool readOnly)
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var ptn = trigger.ProcessTaskNotifications.AddNew();

			// Set ReadOnly after we have created the ProcessTaskNotification
			trigger.ReadOnly = readOnly;
			AssertEquals(readOnly, ptn.ReadOnly);
		}

		#endregion

		#region PQ_EmailTextFallbackToTemplate

		public void TestPQ_EmailTextFallbackToTemplate()
		{
			ProcessTaskNotification templateNotification = WorkflowTemplate.WorkflowItems.Milestones[0].ProcessTaskNotifications.AddNew();
			templateNotification.PQ_EmailText = "TemplateText";
			WorkflowTemplate.Factory.Save();
			Dummy.Factory.Save();

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("1 milestone created from the template", 1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("1 notification created for milestone from the template", 1, Dummy.WorkflowItems.Milestones[0].ProcessTaskNotifications.Count);
			ProcessTaskNotification notification = Dummy.WorkflowItems.Milestones[0].ProcessTaskNotifications[0];
			AssertEquals("TemplateText", notification.PQ_EmailTextFallbackToTemplate);

			notification.PQ_EmailTextFallbackToTemplate = "OverriddenText";
			AssertEquals("OverriddenText", notification.PQ_EmailTextFallbackToTemplate);

			notification.PQ_EmailTextFallbackToTemplate = "";
			AssertEquals("TemplateText", notification.PQ_EmailTextFallbackToTemplate);
		}

		#endregion

		#region SourceTemplateNotification

		public void TestTemplatePK()
		{
			ProcessTaskNotification templateNotification = WorkflowTemplate.WorkflowItems.Milestones[0].ProcessTaskNotifications.AddNew();
			templateNotification.PQ_EmailText = "TemplateText";
			WorkflowTemplate.Factory.Save();
			Dummy.Factory.Save();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			var notification = Dummy.WorkflowItems.Milestones[0].ProcessTaskNotifications[0];

			AssertEquals("Have the right notification reference.", templateNotification.PK, notification.PQ_SourceTemplateNotification);
			AssertEquals(true, notification.TemplateSourcePKInfo.ReadOnly);
			AssertEquals(true, templateNotification.TemplateSourcePKInfo.ReadOnly);

			AssertEquals(WorkflowTemplate.PK, notification.TemplateSourcePK);
			AssertEquals(WorkflowTemplate.PK, notification.TemplateSource.PK);
		}

		public void TestPQ_SourceTemplateNotification_AllowDelete()
		{
			ProcessTaskNotification templateNotification = WorkflowTemplate.WorkflowItems.Milestones[0].ProcessTaskNotifications.AddNew();
			templateNotification.PQ_EmailText = "TemplateText";
			WorkflowTemplate.Factory.Save();
			Dummy.Factory.Save();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			var notification = Dummy.WorkflowItems.Milestones[0].ProcessTaskNotifications[0];

			AssertEquals("Pre condition", templateNotification.PK, notification.PQ_SourceTemplateNotification);

			templateNotification.Delete();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region PQ_SU_Document

		public void TestPQ_EmailTextInfo()
		{
			ProcessTaskNotification ptNotification = Factory.New<ProcessTaskNotification>();
			AssertEquals("Precondition", ZString.Empty, ptNotification.PQ_EmailText);
			AssertEquals("Precondition", false, ptNotification.OverrideEmail);
			AssertEquals("PQ_EmailTextInfo should be read-only when OverrideEmail is false (and by default)", true, ptNotification.PQ_EmailTextInfo.ReadOnly);

			ptNotification.OverrideEmail = true;
			AssertEquals("PQ_EmailTextInfo should be editable when OverrideEmail is true", false, ptNotification.PQ_EmailTextInfo.ReadOnly);

			ptNotification.OverrideEmail = false;
			AssertEquals("PQ_EmailTextInfo should be read-only when OverrideEmail is false", true, ptNotification.PQ_EmailTextInfo.ReadOnly);
		}

		#endregion

		#region PQ_TriggerType

		CodeDescriptionPairList WorkflowTriggerActionTypeConstantsList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs, "");
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendDocument, "");
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "");
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail, "");
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendXML, "");
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback, "");
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML, "");

				return result;
			}
		}

		public void TestSetPQ_SU_DocumentInfo_ReadOnly()
		{
			ProcessTaskNotification ptNotification = GetNewNotificationWithParentJob();
			StmMenuItem document = Factory.NewWithValidTestData<StmMenuItem>();
			ptNotification.PQ_SU_Document = document.PK;
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertEquals("PQ_SU_Document should be editable when PQ_TriggerType is 'DOC' (Document Send)", false, ptNotification.PQ_SU_DocumentInfo.ReadOnly);
			AssertEquals("PQ_SU_Document should preserve its value until it becomes read-only", document.PK, ptNotification.PQ_SU_Document);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertEquals("PQ_SU_Document should be editable when PQ_TriggerType is 'EDC' (Add Documen tTo eDocs)", false, ptNotification.PQ_SU_DocumentInfo.ReadOnly);

			for (int i = 0; i < WorkflowTriggerActionTypeConstantsList.Count; i++)
			{
				if (WorkflowTriggerActionTypeConstantsList[i].Code != WorkflowTriggerActionTypeConstants.Codes.SendDocument && WorkflowTriggerActionTypeConstantsList[i].Code != WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs)
				{
					ptNotification.PQ_SU_Document = Guid.NewGuid();
					ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstantsList[i].Code;
					AssertEquals("PQ_SU_Document should be read-only when PQ_TriggerType is NOT 'DOC' (Document Send) or 'EDC' (Publish Document As eDoc)", true, ptNotification.PQ_SU_DocumentInfo.ReadOnly);
					AssertEquals("PQ_SU_Document should be reset to empty string when it becomes read-only", ZGuid.Empty, ptNotification.PQ_SU_Document);
				}
			}
		}

		public void TestPQ_TriggerTypeSetsPQ_TriggerPartyInfo_ReadOnly()
		{
			ProcessTaskNotification ptNotification = GetNewNotificationWithParentJob();
			var workflowDescriptor = ptNotification.Parent.GetWorkflowDescriptor();

			foreach (string code in WorkflowTriggerActionTypeConstantsList.GetAllCodes())
			{
				ptNotification.PQ_Calc_TriggerParty = "FOO";
				ptNotification.PQ_TriggerType = code;
				if (
					!workflowDescriptor.IsPrintingTriggerAction(ptNotification.PQ_TriggerType)
					&& !workflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(ptNotification.PQ_TriggerType)
					&& !workflowDescriptor.IsManifestMessagingTriggerAction(ptNotification.PQ_TriggerType)
					|| ptNotification.PQ_TriggerType == "STA"
					|| ptNotification.PQ_TriggerType == "DUM"
					|| ptNotification.PQ_TriggerType == "")
				{
					AssertEquals($"PQ_Calc_TriggerParty should not be editable, when PQ_TriggerType={code}", true, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
					AssertEquals($"PQ_Calc_TriggerParty should be reset to empty string when it becomes read-only, PQ_TriggerType={code}", ZString.Empty, ptNotification.PQ_Calc_TriggerParty);
				}
				else
				{
					AssertEquals($"PQ_Calc_TriggerParty should be editable, when PQ_TriggerType={code}", false, ptNotification.PQ_Calc_TriggerPartyInfo.ReadOnly);
					AssertEquals($"PQ_Calc_TriggerParty should preserve its value until it becomes read-only, PQ_TriggerType={code}", "FOO", ptNotification.PQ_Calc_TriggerParty);
				}
			}
		}

		public void TestPQ_TriggerTypeSetsPQ_TriggerPartyServiceInfo_ReadOnly()
		{
			var ptNotification = GetNewNotificationWithParentJob();
			foreach (string code in WorkflowTriggerActionTypeConstantsList.GetAllCodes())
			{
				ptNotification.PQ_Calc_TriggerParty = "foo";
				ptNotification.PQ_TriggerPartyService = "bar";
				ptNotification.PQ_TriggerType = code;
				if (WorkflowTriggerActionTypeConstants.IsXmlUniversalShipment(ptNotification.PQ_TriggerType))
				{
					AssertEquals($"PQ_TriggerPartyService should be editable, when PQ_TriggerType={code}.", false, ptNotification.PQ_TriggerPartyServiceInfo.ReadOnly);
					AssertEquals($"PQ_TriggerPartyService should preserve its value until it becomes read-only, PQ_TriggerType={code}.", "foo", ptNotification.PQ_TriggerPartyService);
				}
				else
				{
					AssertEquals($"PQ_TriggerPartyService should not be editable, when PQ_TriggerType={code}.", true, ptNotification.PQ_TriggerPartyServiceInfo.ReadOnly);
					AssertEquals($"PQ_TriggerPartyService should be reset to empty string when it becomes read-only, PQ_TriggerType={code}.", ZString.Empty, ptNotification.PQ_TriggerPartyService);
				}
			}
		}

		public void TestSetPQ_MessagePurposeInfo_ReadOnly()
		{
			ProcessTaskNotification ptNotification = GetNewNotificationWithParentJob();

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertEquals("PQ_MessagePurpose should be read-only when PQ_TriggerType is 'EDC' (Add Document To eDocs)", true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertEquals("PQ_MessagePurpose should be read-only when PQ_TriggerType is 'DOC' (Send Document)", true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue;
			AssertEquals("PQ_MessagePurpose should be read-only when PQ_TriggerType is 'CAR' (Auto Rate Costs And Revenue)", true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts;
			AssertEquals("PQ_MessagePurpose should be read-only when PQ_TriggerType is 'COS' (Auto Rate Costs)", true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue;
			AssertEquals("PQ_MessagePurpose should be read-only when PQ_TriggerType is 'REV' (Auto RateRevenue)", true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML;
			AssertEquals("PQ_MessagePurpose should be read-only when PQ_TriggerType is 'XUM' (SendUniversalShipmentManifestXML)", true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			for (int i = 0; i < WorkflowTriggerActionTypeConstantsList.Count; i++)
			{
				if (WorkflowTriggerActionTypeConstantsList[i].Code != WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs
					&& WorkflowTriggerActionTypeConstantsList[i].Code != WorkflowTriggerActionTypeConstants.Codes.SendDocument
					&& WorkflowTriggerActionTypeConstantsList[i].Code != WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue
					&& WorkflowTriggerActionTypeConstantsList[i].Code != WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue
					&& WorkflowTriggerActionTypeConstantsList[i].Code != WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts
					&& WorkflowTriggerActionTypeConstantsList[i].Code != WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML
				)
				{
					ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstantsList[i].Code;
					AssertEquals("PQ_MessagePurpose should be editable when PQ_TriggerType is NOT 'EDC' (Add Document To eDocs)", false, ptNotification.PQ_MessagePurposeInfo.ReadOnly);
				}
			}
		}

		public void TestFieldReadonliness_WhenTriggerTypeIsEConversationRelated()
		{
			var triggerAction = GetNewNotificationWithParentJob();

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage;
			AssertFieldReadonlinessForEConversationTriggerAction(triggerAction);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage;
			AssertFieldReadonlinessForEConversationTriggerAction(triggerAction);
		}

		static void AssertFieldReadonlinessForEConversationTriggerAction(ProcessTaskNotification triggerAction)
		{
			AssertEquals(false, triggerAction.PQ_TriggerTypeInfo.ReadOnly);
			AssertEquals(false, triggerAction.PQ_EmailTextFallbackToTemplateInfo.ReadOnly);

			AssertEquals(true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals(true, triggerAction.PQ_TriggerPartyServiceInfo.ReadOnly);
			AssertEquals(true, triggerAction.PQ_EmailAddrInfo.ReadOnly);
			AssertEquals(true, triggerAction.PQ_MessagePurposeInfo.ReadOnly);
			AssertEquals(true, triggerAction.PQ_SU_DocumentInfo.ReadOnly);
			AssertEquals(true, triggerAction.PQ_FieldNameInfo.ReadOnly);
			AssertEquals(true, triggerAction.PQ_FieldValueInfo.ReadOnly);
			AssertEquals(true, triggerAction.PQ_P0_WorkflowTemplateInfo.ReadOnly);
			AssertEquals(true, triggerAction.PQ_SQInfo.ReadOnly);
			AssertEquals(true, triggerAction.OverrideEmailInfo.ReadOnly);
		}

		#endregion

		#region PQ_SU_Document

		public void TestPQ_SU_DocumentInfo()
		{
			ProcessTaskNotification ptNotification = Factory.New<ProcessTaskNotification>();
			AssertEquals("PQ_SU_Document should be read-only by default", true, ptNotification.PQ_SU_DocumentInfo.ReadOnly);
		}

		#endregion

		#region PQ_TriggerParty

		public void TestPQ_TriggerPartyInfo()
		{
			ProcessTaskNotification ptNotification = Factory.New<ProcessTaskNotification>();
			AssertEquals("PQ_TriggerParty should be read-only by default", true, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
		}

		#endregion

		#region PQ_TriggerPartyService

		public void TestPQ_TriggerPartyServiceInfo()
		{
			var ptNotification = Factory.New<ProcessTaskNotification>();
			AssertEquals("PQ_TriggerPartyService should be read-only by default", true, ptNotification.PQ_TriggerPartyServiceInfo.ReadOnly);
		}

		#endregion

		#region PQ_SQ_ReadOnly

		public void TestPQ_SQ_ReadOnly()
		{
			var ptNotification = Factory.New<ProcessTaskNotification>();
			AssertEquals(true, ptNotification.PQ_SQ_ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertEquals(true, ptNotification.PQ_SQ_ReadOnly);

			ptNotification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			AssertEquals(false, ptNotification.PQ_SQ_ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertEquals(true, ptNotification.PQ_SQ_ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			AssertEquals(true, ptNotification.PQ_SQ_ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			AssertEquals(true, ptNotification.PQ_SQ_ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail;
			AssertEquals(true, ptNotification.PQ_SQ_ReadOnly);

			ptNotification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery;
			AssertEquals(true, ptNotification.PQ_SQ_ReadOnly);
		}

		public void TestClearPQ_SQIfNeeded()
		{
			ProcessTaskNotification ptNotification = Factory.New<ProcessTaskNotification>();
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			ptNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			ptNotification.PQ_SQ = ZGuid.NewZGuid();
			ptNotification.PQ_Calc_TriggerParty = "";
			Assert(ptNotification.PQ_SQ.IsEmpty);

			ptNotification.PQ_SQ = ZGuid.NewZGuid();
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			Assert(ptNotification.PQ_SQ.IsEmpty);
		}

		#endregion

		#region PQ_MessagePurpose_ReadOnly

		public void TestPQ_MessagePurpose_ReadOnly()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow);

			DummyWorkflowDescriptor.Instance.WorkflowTriggerActionTypeList.AddPair("XXX", "Dummy Action");

			var dummy = Factory.New<DummyWithWorkflow>();
			var task = ((IWorkflowProvider)dummy).WorkflowItems.Tasks.AddNew();
			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			task.TriggerConditions.TriggerEventCode = "ATH";

			var ptNotification = task.ProcessTaskNotifications.AddNew();
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertEquals(false, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendARInvoice;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.GenerateARInvoiceToEdocs;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoPack;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
			AssertEquals(false, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCRESAMessage;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCIN750Message;
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			ptNotification.PQ_TriggerType = "XXX";
			AssertEquals(true, ptNotification.PQ_MessagePurposeInfo.ReadOnly);

			DummyBusinessObject.TypeDecider.TypeForLoadOverride = null;
		}

		#endregion

		#region PQ_MacroType

		public void TestPQMacroType_WhenMcrIsEnabledAndTriggerTypeIsFld_NotReadOnly()
		{
			using (WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var task = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
				var notification = task.ProcessTaskNotifications.AddNew();

				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				Assert(!notification.PQ_MacroTypeCode_ReadOnly);

				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
				Assert(!notification.PQ_MacroTypeCode_ReadOnly);
			}
		}

		public void TestPQMacroType_WhenMcrIsDisabledAndTriggerTypeIsFld_ReadOnly()
		{
			using (WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var task = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
				var notification = task.ProcessTaskNotifications.AddNew();

				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				Assert(notification.PQ_MacroTypeCode_ReadOnly);
			}
		}

		public void TestPQMacroType_WhenMcrIsEnabledAndTriggerTypeIsNotFld_ReadOnly()
		{
			using (WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var task = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
				var notification = task.ProcessTaskNotifications.AddNew();

				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies;
				Assert(notification.PQ_MacroTypeCode_ReadOnly);
			}
		}

		public void TestPQMacroType_SetValue_SetsCode1()
		{
			var task = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification.PQ_MacroTypeCode = EventReferenceConditionList.Codes.ConditionWithMacros;

			AssertEquals(EventReferenceConditionList.Codes.ConditionWithMacros, notification.PQ_Code1);
		}

		public void TestPQMacroType_CheckAttributes()
		{
			var propertyInfo = typeof(ProcessTaskNotification).GetProperty(nameof(ProcessTaskNotification.PQ_MacroTypeCode));

			var listAttribute = propertyInfo.GetCustomAttribute<ListAttribute>();
			AssertNotNull(listAttribute);
			AssertEquals("Lookups.MacroTypeCodeList", listAttribute.ListDataSourceMember);
		}

		public void TestMacroCodeFieldType_WithNullCode_IsDefault()
		{
			var task = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();

			AssertEquals(nameof(FieldType.TextMacro), notification.MacroCodeFieldType);
		}

		public void TestMacroCodeFieldType_WithMcrCode_IsAntlrMacro()
		{
			var task = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification.PQ_MacroTypeCode = EventReferenceConditionList.Codes.ConditionWithMacros;

			AssertEquals(nameof(FieldType.AntlrMacro), notification.MacroCodeFieldType);
		}

		public void TestPQMacroTypeCodeInfo_SetPQMacroTypeCode_MatchesValue()
		{
			var task = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification.PQ_MacroTypeCode = EventReferenceConditionList.Codes.ConditionWithMacros;

			AssertEquals(EventReferenceConditionList.Codes.ConditionWithMacros, notification.PQ_MacroTypeCodeInfo.Value);
		}

		#endregion

		#region PQ_P0_WorkflowTemplate

		public void TestPQ_P0_WorkflowTemplate_ReadOnly()
		{
			var task = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertEquals(true, notification.PQ_P0_WorkflowTemplate_ReadOnly);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			AssertEquals(false, notification.PQ_P0_WorkflowTemplate_ReadOnly);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			AssertEquals(false, notification.PQ_P0_WorkflowTemplate_ReadOnly);
		}

		public void TestChangeTriggerTypeToNonApplyWorkflowTemplate_ShouldClearWorkflowTemplateField()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var task = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			notification.PQ_P0_WorkflowTemplate = template.PK;
			AssertEquals(template.PK, notification.PQ_P0_WorkflowTemplate);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertEquals(ZGuid.Empty, notification.PQ_P0_WorkflowTemplate);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			notification.PQ_P0_WorkflowTemplate = template.PK;
			AssertEquals(template.PK, notification.PQ_P0_WorkflowTemplate);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertEquals(ZGuid.Empty, notification.PQ_P0_WorkflowTemplate);
		}

		#endregion

		#region TestPQ_Calc_TriggerPartyChanges

		public void TestPQ_Calc_TriggerPartyChanges()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var task = ((IWorkflowProvider)declaration).WorkflowItems.Tasks.AddNew();
			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			task.TriggerConditions.TriggerEventCode = "ATH";

			var ptNotification = task.ProcessTaskNotifications.AddNew();
			AssertEquals("ptNotification.PQ_Calc_TriggerPartyInfo.ReadOnly", true, ptNotification.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("ptNotification.PQ_TriggerPartyInfo.ReadOnly", true, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals("ptNotification.PQ_OH_RecipientInfo.ReadOnly", true, ptNotification.PQ_OH_RecipientInfo.ReadOnly);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertEquals("ptNotification.PQ_Calc_TriggerPartyInfo.ReadOnly", false, ptNotification.PQ_Calc_TriggerPartyInfo.ReadOnly);
			ptNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			AssertEquals("ptNotification.PQ_TriggerParty", MessageRecipientPartyTypeList.Codes.Print, ptNotification.PQ_TriggerParty);
			AssertEquals("ptNotification.PQ_TriggerPartyInfo.ReadOnly", true, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals("ptNotification.PQ_OH_RecipientInfo.ReadOnly", true, ptNotification.PQ_OH_RecipientInfo.ReadOnly);

			ptNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertEquals("ptNotification.PQ_TriggerParty", MessageRecipientPartyTypeList.Codes.OrgProxy, ptNotification.PQ_TriggerParty);
			AssertEquals("ptNotification.PQ_TriggerPartyInfo.ReadOnly", true, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals("ptNotification.PQ_OH_RecipientInfo.ReadOnly", true, ptNotification.PQ_OH_RecipientInfo.ReadOnly);

			ptNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
			AssertEquals("ptNotification.PQ_TriggerParty", MessageRecipientPartyTypeList.Codes.OrgProxy, ptNotification.PQ_TriggerParty);
			AssertEquals("ptNotification.PQ_TriggerPartyInfo.ReadOnly", false, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals("ptNotification.PQ_OH_RecipientInfo.ReadOnly", false, ptNotification.PQ_OH_RecipientInfo.ReadOnly);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "D@#$22";
			ptNotification.PQ_OH_Recipient = org.PK;

			ptNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			AssertEquals("ptNotification.PQ_TriggerParty", MessageRecipientPartyTypeList.Codes.Consignee, ptNotification.PQ_TriggerParty);
			AssertEquals("ptNotification.PQ_TriggerPartyInfo.ReadOnly", true, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals("ptNotification.PQ_OH_RecipientInfo.ReadOnly", true, ptNotification.PQ_OH_RecipientInfo.ReadOnly);
			AssertEquals("ptNotification.PQ_OH_Recipient", ZGuid.Empty, ptNotification.PQ_OH_Recipient);

			ptNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
			AssertEquals("ptNotification.PQ_TriggerParty", MessageRecipientPartyTypeList.Codes.Consignee, ptNotification.PQ_TriggerParty);
			AssertEquals("ptNotification.PQ_TriggerPartyInfo.ReadOnly", false, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals("ptNotification.PQ_OH_RecipientInfo.ReadOnly", false, ptNotification.PQ_OH_RecipientInfo.ReadOnly);
			AssertEquals("ptNotification.PQ_OH_Recipient", ZGuid.Empty, ptNotification.PQ_OH_Recipient);

			ptNotification.PQ_OH_Recipient = org.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var ptNotificationDiffFactory = newFactory.Load<ProcessTaskNotification>(ptNotification.PK);
			AssertEquals("ptNotificationDiffFactory.PQ_TriggerType", WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, ptNotificationDiffFactory.PQ_TriggerType);
			AssertEquals("ptNotificationDiffFactory.PQ_Calc_TriggerParty", MessageRecipientPartyTypeList.SpecialCodes.Other, ptNotificationDiffFactory.PQ_Calc_TriggerParty);
			AssertEquals("ptNotificationDiffFactory.PQ_TriggerParty", MessageRecipientPartyTypeList.Codes.Consignee, ptNotificationDiffFactory.PQ_TriggerParty);
			AssertEquals("ptNotificationDiffFactory.PQ_TriggerPartyInfo.ReadOnly", false, ptNotificationDiffFactory.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals("ptNotificationDiffFactory.PQ_OH_RecipientInfo.ReadOnly", false, ptNotificationDiffFactory.PQ_OH_RecipientInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region New Properties

		public void TestTriggerPartyIsRequired()
		{
			var ptNotification = GetNewNotificationWithParentJob();
			// true
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerParty = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLWithAWB;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallbackSimplified;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionBatchXML;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertEquals(true, ptNotification.TriggerPartyIsRequired);
			// false
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertEquals(false, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage;
			AssertEquals(false, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendFormBuilderXml;
			AssertEquals(false, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.StartDestinationPortClearanceProcess;
			AssertEquals(false, ptNotification.TriggerPartyIsRequired);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.RecognizeRevenue;
			AssertEquals(false, ptNotification.TriggerPartyIsRequired);
		}

		public void TestIsTriggerPartyServiceAvailable()
		{
			var ptNotification = GetNewNotificationWithParentJob();
			foreach (string code in WorkflowTriggerActionTypeConstantsList.GetAllCodes())
			{
				ptNotification.PQ_Calc_TriggerParty = "foo";
				ptNotification.PQ_TriggerPartyService = "bar";
				ptNotification.PQ_TriggerType = code;
				var expectedAvailablity = WorkflowTriggerActionTypeConstants.IsXmlUniversalShipment(ptNotification.PQ_TriggerType);
				AssertEquals($"IsTriggerPartyServiceAvailable should be true when PQ_TriggerType=XUS, not {code}.", expectedAvailablity, ptNotification.IsTriggerPartyServiceAvailable);
			}
		}

		#region OverrideEmail

		public void TestOverrideEmail()
		{
			ProcessTask parentTask = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTaskNotificationForTest ptNotification = Factory.NewWithValidTestData<ProcessTaskNotificationForTest>();
			ptNotification.PQ_P9 = parentTask.PK;
			AssertEquals("Precondition", true, ptNotification.OverrideEmailInfo.ReadOnly);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			AssertEquals("OverrideEmail should become editable when PQ_TriggerType changes to 'NTF' (notification email)", false, ptNotification.OverrideEmailInfo.ReadOnly);
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail;
			AssertEquals("OverrideEmail should become editable when PQ_TriggerType changes to 'NBT' (notification email [body-only])", false, ptNotification.OverrideEmailInfo.ReadOnly);
			ptNotification.OverrideEmail = true;
			AssertEquals("OverrideEmail setter should set PQ_EmailText to 'Enter email notification text' when OverrideEmail changes its value from false to true", "Enter email notification text", ptNotification.PQ_EmailText);

			for (int i = 0; i < WorkflowTriggerActionTypeConstantsList.Count; i++)
			{
				ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstantsList[i].Code;
				if (ptNotification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.NotificationEmail)
				{
					AssertEquals("OverrideEmail should be editable when PQ_TriggerType is 'NTF' (notification email)", false, ptNotification.OverrideEmailInfo.ReadOnly);
				}
				else if (ptNotification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail)
				{
					AssertEquals("OverrideEmail should be editable when PQ_TriggerType is 'NBT' (notification email [body-only])", false, ptNotification.OverrideEmailInfo.ReadOnly);
				}
				else
				{
					AssertEquals("OverrideEmail should be read-only when PQ_TriggerType is not 'NTF' (notification email) or 'NBT' (notification email [body-only])", true, ptNotification.OverrideEmailInfo.ReadOnly);
				}
			}

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			ptNotification.OverrideEmail = true;
			AssertEquals("ProcessTaskNotification.OverrideEmail setter/getter check", true, ptNotification.OverrideEmail);
			ptNotification.PQ_EmailText = "some email text";
			ptNotification.OverrideEmail = false;
			AssertEquals("ProcessTaskNotification.OverrideEmail setter/getter check", false, ptNotification.OverrideEmail);
			AssertEquals("PQ_EmailText should be cleared when OverrideEmail becomes false", ZString.Empty, ptNotification.PQ_EmailText);

			parentTask.P9_ParentTableCode = ProcessTaskTemplateSchema.Constants.Prefix;
			ptNotification = Factory.NewWithValidTestData<ProcessTaskNotificationForTest>();
			ptNotification.PQ_TriggerType = "foo";
			ptNotification.PQ_P9 = parentTask.PK;
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			AssertEquals("OverrideEmail should be true ProcessTaskNotification belongs to template", true, ptNotification.OverrideEmail);
		}

		public void TestEmailText_MacroEvaluation()
		{
			var dummyTask = Dummy.WorkflowItems.Milestones.AddNew();
			var notification = dummyTask.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "(*Logs.AddedLog.User.GS_Code*)@boggo.com";
			var addedLog = dummyTask.Logs.AddedLog;

			AssertNotNull(addedLog);
			AssertEquals(GlbStaff.CurrentUser.GS_Code + "@boggo.com", TriggerActionCommunicationModeSubstitutor.Substitute(notification, Dummy, null, MessageDelivery.CommunicationModeSubstitutorProperty.EmailAddress, notification.PQ_EmailAddr));
			Dummy.Factory.Save();

			Assert("ADD/EDT are non persisted for ProcessTask", !addedLog.IsInDatabase);
			AssertEquals("AddedLog is non persisted so we can't access it here (users need to change these macros since ADD events are being deprecated)", "@boggo.com", TriggerActionCommunicationModeSubstitutor.Substitute(notification, Dummy, null, MessageDelivery.CommunicationModeSubstitutorProperty.EmailAddress, notification.PQ_EmailAddr));
		}

		#endregion

		#region Field Name and Value

		public void TestFieldNameAndValue()
		{
			ProcessTaskNotification notification = Factory.New<ProcessTaskNotification>();

			AssertNotEquals("Precondition", WorkflowTriggerActionTypeConstants.Codes.SetField, notification.PQ_TriggerType);

			notification.PQ_FieldName = "Abcd";
			notification.PQ_FieldValue = "Xyz";

			AssertEquals(ZString.Empty, notification.PQ_FieldName);
			AssertEquals(ZString.Empty, notification.PQ_FieldValue);
			Assert(notification.PQ_FieldNameInfo.ReadOnly);
			Assert(notification.PQ_FieldValueInfo.ReadOnly);

			AssertEquals("Abcd", notification.PQ_EmailAddr);
			AssertEquals("Xyz", notification.PQ_EmailText);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;

			AssertEquals(ZString.Empty, notification.PQ_FieldName);
			AssertEquals(ZString.Empty, notification.PQ_FieldValue);
			Assert(notification.PQ_FieldNameInfo.ReadOnly);
			Assert(notification.PQ_FieldValueInfo.ReadOnly);

			AssertEquals(ZString.Empty, notification.PQ_EmailAddr);
			AssertEquals(ZString.Empty, notification.PQ_EmailText);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			AssertEquals("Abcd", notification.PQ_FieldName);
			AssertEquals("Xyz", notification.PQ_FieldValue);
			Assert(!notification.PQ_FieldNameInfo.ReadOnly);
			Assert(!notification.PQ_FieldValueInfo.ReadOnly);

			AssertEquals(ZString.Empty, notification.PQ_EmailAddr);
			AssertEquals(ZString.Empty, notification.PQ_EmailText);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;

			AssertEquals("Abcd", notification.PQ_FieldName);
			AssertEquals("Xyz", notification.PQ_FieldValue);
			Assert(!notification.PQ_FieldNameInfo.ReadOnly);
			Assert(!notification.PQ_FieldValueInfo.ReadOnly);

			AssertEquals(ZString.Empty, notification.PQ_EmailAddr);
			AssertEquals(ZString.Empty, notification.PQ_EmailText);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;

			AssertEquals("Abcd", notification.PQ_FieldName);
			AssertEquals(ZString.Empty, notification.PQ_FieldValue);
			Assert(!notification.PQ_FieldNameInfo.ReadOnly);
			Assert(notification.PQ_FieldValueInfo.ReadOnly);

			AssertEquals(ZString.Empty, notification.PQ_EmailAddr);
			AssertEquals(ZString.Empty, notification.PQ_EmailText);
		}

		#endregion

		#region Action Reference

		public void TestActionReferenceAndOffset()
		{
			var notification = Factory.New<ProcessTaskNotification>();

			AssertNotEquals("Precondition", WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent, notification.PQ_TriggerType);

			notification.PQ_ActionReference = "Abcd";

			AssertEquals(ZString.Empty, notification.PQ_ActionReference);
			Assert(notification.PQ_ActionReferenceInfo.ReadOnly);
			Assert(notification.PQ_OffsetInfo.ReadOnly);

			AssertEquals("Abcd", notification.PQ_EmailAddr);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			AssertEquals(ZString.Empty, notification.PQ_ActionReference);
			Assert(notification.PQ_ActionReferenceInfo.ReadOnly);
			Assert(notification.PQ_OffsetInfo.ReadOnly);

			AssertEquals(ZString.Empty, notification.PQ_EmailAddr);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;

			AssertEquals(ZString.Empty, notification.PQ_ActionReference);
			Assert(notification.PQ_ActionReferenceInfo.ReadOnly);
			Assert(notification.PQ_OffsetInfo.ReadOnly);

			AssertEquals(ZString.Empty, notification.PQ_EmailAddr);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;

			AssertEquals("Abcd", notification.PQ_ActionReference);
			Assert(!notification.PQ_ActionReferenceInfo.ReadOnly);
			Assert(!notification.PQ_OffsetInfo.ReadOnly);

			AssertEquals(ZString.Empty, notification.PQ_EmailAddr);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.EnrolInWiseTechAcademyCourse;

			AssertEquals("Abcd", notification.PQ_ActionReference);
			Assert(!notification.PQ_ActionReferenceInfo.ReadOnly);
			Assert(notification.PQ_OffsetInfo.ReadOnly);

			AssertEquals(ZString.Empty, notification.PQ_EmailAddr);
		}

		#endregion

		#region Offset

		public void TestClearPQ_OffsetIfNeeded()
		{
			ProcessTaskNotification ptNotification = Factory.New<ProcessTaskNotification>();
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;
			ptNotification.PQ_Offset = new ZDateTime(2022, 1, 1);

			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			Assert(ptNotification.PQ_Offset.IsEmpty);
		}

		#endregion

		#endregion

		#region Set Default Values

		public void TestSetDefaultValues()
		{
			ProcessTaskNotificationForTest ptNotification = Factory.NewWithValidTestData<ProcessTaskNotificationForTest>();

			AssertEquals(true, ptNotification.PQ_SU_DocumentInfo.ReadOnly);
			AssertEquals(true, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals(false, ptNotification.Get_overrideEmail());
		}

		#endregion

		#region Clone, CopyPersistentValuesFrom and CopyValuesFrom

		public void TestClone()
		{
			ProcessTask task = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTaskNotification pTnotification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			pTnotification.PQ_P9 = task.PK;
			Factory.Save();

			pTnotification.PQ_TriggerType = "";
			pTnotification.PQ_TriggerParty = "STA";
			pTnotification.PQ_TriggerPartyService = "SVC";
			pTnotification.PQ_EmailText = "this should not be cloned";
			ZGuid pQ_SU_Document = ZGuid.NewZGuid();
			pTnotification.PQ_SU_Document = pQ_SU_Document;
			pTnotification.PQ_MessagePurpose = "SHP";

			ProcessTaskNotification clonedNotifcation = (ProcessTaskNotification)pTnotification.Clone();
			AssertEquals("PQ_P9 should be left empty for clone", ZGuid.Empty, clonedNotifcation.PQ_P9);
			AssertEquals("PQ_TriggerType should be copied to clone", "", clonedNotifcation.PQ_TriggerType);
			AssertEquals("PQ_TriggerParty should be copied to clone", "STA", clonedNotifcation.PQ_TriggerParty);
			AssertEquals("PQ_TriggerPartyService should be copied to clone", "SVC", clonedNotifcation.PQ_TriggerPartyService);
			AssertEquals("PQ_EmailText should be left empty for clone", ZString.Empty, clonedNotifcation.PQ_EmailText);
			AssertEquals("PQ_SU_Document should be copied to clone", pQ_SU_Document, clonedNotifcation.PQ_SU_Document);
			AssertEquals("PQ_MessagePurpose should be copied to clone", "SHP", clonedNotifcation.PQ_MessagePurpose);
		}

		public void TestCloneSetField()
		{
			ProcessTaskNotification notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			Factory.Save();

			notification.PQ_TriggerType = "";
			notification.PQ_EmailAddr = "Some Field";
			notification.PQ_EmailText = "Some Value";
			ProcessTaskNotification clonedNotification = (ProcessTaskNotification)notification.Clone();

			AssertEquals("", clonedNotification.PQ_TriggerType);
			AssertEquals("Some Field", clonedNotification.PQ_EmailAddr);
			AssertEquals(ZString.Empty, clonedNotification.PQ_EmailText);
			AssertEquals(ZString.Empty, clonedNotification.PQ_FieldName);
			AssertEquals(ZString.Empty, clonedNotification.PQ_FieldValue);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			clonedNotification = (ProcessTaskNotification)notification.Clone();

			AssertEquals(WorkflowTriggerActionTypeConstants.Codes.SetField, clonedNotification.PQ_TriggerType);
			AssertEquals(ZString.Empty, clonedNotification.PQ_EmailAddr);
			AssertEquals(ZString.Empty, clonedNotification.PQ_EmailText);
			AssertEquals("Some Field", clonedNotification.PQ_FieldName);
			AssertEquals("Some Value", clonedNotification.PQ_FieldValue);

			clonedNotification.PQ_TriggerType = "";

			AssertEquals("Some Field", clonedNotification.PQ_EmailAddr);
			AssertEquals("Some Value", clonedNotification.PQ_EmailText);
			AssertEquals(ZString.Empty, clonedNotification.PQ_FieldName);
			AssertEquals(ZString.Empty, clonedNotification.PQ_FieldValue);
		}

		public void TestCloneScheduleEvent()
		{
			ProcessTaskNotification notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			Factory.Save();

			notification.PQ_TriggerType = "";
			notification.PQ_EmailAddr = "Some Reference";
			ProcessTaskNotification clonedNotification = (ProcessTaskNotification)notification.Clone();

			AssertEquals("", clonedNotification.PQ_TriggerType);
			AssertEquals("Some Reference", clonedNotification.PQ_EmailAddr);
			AssertEquals(ZString.Empty, clonedNotification.PQ_ActionReference);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;
			clonedNotification = (ProcessTaskNotification)notification.Clone();

			AssertEquals(WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent, clonedNotification.PQ_TriggerType);
			AssertEquals(ZString.Empty, clonedNotification.PQ_EmailAddr);
			AssertEquals("Some Reference", clonedNotification.PQ_ActionReference);

			clonedNotification.PQ_TriggerType = "";

			AssertEquals("Some Reference", clonedNotification.PQ_EmailAddr);
			AssertEquals(ZString.Empty, clonedNotification.PQ_ActionReference);
		}

		public void TestCloneFieldNameForApplyTagTriggerType()
		{
			ProcessTaskNotification notification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			Factory.Save();

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			notification.PQ_EmailAddr = "Filed Nmae";
			ProcessTaskNotification clonedNotification = (ProcessTaskNotification)notification.Clone();

			AssertEquals("Filed Nmae", clonedNotification.PQ_FieldName);
		}

		#endregion

		#region OnLoaded

		public void TestOnLoaded()
		{
			ProcessTaskNotificationForTest ptNotification = Factory.NewWithValidTestData<ProcessTaskNotificationForTest>();
			ZGuid ptNotification_PK = ptNotification.PK;
			Factory.Save();
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			ptNotification = otherFactory.Load<ProcessTaskNotificationForTest>(ptNotification_PK);

			AssertEquals(true, ptNotification.PQ_SU_DocumentInfo.ReadOnly);
			AssertEquals(true, ptNotification.PQ_TriggerPartyInfo.ReadOnly);
			AssertEquals(true, ptNotification.PQ_TriggerPartyServiceInfo.ReadOnly);

			AssertOverrideEmail(otherFactory, ptNotification, "OverrideEmail should be false when PQ_EmailText is null", (ZString)null, false);
			AssertOverrideEmail(otherFactory, ptNotification, "OverrideEmail should be false when PQ_EmailText is blank (empty string)", ZString.Empty, false);
			AssertOverrideEmail(otherFactory, ptNotification, "OverrideEmail should be true when PQ_EmailText is not empty", "some email text", true);
		}

		#endregion

		#region OnSaving

		public void TestOnSaving()
		{
			ProcessTaskNotificationForTest ptNotification = Factory.NewWithValidTestData<ProcessTaskNotificationForTest>();
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			AssertEquals("Precondition", false, ptNotification.OverrideEmailInfo.ReadOnly);
			ptNotification.OverrideEmail = true;
			AssertEquals("Precondition", "Enter email notification text", ptNotification.PQ_EmailText);
			Factory.Save();
			AssertEquals("If PQ_EmailText = 'Enter email notification text' then PQ_EmailText should be set to empty string on saving", ZString.Empty, ptNotification.PQ_EmailText);

			ptNotification = Factory.NewWithValidTestData<ProcessTaskNotificationForTest>();
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail;
			AssertEquals("Precondition", false, ptNotification.OverrideEmailInfo.ReadOnly);
			ptNotification.OverrideEmail = true;
			AssertEquals("Precondition", "Enter email notification text", ptNotification.PQ_EmailText);
			Factory.Save();
			AssertEquals("If PQ_EmailText = 'Enter email notification text' then PQ_EmailText should be set to empty string on saving", ZString.Empty, ptNotification.PQ_EmailText);
		}

		#endregion

		#region Milestones and Exceptions

		public void TestTemplateNotificationComparerShouldIgnoreEmailAddrWhenTriggerPartyIsPersonRelatedEmailTriggerParty()
		{
			var p = Factory.New<ProcessTask>();
			p.IsWorkflowTrigger = true;
			var p1 = p.ProcessTaskNotifications.AddNew();
			p1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			var p2 = p.ProcessTaskNotifications.AddNew();
			p2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			Assert(p1.IsMatchForItemTemplateMerge(p2));

			p1.PQ_EmailAddr = "test@test.com";
			Assert(p1.IsMatchForItemTemplateMerge(p2));

			p1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail;
			p2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail;
			Assert(p1.IsMatchForItemTemplateMerge(p2));

			p1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail;
			p2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail;
			Assert(p1.IsMatchForItemTemplateMerge(p2));

			p1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			Assert(!p1.IsMatchForItemTemplateMerge(p2));
		}

		public void TestHumanReadableName()
		{
			var processTask = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
			processTask.P9_Description = "Jerple";
			var notification = processTask.ProcessTaskNotifications.AddNew();

			processTask.P9_Type = "TRG";
			AssertEquals("Trigger Action - Jerple", notification.HumanReadableName);

			processTask.P9_Type = "MIL";
			AssertEquals("Milestone Action - Jerple", notification.HumanReadableName);

			processTask.P9_Type = "";
			AssertEquals("Action - Jerple", notification.HumanReadableName);

			processTask.Delete();
			AssertEquals("ProcessTaskNotification", notification.HumanReadableName);
		}

		public void TestEmailAddressIsNotMatch()
		{
			var p = Factory.New<ProcessTask>();
			p.IsWorkflowTrigger = true;
			var p1 = p.ProcessTaskNotifications.AddNew();
			var p2 = p.ProcessTaskNotifications.AddNew();
			Assert(p1.IsMatchForItemTemplateMerge(p2));
			p1.PQ_EmailAddr = "bodgy";
			Assert(!p1.IsMatchForItemTemplateMerge(p2));
		}

		public void TestIsMatchForItemTemplateMerge()
		{
			ZGuid rightZGuid = ZGuid.NewZGuid();
			ZGuid wrongZGuid = ZGuid.NewZGuid();
			AssertNotEquals("Precondition", rightZGuid, wrongZGuid);
			ZString rightZString = "STA";
			ZString wrongZString = "DUM";

			ProcessTask pT = Factory.NewWithValidTestData<ProcessTask>();
			DummyWithWorkflow dummyJob = Factory.New<DummyWithWorkflow>();
			pT.P9_ParentID = dummyJob.PK;
			pT.P9_Type = Core.Constants.Workflow.MilestoneType;
			pT.TriggerConditions.TriggerEventCode = rightZString;
			pT.TemplateConditions.TemplateCondition1 = rightZString;
			pT.TemplateConditions.TemplateCondition2 = rightZString;
			ProcessTaskNotification pTNotification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			pTNotification.PQ_P9 = pT.PK;
			pTNotification.PQ_TriggerType = rightZString;
			pTNotification.PQ_SU_Document = rightZGuid;
			pTNotification.PQ_TriggerParty = rightZString;
			pTNotification.PQ_TriggerPartyService = rightZString;
			pTNotification.PQ_MessagePurpose = rightZString;

			ProcessTask pTFromTemplate = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTaskTemplate pTTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			pTFromTemplate.P9_ParentID = pTTemplate.PK;
			pTFromTemplate.P9_Type = Core.Constants.Workflow.MilestoneType;
			pTFromTemplate.TriggerConditions.TriggerEventCode = rightZString;
			pTFromTemplate.TemplateConditions.TemplateCondition1 = rightZString;
			pTFromTemplate.TemplateConditions.TemplateCondition2 = rightZString;
			ProcessTaskNotification pTNotificationFromTemplate = Factory.NewWithValidTestData<ProcessTaskNotification>();
			pTNotificationFromTemplate.PQ_P9 = pTFromTemplate.PK;
			pTNotificationFromTemplate.PQ_TriggerType = rightZString;
			pTNotificationFromTemplate.PQ_SU_Document = rightZGuid;
			pTNotificationFromTemplate.PQ_TriggerParty = rightZString;
			pTNotificationFromTemplate.PQ_TriggerPartyService = rightZString;
			pTNotificationFromTemplate.PQ_MessagePurpose = rightZString;

			AssertEquals("should return true when all conditions are met (see list of conditions in further asserts in this test's code)", true, pTNotification.IsMatchForItemTemplateMerge(pTNotificationFromTemplate));

			pT.P9_Type = wrongZString;
			AssertEquals("Let's not be stupid and check the parent when merging notifications since we should have already done that before calling this...",
				true, pTNotification.IsMatchForItemTemplateMerge(pTNotificationFromTemplate));

			pT.P9_Type = Core.Constants.Workflow.MilestoneType;
			pTNotificationFromTemplate.PQ_TriggerType = wrongZString;
			AssertEquals("should return false if not PQ_TriggerType == itemTemplate.PQ_TriggerType", false, pTNotification.IsMatchForItemTemplateMerge(pTNotificationFromTemplate));

			pTNotificationFromTemplate.PQ_TriggerType = rightZString;
			pTNotificationFromTemplate.PQ_SU_Document = wrongZGuid;
			AssertEquals("should return false if not PQ_SU_Document == itemTemplate.PQ_SU_Document", false, pTNotification.IsMatchForItemTemplateMerge(pTNotificationFromTemplate));

			pTNotificationFromTemplate.PQ_SU_Document = rightZGuid;
			pTNotificationFromTemplate.PQ_TriggerParty = wrongZString;
			AssertEquals("should return false if not PQ_TriggerParty == itemTemplate.PQ_TriggerParty", false, pTNotification.IsMatchForItemTemplateMerge(pTNotificationFromTemplate));

			pTNotificationFromTemplate.PQ_TriggerParty = rightZString;
			pTNotificationFromTemplate.PQ_TriggerPartyService = wrongZString;
			AssertEquals("should return false if not PQ_TriggerPartyService == itemTemplate.PQ_TriggerPartyService", false, pTNotification.IsMatchForItemTemplateMerge(pTNotificationFromTemplate));

			pTNotificationFromTemplate.PQ_TriggerPartyService = rightZString;
			pTNotificationFromTemplate.PQ_MessagePurpose = wrongZString;
			AssertEquals("should return false if not PQ_MessagePurpose == itemTemplate.PQ_MessagePurpose", false, pTNotification.IsMatchForItemTemplateMerge(pTNotificationFromTemplate));
		}

		public void TestIsMatchForItemTemplateMerge_Triggers()
		{
			ZGuid document1 = ZGuid.NewZGuid();
			ZGuid document2 = ZGuid.NewZGuid();
			ZGuid tradeAgree1 = ZGuid.NewZGuid();
			ZGuid tradeAgree2 = ZGuid.NewZGuid();

			TemplateProcessTask triggerTemplate = Factory.New<TemplateProcessTask>();
			triggerTemplate.IsWorkflowTrigger = true;
			triggerTemplate.TriggerConditions.TriggerEventCode = Events.PickedUp.Code;
			ProcessTaskNotification actionTemplate1 = AddNewTriggerAction(triggerTemplate, "DOC", document1, "BTP", "APP");
			ProcessTaskNotification actionTemplate2 = AddNewTriggerAction(triggerTemplate, "DOC", document2, "CON", "INV");
			ProcessTaskNotification actionTemplate3 = AddNewTriggerAction(triggerTemplate, "XML", ZGuid.Empty, "CNE", "");
			ProcessTaskNotification actionTemplate4 = AddNewTriggerAction(triggerTemplate, "XML", ZGuid.Empty, "ORP", "");
			ProcessTaskNotification actionTemplate5 = AddNewTriggerAction(triggerTemplate, "NTF", ZGuid.Empty, "ORP", "");

			TemplateProcessTask milestoneTemplate = Factory.New<TemplateProcessTask>();
			milestoneTemplate.IsMilestone = true;
			milestoneTemplate.TriggerConditions.TriggerEventCode = Events.PickedUp.Code;
			ProcessTaskNotification actionTemplate6 = AddNewTriggerAction(milestoneTemplate, "DOC", document2, "CON", "INV");

			TemplateProcessTask anotherTriggerTemplate = Factory.New<TemplateProcessTask>();
			anotherTriggerTemplate.IsWorkflowTrigger = true;
			anotherTriggerTemplate.TriggerConditions.TriggerEventCode = Events.Delivered.Code;
			ProcessTaskNotification actionTemplate7 = AddNewTriggerAction(anotherTriggerTemplate, "DOC", document2, "CON", "INV");

			ProcessTask trigger = Factory.New<ProcessTask>();
			trigger.IsWorkflowTrigger = true;
			trigger.TriggerConditions.TriggerEventCode = Events.PickedUp.Code;
			ProcessTaskNotification triggerAction = AddNewTriggerAction(trigger, "DOC", document2, "CON", "INV");
			Assert(!triggerAction.IsMatchForItemTemplateMerge(actionTemplate1));
			Assert(triggerAction.IsMatchForItemTemplateMerge(actionTemplate2));
			Assert(!triggerAction.IsMatchForItemTemplateMerge(actionTemplate3));
			Assert(!triggerAction.IsMatchForItemTemplateMerge(actionTemplate4));
			Assert(!triggerAction.IsMatchForItemTemplateMerge(actionTemplate5));
			Assert("We should have already compared the parents before comparing the children.", triggerAction.IsMatchForItemTemplateMerge(actionTemplate6));
			Assert("We should have already compared the parents before comparing the children.", triggerAction.IsMatchForItemTemplateMerge(actionTemplate7));
		}

		public void TestIsMatchForItemTemplateMerge_SetField()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.IsWorkflowTrigger = true;

			ProcessTaskNotification notification1 = Factory.New<ProcessTaskNotification>();
			notification1.PQ_P9 = task.PK;
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification1.PQ_FieldName = "Field 1";
			notification1.PQ_FieldValue = "Value 1";

			ProcessTaskNotification notification2 = Factory.New<ProcessTaskNotification>();
			notification2.PQ_P9 = task.PK;
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification2.PQ_FieldName = "Field 2";
			notification2.PQ_FieldValue = "Value 2";

			ProcessTaskNotification notification3 = Factory.New<ProcessTaskNotification>();
			notification3.PQ_P9 = task.PK;
			notification3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification3.PQ_FieldName = "Field 1";
			notification3.PQ_FieldValue = "Value 3";

			Assert(notification1.IsMatchForItemTemplateMerge(notification1));
			Assert(!notification1.IsMatchForItemTemplateMerge(notification2));
			Assert(notification1.IsMatchForItemTemplateMerge(notification3));
		}

		public void TestIsMatchForItemTemplateMerge_Printer()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.IsWorkflowTrigger = true;

			ProcessTaskNotification notification1 = Factory.New<ProcessTaskNotification>();
			notification1.PQ_P9 = task.PK;
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			notification1.PQ_SQ = ZGuid.NewZGuid();

			ProcessTaskNotification notification2 = Factory.New<ProcessTaskNotification>();
			notification2.PQ_P9 = task.PK;
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification2.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			notification2.PQ_SQ = ZGuid.NewZGuid();

			ProcessTaskNotification notification3 = Factory.New<ProcessTaskNotification>();
			notification3.PQ_P9 = task.PK;
			notification3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification3.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			notification3.PQ_SQ = notification1.PQ_SQ;

			Assert(notification1.IsMatchForItemTemplateMerge(notification1));
			Assert("Different printer", !notification1.IsMatchForItemTemplateMerge(notification2));
			Assert(notification1.IsMatchForItemTemplateMerge(notification3));
		}

		ProcessTaskNotification AddNewTriggerAction(ProcessTask triggerOrMilestone, string triggerType, ZGuid document, string triggerParty, string purpose)
		{
			ProcessTaskNotification action = triggerOrMilestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = triggerType;
			action.PQ_SU_Document = document;
			action.PQ_TriggerParty = triggerParty;
			action.PQ_MessagePurpose = purpose;
			return action;
		}

		#region TestIsMatchForItemTemplateMerge_ConsistentOnParentAndChild

		public void TestIsMatchForItemTemplateMerge_ConsistentOnParentAndChild_Simpler()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			var templateTrigger = Factory.NewWithValidTestData<ProcessTask>();
			templateTrigger.P9_ParentID = template.PK;
			templateTrigger.P9_ParentTableCode = template.TablePrefix;
			templateTrigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			templateTrigger.TriggerConditions.TriggerEventCode = Events.Authorised.Code;

			var templateTriggerAction = Factory.New<ProcessTaskNotification>();
			templateTriggerAction.PQ_P9 = templateTrigger.PK;
			templateTriggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;

			var jobTrigger = Factory.New<ProcessTask>();
			jobTrigger.IsWorkflowTrigger = true;
			jobTrigger.TriggerConditions.TriggerEventCode = Events.Authorised.Code;

			var jobTriggerAction = Factory.New<ProcessTaskNotification>();
			jobTriggerAction.PQ_P9 = jobTrigger.PK;
			jobTriggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;

			Factory.Save();

			var triggersToReload = new ProcessTask[] { templateTrigger, jobTrigger };

			var fieldsForMatching = new string[] { ProcessTasksSchema.Constants.P9_Description, "TemplateConditions.TemplateCondition1", "TemplateConditions.TemplateCondition2", "TriggerConditions.TriggerCondition", ProcessTasksSchema.Constants.P9_RespondToCascadedEvents, ProcessTasksSchema.Constants.P9_CascadedEventsContext, "TemplateConditions.TemplateCondition2Value", "TriggerConditions.TriggerConditionValue" };
			foreach (var fieldName in fieldsForMatching)
			{
				triggersToReload.ToList().ForEach(t => t.Reload());
				SetOtherFieldsIfNecessary(templateTrigger, jobTrigger, fieldName);
				SetFieldToDifferentValue(templateTrigger, jobTrigger, fieldName);
				AssertIsMatchForItemTemplateMerge(string.Format("WHEN templateTrigger.{0} != jobTrigger.{0} THEN IsMatchForItemTemplateMerge should be false", fieldName), templateTrigger, jobTrigger, false);
			}

			var ignoredFieldsForMatching = new string[] { ProcessTasksSchema.Constants.P9_LineTriggerType, ProcessTasksSchema.Constants.P9_EstimatedDefaultedFrom };
			foreach (var fieldName in ignoredFieldsForMatching)
			{
				triggersToReload.ToList().ForEach(t => t.Reload());
				SetOtherFieldsIfNecessary(templateTrigger, jobTrigger, fieldName);
				SetFieldToDifferentValue(templateTrigger, jobTrigger, fieldName);
				AssertIsMatchForItemTemplateMerge(string.Format("WHEN templateTrigger.{0} != jobTrigger.{0} THEN IsMatchForItemTemplateMerge should be true", fieldName), templateTrigger, jobTrigger, true);
			}
		}

		void SetFieldToDifferentValue(ProcessTask templateTrigger, ProcessTask jobTrigger, string fieldName)
		{
			if (ReflectionExtensions.IsString(templateTrigger, fieldName))
			{
				ReflectionExtensions.SetPropertyValue(templateTrigger, fieldName, new ZString("ABC"));
				ReflectionExtensions.SetPropertyValue(jobTrigger, fieldName, new ZString("DEF"));
			}
			else if (ReflectionExtensions.IsBool(templateTrigger, fieldName))
			{
				ReflectionExtensions.SetPropertyValue(templateTrigger, fieldName, new ZBool(true));
				ReflectionExtensions.SetPropertyValue(jobTrigger, fieldName, new ZBool(false));
			}
			else
			{
				throw new NotImplementedException("type is not implemented");
			}
		}

		void SetOtherFieldsIfNecessary(ProcessTask templateTrigger, ProcessTask jobTrigger, string fieldName)
		{
			if (fieldName == ProcessTasksSchema.Constants.P9_CascadedEventsContext)
			{
				templateTrigger.P9_RespondToCascadedEvents = true;
				jobTrigger.P9_RespondToCascadedEvents = true;
			}
			else if (fieldName == "TemplateConditions.TemplateCondition2Value")
			{
				templateTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				jobTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			}
			else if (fieldName == "TriggerConditions.TriggerConditionValue")
			{
				templateTrigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
				jobTrigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			}
		}

		void AssertIsMatchForItemTemplateMerge(string message, ProcessTask templateTrigger, ProcessTask jobTrigger, bool expected)
		{
			AssertEquals(string.Format("{0}: jobTrigger.IsMatchForItemTemplateMerge should return {1}", message, expected), expected, jobTrigger.IsMatchForItemTemplateMerge(templateTrigger));
		}

		#endregion

		#endregion

		#region TestRefreshReadOnlyForAllProperties

		public void TestRefreshReadOnlyForAllProperties()
		{
			ProcessTaskNotificationForTest notification = Factory.New<ProcessTaskNotificationForTest>();
			notification.PQ_SU_Document = ZGuid.NewZGuid();
			notification.PQ_TriggerParty = "Blh";
			notification.PQ_TriggerPartyService = "Blh";
			notification.PQ_Code1 = "Blh";
			notification.PQ_Offset = new ZDateTime(2022, 1, 1);
			notification.RefreshReadOnlyForAllProperties();

			AssertEquals("TestRefreshReadOnlyForAllProperties should call ClearPQ_SU_DocumentIfNeeded", ZGuid.Empty, notification.PQ_SU_Document);
			AssertEquals("TestRefreshReadOnlyForAllProperties should call ClearPQ_TriggerPartyIfNeeded", ZString.Empty, notification.PQ_TriggerParty);
			AssertEquals("TestRefreshReadOnlyForAllProperties should call ClearPQ_TriggerPartyServiceIfNeeded", ZString.Empty, notification.PQ_TriggerPartyService);
			Assert("TestRefreshReadOnlyForAllProperties should call SetOverrideEmailLocalVariable", notification.SetOverrideEmailLocalVariable_WasCalled);
			AssertEquals("TestRefreshReadOnlyForAllProperties should call ClearPQ_MessagePurposeIfNeeded", ZString.Empty, notification.PQ_MessagePurpose);
			AssertEquals("TestRefreshReadOnlyForAllProperties should call ClearPQ_MacroTypeCodeIfNeeded", ZString.Empty, notification.PQ_MacroTypeCode);
			AssertEquals("TestRefreshReadOnlyForAllProperties should call ClearPQ_PQ_OffsetIfNeeded", ZDateTime.Empty, notification.PQ_Offset);
		}

		public void TestRefreshReadOnlyForAllProperties_WithTriggerTypeSetField_SetsMacroTypeCodeToDefault()
		{
			using (WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var notification = Factory.New<ProcessTaskNotificationForTest>();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				notification.PQ_MacroTypeCode = EventReferenceConditionList.Codes.ConditionWithMacros;

				notification.RefreshReadOnlyForAllProperties();
				AssertNotNullOrEmpty(notification.PQ_MacroTypeCode);
			}
		}

		#endregion

		#region Implementation

		#region Dummies

		void FireTriggerAction(BusinessObject triggerJob, IBaseTrigger trigger, ProcessTaskNotification action, INotifications notifications = null)
		{
			using (WorkflowTriggerActionTracker.TrackTriggerActions(trigger.Factory))
			{
				ObjectFactory.Get<IWorkflowTriggerActionProcessorCreator>().GetSetFieldProcessor(new WorkflowTriggerActionSource(triggerJob, trigger, action, new ExampleLog(action), null)).Process(notifications);
				WorkflowTriggerActionTracker.OnAllActionsRun(trigger.Factory);
			}
		}

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;

		ProcessTaskTemplate WorkflowTemplate
		{
			get
			{
				if (workflowTemplate == null)
				{
					BusinessObjectFactory taskTemplateFactory = new BusinessObjectFactory();
					workflowTemplate = taskTemplateFactory.NewWithValidTestData<ProcessTaskTemplate>();
					workflowTemplate.P0_ProcessType = Dummy.WorkflowItems.WorkflowType;

					ProcessTask task = workflowTemplate.WorkflowItems.AddNew();
					ProcessTask milestone = workflowTemplate.WorkflowItems.AddNew();
					milestone.IsMilestone = true;
				}
				return workflowTemplate;
			}
		}
		ProcessTaskTemplate workflowTemplate;

		#endregion

		internal IBMTestHelper BMSTestHelper
		{
			get { return ObjectFactory.Get<IBMTestHelper>(); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ProcessTaskNotification result = Factory.NewWithValidTestData<ProcessTaskNotificationForTest>();
			result.PQ_P9 = ZGuid.Empty;
			return result;
		}

		ProcessTaskNotification GetNewNotificationWithParentJob()
		{
			var shipment = (IWorkflowProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var task = shipment.WorkflowItems.Triggers.AddNew();

			return task.ProcessTaskNotifications.AddNew();
		}

		void AssertOverrideEmail(BusinessObjectFactory factory, ProcessTaskNotificationForTest ptNotification, ZString assertionFailedMessage, ZString pQ_EmailText, ZBool expectedOverrideEmail)
		{
			ZGuid ptNotification_PK = ptNotification.PK;
			ptNotification.PQ_EmailText = pQ_EmailText;
			factory.Save();
			factory = new BusinessObjectFactory();
			ptNotification = factory.Load<ProcessTaskNotificationForTest>(ptNotification_PK);
			AssertEquals(ptNotification.Get_overrideEmail(), ptNotification.OverrideEmail);
			AssertEquals(assertionFailedMessage, expectedOverrideEmail, ptNotification.OverrideEmail);
		}

		IBMTestHelper BMTestHelper
		{
			get { return bmTestHelper ?? (bmTestHelper = ObjectFactory.Get<IBMTestHelper>()); }
		}
		IBMTestHelper bmTestHelper;

		public class ProcessTaskNotificationForTest : ProcessTaskNotification
		{
			public ProcessTaskNotificationForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SetOverrideEmailLocalVariable()
			{
				base.SetOverrideEmailLocalVariable();
				SetOverrideEmailLocalVariable_WasCalled = true;
			}
			public bool SetOverrideEmailLocalVariable_WasCalled;

			public ZBool Get_overrideEmail()
			{
				return overrideEmail;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			AssertNotNull(DummyWorkflowDescriptor.Instance);
		}

		#endregion
	}
}
