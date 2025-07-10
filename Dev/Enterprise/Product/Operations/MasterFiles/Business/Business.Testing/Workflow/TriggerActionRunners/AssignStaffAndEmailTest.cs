using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AssignStaffAndEmailTest : TestCaseWithFactory
	{
		[TestDate(2020, 1, 1)]
		public void TestProcessTaskASETriggerActionsAreSetAndEmailWithFieldValueEvaluationAfterValidation()
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
				// Staff
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "XR1";
				staff.GS_LoginName = "XR1";
				staff.GS_EmailAddress = "stf@mail.com";

				var mgr = Factory.New<GlbStaff>();
				mgr.GS_Code = "XR2";
				staff.GS_LoginName = "XR2";
				mgr.GS_EmailAddress = "mgr@mail.com";

				var mgrLink = Factory.New<GlbStaffManager>();
				mgrLink.GSM_GS_Staff = staff.PK;
				mgrLink.GSM_GS_Manager = mgr.PK;
				mgrLink.GSM_ManagerType = "DRM";
				mgrLink.GSM_EffectiveDate = new ZDateTime(2019, 1, 1);

				staff.Managers.Add(mgrLink);

				//Job1
				var job1 = Factory.NewWithValidTestData<DummyWithWorkflow>();

				var task1_1 = job1.WorkflowItems.Tasks.AddNew();
				task1_1.P9_Description = "Task To Assign Staff";
				task1_1.P9_GS_NKAssignedStaffMember = string.Empty;
				task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

				var trigger1_1 = job1.WorkflowItems.Triggers.AddNew();
				trigger1_1.P9_Description = "Trigger me";
				trigger1_1.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;
				trigger1_1.P9_LineTriggerType = "TSK";

				var action1_1_1 = trigger1_1.ProcessTaskNotifications.AddNew();
				action1_1_1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AssignStaffandEmail;
				action1_1_1.PQ_EmailText = "This is the email content";
				action1_1_1.PQ_FieldValue = $"<GetCurrentManagerCodeByType(\"{staff.PK.ToString()}\",\"DRM\")>";

				Factory.Save();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				job1.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				Factory.Save();
				Env.ClearAllEmailsCreated();
				var logwalker = MasterFilesTestHelper.RunLogWalker();

				job1.Reload();
				job1.WorkflowItems.Reload(true);
				AssertEquals("Should assign task to manager", mgr.GS_Code, job1.WorkflowItems.Tasks[0].P9_GS_NKAssignedStaffMember);
				AssertEmailSent("Email should have been triggered for ADD event", new[] { mgr.GS_EmailAddress.ToString() }, "This is the email content");
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestProcessTaskASETriggerActionsAreSetAndEmailAfterValidation()
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
				// Staff
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "XR1";
				staff.GS_EmailAddress = "stf@mail.com";

				//Job1
				var job1 = Factory.NewWithValidTestData<DummyWithWorkflow>();

				var task1_1 = job1.WorkflowItems.Tasks.AddNew();
				task1_1.P9_Description = "Task To Assign Staff";
				task1_1.P9_GS_NKAssignedStaffMember = string.Empty;
				task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

				var trigger1_1 = job1.WorkflowItems.Triggers.AddNew();
				trigger1_1.P9_Description = "Trigger me";
				trigger1_1.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;
				trigger1_1.P9_LineTriggerType = "TSK";

				var action1_1_1 = trigger1_1.ProcessTaskNotifications.AddNew();
				action1_1_1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AssignStaffandEmail;
				action1_1_1.PQ_EmailText = "This is the email content";
				action1_1_1.PQ_FieldValue = staff.GS_Code;

				Factory.Save();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				job1.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				Factory.Save();
				Env.ClearAllEmailsCreated();
				var logwalker = MasterFilesTestHelper.RunLogWalker();

				job1.Reload();
				job1.WorkflowItems.Reload(true);
				AssertEquals("Should assign task to staff", job1.WorkflowItems.Tasks[0].P9_GS_NKAssignedStaffMember, staff.GS_Code);
				AssertEmailSent("Email should have been triggered for ADD event", new[] { staff.GS_EmailAddress.ToString() }, "This is the email content");
			}
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
	}
}
