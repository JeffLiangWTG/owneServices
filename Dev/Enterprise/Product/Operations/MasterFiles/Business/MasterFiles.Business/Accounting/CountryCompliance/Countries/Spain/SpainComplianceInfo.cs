using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class SpainComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		ITaxMessagesGroupProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Spain;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.SpainCodeTypes.NIF;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.SpainCodeTypes.NIF;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => EnvProxy.Instance.IsProductionSystem ? new ZDate(2022, 7, 1) : new ZDate(2022, 4, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => (this as IComplianceInfoElectronicInvoicing).GetEInvoicingComplianceDate();

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Spain;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.XCL,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TCR = "TCR";
			public const string TCD = "TCD";
			public const string XCL = "XCL";
		}

		public static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI => ResString.GetMultilingualString("ESComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString TCR => ResString.GetMultilingualString("ESComplianceSubTypeCodeList|TCR", "Tax Credit Note");
			public static MultilingualString TCD => ResString.GetMultilingualString("ESComplianceSubTypeCodeList|TCD", "Tax Debit Note");
			public static MultilingualString XCL => ResString.GetMultilingualString("ESComplianceSubTypeCodeList|XCL", "Reimbursement / Disbursement / Excluded Supply");
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "Factura";
			public const string TCR = "Nota de Crédito";
			public const string TCD = "Nota de Débito";
			public const string XCL = "Factura de Suplido";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TXI = "Used in both Receivables and Payables to sub-classify original Invoice Transactions.";
			public const string TCR = "Used in both Receivables and Payables to sub-classify Amending Credit Note transactions. Credit Notes that don't amend or reverse an original transaction are not permitted in Spain.";
			public const string TCD = "Used in both Receivables and Payables to sub-classify Amending Invoice transactions.";
			public const string XCL = "Used in both Receivables and Payables to formally identify Invoice and Credit Note transactions recorded for disbursement / reimbursement purposes.  Should be used in conjunction with the EXCLUDE tax ID.";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TaxDocumentsAndExcludedSupply, RuleSetDescriptions.TaxDocumentsAndExcludedSupply);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.TaxDocumentsAndExcludedSupply;
		}

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.TaxDocumentsAndExcludedSupply)
			{
				AddComplianceSubTypeAttributionRulesForDefault(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode)) //this is valid for UT and CountryComplianceInfoDisplayForm
			{
				AddComplianceSubTypeAttributionRulesForDefault(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("TaiwanComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefault(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Spain;
			configuration.SubType = SpainComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Spain;
			configuration.SubType = SpainComplianceInfo.ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = SpainComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Spain;
			configuration.SubType = SpainComplianceInfo.ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = SpainComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Spain;
			configuration.SubType = SpainComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Spain;
			configuration.SubType = SpainComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = SpainComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Spain;
			configuration.SubType = SpainComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.ParentTransactionSubType = SpainComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = defaultCollection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Spain;
			configuration.SubType = SpainComplianceInfo.ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;
		}

		#region SuppressResourceStringsCheckRegion

		public static class RuleSetCodes
		{
			public const string TaxDocumentsAndExcludedSupply = "1";
		}

		static class RuleSetDescriptions
		{
			public const string TaxDocumentsAndExcludedSupply = "Tax Documents and Excluded Supply";
		}

		#endregion

		#endregion

		#region ITaxMessagesGroupProvider

		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.A, Description = TaxMessageGroupDescriptions.A, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.A },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.B, Description = TaxMessageGroupDescriptions.B, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.B },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.C, Description = TaxMessageGroupDescriptions.C, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.C },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.D, Description = TaxMessageGroupDescriptions.D, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.D },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E, Description = TaxMessageGroupDescriptions.E, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.F, Description = TaxMessageGroupDescriptions.F, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.F },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.G, Description = TaxMessageGroupDescriptions.G, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.G },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.H, Description = TaxMessageGroupDescriptions.H, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.H },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.I, Description = TaxMessageGroupDescriptions.I, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.I },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.J, Description = TaxMessageGroupDescriptions.J, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.J },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.K, Description = TaxMessageGroupDescriptions.K, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.K },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.L, Description = TaxMessageGroupDescriptions.L, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.L },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M, Description = TaxMessageGroupDescriptions.M, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.M },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.X, Description = TaxMessageGroupDescriptions.X, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.X },
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string A = "A";
			public const string B = "B";
			public const string C = "C";
			public const string D = "D";
			public const string E = "E";
			public const string F = "F";
			public const string G = "G";
			public const string H = "H";
			public const string I = "I";
			public const string J = "J";
			public const string K = "K";
			public const string L = "L";
			public const string M = "M";
			public const string X = "X";
		}

		public static class GovernmentTaxGroupCodes
		{
			public const string A = "01S1";
			public const string B = "01S2";
			public const string C = "08N2";
			public const string D = "02E1";
			public const string E = "02E2";
			public const string F = "02E3";
			public const string G = "01E4";
			public const string H = "01E6";
			public const string I = "01N1";
			public const string J = "01N2";
			public const string K = "10N1";
			public const string L = "09S1";
			public const string M = "12S1";
			public const string X = "XX";
		}

		#region SuppressResourceStringsCheckRegion

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString A => (NoResString)"Subject to IVA - Not Exempt";
			public static MultilingualString B => (NoResString)"Reverse Charge IVA (except Intracommunity purchases)";
			public static MultilingualString C => (NoResString)"Subject to IGIC/IPSI";
			public static MultilingualString D => (NoResString)"AR Only - Exempt - Export E1 (Art 20)";
			public static MultilingualString E => (NoResString)"AR Only - Exempt - Export E2 (Art 21)";
			public static MultilingualString F => (NoResString)"AR Only - Exempt - Export E3 (Art 22)";
			public static MultilingualString G => (NoResString)"AR Only - Exempt - E4 (Art 24)";
			public static MultilingualString H => (NoResString)"AR Only - Exempt - Other E6";
			public static MultilingualString I => (NoResString)"AR Only - Not Subject to IVA - Art 7 and 14";
			public static MultilingualString J => (NoResString)"AR Only - Not Subject to IVA - Location Rules";
			public static MultilingualString K => (NoResString)"AR Only - Collection on behalf of third parties";
			public static MultilingualString L => (NoResString)"AP Only - Intracommunity Purchases";
			public static MultilingualString M => (NoResString)"AP Only - Leasing operations of business premises";
			public static MultilingualString X => (NoResString)"Exclude from reporting";
		}

		#endregion

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => Array.Empty<string>();

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType) => false;

		#endregion
	}
}
