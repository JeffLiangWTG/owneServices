using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.UruguayOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class UruguayComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IComplianceSubTypeAllocationOverrideConfigurationProvider,
		IComplianceRegistryDefaultProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Uruguay;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.RUT;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.RUT;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TKT,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TKD,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TKC,
				ComplianceSubTypes.YXI,
				ComplianceSubTypes.YKT,
				ComplianceSubTypes.YCD,
				ComplianceSubTypes.YKD,
				ComplianceSubTypes.YCR,
				ComplianceSubTypes.YKR,
				ComplianceSubTypes.XCL,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TKT = "TKT";
			public const string TCD = "TCD";
			public const string TKD = "TKD";
			public const string TCR = "TCR";
			public const string TKC = "TKC";
			public const string YXI = "YXI";
			public const string YKT = "YKT";
			public const string YCD = "YCD";
			public const string YKD = "YKD";
			public const string YCR = "YCR";
			public const string YKR = "YKR";
			public const string XCL = "XCL";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString TKT => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|TKT", "Ticket (Simplified Invoice)");
			public static MultilingualString TCD => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|TCD", "Tax Debit Note");
			public static MultilingualString TKD => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|TKD", "Ticket Debit Note");
			public static MultilingualString TCR => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|TCR", "Tax Credit Note");
			public static MultilingualString TKC => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|TKC", "Ticket Credit Note");
			public static MultilingualString YXI => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|YXI", "Contingency Tax Invoice");
			public static MultilingualString YKT => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|YKT", "Contingency Ticket (Simplified Invoice)");
			public static MultilingualString YCD => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|YCD", "Contingency Tax Debit Note");
			public static MultilingualString YKD => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|YKD", "Contingency Ticket Debit Note");
			public static MultilingualString YCR => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|YCR", "Contingency Tax Credit Note");
			public static MultilingualString YKR => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|YKR", "Contingency Ticket Credit Note");
			public static MultilingualString XCL => ResString.GetMultilingualString("UYComplianceSubTypeCodeList|XCL", "Reimbursement / Disbursement / Excluded Supply");
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "e-Factura";
			public const string TKT = "e-Ticket";
			public const string TCD = "Nota de Débito de e-Factura";
			public const string TKD = "Nota de Débito de e-Ticket";
			public const string TCR = "Nota de Crédito de e-Factura";
			public const string TKC = "Nota de Crédito de e-Ticket";
			public const string YXI = "e-Factura Contingencia";
			public const string YKT = "e-Ticket Contingencia";
			public const string YCD = "Nota de Débito de e-Factura Contingencia";
			public const string YKD = "Nota de Débito de e-Ticket Contingencia";
			public const string YCR = "Nota de Crédito de e-Factura Contingencia";
			public const string YKR = "Nota de Crédito de e-Ticket Contingencia";
			public const string XCL = "DOCUMENTO DE REEMBOLSO";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no traslate")]
		static class ComplianceSubTypeInternalImplementationNote
		{
			public const string TXI = "Used in Receivables when invoicing a UY IVA registered business. Used in Payables to record Invoices received from any Payables organisation whether UY IVA registered or not (For Example, it can be used for both, local and foreign Invoices).";
			public const string TKT = "Used in Receivables when invoicing an organsiation that is NOT UY IVA registered.";
			public const string TCD = "Used in Receivables when recording an amending/reversing invoice for a UY IVA registered business. Identifies that a Debit Note related to an e-Factura fiscal document was issued against the Receivables Invoice (INV) transaction. Used in Payables to record Debit Notes received from any Payables organisation whether UY IVA registered or not (For Example, it can be used for both, local and foreign Debit Notes).";
			public const string TKD = "Used in Receivables when recording an amending/reversing invoice for organisations that are NOT UY IVA registered. Identifies that a Debit Note related to an e-Ticket fiscal document was issued against the Receivables Invoice (INV) transaction. ";
			public const string TCR = "Used in Receivables when recording an amending/reversing Credit Note for a UY IVA registered business. Identifies that a Credit Note related to an e-Factura fiscal document was issued against the Receivables Invoice (INV) transaction. Used in Payables to record Credit Notes received from any Payables organisation whether UY IVA registered or not (For Example, it can be used for both, local and foreign Credit Notes).";
			public const string TKC = "Used in Receivables when recording an amending/reversing Credit Note for organisations that are NOT UY IVA registered. Identifies that a Credit Note related to an e-Ticket fiscal document was issued against the Receivables Invoice (INV) transaction.";
			public const string YXI = "The contingency version of TXI e-Factura. Used in Receivables and Payables when contingency paper fiscal documents are issued.";
			public const string YKT = "The contingency version of TKT e-Ticket. Used in Receivables when contingency paper fiscal documents are issued.";
			public const string YCD = "The contingency version of TCD Nota de Débitoo e-Factura. Used in Receivables and Payables  when contingency paper fiscal documents are issued.";
			public const string YKD = "The contingency version of TKD Nota de Débito e-Ticket. Used in Receivables when contingency paper fiscal documents are issued.";
			public const string YCR = "The contingency version of TCR Nota de Crédito e-Factura. Used in Receivables and Payables when contingency paper fiscal documents are issued.";
			public const string YKR = "The contingency version of TKC Nota de Crédito e-Ticket. Used in Receivables when contingency paper fiscal documents are issued.";
			public const string XCL = "Used to formally identify Reimbursement / Disbursement / Excluded Supply transactions. Used in conjuction with the Exclude Tax ID. This type is used to identify transctions where no reportable supply was made or received and no fiscal document is required.";
		}

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplementationNote.TXI, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TKT => new ComplianceSubType(ComplianceSubTypeCodes.TKT, () => ComplianceSubTypeDescriptions.TKT, () => ComplianceSubTypeLocalDescriptions.TKT, () => ComplianceSubTypeInternalImplementationNote.TKT, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplementationNote.TCD, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TKD => new ComplianceSubType(ComplianceSubTypeCodes.TKD, () => ComplianceSubTypeDescriptions.TKD, () => ComplianceSubTypeLocalDescriptions.TKD, () => ComplianceSubTypeInternalImplementationNote.TKD, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplementationNote.TCR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TKC => new ComplianceSubType(ComplianceSubTypeCodes.TKC, () => ComplianceSubTypeDescriptions.TKC, () => ComplianceSubTypeLocalDescriptions.TKC, () => ComplianceSubTypeInternalImplementationNote.TKC, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType YXI => new ComplianceSubType(ComplianceSubTypeCodes.YXI, () => ComplianceSubTypeDescriptions.YXI, () => ComplianceSubTypeLocalDescriptions.YXI, () => ComplianceSubTypeInternalImplementationNote.YXI, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType YKT => new ComplianceSubType(ComplianceSubTypeCodes.YKT, () => ComplianceSubTypeDescriptions.YKT, () => ComplianceSubTypeLocalDescriptions.YKT, () => ComplianceSubTypeInternalImplementationNote.YKT, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType YCD => new ComplianceSubType(ComplianceSubTypeCodes.YCD, () => ComplianceSubTypeDescriptions.YCD, () => ComplianceSubTypeLocalDescriptions.YCD, () => ComplianceSubTypeInternalImplementationNote.YCD, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType YKD => new ComplianceSubType(ComplianceSubTypeCodes.YKD, () => ComplianceSubTypeDescriptions.YKD, () => ComplianceSubTypeLocalDescriptions.YKD, () => ComplianceSubTypeInternalImplementationNote.YKD, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType YCR => new ComplianceSubType(ComplianceSubTypeCodes.YCR, () => ComplianceSubTypeDescriptions.YCR, () => ComplianceSubTypeLocalDescriptions.YCR, () => ComplianceSubTypeInternalImplementationNote.YCR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType YKR => new ComplianceSubType(ComplianceSubTypeCodes.YKR, () => ComplianceSubTypeDescriptions.YKR, () => ComplianceSubTypeLocalDescriptions.YKR, () => ComplianceSubTypeInternalImplementationNote.YKR, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplementationNote.XCL);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.TipoCFE)
			{
				AddComplianceSubTypeAttributionRulesForTipoCFE(collection);
			}
			else if (ruleSetCode == RuleSetCodes.TipoCFC)
			{
				AddComplianceSubTypeAttributionRulesForTipoCFC(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode)) //this is valid for UT and CountryComplianceInfoDisplayForm
			{
				AddComplianceSubTypeAttributionRulesForTipoCFE(collection);
				AddComplianceSubTypeAttributionRulesForTipoCFC(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("UruguayComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
			}
		}

		void AddComplianceSubTypeAttributionRulesForTipoCFE(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			#region Receivables

			var configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TKT;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TKD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TKT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TKD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TKC;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TKC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TKC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TKD;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TKC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TKT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			#endregion

			#region Payables

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFE;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFE;

			#endregion
		}

		void AddComplianceSubTypeAttributionRulesForTipoCFC(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			#region Receivables

			var configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.YXI;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.YCR;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.YXI;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.YCD;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Uruguay;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YKT;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YKD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.YKT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YKD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.YKR;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YKR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YKR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.YKT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.YKR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.YKD;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			#endregion

			#region Payables

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Uruguay;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.TipoCFC;
			configuration.RuleSetDescription = RuleSetDescriptions.TipoCFC;

			#endregion
		}

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TipoCFE, RuleSetDescriptions.TipoCFE);
			rulesetList.AddPair(RuleSetCodes.TipoCFC, RuleSetDescriptions.TipoCFC);

			return rulesetList;
		}

		public static class RuleSetCodes
		{
			public const string TipoCFE = "1";
			public const string TipoCFC = "2";
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "An explanation that must be at least 15 characters long")]
		static class RuleSetDescriptions
		{
			public const string TipoCFE = "CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents)";
			public const string TipoCFC = "CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents)";
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.TipoCFE;
		}

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => EnvProxy.Instance.IsProductionSystem ? new ZDate(2022, 8, 1) : new ZDate(2022, 7, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		public string GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Uruguay;

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
		{
			return GetEInvoicingEligibleComplianceSubTypeList().Contains(complianceSubType);
		}

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => GetEInvoicingEligibleComplianceSubTypeList();

		public static HashSet<string> GetEInvoicingEligibleComplianceSubTypeList()
		{
			return new HashSet<string>
			{
				ComplianceSubTypeCodes.TXI,
				ComplianceSubTypeCodes.TKT,
				ComplianceSubTypeCodes.TCD,
				ComplianceSubTypeCodes.TKD,
				ComplianceSubTypeCodes.TCR,
				ComplianceSubTypeCodes.TKC,
				ComplianceSubTypeCodes.YXI,
				ComplianceSubTypeCodes.YKT,
				ComplianceSubTypeCodes.YCD,
				ComplianceSubTypeCodes.YKD,
				ComplianceSubTypeCodes.YCR,
				ComplianceSubTypeCodes.YKR,
			};
		}

		#endregion

		#region IComplianceSubTypeAllocationOverrideConfigurationProvider

		void IComplianceSubTypeAllocationOverrideConfigurationProvider.GetDefaults(ComplianceSubTypeAllocationOverrideConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YXI;
			configuration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCD;
			configuration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YCR;
			configuration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKT;
			configuration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKD;
			configuration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKR;
			configuration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
		}

		#endregion IComplianceSubTypeAllocationOverrideConfigurationProvider

		#region IComplianceRegistryDefaultProvider

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry() => AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled)
			=> isEInvoicingEnabled
				? AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate
				: AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> isEInvoicingEnabled
				? ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotChangeDefaultValue(proposedValue, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, CountryCode)
				: ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(proposedValue, CountryCode, alternateErrorMessage: ErrorMessageForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(proposedValue));

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> isEInvoicingEnabled
				? string.Empty
				: ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(proposedValue, CountryCode, alternateErrorMessage: ErrorMessageForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(proposedValue));

		static string ErrorMessageForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue)
			=> Res.GetString("2DF73A95-B513-491B-8523-09D027DBC9BE", "'{0}' is not valid for country/region '{1}' when registry '{2}' is not enabled.", proposedValue, Core.Constants.CountryCodes.Uruguay, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Caption);

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => null;

		#endregion
	}
}
