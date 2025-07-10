using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TriggerConditionTest : TemplateApplicationTestCase
	{
		#region RFP

		public void TestRFP()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger.TriggerConditions.TriggerConditionValue = "CMP=NOG";
			var notification = MakeNotification(trigger);
			dummy.Logs.AddNew(Events.CustomisableEvent00, new KeyValuePair<string, string>("CMP", "NOG"));
			AssertNotEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		public void TestRFPVariousKeySizesFunction()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var trigger1 = MakeTrigger(dummy, Events.CustomisableEvent00Code);
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger1.TriggerConditions.TriggerConditionValue = "CMP=NOG";
			MakeNotification(trigger1);

			var trigger2 = MakeTrigger(dummy, Events.CustomisableEvent00Code);
			trigger2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger2.TriggerConditions.TriggerConditionValue = "DEP=NOG";
			MakeNotification(trigger2);

			var trigger3 = MakeTrigger(dummy, Events.CustomisableEvent00Code);
			trigger3.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger3.TriggerConditions.TriggerConditionValue = "NAM=NOG";
			MakeNotification(trigger3);

			dummy.Logs.AddNew(Events.CustomisableEvent00, new KeyValuePair<string, string>("CMP", "NOG"));
			dummy.Logs.AddNew(Events.CustomisableEvent00, new KeyValuePair<string, string>("DEP", "NOG"));
			dummy.Logs.AddNew(Events.CustomisableEvent00, new KeyValuePair<string, string>("NAM", "NOG"));

			CombineAssertions("Triggered", () =>
			{
				AssertNotEquals(ZDateTimeOffset.Empty, trigger1.P9_ActualDateForBinding);
				AssertNotEquals(ZDateTimeOffset.Empty, trigger2.P9_ActualDateForBinding);
				AssertNotEquals(ZDateTimeOffset.Empty, trigger3.P9_ActualDateForBinding);
			});
		}

		public void TestRFP_DoNotFireWhenConditionInvalid()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger.TriggerConditions.TriggerConditionValue = "JabbaJabbaJabba";
			var notification = MakeNotification(trigger);
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		public void TestREF_ReapplyTemplateWithCascadingMilestone()
		{
			void CreateMilestone(ProcessTaskTemplate template, string eventCode)
			{
				var milestone = template.WorkflowItems.Milestones.AddNew();
				milestone.TriggerConditions.TriggerEventCode = eventCode;
				milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
				milestone.TriggerConditions.TriggerConditionValue = "MYREFERENCE";
				milestone.P9_RespondToCascadedEvents = true;
				milestone.P9_CascadedEventsContext = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			}

			var shipmentTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			shipmentTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			CreateMilestone(shipmentTemplate, Events.CustomisableEvent00Code);
			CreateMilestone(shipmentTemplate, Events.CustomisableEvent01Code);

			Factory.Save();

			var consol = Factory.New<Forwarding.IForwardingConsol>();
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			consol.AddShipment(shipment);

			Factory.Save();
			var workflowProvider = ((IWorkflowProvider)shipment);
			AssertEquals(2, workflowProvider.WorkflowItems.Milestones.Count);

			var log = ((IStmALogParent)shipment).Logs.AddNew(Events.CustomisableEvent00, "MYREFERENCE");
			AssertMilestoneFireCount("Shipment event after milestone exists should not fire", 0, workflowProvider.WorkflowItems.Milestones[Events.CustomisableEvent00]);

			workflowProvider.WorkflowItems.Milestones.DeleteAll();
			Factory.Save();
			AssertMilestoneFireCount("Shipment event before milestone exists should not fire", 0, workflowProvider.WorkflowItems.Milestones[Events.CustomisableEvent00]);

			((IStmALogParent)consol).Logs.AddNew(Events.CustomisableEvent01, "MYREFERENCE");
			Factory.Save();
			AssertMilestoneFireCount("Consol event after milestone exists should fire", 1, workflowProvider.WorkflowItems.Milestones[Events.CustomisableEvent01]);
		}

		#endregion

		#region MCR

		public void TestMCR_EventAddedDuringActionDelay_EventSource()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Event.Source.Contains(\"Dummy\")";

			var notification = MakeNotification(trigger);
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			notification.PQ_FieldName = "<Z0_Code>";
			notification.PQ_FieldValue = "KEH";

			using (Factory.OnSaveDelayer.TemporaryChangeToDelayStrategy())
			{
				dummy.Logs.AddNew(Events.CustomisableEvent00);
			}

			AssertMilestoneFireCount(1, trigger);
			AssertEquals("KEH", dummy.Z0_Code);
		}

		public void TestMCR_EventAddedDuringActionDelay_IFCReferencingP9_ActualDate()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Event.Source.Contains(\"Dummy\")";

			var notification = MakeNotification(trigger);
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			notification.PQ_FieldName = "<Z0_Date>";
			notification.PQ_FieldValue = "<P9_ActualDate>";

			using (Factory.OnSaveDelayer.TemporaryChangeToDelayStrategy())
			{
				dummy.Logs.AddNew(Events.CustomisableEvent00);
			}

			AssertMilestoneFireCount(1, trigger);
			AssertEquals(trigger.P9_ActualDate, dummy.Z0_Date);
		}

		public void TestMCR_LineTrigger_Caching()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = MakeTask(dummy, description: "Golden Saint");
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var task2 = MakeTask(dummy, description: "Twilight Spectre");
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var task3 = MakeTask(dummy, description: "Ancestral Ghost");
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var trigger = MakeTrigger(dummy);
			trigger.TriggerConditions.TriggerEventCode = Events.StatusChangeCode;
			trigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Source.P9_Description == \"Twilight Spectre\"";

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(100, (int)trigger.P9_TriggerFiredCountdown);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);
		}

		public void TestMCR_LineTrigger_Caching2()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = MakeTask(dummy, description: "Golden Saint");
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var task2 = MakeTask(dummy, description: "Twilight Spectre");
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var task3 = MakeTask(dummy, description: "Ancestral Ghost");
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var trigger = MakeTrigger(dummy);
			trigger.TriggerConditions.TriggerEventCode = Events.StatusChangeCode;
			trigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Source.P9_Description == \"Twilight Spectre\"";

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);
		}

		[TestDate(2020, 1, 1)]
		public void TestMCR_Variance_NoPreviousEventDate()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = MakeTask(dummy, description: "Golden Saint");
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var trigger = MakeTrigger(dummy);
			trigger.TriggerConditions.TriggerEventCode = Events.StatusChangeCode;
			trigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Variance(Trigger.PreviousEventDate, Event.EventTime).TotalMinutes > 10.0";

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);
		}

		[TestDate(2020, 1, 1)]
		public void TestMCR_Variance_TimeDifferenceLessThanVariance_DoesNotActivateTrigger()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = MakeTask(dummy, description: "Golden Saint");
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var trigger = MakeTrigger(dummy);
			trigger.TriggerConditions.TriggerEventCode = Events.StatusChangeCode;
			trigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Variance(Trigger.PreviousEventDate, Event.EventTime).TotalMinutes > 10.0";

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);

			TestDateAttribute.AddMinutes(10);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);
		}

		[TestDate(2020, 1, 1)]
		public void TestMCR_Variance_TimeDifferenceGreaterThanVariance_ActivatesTrigger()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = MakeTask(dummy, description: "Golden Saint");
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var trigger = MakeTrigger(dummy);
			trigger.TriggerConditions.TriggerEventCode = Events.StatusChangeCode;
			trigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Variance(Trigger.PreviousEventDate, Event.EventTime).TotalMinutes > 10.0";

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);

			TestDateAttribute.AddMinutes(11);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(98, (int)trigger.P9_TriggerFiredCountdown);
		}

		[TestDate(2020, 1, 1)]
		public void TestMCR_MacrosOnShipmentDontGoBang()
		{
			var dummy = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
			var trigger = MakeTrigger(dummy);
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "TriggerSource.WorkflowItems.Triggers.Where({\"<P9_Type>\"==\"TRG\"})";
			((BusinessObject)dummy).GetLogs().AddNew(Events.CustomisableEvent00);
			AssertEquals(100, (int)trigger.P9_TriggerFiredCountdown);
		}

		[ExpectNoExceptions]
		public void TestMCR_MacrosDoNotThrowExceptionForInvalidZDateTimeSubtraction()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var dummy = (IWorkflowProvider)shipment;
			var task1 = MakeTask(dummy);
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			task1.P9_CompletedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			shipment.JS_E_DEP = ZDateTime.UtcNow;

			var trigger = MakeTrigger(dummy);
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Source.JS_E_DEP - Source.JS_E_ARV";

			var log = ((BusinessObject)dummy).GetLogs().AddNew(Events.CustomisableEvent00);
			var areTriggerConditionMetResult = TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, (BusinessObject)shipment);

			Assert(!areTriggerConditionMetResult);
		}

		public void TestMCR_TriggerActivates_ForVariable()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = @"@Event.Params.LOC==""ABC""";

			AssertEquals(100, (int)trigger.P9_TriggerFiredCountdown);

			dummy.Logs.AddNew(Events.CustomisableEvent00, "FreeText|LOC=ABCD");
			AssertEquals(100, (int)trigger.P9_TriggerFiredCountdown);

			dummy.Logs.AddNew(Events.CustomisableEvent00, "FreeText|LOC=ABC");
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);
		}

		#endregion

		#region UDF

		public void TestUDF_Success()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00.Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"1\" == \"1\"";

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertNotEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		public void TestUDF_Failure()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00.Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"0\" == \"1\"";

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		public void TestUDF_NotABoolean()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00.Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "Not a boolean value";

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		public void TestUDF_Invalid()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00.Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<InvalidProperty>\"";

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		public void TestUDF_WithException()
		{
			var testMailNotificationUser = Factory.New<GlbStaff>();
			testMailNotificationUser.GS_Code = "GRP";
			testMailNotificationUser.GS_LoginName = "GRP";
			testMailNotificationUser.GS_IsSystemAccount = false;
			testMailNotificationUser.GS_EmailAddress = "FakeEmail";
			var testMailNotificationGroup = Factory.New<GlbGroup>();
			testMailNotificationGroup.GG_Code = "eHubGroup";
			var groupLink = Factory.New<GlbGroupLink>();
			groupLink.GK_GG = testMailNotificationGroup.PK;
			groupLink.GK_GS = testMailNotificationUser.PK;

			var dummy = Factory.New<DummyWithWorkflow>();
			RawDataRegistry.Instance.NotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testMailNotificationGroup.PK.ToGuid());

			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00.Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<ThrowingProperty>\"";

			AssertNoExceptionThrown(() => dummy.Logs.AddNew(Events.CustomisableEvent00));
			AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		public void TestUDF_UsingJobProperty()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var trigger = MakeTrigger(job, Events.CustomisableEvent00.Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<Z0_NVarCharMax>\" == \"ABC\"";

			job.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);

			job.Z0_NVarCharMax = "ABC";

			job.Logs.AddNew(Events.CustomisableEvent00);
			AssertNotEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		public void TestUDF_UsingTaskProperty()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = MakeTask(job);

			var trigger = MakeTrigger(job, Events.CustomisableEvent00.Code);
			trigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<P9_Description>\" == \"ABC\"";

			task.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);

			task.P9_Description = "ABC";

			task.Logs.AddNew(Events.CustomisableEvent00);
			AssertNotEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		public void TestUDF_LineTrigger_Caching()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = MakeTask(dummy, description: "Golden Saint");
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var task2 = MakeTask(dummy, description: "Twilight Spectre");
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var task3 = MakeTask(dummy, description: "Ancestral Ghost");
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var trigger = MakeTrigger(dummy);
			trigger.TriggerConditions.TriggerEventCode = Events.StatusChangeCode;
			trigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<P9_Description>\" == \"Twilight Spectre\"";

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(100, (int)trigger.P9_TriggerFiredCountdown);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);
		}

		public void TestUDF_TriggeringEvent()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var trigger = MakeTrigger(dummy, Events.CustomisableEvent00.Code);
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<SL_SE_NKEvent>\" == \"Z00\"";
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertNotEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
		}

		#endregion
	}
}
