using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class StaffAssignmentsUserControlTest : TestCaseWithFactory
	{
		class StaffAssignmentsUserControlForTest : StaffAssignmentsUserControl
		{
			public ZCheckBox ShowForAllCompaniesCheckBoxGetter
			{
				get { return ShowForAllCompaniesCheckBox; }
			}
		}

		public void TestShowForAllCompaniesCheckBoxIsNeverReadOnly()
		{
			using (ZTestForm testForm = new ZTestForm())
			using (StaffAssignmentsUserControlForTest testControl = new StaffAssignmentsUserControlForTest())
			{
				testForm.Controls.Add(testControl);
				testControl.SetReadOnlyIncludingChildren();
				Assert(!testControl.ShowForAllCompaniesCheckBoxGetter.ReadOnly);
			}
		}

		public void TestSecurityDeniesShowingForAllCompanies()
		{
			bool previousValue = Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed;

			try
			{
				OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

				using (ZTestForm testForm = new ZTestForm(testOrg))
				using (StaffAssignmentsUserControlForTest testControl = new StaffAssignmentsUserControlForTest())
				{
					testForm.Controls.Add(testControl);
					Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = false;

					testControl.ShowForAllCompaniesCheckBoxGetter.Checked = true;
					Assert(!testControl.ShowForAllCompaniesCheckBoxGetter.Checked);
					AssertEquals(Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(testOrg.StaffAssignments.CompanySpecific);

					Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = true;

					testControl.ShowForAllCompaniesCheckBoxGetter.Checked = true;
					Assert(testControl.ShowForAllCompaniesCheckBoxGetter.Checked);
					Assert(!testOrg.StaffAssignments.CompanySpecific);
				}
			}
			finally
			{
				Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = previousValue;
			}
		}

		[RequiresSTA]
		public void TestShowForAllCompaniesCheckBoxDefaultValueControlledByRegistryItem()
		{
			AssertShowForAllCompaniesCheckBoxDefaultValueControlledByRegistryItem(true, false);
			AssertShowForAllCompaniesCheckBoxDefaultValueControlledByRegistryItem(true, true);
			AssertShowForAllCompaniesCheckBoxDefaultValueControlledByRegistryItem(false, false);
			AssertShowForAllCompaniesCheckBoxDefaultValueControlledByRegistryItem(false, true);
		}

		#region Implementation

		void AssertShowForAllCompaniesCheckBoxDefaultValueControlledByRegistryItem(bool registryValue, bool securityRight)
		{
			var previousValue = Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed;

			try
			{
				var testOrg = Factory.New<OrgHeader>();

				using (var testForm = new ZForm(testOrg))
				using (var testControl = new StaffAssignmentsUserControlForTest())
				using (OrganisationRegistry.Instance.DefaultBehaviorOfShowForAllCompaniesCheckbox.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
				{
					Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = securityRight;
					testForm.Controls.Add(testControl);
					testForm.Show();
					AssertEquals(registryValue && securityRight, testControl.ShowForAllCompaniesCheckBoxGetter.Checked);
				}
			}
			finally
			{
				Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = previousValue;
			}
		}

		#endregion
	}
}
