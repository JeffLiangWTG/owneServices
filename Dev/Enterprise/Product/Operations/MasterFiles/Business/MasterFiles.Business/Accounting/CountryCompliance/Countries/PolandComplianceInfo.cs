using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class PolandComplianceInfo : CountryComplianceInfo, IComplianceSubTypeCodeProvider, IComplianceSubTypeRulesWithMultipleRuleSetProvider, IComplianceInfoElectronicInvoicing
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Poland;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.PolandCodeTypes.PTU;
		protected override string GetConsumptionTaxCode() => OrgCusCode.PolandCodeTypes.PTU;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.PolandCodeTypes.NIP;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.DCR,
				ComplianceSubTypes.DSB,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TXI
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string DCR = "DCR";
			public const string DSB = "DSB";
			public const string TCD = "TCD";
			public const string TXI = "TXI";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString DCR => ResString.GetMultilingualString("PLComplianceSubTypeCodeList|DCR", "Disbursement Credit Note");
			public static MultilingualString DSB => ResString.GetMultilingualString("PLComplianceSubTypeCodeList|DSB", "Disbursement Invoice");
			public static MultilingualString TCD => ResString.GetMultilingualString("PLComplianceSubTypeCodeList|TCD", "Tax Debit Note");
			public static MultilingualString TXI => ResString.GetMultilingualString("PLComplianceSubTypeCodeList|TXI", "Tax Invoice");
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string DCR = "NOTA KREDYTOWA";
			public const string DSB = "NOTA OBCIĄŻENIOWA";
			public const string TCD = "FAKTURA KORYGUJĄCA";
			public const string TXI = "FAKTURA VAT";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string DCR = "Used in both Receivables and Payables to identify disbursement Credit Note transactions. This document type should never record an amount of VAT.";
			public const string DSB = "Used in both Receivables and Payables to identify disbursement Invoice transactions. This document type should never record an amount of VAT.";
			public const string TCD = "Used in both Receivables and Payables to issue an \"Invoice Correction\" document. Used to sub-classify amending/reversing Invoice (INV) and Credit Note (CRD).";
			public const string TXI = "Used in both Receivables and Payables to sub-classify original Invoice (INV) transaction.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType DCR => new ComplianceSubType(ComplianceSubTypeCodes.DCR, () => ComplianceSubTypeDescriptions.DCR, () => ComplianceSubTypeLocalDescriptions.DCR, () => ComplianceSubTypeInternalImplemenationNote.DCR);
			public static ComplianceSubType DSB => new ComplianceSubType(ComplianceSubTypeCodes.DSB, () => ComplianceSubTypeDescriptions.DSB, () => ComplianceSubTypeLocalDescriptions.DSB, () => ComplianceSubTypeInternalImplemenationNote.DSB);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.Default, RuleSetDescriptions.Default);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.Default;
		}

		public static class RuleSetCodes
		{
			public const string Default = "1";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		static class RuleSetDescriptions
		{
			public const string Default = "Default Poland Rule Set";
		}

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			switch (ruleSetCode)
			{
				case RuleSetCodes.Default:
					AddComplianceSubTypeAttributionRulesForDefault(collection);
					break;
				case null: //this is valid for UT and CountryComplianceInfoDisplayForm
				case "":
					AddComplianceSubTypeAttributionRulesForDefault(collection);
					break;
				default:
					ErrorReporter.ReportOnce("PolandComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
					break;
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefault(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			ComplianceSubTypeAttributionRuleConfiguration configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DSB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;
		}

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate()
			=> ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables()
			=> ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus()
			=> Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany)
			=> string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription()
			=> ResString.GetMultilingualString("093955e7-7683-4926-b815-cf5782b52660", "Pending User Action");

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName()
			=> AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType()
			=> AccTransactionHeaderAuthorisationRecordTypes.Poland;

		#endregion
	}
}
