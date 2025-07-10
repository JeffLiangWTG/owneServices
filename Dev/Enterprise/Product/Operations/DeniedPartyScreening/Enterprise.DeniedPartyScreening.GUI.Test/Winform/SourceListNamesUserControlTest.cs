using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class SourceListNamesUserControlTest : TestCaseWithFactory
	{
		public void TestSetDataBinding_NoExceptionThrown()
		{
			using (var form = new ZForm())
			using (var userControl = new SourceListNamesUserControl())
			{
				form.Controls.Add(userControl);
				AssertNoExceptionThrown(() => userControl.SetDataBinding(null, ""));
			}
		}

		public void TestSourceListNamesUserControl_WithValidSourceListNames()
		{
			using (var form = new ZForm())
			using (var userControl = new SourceListNamesUserControl())
			{
				form.Controls.Add(userControl);

				var winModel = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA" }));
				userControl.SetDataBinding(winModel, "");

				form.Show();

				AssertNotNull(userControl.FindSingleOrDefault<ZPanel>("SourceListContentPanel"));
				AssertNotNull(userControl.FindSingleOrDefault<ZPanel>("SourceListHeaderPanel"));
				AssertNotNull(userControl.FindSingleOrDefault<KTableLayoutPanel>("SourceListItemsPanel"));
				AssertNotNull(userControl.FindSingleOrDefault<ZLinkLabel>("SourceListNamesExpander"));
				AssertEquals(WinformConstants.ArrowDown, userControl.FindSingleOrDefault<ZLinkLabel>("SourceListNamesExpander").Text);
				AssertNotNull(userControl.FindSingleOrDefault<ZLabel>("SourceListNamesLabel"));
				AssertNotNull(userControl.FindSingleOrDefault<KTableLayoutPanel>("SourceListTableLayoutPanel"));
			}
		}

		public void TestSourceListContentPanelExpandCollapse()
		{
			using (var form = new ZForm())
			using (var userControl = new SourceListNamesUserControl())
			{
				form.Controls.Add(userControl);

				var winModel = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA", "BBB", "CCC" }));
				userControl.SetDataBinding(winModel, "");

				form.Show();

				AssertNotNull("HeaderPanel Should display", userControl.FindSingleOrDefault<ZPanel>("SourceListHeaderPanel"));

				var expander = userControl.FindSingle<ZLinkLabel>("SourceListNamesExpander");
				var tableLayoutPanel = userControl.FindSingleOrDefault<KTableLayoutPanel>("SourceListTableLayoutPanel");

				AssertNotNull("TabLayout Should display", tableLayoutPanel);
				AssertNotNull("Expander Should display", expander);

				expander.PerformClick_ForTest();

				AssertEquals("ArrowUp Should display", WinformConstants.ArrowUp, expander.Text);
				AssertEquals("ContentPanel Expanded", 100F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("ContentPanel Expanded", SizeType.Percent, tableLayoutPanel.RowStyles[1].SizeType);

				expander.PerformClick_ForTest();

				AssertEquals("ArrowDown Should display", WinformConstants.ArrowDown, expander.Text);
				AssertEquals("ContentPanel Collapsed", 0F, tableLayoutPanel.RowStyles[1].Height);
				AssertEquals("ContentPanel Collapsed", SizeType.Absolute, tableLayoutPanel.RowStyles[1].SizeType);
			}
		}

		public void TestHideAndDisplayShowMoreUserControl()
		{
			using (var form = new ZForm())
			using (var userControl = new SourceListNamesUserControl())
			{
				form.Controls.Add(userControl);

				var winModel = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA", "BBB", "CCC" }));
				winModel.IsExpanderExpanded = true;
				userControl.SetDataBinding(winModel, "");

				form.Show();

				var genericMatchShowMoreUserControl = userControl.FindSingle<GenericMatchShowMoreUserControl>();
				AssertEquals(false, genericMatchShowMoreUserControl.Visible);

				CommonTestDataHelper.CreateComplianceList(Factory, "AAA", isExcluded: true);
				Factory.Save();

				winModel = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA", "BBB", "CCC" }));
				userControl.SetDataBinding(winModel, "");
				AssertEquals(true, genericMatchShowMoreUserControl.Visible);
			}
		}

		public void TestGenericMatchShowMoreUserControl()
		{
			CommonTestDataHelper.CreateComplianceList(Factory, "AAA", isExcluded: true);
			Factory.Save();
			var winModel = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA", "BBB", "CCC" }));
			winModel.IsExpanderExpanded = true;

			using (var form = new ZForm())
			using (var userControl = new SourceListNamesUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();

				var panel = userControl.FindSingle<ZPanel>("OtherInfoBorderPanel");
				AssertEquals(false, panel.Visible);
				AssertEquals(false, winModel.IsInnerExpanderExpanded);

				var showMoreLessLabel = userControl.FindSingle<ZLinkLabel>("ShowMoreLessLabel");
				AssertEquals(WinformConstants.ArrowDown + winModel.InnerExpanderTitle, showMoreLessLabel.Text);

				showMoreLessLabel.OnLinkClicked_Exposed(null);
				AssertEquals(true, panel.Visible);
				AssertEquals(true, winModel.IsInnerExpanderExpanded);
				AssertEquals(WinformConstants.ArrowUp + winModel.InnerExpanderTitle, showMoreLessLabel.Text);

				showMoreLessLabel.OnLinkClicked_Exposed(null);
				AssertEquals(false, panel.Visible);
				AssertEquals(false, winModel.IsInnerExpanderExpanded);
				AssertEquals(WinformConstants.ArrowDown + winModel.InnerExpanderTitle, showMoreLessLabel.Text);
			}
		}
	}
}
