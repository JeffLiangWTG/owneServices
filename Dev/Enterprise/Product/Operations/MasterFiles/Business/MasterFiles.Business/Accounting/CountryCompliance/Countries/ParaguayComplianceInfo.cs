using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ParaguayComplianceInfo : CountryComplianceInfo, IComplianceSubTypeCodeProvider, IComplianceSubTypeRulesWithMultipleRuleSetProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Paraguay;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.ParaguayCodeTypes.RUC;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.ParaguayCodeTypes.RUC;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCR,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TCD = "TCD";
			public const string TCR = "TCR";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI => ResString.GetMultilingualString("PYComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString TCD => ResString.GetMultilingualString("PYComplianceSubTypeCodeList|TCD", "Debit Note");
			public static MultilingualString TCR => ResString.GetMultilingualString("PYComplianceSubTypeCodeList|TCR", "Credit Note");
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "Factura";
			public const string TCD = "Nota de Débito";
			public const string TCR = "Nota de Crédito";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TXI = "Used in both Receivables and Payables to sub classify Invoice (INV) transactions.";
			public const string TCD = "Used in both Receivables and Payables to sub-classify Amending or Reversal Invoice (INV) transactions.";
			public const string TCR = "Used in both Receivables and Payables to sub-classify Amending or Reversal Credit Note (CRD) transactions.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI);

			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD);

			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.TXITCRTCD)
			{
				AddComplianceSubTypeAttributionRulesTXITCRTCD(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode))
			{
				AddComplianceSubTypeAttributionRulesTXITCRTCD(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("ParaguayComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
			}
		}

		void AddComplianceSubTypeAttributionRulesTXITCRTCD(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			#region AR

			ComplianceSubTypeAttributionRuleConfiguration configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCD;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCD;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCD;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCD;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCD;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCD;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCD;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCD;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCD;

			#endregion AR
		}

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TXITCRTCD, RuleSetDescriptions.TXITCRTCD);
			return rulesetList;
		}

		public static class RuleSetCodes
		{
			public const string TXITCRTCD = "1";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Descriptions for compliance rule sets are not translated")]
		static class RuleSetDescriptions
		{
			public const string TXITCRTCD = "TXI, TCR & TCD";
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.TXITCRTCD;
		}
		#endregion
	}
}
