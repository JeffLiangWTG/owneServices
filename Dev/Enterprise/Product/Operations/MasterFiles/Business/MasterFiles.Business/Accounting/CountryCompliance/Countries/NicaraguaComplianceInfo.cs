using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class NicaraguaComplianceInfo : CountryComplianceInfo, IComplianceSubTypeCodeProvider, IComplianceSubTypeRulesWithMultipleRuleSetProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Nicaragua;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.NicaraguaCodeTypes.RUC;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.NicaraguaCodeTypes.RUC;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TXA,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.XCL,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TCR = "TCR";
			public const string TXA = "TXA";
			public const string TCD = "TCD";
			public const string XCL = "XCL";
		}

		public static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TCR => ResString.GetMultilingualString("ESComplianceSubTypeCodeList|TCR", "Tax Credit Note");
			public static MultilingualString TXA => ResString.GetMultilingualString("ESComplianceSubTypeCodeList|TXA", "Tax Invoice Type A");
			public static MultilingualString TCD => ResString.GetMultilingualString("ESComplianceSubTypeCodeList|TCD", "Tax Debit Note");
			public static MultilingualString XCL => ResString.GetMultilingualString("ESComplianceSubTypeCodeList|XCL", "Reimbursement / Disbursement / Excluded Supply");
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TCR = "Nota de Crédito";
			public const string TXA = "Factura A";
			public const string TCD = "Nota de Débito";
			public const string XCL = "Documento de Reembolso";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TCR = "Used to record Nota de Crédito. Used to record amending credit notes (CRD) with a parent TXA transaction";
			public const string TXA = "Used to record Factura A Invoices. Used to identify invoices (INV) where the taxpayer has an IVA obligation";
			public const string TCD = "Used to record Nota de Débito. Used to record amending debit notes (INV) with a parent TXA transaction";
			public const string XCL = "Used to formally identify Reimbursement / Disbursement / Excluded Supply transactions. Used in conjuction with the 'Exclude' Tax ID. This type is used to identify transctions where no reportable supply was made or received and no fiscal document is required.";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR);
			public static ComplianceSubType TXA => new ComplianceSubType(ComplianceSubTypeCodes.TXA, () => ComplianceSubTypeDescriptions.TXA, () => ComplianceSubTypeLocalDescriptions.TXA, () => ComplianceSubTypeInternalImplemenationNote.TXA);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TaxDocumentsAAndExcludedSupply, RuleSetDescriptions.TaxDocumentsAAndExcludedSupply);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.TaxDocumentsAAndExcludedSupply;
		}

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			switch (ruleSetCode)
			{
				case RuleSetCodes.TaxDocumentsAAndExcludedSupply:
					AddComplianceSubTypeAttributionRulesForTaxDocumentsAAndExcludedSupply(collection);
					break;
				case null:
				case "":
					AddComplianceSubTypeAttributionRulesForTaxDocumentsAAndExcludedSupply(collection);
					break;
				default:
					ErrorReporter.ReportOnce("NicaraguaComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
					break;
			}
		}

		void AddComplianceSubTypeAttributionRulesForTaxDocumentsAAndExcludedSupply(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Nicaragua;
			configuration.SubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Nicaragua;
			configuration.SubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Nicaragua;
			configuration.SubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Nicaragua;
			configuration.SubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Nicaragua;
			configuration.SubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Nicaragua;
			configuration.SubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Nicaragua;
			configuration.SubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = NicaraguaComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAAndExcludedSupply;
		}

		#region SuppressResourceStringsCheckRegion

		public static class RuleSetCodes
		{
			public const string TaxDocumentsAAndExcludedSupply = "1";
		}

		static class RuleSetDescriptions
		{
			public const string TaxDocumentsAAndExcludedSupply = "Tax Documents A and Excluded Supply";
		}

		#endregion

		#endregion
	}
}
