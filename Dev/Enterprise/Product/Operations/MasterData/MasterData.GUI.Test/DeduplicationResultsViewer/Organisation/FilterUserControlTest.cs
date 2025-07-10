using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Test
{
	public class FilterUserControlTest : TestCase
	{
		public void TestExpandAndCollapseFilterArea()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var expandCollapseButton = potentialDuplicatesUserControl.FindSingle<ZButton>("ExpandCollapseButton");
				var tableLayoutPanel = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.FindSingle<KTableLayoutPanel>("MainTableLayoutPanel");
				CombineAssertions(() =>
				{
					AssertEquals(2, tableLayoutPanel.Controls.Count);
					AssertEquals("⯅ ", expandCollapseButton.Text);
				});

				expandCollapseButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, tableLayoutPanel.Controls.Count);
					AssertEquals("⯆ ", expandCollapseButton.Text);
				});

				expandCollapseButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(2, tableLayoutPanel.Controls.Count);
					AssertEquals("⯅ ", expandCollapseButton.Text);
				});
			}
		}
	}
}
