using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WhsProductPrefsUserControlTest : KUserControlTest
	{
		#region TestSerialNumberCheckBox

		public void TestSerialNumberCheckBox()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZForm(orgHeader))
			using (var control = new WhsProductPrefsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.UseSerialNumberCheckBox.Visible);
			}
		}

		#endregion

		#region TestControlsAreSetToReadOnlyCorrectly

		public void TestControlsAreSetToReadOnlyCorrectly()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZForm(orgHeader))
			using (var control = new WhsProductPrefsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.ABCAnalysisMethodDropEdit.ReadOnly);
				AssertEquals(true, control.ABCAnalysisPeriodDropEdit.ReadOnly);
				AssertEquals(false, control.IncludeClientInABCAnalysisCheckBox.ReadOnly);
				AssertEquals(false, control.IncludeClientInABCAnalysisCheckBox.Checked);

				control.IncludeClientInABCAnalysisCheckBox.Checked = true;
				AssertEquals(false, control.ABCAnalysisMethodDropEdit.ReadOnly);
				AssertEquals(false, control.ABCAnalysisPeriodDropEdit.ReadOnly);

				control.IncludeClientInABCAnalysisCheckBox.Checked = false;
				AssertEquals(true, control.ABCAnalysisMethodDropEdit.ReadOnly);
				AssertEquals(true, control.ABCAnalysisPeriodDropEdit.ReadOnly);

				control.IncludeClientInABCAnalysisCheckBox.Checked = true;
				control.ABCAnalysisMethodDropEdit.Text = "XXX";
				control.ABCAnalysisPeriodDropEdit.Text = "XXX";
				control.IncludeClientInABCAnalysisCheckBox.Checked = false;
				AssertEquals(true, control.ABCAnalysisMethodDropEdit.ReadOnly);
				AssertEquals("DEF", control.ABCAnalysisMethodDropEdit.Text);
				AssertEquals(true, control.ABCAnalysisPeriodDropEdit.ReadOnly);
				AssertEquals("DEF", control.ABCAnalysisPeriodDropEdit.Text);
			}
		}

		#endregion

		#region TestInitialReadOnly

		public void TestInitialReadOnly()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MiscServ.OM_WhsABCAnalysisEnabled = true;

			using (var form = new ZForm(orgHeader))
			using (var control = new WhsProductPrefsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.ABCAnalysisPeriodDropEdit.ReadOnly);
				AssertEquals(false, control.ABCAnalysisMethodDropEdit.ReadOnly);
			}
		}

		#endregion
	}
}
