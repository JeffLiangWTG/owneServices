using System;
using System.Collections.Generic;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(ElSalvadorComplianceInfo))]
	sealed class ElSalvadorComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.ElSalvador;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TCD", "TCR", "TXE", "TXI", "TXN", "TCF" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TCD", "TCR", "TXE", "TXI", "TXN", "TCF" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TCD", "TCR", "TXE", "TXI", "TXN", "TCF" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TCD", "TXE", "TXI", "TXN" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TCD", "TXE", "TXI", "TXN" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR", "TCF" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TCR", "TCF" };

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse =>
			new Dictionary<string, TransactionTypeOfUse>
			{
				{ "TCD", TransactionTypeOfUse.INV },
				{ "TCR", TransactionTypeOfUse.CRD },
				{ "TXE", TransactionTypeOfUse.INV },
				{ "TXI", TransactionTypeOfUse.INV },
				{ "TXN", TransactionTypeOfUse.INV },
				{ "TCF", TransactionTypeOfUse.CRD }
			};

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCD";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Debit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota de Debito";

		protected override Func<ComplianceSubTypeAttributionRuleConfiguration, string>[] ExpectedComplianceRuleFields =>
			new Func<ComplianceSubTypeAttributionRuleConfiguration, string>[]
			{
				x => x.Country,
				x => x.SubType,
				x => x.LedgerType,
				x => x.InvoiceType,
				x => x.TaxInvoiceRule,
				x => x.DisbursementRule,
				x => x.OriginalRule,
				x => x.OrganisationLocation,
				x => x.ParentTransactionSubType,
				x => x.TaxRegistrationType,
				x => x.ExporterExemption,
				x => x.TaxIDCode,
				x => x.RuleSetCode,
				x => x.RuleSetDescription
			};

		protected override string ExpectedComplianceRules =>
@"SV,TCD,AR,INV,TID,ALL,ARO,,,NRC,NON,,1,TXI, TCD, TCR, TXN, TXE & TCF
SV,TCR,AR,CRD,TID,ALL,OTO,,,NRC,NON,,1,TXI, TCD, TCR, TXN, TXE & TCF
SV,TCR,AR,CRD,TID,ALL,ARO,,,NRC,NON,,1,TXI, TCD, TCR, TXN, TXE & TCF
SV,TXI,AR,INV,TID,ALL,OTO,,,NRC,NON,,1,TXI, TCD, TCR, TXN, TXE & TCF
SV,TXN,AR,INV,TID,ALL,OTO,,,,NON,,1,TXI, TCD, TCR, TXN, TXE & TCF
SV,TXE,AR,INV,TID,ALL,OTO,,,,EXV,,1,TXI, TCD, TCR, TXN, TXE & TCF
SV,TCF,AR,CRD,TID,ALL,ARO,,TXN,,NON,,1,TXI, TCD, TCR, TXN, TXE & TCF
SV,TCF,AR,CRD,TID,ALL,ARO,,TXE,,EXV,,1,TXI, TCD, TCR, TXN, TXE & TCF
";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public override void TestHasExtraTaxInfo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(true, complianceInfo.HasExtraTaxInfo());
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("RET Amt", complianceInfo.GetExtraTaxOSAmountCaption().ShortCaption);
			AssertEquals("RET Amount", complianceInfo.GetExtraTaxOSAmountCaption().Caption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("RET Local", complianceInfo.GetExtraTaxLocalAmountCaption().Caption);
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

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(ElSalvadorComplianceInfo.RuleSetCodes.AllSubTypes, ruleSetProvider.GetDefaultRuleSet());
		}

		public void TestComplianceSubTypeAttributionGetRuleSet()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(1, ruleSet.Count);
			Assert(ruleSet.ContainsCode(ElSalvadorComplianceInfo.RuleSetCodes.AllSubTypes));
		}
	}
}
