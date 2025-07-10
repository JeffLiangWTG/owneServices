using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(NumberRangesUserControl))]
	class NumberRangesUserControlTest : TestCaseWithFactory
	{
		public void TestCaptions()
		{
			using (var control = new NumberRangesUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("RangesGroupBox caption must be", "Number Ranges", control.RangesGroupBox.CaptionResourceString.Caption);
					AssertEquals("NumberFountainsDeleteButton caption must be", "Delete", control.NumberFountainsDeleteButton.CaptionResourceString.Caption);
					AssertEquals("NumberFountainsEditButton caption must be", "Edit...", control.NumberFountainsEditButton.CaptionResourceString.Caption);
					AssertEquals("NumberFountainsAddButton caption must be", "Add...", control.NumberFountainsAddButton.CaptionResourceString.Caption);
					AssertEquals("RangesTabPage caption must be", "Ranges", control.RangesTabPage.CaptionResourceString.Caption);
					AssertEquals("MatchingDetailsTabPage caption must be", "Matching Details", control.MatchingDetailsTabPage.CaptionResourceString.Caption);
					AssertEquals("MatchingDetailsGroupBox caption must be", "Matching Details", control.MatchingDetailsGroupBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestComponents()
		{
			using (var control = new NumberRangesUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<ZGroupBox>("RangesGroupBox must be ZGroupBox", control.RangesGroupBox);
					AssertType<ZGrid>("RangesGrid must be ZGrid", control.RangesGrid);
					AssertType<ZPanel>("NumberFountainsButtonsPanel must be ZPanel", control.NumberFountainsButtonsPanel);
					AssertType<ZButton>("NumberFountainsAddButton must be ZButton", control.NumberFountainsAddButton);
					AssertType<ZButton>("NumberFountainsDeleteButton must be ZButton", control.NumberFountainsDeleteButton);
					AssertType<ZButton>("NumberFountainsEditButton must be ZButton", control.NumberFountainsEditButton);
					AssertType<ZTabControl>("NumberRangesTabControl must be ZTabControl", control.NumberRangesTabControl);
					AssertType<ZTabPage>("RangesTabPage must be ZTabPage", control.RangesTabPage);
					AssertType<ZTabPage>("MatchingDetailsTabPage must be ZTabPage", control.MatchingDetailsTabPage);
					AssertType<ZGroupBox>("MatchingDetailsGroupBox must be ZGroupBox", control.MatchingDetailsGroupBox);
					AssertType<ZGrid>("MatchingDetailsGrid must be ZGrid", control.MatchingDetailsGrid);
				});
			}
		}

		public void TestDefaultColumnsForGlbStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.Fountains.AddNew();

			using (var form = new ZForm(staff))
			using (var userControl = new NumberRangesUserControl())
			{
				userControl.SetDataBinding(staff, "");
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				var matchingDetailsGrid = userControl.MatchingDetailsGrid;
				CombineAssertions(() =>
				{
					Assert("Should have NRM_RangeType column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.NRM_RangeType).IsVisible);
					Assert("Should have NRM_Prefix column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.NRM_Prefix).IsVisible);
					Assert("Should have PatentNumber column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.PatentNumber).IsVisible);
					Assert("Should have CustomsArea column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.CustomsArea).IsVisible);
					Assert("Should NOT have NRM_OH_Client column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.NRM_OH_Client).IsUnavailable);
					Assert("Should NOT have NRM_WW_Whs column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.NRM_WW_Whs).IsUnavailable);
					Assert("Should have CurrentNumber column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle("CurrentNumber").IsVisible);
					Assert("Should have MinimumValue column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle("MinimumValue").IsVisible);
					Assert("Should have MaximumValue column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle("MaximumValue").IsVisible);
				});
			}
		}

		public void TestDefaultColumnsForOrgHeader()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.Fountains.AddNew();

			using (var form = new ZForm(orgHeader))
			using (var userControl = new NumberRangesUserControl())
			{
				userControl.SetDataBinding(orgHeader, "");
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				var matchingDetailsGrid = userControl.MatchingDetailsGrid;
				CombineAssertions(() =>
				{
					Assert("Should have NRM_RangeType column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.NRM_RangeType).IsVisible);
					Assert("Should have NRM_Prefix column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.NRM_Prefix).IsVisible);
					Assert("Should NOT have PatentNumber column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.PatentNumber).IsUnavailable);
					Assert("Should NOT have CustomsArea column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.CustomsArea).IsUnavailable);
					Assert("Should have NRM_OH_Client column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.NRM_OH_Client).IsVisible);
					Assert("Should have NRM_WW_Whs column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle(StmNumberRangeMatchingDetail.Schema.NRM_WW_Whs).IsVisible);
					Assert("Should have CurrentNumber column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle("CurrentNumber").IsVisible);
					Assert("Should have MinimumValue column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle("MinimumValue").IsVisible);
					Assert("Should have MaximumValue column in MatchingDetailsGrid", matchingDetailsGrid.GetColumnStyle("MaximumValue").IsVisible);
				});
			}
		}

		public void TestEvents()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.Fountains.AddNew();
			AssertEvents<ViewStmNumsEditorForm>(staff);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.Fountains.AddNew();
			AssertEvents<OrganisationViewStmNumsEditorForm>(orgHeader);

			void AssertEvents<T>(IViewStmNumsOwner owner)
			{
				using (var form = new ZForm(owner))
				using (var userControl = new NumberRangesUserControl())
				{
					userControl.SetDataBinding(owner, "");
					userControl.Dock = DockStyle.Fill;
					form.Controls.Add(userControl);
					form.Show();
					userControl.NumberFountainsAddButton.PerformClick();
					AssertEquals("Please save changes first.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.NumberFountainsDeleteButton.PerformClick();
					AssertEquals("Please save changes first.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.NumberFountainsEditButton.PerformClick();
					AssertEquals("Please select a Number Fountain from the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessages();
					userControl.NumberFountainsDeleteButton.PerformClick();
					AssertEquals("Please select a Number Fountain from the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					userControl.NumberFountainsAddButton.PerformClick();
					AssertType<T>(ZFormModaliser.LastFormShownDialogForTest);
					ZFormModaliser.LastFormShownDialogForTest.Close();

					userControl.RangesGrid.Select(0);
					userControl.NumberFountainsEditButton.PerformClick();
					AssertType<T>(ZFormModaliser.LastFormShownDialogForTest);
					ZFormModaliser.LastFormShownDialogForTest.Close();

					userControl.NumberFountainsDeleteButton.PerformClick();
					AssertEquals("This would delete number fountain customization. Continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestNumberFountainsButtonsPanelConfigSecurity_GlbStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Env.Security.StaffModifyAll.IsAllowed = true;
			Factory.Save();
			NumberFountainsButtonsPanelConfigSecurityCore(staff, expectedEnabledValue: true);

			Env.Security.StaffModifyAll.IsAllowed = false;
			Factory.Save();
			NumberFountainsButtonsPanelConfigSecurityCore(staff, expectedEnabledValue: false);
		}

		public void TestNumberFountainsButtonsPanelConfigSecurity_OrgHeader()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "Org1";

			Env.Security.OrgConfigModify.IsAllowed = true;
			Factory.Save();
			NumberFountainsButtonsPanelConfigSecurityCore(header, expectedEnabledValue: true);

			Env.Security.OrgConfigModify.IsAllowed = false;
			Factory.Save();
			NumberFountainsButtonsPanelConfigSecurityCore(header, expectedEnabledValue: false);
		}

		void NumberFountainsButtonsPanelConfigSecurityCore(IViewStmNumsOwner owner, bool expectedEnabledValue)
		{
			using (var form = new ZForm(owner))
			using (var userControl = new NumberRangesUserControl())
			{
				userControl.SetDataBinding(owner, "");
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.Show();
				Application.DoEvents();

				var rangesTabPage = (ZTabPage)userControl.Controls.Find("RangesTabPage", true)[0];
				userControl.NumberRangesTabControl.SelectedTab = rangesTabPage;
				var numberFountainsButtonsPanel = (ZPanel)userControl.Controls.Find("NumberFountainsButtonsPanel", true)[0];

				AssertNumberFountainButtons(numberFountainsButtonsPanel, "NumberFountainsAddButton", expectedEnabledValue);
				AssertNumberFountainButtons(numberFountainsButtonsPanel, "NumberFountainsEditButton", expectedEnabledValue);
				AssertNumberFountainButtons(numberFountainsButtonsPanel, "NumberFountainsDeleteButton", expectedEnabledValue);
			}

			static void AssertNumberFountainButtons(ZPanel numberFountainsButtonsPanel, string buttonName, bool expectedEnabledValue)
			{
				var numberFountainsButton = (ZButton)numberFountainsButtonsPanel.Controls.Find(buttonName, true)[0];
				AssertEquals($"Should {(expectedEnabledValue ? "" : "NOT ")}be enabled", expectedEnabledValue, numberFountainsButton.Enabled);
			}
		}
	}
}
