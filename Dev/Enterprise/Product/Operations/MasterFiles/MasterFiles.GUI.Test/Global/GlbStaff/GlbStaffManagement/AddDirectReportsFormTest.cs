using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AddDirectReportsForm))]
	sealed class AddDirectReportsFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestAddDirectReportsButton()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(staff, "PRM");

			using (var form = new AddDirectReportsForm(addDirectReportsBiZo))
			{
				form.Show();
				var formClosed = false;
				form.Closed += (o, e) => { formClosed = true; };
				form.FilterItemModule.PerformSearch_ForTest();
				form.FilterItemModule.DisplayGrid.SelectSingleElement(staff);
				form.AddDirectReportsButton.PerformClick();
				AssertEquals("Form should close after direct reports added", true, formClosed);
				AssertEquals("The staff member should have been added as their own manager", 1, staff.DirectReports.Count);
				AssertEquals("The staff member should have been added as their own manager", staff, staff.DirectReports[0].Staff);
			}
		}

		[RequiresSTA]
		public void TestAddDirectReportsButtonSharedRole()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(staff, "HRM");

			using (var form = new AddDirectReportsForm(addDirectReportsBiZo))
			{
				form.Show();
				var formClosed = false;
				form.Closed += (o, e) => { formClosed = true; };
				form.FilterItemModule.PerformSearch_ForTest();
				form.FilterItemModule.DisplayGrid.SelectSingleElement(staff);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.AddDirectReportsButton.PerformClick();
				AssertEquals("Form should not close since cancel was clicked", false, formClosed);
				AssertEquals("No managers should be added", 0, staff.DirectReports.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.AddDirectReportsButton.PerformClick();
				AssertEquals("Form should close after direct reports added", true, formClosed);
				AssertEquals("The staff member should have been added as their own manager", 1, staff.DirectReports.Count);
				AssertEquals("The staff member should have been added as their own manager", staff, staff.DirectReports[0].Staff);
			}
		}

		[RequiresSTA]
		public void TestAddDirectReportsButtonSharedRoleSupersede()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var existingReport = Factory.NewWithValidTestData<GlbStaff>();
			var existingReportManagerRecord = StaffManagerTestHelper.AddManager(existingReport, existingReport, "HRM", new ZDateTime(2019, 04, 04));
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "HRM")
			{
				EffectiveDate = new ZDateTime(2019, 06, 13)
			};

			using (var form = new AddDirectReportsForm(addDirectReportsBiZo))
			{
				form.Show();
				var formClosed = false;
				form.Closed += (o, e) => { formClosed = true; };
				form.FilterItemModule.PerformSearch_ForTest();
				form.FilterItemModule.DisplayGrid.SelectAllElements(x => ((GlbStaff)x).GS_LoginName != User.SupportUserName);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.AddDirectReportsButton.PerformClick();
				AssertEquals("Form should close after direct reports added", true, formClosed);
				AssertEquals("ExistingReport's manager should have been superseded", 2, manager.DirectReports.Count);
				AssertEquals("ExistingReport's manager should have been superseded", false, existingReportManagerRecord.IsDeleted);
				AssertEquals("ExistingReport's manager should have been superseded", addDirectReportsBiZo.EffectiveDate.AddDays(-1), existingReportManagerRecord.GSM_EndDate);
			}
		}

		[TestDate(2019, 06, 13)]
		[RequiresSTA]
		public void TestAddDirectReportsButtonSharedRoleShare()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var existingReport = Factory.NewWithValidTestData<GlbStaff>();
			var existingFutureReport = Factory.NewWithValidTestData<GlbStaff>();
			var existingReportManagerRecord = StaffManagerTestHelper.AddManager(existingReport, existingReport, "HRM", new ZDateTime(2019, 04, 04));
			var existingFutureReportManagerRecord = StaffManagerTestHelper.AddManager(existingReport, existingReport, "HRM", new ZDateTime(2019, 08, 13));
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "HRM")
			{
				EffectiveDate = new ZDateTime(2019, 06, 13)
			};

			using (var form = new AddDirectReportsForm(addDirectReportsBiZo))
			{
				form.Show();
				var formClosed = false;
				form.Closed += (o, e) => { formClosed = true; };
				form.FilterItemModule.PerformSearch_ForTest();
				form.FilterItemModule.DisplayGrid.SelectAllElements(x => ((GlbStaff)x).GS_LoginName != User.SupportUserName);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); 
				form.AddDirectReportsButton.PerformClick();
				AssertEquals("Form should close after direct reports added", true, formClosed);
				AssertEquals("ExistingReport's manager should not have been superseded", 3, manager.DirectReports.Count);
				AssertEquals("ExistingReport's manager should not have been superseded", false, existingReportManagerRecord.IsDeleted);
				AssertEquals("ExistingFutureReport's manager should not have been superseded", false, existingFutureReportManagerRecord.IsDeleted);
				AssertEquals("ExistingReport's manager should not have been superseded", ZDateTime.Empty, existingReportManagerRecord.GSM_EndDate);
				AssertEquals("ExistingFutureReport's manager should not have been superseded", ZDateTime.Empty, existingFutureReportManagerRecord.GSM_EndDate);
			}
		}

		[TestDate(2019, 05, 05)]
		[RequiresSTA]
		public void TestAddDirectReportsButtonExistingManagerEncompassed()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var existingReport = Factory.NewWithValidTestData<GlbStaff>();
			existingReport.GS_FullName = "Toad Stool";
			var existingReport2 = Factory.NewWithValidTestData<GlbStaff>();
			existingReport2.GS_FullName = "Toast Duel";
			StaffManagerTestHelper.AddManager(existingReport, existingReport, "PRM", new ZDateTime(2019, 03, 04));
			StaffManagerTestHelper.AddManager(existingReport2, existingReport2, "PRM", new ZDateTime(2019, 03, 04));
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "PRM")
			{
				EffectiveDate = new ZDateTime(2019, 01, 01)
			};

			using (var form = new AddDirectReportsForm(addDirectReportsBiZo))
			{
				form.Show();
				var formClosed = false;
				form.Closed += (o, e) => { formClosed = true; };
				form.FilterItemModule.PerformSearch_ForTest();
				form.FilterItemModule.GridCollection.ApplySort(existingReport.GS_FullNameInfo.PropertyDescriptor,ListSortDirection.Descending);
				form.FilterItemModule.DisplayGrid.SelectAllElements(x => ((GlbStaff)x).GS_LoginName != User.SupportUserName);
				form.AddDirectReportsButton.PerformClick();
				AssertEquals("Form should close after direct reports added", true, formClosed);
				AssertEquals("ExistingReports' managers should not have been superseded", 1, manager.DirectReports.Count);
				AssertEquals("The staff member should have been added as their own manager", manager, manager.DirectReports[0].Staff);
				AssertEquals("The following Direct Reports could not be set since their existing manager(s)' effective dates are preceded by the new record. Update these staff members manually from their staff record:\r\nToast Duel\r\nToad Stool", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestAddDirectReportsButtonValidation()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "XRM")
			{
				EffectiveDate = new ZDateTime(2019, 06, 13)
			};

			using (var form = new AddDirectReportsForm(addDirectReportsBiZo))
			{
				form.Show();
				var formClosed = false;
				form.Closed += (o, e) => { formClosed = true; };
				form.FilterItemModule.PerformSearch_ForTest();
				form.FilterItemModule.DisplayGrid.SelectAllElements();
				form.AddDirectReportsButton.PerformClick();
				AssertEquals("Validation Error", "Fix validation errors before continuing", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No direct reports should have been added", 0, manager.DirectReports.Count);
				AssertEquals("Form should not close", false, formClosed);
			}
		}

		[RequiresSTA]
		public void TestAddDirectReportsButtonNoSelectedElements()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "PRM")
			{
				EffectiveDate = new ZDateTime(2019, 06, 13)
			};

			using (var form = new AddDirectReportsForm(addDirectReportsBiZo))
			{
				form.Show();
				var formClosed = false;
				form.Closed += (o, e) => { formClosed = true; };
				form.AddDirectReportsButton.PerformClick();
				AssertEquals("No records selected error", "Please select one or more records from the grid", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No direct reports should have been added", 0, manager.DirectReports.Count);
				AssertEquals("Form should not close", false, formClosed);
			}
		}

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "XRM");
			Factory.Save();
			return new AddDirectReportsForm(addDirectReportsBiZo);
		}

		#endregion
	}
}
