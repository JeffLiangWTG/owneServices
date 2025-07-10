using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SalesTeamForm))]
	sealed class SalesTeamFormTest : ZFormBasherTest
	{
		#region Security

		public void TestHideCommissionTabPageIfSecurityDenied()
		{
			Env.Security.SalesTeamsEditCommission.IsAllowed = true;
			Env.Security.SalesTeamsViewCommission.IsAllowed = true;
			using (var form = new SalesTeamFormForTesting())
			{
				form.Show();
				AssertHasCoveringLabel(form.CommissionTabPage_Exposed, false);
			}

			Env.Security.SalesTeamsEditCommission.IsAllowed = true;
			Env.Security.SalesTeamsViewCommission.IsAllowed = false;
			using (var form = new SalesTeamFormForTesting())
			{
				form.Show();
				AssertHasCoveringLabel(form.CommissionTabPage_Exposed, false);
			}

			Env.Security.SalesTeamsEditCommission.IsAllowed = false;
			Env.Security.SalesTeamsViewCommission.IsAllowed = true;
			using (var form = new SalesTeamFormForTesting())
			{
				form.Show();
				AssertHasCoveringLabel(form.CommissionTabPage_Exposed, false);
			}

			Env.Security.SalesTeamsEditCommission.IsAllowed = false;
			Env.Security.SalesTeamsViewCommission.IsAllowed = false;
			using (var form = new SalesTeamFormForTesting())
			{
				form.Show();
				AssertHasCoveringLabel(form.CommissionTabPage_Exposed, true);
			}
		}

		[RequiresSTA]
		public void TestCommissionTabPageIsReadOnlyIfSecurityDenied()
		{
			Env.Security.SalesTeamsViewCommission.IsAllowed = true;

			Env.Security.SalesTeamsEditCommission.IsAllowed = false;
			using (var form = new SalesTeamFormForTesting())
			{
				form.Show();
				form.MainTabControl_Exposed.SelectedTab = form.CommissionTabPage_Exposed;
				AssertTabPageReadOnly(form.CommissionTabPage_Exposed, true);
			}

			Env.Security.SalesTeamsEditCommission.IsAllowed = true;
			using (var form = new SalesTeamFormForTesting())
			{
				form.Show();
				form.MainTabControl_Exposed.SelectedTab = form.CommissionTabPage_Exposed;
				AssertTabPageReadOnly(form.CommissionTabPage_Exposed, false);
			}
		}

		void AssertHasCoveringLabel(ZTabPage tabPage, bool expected)
		{
			var topControl = tabPage.Controls[0];
			var topLabel = topControl as ZLabel;
			if (topLabel == null)
			{
				AssertEquals(expected, false);
			}
			else
			{
				if (expected)
				{
					AssertEquals(DockStyle.Fill, topLabel.Dock);
				}
				else
				{
					AssertNotEquals(DockStyle.Fill, topLabel.Dock);
				}
			}
		}

		void AssertTabPageReadOnly(ZTabPage tabPage, bool expected)
		{
			foreach (Control control in tabPage.Controls)
			{
				AssertEquals(expected, !control.Enabled);
			}
		}

		#endregion

		public void TestFormCaption()
		{
			using (SalesTeamForm salesTeamForm = (SalesTeamForm)GetFormToBashCore())
			{
				AssertEquals("Form caption is Sales Team", "Sales Team", Res.GetString("cfc8df08-8570-4e34-bfd7-bbf033a1790f", "Sales Team"));
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return NewSalesTeamForm(Factory.New<SalesTeam>());
		}

		SalesTeamForm NewSalesTeamForm(SalesTeam salesTeam)
		{
			return new SalesTeamForm(salesTeam);
		}

		#endregion

		#region Classes

		class SalesTeamFormForTesting : SalesTeamForm
		{
			public SalesTeamFormForTesting()
			{
			}

			public ZTemplateTabControl MainTabControl_Exposed
			{
				get { return base.MainTabControl; }
			}

			public ZTabPage CommissionTabPage_Exposed
			{
				get { return base.CommissionTabPage; }
			}
		}

		#endregion
	}
}
