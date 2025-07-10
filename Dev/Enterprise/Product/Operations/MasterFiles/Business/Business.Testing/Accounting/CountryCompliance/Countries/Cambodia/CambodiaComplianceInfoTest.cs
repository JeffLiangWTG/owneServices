using System.Collections.Generic;
using Enterprise.Integration.Compliance;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(CambodiaComplianceInfo))]
	sealed class CambodiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Cambodia;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TXI", "TCR", "CXI", "CCR", "DSB", "DCR" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXI", "TCR", "CXI", "CCR", "DSB", "DCR" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "CXI", "DSB", "DCR" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR", "CCR", "DSB", "DCR" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TXI";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Tax Invoice";

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> {
			{ "TXI", LedgerOfUse.AR }, { "TCR", LedgerOfUse.AR },
			{ "CXI", LedgerOfUse.AR }, { "CCR", LedgerOfUse.AR },
			{ "DSB", LedgerOfUse.AR }, { "DCR", LedgerOfUse.AR }
		};

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse> {
			{ "TXI", TransactionTypeOfUse.INV }, { "TCR", TransactionTypeOfUse.CRD }, { "CXI", TransactionTypeOfUse.INV },
			{ "CCR", TransactionTypeOfUse.CRD }, { "DSB", TransactionTypeOfUse.ALL }, { "DCR", TransactionTypeOfUse.ALL }
		};

		protected override string ExpectedComplianceRules => @"KH,TXI,AR,INV,TID,ALL,ALL,,,,1,Tax and Commercial Invoice,,KH,,,,
KH,TCR,AR,CRD,TID,ALL,ALL,,,,1,Tax and Commercial Invoice,,KH,,,,
KH,CXI,AR,INV,TID,ALL,ALL,,,,1,Tax and Commercial Invoice,,,,,,
KH,CCR,AR,CRD,TID,ALL,ALL,,,,1,Tax and Commercial Invoice,,,,,,
KH,TXI,AR,INV,TID,NDB,ALL,,,,2,Tax, Commercial and Disbursement Invoice,,KH,,,,
KH,TCR,AR,CRD,TID,NDB,ALL,,,,2,Tax, Commercial and Disbursement Invoice,,KH,,,,
KH,CXI,AR,INV,TID,NDB,ALL,,,,2,Tax, Commercial and Disbursement Invoice,,,,,,
KH,CCR,AR,CRD,TID,NDB,ALL,,,,2,Tax, Commercial and Disbursement Invoice,,,,,,
KH,DSB,AR,INV,EXL,DSB,ALL,,,,2,Tax, Commercial and Disbursement Invoice,,,,,,
KH,DCR,AR,CRD,EXL,DSB,ALL,,,,2,Tax, Commercial and Disbursement Invoice,,,,,,
";

		public void TestComplianceSubTypeAttributionRule_DefaultValue_Cambodia()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);
			AssertEquals(CambodiaComplianceInfo.RuleSetCodes.TaxAndCommercialInvoice, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(2, ruleSet.Count);
			Assert(ruleSet.ContainsCode(CambodiaComplianceInfo.RuleSetCodes.TaxAndCommercialInvoice));
			Assert(ruleSet.ContainsCode(CambodiaComplianceInfo.RuleSetCodes.TaxCommercialAndDisbursementInvoice));
		}
	}
}
