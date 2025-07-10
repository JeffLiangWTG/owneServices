using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.ElSalvadorOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ElSalvadorComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeTaxRegistrationTypeRuleProvider,
		IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.ElSalvador;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.NRC;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.NRC;
		protected override bool? GetIsReciprocal() => false;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override bool? HasExtraTaxInfo() => true;
		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSRET", "RET Amt", "RET Amount", "");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalRET", "RET Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSIVA", "IVA Amt", "IVA Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalIVA", "IVA Local");

		#endregion

		#region ComplianceSubType

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TXE,
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TXN,
				ComplianceSubTypes.TCF
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TCD = "TCD";
			public const string TCR = "TCR";
			public const string TXE = "TXE";
			public const string TXI = "TXI";
			public const string TXN = "TXN";
			public const string TCF = "TCF";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TCD => ResString.GetMultilingualString("SVComplianceSubTypeCodeList|TCD", "Tax Debit Note");
			public static MultilingualString TCR => ResString.GetMultilingualString("SVComplianceSubTypeCodeList|TCR", "Tax Credit Note");
			public static MultilingualString TXE => ResString.GetMultilingualString("SVComplianceSubTypeCodeList|TXE", "Tax Invoice - Export");
			public static MultilingualString TXI => ResString.GetMultilingualString("SVComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString TXN => ResString.GetMultilingualString("SVComplianceSubTypeCodeList|TXN", "Tax Invoice - Final Consumer");
			public static MultilingualString TCF => ResString.GetMultilingualString("SVComplianceSubTypeCodeList|TCF", "Non Tax Credit Note");
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TCD = "Nota de Debito";
			public const string TCR = "Nota de Credito";
			public const string TXE = "Factura de Exportacion";
			public const string TXI = "Comprobante de Credito Fiscal";
			public const string TXN = "Factura";
			public const string TCF = "Nota de Credito sin Credito Fiscal";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TCD = "Used in Receivables and Payables to sub-classify Amending/Reversing Invoice (INV) transactions where the recipient has an NRC – Número de Registro de Contribuyente (VAT Registration).";
			public const string TCR = "Used in Receivables and Payables to sub-classify Original/Amending/Reversing Credit Note (CRD) transactions where the recipient has an NRC – Número de Registro de Contribuyente (VAT Registration).";
			public const string TXE = "Used in Receivables and Payables to sub-classify Original Invoice (INV) transactions, where invoice recipient does not have an NRC – Número de Registro de Contribuyente (VAT Registration) and is the holder of a current Export Exemption Certificate.";
			public const string TXI = "Used in Receivables and Payables to sub-classify Original Invoice (INV) transactions where the recipient has an NRC – Número de Registro de Contribuyente (VAT Registration).";
			public const string TXN = "Used in Receivables and Payables to sub-classify Original Invoice (INV) transactions, where invoice recipient does not have NRC – Número de Registro de Contribuyente (VAT Registration).";
			public const string TCF = "Used in Receivables and Payables to sub-classify Reversing Credit Note (CRD) transactions where the recipient has no NRC - Número de Registro de Contribuyente (VAT Registration).";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TXE => new ComplianceSubType(ComplianceSubTypeCodes.TXE, () => ComplianceSubTypeDescriptions.TXE, () => ComplianceSubTypeLocalDescriptions.TXE, () => ComplianceSubTypeInternalImplemenationNote.TXE, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXN => new ComplianceSubType(ComplianceSubTypeCodes.TXN, () => ComplianceSubTypeDescriptions.TXN, () => ComplianceSubTypeLocalDescriptions.TXN, () => ComplianceSubTypeInternalImplemenationNote.TXN, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCF => new ComplianceSubType(ComplianceSubTypeCodes.TCF, () => ComplianceSubTypeDescriptions.TCF, () => ComplianceSubTypeLocalDescriptions.TCF, () => ComplianceSubTypeInternalImplemenationNote.TCF, transactionType: TransactionTypeOfUse.CRD);
		}

		CodeDescriptionPairList IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider.GetTaxRegistrationTypeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(TaxRegistrationTypeCodes.OrgRegisteredForTaxInElSalvador, TaxRegistrationTypeDescriptions.OrgRegisteredForTax);
			return list;
		}

		bool IComplianceSubTypeTaxRegistrationTypeRuleProvider.IsTaxRegistrationTypeRuleApplicable(IAccComplianceRule rule, OrgHeader header)
		{
			return GlbCompany.CurrentCompany.Country.Code != CountryCode
				|| rule.TaxRegistrationType.IsEmpty
				|| (rule.TaxRegistrationType == TaxRegistrationTypeCodes.OrgRegisteredForTaxInElSalvador && header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.NRC, CountryCode) != null);
		}

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.AllSubTypes, RuleSetDescriptions.AllSubTypes);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
			=> RuleSetCodes.AllSubTypes;

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.OrgRegisteredForTaxInElSalvador;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.AllSubTypes;
			configuration.RuleSetDescription = RuleSetDescriptions.AllSubTypes;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.OrgRegisteredForTaxInElSalvador;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.AllSubTypes;
			configuration.RuleSetDescription = RuleSetDescriptions.AllSubTypes;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.OrgRegisteredForTaxInElSalvador;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.AllSubTypes;
			configuration.RuleSetDescription = RuleSetDescriptions.AllSubTypes;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.OrgRegisteredForTaxInElSalvador;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.AllSubTypes;
			configuration.RuleSetDescription = RuleSetDescriptions.AllSubTypes;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TXN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.AllSubTypes;
			configuration.RuleSetDescription = RuleSetDescriptions.AllSubTypes;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TXE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.AllSubTypes;
			configuration.RuleSetDescription = RuleSetDescriptions.AllSubTypes;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCF;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXN;
			configuration.RuleSetCode = RuleSetCodes.AllSubTypes;
			configuration.RuleSetDescription = RuleSetDescriptions.AllSubTypes;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCF;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXE;
			configuration.RuleSetCode = RuleSetCodes.AllSubTypes;
			configuration.RuleSetDescription = RuleSetDescriptions.AllSubTypes;
		}

		#endregion

		public static class RuleSetCodes
		{
			public const string AllSubTypes = "1";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sub types string")]
		static class RuleSetDescriptions
		{
			public const string AllSubTypes = "TXI, TCD, TCR, TXN, TXE & TCF";
		}

		#endregion
	}
}
