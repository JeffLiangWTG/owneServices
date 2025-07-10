using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CostaRicaComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.CostaRica;
		protected override string GetConsumptionTaxRegistrationCode() => CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber;
		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;
		protected override bool? HasExtraTaxInfo() => true;
		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|CR|OSSPV", "Exon. Amt", "Exon. Amount", "");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|CR|LocalSPV", "Exon. Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSIVA", "IVA Amt", "IVA Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalIVA", "IVA Local");

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.XCL,
				ComplianceSubTypes.TXE,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TCD = "TCD";
			public const string TCR = "TCR";
			public const string TXI = "TXI";
			public const string XCL = "XCL";
			public const string TXE = "TXE";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TCD => ResString.GetMultilingualString("CRComplianceSubTypeCodeList|TCD", "Debit Note");
			public static MultilingualString TCR => ResString.GetMultilingualString("CRComplianceSubTypeCodeList|TCR", "Tax Credit Note");
			public static MultilingualString TXI => ResString.GetMultilingualString("CRComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString XCL => ResString.GetMultilingualString("CRComplianceSubTypeCodeList|XCL", "Reimbursement/Disbursement/Excluded Supply");
			public static MultilingualString TXE => ResString.GetMultilingualString("CRComplianceSubTypeCodeList|TXE", "Export Invoice");
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TCD = "Nota de Débito";
			public const string TCR = "Nota de Crédito";
			public const string TXI = "Factura";
			public const string XCL = "Documento de Reembolso";
			public const string TXE = "Factura de Exportación";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TCD = "Used in both Receivables and Payables to sub-classify Amending/reversing Invoice (INV) transactions.";
			public const string TCR = "Used in both Receivables and Payables to sub-classify Amending/reversing Credit Note (CRD) transactions.";
			public const string TXI = "Used in both Receivables and Payables to sub-classify Original Invoice (INV) transactions.";
			public const string XCL = "Used to formally identify Reimbursement / Disbursement / Excluded Supply transactions. Used in conjunction with the 'Exclude' Tax ID. This type is used to identify transactions where no reportable supply was made or received and no fiscal document is required.";
			public const string TXE = "Used in Receivables to sub-classify Original Export Invoice (INV) transactions.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
			public static ComplianceSubType TXE => new ComplianceSubType(ComplianceSubTypeCodes.TXE, () => ComplianceSubTypeDescriptions.TXE, () => ComplianceSubTypeLocalDescriptions.TXE, () => ComplianceSubTypeInternalImplemenationNote.TXE, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet() => RuleSetCodes.TXI_TXE_TCR_TCD_XCL;

		public static class RuleSetCodes
		{
			public const string TXI_TXE_TCR_TCD_XCL = "1";
		}

		#region SuppressResourceStringsCheckRegion

		static class RuleSetDescriptions
		{
			public const string TXI_TXE_TCR_TCD_XCL = "TXI, TXE, TCR, TCD & XCL";
		}

		#endregion

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			var configuration = collection.AddNew();

			//TCD
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = Constants.CountryCodes.CostaRica;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//TCD
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = Constants.CountryCodes.CostaRica;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//TCR
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = Constants.CountryCodes.CostaRica;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//TCR
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = Constants.CountryCodes.CostaRica;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//TXI
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = Constants.CountryCodes.CostaRica;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//TXE
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TXE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//TCD
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXE;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//TCD
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//TCR
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//TCR
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXE;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.CostaRica;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.RuleSetCode = RuleSetCodes.TXI_TXE_TCR_TCD_XCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL;
		}

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TXI_TXE_TCR_TCD_XCL, RuleSetDescriptions.TXI_TXE_TCR_TCD_XCL);

			return rulesetList;
		}

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => string.Empty;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.CostaRica;

		#endregion IComplianceInfoElectronicInvoicing

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
			=> GetEInvoicingEligibleComplianceSubTypeList().Contains(complianceSubType);

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing()
			=> GetEInvoicingEligibleComplianceSubTypeList();

		public static HashSet<string> GetEInvoicingEligibleComplianceSubTypeList()
			=> [
					ComplianceSubTypeCodes.TXI,
					ComplianceSubTypeCodes.TXE,
					ComplianceSubTypeCodes.TCR,
					ComplianceSubTypeCodes.TCD
				];

		#endregion IComplianceInfoElectronicInvoicingEligibleSubType
	}
}
