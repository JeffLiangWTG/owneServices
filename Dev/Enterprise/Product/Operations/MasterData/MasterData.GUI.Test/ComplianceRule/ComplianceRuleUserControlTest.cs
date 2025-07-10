using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI.Test
{
	class ComplianceRuleUserControlTest : TestCaseWithFactory
	{
		public void TestComplianceRuleGrid()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var rules = (country.ComplianceRules as ComplianceRuleCollection);
			rules.AddNew();
			rules.AddNew();

			using var form = new ZForm(country);
			using var control = new ComplianceRuleUserControl();
			control.SetBindingMember("ComplianceRules");
			form.Controls.Add(control);
			form.Show();

			AssertEquals(2, control.ComplianceRuleGrid.ListManager.Count);
			AssertContainsExactElementsInAnyOrder(new[] { ("CRU_Origin", true), ("CRU_Destination", true), ("CRU_HarmonizedCode", true), ("CRU_RiskStatus", true) }, control.ComplianceRuleGrid.Columns.Select(u => (u.ColumnName, u.IsVisible)));
		}
	}
}
