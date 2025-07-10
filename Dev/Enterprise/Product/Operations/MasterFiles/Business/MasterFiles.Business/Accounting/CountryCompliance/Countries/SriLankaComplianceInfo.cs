using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.SriLankaOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class SriLankaComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider,
		IComplianceSubTypeTaxInvoiceRulePrecedenceProvider,
		IComplianceSubTypeTaxRegistrationTypePrecedenceProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.SriLanka;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.STX,
				ComplianceSubTypes.STC,
				ComplianceSubTypes.NTI,
				ComplianceSubTypes.NCR
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TCR = "TCR";
			public const string STX = "STX";
			public const string STC = "STC";
			public const string NTI = "NTI";
			public const string NCR = "NCR";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI => ResString.GetMultilingualString("LKComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString TCR => ResString.GetMultilingualString("LKComplianceSubTypeCodeList|TCR", "Tax Credit Note");
			public static MultilingualString STX => ResString.GetMultilingualString("LKComplianceSubTypeCodeList|STX", "Suspended Tax Invoice");
			public static MultilingualString STC => ResString.GetMultilingualString("LKComplianceSubTypeCodeList|STC", "Suspended Tax Credit Note");
			public static MultilingualString NTI => ResString.GetMultilingualString("LKComplianceSubTypeCodeList|NTI", "Invoice");
			public static MultilingualString NCR => ResString.GetMultilingualString("LKComplianceSubTypeCodeList|NCR", "Credit Note");
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "Tax Invoice";
			public const string TCR = "Tax Credit Note";
			public const string STX = "Suspended Tax Invoice";
			public const string STC = "Suspended Tax Credit Note";
			public const string NTI = "Invoice";
			public const string NCR = "Credit Note";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TXI = "Used for AR and AP Invoices with standard Tax IDs.";
			public const string TCR = "Used for AR and AP Credit Notes with standard Tax IDs.";
			public const string STX = "Used for AR and AP Invoices with Suspended Tax IDs.";
			public const string STC = "Used for AR and AP Credit Notes with Suspended Tax IDs.";
			public const string NTI = "Used for AR and AP Invoices that do not fall under the VAT or SVAT schemes, for example where no reportable supply was made or received or where the invoice recipient is not registered for VAT.";
			public const string NCR = "Used for AR and AP Credit Notes that do not fall under the VAT or SVAT schemes, for example where no reportable supply was made or received or where the invoice recipient is not registered for VAT.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR);
			public static ComplianceSubType STX => new ComplianceSubType(ComplianceSubTypeCodes.STX, () => ComplianceSubTypeDescriptions.STX, () => ComplianceSubTypeLocalDescriptions.STX, () => ComplianceSubTypeInternalImplemenationNote.STX);
			public static ComplianceSubType STC => new ComplianceSubType(ComplianceSubTypeCodes.STC, () => ComplianceSubTypeDescriptions.STC, () => ComplianceSubTypeLocalDescriptions.STC, () => ComplianceSubTypeInternalImplemenationNote.STC);
			public static ComplianceSubType NTI => new ComplianceSubType(ComplianceSubTypeCodes.NTI, () => ComplianceSubTypeDescriptions.NTI, () => ComplianceSubTypeLocalDescriptions.NTI, () => ComplianceSubTypeInternalImplemenationNote.NTI);
			public static ComplianceSubType NCR => new ComplianceSubType(ComplianceSubTypeCodes.NCR, () => ComplianceSubTypeDescriptions.NCR, () => ComplianceSubTypeLocalDescriptions.NCR, () => ComplianceSubTypeInternalImplemenationNote.NCR);
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

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.Default)
			{
				AddComplianceSubTypeAttributionRulesForDefault(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode)) //this is valid for UT and CountryComplianceInfoDisplayForm
			{
				AddComplianceSubTypeAttributionRulesForDefault(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("SriLankaComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefault(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.STX;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.SVATOrganizations;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.STX;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.SVATOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.STX;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.STX;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.SVATOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.STC;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.STC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.SVATOrganizations;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.STC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.SVATOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.STX;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRegisteredOrganizations;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRegisteredOrganizations;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRegisteredOrganizations;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRegisteredOrganizations;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.NTI;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NTI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.SpecificTaxIDs;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;
			configuration.TaxIDCode = "NOTREPORT";

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.SriLanka;
			configuration.SubType = ComplianceSubTypeCodes.NCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.SpecificTaxIDs;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;
			configuration.ParentTransactionSubType = ZString.Empty;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;
			configuration.TaxIDCode = "NOTREPORT";
		}

		#endregion

		#region IComplianceSubTypeTaxRegistrationTypeRuleProvider

		bool IComplianceSubTypeTaxRegistrationTypeRuleProvider.IsTaxRegistrationTypeRuleApplicable(IAccComplianceRule rule, OrgHeader header)
		{
			return rule.TaxRegistrationType.IsEmpty
				|| rule.TaxRegistrationType == TaxRegistrationTypeCodes.AllOrganizations
				|| (rule.TaxRegistrationType == TaxRegistrationTypeCodes.SVATOrganizations
						&& header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.SVT, CountryCode) != null)
				|| (rule.TaxRegistrationType == TaxRegistrationTypeCodes.NotRegisteredOrganizations
						&& header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, CountryCode) == null
						&& header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.SVT, CountryCode) == null);
		}

		CodeDescriptionPairList IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider.GetTaxRegistrationTypeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(TaxRegistrationTypeCodes.SVATOrganizations, TaxRegistrationTypeDescriptions.SVATOrganizations);
			list.AddPair(TaxRegistrationTypeCodes.AllOrganizations, TaxRegistrationTypeDescriptions.AllOrganizations);
			list.AddPair(TaxRegistrationTypeCodes.NotRegisteredOrganizations, TaxRegistrationTypeDescriptions.NotRegisteredOrganizations);
			return list;
		}

		#endregion

		ZString[] IComplianceSubTypeTaxInvoiceRulePrecedenceProvider.ComplianceSubTypeTaxInvoiceRulePrecedenceList()
		{
			return new ZString[]
			{
				TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly,
				TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly,
				TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount,
				TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax,
				TaxInvoiceRuleCodes.ContainsAnAmountOfTax,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID,
				TaxInvoiceRuleCodes.ContainsNoTaxIDs,
				TaxInvoiceRuleCodes.All,
				ZString.Empty
			};
		}

		ZString[] IComplianceSubTypeTaxRegistrationTypePrecedenceProvider.ComplianceSubTypeTaxRegistrationTypePrecedenceList()
		{
			return new ZString[]
			{
				TaxRegistrationTypeCodes.SVATOrganizations,
				TaxRegistrationTypeCodes.NotRegisteredOrganizations,
				TaxRegistrationTypeCodes.AllOrganizations,
				ZString.Empty
			};
		}

		#region SuppressResourceStringsCheckRegion

		public static class RuleSetCodes
		{
			public const string Default = "1";
		}

		static class RuleSetDescriptions
		{
			public const string Default = "TXI, TCR, STX, STC";
		}
		#endregion
	}
}
