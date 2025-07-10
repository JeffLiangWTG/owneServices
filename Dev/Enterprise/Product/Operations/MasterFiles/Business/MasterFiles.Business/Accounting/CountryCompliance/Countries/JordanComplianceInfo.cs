using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class JordanComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IEInvoicingRegistryProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Jordan;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.GSTCode;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList { ComplianceSubTypes.TXI, ComplianceSubTypes.TXC, ComplianceSubTypes.NTI, ComplianceSubTypes.NTC };
		}

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplementationNote.TXI, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXC => new ComplianceSubType(ComplianceSubTypeCodes.TXC, () => ComplianceSubTypeDescriptions.TXC, () => ComplianceSubTypeLocalDescriptions.TXC, () => ComplianceSubTypeInternalImplementationNote.TXC, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType NTI => new ComplianceSubType(ComplianceSubTypeCodes.NTI, () => ComplianceSubTypeDescriptions.NTI, () => ComplianceSubTypeLocalDescriptions.NTI, () => ComplianceSubTypeInternalImplementationNote.NTI, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType NTC => new ComplianceSubType(ComplianceSubTypeCodes.NTC, () => ComplianceSubTypeDescriptions.NTC, () => ComplianceSubTypeLocalDescriptions.NTC, () => ComplianceSubTypeInternalImplementationNote.NTC, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TXC = "TXC";
			public const string NTI = "NTI";
			public const string NTC = "NTC";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI => ResString.GetMultilingualString("JOComplianceSubTypeCodeList|TXI", "Tax Invoice");

			public static MultilingualString TXC => ResString.GetMultilingualString("JOComplianceSubTypeCodeList|TXC", "Tax Credit Note");

			public static MultilingualString NTI => ResString.GetMultilingualString("JOComplianceSubTypeCodeList|NTI", "Non-Tax Invoice");

			public static MultilingualString NTC => ResString.GetMultilingualString("JOComplianceSubTypeCodeList|NTC", "Non-Tax Credit Note");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "الفاتورة الضريبية";
			public const string TXC = "إشعار الائتمان الضريبي";
			public const string NTI = "فاتورة غير ضريبية";
			public const string NTC = "مذكرة ائتمان غير ضريبية";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		static class ComplianceSubTypeInternalImplementationNote
		{
			public const string TXI = "Tax Original Invoice and Tax Invoice created from Original Tax Invoice as Amendment/Reversal.";
			public const string TXC = "Tax Credit Note created from Original Tax Invoice as Amendment/Reversal.";
			public const string NTI = "Non-Tax Original Invoice and Non-Tax Invoice created from Original Non-Tax Invoice as Amendment/Reversal.";
			public const string NTC = "Non-Tax Credit Note created from Original Non-Tax Invoice as Amendment/Reversal.";
		}

		#region IEInvoicingRegistryProvider

		bool IEInvoicingRegistryProvider.ShouldAutoSetEReportingComplianceDate => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => string.Empty;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Jordan;

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		public static class RuleSetCodes
		{
			public const string DefaultJordanRuleSet = "1";
			public const string JordanRuleSet2 = "2";
			public const string JordanRuleSet3 = "3";
			public const string JordanRuleSet4 = "4";
			public const string JordanRuleSet5 = "5";
			public const string JordanRuleSet6 = "6";
			public const string JordanRuleSet7 = "7";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Descriptions for compliance rule sets are not translated")]
		static class RuleSetDescriptions
		{
			public const string DefaultJordanRuleSet = "TXI(Tax Invoice)";
			public const string JordanRuleSet2 = "TXI(Tax Invoice Excluding Not Reportable and Excluded from Tax Base Charges)";
			public const string JordanRuleSet3 = "NTI(Non-Tax Invoice)";
			public const string JordanRuleSet4 = "NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice)";
			public const string JordanRuleSet5 = "NTI(Non-Tax Invoice plus Not Reportable Invoice, Excluded from Tax Base Invoice, and Exempt Invoice)";
			public const string JordanRuleSet6 = "ALL(Tax Invoice plus Non-Tax Invoice)";
			public const string JordanRuleSet7 = "ALL(Tax Invoice, Not Reportable Invoice, Excluded from Tax Base Invoice, plus Non-Tax Invoice)";
		}

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.DefaultJordanRuleSet, RuleSetDescriptions.DefaultJordanRuleSet);
			rulesetList.AddPair(RuleSetCodes.JordanRuleSet2, RuleSetDescriptions.JordanRuleSet2);
			rulesetList.AddPair(RuleSetCodes.JordanRuleSet3, RuleSetDescriptions.JordanRuleSet3);
			rulesetList.AddPair(RuleSetCodes.JordanRuleSet4, RuleSetDescriptions.JordanRuleSet4);
			rulesetList.AddPair(RuleSetCodes.JordanRuleSet5, RuleSetDescriptions.JordanRuleSet5);
			rulesetList.AddPair(RuleSetCodes.JordanRuleSet6, RuleSetDescriptions.JordanRuleSet6);
			rulesetList.AddPair(RuleSetCodes.JordanRuleSet7, RuleSetDescriptions.JordanRuleSet7);
			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet() => RuleSetCodes.DefaultJordanRuleSet;

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			switch (ruleSetCode)
			{
				case RuleSetCodes.DefaultJordanRuleSet:
					AddComplianceSubTypeAttributionRulesForDefaultJordanRuleSet(collection);
					break;
				case RuleSetCodes.JordanRuleSet2:
					AddComplianceSubTypeAttributionRulesForJordanRuleSet2(collection);
					break;
				case RuleSetCodes.JordanRuleSet3:
					AddComplianceSubTypeAttributionRulesForJordanRuleSet3(collection);
					break;
				case RuleSetCodes.JordanRuleSet4:
					AddComplianceSubTypeAttributionRulesForJordanRuleSet4(collection);
					break;
				case RuleSetCodes.JordanRuleSet5:
					AddComplianceSubTypeAttributionRulesForJordanRuleSet5(collection);
					break;
				case RuleSetCodes.JordanRuleSet6:
					AddComplianceSubTypeAttributionRulesForJordanRuleSet6(collection);
					break;
				case RuleSetCodes.JordanRuleSet7:
					AddComplianceSubTypeAttributionRulesForJordanRuleSet7(collection);
					break;
				case null:
				case "":
					AddComplianceSubTypeAttributionRulesForDefaultJordanRuleSet(collection);
					AddComplianceSubTypeAttributionRulesForJordanRuleSet2(collection);
					AddComplianceSubTypeAttributionRulesForJordanRuleSet3(collection);
					AddComplianceSubTypeAttributionRulesForJordanRuleSet4(collection);
					AddComplianceSubTypeAttributionRulesForJordanRuleSet5(collection);
					AddComplianceSubTypeAttributionRulesForJordanRuleSet6(collection);
					AddComplianceSubTypeAttributionRulesForJordanRuleSet7(collection);
					break;
				default:
					ErrorReporter.ReportOnce("JordanComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code : " + ruleSetCode);
					break;
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefaultJordanRuleSet(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//AR INV TID ALL -> TXI
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.DefaultJordanRuleSet;
			configuration.RuleSetDescription = RuleSetDescriptions.DefaultJordanRuleSet;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD TID ARO TXI -> TXC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.DefaultJordanRuleSet;
			configuration.RuleSetDescription = RuleSetDescriptions.DefaultJordanRuleSet;
			configuration.SubType = ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
		}

		void AddComplianceSubTypeAttributionRulesForJordanRuleSet2(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//AR INV TNE ALL -> TXI
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet2;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet2;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD TNE ARO TXI ->TXC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet2;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet2;
			configuration.SubType = ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
		}

		void AddComplianceSubTypeAttributionRulesForJordanRuleSet3(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//AR INV NON ALL -> NTI
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet3;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet3;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD NON ARO NTI -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet3;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet3;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;
		}

		void AddComplianceSubTypeAttributionRulesForJordanRuleSet4(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//AR INV EXL ALL -> NTI
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet4;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet4;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD EXL ARO TXI -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet4;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet4;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;

			//AR INV NOT ALL -> NTI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet4;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet4;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD NOT ARO NTI -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet4;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet4;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;

			//AR INV NON ALL -> NTI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet4;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet4;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD NOT ARO NTI -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet4;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet4;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;
		}

		void AddComplianceSubTypeAttributionRulesForJordanRuleSet5(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//AR INV EXL ALL -> NTI
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet5;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet5;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD EXL ARO NTI -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet5;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet5;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;

			//AR INV NOT ALL -> NTI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet5;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet5;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD NOT ARO NTI -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet5;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet5;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;

			//AR INV EXT ALL -> NTI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet5;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet5;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.AllWithExemptTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD EXT ARO NTI -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet5;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet5;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.AllWithExemptTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;

			//AR INV NON ALL -> NTI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet5;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet5;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD NON ARO NTI -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet5;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet5;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;
		}

		void AddComplianceSubTypeAttributionRulesForJordanRuleSet6(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//AR INV TID ALL -> TXI
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet6;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet6;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD TID ARO -> TXC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet6;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet6;
			configuration.SubType = ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR INV NON ALL -> NTI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet6;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet6;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD NON ARO -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet6;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet6;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
		}

		void AddComplianceSubTypeAttributionRulesForJordanRuleSet7(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//AR INV TNE ALL -> TXI
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet7;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet7;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD TNE ARO -> TXC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet7;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet7;
			configuration.SubType = ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR INV EXL ALL -> NTI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet7;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet7;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD EXL ARO -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet7;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet7;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR INV NOT ALL -> NTI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet7;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet7;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD NOT ARO -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet7;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet7;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR INV NON ALL -> NTI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet7;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet7;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;

			//AR CRD NON ARO -> NTC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.RuleSetCode = RuleSetCodes.JordanRuleSet7;
			configuration.RuleSetDescription = RuleSetDescriptions.JordanRuleSet7;
			configuration.SubType = ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsNoTaxIDs;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
		}

		#endregion

	}
}
