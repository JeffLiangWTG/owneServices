using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(RefCountryForm))]
	public class RefCountryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var country = Factory.New<RefCountry>();
			var form = new RefCountryForm(country)
			{
				Width = 840,
				Height = 760
			};
			return form;
		}

		public void TestPopupImpactFormWhenIsActiveHasChanges()
		{
			Factory.Save();
			using (var form = new RefCountryForm(country))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.BusinessEntity.RN_Desc = "Test Desc";
				form.BusinessEntity.RN_RX_NKLocalCurrency = "USD";
				form.BusinessEntity.RN_IsActive = !form.BusinessEntity.RN_IsActive;
				form.FireSaveButton();

				AssertNull(ZFormModaliser.LastFormShownForTest);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertContains("If a country/region is incorrectly modified there will be a large negative impact on the system and its behaviors.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUnableToRemoveStates()
		{
			using (var form = new RefCountryForm(country))
			{
				var statesGrid = (ZGrid)form.Controls.Find("ZGridStates", true).Single();
				AssertEquals(RemoveAction.NoRemovePossible, statesGrid.RemoveAction);
			}
		}

		public void TestResize()
		{
			using (var form = new RefCountryForm(country))
			{
				AssertEquals(System.Windows.Forms.AutoSizeMode.GrowAndShrink, form.AutoSizeMode);
				AssertEquals(System.Windows.Forms.FormBorderStyle.Sizable, form.FormBorderStyle);
			}
		}

		[RequiresSTA]
		public void TestRefCountryStatesGridEditableFields()
		{
			using (var form = new RefCountryForm(country))
			{
				var statesGrid = (ZGrid)form.Controls.Find("ZGridStates", true).Single();
				AssertEquals(true, statesGrid.GetColumnStyle(RefCountryStates.Schema.RW_Code).IsReadOnly);
				AssertEquals(true, statesGrid.GetColumnStyle(RefCountryStates.Schema.RW_DescriptionMultilingual).IsReadOnly);
				AssertEquals(false, statesGrid.GetColumnStyle(RefCountryStates.Schema.RW_IsActive).IsReadOnly);
			}
		}

		public void TestIsMarkSanctionedCheckbox_ShouldBeReadonly()
		{
			using (var form = new RefCountryForm(country))
			{
				var markCheckbox = (ZCheckBox)form.Controls.Find("isMarkSanctionedCheckBox", true).Single();
				AssertEquals(true, markCheckbox.ReadOnly);
			}
		}

		public void TestAddDeniedPartyScreeningLogsTabPage()
		{
			using (var form = new RefCountryForm(country))
			{
				var mainTabControl = (ZTabControl)((ZLogsTabPage)form.GetField("zLogsTabPage1")).Controls[0].Controls[0];

				AssertEquals(2, mainTabControl.TabPages.Count);
				AssertEquals("Denied Party Screening Logs", mainTabControl.TabPages[1].Text);
				AssertEquals(typeof(StmEntityScreeningLogControl), mainTabControl.TabPages[1].Controls[0].GetType());
			}
		}

		public void TestSwitchToDpsLogsTab()
		{
			using (var form = new RefCountryForm(country))
			{
				form.Show();
				var templateTabControl = form.Controls[0] as ZTemplateTabControl;
				AssertNotEquals("Logs", templateTabControl.SelectedTab.Text);

				form.SwitchToDpsLogsTab();
				AssertEquals("Logs", templateTabControl.SelectedTab.Text);

				var mainTabControl = (ZTabControl)templateTabControl.SelectedTab.Controls[0].Controls[0];
				AssertEquals("Denied Party Screening Logs", mainTabControl.SelectedTab.Text);
			}
		}

		public void TestAddComplianceRuleTab()
		{
			AssertAddComplianceRuleTab(true, true, true);
			AssertAddComplianceRuleTab(true, false, false);
			AssertAddComplianceRuleTab(false, true, false);

			void AssertAddComplianceRuleTab(bool enableComplianceWise, bool enableCommodityScreening, bool excepted)
			{
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(enableComplianceWise)))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceWise))
				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(enableCommodityScreening))
				using (var form = new RefCountryForm(country))
				{
					var complianceRulesTabPage = (ZTabPage)form.GetField("zComplianceRulesTabPage");
					AssertNotNull(complianceRulesTabPage);
					AssertEquals(excepted, complianceRulesTabPage.TabVisible);

					var complianceRuleUserControl = complianceRulesTabPage.FindSingleOrDefault<ComplianceRisk.Integration.IComplianceRuleUserControl>();
					if (excepted)
					{
						AssertNotNull(complianceRuleUserControl);
					}
					else
					{
						AssertNull(complianceRuleUserControl);
					}
				}
			}
		}

		public void TestTabNonWorkingDaysExists()
		{
			using (var form = new RefCountryForm(country))
			{
				var nonWorkingDayTab = (ZTabPage)form.Controls.Find("zNonWorkingDaysTabPage", true).SingleOrDefault();
				AssertNotNull(nonWorkingDayTab);
			}
		}

		public void TestShowInnersColumnsVisibility()
		{
			AssertShowInnersColumnsVisibility(true);
			AssertShowInnersColumnsVisibility(false);
		}

		void AssertShowInnersColumnsVisibility(bool isVisible)
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isVisible))
			using (var form = new RefCountryForm(country))
			{
				var rulesGrid = (ZGrid)form.Controls.Find("RulesGrid", true).Single();
				var showInners = rulesGrid.GetColumnStyle(RefCountryRules.Schema.R7_ShowInner);

				if (isVisible)
				{
					AssertNotNull(showInners);
					AssertEquals(isVisible, showInners.IsVisible);
				}
				else
				{
					AssertNull(showInners);
				}
			}
		}

		public void TestTabNonWorkingDaysFields()
		{
			using (var form = new RefCountryForm(country))
			{
				var nonWorkingDayTab = (ZTabPage)form.Controls.Find("zNonWorkingDaysTabPage", true).SingleOrDefault();
				AssertEquals(2, nonWorkingDayTab.Controls.Count);
				AssertNotNull(nonWorkingDayTab.Controls.Find("WeekendsGroupBox", false));
				AssertNotNull(nonWorkingDayTab.Controls.Find("HolidaysGridGroupBox", false));
			}
		}

		public void TestDoubleClickHolidayOnCountryOrRegionModuleToHolidayModule()
		{
			var countryStates = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates.RW_RN_NKCountryCode = country.RN_Code;
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentID = country.PK;
			holiday.GH_ParentTableCode = country.TablePrefix;
			Factory.Save();
			using (var form = new RefCountryForm(country))
			{
				form.Show();
				var nonWorkingDayTab = (ZTabPage)form.Controls.Find("zNonWorkingDaysTabPage", true).SingleOrDefault();
				AssertEquals(2, nonWorkingDayTab.Controls.Count);
				AssertNotNull(nonWorkingDayTab.Controls.Find("WeekendsGroupBox", false));
				AssertNotNull(nonWorkingDayTab.Controls.Find("HolidaysGridGroupBox", false));
				var holidaysGridGroupBox = (ZGroupBox)nonWorkingDayTab.Controls.Find("HolidaysGridGroupBox", true).SingleOrDefault();
				holidaysGridGroupBox.Show();
				var holidaysGrid = (ZGrid)holidaysGridGroupBox.Controls.Find("ZGridNonWorkingDayHolidays", true).SingleOrDefault();
				holidaysGrid.Show();
				holidaysGrid.PerformDoubleClickForTest();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestIsNotEBLSupportedColumnExists()
		{
			using (var form = new RefCountryForm(country))
			{
				var rulesGrid = (ZGrid)form.Controls.Find("RulesGrid", true).Single();
				var isEBLNotSupported = rulesGrid.GetColumnStyle("R7_IsEBLNotSupported");
				AssertNotNull(isEBLNotSupported);
				AssertEquals(true, isEBLNotSupported.IsVisible);
				AssertEquals("eBL Not Supported", isEBLNotSupported.CaptionResourceString.Caption);

				var rule = Factory.New<RefCountryRules>();
				AssertEquals("The default value should be false", false, rule.R7_IsEBLNotSupported);
			}
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (RefCountryForm)GetFormToBashCore())
			{
				AssertNotNull("RefCountryForm should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		protected override bool AllowFormSizeFixed => true;

		protected override void SetUp()
		{
			base.SetUp();
			country = Factory.NewWithValidTestData<RefCountry>();
		}
		RefCountry country;
	}
}
