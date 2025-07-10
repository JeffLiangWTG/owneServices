using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ProcessTaskLogsTest : TestCaseWithFactory
	{
		#region Setup

		static ProcessTask CreateMilestone(DummyWithWorkflow dummy, string name)
		{
			var mil = dummy.WorkflowItems.Milestones.AddNew();
			mil.TriggerConditions.TriggerEventCode = Events.CustomisableEvent99Code;
			mil.P9_Description = name;
			return mil;
		}

		static ProcessTaskNotification CreateNotification(ProcessTask milestone, string emailAddr)
		{
			var notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = emailAddr;
			return notification;
		}

		#endregion

		public void TestLogsCaching_SetActualDate_IgnoreUnrelatedEvents()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = CreateMilestone(dummy, "Melbourne more like Smellbourne.");
			CreateNotification(milestone, "thesickestburn@oohhyeaah.com");

			var log1 = milestone.Logs.AddNew(Events.CustomisableEvent00);
			var log2 = dummy.Logs.AddNew(Events.CustomisableEvent01);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var milestoneInNewFactory = newFactory.Load<ProcessTask>(milestone.PK);
			milestoneInNewFactory.SetMilestoneActualDateForTest(ZDateTime.Now);

			AssertCollectionNotContains(log1.PK, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.Select(s => s.PK));
			AssertCollectionNotContains(log2.PK, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.Select(s => s.PK));
		}

		public void TestLogsCaching_SetSceduleDate_IgnoreUnrelatedEvents()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = CreateMilestone(dummy, "Melbourne more like Smellbourne.");
			CreateNotification(milestone, "thesickestburn@oohhyeaah.com");

			var log1 = milestone.Logs.AddNew(Events.CustomisableEvent00);
			var log2 = dummy.Logs.AddNew(Events.CustomisableEvent01);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var milestoneInNewFactory = newFactory.Load<ProcessTask>(milestone.PK);
			milestoneInNewFactory.P9_ScheduledDate = ZDateTime.Now;

			AssertCollectionNotContains(log1.PK, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.Select(s => s.PK));
			AssertCollectionNotContains(log2.PK, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.Select(s => s.PK));
		}

		public void TestLogsCaching_SetActualDate_DontLoadEverything()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = CreateMilestone(dummy, "The angry trigger");
			CreateNotification(milestone, "readyto@explode.com");

			for (int i = 0; i < 20; i++)
			{
				milestone.SetMilestoneActualDateForTest(ZDateTime.UtcNow.AddDays(i));
				Factory.Save(); // We are seeding the cache;
			}

			AssertEquals(1, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.OfType<StmALog>().Count(s => s.SL_Parent == milestone.PK && s.SL_SE_NKEvent == Events.WorkflowTriggerEventCode));

			var newFactory = Factory.CreateNewFactory();
			var milestoneInNewFactory = newFactory.Load<ProcessTask>(milestone.PK);
			milestoneInNewFactory.SetMilestoneActualDateForTest(ZDateTime.Now);

			AssertEquals(0, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.OfType<StmALog>().Count(s => s.SL_Parent == milestone.PK));
		}

		public void TestLogsCaching_SetScheduledDate_DontLoadEverything()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = CreateMilestone(dummy, "The angry trigger");
			CreateNotification(milestone, "readyto@explode.com");

			for (int i = 0; i < 20; i++)
			{
				milestone.P9_ScheduledDate = ZDateTime.UtcNow.AddDays(i);
				Factory.Save(); // We are seeding the cache;
			}

			AssertEquals(20, ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.OfType<StmALog>().Count(s => s.SL_Parent == dummy.PK && s.SL_SE_NKEvent == milestone.TriggerConditions.TriggerEventCode));

			var newFactory = Factory.CreateNewFactory();
			var milestoneInNewFactory = newFactory.Load<ProcessTask>(milestone.PK);
			milestoneInNewFactory.P9_ScheduledDate = ZDateTime.Now;

			AssertEquals(4, ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects.OfType<StmALog>().Count(s => s.SL_Parent == dummy.PK));
		}

		[TestDate(2021, 09, 23)]
		[TestDateIncremental(seconds: 1)]
		public void TestLogs_IncludeStatusChangeMode()
		{
			var processTaskStatusChangeModeTracker = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();
			processTaskStatusChangeModeTracker.Clear();

			var task = Factory.New<ProcessTask>();
			var user = Factory.New<GlbStaff>();
			task.P9_GS_NKAssignedStaffMember = user.GS_Code;

			var allStatusChageModeCodes = new ProcessTaskStatusChangeModeCodeList().GetAllCodes();

			foreach (var statusChageModeCode in allStatusChageModeCodes)
			{
				processTaskStatusChangeModeTracker.SetCurrent(statusChageModeCode);
				task.P9_Status = task.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended ? ProcessTaskStatusCodeList.Codes.Working : ProcessTaskStatusCodeList.Codes.Suspended;
			}

			Factory.Save();

			var logsReferences = task.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusChange)
				.OrderBy(l => l.SL_EventTime)
				.Select(log => log.SL_Reference)
				.ToArray();

			for (int i = 0; i < allStatusChageModeCodes.Length; i++)
			{
				var expectedStatusChangeMode = allStatusChageModeCodes[i];
				var actualLogReference = logsReferences[i];

				AssertContains($"{ProcessTaskStatusChangeLog.StatusChangeModeParameterCode}={expectedStatusChangeMode}", actualLogReference);
			}
		}

		public void TestLogs_WithoutAddAndEdit()
		{
			var task = Factory.New<ProcessTask>();
			var user = Factory.New<GlbStaff>();
			task.P9_GS_NKAssignedStaffMember = user.GS_Code;
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var taskReload = newFactory.Load<ProcessTask>(task.PK);
			AssertNull(taskReload.Logs.AddedLog);
			AssertEquals(0, taskReload.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.EditedARecord).Count());
		}

		public void TestLogs_WithAddAndEdit()
		{
			var tempValues = new EnableAddEditAndDeleteLogsItemCollection();
			tempValues.Add(new EnableAddEditAndDeleteLogsItem()
			{
				Table = ProcessTasksSchema.Constants.TableName,
				EnableADDLogs = true,
				EnableEDTLogs = true,
				EnableDELLogs = true,
			});

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValues))
			{
				var task = Factory.New<ProcessTask>();
				var user = Factory.New<GlbStaff>();
				task.P9_GS_NKAssignedStaffMember = user.GS_Code;
				Factory.Save();

				AssertNotNull(task.Logs.AddedLog);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				Factory.Save();

				AssertEquals(1, task.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.EditedARecord).Count());
			}
		}

		[TestDate(2021, 09, 23)]
		[TestDateIncremental(seconds: 1)]
		public void TestLogs_IncludeStatusChangeMode_WhenSetTemporaryOnTask()
		{
			var task = Factory.New<ProcessTask>();
			var user = Factory.New<GlbStaff>();
			task.P9_GS_NKAssignedStaffMember = user.GS_Code;
			Factory.Save();

			var allStatusChageModeCodes = new ProcessTaskStatusChangeModeCodeList().GetAllCodes();

			foreach (var statusChageModeCode in allStatusChageModeCodes)
			{
				using (task.SetTemporaryStatusChangeMode(statusChageModeCode))
				{
					task.P9_Status = task.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended ? ProcessTaskStatusCodeList.Codes.Working : ProcessTaskStatusCodeList.Codes.Suspended;
				}
			}

			Factory.Save();

			var logsReferences = task.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusChange)
				.OrderBy(l => l.SL_EventTime)
				.Select(log => log.SL_Reference)
				.ToArray();

			for (int i = 0; i < allStatusChageModeCodes.Length; i++)
			{
				var expectedStatusChangeMode = allStatusChageModeCodes[i];
				var actualLogReference = logsReferences[i];

				AssertContains($"{ProcessTaskStatusChangeLog.StatusChangeModeParameterCode}={expectedStatusChangeMode}", actualLogReference);
			}
		}
	}
}
