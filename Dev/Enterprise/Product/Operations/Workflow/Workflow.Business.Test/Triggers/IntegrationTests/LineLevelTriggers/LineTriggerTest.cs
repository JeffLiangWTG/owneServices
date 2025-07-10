using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.WorkflowManager.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	public class LineTriggerTest : WorkflowTestCase
	{
		public class WorkflowDescriptorsForTest : WorkflowDescriptors, IDisposable
		{
			public WorkflowDescriptorsForTest()
			{
				AddDescriptor(new DummyLineWorkflowDescriptor());
				AddDescriptor(DummyWorkflowDescriptor.Instance);
				overrideDescriptorsDelegate = OverrideWorkflowDescriptorsDelegate(() => this);
				DummyWorkflowDescriptor.Instance.SupportedTriggerLineTypes = new[] { DummyLineType };
				var list = new CodeDescriptionPairList();
				list.AddPair(DummyLineType, "It's the dummy line");
			}

			public void Dispose()
			{
				overrideDescriptorsDelegate.Dispose();
			}

			readonly IDisposable overrideDescriptorsDelegate;
		}

		public const string DummyLineType = "SME";

		#region Test Classes

		public class DummyWithLines : DummyWithWorkflow
		{
			public DummyWithLines(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyLine AddLine()
			{
				var line = Factory.New<DummyLine>();
				line.Z0_Guid = PK;
				return line;
			}

			public DummyLineWithProcessHandlingInfo AddLineWithProcessHandlingInfo()
			{
				var line = Factory.New<DummyLineWithProcessHandlingInfo>();
				line.Z0_Guid = PK;
				return line;
			}

			public DummyLine[] Lines => Factory.Load<DummyLine>(new ZQuery(DummyBizoSchema.Z0_Guid, PK));
			public new DummyWithLinesProcessTaskCollection WorkflowItems => (DummyWithLinesProcessTaskCollection)base.WorkflowItems;
			protected override DummyProcessTaskCollection GetNewWorkflowItems() => this.GetOrCreateProcessTaskCollection(() => new DummyWithLinesProcessTaskCollection(this));
		}

		public class DummyWithLinesProcessTaskCollection : DummyProcessTaskCollection
		{
			public DummyWithLinesProcessTaskCollection(BusinessObject master)
				: base(master)
			{
			}
		}

		public sealed class DummyLineWorkflowDescriptor : WorkflowDescriptor
		{
			public DummyLineWorkflowDescriptor()
			{
			}

			public override string Code => DummyLineType;
			public override IMultilingualString Description => (NoResString)"Dummy Line type";
			public override Type WorkflowProviderType => typeof(DummyLine);
			public override ControllerID ControllerID => DummyControllerIDs.Dummy;
		}

		public class DummyLine : DummyEnterpriseBusinessObject, ICustomProcessTaskHandlerProvider
		{
			public DummyLine(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyWithWorkflow Parent => Factory.Load<DummyWithWorkflow>(Z0_Guid);

			public IProcessTaskHandler GetHandler(IStmALog log)
			{
				return new LineProcessTaskHandler(Parent, this, log, DummyLineType);
			}
		}

		public class DummyLineWithProcessHandlingInfo : DummyEnterpriseBusinessObject, IProcessHandlingInfoProvider
		{
			public DummyLineWithProcessHandlingInfo(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyWithWorkflow Parent => Factory.Load<DummyWithWorkflow>(Z0_Guid);

			ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
			{
				get { return new DummyLineProcessHandlingInfo(this); }
			}
		}

		class DummyLineProcessHandlingInfo : ProcessHandlingInfo
		{
			public DummyLineProcessHandlingInfo(DummyLineWithProcessHandlingInfo dummy)
				: base(dummy)
			{
			}

			protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
			{
				return Enumerable.Empty<CascadingLink>();
			}

			protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
			{
				return TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(DummyLine.Parent, logBeingAdded, DummyLineType).Select(s => s.trigger);
			}

			DummyLineWithProcessHandlingInfo DummyLine
			{
				get { return (DummyLineWithProcessHandlingInfo)LogParent; }
			}
		}

		class NotificationTestBuilder
		{
			public NotificationTestBuilder(BusinessObjectFactory factory, Action<ProcessTaskNotification> setupNotification)
			{
				Factory = factory;
				Setup = setupNotification;
			}

			Action<ProcessTaskNotification> Setup { get; }
			BusinessObjectFactory Factory { get; }

			public class NotificationTestTuple
			{
				public DummyWithLines Parent { get; set; }
				public DummyLine Line { get; set; }
				public ProcessTask Trigger { get; set; }
				public ProcessTaskNotification Notification { get; set; }
			}

			public void ExecuteAndAssert(Action<NotificationTestTuple> assertNotificationResult, string expectedError = null, bool runLogWalker = true)
			{
				var tuple = new NotificationTestTuple
				{
					Parent = Factory.New<DummyWithLines>()
				};
				tuple.Line = tuple.Parent.AddLine();
				tuple.Line.Z0_IsSystem = false;
				tuple.Trigger = tuple.Parent.WorkflowItems.Triggers.AddNew();
				tuple.Trigger.P9_LineTriggerType = DummyLineType;
				tuple.Trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
				tuple.Notification = tuple.Trigger.ProcessTaskNotifications.AddNew();
				Setup(tuple.Notification);
				tuple.Line.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
				Factory.Save();
				if (runLogWalker)
				{
					AssertContains(expectedError, RunLogWalker());
				}

				assertNotificationResult(tuple);
			}
		}

		#endregion

		#region TestLinesThatDoNotImplementWorkflow_LineEventsFire

		public void TestLinesThatDoNotImplementWorkflow_MCRTriggerConditionsWorkDuringSave()
		{
			CheckLinesThatDoNotImplementWorkflow_TriggerConditionsWorkDuringSave(EventReferenceConditionList.Codes.ConditionWithMacros);
		}

		public void TestLinesThatDoNotImplementWorkflow_UDFriggerConditionsWorkDuringSave()
		{
			CheckLinesThatDoNotImplementWorkflow_TriggerConditionsWorkDuringSave(EventReferenceConditionList.Codes.UserDefined);
		}

		public void CheckLinesThatDoNotImplementWorkflow_TriggerConditionsWorkDuringSave(string triggerCondition)
		{
			using (new WorkflowDescriptorsForTest())
			{
				var parent = Factory.New<DummyWithLines>();
				var line1 = parent.AddLine();
				var line2 = parent.AddLine();
				var trigger = parent.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerCondition = triggerCondition;
				trigger.TriggerConditions.TriggerConditionValue = "\"1\"==\"1\"";
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
				trigger.P9_LineTriggerType = DummyLineType;
				SetupEmailNotification(trigger.ProcessTaskNotifications.AddNew());
				Factory.Save();

				line1.FactorySaving += (s, e) => line1.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
				Factory.Save();

				AssertNotEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
			}
		}

		public void TestLinesThatDoNotImplementWorkflow_LineEventsFire()
		{
			using (new WorkflowDescriptorsForTest())
			{
				var parent = Factory.New<DummyWithLines>();
				var line1 = parent.AddLine();
				var line2 = parent.AddLine();
				var trigger = parent.WorkflowItems.Triggers.AddNew();
				trigger.P9_LineTriggerType = DummyLineType;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
				SetupEmailNotification(trigger.ProcessTaskNotifications.AddNew());

				line1.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
				line2.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
				AssertEquals(2, trigger.GetLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode).Count());
			}
		}

		/// <summary>
		/// Since SJ_ProcessTaskParentID is being used to block parallelism in the LWK,
		/// We are letting line triggers parallelize here.
		/// </summary>
		public void TestLinesDoNotLockEachOtherInLWK()
		{
			using (new WorkflowDescriptorsForTest())
			{
				var parent = Factory.New<DummyWithLines>();
				var line1 = parent.AddLine();
				var line2 = parent.AddLine();
				var trigger = parent.WorkflowItems.Triggers.AddNew();
				trigger.P9_LineTriggerType = DummyLineType;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
				SetupEmailNotification(trigger.ProcessTaskNotifications.AddNew());

				line1.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
				line2.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
				Factory.Save();
				LogWalkerRunner.Master().Process(new LoggerForTest(), CancellationToken.None);

				AssertEquals("The parent of the line can be identified", 1, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ProcessTaskParentID, line1.PK)).Length);
				AssertEquals("The parent of the line can be identified", 1, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ProcessTaskParentID, line2.PK)).Length);
			}
		}

		public void TestNullParent()
		{
			using (new WorkflowDescriptorsForTest())
			{
				var parent = Factory.New<DummyWithLines>();
				var line1 = parent.AddLine();
				var trigger = parent.WorkflowItems.Triggers.AddNew();
				trigger.P9_LineTriggerType = DummyLineType;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;

				var log = line1.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
				Factory.Save();
				line1.Delete();
				Factory.Save();

				AssertNoExceptionThrown(() => MasterFilesTestHelper.RunLogWalker());
				AssertEquals("The parent of the line can be identified", 1, Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ProcessTaskParentID, line1.PK)).Length);
			}
		}

		#endregion

		#region TestNTF

		public void TestNTF()
		{
			using (new WorkflowDescriptorsForTest())
			{
				new NotificationTestBuilder(Factory, SetupEmailNotification).ExecuteAndAssert(_ => AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count));
			}
		}

		static void SetupEmailNotification(ProcessTaskNotification notification)
		{
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "bung@bung.bung";
		}

		#endregion

		#region TestTMP

		public void TestTMP()
		{
			using (new WorkflowDescriptorsForTest())
			{
				new NotificationTestBuilder(Factory, SetupWorkflowTemplate).ExecuteAndAssert(n => AssertEquals(0, Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, n.Line.PK)).Length),
					"The job that the template should be applied to cannot be found.");
			}
		}

		void SetupWorkflowTemplate(ProcessTaskNotification notification)
		{
			var standardTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			standardTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			var templateTask = standardTemplate.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "MAYB BB";

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			notification.PQ_P0_WorkflowTemplate = standardTemplate.PK;
		}

		const string DansDirtyUndies = "DirtyUndies";

		#endregion

		#region TestFLD

		public void TestFLD()
		{
			using (new WorkflowDescriptorsForTest())
			{
				new NotificationTestBuilder(Factory, SetupFldNotification).ExecuteAndAssert(n =>
				{
					n.Line.Reload();
					AssertEquals(DansDirtyUndies, n.Line.Z0_NVarChar);
				});
			}
		}

		static void SetupFldNotification(ProcessTaskNotification notification)
		{
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification.PQ_EmailAddr = "bung@bung.bung";
			notification.PQ_FieldName = "<Z0_NVarChar>";
			notification.PQ_FieldValue = DansDirtyUndies;
		}

		#endregion

		#region TestIFC

		public void TestIFC()
		{
			using (new WorkflowDescriptorsForTest())
			{
				new NotificationTestBuilder(Factory, SetupIfcNotification).ExecuteAndAssert(n =>
				{
					n.Line.Reload();
					AssertEquals(DansDirtyUndies, n.Line.Z0_NVarChar);
				}, null, false);
			}
		}

		static void SetupIfcNotification(ProcessTaskNotification notification)
		{
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			notification.PQ_EmailAddr = "bung@bung.bung";
			notification.PQ_FieldName = "<Z0_NVarChar>";
			notification.PQ_FieldValue = DansDirtyUndies;
		}

		#endregion
	}

	[TestedType(typeof(TasksAndMilestonesLoader))]
	class TasksAndMilestonesLoaderLineTriggerTest : LogSubscriberTest<TasksAndMilestonesLoader>
	{
		public void TestProcess_LinetriggerLogsWithDeferredWorkflowFiring_WithProcessHandlingInfo()
		{
			var parent = Factory.New<LineTriggerTest.DummyWithLines>();
			var line1 = parent.AddLineWithProcessHandlingInfo();
			var trigger = parent.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = LineTriggerTest.DummyLineType;
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			Factory.Save();

			line1.Logs.AddNew(new EventValue(AutoEvents.CustomisableEvent00, eventTime: new ZDateTimeOffset(2017, 03, 08), deferFiringWorkflow: true));
			AssertEquals("Precondition", 0, trigger.GetLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode).Count());
			Factory.Save();

			using (ObjectFactory.Substitute("IDummy", line1))
			{
				RunLogWalkerCycleForTest();
			}
			trigger.GetLogs().GetAllLogs().Reload(true);
			AssertEquals(1, trigger.GetLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode).Count());
		}

		#region SetUp / Tear Down

		protected override void SetUp()
		{
			base.SetUp();
			TypeDecider.AddSubstitution(typeof(WorkflowSupportableTableNames), typeof(WorkflowSupportableTableNamesWithDummy));
		}

		#endregion
	}
}
