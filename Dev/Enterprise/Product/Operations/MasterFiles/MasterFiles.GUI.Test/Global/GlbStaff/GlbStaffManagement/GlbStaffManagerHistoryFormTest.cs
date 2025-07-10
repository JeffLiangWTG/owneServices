using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbStaffManagerHistoryForm))]
	sealed class GlbStaffManagerHistoryFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestGSM_GS_ManagerWarningsShouldPreventSaving()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var validManager = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord = StaffManagerTestHelper.AddManager(staff, validManager, "HRM");
			AssertNoErrors(managerRecord.GSM_GS_ManagerInfo);
			Factory.Save();

			var managerCollection = new GlbStaffManagerCollection(staff, "HRM");
			using (var form = new GlbStaffManagerHistoryForm(managerCollection))
			{
				var otherManagerRecord = StaffManagerTestHelper.AddManager(staff, staff, "HRM");
				AssertHasError(otherManagerRecord.GSM_GS_ManagerInfo, "A staff member cannot be their own manager.");
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.FireSaveButton();
				AssertEquals("Should not have saved", false, otherManagerRecord.IsInDatabase);
			}
		}

		[TestDate(2018, 01, 01)]
		[RequiresSTA]
		public void TestGSM_ManagerTypeWarningsShouldPreventSaving()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var firstManager = Factory.NewWithValidTestData<GlbStaff>();
			var secondManager = Factory.NewWithValidTestData<GlbStaff>();

			var firstManagerRecord = StaffManagerTestHelper.AddManager(staff, firstManager, "PRM", new ZDateTime(2017, 09, 09));
			AssertNoErrors(firstManagerRecord.GSM_ManagerTypeInfo);
			Factory.Save();

			var managerCollection = new GlbStaffManagerCollection(staff, "PRM");
			using (var form = new GlbStaffManagerHistoryForm(managerCollection))
			{
				var secondManagerRecord = StaffManagerTestHelper.AddManager(staff, secondManager, "PRM");
				AssertHasWarning(secondManagerRecord.GSM_ManagerTypeInfo, secondManagerRecord.Validation.CannotShareRoleMessage);
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.FireSaveButton();
				AssertEquals("Roles that have not been flagged as Shared Role Allowed in the registry cannot have more than one manager for any given date. Please manually fix records which are displaying warnings about shared roles so that effective date ranges do not overlap.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have saved", false, secondManagerRecord.IsInDatabase);

				firstManagerRecord.GSM_EndDate = new ZDateTime(2017, 12, 31);

				secondManagerRecord.RunPreSaveValidation();
				AssertNoErrors(secondManagerRecord.GSM_ManagerTypeInfo);

				form.FireSaveButton();
				AssertEquals("Should have saved", true, secondManagerRecord.IsInDatabase);
			}
		}

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			return new GlbStaffManagerHistoryForm(staff.Managers);
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		#endregion
	}
}
