using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	class CustomsSendMessageHelperTest : TestCaseWithFactory
	{
		public void TestIsSupervisorApprovedSendingWithMessageErrors_WithClearance()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var user = Factory.New<GlbStaff>();
			user.FillWithValidTestData();
			user.GS_Code = "YYY";
			user.GS_LoginName = "admin";
			user.GS_IsController = true;
			user.StaffPlainTextPassword = "password";
			user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			group.Staff.Add(user);
			Factory.Save();

			Env.Security.AllowMessageErrors.IsAllowed = false;

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.InitAction(Messaging.Business.UpdateActionCode.Add);
			clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single().SendToCustoms = true;

			clearance.RunPreSaveValidation();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AssertEquals("Should not be approved", false, CustomsSendMessageHelper.IsSupervisorApprovedSendingWithMessageErrors(clearance, clearance.Logs));

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertEquals("Should be approved", true, CustomsSendMessageHelper.IsSupervisorApprovedSendingWithMessageErrors(clearance, clearance.Logs));
		}

		public void TestIsSupervisorApprovedSendingWithMessageErrors_WithConsignment()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var user = Factory.New<GlbStaff>();
			user.FillWithValidTestData();
			user.GS_Code = "YYY";
			user.GS_LoginName = "admin";
			user.GS_IsController = true;
			user.StaffPlainTextPassword = "password";
			user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			group.Staff.Add(user);
			Factory.Save();

			Env.Security.AllowMessageErrors.IsAllowed = false;

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.InitAction(Messaging.Business.UpdateActionCode.Add);
			clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single().SendToCustoms = true;

			consignment.MarkAsNeedingValidationIncludingChildren();
			consignment.RunPreSaveValidation();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AssertEquals("Should not be approved", false, CustomsSendMessageHelper.IsSupervisorApprovedSendingWithMessageErrors(consignment, consignment.Logs));

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertEquals("Should be approved", true, CustomsSendMessageHelper.IsSupervisorApprovedSendingWithMessageErrors(consignment, consignment.Logs));
		}

		public void TestMutexLocked_Message()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryNum = "1234";
			clearance.LockSendCustomsMessageMutex();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			CustomsSendMessageHelper.SendToCustoms(clearance, [new CusUSLVConsignmentForMessaging(consignment)], Messaging.Business.UpdateActionCode.Add);

			AssertEndsWith("", "is sending messages for this Low Value Entries job, please try again later.", UnitTestUserNotification.Instance.LastMessage.Text);
			clearance.UnlockSendCustomsMessageMutex();
		}
	}
}
