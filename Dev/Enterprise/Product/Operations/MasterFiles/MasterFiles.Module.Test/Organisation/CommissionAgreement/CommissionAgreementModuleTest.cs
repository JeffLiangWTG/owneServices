using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CommissionAgreementModule))]
	sealed class CommissionAgreementModuleTest : ZModuleBasherTest
	{
		#region ID

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgCommissionAgreement;
		}

		#endregion

		#region Allowed Actions

		public void TestAllowDelete()
		{
			using (var module = new CommissionAgreementModule())
			{
				AssertEquals(false, module.AllowDelete);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = new CommissionAgreementModule())
			{
				AssertEquals(true, module.AllowEdit);
			}
		}

		public void TestAllowNew()
		{
			using (var module = new CommissionAgreementModule())
			{
				AssertEquals(false, module.AllowNew);
			}
		}

		public void TestAllowView()
		{
			using (var module = new CommissionAgreementModule())
			{
				AssertEquals(true, module.AllowView);
			}
		}

		#endregion

		#region Security

		public void TestSecurity()
		{
			using (var module = new CommissionAgreementModule())
			using (var opportunityModule = ZModuleFactory.Instance.Create(ModuleIDs.Opportunity))
			{
				var expectedSecurityCheckpoint = opportunityModule.SecurityCheckpoint;
				AssertEquals("OrgCommissionAgreements are viewed on the Opportunity", expectedSecurityCheckpoint, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region Workflow

		public void TestSupportsWorkflow()
		{
			using (var module = new CommissionAgreementModule())
			{
				AssertEquals(false, module.SupportsWorkflow);
			}
		}

		#endregion

		#region Action Menus

		[RequiresSTA]
		public void TestRemoveFromCalculationQueueMenu()
		{
			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();

			var queue1 = Factory.New<OrgCommissionCalculationQueue>();
			queue1.CAQ_CA0 = agreement1.PK;
			Factory.Save();

			using (var module = new CommissionAgreementModule())
			{
				var menu = module.FormActionMenu.Single(x => x.Text == "&Actions")
					.MenuItems.OfType<MenuItem>()
					.SingleOrDefault(x => x.Text.Contains("Remove from Calculation Queue"));

				AssertNull(menu);
			}

			using (var module = new CommissionAgreementModule() { ShouldShowCalculationQueueActionMenuItems = true })
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var menu = module.FormActionMenu.Single(x => x.Text == "&Actions")
					.MenuItems.OfType<MenuItem>()
					.Single(x => x.Text.Contains("Remove from Calculation Queue"));

					menu.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

					Env.Security.CommissionCalculationQueueEdit.IsAllowed = false;
					menu.PerformClick();
					AssertEquals(Env.Security.CommissionCalculationQueueEdit.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();

					Env.Security.CommissionCalculationQueueEdit.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					menu.PerformClick();
					AssertEquals(@"Are you sure you wish to remove the Agreement(s) from the Calculation Queue?", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(true, queue1.IsDeleted);
				}
			}
		}

		#endregion
	}
}
