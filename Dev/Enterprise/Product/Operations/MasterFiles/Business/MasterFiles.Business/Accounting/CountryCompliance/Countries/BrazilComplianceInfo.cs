using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.BrazilOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class BrazilComplianceInfo : CountryComplianceInfo,
		IComplianceRegistryDefaultProvider,
		IComplianceSubTypeCodeProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Brazil;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.CMT;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.CMT;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.CNPJ;
		protected override bool? GetIsReciprocal() => true;

		protected override string GetRecipientLocalBusinessRegNumberCodeType() => BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalTaxPayerRegistration;
		protected override string GetRecipientLocalBusinessReg2NumberCodeType() => BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration;
		protected override string GetRecipientLocalBusinessRegHeading() => "IM";
		protected override string GetRecipientLocalBusinessReg2Heading() => "CPF";

		#endregion

		#region Registry Defaults

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry() => AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled) => null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> isEInvoicingEnabled
				? string.Empty
				: ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(proposedValue, CountryCode);

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> isEInvoicingEnabled
				? string.Empty
				: ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(proposedValue, CountryCode);

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => false;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => null;

		#endregion

		#region IComplianceSubTypeCodeProvider

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing()
			=> GetEInvoicingEligibleComplianceSubTypeList;

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
			=> GetEInvoicingEligibleComplianceSubTypeList.Contains(complianceSubType);

		static readonly ImmutableHashSet<string> GetEInvoicingEligibleComplianceSubTypeList = new[]
		{
			ComplianceSubTypeCodes.NFS,
			ComplianceSubTypeCodes.CNS,
		}.ToImmutableHashSet();

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => EnvProxy.Instance.IsProductionSystem ? ZDate.Empty : new ZDate(2021, 4, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Brazil;

		#endregion

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.NFS,
				ComplianceSubTypes.NFE,
				ComplianceSubTypes.CNS,
				ComplianceSubTypes.CNE,
				ComplianceSubTypes.XND,
				ComplianceSubTypes.XNC
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string NFS = "NFS";
			public const string NFE = "NFE";
			public const string CNS = "CNS";
			public const string CNE = "CNE";
			public const string XND = "XND";
			public const string XNC = "XNC";
		}

		internal static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString NFS => ResString.GetMultilingualString("BRComplianceSubTypeCodeList|NFS", "ELECTRONIC INVOICE FOR SERVICES");
			public static MultilingualString NFE => ResString.GetMultilingualString("BRComplianceSubTypeCodeList|NFE", "ELECTRONIC INVOICE FOR GOODS DELIVERY");
			public static MultilingualString CNS => ResString.GetMultilingualString("BRComplianceSubTypeCodeList|CNS", "CANCELLATION OF ELECTRONIC INVOICE FOR SERVICES");
			public static MultilingualString CNE => ResString.GetMultilingualString("BRComplianceSubTypeCodeList|CNE", "CANCELLATION OF ELECTRONIC INVOICE FOR GOODS DELIVERY");
			public static MultilingualString XND => ResString.GetMultilingualString("BRComplianceSubTypeCodeList|XND", "DEBIT NOTE");
			public static MultilingualString XNC => ResString.GetMultilingualString("BRComplianceSubTypeCodeList|XNC", "CREDIT NOTE");
		}

		static class ComplianceSubTypes
		{
			public static ComplianceSubType NFS => new ComplianceSubType(ComplianceSubTypeCodes.NFS, () => ComplianceSubTypeDescriptions.NFS, () => ComplianceSubTypeLocalDescriptions.NFS, () => ComplianceSubTypeInternalImplemenationNote.NFS, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType NFE => new ComplianceSubType(ComplianceSubTypeCodes.NFE, () => ComplianceSubTypeDescriptions.NFE, () => ComplianceSubTypeLocalDescriptions.NFE, () => ComplianceSubTypeInternalImplemenationNote.NFE, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType CNS => new ComplianceSubType(ComplianceSubTypeCodes.CNS, () => ComplianceSubTypeDescriptions.CNS, () => ComplianceSubTypeLocalDescriptions.CNS, () => ComplianceSubTypeInternalImplemenationNote.CNS, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType CNE => new ComplianceSubType(ComplianceSubTypeCodes.CNE, () => ComplianceSubTypeDescriptions.CNE, () => ComplianceSubTypeLocalDescriptions.CNE, () => ComplianceSubTypeInternalImplemenationNote.CNE, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType XND => new ComplianceSubType(ComplianceSubTypeCodes.XND, () => ComplianceSubTypeDescriptions.XND, () => ComplianceSubTypeLocalDescriptions.XND, () => ComplianceSubTypeInternalImplemenationNote.XND, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType XNC => new ComplianceSubType(ComplianceSubTypeCodes.XNC, () => ComplianceSubTypeDescriptions.XNC, () => ComplianceSubTypeLocalDescriptions.XNC, () => ComplianceSubTypeInternalImplemenationNote.XNC, transactionType: TransactionTypeOfUse.CRD);
		}

		#region SuppressResourceStringsCheckRegion

		internal static class ComplianceSubTypeLocalDescriptions
		{
			public const string NFS = "NOTA FISCAL DE SERVIÇOS ELETRÔNICA";
			public const string NFE = "NOTA FISCAL ELETRÔNICA";
			public const string CNS = "CANCELAMENTO DE NOTA FISCAL DE SERVIÇOS ELETRÔNICA";
			public const string CNE = "CANCELAMENTO DE NOTA FISCAL ELETRÔNICA";
			public const string XND = "NOTA DE DEBITO";
			public const string XNC = "NOTA DE CREDITO";
		}

		internal static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string NFS = "Used in both Receivables and Payables when recording NFS-e Nota Fiscal de Serviços Eletrònica invoices. This type of document is used to charge services listed in the 'Lei Complementar 113/06'.";
			public const string NFE = "Used in both Receivables and Payables when recording NF-e Nota Fiscal Eletrònica invoices. This type of document is used to invoice charges related to delivery of goods and reported to the local tax authority.";
			public const string CNS = "Used in both Receivables and Payables to sub-classify in CW reversions or cancellations of NFS Compliance Sub Types. This compliance sub type is internal for CW and does not exist as such for Brazil tax regime.";
			public const string CNE = "Used in both Receivables and Payables to sub-classify in CW reversions or cancellations of NFE Compliance Sub Types. This compliance sub type is internal for CW and does not exist as such for Brazil tax regime.";
			public const string XND = "Used in both Receivables and Payables to sub-classify INV transactions including reimbursements / disbursements and other charges that cannot be registered with electronic invoices.";
			public const string XNC = "Used in both Receivables and Payables to sub-classify CRD transactions including reimbursements / disbursements and other charges that cannot be registered with electronic invoices.";
		}

		#endregion

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.CancellationsOnly, RuleSetDescriptions.CancellationsOnly);
			rulesetList.AddPair(RuleSetCodes.NFSeAndCancellations, RuleSetDescriptions.NFSeAndCancellations);
			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
			=> RuleSetCodes.CancellationsOnly;

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.CancellationsOnly)
			{
				AddComplianceSubTypeAttributionRulesForCancellationsOnly(collection);
			}
			else if (ruleSetCode == RuleSetCodes.NFSeAndCancellations)
			{
				AddComplianceSubTypeAttributionRulesForNFSeAndCancellations(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode)) //this is valid for UT and CountryComplianceInfoDisplayForm
			{
				AddComplianceSubTypeAttributionRulesForCancellationsOnly(collection);
				AddComplianceSubTypeAttributionRulesForNFSeAndCancellations(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("BrazilComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code : " + ruleSetCode);
			}
		}

		const string ISS = nameof(ISS);

		void AddComplianceSubTypeAttributionRulesForCancellationsOnly(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//CNS
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.CancellationsOnly;
			configuration.RuleSetDescription = RuleSetDescriptions.CancellationsOnly;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			configuration.ExcludedTaxSystem = ISS;

			//CNS
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.CancellationsOnly;
			configuration.RuleSetDescription = RuleSetDescriptions.CancellationsOnly;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			configuration.RequiredTaxSystem = ISS;

			//XNC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.CancellationsOnly;
			configuration.RuleSetDescription = RuleSetDescriptions.CancellationsOnly;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			configuration.ExcludedTaxSystem = ISS;

			//XNC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.CancellationsOnly;
			configuration.RuleSetDescription = RuleSetDescriptions.CancellationsOnly;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			configuration.ExcludedTaxSystem = ISS;

			//XND
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.CancellationsOnly;
			configuration.RuleSetDescription = RuleSetDescriptions.CancellationsOnly;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			configuration.ExcludedTaxSystem = ISS;

			//XND
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.CancellationsOnly;
			configuration.RuleSetDescription = RuleSetDescriptions.CancellationsOnly;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			configuration.ExcludedTaxSystem = ISS;
		}

		void AddComplianceSubTypeAttributionRulesForNFSeAndCancellations(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//NFS
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.NFSeAndCancellations;
			configuration.RuleSetDescription = RuleSetDescriptions.NFSeAndCancellations;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RequiredTaxSystem = ISS;

			//XND
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.NFSeAndCancellations;
			configuration.RuleSetDescription = RuleSetDescriptions.NFSeAndCancellations;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ExcludedTaxSystem = ISS;

			//CNS
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.NFSeAndCancellations;
			configuration.RuleSetDescription = RuleSetDescriptions.NFSeAndCancellations;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			configuration.RequiredTaxSystem = ISS;

			//XNC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.NFSeAndCancellations;
			configuration.RuleSetDescription = RuleSetDescriptions.NFSeAndCancellations;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			configuration.ExcludedTaxSystem = ISS;

			//XNC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.NFSeAndCancellations;
			configuration.RuleSetDescription = RuleSetDescriptions.NFSeAndCancellations;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			configuration.ExcludedTaxSystem = ISS;

			//XND
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.NFSeAndCancellations;
			configuration.RuleSetDescription = RuleSetDescriptions.NFSeAndCancellations;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			configuration.ExcludedTaxSystem = ISS;

			//XND
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.NFSeAndCancellations;
			configuration.RuleSetDescription = RuleSetDescriptions.NFSeAndCancellations;
			configuration.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
			configuration.ExcludedTaxSystem = ISS;
		}

		public static class RuleSetCodes
		{
			public const string CancellationsOnly = "1";
			public const string NFSeAndCancellations = "2";
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Descriptions for compliance rule sets are not translated")]
		static class RuleSetDescriptions
		{
			public const string CancellationsOnly = "Cancellations Only";
			public const string NFSeAndCancellations = "NFSe and Cancellations";
		}

		#endregion
	}
}
