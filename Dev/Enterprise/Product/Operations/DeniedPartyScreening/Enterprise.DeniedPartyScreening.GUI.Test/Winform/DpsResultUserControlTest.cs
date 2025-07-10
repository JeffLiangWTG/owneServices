using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class DpsResultUserControlTest : TestCaseWithFactory
	{
		public void TestShowAndHideScreenedPartiesList()
		{
			var winModel = new DpsResultWinModel(DpsResultWinModelTest.CreateResultModel(Factory));

			using (var form = new ZForm())
			using (var userControl = new DpsResultUserControlForTest())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();
				AssertNotNull("Show more control display as default", GetShowMoreControl());

				userControl.SwitchBarPictureBox_ClickExposed();
				AssertNull("Show more control removed", GetShowMoreControl());

				var panel = GetScreenedPartyLeftPanel();
				AssertNotNull("Screened Party Left Panel display", panel);

				var screenedPartyListItemUserControls = panel.Controls.Find("ScreenedPartyListItemUserControl", true).OfType<ScreenedPartyListItemUserControl>().ToArray();
				AssertEquals("List items added to panel", 2, screenedPartyListItemUserControls.Length);

				userControl.SwitchBarPictureBox_ClickExposed();
				AssertNotNull("Show more control display", GetShowMoreControl());
				AssertNull("Screened Party Left Panel removed", GetScreenedPartyLeftPanel());

				ShowMoreUserControl GetShowMoreControl()
				{
					return userControl.DpsResultTableLayoutPanel.Controls.OfType<ShowMoreUserControl>().FirstOrDefault();
				}

				ZPanel GetScreenedPartyLeftPanel()
				{
					return userControl.DpsResultTableLayoutPanel.Controls.OfType<ZPanel>().FirstOrDefault(u => u.Name == "ScreenedPartyLeftPanel");
				}
			}
		}

		public void TestScreenedPartiesListChange()
		{
			var winModel = new DpsResultWinModel(DpsResultWinModelTest.CreateResultModel(Factory));

			using (var form = new ZForm())
			using (var userControl = new DpsResultUserControlForTest())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				AssertEquals(winModel.TotalScreenedParties[0], winModel.SelectedScreenedParty);
				AssertEquals("List items added to panel", 2, GetScreenedPartyListItemUserControls().Length);
				AssertEquals("2", userControl.MatchesNumberLabel.Text);
				AssertEquals(winModel.TotalScreenedParties[0], userControl.ScreenedPartyControl.ScreenedPartyWinModel);

				winModel.ScreenedParties[0].ExecuteSaveCommand();
				AssertEquals("Current item changed", winModel.TotalScreenedParties[2], winModel.SelectedScreenedParty);
				AssertEquals("Saved item removed", 1, GetScreenedPartyListItemUserControls().Length);
				AssertEquals("Number updated", "1", userControl.MatchesNumberLabel.Text);
				AssertEquals("Screened party user control data updated", winModel.TotalScreenedParties[2], userControl.ScreenedPartyControl.ScreenedPartyWinModel);

				ScreenedPartyListItemUserControl[] GetScreenedPartyListItemUserControls()
				{
					return userControl.ScreenedPartyLeftPanel.Controls.Find("ScreenedPartyListItemUserControl", true).OfType<ScreenedPartyListItemUserControl>().ToArray();
				}
			}
		}

		public void TestHideScreenedPartyLeftPanelWhenOnlyOneScreenedParty()
		{
			var model = DpsResultWinModelTest.CreateResultModel(Factory);
			model.ScreenedPartyModels.Remove(model.ScreenedPartyModels[2]);
			var winModel = new DpsResultWinModel(model);
			AssertEquals("Pre-condition", 1, winModel.ScreenedPartiesCount);

			using (var form = new ZForm())
			using (var userControl = new DpsResultUserControlForTest())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				AssertNull("Not show ScreenedPartyLeftPanel when only one screened party", userControl.DpsResultTableLayoutPanel.Controls.OfType<ZPanel>().FirstOrDefault(u => u.Name == "ScreenedPartyLeftPanel"));
				AssertEquals("Only one column", 1, userControl.DpsResultTableLayoutPanel.ColumnCount);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DpsResultWinModelTest.SetUpComplianceList(Factory);
		}

		class DpsResultUserControlForTest : DpsResultUserControl
		{
			public void SwitchBarPictureBox_ClickExposed() => SwitchBarPictureBox_Click(null, null);
		}
	}
}
