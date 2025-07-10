using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI.Test
{
	public class ComplianceRuleBulkUpdateMenuItemTest : TestCaseWithFactory
	{
		public void TestCopyToOtherCountriesMenuExist()
		{
			using var control = new ComplianceRuleUserControl();
			var bulkUpdateMenu = control.ComplianceRuleGrid.ContextMenu.MenuItems.FindByText("Copy To Other Countries/Regions", true);
			AssertNotNull("Bulk update compliance rule menu exist", bulkUpdateMenu);
		}

		public void TestCopyToOtherCountriesMenuEnableOrDisable()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var rules = (country.ComplianceRules as ComplianceRuleCollection);
			rules.AddNew();
			rules.AddNew();

			using var form = new ZForm(country);
			using var control = new ComplianceRuleUserControl();
			var bulkUpdateMenu = control.ComplianceRuleGrid.ContextMenu.MenuItems.FindByText("Copy To Other Countries/Regions", true);
			form.Controls.Add(control);
			form.Show();

			Assert("Bulk update compliance rule menu disabled", !bulkUpdateMenu.Enabled);
			control.ComplianceRuleGrid.PerformMouseDownForTest(0, 1);

			Assert("Bulk update compliance rule menu Enabled", bulkUpdateMenu.Enabled);
		}

		public void TestCopyToOtherCountries()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var rules = (country.ComplianceRules as ComplianceRuleCollection);
			var rule = rules.AddNew();
			rule.CRU_Origin = "AU";
			rule.CRU_Destination = "CN";
			rule.CRU_HarmonizedCode = "1234";
			rule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			Factory.Save();

			using var form = new ZForm(country);
			using var control = new ComplianceRuleUserControl();
			var bulkUpdateMenu = control.ComplianceRuleGrid.ContextMenu.MenuItems.FindByText("Copy To Other Countries/Regions", true);
			form.Controls.Add(control);
			form.Show();
			control.ComplianceRuleGrid.PerformMouseDownForTest(0, 1);
			bulkUpdateMenu.PerformClick();
			using var popup = Application.OpenForms.OfType<EmbeddedModulePopup>().Single();
			var filter = popup.FindAll<ZFilterStripControl>().Single();
			filter.FirePerformSearch();

			filter.FilteredGrid.Select(1);
			var origin = filter.FilteredGrid.SelectedElements.Cast<RefCountry>().First().RN_Code;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var button = (ZButton)popup.Controls.Find("OK_Button", true)[0];
			button.PerformClick();

			var copiedCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, origin);
			var copiedRules = (copiedCountry.ComplianceRules as ComplianceRuleCollection);
			Assert("Copy rules success.", copiedRules.Any(x => ((ComplianceRule)x).CRU_Destination == "CN" && ((ComplianceRule)x).CRU_HarmonizedCode == "1234"));
			AssertEquals("Selected Compliance Rules have been copied successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCopyToOtherCountries_ShowPopupMessageWhenFormNotSaved()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var rules = (country.ComplianceRules as ComplianceRuleCollection);
			var rule = rules.AddNew();
			rule.CRU_Origin = "AU";
			rule.CRU_Destination = "CN";
			rule.CRU_HarmonizedCode = "1234";

			using var form = new ZForm(country);
			using var control = new ComplianceRuleUserControl();
			var bulkUpdateMenu = control.ComplianceRuleGrid.ContextMenu.MenuItems.FindByText("Copy To Other Countries/Regions", true);
			form.Controls.Add(control);
			form.Show();
			control.ComplianceRuleGrid.PerformMouseDownForTest(0, 1);
			bulkUpdateMenu.PerformClick();

			AssertEquals("Please save the form before copying the compliance rules.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCopyComplianceRules_FilterOutDuplicateAndExistRules()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var rules = country.ComplianceRules as ComplianceRuleCollection;
			var rule1 = rules.AddNew();
			rule1.CRU_Origin = "CN";
			rule1.CRU_Destination = "AU";
			rule1.CRU_HarmonizedCode = "1234";
			rule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			var rule2 = rules.AddNew();
			rule2.CRU_Origin = "DE";
			rule2.CRU_Destination = "AU";
			rule2.CRU_HarmonizedCode = "1234";
			rule2.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			Factory.Save();

			var cnCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var usCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var auCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var selectedCountries = new RefCountry[] { cnCountry, usCountry, auCountry };
			var countries = new ComplianceRuleTargetCountryCollection();
			countries.AddAll(selectedCountries);

			var item = new ComplianceRuleBulkUpdateMenuItemForTest(null);
			item.CopyComplianceRulesExposed(new ComplianceRule[] { rule1, rule2 }, countries);
			var queryCN = new ZQuery(ComplianceRuleSchema.CRU_Origin, "CN");
			var queryUS = new ZQuery(ComplianceRuleSchema.CRU_Origin, "US");
			var queryAU = new ZQuery(ComplianceRuleSchema.CRU_Origin, "AU");
			AssertContainsExactElementsInAnyOrder(new[] { ("CN", "AU", "1234") }, Factory.Load<ComplianceRule>(queryCN).Select(u => (u.CRU_Origin.ToString(), u.CRU_Destination.ToString(), u.CRU_HarmonizedCode.ToString())));
			AssertContainsExactElementsInAnyOrder(new[] { ("US", "AU", "1234") }, Factory.Load<ComplianceRule>(queryUS).Select(u => (u.CRU_Origin.ToString(), u.CRU_Destination.ToString(), u.CRU_HarmonizedCode.ToString())));
			AssertEquals(0, Factory.Load<ComplianceRule>(queryAU).Length);
		}

		public void TestCopyComplianceRules_RulesWithoutOriginOrWithoutDestination()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var rules = country.ComplianceRules as ComplianceRuleCollection;
			var rule1 = rules.AddNew();
			rule1.CRU_Origin = "";
			rule1.CRU_Destination = "AU";
			rule1.CRU_HarmonizedCode = "12345";
			rule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			var rule2 = rules.AddNew();
			rule2.CRU_Origin = "AU";
			rule2.CRU_Destination = "";
			rule2.CRU_HarmonizedCode = "1234";
			rule2.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			Factory.Save();

			var cnCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var selectedCountries = new RefCountry[] { cnCountry };
			var countries = new ComplianceRuleTargetCountryCollection();
			countries.AddAll(selectedCountries);

			var item = new ComplianceRuleBulkUpdateMenuItemForTest(null);
			item.CopyComplianceRulesExposed(new ComplianceRule[] { rule1, rule2 }, countries);
			var query1 = new ZQuery(ComplianceRuleSchema.CRU_Origin, "CN");
			AssertContainsExactElementsInAnyOrder(new[] { ("CN", "AU", "12345"), ("CN", "", "1234") }, Factory.Load<ComplianceRule>(query1).Select(u => (u.CRU_Origin.ToString(), u.CRU_Destination.ToString(), u.CRU_HarmonizedCode.ToString())));
		}

		public void TestCopyComplianceRulesWithRiskStatus()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var rules = country.ComplianceRules as ComplianceRuleCollection;
			var ruleWithStatus = rules.AddNew();
			ruleWithStatus.CRU_Origin = "AU";
			ruleWithStatus.CRU_Destination = "US";
			ruleWithStatus.CRU_HarmonizedCode = "1234";
			ruleWithStatus.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			var ruleWithBlockStatusSameHsCode = rules.AddNew();
			ruleWithBlockStatusSameHsCode.CRU_Origin = "US";
			ruleWithBlockStatusSameHsCode.CRU_Destination = "AU";
			ruleWithBlockStatusSameHsCode.CRU_HarmonizedCode = "4321";
			ruleWithBlockStatusSameHsCode.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var ruleWithReleaseStatusSameHsCode = rules.AddNew();
			ruleWithReleaseStatusSameHsCode.CRU_Origin = "NZ";
			ruleWithReleaseStatusSameHsCode.CRU_Destination = "AU";
			ruleWithReleaseStatusSameHsCode.CRU_HarmonizedCode = "4321";
			ruleWithReleaseStatusSameHsCode.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			var existingRuleToCopy = rules.AddNew();
			existingRuleToCopy.CRU_Origin = "AU";
			existingRuleToCopy.CRU_Destination = "US";
			existingRuleToCopy.CRU_HarmonizedCode = "111222";
			existingRuleToCopy.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			var existingRule = Factory.New<ComplianceRule>();
			existingRule.CRU_Origin = "CN";
			existingRule.CRU_Destination = "US";
			existingRule.CRU_HarmonizedCode = "111222";
			existingRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			Factory.Save();

			var copiedCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var countries = new ComplianceRuleTargetCountryCollection();
			countries.AddAll(new[] { copiedCountry });

			var item = new ComplianceRuleBulkUpdateMenuItemForTest(null);
			item.CopyComplianceRulesExposed(new[] { ruleWithStatus, ruleWithBlockStatusSameHsCode, ruleWithReleaseStatusSameHsCode, existingRuleToCopy }, countries);

			var countryRules = copiedCountry.ComplianceRules as ComplianceRuleCollection;

			AssertContainsExactElementsInAnyOrder(
				"When destination, hs code are the same, use the first rule's risk status, ignore duplicate rules",
				new[]
				{
					("CN", "US", "1234", ComplianceRiskStatusCodeList.Codes.Released),
					("CN", "AU", "4321", ComplianceRiskStatusCodeList.Codes.Blocked),
					("CN", "US", "111222", ComplianceRiskStatusCodeList.Codes.Released),
				}, countryRules.Select(u => (u.CRU_Origin.ToString(), u.CRU_Destination.ToString(), u.CRU_HarmonizedCode.ToString(), u.CRU_RiskStatus.ToString())));

			AssertEquals("Selected Compliance Rules have been copied successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		class ComplianceRuleBulkUpdateMenuItemForTest : ComplianceRuleBulkUpdateMenuItem
		{
			public ComplianceRuleBulkUpdateMenuItemForTest(ZGrid grid) : base(grid)
			{
			}

			public void CopyComplianceRulesExposed(ComplianceRule[] selectedRules, ComplianceRuleTargetCountryCollection countries) => CopyComplianceRules(selectedRules, countries);
		}
	}
}
