using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test.Glow
{
	public class GlowStmaLogInsertTests : TestCaseWithFactory
	{
		public void TestAddSingleTaskOnCreate()
		{
			WorkflowTest(
				setupWorkflow: () =>
				{
					AddTaskToTemplate("APP", "Task Add Test");
				},
				test: () =>
				{
					var holidayID = ManuallyInsertHoliday();
					ManuallyInsertStmALog(holidayID);

					MasterFilesTestHelper.RunLogWalker();

					AssertHolidayTaskExists(holidayID, "APP", "Task Add Test");
					AssertTemplateTaskExists("APP", "Task Add Test");
				});
		}

		public void TestAddMulitpleTaskOnCreate()
		{
			WorkflowTest(
				setupWorkflow: () =>
				{
					AddTaskToTemplate("APP", "Task Add Test");
					AddTaskToTemplate("UDF", "Task Undefined", seq: 2);
					AddTaskToTemplate("ABC", "Task Learn", seq: 3);
				},
				test: () =>
				{
					var holidayID = ManuallyInsertHoliday();
					ManuallyInsertStmALog(holidayID);

					MasterFilesTestHelper.RunLogWalker();

					AssertHolidayTaskExists(holidayID, "APP", "Task Add Test");
					AssertHolidayTaskExists(holidayID, "UDF", "Task Undefined", seq: 2);
					AssertHolidayTaskExists(holidayID, "ABC", "Task Learn", seq: 3);
					AssertTemplateTaskExists("APP", "Task Add Test");
					AssertTemplateTaskExists("UDF", "Task Undefined", seq: 2);
					AssertTemplateTaskExists("ABC", "Task Learn", seq: 3);
				});
		}

		public void TestAddSingleTaskAndTriggerEmailOnCreate()
		{
			WorkflowTest(
				setupWorkflow: () =>
				{
					AddTaskToTemplate("APP", "Task Add Test");
					AddEmailTriggerToTemplate("Add Trigger Description", "ADD");
				},
				test: () =>
				{
					var holidayID = ManuallyInsertHoliday();
					ManuallyInsertStmALog(holidayID);

					var ss = MasterFilesTestHelper.RunLogWalker();

					AssertHolidayTaskExists(holidayID, "APP", "Task Add Test");
					AssertTemplateTaskExists("APP", "Task Add Test");
					AssertHolidayEmailTriggerExists(holidayID, "Add Trigger Description", "ADD");
					AssertTemplateEmailTriggerExists("Add Trigger Description", "ADD");

					AssertEmailSent("Email should have been triggered for ADD event", new[] { "glowtest@test.com" }, "This is the email content");
				});
		}

		public void TestAddSingleTaskAndTriggerEmailOnEdit()
		{
			WorkflowTest(
				setupWorkflow: () =>
				{
					AddTaskToTemplate("APP", "Task Add Test");
					AddEmailTriggerToTemplate("Add Trigger Description", "ADD");
					AddEmailTriggerToTemplate("Edit Trigger Description", "EDT", emailText: "edit email", emailAddress: "anEdit@email.com");
				},
				test: () =>
				{
					var holidayID = ManuallyInsertHoliday();
					ManuallyInsertStmALog(holidayID);

					MasterFilesTestHelper.RunLogWalker();

					AssertHolidayTaskExists(holidayID, "APP", "Task Add Test");
					AssertTemplateTaskExists("APP", "Task Add Test");
					AssertHolidayEmailTriggerExists(holidayID, "Add Trigger Description", "ADD");
					AssertTemplateEmailTriggerExists("Add Trigger Description", "ADD");
					AssertHolidayEmailTriggerExists(holidayID, "Edit Trigger Description", "EDT");
					AssertTemplateEmailTriggerExists("Edit Trigger Description", "EDT");

					AssertEmailSent("Email should have been triggered for ADD event", new[] { "glowtest@test.com" }, "This is the email content");

					Env.ClearAllEmailsCreated();
					ManuallyInsertStmALog(holidayID, eventType: "EDT");
					MasterFilesTestHelper.RunLogWalker();
					AssertEmailSent("Email should have been triggered for Edit event", new[] { "anEdit@email.com" }, "edit email");
				});
		}

		public void TestAddSingleTaskAndTSKTriggerFailsWhenProcessTasksDisabled()
		{
			// the registry entry System -> Audit Logs -> Enable Add and Edit Logs [EnableAddEditAndDeleteLogsItemsRegistryItem] has to be enabled
			// for workflow templates to be run on logs for ProcessTasks
			processTasksEnabled = false;
			WorkflowTest(
				setupWorkflow: () =>
				{
					AddTaskToTemplate("APP", "Task Add Test");
					AddEmailTriggerToTemplate("Add Trigger Description", "ADD", linetrigger: "TSK");
				},
				test: () =>
				{
					var holidayID = ManuallyInsertHoliday();

					ManuallyInsertStmALog(holidayID, table: "ProcessTasks");
					MasterFilesTestHelper.RunLogWalker();

					AssertEquals(0, Env.AllEmailsCreated.Count());
				});
		}

		public void TestAddSingleTaskAndTSKTriggerWorksWhenProcessTasksEnabled()
		{
			// the registry entry System -> Audit Logs -> Enable Add and Edit Logs [EnableAddEditAndDeleteLogsItemsRegistryItem] has to be enabled
			// for workflow templates to be run on logs for ProcessTasks
			processTasksEnabled = true;
			WorkflowTest(
				setupWorkflow: () =>
				{
					AddTaskToTemplate("APP", "Task Add Test");
					AddEmailTriggerToTemplate("Add Trigger Description", "ADD", linetrigger: "TSK");
				},
				test: () =>
				{
					var holidayID = ManuallyInsertHoliday();

					ManuallyInsertStmALog(holidayID);
					MasterFilesTestHelper.RunLogWalker();

					AssertHolidayTaskExists(holidayID, "APP", "Task Add Test");
					AssertTemplateTaskExists("APP", "Task Add Test");
					AssertHolidayEmailTriggerExists(holidayID, "Add Trigger Description", "ADD");
					AssertTemplateEmailTriggerExists("Add Trigger Description", "ADD");

					AssertEmailSent("Email should have been triggered for ADD event", new[] { "glowtest@test.com" }, "This is the email content");
				});
		}

		public void TestAddSingleTaskAndTSKTriggerEmailOnEdit()
		{
			// the registry entry System -> Audit Logs -> Enable Add and Edit Logs [EnableAddEditAndDeleteLogsItemsRegistryItem] has to be enabled
			// for workflow templates to be run on logs for ProcessTasks
			processTasksEnabled = true;
			WorkflowTest(
				 setupWorkflow: () =>
				{
					AddTaskToTemplate("APP", "Task Add Test");
					AddEmailTriggerToTemplate("Add Trigger Description", "ADD", linetrigger: "TSK");
					AddEmailTriggerToTemplate("Edit Trigger Description", "EDT", linetrigger: "TSK", emailText: "edit email", emailAddress: "anEdit@email.com");
				},
				test: () =>
				{
					var holidayID = ManuallyInsertHoliday();

					ManuallyInsertStmALog(holidayID);
					MasterFilesTestHelper.RunLogWalker();

					AssertHolidayTaskExists(holidayID, "APP", "Task Add Test");
					AssertTemplateTaskExists("APP", "Task Add Test");
					AssertHolidayEmailTriggerExists(holidayID, "Add Trigger Description", "ADD");
					AssertTemplateEmailTriggerExists("Add Trigger Description", "ADD");

					AssertEmailSent("Email should have been triggered for ADD event", new[] { "glowtest@test.com" }, "This is the email content");

					Env.ClearAllEmailsCreated();
					ManuallyInsertStmALog(GetProcessTaskId(holidayID), eventType: "EDT", table: "ProcessTasks");
					MasterFilesTestHelper.RunLogWalker();
					AssertEmailSent("Email should have been triggered for Edit event", new[] { "anEdit@email.com" }, "edit email");
				});
		}

		ProcessTask AddTaskToTemplate(string type, string description, int seq = 1)
		{
			var task = template.WorkflowItems.AddNew();
			task.P9_Description = description;
			task.P9_Type = type;
			task.P9_Sequence = seq;
			factory.Save();
			return task;
		}

		void AssertHolidayTaskExists(Guid parentid, string type, string description, int seq = 1) => AssertProcessTask(parentid, "GA", type, description, seq);

		void AssertTemplateTaskExists(string type, string description, int seq = 1) => AssertProcessTask(template.PK.ToGuid(), "P0", type, description, seq);

		void AssertProcessTask(Guid parentid, string tablecode, string type, string description, int seq = 1) => AssertEquals(1, Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.PROCESSTASKS WHERE P9_TYPE='{type}' AND P9_ParentID='{parentid}' AND P9_ParentTableCode='{tablecode}' AND P9_Description='{description}' AND P9_Sequence = {seq}"));

		void AssertHolidayEmailTriggerExists(Guid parentId, string description, string eventCode) => AssertTrigger(parentId, "GA", description, eventCode);

		void AssertTemplateEmailTriggerExists(string description, string eventCode) => AssertTrigger(template.PK.ToGuid(), "P0", description, eventCode);

		void AssertTrigger(Guid parentId, string tablecode, string description, string eventCode) => AssertEquals(1, Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.PROCESSTASKS WHERE P9_TYPE='TRG' AND P9_ParentID='{parentId}' AND P9_ParentTableCode='{tablecode}' AND P9_Description='{description}' AND P9_SE_NKMilestoneEvent='{eventCode}'"));

		ProcessTask AddEmailTriggerToTemplate(string description, string eventCode, string emailText = "This is the email content", string emailAddress = "glowtest@test.com", string linetrigger = "")
		{
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = description;
			trigger.P9_Type = "TRG";
			trigger.TriggerConditions.TriggerEventCode = eventCode;
			trigger.P9_LineTriggerType = linetrigger;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_EmailText = emailText;
			action.PQ_TriggerParty = "EML";
			action.PQ_MessagePurpose = "INV";
			action.PQ_EmailAddr = emailAddress;

			factory.Save();
			return trigger;
		}

		static void AssertEmailSent(string message, string[] recipients, string textContainedInBody)
		{
			foreach (var recipient in recipients)
			{
				var email = Env.AllEmailsCreated.SingleOrDefault(e => e.Recipients.Cast<RecipientDef>().Any(r => r.Email == recipient));

				CombineAssertions(message + ", recipient " + recipient, () =>
				{
					AssertNotNull("An email should have been sent", email);
					AssertContains("Body", textContainedInBody, email.Body);
				});
			}
		}

		void WorkflowTest(Action setupWorkflow = null, Action test = null)
		{
			template = null;
			staff = null;
			factory = null;
			Env.ClearAllEmailsCreated();

			using (new DisposableAction(() => processTasksEnabled = false))
			{
				var tempValue = new EnableAddEditAndDeleteLogsItemCollection();
				tempValue.Add(new EnableAddEditAndDeleteLogsItem()
				{
					Table = ProcessTasksSchema.Constants.TableName,
					EnableADDLogs = processTasksEnabled,
					EnableEDTLogs = processTasksEnabled,
					EnableDELLogs = processTasksEnabled,
				});

				using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
				{
					DisableSHOWorkflows();
					SetupWorkflow(setupWorkflow);
					test();
				}
			}
		}

		void SetupWorkflow(Action setup)
		{
			factory = new BusinessObjectFactory();
			template = factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Staff Holiday Test";
			template.P0_ProcessType = "SHO";
			template.P0_IsActive = true;
			staff = factory.New<GlbStaff>();
			setup();
			factory.Save();
		}

		Guid ManuallyInsertHoliday()
		{
			var holidayID = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery($@"
INSERT INTO dbo.GLBSTAFFHOLIDAY(GA_PK, GA_RecordType, GA_WorkHolidayType, GA_ApprovalStatus, Ga_GS, GA_StartTime, GA_SystemCreateTimeUtc, GA_SystemCreateUser, GA_SystemLastEditUser, GA_SystemLastEditTimeUtc)
VALUES('{holidayID}', 'LEV', 'ANN', 'REQ', '{staff.PK}', '2023-06-16T11:35:10', GETUTCDATE(), 'TST', 'TST', GETUTCDATE())
");
			return holidayID;
		}

		Guid ManuallyInsertStmALog(Guid holidayId, string eventType = "ADD", string table = "GlbStaffHoliday", int fireWorkflow = 1)
		{
			var stmalogID = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery($@"
INSERT INTO dbo.STMALOG(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_GS_nkuser, SL_SE_NKEvent, SL_GB_NKBranch, SL_GE_NKDepartment, SL_FireWorkflow)
VALUES('{stmalogID}', '{table}', '{holidayId}', GETUTCDATE(), GETDATE(), 'TST', '{eventType}', 'BRN', 'SYD', {fireWorkflow})");
			return stmalogID;
		}

		Guid GetProcessTaskId(Guid holidayID) => Db.Connection.ExecuteScalar<Guid>($@"SELECT P9_PK FROM dbo.PROCESSTASKS WHERE P9_ParentTableCode = 'GA' AND P9_ParentID = '{holidayID}'");

		void DisableSHOWorkflows() => Db.Connection.ExecuteNonQuery("UPDATE dbo.ProcessTaskTemplate SET P0_IsActive = 0 WHERE P0_ProcessType = 'SHO'");

		ProcessTaskTemplate template;
		GlbStaff staff;
		BusinessObjectFactory factory;
		bool processTasksEnabled;
	}
}
