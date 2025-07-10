using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class VietnamComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IComplianceInfoEInvoicingGUIActionProvider,
		IComplianceInfoEInvoicingGUIActionDocumentRequest,
		IComplianceSubTypeAndNumberUpdateRules,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IEInvoicingRegistryProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.VietNam;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList { ComplianceSubTypes.EXI, ComplianceSubTypes.TXI };
		}

		public static class ComplianceSubTypeCodes
		{
			public const string EXI = "EXI";
			public const string TXI = "TXI";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString EXI { get { return ResString.GetMultilingualString("VNComplianceSubTypeCodeList|EXI", "VN Export Invoice"); } }
			public static MultilingualString TXI { get { return ResString.GetMultilingualString("VNComplianceSubTypeCodeList|TXI", "VN Govt Tax Invoice"); } }
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string EXI = "HÓA ĐƠN XUẤT KHẨU";
			public const string TXI = "HÓA ĐƠN GIÁ TRỊ GIA TĂNG";
		}

		static class ComplianceSubTypeInternalImplementationNote
		{
			public const string EXI = "Used in both Receivables and Payables to document transctions that required an \"Export\" Invoice Fiscal document.";
			public const string TXI = "Used in both Receivables and Payables to sub-classify Original Invoice (INV) transactions.";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType EXI => new ComplianceSubType(ComplianceSubTypeCodes.EXI, () => ComplianceSubTypeDescriptions.EXI, () => ComplianceSubTypeLocalDescriptions.EXI, () => ComplianceSubTypeInternalImplementationNote.EXI);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplementationNote.TXI);
		}

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => string.Empty;

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
		{
			return GetEInvoicingEligibleComplianceSubTypeList().Contains(complianceSubType);
		}

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => GetEInvoicingEligibleComplianceSubTypeList();

		public static HashSet<string> GetEInvoicingEligibleComplianceSubTypeList()
		{
			return new HashSet<string> { ComplianceSubTypeCodes.TXI, ComplianceSubTypeCodes.EXI };
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProvider

		public bool IsCountryEnableComplianceEInvoicing(bool isAPTransaction = false) => !isAPTransaction && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		public string NotEligibleForRequestsMessage => Res.GetString("96d0a078-a320-4111-94b3-503903223c47", @"The transaction is not eligible for requests due to one of the following conditions:
1. The E-Reporting Status of the transaction is not equal to SUC.
2. The transaction has not been approved.");

		public IEnumerable<AccTransactionHeader> GetEligibleInvoices(IEnumerable<AccTransactionHeader> selectedTransactions)
		{
			return selectedTransactions.Where(x => ((!x.AH_IsCancelled && x.AH_TransactionType == TransactionTypes.Invoice)
													&& GetEInvoicingEligibleComplianceSubTypeList().Contains(x.AH_ComplianceSubType))
													|| x.AH_TransactionType == TransactionTypes.CreditNote).ToList();
		}

		public bool ExistActiveDocumentRequestPivot(ZDateTime lastSentTime)
		{
			return lastSentTime > ZDateTime.UtcNow.AddHours(-1);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionDocumentRequest

		public ZString DocumentRequestMenuName => Res.GetString("40e438e6-86d5-4a89-a4d1-6dd0be7f78f1", "Request e-Invoice Copy");

		public ZString DocumentRequestActionInformation => Res.GetString("3eb47aa1-4b57-4a63-ad05-078942b85099", @"Your request for a copy of the tax invoice is being processed.
Please Note:
- A request for a copy of the tax invoice can only be made for new INV, INV amendment with CRD, canceled INV.
- A new request for a copy of the tax invoice is allowed where response from an existing request has not been received after 60 minutes from request submission.");

		#endregion

		#region IComplianceSubTypeAndNumberUpdateRules

		bool IComplianceSubTypeAndNumberUpdateRules.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed => true;

		ZString IComplianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(AccTransactionHeader[] transactionHeaders) => ZString.Empty;

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(AccTransactionHeader transactionHeader) => true;

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceDocDateAllowed(AccTransactionHeader transactionHeader) => false;

		public static class RuleSetCodes
		{
			public const string TXIExcludingNotReportableCharges = "1";
			public const string TXIExcludingNotReportableAndExcludedFromTaxBaseCharges = "2";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		static class RuleSetDescriptions
		{
			public const string TXIExcludingNotReportableCharges = "TXI (excluding Not Reportable charges)";
			public const string TXIExcludingNotReportableAndExcludedFromTaxBaseCharges = "TXI (excluding Not Reportable and Excluded from Tax Base charges)";
		}

		public CodeDescriptionPairList GetRuleSet()
		{
			var codeSetPairList = new CodeDescriptionPairList();
			codeSetPairList.AddPair(RuleSetCodes.TXIExcludingNotReportableCharges, RuleSetDescriptions.TXIExcludingNotReportableCharges);
			codeSetPairList.AddPair(RuleSetCodes.TXIExcludingNotReportableAndExcludedFromTaxBaseCharges, RuleSetDescriptions.TXIExcludingNotReportableAndExcludedFromTaxBaseCharges);
			return codeSetPairList;
		}

		public string GetDefaultRuleSet()
		{
			return RuleSetCodes.TXIExcludingNotReportableCharges;
		}

		public void SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			switch (ruleSetCode)
			{
				case RuleSetCodes.TXIExcludingNotReportableCharges:
					AddComplianceSubTypeAttributionRuleConfigurationsForTXIExcludingNotReportableCharges(collection);
					break;
				case RuleSetCodes.TXIExcludingNotReportableAndExcludedFromTaxBaseCharges:
					AddComplianceSubTypeAttributionRuleConfigurationsForTXIExcludingNotReportableAndExcludedFromTaxBaseCharges(collection);
					break;
				case null:
				case "":
					AddComplianceSubTypeAttributionRuleConfigurationsForTXIExcludingNotReportableCharges(collection);
					AddComplianceSubTypeAttributionRuleConfigurationsForTXIExcludingNotReportableAndExcludedFromTaxBaseCharges(collection);
					break;
				default:
					ErrorReporter.ReportOnce("VietnamComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
					break;
			}
		}

		void AddComplianceSubTypeAttributionRuleConfigurationsForTXIExcludingNotReportableCharges(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.VietNam;
			configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.VietNam;
			configuration.RuleSetCode = RuleSetCodes.TXIExcludingNotReportableCharges;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.VietNam;
			configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXIExcludingNotReportableCharges;
		}

		void AddComplianceSubTypeAttributionRuleConfigurationsForTXIExcludingNotReportableAndExcludedFromTaxBaseCharges(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.VietNam;
			configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = Constants.CountryCodes.VietNam;
			configuration.RuleSetCode = RuleSetCodes.TXIExcludingNotReportableAndExcludedFromTaxBaseCharges;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.VietNam;
			configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXIExcludingNotReportableAndExcludedFromTaxBaseCharges;
		}

		#endregion

		#region IEInvoicingRegistryProvider

		bool IEInvoicingRegistryProvider.ShouldAutoSetEReportingComplianceDate => true;

		#endregion
	}
}
