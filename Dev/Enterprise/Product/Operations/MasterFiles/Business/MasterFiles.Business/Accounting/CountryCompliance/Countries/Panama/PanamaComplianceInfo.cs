using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static Enterprise.MasterFiles.Business.PanamaOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class PanamaComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		ITaxMessagesGroupProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceRegistryDefaultProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Panama;

		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.RUC;

		protected override string GetLocalBusinessRegNoCodeType() => GetConsumptionTaxRegistrationCode();

		protected override string GetConsumptionTaxCode() => "ITBMS"; // TODO: Investigate this code, and how it integrates with OrgCusCodes. It has a 5 character long name that breaks assumptions.

		protected override bool? GetIsReciprocal() => false;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.XCL
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TCR = "TCR";
			public const string TCD = "TCD";
			public const string XCL = "XCL";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI { get { return ResString.GetMultilingualString("PAComplianceSubTypeCodeList|TXI", "Invoice"); } }
			public static MultilingualString TCR { get { return ResString.GetMultilingualString("PAComplianceSubTypeCodeList|TCR", "Credit Note"); } }
			public static MultilingualString TCD { get { return ResString.GetMultilingualString("PAComplianceSubTypeCodeList|TCD", "Debit Note"); } }
			public static MultilingualString XCL { get { return ResString.GetMultilingualString("PAComplianceSubTypeCodeList|XCL", "Reimbursement / Disbursement / Excluded Supply"); } }
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "Factura";
			public const string TCR = "Nota de Crédito";
			public const string TCD = "Nota de Débito";
			public const string XCL = "Documento de Reembolso";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TCD = "Used in both Receivables and Payables to sub-classify Amending/Reversing Invoice (INV) transactions.";
			public const string TCR = "Used in both Receivables and Payables to sub-classify Amending/reversing Credit Note (CRD) transactions.";
			public const string TXI = "Used in both Receivables and Payables to sub-classify Original Invoice (INV) transactions.";
			public const string XCL = "Used to formally identify Reimbursement / Disbursement / Excluded Supply transactions. Used in conjuction with the 'Exclude' Tax ID. This type is used to identify transctions where no reportable supply was made or received and no fiscal document is required.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
		}

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		public string GetGovernmentAllocatedNumberColumnName() => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Panama;

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
		{
			return GetEInvoicingEligibleComplianceSubTypeList().Contains(complianceSubType.ToString());
		}

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => GetEInvoicingEligibleComplianceSubTypeList();

		static string[] GetEInvoicingEligibleComplianceSubTypeList()
		{
			return new string[]
			{
				ComplianceSubTypeCodes.TXI,
				ComplianceSubTypeCodes.TCR,
				ComplianceSubTypeCodes.TCD,
				ComplianceSubTypeCodes.XCL,
			};
		}

		#endregion

		#region ITaxMessagesGroupProvider

		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N1, Description = TaxMessageGroupDescriptions.N1, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N1 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N2, Description = TaxMessageGroupDescriptions.N2, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N2 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N3, Description = TaxMessageGroupDescriptions.N3, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N3 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N4, Description = TaxMessageGroupDescriptions.N4, Bool = false, RelatedItemCode = TaxMessageGroupGovtCodes.N4 },
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string N1 = "I0";
			public const string N2 = "I7";
			public const string N3 = "I10";
			public const string N4 = "I15";
		}

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString N1 => (NoResString)"ITBMS Exento";
			public static MultilingualString N2 => (NoResString)"ITBMS 7%";
			public static MultilingualString N3 => (NoResString)"ITBMS 10%";
			public static MultilingualString N4 => (NoResString)"ITBMS 15%";
		}

		public static class TaxMessageGroupGovtCodes
		{
			public const string N1 = "00";
			public const string N2 = "01";
			public const string N3 = "02";
			public const string N4 = "03";
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TXITCRTCDAndXCL, RuleSetDescriptions.TXITCRTCDAndXCL);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
			=> RuleSetCodes.TXITCRTCDAndXCL;

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			var configuration = collection.AddNew();
			//TXI
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//TCR
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//TCR
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//TCR
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//TCD
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//TCD
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = Constants.CountryCodes.Panama;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXITCRTCDAndXCL;
			configuration.RuleSetDescription = RuleSetDescriptions.TXITCRTCDAndXCL;
		}

		public static class RuleSetCodes
		{
			public const string TXITCRTCDAndXCL = "1";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		static class RuleSetDescriptions
		{
			public const string TXITCRTCDAndXCL = "TXI, TCR, TCD & XCL";
		}

		#endregion

		#region IComplianceRegistryDefaultProvider

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry() => ComplianceNumberAllocationDateOptions.NoControl.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled) => isEInvoicingEnabled ? AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post : null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => null;

		#endregion
	}
}
