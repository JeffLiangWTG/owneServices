using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(ParaguayComplianceInfo))]
	sealed class ParaguayComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Paraguay;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TXI", "TCD", "TCR" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXI", "TCD", "TCR" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TXI", "TCD", "TCR" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCD", "TCR" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCD", "TCR" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TCD", "TCR" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TCD", "TCR" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCR";

		protected override string ExpectedComplianceSubTypeDescription => "Credit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota de Crédito";

		protected override string ExpectedComplianceRules => @"PY,TXI,AR,INV,TID,ALL,OTO,,,,1,TXI, TCR & TCD,,,,,,
PY,TCR,AR,CRD,TID,ALL,ARO,,TXI,,1,TXI, TCR & TCD,,,,,,
PY,TCD,AR,INV,TID,ALL,ARO,,TXI,,1,TXI, TCR & TCD,,,,,,";

		#region TestComplianceSubTypeRulesWithMultipleRuleSetProvider

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_Paraguay()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);
			AssertEquals(ParaguayComplianceInfo.RuleSetCodes.TXITCRTCD, ruleSetProvider.GetDefaultRuleSet());
			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(1, ruleSet.Count);
			Assert(ruleSet.ContainsCode(ParaguayComplianceInfo.RuleSetCodes.TXITCRTCD));
		}

		#endregion
	}
}
