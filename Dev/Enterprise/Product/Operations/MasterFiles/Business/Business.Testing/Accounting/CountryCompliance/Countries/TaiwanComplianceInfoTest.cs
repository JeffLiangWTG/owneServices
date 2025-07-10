using System;
using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(TaiwanComplianceInfo))]
	sealed class TaiwanComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Taiwan;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "NTC", "NTI", "TCD", "TCE", "TCR", "TDC", "TDI", "TDP", "TSD", "TSX", "TXC", "TXE", "TXI", "TXP", "TXS", "XCL", "ZNG" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "NTC", "NTI", "TCD", "TCE", "TCR", "TDC", "TDI", "TDP", "TSD", "TSX", "TXC", "TXE", "TXI", "TXP", "TXS", "XCL", "ZNG" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "NTC", "NTI", "TCD", "TCE", "TCR", "TDC", "TDI", "TDP", "TSD", "TSX", "TXC", "TXE", "TXI", "TXP", "TXS", "XCL", "ZNG" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "NTC", "NTI", "TCD", "TCE", "TCR", "TDC", "TDI", "TDP", "TSD", "TSX", "TXC", "TXE", "TXI", "TXP", "TXS", "XCL", "ZNG" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "NTC", "NTI", "TCD", "TCE", "TCR", "TDC", "TDI", "TDP", "TSD", "TSX", "TXC", "TXE", "TXI", "TXP", "TXS", "XCL", "ZNG" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "NTC", "NTI", "TCD", "TCE", "TCR", "TDC", "TDI", "TDP", "TSD", "TSX", "TXC", "TXE", "TXI", "TXP", "TXS", "XCL", "ZNG" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "NTC", "NTI", "TCD", "TCE", "TCR", "TDC", "TDI", "TDP", "TSD", "TSX", "TXC", "TXE", "TXI", "TXP", "TXS", "XCL", "ZNG" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "NTI";

		protected override string ExpectedComplianceSubTypeDescription => "Non GUI Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "\u6536\u64da";

		protected override string ExpectedComplianceRules => @"TW,NTC,AR,CRD,TID,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,NTC,AR,CRD,TID,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,,,,,
TW,NTC,AP,CRD,TID,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,NTI,AR,INV,TID,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,NTI,AR,INV,TID,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,,,,,
TW,NTI,AP,INV,TID,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,TCD,AR,CRD,TXN,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,,,,,
TW,TCR,AR,CRD,TXN,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,TCR,AP,CRD,TXN,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,TDP,AR,INV,TXN,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,,,,,
TW,TXC,AR,INV,TXN,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,TXC,AP,INV,TXN,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,XCL,AR,INV,EXL,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,XCL,AR,INV,EXL,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,,,,,
TW,XCL,AR,CRD,EXL,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,XCL,AR,CRD,EXL,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,,,,,
TW,XCL,AP,INV,EXL,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,XCL,AP,CRD,EXL,ALL,ALL,,,,1,TXC, TCR, TDP, TCD,,TW,,,,
TW,NTC,AR,CRD,TID,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,NTC,AR,CRD,TID,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,,,,,
TW,NTC,AP,CRD,TID,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,NTI,AR,INV,TID,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,NTI,AR,INV,TID,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,,,,,
TW,NTI,AP,INV,TID,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,TCD,AR,CRD,TXN,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,,,,,
TW,TCE,AR,CRD,TXN,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,TCE,AP,CRD,TXN,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,TDP,AR,INV,TXN,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,,,,,
TW,TXE,AR,INV,TXN,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,TXE,AP,INV,TXN,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,XCL,AR,INV,EXL,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,XCL,AR,INV,EXL,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,,,,,
TW,XCL,AR,CRD,EXL,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,XCL,AR,CRD,EXL,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,,,,,
TW,XCL,AP,INV,EXL,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,XCL,AP,CRD,EXL,ALL,ALL,,,,2,TXE, TCE, TDP, TCD,,TW,,,,
TW,NTC,AR,CRD,TID,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,NTC,AR,CRD,TID,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,,,,,
TW,NTC,AP,CRD,TID,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,NTI,AR,INV,TID,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,NTI,AR,INV,TID,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,,,,,
TW,NTI,AP,INV,TID,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,TCD,AR,CRD,TXN,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,,,,,
TW,TCR,AR,CRD,TXN,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,TCR,AP,CRD,TXN,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,TDP,AR,INV,TXN,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,,,,,
TW,TXC,AR,INV,TXN,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,TXC,AP,INV,TXN,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,XCL,AR,INV,EXL,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,XCL,AR,INV,EXL,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,,,,,
TW,XCL,AR,CRD,EXL,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,XCL,AR,CRD,EXL,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,,,,,
TW,XCL,AP,INV,EXL,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,XCL,AP,CRD,EXL,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,TW,,,,
TW,ZNG,AR,INV,ATZ,ALL,ALL,,,,3,TXC, TCR, TDP, TCD, ZNG,,,,,,
TW,NTC,AR,CRD,TID,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,NTC,AR,CRD,TID,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,,,,,
TW,NTC,AP,CRD,TID,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,NTI,AR,INV,TID,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,NTI,AR,INV,TID,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,,,,,
TW,NTI,AP,INV,TID,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,TCD,AR,CRD,TXN,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,,,,,
TW,TCE,AR,CRD,TXN,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,TCE,AP,CRD,TXN,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,TDP,AR,INV,TXN,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,,,,,
TW,TXE,AR,INV,TXN,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,TXE,AP,INV,TXN,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,XCL,AR,INV,EXL,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,XCL,AR,INV,EXL,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,,,,,
TW,XCL,AR,CRD,EXL,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,XCL,AR,CRD,EXL,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,,,,,
TW,XCL,AP,INV,EXL,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,XCL,AP,CRD,EXL,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,TW,,,,
TW,ZNG,AR,INV,ATZ,ALL,ALL,,,,4,TXE, TCE, TDP, TCD, ZNG,,,,,,
";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override bool ExpectedShouldAutoSetEReportingComplianceDateValue => true;

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_Taiwan()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(TaiwanComplianceInfo.RuleSetCodes.TXCTCR, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(4, ruleSet.Count);
			Assert(ruleSet.ContainsCode(TaiwanComplianceInfo.RuleSetCodes.TXCTCR));
			Assert(ruleSet.ContainsCode(TaiwanComplianceInfo.RuleSetCodes.TXETCE));
			Assert(ruleSet.ContainsCode(TaiwanComplianceInfo.RuleSetCodes.TXCTCRZNG));
			Assert(ruleSet.ContainsCode(TaiwanComplianceInfo.RuleSetCodes.TXETCEZNG));
		}

		public void TestComplianceSubTypeDependencyConfigurationForTaiwan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				AssertEquals(1, collection.Count);
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[0], "TW", "TDI", "TXI");
			}

			void AssertComplianceSubTypeDependencyConfigurationCollection(ComplianceSubTypeDependencyConfiguration item, string expectedCountry, string expectedChildSubType, string expectedParentSubType)
			{
				AssertEquals("Country", expectedCountry, item.Country);
				AssertEquals("Child Sub Type", expectedChildSubType, item.ChildSubType);
				AssertEquals("Parent Sub Type", expectedParentSubType, item.ParentSubType);
			}
		}

		public void TestAllWithTaxIDAndZeroTaxAmount()
		{
			var freevat = Factory.NewWithValidTestData<AccTaxRate>();
			freevat.AT_Code = "FREEVAT";

			var vat = Factory.NewWithValidTestData<AccTaxRate>();
			vat.AT_Code = "VAT";
			Factory.Save();

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AT = freevat.PK;

			var line1 = Factory.NewWithValidTestData<AccTransactionLines>();
			line1.AL_AT = vat.PK;

			IComplianceSubTypeTaxInvoiceRuleWithTaxIDAndZeroAmount info = new TaiwanComplianceInfo();
			Assert(info.AllWithTaxIDAndZeroTaxAmount(new List<AccTransactionLines> { line }));
			Assert(!info.AllWithTaxIDAndZeroTaxAmount(new List<AccTransactionLines> { line1 }));
		}

		public void TestIComplianceSubTypeRuleSortByParentTransactionSubType()
		{
			var info = new TaiwanComplianceInfo();
			Assert(info is IComplianceSubTypeRuleSortByParentTransactionSubType);
		}

		#region IComplianceSequencesValidationProvider

		public void TestIsPrintingAuthorizationNumberLengthValid()
		{
			var complianceSequenceValidatorProvider = GetCountryComplianceInfo as IComplianceSequenceValidationProvider;

			AssertEquals(expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberLengthValid("anyValue"));
		}

		public void TestIsPrintingAuthorizationNumberFormatValid()
		{
			var complianceSequenceValidatorProvider = GetCountryComplianceInfo as IComplianceSequenceValidationProvider;

			AssertEquals(expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberFormatValid("anyValue"));
		}

		public void TestGetAccComplianceSequenceValidation()
		{
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			var complianceSequenceValidatorProvider = GetCountryComplianceInfo as IComplianceSequenceValidationProvider;
			var complianceSequenceValidation = complianceSequenceValidatorProvider.GetAccComplianceSequenceValidation(complianceSequence);

			AssertType(typeof(AccComplianceSequenceTaiwanValidation), complianceSequenceValidation);
		}

		#endregion IComplianceSequencesValidationProvider
	}
}
