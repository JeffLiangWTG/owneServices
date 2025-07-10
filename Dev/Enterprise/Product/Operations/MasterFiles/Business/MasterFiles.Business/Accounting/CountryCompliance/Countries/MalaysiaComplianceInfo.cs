using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Interfaces.ComplianceSubTypes;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.MalaysiaOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class MalaysiaComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoEInvoicingGUIActionDocumentRequest,
		IComplianceInfoEInvoicingGUIActionProvider,
		IComplianceInfoEInvoicingGUIActionStatusRequest,
		IComplianceSubTypeValidation,
		IComplianceSubTypeGUIProvider,
		IEInvoicingRegistryProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Malaysia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.SER;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.SER;
		protected override string GetLocalBusinessRegNoCodeType() => MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccTransactionHeaderSchema.Constants.AH_GovernmentAllocatedID;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Malaysia;

		#endregion

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList { ComplianceSubTypes.SubType_01, ComplianceSubTypes.SubType_02, ComplianceSubTypes.SubType_03, ComplianceSubTypes.SubType_11, ComplianceSubTypes.SubType_12 };
		}

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.DefaultMalaysiaRuleSet, RuleSetDescriptions.DefaultMalaysiaRuleSet);
			rulesetList.AddPair(RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion, RuleSetDescriptions.MalaysiaRuleSet2IncludingNotReportableTransacion);
			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet() => RuleSetCodes.DefaultMalaysiaRuleSet;

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.DefaultMalaysiaRuleSet)
			{
				AddComplianceSubTypeAttributionRulesForDefaultMalaysiaRuleSet(collection);
			}
			else if (ruleSetCode == RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion)
			{
				AddComplianceSubTypeAttributionRulesForMalaysiaRuleSet2IncludingNotReportableTransacionRuleSet(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode)) //this is valid for UT and CountryComplianceInfoDisplayForm
			{
				AddComplianceSubTypeAttributionRulesForDefaultMalaysiaRuleSet(collection);
				AddComplianceSubTypeAttributionRulesForMalaysiaRuleSet2IncludingNotReportableTransacionRuleSet(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("MalaysiaComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code : " + ruleSetCode);
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefaultMalaysiaRuleSet(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//01
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.DefaultMalaysiaRuleSet;
			configuration.RuleSetDescription = RuleSetDescriptions.DefaultMalaysiaRuleSet;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;

			//02
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.DefaultMalaysiaRuleSet;
			configuration.RuleSetDescription = RuleSetDescriptions.DefaultMalaysiaRuleSet;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_02;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01;

			//03
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.DefaultMalaysiaRuleSet;
			configuration.RuleSetDescription = RuleSetDescriptions.DefaultMalaysiaRuleSet;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_03;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01;

			//11
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.DefaultMalaysiaRuleSet;
			configuration.RuleSetDescription = RuleSetDescriptions.DefaultMalaysiaRuleSet;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_11;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;

			//12
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.DefaultMalaysiaRuleSet;
			configuration.RuleSetDescription = RuleSetDescriptions.DefaultMalaysiaRuleSet;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_12;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
		}

		void AddComplianceSubTypeAttributionRulesForMalaysiaRuleSet2IncludingNotReportableTransacionRuleSet(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//01
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.RuleSetDescription = RuleSetDescriptions.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;

			//02
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.RuleSetDescription = RuleSetDescriptions.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_02;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01;

			//03
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.RuleSetDescription = RuleSetDescriptions.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_03;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01;

			//01
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.RuleSetDescription = RuleSetDescriptions.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;

			//02
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.RuleSetDescription = RuleSetDescriptions.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_02;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01;

			//03
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.RuleSetDescription = RuleSetDescriptions.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_03;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01;

			//11
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.RuleSetDescription = RuleSetDescriptions.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_11;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;

			//12
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.RuleSetDescription = RuleSetDescriptions.MalaysiaRuleSet2IncludingNotReportableTransacion;
			configuration.SubType = MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_12;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
		}

		static class ComplianceSubTypes
		{
			public static ComplianceSubType SubType_01 => new ComplianceSubType(ComplianceSubTypeCodes.SubType_01, () => ComplianceSubTypeDescriptions.SubType_01, () => ComplianceSubTypeLocalDescriptions.SubType_01, () => ComplianceSubTypeInternalImplementationNote.SubType_01, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType SubType_02 => new ComplianceSubType(ComplianceSubTypeCodes.SubType_02, () => ComplianceSubTypeDescriptions.SubType_02, () => ComplianceSubTypeLocalDescriptions.SubType_02, () => ComplianceSubTypeInternalImplementationNote.SubType_02, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType SubType_03 => new ComplianceSubType(ComplianceSubTypeCodes.SubType_03, () => ComplianceSubTypeDescriptions.SubType_03, () => ComplianceSubTypeLocalDescriptions.SubType_03, () => ComplianceSubTypeInternalImplementationNote.SubType_03, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType SubType_11 => new ComplianceSubType(ComplianceSubTypeCodes.SubType_11, () => ComplianceSubTypeDescriptions.SubType_11, () => ComplianceSubTypeLocalDescriptions.SubType_11, () => ComplianceSubTypeInternalImplementationNote.SubType_11, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType SubType_12 => new ComplianceSubType(ComplianceSubTypeCodes.SubType_12, () => ComplianceSubTypeDescriptions.SubType_12, () => ComplianceSubTypeLocalDescriptions.SubType_12, () => ComplianceSubTypeInternalImplementationNote.SubType_12, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
		}

		public static class ComplianceSubTypeCodes
		{
			public const string SubType_01 = "01";
			public const string SubType_02 = "02";
			public const string SubType_03 = "03";
			public const string SubType_11 = "11";
			public const string SubType_12 = "12";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString SubType_01 { get { return ResString.GetMultilingualString("MYComplianceSubTypeCodeList|SubType_01", "Tax Invoice"); } }
			public static MultilingualString SubType_02 { get { return ResString.GetMultilingualString("MYComplianceSubTypeCodeList|SubType_02", "Tax Credit Note"); } }
			public static MultilingualString SubType_03 { get { return ResString.GetMultilingualString("MYComplianceSubTypeCodeList|SubType_03", "Tax Debit Note"); } }
			public static MultilingualString SubType_11 { get { return ResString.GetMultilingualString("MYComplianceSubTypeCodeList|SubType_11", "Self-billed Invoice"); } }
			public static MultilingualString SubType_12 { get { return ResString.GetMultilingualString("MYComplianceSubTypeCodeList|SubType_12", "Self-billed Credit Note"); } }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string SubType_01 = "Fakta cukai";
			public const string SubType_02 = "Nota Kredit Tax";
			public const string SubType_03 = "Nota Debit Tax";
			public const string SubType_11 = "Invois yang dibilkan sendiri";
			public const string SubType_12 = "Nota Kredit yang dibilkan sendiri";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		static class ComplianceSubTypeInternalImplementationNote
		{
			public const string SubType_01 = "Original invoice.";
			public const string SubType_02 = "Credit Note created from original invoice as Amendment/Reversal.";
			public const string SubType_03 = "Debit Note created from original invoice as Amendment.";
			public const string SubType_11 = "Self-billed invoice.";
			public const string SubType_12 = "Credit Note created from original self-billed invoice as Amendment/Reversal.";
		}

		public static class RuleSetCodes
		{
			public const string DefaultMalaysiaRuleSet = "1";
			public const string MalaysiaRuleSet2IncludingNotReportableTransacion = "2";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Descriptions for compliance rule sets are not translated")]
		static class RuleSetDescriptions
		{
			public const string DefaultMalaysiaRuleSet = "Tax Invoice";
			public const string MalaysiaRuleSet2IncludingNotReportableTransacion = "Tax Invoice and Not Reportable Invoice";
		}

		public ZString ErrorMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader)
		{
			var result = ZString.Empty;

			if (transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && transactionHeader.AH_TransactionType == TransactionTypes.Invoice)
			{
				if (transactionHeader.AH_ComplianceSubType.IsEmpty)
				{
					return result;
				}

				if (transactionHeader.AH_ComplianceSubType != MalaysiaComplianceInfo.ComplianceSubTypeCodes.SubType_01)
				{
					result = Res.GetString("90B9AADC-E9C8-4B87-8960-0401B0E7151A", $"Enter Compliance Sub Type {ComplianceSubTypeCodes.SubType_01} for Tax Invoice.");
				}
			}
			return result;
		}

		ZString IComplianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader)
		{
			return ZString.Empty;
		}

		bool IComplianceSubTypeGUIProvider.ComplianceSubTypeIsReadOnly(bool hasBeenCreatedAsAmending, string ledger, string transactionType, bool originalTransactionReferenceIsEmpty)
		{
			if (ledger != LedgerTypes.AccountsReceivable)
			{
				return false;
			}

			if (hasBeenCreatedAsAmending)
			{
				return true;
			}

			return transactionType == TransactionTypes.CreditNote ? originalTransactionReferenceIsEmpty : !originalTransactionReferenceIsEmpty;
		}

		bool IComplianceSubTypeGUIProvider.ShouldClearComplianceSubType(ZGuid originalTransactionReference)
		{
			return originalTransactionReference.IsEmpty;
		}

		#region IComplianceInfoEInvoicingGUIActionDocumentRequest

		public ZString DocumentRequestMenuName => Res.GetString("444BB4F1-AC8A-4AB1-B28F-D4D16151696F", "Request e-Invoice Document");

		public ZString DocumentRequestActionInformation => Res.GetString("34A5CC6C-1875-4AF1-B7DE-0B1B3A933A85", @"Your request for a copy of the tax invoice is being processed.
Please Note:
- A request for a copy of the tax invoice can only be made for an Invoice or Credit Note in which E-Reporting Status = SUC.
- A new request for a copy of the tax invoice is allowed where response from an existing request has not been received after 60 minutes from request submission.");

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProvider

		public IEnumerable<AccTransactionHeader> GetEligibleInvoices(IEnumerable<AccTransactionHeader> selectedTransactions)
		{
			return selectedTransactions.Where(x => (x.AH_Ledger == LedgerTypes.AccountsReceivable || x.AH_Ledger == LedgerTypes.AccountsPayable) && (x.AH_TransactionType == TransactionTypes.Invoice || x.AH_TransactionType == TransactionTypes.CreditNote));
		}

		public bool ExistActiveDocumentRequestPivot(ZDateTime lastSentTime)
		{
			return lastSentTime > ZDateTime.UtcNow.AddHours(-1);
		}

		public bool IsCountryEnableComplianceEInvoicing(bool isAPTransaction = false) =>
			isAPTransaction ? AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.Value : AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		#endregion

		#region IComplianceInfoEInvoicingGUIActionStatusRequest

		public ZString StatusRequestMenuName => Res.GetString("AB3DB8EF-05CB-42A6-BF19-827C09C242A6", "Request e-Invoice Status");

		public ZString StatusRequestActionInformation => Res.GetString("EF1190DF-8AAD-484A-AA75-2D1CC41679AF", @"Your request is being processed.
Please note:
1.Only transactions that have an e-Reporting status of 'DLV - Delivered' or 'IMP - In Processing' or 'FAL - Failed' with E-Reporting Govt # are eligible for 'Request e-Invoice Status'.
2.A new request is allowed when response from an existing request has not been received after 30 minutes from request submission.");

		#endregion

		#region IEInvoicingRegistryProvider

		bool IEInvoicingRegistryProvider.ShouldAutoSetEReportingComplianceDate => true;

		#endregion
	}
}
