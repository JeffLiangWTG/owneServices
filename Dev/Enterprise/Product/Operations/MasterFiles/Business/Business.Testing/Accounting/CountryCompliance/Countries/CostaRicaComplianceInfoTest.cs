using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(CostaRicaComplianceInfo))]
	sealed class CostaRicaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.CostaRica;

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new[] { "TXI", "TXE", "TCR", "TCD" };

		protected override ZDate ExpectedEInvoicingComplianceDate => ZDate.Empty;

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.CostaRica;

		protected override string ExpectedGovernmentAllocatedNumberColumnName => string.Empty;

		protected override ZString ExpectedDefaultEInvoicingSubmitPivotStatus => "QUE";

		protected override string ExpectedDefaultEInvoicingSubmitPivotStatusForPayables => string.Empty;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "XCL", "TXE" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "XCL", "TXE" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TCD", "TXI", "XCL", "TXE" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TCD", "TXI", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TCR", "XCL" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCD";

		protected override string ExpectedComplianceSubTypeDescription => "Debit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota de D\u00e9bito";

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse>
		{
			{ "TCD", TransactionTypeOfUse.INV }, { "TCR", TransactionTypeOfUse.CRD }, { "TXI", TransactionTypeOfUse.INV },
			{ "XCL", TransactionTypeOfUse.ALL }, { "TXE", TransactionTypeOfUse.INV }
		};

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> { { "TXE", LedgerOfUse.AR } };

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public override void TestHasExtraTaxInfo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(true, complianceInfo.HasExtraTaxInfo());
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("Exon. Amt", complianceInfo.GetExtraTaxOSAmountCaption().ShortCaption);
			AssertEquals("Exon. Amount", complianceInfo.GetExtraTaxOSAmountCaption().Caption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("Exon. Local", complianceInfo.GetExtraTaxLocalAmountCaption().Caption);
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("IVA Amt", complianceInfo.GetTaxOSAmountCaption().ShortCaption);
			AssertEquals("IVA Amount", complianceInfo.GetTaxOSAmountCaption().Caption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("IVA Local", complianceInfo.GetTaxLocalAmountCaption().Caption);
		}

		protected override string ExpectedComplianceRules => @"CR,TCD,AR,INV,TID,ALL,ARO,CR,TXI,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,TCD,AR,INV,TID,ALL,ARO,CR,TCR,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,TCR,AR,CRD,TID,ALL,ARO,CR,TCD,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,TCR,AR,CRD,TID,ALL,ARO,CR,TXI,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,TXI,AR,INV,TID,ALL,OTO,CR,,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,TXE,AR,INV,TXX,ALL,OTO,,,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,TCD,AR,INV,TXX,ALL,ARO,,TXE,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,TCD,AR,INV,TXX,ALL,ARO,,TCR,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,TCR,AR,CRD,TXX,ALL,ARO,,TCD,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,TCR,AR,CRD,TXX,ALL,ARO,,TXE,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,XCL,AR,INV,EXL,ALL,OTO,,,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,XCL,AR,CRD,EXL,ALL,OTO,,,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,XCL,AR,INV,EXL,ALL,ARO,,XCL,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
CR,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,1,TXI, TXE, TCR, TCD & XCL,,,,,,
";

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_CostaRica()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(CostaRicaComplianceInfo.RuleSetCodes.TXI_TXE_TCR_TCD_XCL, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();

			AssertEquals(1, ruleSet.Count);
			Assert(ruleSet.ContainsCode(CostaRicaComplianceInfo.RuleSetCodes.TXI_TXE_TCR_TCD_XCL));
		}
	}
}
