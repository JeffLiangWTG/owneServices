using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Countries.Colombia;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.ColombiaOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ColombiaComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRuleProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IComplianceRegistryDefaultProvider,
		IComplianceSequenceValidationProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Colombia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.NIT;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.NIT;
		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		protected override bool? HasExtraTaxInfo() => true;
		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSRET", "RET Amt", "RET Amount", "");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalRET", "RET Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSIVA", "IVA Amt", "IVA Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalIVA", "IVA Local");

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TDR,
				ComplianceSubTypes.DSO,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TCR = "TCR";
			public const string TXI = "TXI";
			public const string TDR = "TDR";
			public const string DSO = "DSO";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TCR => ResString.GetMultilingualString("COComplianceSubTypeCodeList|TCR", "Tax Credit Note");
			public static MultilingualString TXI => ResString.GetMultilingualString("COComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString TDR => ResString.GetMultilingualString("COComplianceSubTypeCodeList|TDR", "Tax Debit Note");
			public static MultilingualString DSO => ResString.GetMultilingualString("COComplianceSubTypeCodeList|DSO", "Other Documents");
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TCR = "Nota De Credito";
			public const string TXI = "Factura De Venta";
			public const string TDR = "Nota de Debito";
			public const string DSO = "Documento soporte en adquisiciones efectuadas a no obligados a facturar";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TCR = "Used in both Receivables and Payables to sub-classify Credit Note (CRD) transactions.";
			public const string TXI = "Used in both Receivables and Payables to sub-classify Invoice (INV) transactions.";
			public const string TDR = "Used in both Receivables and Payables to sub-classify Amending Invoice transactions.";
			public const string DSO = "Used in Payables to sub-classify transactions issued by the buyer when purchasing goods or services to a supplier that is not obliged to issue a sales invoice. The Documento soporte en adquisiciones efectuadas a no obligados a facturar will allow the buyer to recognise costs, deductions and taxes related to that purchase.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TDR => new ComplianceSubType(ComplianceSubTypeCodes.TDR, () => ComplianceSubTypeDescriptions.TDR, () => ComplianceSubTypeLocalDescriptions.TDR, () => ComplianceSubTypeInternalImplemenationNote.TDR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType DSO => new ComplianceSubType(ComplianceSubTypeCodes.DSO, () => ComplianceSubTypeDescriptions.DSO, () => ComplianceSubTypeLocalDescriptions.DSO, () => ComplianceSubTypeInternalImplemenationNote.DSO, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
		}

		#endregion

		#region IComplianceSubTypeRuleProvider

		void IComplianceSubTypeRuleProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			#region AR

			ComplianceSubTypeAttributionRuleConfiguration configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TDR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;

			#endregion

		}

		#endregion

		#region IComplianceInfoElectronicInvoicingElegibleSubType

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
			=> GetEInvoicingEligibleComplianceSubTypeList().Contains(complianceSubType);

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => GetEInvoicingEligibleComplianceSubTypeList();

		public static HashSet<string> GetEInvoicingEligibleComplianceSubTypeList()
		{
			return new HashSet<string>
			{
				ComplianceSubTypeCodes.TXI,
				ComplianceSubTypeCodes.TDR,
				ComplianceSubTypeCodes.TCR,
			};
		}

		#endregion

		#region IComplianceRegistryDefaultProvider

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry() => AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled)
			=> isEInvoicingEnabled ? AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post : null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => null;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Colombia;

		#endregion

		#region IComplianceSequencesValidationProvider

		bool IComplianceSequenceValidationProvider.IsPrintingAuthorizationNumberLengthValid(string number) => true;

		bool IComplianceSequenceValidationProvider.IsPrintingAuthorizationNumberFormatValid(string number) => true;

		AccComplianceSequenceValidation IComplianceSequenceValidationProvider.GetAccComplianceSequenceValidation(AccComplianceSequence sequence)
		{
			return new AccComplianceSequenceColombiaValidation(sequence);
		}

		#endregion
	}
}
