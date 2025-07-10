using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SalesClientSummaryUserControlTest : TestCaseWithFactory
	{
		public void TestCompetitorTypeGridExpectedColumns()
		{
			using (var control = new SalesClientSummaryUserControlForTest())
			{
				var columns = control.CompetitorTypeGrid_Exposed.ColumnStyles.Cast<ZGridColumnInfo>();

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"OCP_Type",
					"Description",
					"OCP_OH_Competitor",
					"OrganisationName",
					"CompanyLevel"
				}, columns.Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestSubscriptionsSecurityCheck()
		{
			var staffWithAccess = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferences.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithAccess.PK;
			staffWithAccess.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			var staffWithoutAccess = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferences.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutAccess.PK;
			staffWithoutAccess.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Subscriptions.AddNew();

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "e@ma.il";
			contact.Subscriptions.AddNew();
			Factory.Save();

			var reloadedOrg = Factory.Load<OrgHeader>(org.PK);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new ZForm(reloadedOrg))
				using (var control = new SalesClientSummaryUserControlForTest())
				{
					form.Controls.Add(control);
					form.Show();

					AssertEquals(false, control.SubscriptionsGrid_Exposed.ReadOnly);
				}
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new ZForm(reloadedOrg))
				using (var control = new SalesClientSummaryUserControlForTest())
				{
					form.Controls.Add(control);
					form.Show();

					AssertEquals(true, control.SubscriptionsGrid_Exposed.ReadOnly);
				}
			}
		}

		public void TestSubscriptionsGridChangingPreferencesShouldShowPopupOnce()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "e@ma.il";
			Factory.Save();

			using (var form = new ZForm(org))
			using (var control = new SalesClientSummaryUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				//Need to change the test to do for CurrentCellChanged
				control.SubscriptionsGrid_Exposed.Focus();
				control.SubscriptionsGrid_Exposed.CurrentCell = new DataGridCell(0, 0);
				control.SubscriptionsGrid_Exposed.CurrentCell = new DataGridCell(0, 1);
				var message = (UnitTestUserNotification)Globals.Message;

				AssertEquals("Popup should show", "Un-subscribing this Organization from campaign categories/types will overwrite existing related Subscription Preferences for the Organization's Contacts.", message.LastMessage.Text);
				message.ClearMessages();
				AssertNull(message.LastMessage.Text);

				control.SubscriptionsGrid_Exposed.Focus();
				control.SubscriptionsGrid_Exposed.CurrentCell = new DataGridCell(0, 0);
				control.SubscriptionsGrid_Exposed.CurrentCell = new DataGridCell(0, 1);

				AssertNull(message.LastMessage.Text);
			}
		}

		#region Implementation

		public class SalesClientSummaryUserControlForTest : SalesClientSummaryUserControl
		{
			public ZGrid SubscriptionsGrid_Exposed => SubscriptionsGrid;

			public ZGrid CompetitorTypeGrid_Exposed => CompetitorTypeGrid;
		}

		#endregion
	}
}
