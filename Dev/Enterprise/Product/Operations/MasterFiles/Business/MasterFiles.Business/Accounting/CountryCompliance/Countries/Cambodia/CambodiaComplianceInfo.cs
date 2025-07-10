using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CambodiaComplianceInfo : CountryComplianceInfo, IComplianceSubTypeCodeProvider, IComplianceSubTypeRulesWithMultipleRuleSetProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Cambodia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.CXI,
				ComplianceSubTypes.CCR,
				ComplianceSubTypes.DSB,
				ComplianceSubTypes.DCR,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TCR = "TCR";
			public const string CXI = "CXI";
			public const string CCR = "CCR";
			public const string DSB = "DSB";
			public const string DCR = "DCR";
		}

		public static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI => ResString.GetMultilingualString("KHComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString TCR => ResString.GetMultilingualString("KHComplianceSubTypeCodeList|TCR", "Tax Credit Note");
			public static MultilingualString CXI => ResString.GetMultilingualString("KHComplianceSubTypeCodeList|CXI", "Commercial Invoice");
			public static MultilingualString CCR => ResString.GetMultilingualString("KHComplianceSubTypeCodeList|CCR", "Commercial Credit Note");
			public static MultilingualString DSB => ResString.GetMultilingualString("KHComplianceSubTypeCodeList|DSB", "Disbursement Invoice");
			public static MultilingualString DCR => ResString.GetMultilingualString("KHComplianceSubTypeCodeList|DCR", "Disbursement Credit Note");
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "Tax Invoice";
			public const string TCR = "Tax Credit Note";
			public const string CXI = "Commercial Invoice";
			public const string CCR = "Commercial Credit Note";
			public const string DSB = "Disbursement Invoice";
			public const string DCR = "Disbursement Credit Note";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TXI = "Used to sub-classify Receivables Invoice (INV) transactions where the Debtor is tax registered";
			public const string TCR = "Used to sub-classify Receivables Credit Note (CRD) transactions where the Debtor is tax registered";
			public const string CXI = "Used to sub-classify Receivables Invoice (INV) transactions where the Debtor is not tax registered";
			public const string CCR = "Used to sub-classify Receivables Credit Note (CRD) transactions where the Debtor is not tax registered";
			public const string DSB = "Used to record Invoice (INV) transactions recorded for Disbursement / Reimbursement / Excluded supply purposes and where no Tax Invoice or Commercial Invoice is to be issued / reported";
			public const string DCR = "Used to record Credit Note (CRD) transactions recorded for Disbursement / Reimbursement / Excluded supply purposes and where no Tax Invoice or Commercial Invoice is to be issued / reported";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType CXI => new ComplianceSubType(ComplianceSubTypeCodes.CXI, () => ComplianceSubTypeDescriptions.CXI, () => ComplianceSubTypeLocalDescriptions.CXI, () => ComplianceSubTypeInternalImplemenationNote.CXI, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType CCR => new ComplianceSubType(ComplianceSubTypeCodes.CCR, () => ComplianceSubTypeDescriptions.CCR, () => ComplianceSubTypeLocalDescriptions.CCR, () => ComplianceSubTypeInternalImplemenationNote.CCR, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType DSB => new ComplianceSubType(ComplianceSubTypeCodes.DSB, () => ComplianceSubTypeDescriptions.DSB, () => ComplianceSubTypeLocalDescriptions.DSB, () => ComplianceSubTypeInternalImplemenationNote.DSB, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.ALL);
			public static ComplianceSubType DCR => new ComplianceSubType(ComplianceSubTypeCodes.DCR, () => ComplianceSubTypeDescriptions.DCR, () => ComplianceSubTypeLocalDescriptions.DCR, () => ComplianceSubTypeInternalImplemenationNote.DCR, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.ALL);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TaxAndCommercialInvoice, RuleSetDescriptions.TaxAndCommercialInvoice);
			rulesetList.AddPair(RuleSetCodes.TaxCommercialAndDisbursementInvoice, RuleSetDescriptions.TaxCommercialAndDisbursementInvoice);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.TaxAndCommercialInvoice;
		}

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			switch (ruleSetCode)
			{
				case RuleSetCodes.TaxAndCommercialInvoice:
					AddComplianceSubTypeAttributionRulesForTaxAndCommercialInvoice(collection);
					break;
				case RuleSetCodes.TaxCommercialAndDisbursementInvoice:
					AddComplianceSubTypeAttributionRulesForTaxCommercialAndDisbursementInvoice(collection);
					break;
				case null:
				case "":
					AddComplianceSubTypeAttributionRulesForTaxAndCommercialInvoice(collection);
					AddComplianceSubTypeAttributionRulesForTaxCommercialAndDisbursementInvoice(collection);
					break;
				default:
					ErrorReporter.ReportOnce("CambodiaComplianceInfo_IncorrectRuleSetCode", "Incorrect Cambodia Rule Set Code");
					break;
			}
		}

		void AddComplianceSubTypeAttributionRulesForTaxAndCommercialInvoice(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Cambodia;
			configuration.RuleSetCode = RuleSetCodes.TaxAndCommercialInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxAndCommercialInvoice;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Cambodia;
			configuration.RuleSetCode = RuleSetCodes.TaxAndCommercialInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxAndCommercialInvoice;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.CXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TaxAndCommercialInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxAndCommercialInvoice;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.CCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TaxAndCommercialInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxAndCommercialInvoice;
		}

		void AddComplianceSubTypeAttributionRulesForTaxCommercialAndDisbursementInvoice(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Cambodia;
			configuration.RuleSetCode = RuleSetCodes.TaxCommercialAndDisbursementInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxCommercialAndDisbursementInvoice;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Cambodia;
			configuration.RuleSetCode = RuleSetCodes.TaxCommercialAndDisbursementInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxCommercialAndDisbursementInvoice;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.CXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.RuleSetCode = RuleSetCodes.TaxCommercialAndDisbursementInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxCommercialAndDisbursementInvoice;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.CCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.RuleSetCode = RuleSetCodes.TaxCommercialAndDisbursementInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxCommercialAndDisbursementInvoice;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.DSB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.RuleSetCode = RuleSetCodes.TaxCommercialAndDisbursementInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxCommercialAndDisbursementInvoice;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Cambodia;
			configuration.SubType = CambodiaComplianceInfo.ComplianceSubTypeCodes.DCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.RuleSetCode = RuleSetCodes.TaxCommercialAndDisbursementInvoice;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxCommercialAndDisbursementInvoice;
		}

		#region SuppressResourceStringsCheckRegion

		public static class RuleSetCodes
		{
			public const string TaxAndCommercialInvoice = "1";
			public const string TaxCommercialAndDisbursementInvoice = "2";
		}

		static class RuleSetDescriptions
		{
			public const string TaxAndCommercialInvoice = "Tax and Commercial Invoice";
			public const string TaxCommercialAndDisbursementInvoice = "Tax, Commercial and Disbursement Invoice";
		}

		#endregion

		#endregion
	}
}
