using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(RunSheetDashboardForm))]
	public class RunSheetDashboardFormTest : ZFormBasherTest
	{
		public void TestFormText()
		{
			var dashboards = RunSheetDashboardCollection.New(Factory);
			using (RunSheetDashboardForm form = new RunSheetDashboardForm(dashboards))
			{
				form.Show();
				AssertEquals("Run Sheet Dashboard", form.Text);
				dashboards[0].DateRangeFilter = "Today";
				AssertEquals("Run Sheet Dashboard - Today", form.Text);
				dashboards.SwapFactoryRemoveAllAndAddNew();
				AssertEquals("Should still have same filter text as previously", "Run Sheet Dashboard - Today", form.Text);
				dashboards[0].DateRangeFilter = "Tomorrow";
				AssertEquals("New Dashboard should be hooked", "Run Sheet Dashboard - Tomorrow", form.Text);
				dashboards[0].Branches.Load();
				var selectedBranch = dashboards[0].Branches[0];
				dashboards[0].Branch = selectedBranch.PK;
				AssertEquals(string.Format("Run Sheet Dashboard - Tomorrow - {0}", selectedBranch.GB_BranchName), form.Text);
			}
		}

		public void TestFormTextDifferentLanguage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			{
				var dashboards = RunSheetDashboardCollection.New(Factory);
				using (RunSheetDashboardForm form = new RunSheetDashboardForm(dashboards))
				{
					form.Show();
					AssertEquals("Rollkarten-Konsole", form.Text);
					dashboards[0].DateRangeFilter = "Today";
					AssertEquals("Rollkarten-Konsole - Heute", form.Text);
					dashboards.SwapFactoryRemoveAllAndAddNew();
					AssertEquals("Should still have same filter text as previously", "Rollkarten-Konsole - Heute", form.Text);
					dashboards[0].DateRangeFilter = "Tomorrow";
					AssertEquals("New Dashboard should be hooked", "Rollkarten-Konsole - Morgen", form.Text);
					dashboards[0].Branches.Load();
					var selectedBranch = dashboards[0].Branches[0];
					dashboards[0].Branch = selectedBranch.PK;
					AssertEquals(string.Format("Rollkarten-Konsole - Morgen - {0}", selectedBranch.GB_BranchName), form.Text);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new RunSheetDashboardForm(RunSheetDashboardCollection.New(Factory));
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
		}
	}
}
