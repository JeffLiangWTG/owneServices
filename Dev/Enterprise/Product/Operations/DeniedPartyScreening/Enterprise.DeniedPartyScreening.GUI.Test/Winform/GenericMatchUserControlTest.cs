using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class GenericMatchUserControlTest : TestCase
	{
		public void TestTitleDescriptionAndColorWithMatches()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("party1", "High1", string.Empty, ScoreGrades.High, 100),
				new ScreenedDeniedItemWinModel("party2", "Medium2", string.Empty, ScoreGrades.Medium, 80),
			};

			var winModel = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", matches);

			using (var form = new ZForm())
			using (var userControl = new GenericMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var description = userControl.FindSingle<ZLabel>("ExpanderDescriptionLabel");
				var title = userControl.FindSingle<ZLabel>("ExpanderTitleLabel");

				AssertNotNull("Description Should display", description);
				AssertNotNull("Title Should display", title);

				AssertEquals("Description Should display", Color.FromArgb(255, 55, 63, 80), description.ForeColor);

				AssertEquals("Test Title", title.Text);
				AssertEquals("2 Addresses Matched", description.Text);
			}
		}

		public void TestTitleDescriptionAndColorWithNoMatches()
		{
			var winModel = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", new ObservableCollection<ScreenedDeniedItemWinModel>());

			using (var form = new ZForm())
			using (var userControl = new GenericMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var description = userControl.FindSingle<ZLabel>("ExpanderDescriptionLabel");
				var title = userControl.FindSingle<ZLabel>("ExpanderTitleLabel");

				AssertNotNull("Description Should display", description);
				AssertNotNull("Title Should display", title);

				AssertEquals("Description Should display", Color.FromArgb(255, 109, 109, 109), description.ForeColor);

				AssertEquals("Test Title", title.Text);
				AssertEquals("No Addresses Matched", description.Text);
			}
		}

		public void TestExpandCollapseWithMatches()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("party1", "High1", string.Empty, ScoreGrades.High, 100),
				new ScreenedDeniedItemWinModel("party2", "Medium2", string.Empty, ScoreGrades.Medium, 80),
			};

			var winModel = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", matches);

			using (var form = new ZForm())
			using (var userControl = new GenericMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var expander = userControl.FindSingle<ZLinkLabel>("ProfileContentExpander");
				var tableLayoutPanel = userControl.FindSingleOrDefault<KTableLayoutPanel>("GenericMatchTableLayoutPanel");

				AssertNotNull("TabLayout Should display", tableLayoutPanel);
				AssertNotNull("HeaderPanel Should display", expander);

				AssertNotNull("StackPanel display", userControl.FindSingleOrDefault<ZPanel>("GenericMatchStackPanel"));
				AssertEquals("ContentPanel display", 100F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("ContentPanel display", SizeType.Percent, tableLayoutPanel.RowStyles[1].SizeType);

				expander.PerformClick_ForTest();

				AssertEquals("ContentPanel Collapsed", 0F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("ContentPanel Collapsed", SizeType.Absolute, tableLayoutPanel.RowStyles[1].SizeType);
			}
		}

		public void TestExpandCollapseWithNoMatches()
		{
			var winModel = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", new ObservableCollection<ScreenedDeniedItemWinModel>());

			using (var form = new ZForm())
			using (var userControl = new GenericMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var expander = userControl.FindSingle<ZLinkLabel>("ProfileContentExpander");
				var tableLayoutPanel = userControl.FindSingleOrDefault<KTableLayoutPanel>("GenericMatchTableLayoutPanel");

				AssertNotNull("TabLayout Should display", tableLayoutPanel);
				AssertNotNull("Expander Should display", expander);

				AssertNotNull("StackPanel display", userControl.FindSingleOrDefault<ZPanel>("GenericMatchStackPanel"));
				AssertEquals("No ContentPanel display", 0F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("No ContentPanel display", SizeType.Absolute, tableLayoutPanel.RowStyles[1].SizeType);

				expander.PerformClick_ForTest();

				AssertEquals("ContentPanel not expand", 0F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("ContentPanel not expand", SizeType.Absolute, tableLayoutPanel.RowStyles[1].SizeType);
			}
		}

		public void TestHideAndDisplayShowMoreUserControl()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("party1", "High1", string.Empty, ScoreGrades.High, 100),
				new ScreenedDeniedItemWinModel("party2", "Medium2", string.Empty, ScoreGrades.Medium, 80),
				new ScreenedDeniedItemWinModel("Low3"),
			};

			var winModel = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", matches);

			using (var form = new ZForm())
			using (var userControl = new GenericMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var genericMatchShowMoreUserControl = userControl.FindSingle<GenericMatchShowMoreUserControl>();
				AssertEquals(true, genericMatchShowMoreUserControl.Visible);
				AssertNotNull(winModel.NotifySizeChangedAction);

				matches.RemoveAt(2);
				winModel = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", matches);
				userControl.SetDataBinding(winModel, "");
				AssertEquals(false, genericMatchShowMoreUserControl.Visible);
			}
		}

		public void TestShowScreenedItems()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("party1", "High1", string.Empty, ScoreGrades.High, 100),
				new ScreenedDeniedItemWinModel("party2", "Medium2", string.Empty, ScoreGrades.Medium, 80),
				new ScreenedDeniedItemWinModel("Low3"),
			};

			var winModel = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", matches);

			using (var form = new ZForm())
			using (var userControl = new GenericMatchUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var genericMatchItemUserControls = userControl.FindAll<GenericMatchItemUserControl>().ToArray();
				AssertEquals(3, genericMatchItemUserControls.Length);

				var genericMatchHeaderUserControls = userControl.FindAll<GenericMatchHeaderUserControl>().ToArray();
				AssertEquals(2, genericMatchHeaderUserControls.Length);
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						(winModel.ScreenedPartyHeader, winModel.DeniedPartyHeader),
						(winModel.OtherScreenedPartyHeader, winModel.OtherDeniedPartyHeader)
					},
					genericMatchHeaderUserControls.Select(u => (u.ScreenedPartyHeaderLabel.Text, u.DeniedPartyHeaderLabel.Text)));
			}
		}

		public void TestGenericMatchShowMoreUserControl()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("party", "medium", string.Empty, ScoreGrades.Medium, 60)
			};

			var notifySizeChangedCalled = 0;
			var winModel = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", matches);
			winModel.NotifySizeChangedAction = () => { notifySizeChangedCalled++; };

			using (var form = new ZForm())
			using (var userControl = new GenericMatchShowMoreUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				AssertEquals(1, notifySizeChangedCalled);
				form.Show();

				var panel = userControl.FindSingle<ZPanel>("OtherInfoBorderPanel");
				AssertEquals(false, panel.Visible);

				var genericMatchHeaderUserControls = userControl.FindSingle<GenericMatchHeaderUserControl>();
				AssertEquals(winModel.OtherScreenedPartyHeader, genericMatchHeaderUserControls.ScreenedPartyHeaderLabel.Text);
				AssertEquals(winModel.OtherDeniedPartyHeader, genericMatchHeaderUserControls.DeniedPartyHeaderLabel.Text);

				AssertEquals(false, winModel.IsInnerExpanderExpanded);

				var showMoreLessLabel = userControl.FindSingle<ZLinkLabel>("ShowMoreLessLabel");
				AssertEquals(WinformConstants.ArrowDown + winModel.InnerExpanderTitle, showMoreLessLabel.Text);

				showMoreLessLabel.OnLinkClicked_Exposed(null);
				AssertEquals(true, panel.Visible);
				AssertEquals(WinformConstants.ArrowUp + winModel.InnerExpanderTitle, showMoreLessLabel.Text);
				AssertEquals(2, notifySizeChangedCalled);
				AssertEquals(true, winModel.IsInnerExpanderExpanded);

				showMoreLessLabel.OnLinkClicked_Exposed(null);
				AssertEquals(false, panel.Visible);
				AssertEquals(WinformConstants.ArrowDown + winModel.InnerExpanderTitle, showMoreLessLabel.Text);
				AssertEquals(3, notifySizeChangedCalled);
				AssertEquals(false, winModel.IsInnerExpanderExpanded);
			}
		}
	}
}
