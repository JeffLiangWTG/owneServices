using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CompetitorUserControl))]
	sealed class CompetitorUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new CompetitorUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyCompetitor" }; }
		}

		public override string GetCheckpointNameFromSecurityControlItem(string itemName)
		{
			return "CompetitorIntelligenceModify";
		}

		public void TestTabPageSecurity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCompetitor = false;
			Env.Security.OrgCompetitorView.IsAllowed = true;
			Env.Security.CompetitorIntelligenceView.IsAllowed = true;

			using (var form = new ZForm(org))
			using (var control = new CompetitorUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(2, control.CompetitorTabControl.TabPages.Count);
				foreach (ZTabPage tabPage in control.CompetitorTabControl.TabPages)
				{
					AssertEquals(0, tabPage.Controls.Find("coveringLabel", false).Length);
					AssertNull(tabPage.LicenceCheckpoint);
				}
			}

			org.OH_IsCompetitor = true;
			Env.Security.OrgCompetitorView.IsAllowed = false;
			Env.Security.CompetitorIntelligenceView.IsAllowed = false;

			using (var form = new ZForm(org))
			using (var control = new CompetitorUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(2, control.CompetitorTabControl.TabPages.Count);
				foreach (ZTabPage tabPage in control.CompetitorTabControl.TabPages)
				{
					AssertEquals(1, tabPage.Controls.Find("coveringLabel", false).Length);
					AssertNull(tabPage.LicenceCheckpoint);
				}
			}

			Env.Security.OrgCompetitorView.IsAllowed = true;
			Env.Security.CompetitorIntelligenceView.IsAllowed = true;

			using (var form = new ZForm(org))
			using (var control = new CompetitorUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(2, control.CompetitorTabControl.TabPages.Count);
				foreach (ZTabPage tabPage in control.CompetitorTabControl.TabPages)
				{
					AssertEquals(0, tabPage.Controls.Find("coveringLabel", false).Length);
					AssertNotNull(tabPage.LicenceCheckpoint);
				}
			}
		}
	}
}
