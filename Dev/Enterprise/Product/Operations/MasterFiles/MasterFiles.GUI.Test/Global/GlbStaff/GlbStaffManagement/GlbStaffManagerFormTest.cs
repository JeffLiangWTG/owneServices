using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbStaffManagerForm))]
	sealed class GlbStaffManagerFormTest : ZFormBasherTest
	{
		public void TestClickOkButtonShouldSetEndDateForExistingNonSharedRoleManager()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "PRM", new ZDateTime(2019, 05, 04));
			Factory.Save();

			var managerRecord2 = Factory.New<GlbStaffManager>();
			managerRecord2.GSM_GS_Staff = staff.PK;
			managerRecord2.GSM_GS_Manager = manager2.PK;
			managerRecord2.GSM_ManagerType = "PRM";
			managerRecord2.GSM_EffectiveDate = new ZDateTime(2019, 05, 15);

			using (var form = new GlbStaffManagerForm(managerRecord2))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();

				form.OkButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Existing manager's end date should be set to 1 day preceding the new record.", new ZDateTime(2019, 05, 14), managerRecord1.GSM_EndDate);
			}
		}

		public void TestClickCancelButtonShouldRevertChanges()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "PRM", new ZDateTime(2019, 05, 04));

			var managerRecord2 = Factory.New<GlbStaffManager>();
			managerRecord2.GSM_GS_Staff = staff.PK;
			managerRecord2.GSM_GS_Manager = manager2.PK;
			managerRecord2.GSM_ManagerType = "PRM";
			managerRecord2.GSM_EffectiveDate = new ZDateTime(2019, 05, 15);
			Factory.Save();

			using (var form = new GlbStaffManagerForm(managerRecord2))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();

				form.CancelButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Existing manager's end date should revert.", true, managerRecord1.GSM_EndDate.IsEmpty);
			}
		}

		public void TestClickCancelButtonShouldDeleteUnsavedManager()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "PRM", new ZDateTime(2019, 05, 04));
			Factory.Save();

			var managerRecord2 = Factory.New<GlbStaffManager>();
			managerRecord2.GSM_GS_Staff = staff.PK;
			managerRecord2.GSM_GS_Manager = manager2.PK;
			managerRecord2.GSM_ManagerType = "PRM";
			managerRecord2.GSM_EffectiveDate = new ZDateTime(2019, 05, 15);

			using (var form = new GlbStaffManagerForm(managerRecord2))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();

				form.CancelButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Existing manager should be deleted.", true, managerRecord2.IsDeleted);
			}
		}

		[TestDate(2019, 05, 05)]
		public void TestClickOkButtonWhenPreviousManagerEffectiveDateIsToday()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			manager1.GS_FullName = "Alex Aardvark";
			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "PRM", new ZDateTime(2019, 05, 05));
			Factory.Save();

			var managerRecord2 = Factory.New<GlbStaffManager>();
			managerRecord2.GSM_GS_Staff = staff.PK;
			managerRecord2.GSM_GS_Manager = manager2.PK;
			managerRecord2.GSM_ManagerType = "PRM";
			managerRecord2.GSM_EffectiveDate = new ZDateTime(2019, 05, 05);

			using (var form = new GlbStaffManagerForm(managerRecord2))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.OkButton.PerformClick();

				AssertEquals("Existing manager Alex Aardvark is being superseded despite their effective date being the same as the new manager. The pre-existing record will be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Existing manager should not have been deleted.", false, managerRecord1.IsDeleted);
				AssertEquals("Existing manager's end date should not have changed.", ZDateTime.Empty, managerRecord1.GSM_EndDate);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OkButton.PerformClick();

				AssertEquals("Existing manager Alex Aardvark is being superseded despite their effective date being the same as the new manager. The pre-existing record will be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Existing manager should have been deleted.", true, managerRecord1.IsDeleted);
			}
		}

		[RequiresSTA]
		public void TestInvalidEffectiveDateMessageForNonSharedRole()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "PRM", new ZDateTime(2019, 05, 04));
			Factory.Save();

			var managerRecord2 = StaffManagerTestHelper.AddManager(staff, manager2, "PRM", new ZDateTime(2019, 05, 03));

			using (var form = new GlbStaffManagerForm(managerRecord2))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OkButton.PerformClick();

				AssertEquals("The new Reporting Manager's Effective Date cannot precede any existing managers of the same role. To add historical records, click the History button from the Staff Form's Reporting Manager Grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Existing manager's end date should not have changed.", ZDateTime.Empty, managerRecord1.GSM_EndDate);
			}
		}

		public void TestSelfManagedSharedRoleShouldNotAllowClose()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var staffSelfManagedRecord = StaffManagerTestHelper.AddManager(staff, staff, "HRM", new ZDateTime(2019, 05, 04));
			Factory.Save();

			var manager1Record = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 05, 04));

			using (var form = new GlbStaffManagerForm(manager1Record))
			{
				var isClosed = false;

				form.Show();
				form.FormClosed += (o, e) => { isClosed = true; };

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OkButton.PerformClick();

				AssertNotNull(UnitTestUserNotification.Instance.PreviousMessages.Where(t => t.Text == "One or more existing Reporting Managers exist for this role. Do you wish to supersede the existing Manager(s)?"));
				AssertEquals("Self-managed roles cannot be shared.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Existing manager's end date should not have changed.", ZDateTime.Empty, staffSelfManagedRecord.GSM_EndDate);
				AssertEquals("Form should not have closed.", false, isClosed);
			}
		}

		public void TestInvalidEffectiveDateMessageForSharedRole()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 05, 04));
			Factory.Save();

			var managerRecord2 = StaffManagerTestHelper.AddManager(staff, manager2, "HRM", new ZDateTime(2019, 05, 03));

			using (var form = new GlbStaffManagerForm(managerRecord2))
			{
				var isClosed = false;

				form.Show();
				form.FormClosed += (o, e) => { isClosed = true; };

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OkButton.PerformClick();

				AssertNotNull(UnitTestUserNotification.Instance.PreviousMessages.Where(t => t.Text == "One or more existing Reporting Managers exist for this role. Do you wish to supersede the existing Manager(s)?"));
				AssertEquals("The new Reporting Manager's Effective Date cannot precede any existing managers of the same role. To add historical records, click the History button from the Staff Form's Reporting Manager Grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Existing manager's end date should not have changed.", ZDateTime.Empty, managerRecord1.GSM_EndDate);
				AssertEquals("Form should not have closed.", false, isClosed);
			}
		}

		public void TestSupersedeSharedRole()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var manager3 = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 03, 04), new ZDateTime(2019, 08, 12));
			var managerRecord2 = StaffManagerTestHelper.AddManager(staff, manager2, "HRM", new ZDateTime(2019, 05, 04));
			Factory.Save();

			var managerRecord3 = StaffManagerTestHelper.AddManager(staff, manager3, "HRM", new ZDateTime(2019, 05, 10));

			using (var form = new GlbStaffManagerForm(managerRecord3))
			{
				var isClosed = false;

				form.Show();
				form.FormClosed += (o, e) => { isClosed = true; };

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.OkButton.PerformClick();

				AssertNotNull(UnitTestUserNotification.Instance.PreviousMessages.Where(t => t.Text == "One or more existing Reporting Managers exist for this role. Do you wish to supersede the existing Manager(s)?"));
				AssertEquals("Form should have saved", true, isClosed);
				AssertEquals("Existing manager's end date should be set to 1 day preceding the new record.", new ZDateTime(2019, 05, 09), managerRecord1.GSM_EndDate);
				AssertEquals("Existing manager's end date should be set to 1 day preceding the new record.", new ZDateTime(2019, 05, 09), managerRecord2.GSM_EndDate);
			}
		}

		public void TestDoNotSupersedeSharedRole()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var manager3 = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 03, 04), new ZDateTime(2019, 08, 12));
			var managerRecord2 = StaffManagerTestHelper.AddManager(staff, manager2, "HRM", new ZDateTime(2019, 05, 04));
			Factory.Save();

			var managerRecord3 = StaffManagerTestHelper.AddManager(staff, manager3, "HRM", new ZDateTime(2019, 05, 10));

			using (var form = new GlbStaffManagerForm(managerRecord3))
			{
				var isClosed = false;

				form.Show();
				form.FormClosed += (o, e) => { isClosed = true; };

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.OkButton.PerformClick();

				AssertNotNull(UnitTestUserNotification.Instance.PreviousMessages.Where(t => t.Text == "One or more existing Reporting Managers exist for this role. Do you wish to supersede the existing Manager(s)?"));
				AssertEquals("Form should have saved", true, isClosed);
				AssertEquals("Existing manager's end date should not have changed.", new ZDateTime(2019, 08, 12), managerRecord1.GSM_EndDate);
				AssertEquals("Existing manager's end date should not have changed.", ZDateTime.Empty, managerRecord2.GSM_EndDate);
			}
		}

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var manager = Factory.NewWithValidTestData<GlbStaffManager>();
			Factory.Save();
			return new GlbStaffManagerForm(manager);
		}

		#endregion
	}
}
