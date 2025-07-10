using System.Linq;
using System.Windows.Forms;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Test
{
	[TestedType(typeof(ComplianceRuleBulkUpdateConfirmForm))]
	public class ComplianceRuleBulkUpdateConfirmFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestComplianceRuleGridColumnNames()
		{
			using (var form = new ComplianceRuleBulkUpdateConfirmForm(new ComplianceRuleWrapper(new ComplianceRuleCandidateCollection(), new ComplianceRuleTargetCountryCollection())))
			{
				var expectedColumns = new[] { "Origin", "Destination", "HarmonizedCode", "RiskStatus" };
				AssertContainsExactElementsInAnyOrder(expectedColumns, form.ComplianceRuleGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(a => a.ColumnName).ToList());
			}
		}

		public void TestCountryGridColumnNames()
		{
			using (var form = new ComplianceRuleBulkUpdateConfirmForm(new ComplianceRuleWrapper(new ComplianceRuleCandidateCollection(), new ComplianceRuleTargetCountryCollection())))
			{
				var expectedColumns = new[] { "Code", "Name" };
				AssertContainsExactElementsInAnyOrder(expectedColumns, form.CountryGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(a => a.ColumnName).ToList());
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ComplianceRuleBulkUpdateConfirmForm(new ComplianceRuleWrapper(new ComplianceRuleCandidateCollection(), new ComplianceRuleTargetCountryCollection()));
		}
	}
}
