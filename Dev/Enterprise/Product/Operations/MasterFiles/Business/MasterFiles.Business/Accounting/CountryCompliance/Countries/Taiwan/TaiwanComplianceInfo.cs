using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class TaiwanComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceSubTypeDependencyConfigurationProvider,
		IComplianceSubTypeTaxInvoiceRulePrecedenceProvider,
		IComplianceSubTypeRuleSortByTaxRegistrationLocation,
		IComplianceSubTypeRuleSortByParentTransactionSubType,
		IComplianceSubTypeTaxInvoiceRuleWithTaxIDAndZeroAmount,
		IComplianceSubTypeAndNumberUpdateRules,
		IComplianceSequenceValidationProvider,
		IEInvoicingRegistryProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Taiwan;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceSubTypeDependencyConfigurationProvider

		void IComplianceSubTypeDependencyConfigurationProvider.GetDefaults(ComplianceSubTypeDependencyConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.TDI;
			configuration.ParentSubType = ComplianceSubTypeCodes.TXI;
		}

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.NTC,
				ComplianceSubTypes.NTI,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCE,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TDC,
				ComplianceSubTypes.TDI,
				ComplianceSubTypes.TDP,
				ComplianceSubTypes.TSD,
				ComplianceSubTypes.TSX,
				ComplianceSubTypes.TXC,
				ComplianceSubTypes.TXE,
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TXP,
				ComplianceSubTypes.TXS,
				ComplianceSubTypes.XCL,
				ComplianceSubTypes.ZNG
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string NTC = "NTC";
			public const string NTI = "NTI";
			public const string TCD = "TCD";
			public const string TCE = "TCE";
			public const string TCR = "TCR";
			public const string TDC = "TDC";
			public const string TDI = "TDI";
			public const string TDP = "TDP";
			public const string TSD = "TSD";
			public const string TSX = "TSX";
			public const string TXC = "TXC";
			public const string TXE = "TXE";
			public const string TXI = "TXI";
			public const string TXP = "TXP";
			public const string TXS = "TXS";
			public const string XCL = "XCL";
			public const string ZNG = "ZNG";
		}

		public static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString NTC => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|NTC", "Non GUI Credit Note");
			public static MultilingualString NTI => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|NTI", "Non GUI Invoice");
			public static MultilingualString TCD => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TCD", "Duplicate Credit Note");
			public static MultilingualString TCE => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TCE", "Electronic Credit Note");
			public static MultilingualString TCR => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TCR", "Triplicate Credit Note");
			public static MultilingualString TDC => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TDC", "Duplicate Cash Register GUI");
			public static MultilingualString TDI => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TDI", "Duplicate Computerized GUI");
			public static MultilingualString TDP => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TDP", "Duplicate Handwritten GUI");
			public static MultilingualString TSD => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TSD", "Summary Duplicate GUI (For Input Tax Only)");
			public static MultilingualString TSX => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TSX", "Summary Triplicate GUI (For Input Tax Only)");
			public static MultilingualString TXC => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TXC", "Triplicate Cash Register GUI");
			public static MultilingualString TXE => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TXE", "Electronic GUI");
			public static MultilingualString TXI => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TXI", "Triplicate Computerized GUI");
			public static MultilingualString TXP => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TXP", "Triplicate Handwritten GUI");
			public static MultilingualString TXS => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|TXS", "Customs Levy GUI");
			public static MultilingualString XCL => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|XCL", "Disbursement / Reimbursement");
			public static MultilingualString ZNG => ResString.GetMultilingualString("TWComplianceSubTypeCodeList|ZNG", "Zero Rated - No GUI Issued");
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string NTC = "收據折讓";
			public const string NTI = "收據";
			public const string TCD = "二聯式折讓證明單";
			public const string TCE = "電子發票折讓";
			public const string TCR = "三聯式折讓證明單";
			public const string TDC = "二聯式收銀機統一發票";
			public const string TDI = "二聯式電子計算機統一發票";
			public const string TDP = "二聯式手開統一發票";
			public const string TSD = "彙總登錄每張稅額伍佰元以下之進項(二聯式)";
			public const string TSX = "彙總登錄每張稅額伍佰元以下之進項(三聯式)";
			public const string TXC = "三聯式收銀機統一發票";
			public const string TXE = "電子發票";
			public const string TXI = "三聯式電子計算機統一發票";
			public const string TXP = "三聯式手開統一發票";
			public const string TXS = "海關進口關稅";
			public const string XCL = "代收代付";
			public const string ZNG = "免開零稅率發票";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string NTC = "";
			public const string NTI = "";
			public const string TCD = "";
			public const string TCE = "";
			public const string TCR = "";
			public const string TDC = "TW document reporting category 32 in Sales, 22 purchases. Identifies document printed on pre-printed, pre-numbered, continuous roll \"cash register style\" paper stock. Used when reportable supply of services has been made to receivables organizations that are not VAT registered in Taiwan (e.g.. Individuals, foreign entities, GBRT Taiwan businesses). It is assumed that CW1 user base will be registered for VAT.";
			public const string TDI = "TW document reporting category 31 in Sales, 23 in purchases. Printed on a plain paper document or on pre-printed paper stock. Number is printed by the computer system. Used when reportable supply of services has been made to receivables organizations that are not VAT registered in Taiwan (e.g.. Individuals, foreign entities, GBRT Taiwan businesses). It is assumed that CW1 user base is registered for VAT. Shared number series when issued. Computerised Triplicate and Duplicate GUI are assigned numbers from the same Compliance Invoice Book series.";
			public const string TDP = "TW document reporting category 32 in Sales, 22 in purchases. Hand written on pre-printed, pre-numbered paper. Used when reportable supply of services has been made to receivables organizations that are not VAT registered in Taiwan (e.g.. Individuals, foreign entities, GBRT Taiwan businesses).";
			public const string TSD = "";
			public const string TSX = "";
			public const string TXC = "TW document reporting category 35 in Sales, 25 in purchases.Document printed on pre-printed, pre-numbered, continuous roll \"cash register style\" paper stock. Used when reportable supply of services has been made to TW VAT regostered organizations..";
			public const string TXE = "Used in Receivables when reportable supply of services has been made to a Taiwan VAT registered business. It is assumed that CW1 user base is registered for VAT";
			public const string TXI = "TW document reporting category 31 in Sales, 21 in purchases. Printed on a plain paper document or on pre-printed paper stock. Number is printed by the computer system. Used when reportable supply of services has been made to TW VAT registered organizations. Shared Number series when issued. Computerised Triplicate and Duplicate GUI are assigned numbers from the same Compliance Invoice Book series.";
			public const string TXP = "TW document reporting category 31 in Sales, 21 in purchases.";
			public const string TXS = "";
			public const string XCL = "";
			public const string ZNG = "";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType NTC => new ComplianceSubType(ComplianceSubTypeCodes.NTC, () => ComplianceSubTypeDescriptions.NTC, () => ComplianceSubTypeLocalDescriptions.NTC, () => ComplianceSubTypeInternalImplemenationNote.NTC);
			public static ComplianceSubType NTI => new ComplianceSubType(ComplianceSubTypeCodes.NTI, () => ComplianceSubTypeDescriptions.NTI, () => ComplianceSubTypeLocalDescriptions.NTI, () => ComplianceSubTypeInternalImplemenationNote.NTI);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD);
			public static ComplianceSubType TCE => new ComplianceSubType(ComplianceSubTypeCodes.TCE, () => ComplianceSubTypeDescriptions.TCE, () => ComplianceSubTypeLocalDescriptions.TCE, () => ComplianceSubTypeInternalImplemenationNote.TCE);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR);
			public static ComplianceSubType TDC => new ComplianceSubType(ComplianceSubTypeCodes.TDC, () => ComplianceSubTypeDescriptions.TDC, () => ComplianceSubTypeLocalDescriptions.TDC, () => ComplianceSubTypeInternalImplemenationNote.TDC);
			public static ComplianceSubType TDI => new ComplianceSubType(ComplianceSubTypeCodes.TDI, () => ComplianceSubTypeDescriptions.TDI, () => ComplianceSubTypeLocalDescriptions.TDI, () => ComplianceSubTypeInternalImplemenationNote.TDI);
			public static ComplianceSubType TDP => new ComplianceSubType(ComplianceSubTypeCodes.TDP, () => ComplianceSubTypeDescriptions.TDP, () => ComplianceSubTypeLocalDescriptions.TDP, () => ComplianceSubTypeInternalImplemenationNote.TDP);
			public static ComplianceSubType TSD => new ComplianceSubType(ComplianceSubTypeCodes.TSD, () => ComplianceSubTypeDescriptions.TSD, () => ComplianceSubTypeLocalDescriptions.TSD, () => ComplianceSubTypeInternalImplemenationNote.TSD);
			public static ComplianceSubType TSX => new ComplianceSubType(ComplianceSubTypeCodes.TSX, () => ComplianceSubTypeDescriptions.TSX, () => ComplianceSubTypeLocalDescriptions.TSX, () => ComplianceSubTypeInternalImplemenationNote.TSX);
			public static ComplianceSubType TXC => new ComplianceSubType(ComplianceSubTypeCodes.TXC, () => ComplianceSubTypeDescriptions.TXC, () => ComplianceSubTypeLocalDescriptions.TXC, () => ComplianceSubTypeInternalImplemenationNote.TXC);
			public static ComplianceSubType TXE => new ComplianceSubType(ComplianceSubTypeCodes.TXE, () => ComplianceSubTypeDescriptions.TXE, () => ComplianceSubTypeLocalDescriptions.TXE, () => ComplianceSubTypeInternalImplemenationNote.TXE);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI);
			public static ComplianceSubType TXP => new ComplianceSubType(ComplianceSubTypeCodes.TXP, () => ComplianceSubTypeDescriptions.TXP, () => ComplianceSubTypeLocalDescriptions.TXP, () => ComplianceSubTypeInternalImplemenationNote.TXP);
			public static ComplianceSubType TXS => new ComplianceSubType(ComplianceSubTypeCodes.TXS, () => ComplianceSubTypeDescriptions.TXS, () => ComplianceSubTypeLocalDescriptions.TXS, () => ComplianceSubTypeInternalImplemenationNote.TXS);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
			public static ComplianceSubType ZNG => new ComplianceSubType(ComplianceSubTypeCodes.ZNG, () => ComplianceSubTypeDescriptions.ZNG, () => ComplianceSubTypeLocalDescriptions.ZNG, () => ComplianceSubTypeInternalImplemenationNote.ZNG);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TXCTCR, RuleSetDescriptions.TXCTCR);
			rulesetList.AddPair(RuleSetCodes.TXETCE, RuleSetDescriptions.TXETCE);
			rulesetList.AddPair(RuleSetCodes.TXCTCRZNG, RuleSetDescriptions.TXCTCRZNG);
			rulesetList.AddPair(RuleSetCodes.TXETCEZNG, RuleSetDescriptions.TXETCEZNG);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.TXCTCR;
		}

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.TXCTCR)
			{
				AddComplianceSubTypeAttributionRulesForDefault(collection);
			}
			else if (ruleSetCode == RuleSetCodes.TXETCE)
			{
				AddComplianceSubTypeAttributionRulesForRuleTwo(collection);
			}
			else if (ruleSetCode == RuleSetCodes.TXCTCRZNG)
			{
				AddComplianceSubTypeAttributionRulesForRuleThree(collection);
			}
			else if (ruleSetCode == RuleSetCodes.TXETCEZNG)
			{
				AddComplianceSubTypeAttributionRulesForRuleFour(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode)) //this is valid for UT and CountryComplianceInfoDisplayForm
			{
				AddComplianceSubTypeAttributionRulesForDefault(collection);
				AddComplianceSubTypeAttributionRulesForRuleTwo(collection);
				AddComplianceSubTypeAttributionRulesForRuleThree(collection);
				AddComplianceSubTypeAttributionRulesForRuleFour(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("TaiwanComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefault(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			//NTC
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			//NTI
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			//TCD
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			//TCR
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			//TDP
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			//TXC
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			//XCL
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCR;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCR;
		}

		void AddComplianceSubTypeAttributionRulesForRuleTwo(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			//NTC
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			//NTI
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			//TCD
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			//TCE
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			//TDP
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			//TXE
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			//XCL
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCE;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCE;
		}

		void AddComplianceSubTypeAttributionRulesForRuleThree(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			//NTC
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			//NTI
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			//TCD
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			//TCR
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			//TDP
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			//TXC
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			//XCL
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;

			//ZNG
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.ZNG;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXCTCRZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXCTCRZNG;
		}

		void AddComplianceSubTypeAttributionRulesForRuleFour(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			//NTC
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			//NTI
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			//TCD
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			//TCE
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			//TDP
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			//TXE
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			//XCL
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = Constants.CountryCodes.Taiwan;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;

			//ZNG
			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.Taiwan;
			configuration.SubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.ZNG;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TXETCEZNG;
			configuration.RuleSetDescription = RuleSetDescriptions.TXETCEZNG;
		}

		#region SuppressResourceStringsCheckRegion

		public static class RuleSetCodes
		{
			public const string TXCTCR = "1";
			public const string TXETCE = "2";
			public const string TXCTCRZNG = "3";
			public const string TXETCEZNG = "4";
		}

		static class RuleSetDescriptions
		{
			public const string TXCTCR = "TXC, TCR, TDP, TCD";
			public const string TXETCE = "TXE, TCE, TDP, TCD";
			public const string TXCTCRZNG = "TXC, TCR, TDP, TCD, ZNG";
			public const string TXETCEZNG = "TXE, TCE, TDP, TCD, ZNG";
		}

		#endregion

		#endregion

		#region IComplianceSubTypeAndNumberUpdateRules

		bool IComplianceSubTypeAndNumberUpdateRules.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed => false;

		ZString IComplianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(AccTransactionHeader[] transactionHeaders) => ZString.Empty;

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(AccTransactionHeader transactionHeader) => true;

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceDocDateAllowed(AccTransactionHeader transactionHeader) => transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable;

		#endregion

		public static HashSet<string> GetEInvoicingEligibleComplianceSubTypeList()
		{
			return new HashSet<string>
			{
				ComplianceSubTypeCodes.TXE,
				ComplianceSubTypeCodes.TCE
			};
		}

		ZString[] IComplianceSubTypeTaxInvoiceRulePrecedenceProvider.ComplianceSubTypeTaxInvoiceRulePrecedenceList()
		{
			return new ZString[]
			{
				TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount,
				TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs,
				TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly,
				TaxInvoiceRuleCodes.ContainsAnAmountOfTax,
				TaxInvoiceRuleCodes.ContainsNoTaxIDs,
				TaxInvoiceRuleCodes.All,
				ZString.Empty
			};
		}

		bool IComplianceSubTypeTaxInvoiceRuleWithTaxIDAndZeroAmount.AllWithTaxIDAndZeroTaxAmount(IEnumerable<AccTransactionLines> lines)
		{
			return lines != null && lines.All(x => x.TaxRate != null && x.TaxRate.AT_Code == "FREEVAT");
		}

		bool IComplianceSequenceValidationProvider.IsPrintingAuthorizationNumberLengthValid(string number) => true;

		bool IComplianceSequenceValidationProvider.IsPrintingAuthorizationNumberFormatValid(string number) => true;

		AccComplianceSequenceValidation IComplianceSequenceValidationProvider.GetAccComplianceSequenceValidation(AccComplianceSequence sequence)
		{
			return new AccComplianceSequenceTaiwanValidation(sequence);
		}

		#region IEInvoicingRegistryProvider

		bool IEInvoicingRegistryProvider.ShouldAutoSetEReportingComplianceDate => true;

		#endregion
	}
}
