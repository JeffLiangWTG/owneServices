using System.Collections.Generic;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(JordanComplianceInfo))]
	sealed class JordanComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Jordan;

		protected override string[] ExpectedComplianceSubTypes => ["TXI", "TXC", "NTI", "NTC" ];

		protected override string[] ExpectedReceivablesComplianceSubTypes => ["TXI", "TXC", "NTI", "NTC" ];

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => ["TXI", "NTI" ];

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => ["TXC", "NTC" ];

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TXI";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "الفاتورة الضريبية";

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => ["TXI", "TXC", "NTI", "NTC" ];

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Jordan;

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse>
		{
			{ "TXI", TransactionTypeOfUse.INV },
			{ "TXC", TransactionTypeOfUse.CRD },
			{ "NTI", TransactionTypeOfUse.INV },
			{ "NTC", TransactionTypeOfUse.CRD }
		};

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new()
		{
			{ "TXI", LedgerOfUse.AR },
			{ "TXC", LedgerOfUse.AR },
			{ "NTI", LedgerOfUse.AR },
			{ "NTC", LedgerOfUse.AR }
		};

		#region IEInvoicingRegistryProviderTest

		protected override bool ExpectedShouldAutoSetEReportingComplianceDateValue => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		protected override string ExpectedGovernmentAllocatedNumberColumnName => string.Empty;

		#endregion

		protected override string ExpectedComplianceRules =>
@"JO,TXI,AR,INV,TID,ALL,ALL,,,,1,TXI(Tax Invoice),,,,,,
JO,TXC,AR,CRD,TID,ALL,ARO,,TXI,,1,TXI(Tax Invoice),,,,,,
JO,TXI,AR,INV,TNE,ALL,ALL,,,,2,TXI(Tax Invoice Excluding Not Reportable and Excluded from Tax Base Charges),,,,,,
JO,TXC,AR,CRD,TNE,ALL,ARO,,TXI,,2,TXI(Tax Invoice Excluding Not Reportable and Excluded from Tax Base Charges),,,,,,
JO,NTI,AR,INV,NON,ALL,ALL,,,,3,NTI(Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,NTI,,3,NTI(Non-Tax Invoice),,,,,,
JO,NTI,AR,INV,EXL,ALL,ALL,,,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTC,AR,CRD,EXL,ALL,ARO,,NTI,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTI,AR,INV,NOT,ALL,ALL,,,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTC,AR,CRD,NOT,ALL,ARO,,NTI,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTI,AR,INV,NON,ALL,ALL,,,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,NTI,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTI,AR,INV,EXL,ALL,ALL,,,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTC,AR,CRD,EXL,ALL,ARO,,NTI,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTI,AR,INV,NOT,ALL,ALL,,,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTC,AR,CRD,NOT,ALL,ARO,,NTI,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTI,AR,INV,EXT,ALL,ALL,,,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTC,AR,CRD,EXT,ALL,ARO,,NTI,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTI,AR,INV,NON,ALL,ALL,,,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,NTI,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,TXI,AR,INV,TID,ALL,ALL,,,,6,ALL(Tax Invoice plus Non-Tax Invoice),,,,,,
JO,TXC,AR,CRD,TID,ALL,ARO,,,,6,ALL(Tax Invoice plus Non-Tax Invoice),,,,,,
JO,NTI,AR,INV,NON,ALL,ALL,,,,6,ALL(Tax Invoice plus Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,,,6,ALL(Tax Invoice plus Non-Tax Invoice),,,,,,
JO,TXI,AR,INV,TNE,ALL,ALL,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,TXC,AR,CRD,TNE,ALL,ARO,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTI,AR,INV,EXL,ALL,ALL,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,EXL,ALL,ARO,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTI,AR,INV,NOT,ALL,ALL,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,NOT,ALL,ARO,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTI,AR,INV,NON,ALL,ALL,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,";

		public void TestJordanComplianceSubTypeAttributionRuleSet_GetRuleSetAndDefaultRuleSet()
		{
			var ruleSetProvider = ComplianceInfo as IComplianceSubTypeRulesWithMultipleRuleSetProvider;
			var ruleSetDefault = ruleSetProvider.GetDefaultRuleSet();
			var ruleSet = ruleSetProvider.GetRuleSet();
			
			CombineAssertions(() =>
			{
				AssertEquals("Default Rule Set", JordanComplianceInfo.RuleSetCodes.DefaultJordanRuleSet, ruleSetDefault);

				AssertEquals("Rule Set Count", 7, ruleSet.Count);
				Assert("Rule Set Contains Rule 1", ruleSet.ContainsCode(JordanComplianceInfo.RuleSetCodes.DefaultJordanRuleSet));
				Assert("Rule Set Contains Rule 2", ruleSet.ContainsCode(JordanComplianceInfo.RuleSetCodes.JordanRuleSet2));
				Assert("Rule Set Contains Rule 3", ruleSet.ContainsCode(JordanComplianceInfo.RuleSetCodes.JordanRuleSet3));
				Assert("Rule Set Contains Rule 4", ruleSet.ContainsCode(JordanComplianceInfo.RuleSetCodes.JordanRuleSet4));
				Assert("Rule Set Contains Rule 5", ruleSet.ContainsCode(JordanComplianceInfo.RuleSetCodes.JordanRuleSet5));
				Assert("Rule Set Contains Rule 6", ruleSet.ContainsCode(JordanComplianceInfo.RuleSetCodes.JordanRuleSet6));
				Assert("Rule Set Contains Rule 7", ruleSet.ContainsCode(JordanComplianceInfo.RuleSetCodes.JordanRuleSet7));
			});
		}

		public void TestJordanComplianceSubTypeAttributionRuleSets()
		{
			var expectRuleSetList = new List<string>
			{
				@"JO,TXI,AR,INV,TID,ALL,ALL,,,,1,TXI(Tax Invoice),,,,,,
JO,TXC,AR,CRD,TID,ALL,ARO,,TXI,,1,TXI(Tax Invoice),,,,,,",
				@"JO,TXI,AR,INV,TNE,ALL,ALL,,,,2,TXI(Tax Invoice Excluding Not Reportable and Excluded from Tax Base Charges),,,,,,
JO,TXC,AR,CRD,TNE,ALL,ARO,,TXI,,2,TXI(Tax Invoice Excluding Not Reportable and Excluded from Tax Base Charges),,,,,,",
				@"JO,NTI,AR,INV,NON,ALL,ALL,,,,3,NTI(Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,NTI,,3,NTI(Non-Tax Invoice),,,,,,",
				@"JO,NTI,AR,INV,EXL,ALL,ALL,,,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTC,AR,CRD,EXL,ALL,ARO,,NTI,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTI,AR,INV,NOT,ALL,ALL,,,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTC,AR,CRD,NOT,ALL,ARO,,NTI,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTI,AR,INV,NON,ALL,ALL,,,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,NTI,,4,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice),,,,,,",
				@"JO,NTI,AR,INV,EXL,ALL,ALL,,,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTC,AR,CRD,EXL,ALL,ARO,,NTI,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTI,AR,INV,NOT,ALL,ALL,,,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTC,AR,CRD,NOT,ALL,ARO,,NTI,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTI,AR,INV,EXT,ALL,ALL,,,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTC,AR,CRD,EXT,ALL,ARO,,NTI,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTI,AR,INV,NON,ALL,ALL,,,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,NTI,,5,NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice),,,,,,",
				@"JO,TXI,AR,INV,TID,ALL,ALL,,,,6,ALL(Tax Invoice plus Non-Tax Invoice),,,,,,
JO,TXC,AR,CRD,TID,ALL,ARO,,,,6,ALL(Tax Invoice plus Non-Tax Invoice),,,,,,
JO,NTI,AR,INV,NON,ALL,ALL,,,,6,ALL(Tax Invoice plus Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,,,6,ALL(Tax Invoice plus Non-Tax Invoice),,,,,,",
				@"JO,TXI,AR,INV,TNE,ALL,ALL,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,TXC,AR,CRD,TNE,ALL,ARO,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTI,AR,INV,EXL,ALL,ALL,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,EXL,ALL,ARO,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTI,AR,INV,NOT,ALL,ALL,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,NOT,ALL,ARO,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTI,AR,INV,NON,ALL,ALL,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,
JO,NTC,AR,CRD,NON,ALL,ARO,,,,7,ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice),,,,,,"
			};
			var expectRuleSetCodesList = new List<string> { "1", "2", "3", "4", "5", "6", "7" };
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();

			CombineAssertions(() =>
			{
				for (var i = 0; i < expectRuleSetCodesList.Count; i++)
				{
					collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
					CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(collection, expectRuleSetCodesList[i]);
					AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, expectRuleSetList[i]);
				}
			});
		}

		JordanComplianceInfo ComplianceInfo => TestObject as JordanComplianceInfo;
	}
}
