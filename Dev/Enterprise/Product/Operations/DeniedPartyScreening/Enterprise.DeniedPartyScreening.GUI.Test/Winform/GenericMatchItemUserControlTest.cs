using System.Drawing;
using System.Linq;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class GenericMatchItemUserControlTest : TestCase
	{
		public void TestSetDataBinding_ScreenedAndDeniedPartyInnerPanel()
		{
			using (var form = new ZForm())
			using (var userControl = new GenericMatchItemUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(GetWinModel(ScoreGrades.High), "");
				form.Show();

				var screenedPartyLabels = GetLabels(true);
				var deniedPartyLabels = GetLabels(false);

				AssertEquals("Dummy Screened Party", screenedPartyLabels.Single().Text);
				AssertEquals("Dummy Denied Party", deniedPartyLabels.Single().Text);

				var newLine = System.Environment.NewLine;
				userControl.SetDataBinding(GetWinModel(ScoreGrades.High, $"Address 1{newLine}City{newLine}Postcode{newLine}Country", $"Address 2{newLine}Address 3{newLine}State{newLine}{newLine}"), "");

				screenedPartyLabels = GetLabels(true);
				deniedPartyLabels = GetLabels(false);
				AssertContainsExactElementsInExactOrder(new[] { "Address 1", "City", "Postcode", "Country" }, screenedPartyLabels.Select(u => u.Text));
				AssertContainsExactElementsInExactOrder(new[] { "Address 2", "Address 3", "State" }, deniedPartyLabels.Select(u => u.Text));

				userControl.SetDataBinding(new ScreenedDeniedItemWinModel($"Address 2{newLine}Address 3{newLine}State{newLine}{newLine}"), "");
				screenedPartyLabels = GetLabels(true);
				deniedPartyLabels = GetLabels(false);
				AssertEquals(0, screenedPartyLabels.Length);
				AssertContainsExactElementsInExactOrder(new[] { "Address 2", "Address 3", "State" }, deniedPartyLabels.Select(u => u.Text));

				ZLabel[] GetLabels(bool screenedLabels)
				{
					return userControl.FindSingle<ZPanel>(screenedLabels ? "ScreenedPartyInnerPanel" : "DeniedPartyInnerPanel").FindAll<ZLabel>(u => u.Name == "TextLabel").Reverse().ToArray();
				}
			}
		}

		public void TestSetDataBinding_ScoreLabel()
		{
			using (var form = new ZForm())
			using (var userControl = new GenericMatchItemUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(GetWinModel(ScoreGrades.High), "");
				form.Show();

				var scoreLabel = userControl.FindSingle<ZLabel>("ScoreLabel");
				AssertEquals("90%", scoreLabel.Text);
			}
		}

		public void TestSetDataBinding_BackgroundColor()
		{
			using (var form = new ZForm())
			using (var userControl = new GenericMatchItemUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(GetWinModel(ScoreGrades.High), "");
				form.Show();

				var scoreGradePanel = userControl.FindSingle<ZPanel>("ScoreGradePanel");
				var deniedPartyPanel = userControl.FindSingle<ZPanel>("DeniedPartyPanel");
				AssertEquals(Color.FromArgb(255, 214, 104, 104), scoreGradePanel.BackColor);
				AssertEquals(Color.FromArgb(255, 252, 244, 244), deniedPartyPanel.BackColor);

				userControl.SetDataBinding(GetWinModel(ScoreGrades.Medium), "");
				AssertEquals(Color.FromArgb(255, 235, 171, 76), scoreGradePanel.BackColor);
				AssertEquals(Color.FromArgb(255, 254, 249, 242), deniedPartyPanel.BackColor);

				userControl.SetDataBinding(GetWinModel(ScoreGrades.Low), "");
				AssertEquals(Color.Transparent, scoreGradePanel.BackColor);
				AssertEquals(Color.Transparent, deniedPartyPanel.BackColor);

				userControl.SetDataBinding(new ScreenedDeniedItemWinModel("Dummy"), "");
				AssertEquals(Color.Transparent, scoreGradePanel.BackColor);
				AssertEquals(Color.Transparent, deniedPartyPanel.BackColor);
			}
		}

		public void TestSetDataBinding_BottomBorderPanelVisible()
		{
			using (var form = new ZForm())
			using (var userControl = new GenericMatchItemUserControl())
			{
				var winModel = GetWinModel(ScoreGrades.High);
				winModel.BottomLineVisibility = true;

				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var bottomBorderPanel = userControl.FindSingle<ZPanel>("BottomBorderPanel");
				AssertEquals(true, bottomBorderPanel.Visible);

				winModel.BottomLineVisibility = false;
				userControl.SetDataBinding(winModel, "");
				AssertEquals(false, bottomBorderPanel.Visible);
			}
		}

		ScreenedDeniedItemWinModel GetWinModel(ScoreGrades scoreGrade, string screenedParty = "Dummy Screened Party", string deniedParty = "Dummy Denied Party")
		{
			switch (scoreGrade)
			{
				case ScoreGrades.High:
					return new ScreenedDeniedItemWinModel(screenedParty, deniedParty, "90%", ScoreGrades.High, 90);
				case ScoreGrades.Medium:
					return new ScreenedDeniedItemWinModel(screenedParty, deniedParty, "60%", ScoreGrades.Medium, 60);
				default:
					return new ScreenedDeniedItemWinModel(screenedParty, deniedParty, "40%", ScoreGrades.Low, 40);
			}
		}
	}
}
