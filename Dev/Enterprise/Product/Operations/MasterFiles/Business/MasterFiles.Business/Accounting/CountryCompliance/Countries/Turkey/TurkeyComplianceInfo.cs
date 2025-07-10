using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.TurkeyOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class TurkeyComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeTaxRegistrationTypeRuleProvider,
		IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IOrgHeaderPostingValidation,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoEInvoicingGUIActionProvider,
		IComplianceInfoEInvoicingGUIActionDocumentRequest,
		IComplianceInfoEInvoicingGUIActionDocumentRequestAP,
		IComplianceInfoEInvoicingGUIActionStatusRequest,
		IComplianceInfoEInvoicingGUIActionStatusRequestAP,
		IComplianceInfoEInvoicingGUIActionQueueReversedTransaction,
		IComplianceSubTypeDependencyConfigurationProvider,
		IComplianceRegistryDefaultProvider,
		IProtectComplianceSubTypeForEInvoicingTransactions,
		IEnableTransactionsPendingAllocationAllocateAsReceivable,
		IComplianceSubTypeValidation,
		IComplianceSequenceValidationProvider,
		ITaxMessagesGroupProvider,
		IOriginalInvoiceNumberAndDateValidationDecider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Turkey;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.TradeRegistryNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region Registry Defaults

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry() => AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled)
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> proposedValue == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate
			? ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(proposedValue, CountryCode)
			: proposedValue == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print
			? ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBePrint(proposedValue, CountryCode)
			: null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> proposedValue == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate
			? ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(proposedValue, CountryCode)
			: proposedValue == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print
			? ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBePrint(proposedValue, CountryCode)
			: null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => null;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				// AR Transactions
				ComplianceSubTypes.EAR,
				ComplianceSubTypes.EIN,
				ComplianceSubTypes.EIC,
				ComplianceSubTypes.ICN,
				ComplianceSubTypes.XCL,
				// AP Transactions
				ComplianceSubTypes.PAR,
				ComplianceSubTypes.PIN,
				ComplianceSubTypes.PIC,
				ComplianceSubTypes.PCL,
				// Return of AP Transactions
				ComplianceSubTypes.DAR,
				ComplianceSubTypes.DIN,
				ComplianceSubTypes.DCN,
				ComplianceSubTypes.DCL,
				// Return of AR Transactions
				ComplianceSubTypes.CAR,
				ComplianceSubTypes.CIN,
				ComplianceSubTypes.CCN,
				ComplianceSubTypes.CCL,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string EAR = "EAR";
			public const string EIN = "EIN";
			public const string EIC = "EIC";
			public const string ICN = "ICN";
			public const string XCL = "XCL";

			public const string PAR = "PAR";
			public const string PIN = "PIN";
			public const string PIC = "PIC";
			public const string PCL = "PCL";

			public const string DAR = "DAR";
			public const string DIN = "DIN";
			public const string DCN = "DCN";
			public const string DCL = "DCL";

			public const string CAR = "CAR";
			public const string CIN = "CIN";
			public const string CCN = "CCN";
			public const string CCL = "CCL";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString EAR => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|EAR", "Receivables e-Archive Invoice");
			public static MultilingualString EIN => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|EIN", "Receivables e-Invoice (Basic)");
			public static MultilingualString EIC => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|EIC", "Receivables e-Invoice (Commercial)");
			public static MultilingualString ICN => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|ICN", "Receivables Invoice Cancellation");
			public static MultilingualString XCL => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|XCL", "Receivables Reimbursement / Disbursement / Excluded Supply");

			public static MultilingualString PAR => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|PAR", "Payables e-Archive Invoice");
			public static MultilingualString PIN => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|PIN", "Payables e-Invoice (Basic)");
			public static MultilingualString PIC => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|PIC", "Payables e-Invoice (Commercial)");
			public static MultilingualString PCL => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|PCL", "Payables Reimbursement / Disbursement / Excluded Supply");

			public static MultilingualString DAR => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|DAR", "Return of Purchase e-Archive Invoice");
			public static MultilingualString DIN => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|DIN", "Return of Purchase e-Invoice (Basic)");
			public static MultilingualString DCN => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|DCN", "Return of Purchase Cancellation");
			public static MultilingualString DCL => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|DCL", "Return of Purchase Reimbursement / Disbursement / Excluded Supply");

			public static MultilingualString CAR => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|CAR", "Return of Sale e-Archive Invoice");
			public static MultilingualString CIN => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|CIN", "Return of Sale e-Invoice (Basic)");
			public static MultilingualString CCN => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|CCN", "Return of Sale Invoice Cancellation");
			public static MultilingualString CCL => ResString.GetMultilingualString("TRComplianceSubTypeCodeList|CCL", "Return of Sale Reimbursement / Disbursement / Excluded Supply");
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string EAR = "Satış Faturası (e-Arşiv Fatura)";
			public const string EIN = "Satış Faturası (Temel e-Fatura)";
			public const string EIC = "Satış Faturası (Ticari e-Fatura)";
			public const string ICN = "Satış Faturası İptali";
			public const string XCL = "Satış Faturası Geri Ödeme Belgesi";

			public const string PAR = "Alış Faturası (e-Arşiv Fatura)";
			public const string PIN = "Alış Faturası (Temel e-Fatura)";
			public const string PIC = "Alış Faturası (Ticari e-Fatura)";
			public const string PCL = "Alış Faturası Geri Ödeme Belgesi";

			public const string DAR = "Alıştan İade Faturası (e-Arşiv Fatura)";
			public const string DIN = "Alıştan İade Faturası (Temel e-Fatura)";
			public const string DCN = "Alıştan İade Faturası Fatura İptali";
			public const string DCL = "Alıştan İade Faturası Geri Ödeme Belgesi";

			public const string CAR = "Satıştan İade Faturası (e-Arşiv Fatura)";
			public const string CIN = "Satıştan İade Faturası (Temel e-Fatura)";
			public const string CCN = "Satıştan İade Faturası Fatura İptali";
			public const string CCL = "Satıştan İade Faturası Geri Ödeme Belgesi";
		}
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant String")]
		static class InternalImplementantionNotes
		{
			public const string EAR = "Used to identify Invoice (INV) transactions issued to organizations not registered for Turkey e-Invoicing. The Receivables Organization has neither a TR VTE nor a TR VTC Organization registration code.";
			public const string EIN = "Used to identify Invoice (INV) transactions issued as Basic Invoice to organizations registered for Turkey e-Invoicing. The Receivables Organization should have a TR VTE Organization registration code.";
			public const string EIC = "Used to identify Invoice (INV) transactions issued as Commercial Invoice to organizations registered for Turkey e-Invoicing. The Receivables Organization should have a TR VTC Organization registration code confirming an agreement between the buyer and seller to issue Commercial e-Invoices.";
			public const string ICN = "Used to identify Credit Note (CRD) transactions that cancel or reverse an Invoice transaction.";
			public const string XCL = "Used to identify Invoice (INV) and Credit Note (CRD) transactions that do not require e-Invoice documentation because no taxable supply was made or received (eg. Disbursement / Reimbursement only transactions).";

			public const string PAR = "Used to identify Invoice (INV) transactions issued as e-Archive invoice and received via e-Mail. The Payables Organization has neither a TR VTE nor a TR VTC Organization registration code.";
			public const string PIN = "Used to identify Invoice (INV) transactions issued as Basic Invoice from organizations registered for Turkey e-Invoicing. Depends on payable invoice scenario.";
			public const string PIC = "Used to identify Invoice (INV) transactions issued as Commercial Invoice from organizations registered for Turkey e-Invoicing. Depends on payable invoice scenario.";
			public const string PCL = "Used to identify Invoice (INV) and Credit Note (CRD) transactions that do not require e-Invoice documentation because no taxable supply was made or received (eg. Disbursement / Reimbursement only transactions).";

			public const string DAR = "Used to identify a Credit Note (CRD) to return a Purchase Invoice issued as e-Archive invoice by e-Invoice registered organizations. The Organization has either a TR VTE or a TR VTC Organization registration code.";
			public const string DIN = "Used to identify a Credit Note (CRD) to return a Purchase Invoice issued as Basic Invoice by e-Invoice registered organizations. The Organization should have a TR VTE Organization registration code.";
			public const string DCN = "Used to identify an Invoice (INV) transaction that cancels or reverses a Payables Credit Note transaction.";
			public const string DCL = "Used to identify Invoice (INV) and Credit Note (CRD) transactions that do not require e-Invoice documentation because no taxable supply was made or received (eg. Disbursement / Reimbursement only transactions).";

			public const string CAR = "Used to identify a Credit Note (CRD) to return a Sale Invoice issued as e-Archive invoice. The Organization has neither a TR VTE nor a TR VTC Organization registration code.";
			public const string CIN = "Used to identify a Credit Note (CRD) to return a Sale Invoice issued as Basic Invoice from organizations registered for Turkey e-Invoicing. Depends on payable invoice scenario.";
			public const string CCN = "Used to identify Credit Note (CRD) transactions that cancel or reverse an Invoice transaction.";
			public const string CCL = "Used to identify Invoice (INV) and Credit Note (CRD) transactions that do not require e-Invoice documentation because no taxable supply was made or received (eg. Disbursement / Reimbursement only transactions).";
		}

		static class ComplianceSubTypes
		{
			public static ComplianceSubType EAR => new ComplianceSubType(ComplianceSubTypeCodes.EAR, () => ComplianceSubTypeDescriptions.EAR, () => ComplianceSubTypeLocalDescriptions.EAR, () => InternalImplementantionNotes.EAR, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType EIN => new ComplianceSubType(ComplianceSubTypeCodes.EIN, () => ComplianceSubTypeDescriptions.EIN, () => ComplianceSubTypeLocalDescriptions.EIN, () => InternalImplementantionNotes.EIN, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType EIC => new ComplianceSubType(ComplianceSubTypeCodes.EIC, () => ComplianceSubTypeDescriptions.EIC, () => ComplianceSubTypeLocalDescriptions.EIC, () => InternalImplementantionNotes.EIC, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType ICN => new ComplianceSubType(ComplianceSubTypeCodes.ICN, () => ComplianceSubTypeDescriptions.ICN, () => ComplianceSubTypeLocalDescriptions.ICN, () => InternalImplementantionNotes.ICN, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD,
				transactionCreatingMode: TransactionCreatingMode.Original | TransactionCreatingMode.Reversal);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => InternalImplementantionNotes.XCL, LedgerOfUse.AR);

			public static ComplianceSubType PAR => new ComplianceSubType(ComplianceSubTypeCodes.PAR, () => ComplianceSubTypeDescriptions.PAR, () => ComplianceSubTypeLocalDescriptions.PAR, () => InternalImplementantionNotes.PAR, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PIN => new ComplianceSubType(ComplianceSubTypeCodes.PIN, () => ComplianceSubTypeDescriptions.PIN, () => ComplianceSubTypeLocalDescriptions.PIN, () => InternalImplementantionNotes.PIN, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PIC => new ComplianceSubType(ComplianceSubTypeCodes.PIC, () => ComplianceSubTypeDescriptions.PIC, () => ComplianceSubTypeLocalDescriptions.PIC, () => InternalImplementantionNotes.PIC, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PCL => new ComplianceSubType(ComplianceSubTypeCodes.PCL, () => ComplianceSubTypeDescriptions.PCL, () => ComplianceSubTypeLocalDescriptions.PCL, () => InternalImplementantionNotes.PCL, LedgerOfUse.AP);

			public static ComplianceSubType DAR => new ComplianceSubType(ComplianceSubTypeCodes.DAR, () => ComplianceSubTypeDescriptions.DAR, () => ComplianceSubTypeLocalDescriptions.DAR, () => InternalImplementantionNotes.DAR, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType DIN => new ComplianceSubType(ComplianceSubTypeCodes.DIN, () => ComplianceSubTypeDescriptions.DIN, () => ComplianceSubTypeLocalDescriptions.DIN, () => InternalImplementantionNotes.DIN, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType DCN => new ComplianceSubType(ComplianceSubTypeCodes.DCN, () => ComplianceSubTypeDescriptions.DCN, () => ComplianceSubTypeLocalDescriptions.DCN, () => InternalImplementantionNotes.DCN, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType DCL => new ComplianceSubType(ComplianceSubTypeCodes.DCL, () => ComplianceSubTypeDescriptions.DCL, () => ComplianceSubTypeLocalDescriptions.DCL, () => InternalImplementantionNotes.DCL, LedgerOfUse.AP);

			public static ComplianceSubType CAR => new ComplianceSubType(ComplianceSubTypeCodes.CAR, () => ComplianceSubTypeDescriptions.CAR, () => ComplianceSubTypeLocalDescriptions.CAR, () => InternalImplementantionNotes.CAR, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD,
				transactionCreatingMode: TransactionCreatingMode.Original | TransactionCreatingMode.Amending);
			public static ComplianceSubType CIN => new ComplianceSubType(ComplianceSubTypeCodes.CIN, () => ComplianceSubTypeDescriptions.CIN, () => ComplianceSubTypeLocalDescriptions.CIN, () => InternalImplementantionNotes.CIN, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD,
				transactionCreatingMode: TransactionCreatingMode.Original | TransactionCreatingMode.Amending);
			public static ComplianceSubType CCN => new ComplianceSubType(ComplianceSubTypeCodes.CCN, () => ComplianceSubTypeDescriptions.CCN, () => ComplianceSubTypeLocalDescriptions.CCN, () => InternalImplementantionNotes.CCN, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType CCL => new ComplianceSubType(ComplianceSubTypeCodes.CCL, () => ComplianceSubTypeDescriptions.CCL, () => ComplianceSubTypeLocalDescriptions.CCL, () => InternalImplementantionNotes.CCL, LedgerOfUse.AR);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			// Empty or Null ruleSetCode is a valid case.
			// It is used in module "Tax ID > Actions > Country/region Compliance Info > Compliance Sub Types > Defaulting Rules".
			if (string.IsNullOrEmpty(ruleSetCode) || ruleSetCode == RuleSetCodes.eFactura)
			{
				AddComplianceSubTypeAttributionRulesForReceivableInvoice(collection);
				AddComplianceSubTypeAttributionRulesForReturnOfPurchaseInvoice(collection);
				AddComplianceSubTypeAttributionRulesForPayableInvoice(collection);
				AddComplianceSubTypeAttributionRulesForReturnOfSaleInvoice(collection);
			}

			if (string.IsNullOrEmpty(ruleSetCode) || ruleSetCode == RuleSetCodes.eArchive)
			{
				AddComplianceSubTypeAttributionRulesForReceivableArchiveOnly(collection);
				AddComplianceSubTypeAttributionRulesForReturnOfPurchaseArchiveOnly(collection);
				AddComplianceSubTypeAttributionRulesForPayableArchiveOnly(collection);
				AddComplianceSubTypeAttributionRulesForReturnOfSaleArchiveOnly(collection);
			}

			if (!string.IsNullOrEmpty(ruleSetCode) && !GetRuleSetList().ContainsCode(ruleSetCode))
			{
				ErrorReporter.ReportOnce("TurkeyComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
			}
		}

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet() => GetRuleSetList();

		CodeDescriptionPairList GetRuleSetList()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.eFactura, RuleSetDescriptions.eFactura);
			rulesetList.AddPair(RuleSetCodes.eArchive, RuleSetDescriptions.eArchive);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.eFactura;
		}

		public static class RuleSetCodes
		{
			public const string eFactura = "1";
			public const string eArchive = "2";
		}

		#region SuppressResourceStringsCheckRegion

		static class RuleSetDescriptions
		{
			public const string eFactura = "e-Fatura";
			public const string eArchive = "e-Archive Only";
		}

		#endregion

		ComplianceSubTypeAttributionRuleConfiguration AddAndGetNewRuleSetConfiguration(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var ruleSet = collection.AddNew();
			ruleSet.Country = CountryCode;
			ruleSet.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			return ruleSet;
		}

		void AddComplianceSubTypeAttributionRulesForReceivableInvoice(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.ICN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.ReversalTransactionOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.EAR;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.ICN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.ReversalTransactionOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.EIN;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.EIN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.BasicEInvoiceRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.EIN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.EIN;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.BasicEInvoiceRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			if (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeatures)
			{
				configuration = AddAndGetNewRuleSetConfiguration(collection);
				configuration.SubType = ComplianceSubTypeCodes.ICN;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.CreditNote;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.ReversalTransactionOnly;
				configuration.ParentTransactionSubType = ComplianceSubTypeCodes.EIC;
				configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
				configuration.RuleSetCode = RuleSetCodes.eFactura;
				configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

				configuration = AddAndGetNewRuleSetConfiguration(collection);
				configuration.SubType = ComplianceSubTypeCodes.EIC;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
				configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommercialEInvoiceRegisteredOrganizationTurkey;
				configuration.RuleSetCode = RuleSetCodes.eFactura;
				configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

				configuration = AddAndGetNewRuleSetConfiguration(collection);
				configuration.SubType = ComplianceSubTypeCodes.EIC;
				configuration.LedgerType = LedgerTypes.AccountsReceivable;
				configuration.InvoiceType = TransactionTypes.Invoice;
				configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
				configuration.ParentTransactionSubType = ComplianceSubTypeCodes.EIC;
				configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommercialEInvoiceRegisteredOrganizationTurkey;
				configuration.RuleSetCode = RuleSetCodes.eFactura;
				configuration.RuleSetDescription = RuleSetDescriptions.eFactura;
			}

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.EAR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.EAR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.EAR;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CIN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.EInvoiceRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CAR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;
		}

		void AddComplianceSubTypeAttributionRulesForReturnOfPurchaseInvoice(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.DCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.DCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;
		}

		void AddComplianceSubTypeAttributionRulesForPayableInvoice(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.DAR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.DIN;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.BasicEInvoiceRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.DIN;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommercialEInvoiceRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PIN;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.BasicEInvoiceRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PIC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommercialEInvoiceRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PAR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;
		}

		void AddComplianceSubTypeAttributionRulesForReturnOfSaleInvoice(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CCN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.CAR;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CCN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.CIN;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.CCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.CCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eFactura;
			configuration.RuleSetDescription = RuleSetDescriptions.eFactura;
		}

		void AddComplianceSubTypeAttributionRulesForReceivableArchiveOnly(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.ICN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.ReversalTransactionOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.EAR;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CAR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingTransactionOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.EAR;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.EAR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.EAR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.EAR;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;
		}

		void AddComplianceSubTypeAttributionRulesForReturnOfPurchaseArchiveOnly(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.DCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.DCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;
		}

		void AddComplianceSubTypeAttributionRulesForPayableArchiveOnly(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.DAR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PAR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.PCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.PCL;
		}

		void AddComplianceSubTypeAttributionRulesForReturnOfSaleArchiveOnly(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CCN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.CAR;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.CCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;

			configuration = AddAndGetNewRuleSetConfiguration(collection);
			configuration.SubType = ComplianceSubTypeCodes.CCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.CCL;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.RuleSetCode = RuleSetCodes.eArchive;
			configuration.RuleSetDescription = RuleSetDescriptions.eArchive;
		}

		bool IComplianceSubTypeTaxRegistrationTypeRuleProvider.IsTaxRegistrationTypeRuleApplicable(IAccComplianceRule rule, OrgHeader header)
		{
			if (GlbCompany.CurrentCompany.Country.Code != CountryCode
				|| rule.TaxRegistrationType == TaxRegistrationTypeCodes.AllOrganizations)
			{
				return true;
			}

			var vteOrgCusCode = header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.VTE, CountryCode);
			var vtcOrgCusCode = header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.VTC, CountryCode);

			return ((vteOrgCusCode != null && rule.TaxRegistrationType == TaxRegistrationTypeCodes.BasicEInvoiceRegisteredOrganizationTurkey)
				|| (vtcOrgCusCode != null && rule.TaxRegistrationType == TaxRegistrationTypeCodes.CommercialEInvoiceRegisteredOrganizationTurkey)
				|| ((vteOrgCusCode != null || vtcOrgCusCode != null) && rule.TaxRegistrationType == TaxRegistrationTypeCodes.EInvoiceRegisteredOrganizationTurkey)
				|| (vteOrgCusCode == null && vtcOrgCusCode == null && rule.TaxRegistrationType == TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey));
		}

		#endregion

		#region IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider

		CodeDescriptionPairList IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider.GetTaxRegistrationTypeList()
		{
			var list = new CodeDescriptionPairList();
			if (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeatures)
			{
				list.AddPair(TaxRegistrationTypeCodes.BasicEInvoiceRegisteredOrganizationTurkey, TaxRegistrationTypeDescriptions.BasicEInvoiceRegisteredOrganizationTurkey);
				list.AddPair(TaxRegistrationTypeCodes.CommercialEInvoiceRegisteredOrganizationTurkey, TaxRegistrationTypeDescriptions.CommercialEInvoiceRegisteredOrganizationTurkey);
				list.AddPair(TaxRegistrationTypeCodes.EInvoiceRegisteredOrganizationTurkey, TaxRegistrationTypeDescriptions.EInvoiceRegisteredOrganizationTurkey);
			}
			else
			{
				list.AddPair(TaxRegistrationTypeCodes.BasicEInvoiceRegisteredOrganizationTurkey, TaxRegistrationTypeDescriptions.eInvoiceRegisteredOrganizationTurkey);
			}
			list.AddPair(TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey, TaxRegistrationTypeDescriptions.eInvoiceNotRegisteredOrganizationTurkey);
			list.AddPair(TaxRegistrationTypeCodes.AllOrganizations, TaxRegistrationTypeDescriptions.AllOrganizations);
			return list;
		}

		#endregion

		#region IOrgHeaderPostingValidation

		(ResourceString errorOrWarningMessage, bool isErrorMessage) IOrgHeaderPostingValidation.CheckOrgHeaderForPosting(AccTransactionHeader transaction)
		{
			(var result, var isErrorMessage) = IsPECOrgCusCodeDefined(transaction, out var errorMessage);
			return (!result ? errorMessage : null, isErrorMessage);
		}

		readonly ResourceString PECWarningMessage = ResString.GetMultilingualString("CB6B3E62-CD37-4C05-887B-37D96DF7EEA4", @"A Post Box Alias is required for this debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the organization record for the debtor to include a valid Post Box Alias email address using the registration number type PEC.

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)");
		readonly ResourceString EMailErrorMessage = ResString.GetMultilingualString("15BE3421-8104-404F-834A-0D26743B100D", @"An email address is required for this Debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the Organization record for the Debtor to include a valid email address (Maintain > Master Data > Organization).

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)");

		(bool hasNotErrorOrWarning, bool isErrorMessage) IsPECOrgCusCodeDefined(AccTransactionHeader transaction, out ResourceString errorMessage)
		{
			errorMessage = null;

			if (IsTransactionEligibleForDebtorValidation(transaction, out ZString complianceSubType) && !complianceSubType.IsEmpty)
			{
				var orgCusCode = transaction.Header.CustomsCodes.GetOrgCusCode(OrgCusCodes.PEC,
					RefCountry.LoadFromCountryCode(transaction.Factory,
					CountryCodes.Turkey));
				if (orgCusCode != null)
				{
					return (hasNotErrorOrWarning: true, isErrorMessage: false);
				}
				switch (complianceSubType)
				{
					case ComplianceSubTypeCodes.EAR:
						errorMessage = EMailErrorMessage;
						return (hasNotErrorOrWarning: false, isErrorMessage: true);
					case ComplianceSubTypeCodes.EIC:
					case ComplianceSubTypeCodes.EIN:
					case ComplianceSubTypeCodes.ICN:
						errorMessage = PECWarningMessage;
						return (hasNotErrorOrWarning: false, isErrorMessage: false);
				}
			}

			return (hasNotErrorOrWarning: true, isErrorMessage: false);
		}

		bool IsTransactionEligibleForDebtorValidation(AccTransactionHeader transaction, out ZString complianceSubType)
		{
			var result = false;
			complianceSubType = ZString.Empty;

			if (IsARInvoice(transaction) &&
				transaction.Company.Country.Code == CountryCodes.Turkey &&
				IsCountryEnableComplianceEInvoicing() &&
				transaction.AH_OH.IsValid)
			{
				complianceSubType = transaction.AH_ComplianceSubType != ZString.Empty ?
					transaction.AH_ComplianceSubType :
					((transaction as IComplianceNumberSequence)?.GetMatchingComplianceSubType() ?? ZString.Empty);
				result = true;
			}

			return result;
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProvider

		public bool IsCountryEnableComplianceEInvoicing(bool isAPTransaction = false) =>
			isAPTransaction
			? AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeatures
			: AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeatures;

		public string NotEligibleForRequestsMessage => Res.GetString("D7F82BC7-B1F2-4477-AA75-C602747B65A0", "The transaction is not eligible for requests, or missing an ETTN.");

		public IEnumerable<AccTransactionHeader> GetEligibleInvoices(IEnumerable<AccTransactionHeader> selectedTransactions) =>
			selectedTransactions.Where(x => !x.AH_IsCancelled
			&& (IsARInvoice(x) || IsAPCreditNote(x))
			&& GetEInvoicingEligibleComplianceSubTypeList().Contains(x.AH_ComplianceSubType)).ToList();

		public bool ExistActiveDocumentRequestPivot(ZDateTime lastSentTime)
		{
			return lastSentTime > LatestSentTimeAllowingReRequestDocument;
		}

		ZDateTime LatestSentTimeAllowingReRequestDocument
			=> ZDateTime.UtcNow.AddMinutes(AccountingMasterFilesRegistry.Instance.TaxInvoiceStatusUpdateAutomatedRequestSchedule.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) * -1);

		#endregion

		static bool IsARInvoice(AccTransactionHeader transaction) =>
			transaction.AH_Ledger == LedgerTypes.AccountsReceivable
			&& transaction.AH_TransactionType == TransactionTypes.Invoice;

		static bool IsAPCreditNote(AccTransactionHeader transaction) =>
			transaction.AH_Ledger == LedgerTypes.AccountsPayable
			&& transaction.AH_TransactionType == TransactionTypes.CreditNote;

		#region IComplianceInfoEInvoicingGUIActionDocumentRequest

		public ZString DocumentRequestMenuName => Res.GetString("21286289-53ED-4E41-A915-6246EA949641", "Request e-Invoice PDF Copy");

		public ZString DocumentRequestActionInformation
		{
			get
			{
				return Res.GetString("F94CF785-09C0-45DC-85F6-439B6E274C69", @"Your request for a PDF copy of the tax invoice is being processed.
Please Note:
- No request for a copy of the tax invoice will be made for electronic invoices under the following conditions:
    When transaction does not have a Compliance Sub Type of EIN, EIC, or EAR attached.
    When transaction has been reversed.
    When transaction does not have an ETTN attached.
    When transaction has an existing request for a copy where it is still being processed.");
			}
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionStatusRequest

		public ZString StatusRequestMenuName => Res.GetString("8FA19CF0-B10A-499C-ABBB-EF845ACA4C44", "Request e-Invoice Transaction Status Update");

		public ZString StatusRequestActionInformation
		{
			get
			{
				return Res.GetString("7155038E-730F-46CE-BF57-67442F46DF8B", @"Your request for an update to the status of the electronic invoice is now being processed.
Please Note:
- No status request will be made for electronic invoices under the following conditions:
    When transaction already has a status of Success.
    When transaction has been Canceled.
    When transaction does not have a compliance sub type of EIN, EIC or EAR attached.
    When transaction does not have an ETTN attached.
    When transaction's status has already been requested.");
			}
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionDocumentRequestAP

		public ZString DocumentRequestActionInformationAP
		{
			get
			{
				return Res.GetString("0D2A15D2-DC26-4E97-8153-5CA40083A4A6", @"Your request for a PDF copy of the tax invoice is being processed.
Please Note:
- No request for a copy of the tax invoice will be made for electronic invoices under the following conditions:
    When transaction is not Payable Credit Note
    When transaction does not have a Compliance Sub Type of DIN or DAR attached.
    When transaction has been reversed.
    When transaction does not have an ETTN attached.
    When transaction has an existing request for a copy where it is still being processed.");
			}
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionStatusRequestAP

		public ZString StatusRequestActionInformationAP
		{
			get
			{
				return Res.GetString("867858B0-AD0D-46B6-9EE3-C1E701569A02", @"Your request for an update to the status of the electronic invoice is now being processed.
Please Note:
- No status request will be made for electronic invoices under the following conditions:
    When transaction is not Payable Credit Note.
    When transaction already has a status of Success.
    When transaction has been Canceled.
    When transaction does not have a compliance sub type of DIN or DAR attached.
    When transaction does not have an ETTN attached.
    When transaction's status has already been requested.");
			}
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionQueueReversedTransaction

		public bool RejectReQueueForReversedTransaction(string complianceSubType) => complianceSubType != ComplianceSubTypeCodes.DAR && complianceSubType != ComplianceSubTypeCodes.DIN && complianceSubType != ComplianceSubTypeCodes.CCN;

		#endregion IComplianceInfoEInvoicingGUIActionQueueReversedTransaction

		#region IComplianceSubTypeDependencyConfigurationProvider

		void IComplianceSubTypeDependencyConfigurationProvider.GetDefaults(ComplianceSubTypeDependencyConfigurationCollection collection)
		{
			if (IsCountryEnableComplianceEInvoicing())
			{
				var configuration = collection.AddNew();
				configuration.Country = CountryCode;
				configuration.ChildSubType = ComplianceSubTypeCodes.EIC;
				configuration.ParentSubType = ComplianceSubTypeCodes.EIN;

				configuration = collection.AddNew();
				configuration.Country = CountryCode;
				configuration.ChildSubType = ComplianceSubTypeCodes.DIN;
				configuration.ParentSubType = ComplianceSubTypeCodes.EIN;

				configuration = collection.AddNew();
				configuration.Country = CountryCode;
				configuration.ChildSubType = ComplianceSubTypeCodes.DCL;
				configuration.ParentSubType = ComplianceSubTypeCodes.XCL;

				configuration = collection.AddNew();
				configuration.Country = CountryCode;
				configuration.ChildSubType = ComplianceSubTypeCodes.DAR;
				configuration.ParentSubType = ComplianceSubTypeCodes.EAR;

				configuration = collection.AddNew();
				configuration.Country = CountryCode;
				configuration.ChildSubType = ComplianceSubTypeCodes.DCN;
				configuration.ParentSubType = ComplianceSubTypeCodes.ICN;

				configuration = collection.AddNew();
				configuration.Country = CountryCode;
				configuration.ChildSubType = ComplianceSubTypeCodes.CCL;
				configuration.ParentSubType = ComplianceSubTypeCodes.PCL;
			}
		}

		#endregion

		#region IProtectComplianceSubTypeForEInvoicingTransactions

		string IProtectComplianceSubTypeForEInvoicingTransactions.ErrorMessageIfComplianceSubTypeIsProtected(AccTransactionHeader transaction)
		{
			var valueToProtect = transaction.IsARCreditNote && AllocateAsReceivableConversionConfiguration.TryGetValue(transaction.AH_ComplianceSubTypeInfo.OriginalValue.ToString(), out var complianceSubType)
				? complianceSubType
				: transaction.AH_ComplianceSubTypeInfo.OriginalValue.ToString();
			return
				transaction.IsInDatabase
				&& !transaction.AH_ComplianceSubTypeInfo.OriginalValue.IsEmpty
				&& valueToProtect != transaction.AH_ComplianceSubType
				&& (transaction.IsAPInvoice || transaction.IsARCreditNote)
				&& (ObjectFactory.Get<IEInvoicingHelper>()?.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived) ?? false)
				? Res.GetString("4E9D2FCE-D366-44BA-A4F3-E836DA805B67", "You cannot change the current '{0}' value of Compliance Sub Type for e-Reporting transactions.", valueToProtect)
				: string.Empty;
		}

		#endregion

		#region IEnableTransactionsPendingAllocationAllocateAsReceivable

		string IEnableTransactionsPendingAllocationAllocateAsReceivable.GetEligibleComplianceSubTypeForAllocateAsReceivable(string payableComplianceSubType) =>
			AllocateAsReceivableConversionConfiguration.TryGetValue(payableComplianceSubType, out var result) ? result : payableComplianceSubType;

		bool IEnableTransactionsPendingAllocationAllocateAsReceivable.IsComplianceSubTypeNotEligibleForAllocateAsReceivable(string complianceSubType) =>
			!AllocateAsReceivableConversionConfiguration.ContainsValue(complianceSubType);

		readonly Dictionary<string, string> AllocateAsReceivableConversionConfiguration = new Dictionary<string, string>()
		{
			{ ComplianceSubTypeCodes.PAR, ComplianceSubTypeCodes.CAR },
			{ ComplianceSubTypeCodes.PIN, ComplianceSubTypeCodes.CIN },
			{ ComplianceSubTypeCodes.PIC, ComplianceSubTypeCodes.CIN },
		};

		#endregion

		#region ITaxMessagesGroupProvider

		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code201, Description = TaxMessageGroupDescriptions.Desc201, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code201 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code202, Description = TaxMessageGroupDescriptions.Desc202, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code202 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code204, Description = TaxMessageGroupDescriptions.Desc204, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code204 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code205, Description = TaxMessageGroupDescriptions.Desc205, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code205 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code206, Description = TaxMessageGroupDescriptions.Desc206, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code206 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code207, Description = TaxMessageGroupDescriptions.Desc207, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code207 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code208, Description = TaxMessageGroupDescriptions.Desc208, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code208 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code209, Description = TaxMessageGroupDescriptions.Desc209, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code209 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code211, Description = TaxMessageGroupDescriptions.Desc211, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code211 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code212, Description = TaxMessageGroupDescriptions.Desc212, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code212 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code213, Description = TaxMessageGroupDescriptions.Desc213, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code213 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code214, Description = TaxMessageGroupDescriptions.Desc214, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code214 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code215, Description = TaxMessageGroupDescriptions.Desc215, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code215 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code216, Description = TaxMessageGroupDescriptions.Desc216, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code216 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code217, Description = TaxMessageGroupDescriptions.Desc217, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code217 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code218, Description = TaxMessageGroupDescriptions.Desc218, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code218 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code219, Description = TaxMessageGroupDescriptions.Desc219, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code219 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code220, Description = TaxMessageGroupDescriptions.Desc220, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code220 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code221, Description = TaxMessageGroupDescriptions.Desc221, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code221 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code223, Description = TaxMessageGroupDescriptions.Desc223, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code223 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code225, Description = TaxMessageGroupDescriptions.Desc225, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code225 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code226, Description = TaxMessageGroupDescriptions.Desc226, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code226 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code227, Description = TaxMessageGroupDescriptions.Desc227, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code227 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code228, Description = TaxMessageGroupDescriptions.Desc228, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code228 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code229, Description = TaxMessageGroupDescriptions.Desc229, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code229 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code230, Description = TaxMessageGroupDescriptions.Desc230, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code230 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code231, Description = TaxMessageGroupDescriptions.Desc231, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code231 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code232, Description = TaxMessageGroupDescriptions.Desc232, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code232 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code234, Description = TaxMessageGroupDescriptions.Desc234, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code234 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code235, Description = TaxMessageGroupDescriptions.Desc235, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code235 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code236, Description = TaxMessageGroupDescriptions.Desc236, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code236 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code237, Description = TaxMessageGroupDescriptions.Desc237, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code237 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code238, Description = TaxMessageGroupDescriptions.Desc238, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code238 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code239, Description = TaxMessageGroupDescriptions.Desc239, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code239 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code240, Description = TaxMessageGroupDescriptions.Desc240, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code240 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code241, Description = TaxMessageGroupDescriptions.Desc241, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code241 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code242, Description = TaxMessageGroupDescriptions.Desc242, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code242 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code250, Description = TaxMessageGroupDescriptions.Desc250, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code250 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code301, Description = TaxMessageGroupDescriptions.Desc301, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code301 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code302, Description = TaxMessageGroupDescriptions.Desc302, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code302 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code303, Description = TaxMessageGroupDescriptions.Desc303, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code303 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code304, Description = TaxMessageGroupDescriptions.Desc304, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code304 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code305, Description = TaxMessageGroupDescriptions.Desc305, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code305 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code306, Description = TaxMessageGroupDescriptions.Desc306, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code306 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code307, Description = TaxMessageGroupDescriptions.Desc307, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code307 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code308, Description = TaxMessageGroupDescriptions.Desc308, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code308 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code309, Description = TaxMessageGroupDescriptions.Desc309, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code309 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code310, Description = TaxMessageGroupDescriptions.Desc310, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code310 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code311, Description = TaxMessageGroupDescriptions.Desc311, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code311 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code312, Description = TaxMessageGroupDescriptions.Desc312, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code312 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code313, Description = TaxMessageGroupDescriptions.Desc313, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code313 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code314, Description = TaxMessageGroupDescriptions.Desc314, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code314 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code315, Description = TaxMessageGroupDescriptions.Desc315, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code315 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code316, Description = TaxMessageGroupDescriptions.Desc316, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code316 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code317, Description = TaxMessageGroupDescriptions.Desc317, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code317 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code318, Description = TaxMessageGroupDescriptions.Desc318, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code318 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code319, Description = TaxMessageGroupDescriptions.Desc319, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code319 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code320, Description = TaxMessageGroupDescriptions.Desc320, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code320 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code321, Description = TaxMessageGroupDescriptions.Desc321, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code321 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code322, Description = TaxMessageGroupDescriptions.Desc322, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code322 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code323, Description = TaxMessageGroupDescriptions.Desc323, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code323 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code324, Description = TaxMessageGroupDescriptions.Desc324, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code324 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code325, Description = TaxMessageGroupDescriptions.Desc325, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code325 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code326, Description = TaxMessageGroupDescriptions.Desc326, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code326 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code327, Description = TaxMessageGroupDescriptions.Desc327, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code327 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code328, Description = TaxMessageGroupDescriptions.Desc328, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code328 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code330, Description = TaxMessageGroupDescriptions.Desc330, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code330 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code331, Description = TaxMessageGroupDescriptions.Desc331, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code331 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code332, Description = TaxMessageGroupDescriptions.Desc332, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code332 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code333, Description = TaxMessageGroupDescriptions.Desc333, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code333 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code334, Description = TaxMessageGroupDescriptions.Desc334, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code334 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code335, Description = TaxMessageGroupDescriptions.Desc335, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code335 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code336, Description = TaxMessageGroupDescriptions.Desc336, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code336 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code337, Description = TaxMessageGroupDescriptions.Desc337, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code337 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code338, Description = TaxMessageGroupDescriptions.Desc338, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code338 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code339, Description = TaxMessageGroupDescriptions.Desc339, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code339 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code340, Description = TaxMessageGroupDescriptions.Desc340, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code340 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code341, Description = TaxMessageGroupDescriptions.Desc341, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code341 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code350, Description = TaxMessageGroupDescriptions.Desc350, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code350 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code351, Description = TaxMessageGroupDescriptions.Desc351, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code351 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code601, Description = TaxMessageGroupDescriptions.Desc601, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code601 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code602, Description = TaxMessageGroupDescriptions.Desc602, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code602 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code603, Description = TaxMessageGroupDescriptions.Desc603, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code603 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code604, Description = TaxMessageGroupDescriptions.Desc604, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code604 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code605, Description = TaxMessageGroupDescriptions.Desc605, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code605 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code606, Description = TaxMessageGroupDescriptions.Desc606, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code606 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code607, Description = TaxMessageGroupDescriptions.Desc607, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code607 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code608, Description = TaxMessageGroupDescriptions.Desc608, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code608 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code609, Description = TaxMessageGroupDescriptions.Desc609, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code609 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code610, Description = TaxMessageGroupDescriptions.Desc610, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code610 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code611, Description = TaxMessageGroupDescriptions.Desc611, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code611 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code612, Description = TaxMessageGroupDescriptions.Desc612, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code612 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code613, Description = TaxMessageGroupDescriptions.Desc613, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code613 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code614, Description = TaxMessageGroupDescriptions.Desc614, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code614 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code615, Description = TaxMessageGroupDescriptions.Desc615, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code615 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code616, Description = TaxMessageGroupDescriptions.Desc616, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code616 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code617, Description = TaxMessageGroupDescriptions.Desc617, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code617 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code618, Description = TaxMessageGroupDescriptions.Desc618, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code618 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code619, Description = TaxMessageGroupDescriptions.Desc619, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code619 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code620, Description = TaxMessageGroupDescriptions.Desc620, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code620 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code621, Description = TaxMessageGroupDescriptions.Desc621, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code621 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code622, Description = TaxMessageGroupDescriptions.Desc622, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code622 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code623, Description = TaxMessageGroupDescriptions.Desc623, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code623 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code624, Description = TaxMessageGroupDescriptions.Desc624, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code624 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code625, Description = TaxMessageGroupDescriptions.Desc625, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code625 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code626, Description = TaxMessageGroupDescriptions.Desc626, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code626 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code627, Description = TaxMessageGroupDescriptions.Desc627, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code627 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code801, Description = TaxMessageGroupDescriptions.Desc801, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code801 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code802, Description = TaxMessageGroupDescriptions.Desc802, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code802 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code803, Description = TaxMessageGroupDescriptions.Desc803, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code803 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code804, Description = TaxMessageGroupDescriptions.Desc804, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code804 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code805, Description = TaxMessageGroupDescriptions.Desc805, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code805 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code806, Description = TaxMessageGroupDescriptions.Desc806, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code806 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code807, Description = TaxMessageGroupDescriptions.Desc807, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code807 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code808, Description = TaxMessageGroupDescriptions.Desc808, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code808 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code809, Description = TaxMessageGroupDescriptions.Desc809, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code809 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code810, Description = TaxMessageGroupDescriptions.Desc810, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code810 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code811, Description = TaxMessageGroupDescriptions.Desc811, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code811 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code812, Description = TaxMessageGroupDescriptions.Desc812, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code812 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code813, Description = TaxMessageGroupDescriptions.Desc813, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code813 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code814, Description = TaxMessageGroupDescriptions.Desc814, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code814 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code815, Description = TaxMessageGroupDescriptions.Desc815, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code815 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code816, Description = TaxMessageGroupDescriptions.Desc816, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code816 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code817, Description = TaxMessageGroupDescriptions.Desc817, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code817 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code818, Description = TaxMessageGroupDescriptions.Desc818, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code818 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code819, Description = TaxMessageGroupDescriptions.Desc819, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code819 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code820, Description = TaxMessageGroupDescriptions.Desc820, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code820 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code821, Description = TaxMessageGroupDescriptions.Desc821, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code821 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code822, Description = TaxMessageGroupDescriptions.Desc822, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code822 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code823, Description = TaxMessageGroupDescriptions.Desc823, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code823 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code824, Description = TaxMessageGroupDescriptions.Desc824, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code824 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code825, Description = TaxMessageGroupDescriptions.Desc825, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code825 },
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string Code201 = "201";
			public const string Code202 = "202";
			public const string Code204 = "204";
			public const string Code205 = "205";
			public const string Code206 = "206";
			public const string Code207 = "207";
			public const string Code208 = "208";
			public const string Code209 = "209";
			public const string Code211 = "211";
			public const string Code212 = "212";
			public const string Code213 = "213";
			public const string Code214 = "214";
			public const string Code215 = "215";
			public const string Code216 = "216";
			public const string Code217 = "217";
			public const string Code218 = "218";
			public const string Code219 = "219";
			public const string Code220 = "220";
			public const string Code221 = "221";
			public const string Code223 = "223";
			public const string Code225 = "225";
			public const string Code226 = "226";
			public const string Code227 = "227";
			public const string Code228 = "228";
			public const string Code229 = "229";
			public const string Code230 = "230";
			public const string Code231 = "231";
			public const string Code232 = "232";
			public const string Code234 = "234";
			public const string Code235 = "235";
			public const string Code236 = "236";
			public const string Code237 = "237";
			public const string Code238 = "238";
			public const string Code239 = "239";
			public const string Code240 = "240";
			public const string Code241 = "241";
			public const string Code242 = "242";
			public const string Code250 = "250";
			public const string Code301 = "301";
			public const string Code302 = "302";
			public const string Code303 = "303";
			public const string Code304 = "304";
			public const string Code305 = "305";
			public const string Code306 = "306";
			public const string Code307 = "307";
			public const string Code308 = "308";
			public const string Code309 = "309";
			public const string Code310 = "310";
			public const string Code311 = "311";
			public const string Code312 = "312";
			public const string Code313 = "313";
			public const string Code314 = "314";
			public const string Code315 = "315";
			public const string Code316 = "316";
			public const string Code317 = "317";
			public const string Code318 = "318";
			public const string Code319 = "319";
			public const string Code320 = "320";
			public const string Code321 = "321";
			public const string Code322 = "322";
			public const string Code323 = "323";
			public const string Code324 = "324";
			public const string Code325 = "325";
			public const string Code326 = "326";
			public const string Code327 = "327";
			public const string Code328 = "328";
			public const string Code330 = "330";
			public const string Code331 = "331";
			public const string Code332 = "332";
			public const string Code333 = "333";
			public const string Code334 = "334";
			public const string Code335 = "335";
			public const string Code336 = "336";
			public const string Code337 = "337";
			public const string Code338 = "338";
			public const string Code339 = "339";
			public const string Code340 = "340";
			public const string Code341 = "341";
			public const string Code350 = "350";
			public const string Code351 = "351";
			public const string Code601 = "601";
			public const string Code602 = "602";
			public const string Code603 = "603";
			public const string Code604 = "604";
			public const string Code605 = "605";
			public const string Code606 = "606";
			public const string Code607 = "607";
			public const string Code608 = "608";
			public const string Code609 = "609";
			public const string Code610 = "610";
			public const string Code611 = "611";
			public const string Code612 = "612";
			public const string Code613 = "613";
			public const string Code614 = "614";
			public const string Code615 = "615";
			public const string Code616 = "616";
			public const string Code617 = "617";
			public const string Code618 = "618";
			public const string Code619 = "619";
			public const string Code620 = "620";
			public const string Code621 = "621";
			public const string Code622 = "622";
			public const string Code623 = "623";
			public const string Code624 = "624";
			public const string Code625 = "625";
			public const string Code626 = "626";
			public const string Code627 = "627";
			public const string Code801 = "801";
			public const string Code802 = "802";
			public const string Code803 = "803";
			public const string Code804 = "804";
			public const string Code805 = "805";
			public const string Code806 = "806";
			public const string Code807 = "807";
			public const string Code808 = "808";
			public const string Code809 = "809";
			public const string Code810 = "810";
			public const string Code811 = "811";
			public const string Code812 = "812";
			public const string Code813 = "813";
			public const string Code814 = "814";
			public const string Code815 = "815";
			public const string Code816 = "816";
			public const string Code817 = "817";
			public const string Code818 = "818";
			public const string Code819 = "819";
			public const string Code820 = "820";
			public const string Code821 = "821";
			public const string Code822 = "822";
			public const string Code823 = "823";
			public const string Code824 = "824";
			public const string Code825 = "825";
		}

		#region SuppressResourceStringsCheckRegion

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString Desc201 => (NoResString)@"17/1 Kültür ve Eğitim Amacı Taşıyan İşlemler";
			public static MultilingualString Desc202 => (NoResString)@"17/2-a Sağlık, Çevre Ve Sosyal Yardım Amaçlı İşlemler";
			public static MultilingualString Desc204 => (NoResString)@"17/2-c Yabancı Diplomatik Organ Ve Hayır Kurumlarının Yapacakları Bağışlarla İlgili Mal Ve Hizmet Alışları";
			public static MultilingualString Desc205 => (NoResString)@"17/2-d Taşınmaz Kültür Varlıklarına İlişkin Teslimler ve Mimarlık Hizmetleri";
			public static MultilingualString Desc206 => (NoResString)@"17/2-e Mesleki Kuruluşların İşlemleri";
			public static MultilingualString Desc207 => (NoResString)@"17/3 Askeri Fabrika, Tersane ve Atölyelerin İşlemleri";
			public static MultilingualString Desc208 => (NoResString)@"17/4-c Birleşme, Devir, Dönüşüm ve Bölünme İşlemleri";
			public static MultilingualString Desc209 => (NoResString)@"17/1 Kültür ve Eğitim Amacı Taşıyan İşlemler";
			public static MultilingualString Desc211 => (NoResString)@"17/4-h Zirai Amaçlı Su Teslimleri İle Köy Tüzel Kişiliklerince Yapılan İçme Suyu teslimleri";
			public static MultilingualString Desc212 => (NoResString)@"17/4-ı Serbest Bölgelerde Verilen Hizmetler";
			public static MultilingualString Desc213 => (NoResString)@"17/4-j Boru Hattı İle Yapılan Petrol Ve Gaz Taşımacılığı";
			public static MultilingualString Desc214 => (NoResString)@"17/4-k Organize Sanayi Bölgelerindeki Arsa ve İşyeri Teslimleri İle Konut Yapı Kooperatiflerinin Üyelerine Konut Teslimleri";
			public static MultilingualString Desc215 => (NoResString)@"17/4-l Varlık Yönetim Şirketlerinin İşlemleri";
			public static MultilingualString Desc216 => (NoResString)@"17/4-m Tasarruf Mevduatı Sigorta Fonunun İşlemleri";
			public static MultilingualString Desc217 => (NoResString)@"17/4-n Basın-Yayın ve Enformasyon Genel Müdürlüğüne Verilen Haber Hizmetleri";
			public static MultilingualString Desc218 => (NoResString)@"KDV 17/4-o md. Gümrük Antrepoları, Geçici Depolama Yerleri ile Gümrüklü Sahalarda Vergisiz Satış Yapılan İşyeri, Depo ve Ardiye Gibi Bağımsız Birimlerin Kiralanması";
			public static MultilingualString Desc219 => (NoResString)@"17/4-p Hazine ve Arsa Ofisi Genel Müdürlüğünün işlemleri";
			public static MultilingualString Desc220 => (NoResString)@"17/4-r İki Tam Yıl Süreyle Sahip Olunan Taşınmaz ve İştirak Hisseleri ile 15/7/2023 tarihinden önce kurumların aktifinde kayıtlı Taşınmaz satışı";
			public static MultilingualString Desc221 => (NoResString)@"Geçici 15 Konut Yapı Kooperatifleri, Belediyeler ve Sosyal Güvenlik Kuruluşlarına Verilen İnşaat Taahhüt Hizmeti";
			public static MultilingualString Desc223 => (NoResString)@"Geçici 20/1 Teknoloji Geliştirme Bölgelerinde Yapılan İşlemler";
			public static MultilingualString Desc225 => (NoResString)@"Geçici 23 Milli Eğitim Bakanlığına Yapılan Bilgisayar Bağışları İle İlgili Teslimler";
			public static MultilingualString Desc226 => (NoResString)@"17/2-b Özel Okulları, Üniversite ve Yüksekokullar Tarafından Verilen Bedelsiz Eğitim Ve Öğretim Hizmetleri";
			public static MultilingualString Desc227 => (NoResString)@"17/2-b Kanunların Gösterdiği Gerek Üzerine Bedelsiz Olarak Yapılan Teslim ve Hizmetler";
			public static MultilingualString Desc228 => (NoResString)@"17/2-b Kanunun (17/1) Maddesinde Sayılan Kurum ve Kuruluşlara Bedelsiz Olarak Yapılan Teslimler";
			public static MultilingualString Desc229 => (NoResString)@"17/2-b Gıda Bankacılığı Faaliyetinde Bulunan Dernek ve Vakıflara Bağışlanan Gıda, Temizlik, Giyecek ve Yakacak Maddeleri";
			public static MultilingualString Desc230 => (NoResString)@"17/4-g Külçe Altın, Külçe Gümüş Ve Kiymetli Taşlarin Teslimi";
			public static MultilingualString Desc231 => (NoResString)@"17/4-g Metal Plastik, Lastik, Kauçuk, Kağit, Cam Hurda Ve Atıkların Teslimi";
			public static MultilingualString Desc232 => (NoResString)@"17/4-g Döviz, Para, Damga Pulu, Değerli Kağıtlar, Hisse Senedi ve Tahvil Teslimleri";
			public static MultilingualString Desc234 => (NoResString)@"17/4-ş Konut Finansmanı Amacıyla Teminat Gösterilen ve İpotek Konulan Konutların Teslimi";
			public static MultilingualString Desc235 => (NoResString)@"16/1-c Transit ve Gümrük Antrepo Rejimleri İle Geçici Depolama ve Serbest Bölge Hükümlerinin Uygulandığiı Malların Teslimi";
			public static MultilingualString Desc236 => (NoResString)@"19/2 Usulüne Göre Yürürlüğe Girmiş Uluslararası Anlaşmalar Kapsamındaki İstisnalar (İade Hakkı Tanınmayan)";
			public static MultilingualString Desc237 => (NoResString)@"17/4-t 5300 Sayılı Kanuna Göre Düzenlenen Ürün Senetlerinin İhtisas/Ticaret Borsaları Aracılığıyla İlk Teslimlerinden Sonraki Teslim";
			public static MultilingualString Desc238 => (NoResString)@"17/4-u Varlıkların Varlık Kiralama Şirketlerine Devri İle Bu Varlıkların Varlık Kiralama Şirketlerince Kiralanması ve Devralınan Kuruma Devri";
			public static MultilingualString Desc239 => (NoResString)@"17/4-y Taşınmazların Finansal Kiralama Şirketlerine Devri, Finansal Kiralama Şirketi Tarafından Devredene Kiralanması ve Devri";
			public static MultilingualString Desc240 => (NoResString)@"17/4-z Patentli Veya Faydalı Model Belgeli Buluşa İlişkin Gayri Maddi Hakların Kiralanması, Devri ve Satışı";
			public static MultilingualString Desc241 => (NoResString)@"TürkAkım Gaz Boru Hattı Projesine İlişkin Anlaşmanın (9/b) Maddesinde Yer Alan Hizmetler";
			public static MultilingualString Desc242 => (NoResString)@"KDV 17/4-ö md. Gümrük Antrepoları, Geçici Depolama Yerleri ile Gümrüklü Sahalarda, İthalat ve İhracat İşlemlerine konu mallar ile transit rejim kapsamında işlem gören mallar için verilen ardiye, depolama ve terminal hizmetleri";
			public static MultilingualString Desc250 => (NoResString)@"Diğerleri (Kısmi İstisna)";
			public static MultilingualString Desc301 => (NoResString)@"11/1-a Mal İhracatı";
			public static MultilingualString Desc302 => (NoResString)@"11/1-a Hizmet İhracatı";
			public static MultilingualString Desc303 => (NoResString)@"11/1-a Roaming Hizmetleri";
			public static MultilingualString Desc304 => (NoResString)@"13/a Deniz Hava ve Demiryolu Taşıma Araçlarının Teslimi İle İnşa, Tadil, Bakım ve Onarımları";
			public static MultilingualString Desc305 => (NoResString)@"13/b Deniz ve Hava Taşıma Araçları İçin Liman Ve Hava Meydanlarında Yapılan Hizmetler";
			public static MultilingualString Desc306 => (NoResString)@"13/c Petrol Aramaları ve Petrol Boru Hatlarının İnşa ve Modernizasyonuna İlişkin Yapılan Teslim ve Hizmetler";
			public static MultilingualString Desc307 => (NoResString)@"13/c Maden Arama, Altın, Gümüş ve Platin Madenleri İçin İşletme, Zenginleştirme Ve Rafinaj Faaliyetlerine İlişkin Teslim Ve Hizmetler[KDVGUT-(II/8-4)]";
			public static MultilingualString Desc308 => (NoResString)@"13/d Teşvikli Yatırım Mallarının Teslimi";
			public static MultilingualString Desc309 => (NoResString)@"13/e Liman Ve Hava Meydanlarının İnşası, Yenilenmesi Ve Genişletilmesi";
			public static MultilingualString Desc310 => (NoResString)@"13/f Ulusal Güvenlik Amaçlı Teslim ve Hizmetler";
			public static MultilingualString Desc311 => (NoResString)@"14/1 Uluslararası Taşımacılık";
			public static MultilingualString Desc312 => (NoResString)@" 15/a Diplomatik Organ Ve Misyonlara Yapılan Teslim ve Hizmetler";
			public static MultilingualString Desc313 => (NoResString)@" 15/b Uluslararası Kuruluşlara Yapılan Teslim ve Hizmetler";
			public static MultilingualString Desc314 => (NoResString)@"19/2 Usulüne Göre Yürürlüğe Girmiş Uluslar Arası Anlaşmalar Kapsamındaki İstisnalar";
			public static MultilingualString Desc315 => (NoResString)@"14/3 İhraç Konusu Eşyayı Taşıyan Kamyon, Çekici ve Yarı Romorklara Yapılan Motorin Teslimleri";
			public static MultilingualString Desc316 => (NoResString)@"11/1-a Serbest Bölgelerdeki Müşteriler İçin Yapılan Fason Hizmetler";
			public static MultilingualString Desc317 => (NoResString)@"17/4-s Engellilerin Eğitimleri, Meslekleri ve Günlük Yaşamlarına İlişkin Araç-Gereç ve Bilgisayar Programları";
			public static MultilingualString Desc318 => (NoResString)@"Geçici 29 3996 Sayılı Kanuna Göre Gerçekleştirilecek Projeler ve 652 Sayılı Kanun Hükmünde Kararnameye Göre Kiralama Karşılığı Yaptırılan Eğitim Öğretim Tesislerine İlişkin Projelere İlişkin Teslim ve Hizmetler";
			public static MultilingualString Desc319 => (NoResString)@"13/g Başbakanlık Merkez Teşkilatına Yapılan Araç Teslimleri";
			public static MultilingualString Desc320 => (NoResString)@"Geçici 16 (6111 sayılı K.) İSMEP Kapsamında İstanbul İl Özel İdaresi'ne Bağlı Olarak Faaliyet Gösteren ""İstanbul Proje Koordinasyon Birim""ine Yapılacak Teslim ve Hizmetler";
			public static MultilingualString Desc321 => (NoResString)@"Geçici 26 Birleşmiş Milletler(BM) ile Kuzey Atlantik Antlaşması Teşkilatı(NATO) Temsilcilikleri ve Bu Teşkilatlara Bağlı Program, Fon ve Özel İhtisas Kuruluşları ile İktisadi İşbirliği ve Kalkınma Teşkilatına(OECD) Yapılacak Mal Teslimi ve Hizmet İfaları";
			public static MultilingualString Desc322 => (NoResString)@"11/1-a Türkiye'de İkamet Etmeyenlere Özel Fatura ile Yapılan Teslimler (Bavul Ticareti)";
			public static MultilingualString Desc323 => (NoResString)@"13/ğ 5300 Sayılı Kanuna Göre Düzenlenen Ürün Senetlerinin İhtisas/Ticaret Borsaları Aracılığıyla İlk Teslimi";
			public static MultilingualString Desc324 => (NoResString)@"13/h Türkiye Kızılay Derneğine Yapılan Teslim ve Hizmetler ile Türkiye Kızılay Derneğinin Teslim ve Hizmetleri";
			public static MultilingualString Desc325 => (NoResString)@"13/ı Yem Teslimleri";
			public static MultilingualString Desc326 => (NoResString)@"13/ı Gıda, Tarım ve Hayvancılık Bakanlığı Tarafından Tescil Edilmiş Gübrelerin Teslimi";
			public static MultilingualString Desc327 => (NoResString)@"13/ı Gıda, Tarım ve Hayvancılık Bakanlığı Tarafından Tescil Edilmiş Gübrelerin İçeriğinde Bulunan Hammaddelerin Gübre Üreticilerine Teslimi";
			public static MultilingualString Desc328 => (NoResString)@"13/i Konut veya İşyeri Teslimleri";
			public static MultilingualString Desc330 => (NoResString)@"KDV 13/j md. Organize Sanayi Bölgeleri ile Küçük Sanayi Sitelerinin İnşasına İlişkin Teslim ve Hizmetler";
			public static MultilingualString Desc331 => (NoResString)@"KDV 13/m md. Ar-Ge, Yenilik ve Tasarım Faaliyetlerinde Kullanılmak Üzere Yapılan Yeni Makina ve Teçhizat Teslimlerinde İstisna";
			public static MultilingualString Desc332 => (NoResString)@"KDV Geçici 39. Md. İmalat Sanayiinde Kullanılmak Üzere Yapılan Yeni Makina ve Teçhizat Teslimlerinde İstisna";
			public static MultilingualString Desc333 => (NoResString)@"KDV 13/k md. Kapsamında Genel ve Özel Bütçeli Kamu İdarelerine, İl Özel İdarelerine, Belediyelere ve Köylere bağışlanan Tesislerin İnşasına İlişkin İstisna";
			public static MultilingualString Desc334 => (NoResString)@"KDV 13/l md. Kapsamında Yabancılara Verilen Sağlık Hizmetlerinde İstisna";
			public static MultilingualString Desc335 => (NoResString)@"KDV 13/n Basılı Kitap ve Süreli Yayınların Teslimleri";
			public static MultilingualString Desc336 => (NoResString)@"Geçici 40 UEFA Müsabakaları Kapsamında Yapılacak Teslim ve Hizmetler";
			public static MultilingualString Desc337 => (NoResString)@"Türk Akım Gaz Boru Hattı Projesine İlişkin Anlaşmanın (9/h) Maddesi Kapsamındaki Gaz Taşıma Hizmetleri";
			public static MultilingualString Desc338 => (NoResString)@"İmalatçıların Mal İhracatları";
			public static MultilingualString Desc339 => (NoResString)@"İmalat Sanayii ile Turizme Yönelik Yatırım Teşvik Belgesi Kapsamındaki İnşaat İşlerine İlişkin Teslim ve Hizmetler";
			public static MultilingualString Desc340 => (NoResString)@"Elektrik Motorlu Taşıt Araçlarının Geliştirilmesine Yönelik Mühendislik Hizmetleri";
			public static MultilingualString Desc341 => (NoResString)@"Afetzedelere Bağışlanacak Konutların İnşasına İlişkin İstisna";
			public static MultilingualString Desc350 => (NoResString)@"Diğerleri";
			public static MultilingualString Desc351 => (NoResString)@"KDV - İstisna Olmayan Diğer";
			public static MultilingualString Desc601 => (NoResString)@"Yapim İşleri İle Bu İşlerle Birlikte İfa Edilen Mühendislik-Mimarlik Ve Etüt-Proje Hizmetleri";
			public static MultilingualString Desc602 => (NoResString)@"Etüt, Plan-Proje, Danişmanlik, Denetim Ve Benzeri Hizmetler";
			public static MultilingualString Desc603 => (NoResString)@"Makine, Teçhizat, Demirbaş Ve Taşitlara Ait Tadil, Bakim Ve Onarim Hizmetleri";
			public static MultilingualString Desc604 => (NoResString)@"Yemek Servis Hizmeti";
			public static MultilingualString Desc605 => (NoResString)@"Organizasyon Hizmeti";
			public static MultilingualString Desc606 => (NoResString)@"İşgücü Temin Hizmetleri";
			public static MultilingualString Desc607 => (NoResString)@"Özel Güvenlik Hizmeti";
			public static MultilingualString Desc608 => (NoResString)@"Yapi Denetim Hizmetleri";
			public static MultilingualString Desc609 => (NoResString)@"Fason Olarak Yaptirilan Tekstil Ve Konfeksiyon İşleri, Çanta Ve Ayakkabi Dikim İşleri Ve Bu İşlere Aracilik Hizmetleri";
			public static MultilingualString Desc610 => (NoResString)@"Turistik Mağazalara Verilen Müşteri Bulma / Götürme Hizmetleri";
			public static MultilingualString Desc611 => (NoResString)@"Spor Kulüplerinin Yayin, Reklâm Ve İsim Hakki Gelirlerine Konu İşlemleri";
			public static MultilingualString Desc612 => (NoResString)@"Temizlik Hizmeti";
			public static MultilingualString Desc613 => (NoResString)@"Çevre Ve Bahçe Bakim Hizmetleri";
			public static MultilingualString Desc614 => (NoResString)@"Servis Taşimaciliği Hizmeti";
			public static MultilingualString Desc615 => (NoResString)@"Her Türlü Baski Ve Basim Hizmetleri";
			public static MultilingualString Desc616 => (NoResString)@"Diğer Hizmetler [Kdvgut-(I/C-2.1.3.2.13)]";
			public static MultilingualString Desc617 => (NoResString)@"Hurda Metalden Elde Edilen Külçe Teslimleri";
			public static MultilingualString Desc618 => (NoResString)@"Hurda Metalden Elde Edilenler Dişindaki Bakir, Çinko Demir; Çelik Alüminyum Ve Kurşun Külçe Teslimleri [Kdvgut-(I/C-2.1.3.3.1)]";
			public static MultilingualString Desc619 => (NoResString)@"Bakir, Çinko Ve Alüminyum Ürünlerinin Teslimi";
			public static MultilingualString Desc620 => (NoResString)@"İstisnadan Vazgeçenlerin Hurda Ve Atik Teslimi";
			public static MultilingualString Desc621 => (NoResString)@"Metal, Plastik, Lastik, Kauçuk, Kâğit Ve Cam Hurda Ve Atiklardan Elde Edilen Hammadde Teslimi";
			public static MultilingualString Desc622 => (NoResString)@"Pamuk, Tiftik, Yün Ve Yapaği İle Ham Post Ve Deri Teslimleri";
			public static MultilingualString Desc623 => (NoResString)@"Ağaç Ve Orman Ürünleri Teslimi";
			public static MultilingualString Desc624 => (NoResString)@"Yük Taşimaciliği Hizmeti [Kdvgut-(I/C-2.1.3.2.11)]";
			public static MultilingualString Desc625 => (NoResString)@"Ticari Reklam Hizmetleri [Kdvgut-(I/C-2.1.3.2.15)]";
			public static MultilingualString Desc626 => (NoResString)@"Diğer Teslimler [Kdvgut-(I/C-2.1.3.3.7.)]";
			public static MultilingualString Desc627 => (NoResString)@"Demir-Çelik Ürünlerinin Teslimi [Kdvgut-(I/C-2.1.3.3.8)]";
			public static MultilingualString Desc801 => (NoResString)@"Yapım İşleri ile Bu İşlerle Birlikte İfa Edilen Mühendislik-Mimarlık ve Etüt-Proje Hizmetleri[KDVGUT-(I/C-2.1.3.2.1)]";
			public static MultilingualString Desc802 => (NoResString)@"Etüt, Plan-Proje, Danışmanlık, Denetim ve Benzeri Hizmetler[KDVGUT-(I/C-2.1.3.2.2)]";
			public static MultilingualString Desc803 => (NoResString)@"Makine, Teçhizat, Demirbaş ve Taşıtlara Ait Tadil, Bakım ve Onarım Hizmetleri[KDVGUT- (I/C-2.1.3.2.3)]";
			public static MultilingualString Desc804 => (NoResString)@"Yemek Servis Hizmeti[KDVGUT-(I/C-2.1.3.2.4)]";
			public static MultilingualString Desc805 => (NoResString)@"Organizasyon Hizmeti[KDVGUT-(I/C-2.1.3.2.4)]";
			public static MultilingualString Desc806 => (NoResString)@"İşgücü Temin Hizmetleri[KDVGUT-(I/C-2.1.3.2.5)]";
			public static MultilingualString Desc807 => (NoResString)@"Özel Güvenlik Hizmeti[KDVGUT-(I/C-2.1.3.2.5)]";
			public static MultilingualString Desc808 => (NoResString)@"Yapı Denetim Hizmetleri[KDVGUT-(I/C-2.1.3.2.6)]";
			public static MultilingualString Desc809 => (NoResString)@"Fason Olarak Yaptırılan Tekstil ve Konfeksiyon İşleri, Çanta ve Ayakkabı Dikim İşleri ve Bu İşlere Aracılık Hizmetleri[KDVGUT-(I/C-2.1.3.2.7)]";
			public static MultilingualString Desc810 => (NoResString)@"Turistik Mağazalara Verilen Müşteri Bulma/ Götürme Hizmetleri[KDVGUT-(I/C-2.1.3.2.8)]";
			public static MultilingualString Desc811 => (NoResString)@"Spor Kulüplerinin Yayın, Reklâm ve İsim Hakkı Gelirlerine Konu İşlemleri[KDVGUT-(I/C-2.1.3.2.9)]";
			public static MultilingualString Desc812 => (NoResString)@"Temizlik Hizmeti[KDVGUT-(I/C-2.1.3.2.10)]";
			public static MultilingualString Desc813 => (NoResString)@"Çevre ve Bahçe Bakım Hizmetleri[KDVGUT-(I/C-2.1.3.2.10)]";
			public static MultilingualString Desc814 => (NoResString)@"Servis Taşımacılığı Hizmeti[KDVGUT-(I/C-2.1.3.2.11)]";
			public static MultilingualString Desc815 => (NoResString)@"Her Türlü Baskı ve Basım Hizmetleri[KDVGUT-(I/C-2.1.3.2.12)]";
			public static MultilingualString Desc816 => (NoResString)@"Hurda Metalden Elde Edilen Külçe Teslimleri[KDVGUT-(I/C-2.1.3.3.1)]";
			public static MultilingualString Desc817 => (NoResString)@"Hurda Metalden Elde Edilenler Dışındaki Bakır, Çinko, Demir Çelik, Alüminyum ve Kurşun Külçe Teslimi [KDVGUT-(I/C-";
			public static MultilingualString Desc818 => (NoResString)@"Bakır, Çinko, Alüminyum ve Kurşun Ürünlerinin Teslimi[KDVGUT-(I/C-2.1.3.3.2)]";
			public static MultilingualString Desc819 => (NoResString)@"İstisnadan Vazgeçenlerin Hurda ve Atık Teslimi[KDVGUT-(I/C-2.1.3.3.3)]";
			public static MultilingualString Desc820 => (NoResString)@"Metal, Plastik, Lastik, Kauçuk, Kâğıt ve Cam Hurda ve Atıklardan Elde Edilen Hammadde Teslimi[KDVGUT-(I/C-2.1.3.3.4)]";
			public static MultilingualString Desc821 => (NoResString)@"Pamuk, Tiftik, Yün ve Yapağı İle Ham Post ve Deri Teslimleri[KDVGUT-(I/C-2.1.3.3.5)]";
			public static MultilingualString Desc822 => (NoResString)@"Ağaç ve Orman Ürünleri Teslimi[KDVGUT-(I/C-2.1.3.3.6)]";
			public static MultilingualString Desc823 => (NoResString)@"Yük Taşımacılığı Hizmeti [KDVGUT-(I/C-2.1.3.2.11)]";
			public static MultilingualString Desc824 => (NoResString)@"Ticari Reklam Hizmetleri [KDVGUT-(I/C-2.1.3.2.15)]";
			public static MultilingualString Desc825 => (NoResString)@"Demir-Çelik Ürünlerinin Teslimi [KDVGUT-(I/C-2.1.3.3.8)]";
		}

		#endregion

		#endregion

		public static HashSet<string> GetEInvoicingEligibleComplianceSubTypeList()
		{
			return new HashSet<string>
			{
				ComplianceSubTypeCodes.EAR,
				ComplianceSubTypeCodes.EIN,
				ComplianceSubTypeCodes.EIC,

				ComplianceSubTypeCodes.DAR,
				ComplianceSubTypeCodes.DIN,
			};
		}

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => GetEInvoicingEligibleComplianceSubTypeList();

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
		{
			return GetEInvoicingEligibleComplianceSubTypeList().Contains(complianceSubType);
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

		public static class EInvoiceProfileTypes
		{
			public const string TEMELFATURA = "TEMELFATURA";
			public const string TICARIFATURA = "TICARIFATURA";
			public const string EARSIVFATURA = "EARSIVFATURA";
		}

		public static class EInvoiceTaxCategoryConstants
		{
			public const string DefaultExemptionReasonCode = "351";
			public const string DefaultTaxSchemeName = "KDV";
			public const string DefaultTaxSchemeTaxTypeCode = "0015";
		}

		public static class EInvoiceInfoTypes
		{
			public const string ARInvoice = "SATIS";
			public const string APInvoice = "ALIS";
			public const string TaxExemption = "ISTISNA";
			public const string WithHoldingTax = "TEVKIFAT";
			public const string Return = "IADE";
		}

		#region Extra Tax

		protected override bool? HasExtraTaxInfo() => true;
		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("4B02294C-E5DF-4B59-BDF3-4033EFEC3BD5", "VAT Withholding Amt", "VAT Withholding Amount", "VAT Withholding Amount for Turkey");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("7BF5B098-4860-4E17-B636-9D5954815EF3", "VAT Withholding Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("1b008807-56de-48e0-9196-8c463e1ea557", "VAT Amt", "VAT Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("c0c70ece-ddc5-4a31-b7e8-3043ba37b5a0", "VAT Local");

		#endregion

		bool IComplianceSequenceValidationProvider.IsPrintingAuthorizationNumberLengthValid(string number) => true;

		bool IComplianceSequenceValidationProvider.IsPrintingAuthorizationNumberFormatValid(string number) => true;

		AccComplianceSequenceValidation IComplianceSequenceValidationProvider.GetAccComplianceSequenceValidation(AccComplianceSequence sequence)
			=> new AccComplianceSequenceTurkeyValidation(sequence);

		#region IComplianceSubTypeValidation

		ZString IComplianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader)
			=> IsPostedARCreditNoteWithChangedComplianceSubType(transactionHeader)
			&& IsAmendingTransaction(transactionHeader) && transactionHeader.AH_ComplianceSubType == ComplianceSubTypeCodes.ICN
			? ErrorMessageForSelectedICNComplianceSubType
			: ZString.Empty;

		bool IsPostedARCreditNoteWithChangedComplianceSubType(AccTransactionHeader transactionHeader)
			=> transactionHeader.IsInDatabase
			&& transactionHeader.IsARCreditNote
			&& transactionHeader.AH_ComplianceSubTypeInfo.HasChanges;

		bool IsAmendingTransaction(AccTransactionHeader transactionHeader)
			=> transactionHeader.AH_TransactionBelongsToGroup.IsValid && !transactionHeader.AH_IsCancelled;

		ZString IComplianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader)
		{
			return ZString.Empty;
		}

		ZString ErrorMessageForSelectedICNComplianceSubType => ResString.GetMultilingualString("E7E929FA-6E07-4B0C-ACCE-595716DD7AFA", "You cannot select ICN compliance sub type for Amending Credit Notes.");

		#endregion

		#region IOriginalInvoiceNumberAndDateValidationDecider

		bool IOriginalInvoiceNumberAndDateValidationDecider.ShouldValidateOriginalTransactionNumberAndDate(ZString complianceSubType)
			=> AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.Value
				&& (complianceSubType == ComplianceSubTypeCodes.DAR || complianceSubType == ComplianceSubTypeCodes.DIN);

		#endregion
	}
}
