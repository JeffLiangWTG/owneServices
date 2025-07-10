using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(NicaraguaComplianceInfo))]
	sealed class NicaraguaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Nicaragua;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TCR", "TXA", "TCD", "XCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TCR", "TXA", "TCD", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TCR", "TXA", "TCD", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TCR", "TXA", "TCD", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TCR", "TXA", "TCD", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR", "TXA", "TCD", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TCR", "TXA", "TCD", "XCL" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCR";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Credit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota de Crédito";

		protected override string ExpectedComplianceRules => @"NI,TXA,AR,INV,TID,ALL,OTO,,,,1,Tax Documents A and Excluded Supply,,,,,,
NI,TCR,AR,CRD,TID,ALL,ARO,,TXA,,1,Tax Documents A and Excluded Supply,,,,,,
NI,TCD,AR,INV,TID,ALL,ARO,,TXA,,1,Tax Documents A and Excluded Supply,,,,,,
NI,XCL,AR,INV,EXL,ALL,OTO,,,,1,Tax Documents A and Excluded Supply,,,,,,
NI,XCL,AR,INV,EXL,ALL,ARO,,XCL,,1,Tax Documents A and Excluded Supply,,,,,,
NI,XCL,AR,CRD,EXL,ALL,OTO,,,,1,Tax Documents A and Excluded Supply,,,,,,
NI,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,1,Tax Documents A and Excluded Supply,,,,,,
";

		public void TestComplianceSubTypeAttributionRule_DefaultValue_Nicaragua()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);
			AssertEquals(NicaraguaComplianceInfo.RuleSetCodes.TaxDocumentsAAndExcludedSupply, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(1, ruleSet.Count);
			Assert(ruleSet.ContainsCode(NicaraguaComplianceInfo.RuleSetCodes.TaxDocumentsAAndExcludedSupply));
		}
	}
}
