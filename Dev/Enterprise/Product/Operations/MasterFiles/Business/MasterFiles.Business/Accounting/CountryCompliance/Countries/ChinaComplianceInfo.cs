using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ChinaComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceSubTypeTaxRegistrationTypeRuleProvider,
		IComplianceDocumentStatusProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IComplianceInfoEInvoicingGUIActionDocumentRequest,
		IComplianceInfoEInvoicingGUIActionProvider,
		IComplianceSubTypeAndNumberUpdateRules,
		IComplianceSubTypeValidation,
		IComplianceInfoEInvoicingGUIActionQueuePendingInvoice,
		IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider,
		IComplianceDocumentInfo,
		IEInvoicingRegistryProvider,
		IComplianceInfoEInvoicingGUIActionQueueReversedTransaction
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.China;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => true;

		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSVAT", "VAT Amt", "VAT Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalVAT", "VAT Local");

		#endregion

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#region IComplianceDocumentInfo

		public bool ShouldDisplayComplianceDocumentDate() => true;

		#endregion

		#region ComplianceSubTypeTaxRegistration

		bool IComplianceSubTypeTaxRegistrationTypeRuleProvider.IsTaxRegistrationTypeRuleApplicable(IAccComplianceRule rule, OrgHeader header)
		{
			return header != null &&
				((header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.China) != null
					&& ((rule.TaxRegistrationType == TaxRegistrationTypeCodes.Recoverable && IsTransactionOrganizationTaxRecoverable(header))
						|| (rule.TaxRegistrationType == TaxRegistrationTypeCodes.NotRecoverable && !IsTransactionOrganizationTaxRecoverable(header))))
				|| (rule.TaxRegistrationType == TaxRegistrationTypeCodes.Individual && header.OH_Category == OrgConstants.Category.NaturalPersonIndividual)
				|| (rule.TaxRegistrationType == TaxRegistrationTypeCodes.ForeignOrganisation && header.OH_Category == OrgConstants.Category.Business && header.MainAddress.OA_RN_NKCountryCode != CountryCode));
		}

		bool IsTransactionOrganizationTaxRecoverable(OrgHeader header) => header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.ChinaCodeTypes.VAG, Core.Constants.CountryCodes.China) != null;

		#endregion

		#region IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider

		CodeDescriptionPairList IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider.GetTaxRegistrationTypeList()
		{
			var list = new CodeDescriptionPairList();

			list.AddPair(TaxRegistrationTypeCodes.Individual, TaxRegistrationTypeDescriptions.Individual);
			list.AddPair(TaxRegistrationTypeCodes.ForeignOrganisation, TaxRegistrationTypeDescriptions.ForeignOrganisation);
			return list;
		}

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			var list = new ComplianceSubTypeList();
			list.Add(ComplianceSubTypes.ETA);
			list.Add(ComplianceSubTypes.ETB);
			list.Add(ComplianceSubTypes.FDA);
			list.Add(ComplianceSubTypes.FDB);
			list.Add(ComplianceSubTypes.TXA);
			list.Add(ComplianceSubTypes.TXB);
			return list;
		}

		public static class ComplianceSubTypeCodes
		{
			public const string ETA = "ETA";
			public const string ETB = "ETB";
			public const string TXA = "TXA";
			public const string TXB = "TXB";
			public const string FDA = "FDA";
			public const string FDB = "FDB";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString ETA => ResString.GetMultilingualString("CNComplianceSubTypeCodeList|ETA", "Electronic Special VAT Invoice");
			public static MultilingualString ETB => ResString.GetMultilingualString("CNComplianceSubTypeCodeList|ETB", "Electronic General VAT Invoice");
			public static MultilingualString TXA => ResString.GetMultilingualString("CNComplianceSubTypeCodeList|TXA", "Special VAT Invoice");
			public static MultilingualString TXB => ResString.GetMultilingualString("CNComplianceSubTypeCodeList|TXB", "General VAT Invoice");
			public static MultilingualString FDA => ResString.GetMultilingualString("CNComplianceSubTypeCodeList|FDA", "Fully-Digitalized Electronic Special  VAT Invoice");
			public static MultilingualString FDB => ResString.GetMultilingualString("CNComplianceSubTypeCodeList|FDB", "Fully-Digitalized Electronic General VAT Invoice");
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string ETA = "电子增值税专用发票";
			public const string ETB = "电子增值税普通发票";
			public const string TXA = "增值税专用发票";
			public const string TXB = "增值税普通发票";
			public const string FDA = "全电专用发票";
			public const string FDB = "全电普通发票";
		}

		static class ComplianceSubTypeInternalImplemenationNotes
		{
			public const string ETA = "This is an electronic version of the Special VAT Invoice.";
			public const string ETB = "This is an electronic version of the General VAT Invoice. When the Electronic General VAT Invoice is issued via the senders golden tax system, the receiving party will be able to view the invoice via their own golden tax system.";
			public const string TXA = "This Type of Tax Invoice is issued when both the Issuer and Recipient are General Tax Payers and taxable charges are taxed at a tax rate greater than 0% (non-zero rated). The recipient of this type of invoice will be able to claim an input tax credit.  An Organization is a \"General Tax Payer\" when both a CN VAT and a CN VAG Organisation Registration Code are recorded.";
			public const string TXB = "This Type of Tax Invoice is issued when the Recipient is a Small Scale Tax Payer regardless of the VAT tax rate applied OR when taxable charges are taxed at zero rate. An Organization is a \"Small Scale Tax Payer\" when both a CN VAT and a CN VAS Organisation Registration Code are recorded.";
			public const string FDA = "This is a fully-digitalized electronic version of the Special VAT Invoice.";
			public const string FDB = "This is a fully-digitalized electronic version of the General VAT Invoice.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType ETA => new ComplianceSubType(ComplianceSubTypeCodes.ETA, () => ComplianceSubTypeDescriptions.ETA, () => ComplianceSubTypeLocalDescriptions.ETA, () => ComplianceSubTypeInternalImplemenationNotes.ETA);
			public static ComplianceSubType ETB => new ComplianceSubType(ComplianceSubTypeCodes.ETB, () => ComplianceSubTypeDescriptions.ETB, () => ComplianceSubTypeLocalDescriptions.ETB, () => ComplianceSubTypeInternalImplemenationNotes.ETB);
			public static ComplianceSubType TXA => new ComplianceSubType(ComplianceSubTypeCodes.TXA, () => ComplianceSubTypeDescriptions.TXA, () => ComplianceSubTypeLocalDescriptions.TXA, () => ComplianceSubTypeInternalImplemenationNotes.TXA);
			public static ComplianceSubType TXB => new ComplianceSubType(ComplianceSubTypeCodes.TXB, () => ComplianceSubTypeDescriptions.TXB, () => ComplianceSubTypeLocalDescriptions.TXB, () => ComplianceSubTypeInternalImplemenationNotes.TXB);
			public static ComplianceSubType FDA => new ComplianceSubType(ComplianceSubTypeCodes.FDA, () => ComplianceSubTypeDescriptions.FDA, () => ComplianceSubTypeLocalDescriptions.FDA, () => ComplianceSubTypeInternalImplemenationNotes.FDA);
			public static ComplianceSubType FDB => new ComplianceSubType(ComplianceSubTypeCodes.FDB, () => ComplianceSubTypeDescriptions.FDB, () => ComplianceSubTypeLocalDescriptions.FDB, () => ComplianceSubTypeInternalImplemenationNotes.FDB);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TXATXB, RuleSetDescriptions.TXATXB);
			rulesetList.AddPair(RuleSetCodes.TXAETB, RuleSetDescriptions.TXAETB);
			rulesetList.AddPair(RuleSetCodes.TXATXBETB, RuleSetDescriptions.TXATXBETB);
			rulesetList.AddPair(RuleSetCodes.TXAETBTXB, RuleSetDescriptions.TXAETBTXB);
			rulesetList.AddPair(RuleSetCodes.TXATXBETA, RuleSetDescriptions.TXATXBETA);
			rulesetList.AddPair(RuleSetCodes.TXAETBETA, RuleSetDescriptions.TXAETBETA);
			rulesetList.AddPair(RuleSetCodes.TXATXBETAETB, RuleSetDescriptions.TXATXBETAETB);
			rulesetList.AddPair(RuleSetCodes.TXAETBETATXB, RuleSetDescriptions.TXAETBETATXB);
			rulesetList.AddPair(RuleSetCodes.ETATXB, RuleSetDescriptions.ETATXB);
			rulesetList.AddPair(RuleSetCodes.ETAETB, RuleSetDescriptions.ETAETB);
			rulesetList.AddPair(RuleSetCodes.ETATXBETB, RuleSetDescriptions.ETATXBETB);
			rulesetList.AddPair(RuleSetCodes.ETAETBTXB, RuleSetDescriptions.ETAETBTXB);
			rulesetList.AddPair(RuleSetCodes.ETATXBTXA, RuleSetDescriptions.ETATXBTXA);
			rulesetList.AddPair(RuleSetCodes.ETAETBTXA, RuleSetDescriptions.ETAETBTXA);
			rulesetList.AddPair(RuleSetCodes.ETATXBTXAETB, RuleSetDescriptions.ETATXBTXAETB);
			rulesetList.AddPair(RuleSetCodes.ETAETBTXATXB, RuleSetDescriptions.ETAETBTXATXB);
			rulesetList.AddPair(RuleSetCodes.FDAFDB, RuleSetDescriptions.FDAFDB);
			rulesetList.AddPair(RuleSetCodes.FDAFDBFDBForeignDebtor, RuleSetDescriptions.FDAFDBFDBForeignDebtor);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.TXATXB;
		}

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			switch (ruleSetCode)
			{
				case RuleSetCodes.TXATXB:
					AddComplianceSubTypeAttributionRulesForTXATXB(collection);
					break;
				case RuleSetCodes.TXAETB:
					AddComplianceSubTypeAttributionRulesForTXAETB(collection);
					break;
				case RuleSetCodes.TXATXBETB:
					AddComplianceSubTypeAttributionRulesForTXATXBETB(collection);
					break;
				case RuleSetCodes.TXAETBTXB:
					AddComplianceSubTypeAttributionRulesForTXAETBTXB(collection);
					break;
				case RuleSetCodes.TXATXBETA:
					AddComplianceSubTypeAttributionRulesForTXATXBETA(collection);
					break;
				case RuleSetCodes.TXAETBETA:
					AddComplianceSubTypeAttributionRulesForTXAETBETA(collection);
					break;
				case RuleSetCodes.TXATXBETAETB:
					AddComplianceSubTypeAttributionRulesForTXATXBETAETB(collection);
					break;
				case RuleSetCodes.TXAETBETATXB:
					AddComplianceSubTypeAttributionRulesForTXAETBETATXB(collection);
					break;
				case RuleSetCodes.ETATXB:
					AddComplianceSubTypeAttributionRulesForETATXB(collection);
					break;
				case RuleSetCodes.ETAETB:
					AddComplianceSubTypeAttributionRulesForETAETB(collection);
					break;
				case RuleSetCodes.ETATXBETB:
					AddComplianceSubTypeAttributionRulesForETATXBETB(collection);
					break;
				case RuleSetCodes.ETAETBTXB:
					AddComplianceSubTypeAttributionRulesForETAETBTXB(collection);
					break;
				case RuleSetCodes.ETATXBTXA:
					AddComplianceSubTypeAttributionRulesForETATXBTXA(collection);
					break;
				case RuleSetCodes.ETAETBTXA:
					AddComplianceSubTypeAttributionRulesForETAETBTXA(collection);
					break;
				case RuleSetCodes.ETATXBTXAETB:
					AddComplianceSubTypeAttributionRulesForETATXBTXAETB(collection);
					break;
				case RuleSetCodes.ETAETBTXATXB:
					AddComplianceSubTypeAttributionRulesForETAETBTXATXB(collection);
					break;
				case RuleSetCodes.FDAFDB:
					AddComplianceSubTypeAttributionRulesForFDAFDB(collection);
					break;
				case RuleSetCodes.FDAFDBFDBForeignDebtor:
					AddComplianceSubTypeAttributionRulesForFDAFDBFDBForeignDebtor(collection);
					break;
				case null: //this is valid for UT and CountryComplianceInfoDisplayForm
				case "":
					AddComplianceSubTypeAttributionRulesForTXATXB(collection);
					AddComplianceSubTypeAttributionRulesForTXAETB(collection);
					AddComplianceSubTypeAttributionRulesForTXATXBETB(collection);
					AddComplianceSubTypeAttributionRulesForTXAETBTXB(collection);
					AddComplianceSubTypeAttributionRulesForTXATXBETA(collection);
					AddComplianceSubTypeAttributionRulesForTXAETBETA(collection);
					AddComplianceSubTypeAttributionRulesForTXATXBETAETB(collection);
					AddComplianceSubTypeAttributionRulesForTXAETBETATXB(collection);
					AddComplianceSubTypeAttributionRulesForETATXB(collection);
					AddComplianceSubTypeAttributionRulesForETAETB(collection);
					AddComplianceSubTypeAttributionRulesForETATXBETB(collection);
					AddComplianceSubTypeAttributionRulesForETAETBTXB(collection);
					AddComplianceSubTypeAttributionRulesForETATXBTXA(collection);
					AddComplianceSubTypeAttributionRulesForETAETBTXA(collection);
					AddComplianceSubTypeAttributionRulesForETATXBTXAETB(collection);
					AddComplianceSubTypeAttributionRulesForETAETBTXATXB(collection);
					AddComplianceSubTypeAttributionRulesForFDAFDB(collection);
					AddComplianceSubTypeAttributionRulesForFDAFDBFDBForeignDebtor(collection);
					break;
				default:
					ErrorReporter.ReportOnce("ChinaComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
					break;
			}
		}

		#region Add Configurations

		void AddComplianceSubTypeAttributionRulesForTXATXB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.TXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXB;
		}

		void AddComplianceSubTypeAttributionRulesForTXAETB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.TXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETB;
		}

		void AddComplianceSubTypeAttributionRulesForTXATXBETB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETB;
		}

		void AddComplianceSubTypeAttributionRulesForTXAETBTXB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.TXAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBTXB;
		}

		void AddComplianceSubTypeAttributionRulesForTXATXBETA(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETA;
		}

		void AddComplianceSubTypeAttributionRulesForTXAETBETA(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETA;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETA;
		}

		void AddComplianceSubTypeAttributionRulesForTXATXBETAETB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXATXBETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXATXBETAETB;
		}

		void AddComplianceSubTypeAttributionRulesForTXAETBETATXB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.TXAETBETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.TXAETBETATXB;
		}

		void AddComplianceSubTypeAttributionRulesForETATXB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.ETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXB;
		}

		void AddComplianceSubTypeAttributionRulesForETAETB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.ETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETB;
		}

		void AddComplianceSubTypeAttributionRulesForETATXBETB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.ETATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBETB;
		}

		void AddComplianceSubTypeAttributionRulesForETAETBTXB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXB;
		}

		void AddComplianceSubTypeAttributionRulesForETATXBTXA(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXA;
		}

		void AddComplianceSubTypeAttributionRulesForETAETBTXA(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXA;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXA;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXA;
		}

		void AddComplianceSubTypeAttributionRulesForETATXBTXAETB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXAETB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETATXBTXAETB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETATXBTXAETB;
		}

		void AddComplianceSubTypeAttributionRulesForETAETBTXATXB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.ETB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXATXB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ComplianceSubTypeCodes.TXB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.ETAETBTXATXB;
			configuration.RuleSetDescription = RuleSetDescriptions.ETAETBTXATXB;
		}

		void AddComplianceSubTypeAttributionRulesForFDAFDB(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDB;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDB;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDB;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.FDAFDB;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDB;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDB;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDB;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDB;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDB;
		}

		void AddComplianceSubTypeAttributionRulesForFDAFDBFDBForeignDebtor(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDBFDBForeignDebtor;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDBFDBForeignDebtor;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDBFDBForeignDebtor;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDBFDBForeignDebtor;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDBFDBForeignDebtor;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDBFDBForeignDebtor;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Individual;
			configuration.RuleSetCode = RuleSetCodes.FDAFDBFDBForeignDebtor;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDBFDBForeignDebtor;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.ForeignOrganisation;
			configuration.RuleSetCode = RuleSetCodes.FDAFDBFDBForeignDebtor;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDBFDBForeignDebtor;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDA;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDBFDBForeignDebtor;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDBFDBForeignDebtor;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDBFDBForeignDebtor;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDBFDBForeignDebtor;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.China;
			configuration.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.NotRecoverable;
			configuration.RuleSetCode = RuleSetCodes.FDAFDBFDBForeignDebtor;
			configuration.RuleSetDescription = RuleSetDescriptions.FDAFDBFDBForeignDebtor;
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public static class RuleSetCodes
		{
			public const string TXATXB = "1";
			public const string TXAETB = "2";
			public const string TXATXBETB = "3";
			public const string TXAETBTXB = "4";
			public const string TXATXBETA = "5";
			public const string TXAETBETA = "6";
			public const string TXATXBETAETB = "7";
			public const string TXAETBETATXB = "8";
			public const string ETATXB = "9";
			public const string ETAETB = "10";
			public const string ETATXBETB = "11";
			public const string ETAETBTXB = "12";
			public const string ETATXBTXA = "13";
			public const string ETAETBTXA = "14";
			public const string ETATXBTXAETB = "15";
			public const string ETAETBTXATXB = "16";
			public const string FDAFDB = "17";
			public const string FDAFDBFDBForeignDebtor = "18";
		}

		static class RuleSetDescriptions
		{
			public const string TXATXB = "TXA, TXB";
			public const string TXAETB = "TXA, ETB";
			public const string TXATXBETB = "TXA, TXB, ETB";
			public const string TXAETBTXB = "TXA, ETB, TXB";
			public const string TXATXBETA = "TXA, TXB, ETA";
			public const string TXAETBETA = "TXA, ETB, ETA";
			public const string TXATXBETAETB = "TXA, TXB, ETA, ETB";
			public const string TXAETBETATXB = "TXA, ETB, ETA, TXB";
			public const string ETATXB = "ETA, TXB";
			public const string ETAETB = "ETA, ETB";
			public const string ETATXBETB = "ETA, TXB, ETB";
			public const string ETAETBTXB = "ETA, ETB, TXB";
			public const string ETATXBTXA = "ETA, TXB, TXA";
			public const string ETAETBTXA = "ETA, ETB, TXA";
			public const string ETATXBTXAETB = "ETA, TXB, TXA, ETB";
			public const string ETAETBTXATXB = "ETA, ETB, TXA, TXB";
			public const string FDAFDB = "FDA, FDB";
			public const string FDAFDBFDBForeignDebtor = "FDA, FDB, FDB Foreign Debtor";
		}

		#endregion

		#endregion

		#region IComplianceDocumentStatusProvider

		public class ComplianceDocumentStatusTypes : CodeDescriptionPairList
		{
			public ComplianceDocumentStatusTypes()
			{
				Add(CDD);
				Add(CDN);
				Add(CDF);
				Add(CDP);
				Add(CDI);
				Add(CDV);
				Add(CDR);
				Add(CRR);
			}

			public static CodeDescriptionPair CDD => new CodeDescriptionPair("CDD", ResString.GetMultilingualString("2E4776E7-9A92-481E-A3A8-EDB32E71D39E", "Compliance Document Record Deleted"));

			public static CodeDescriptionPair CDN => new CodeDescriptionPair("CDN", ResString.GetMultilingualString("F3C1F87B-04E9-4891-B156-7B4309C404E1", "Compliance Document Not Issued"));

			public static CodeDescriptionPair CDF => new CodeDescriptionPair("CDF", ResString.GetMultilingualString("4F8D4D96-081E-4537-B3C2-357EF0E4D619", "Compliance Document Processed With Error"));

			public static CodeDescriptionPair CDP => new CodeDescriptionPair("CDP", ResString.GetMultilingualString("4CDB2722-054F-45AC-B2A8-DA2999939F7F", "Compliance Document Processing"));

			public static CodeDescriptionPair CDI => new CodeDescriptionPair("CDI", ResString.GetMultilingualString("7A3BB577-F017-4CDD-B341-16FC20865078", "Compliance Document Issued"));

			public static CodeDescriptionPair CDV => new CodeDescriptionPair("CDV", ResString.GetMultilingualString("8BBA2CAC-D001-4AFC-A045-F90131162C66", "Compliance Document Voided"));

			public static CodeDescriptionPair CDR => new CodeDescriptionPair("CDR", ResString.GetMultilingualString("718F2100-ADCA-4492-9EC9-A4297841076E", "Compliance Document Credited"));

			public static CodeDescriptionPair CRR => new CodeDescriptionPair("CRR", ResString.GetMultilingualString("DB8104CB-C49D-48ED-8FE9-FAA8F1C2ADD5", "Compliance document is reversed due to replacement"));
		}

		protected IEnumerable<ZString> GetCanReverseComplianceDocumentStatusCodes()
		{
			return new ZString[] { ComplianceDocumentStatusTypes.CDD.Code, ComplianceDocumentStatusTypes.CDV.Code, ComplianceDocumentStatusTypes.CDR.Code };
		}

		public static IEnumerable<ZString> GetNeedCheckVoidedAndCreditedAmountStatusCodes()
		{
			return new ZString[] { ComplianceDocumentStatusTypes.CDV.Code, ComplianceDocumentStatusTypes.CDR.Code };
		}

		ICodeDescriptionPairList IComplianceDocumentStatusProvider.GetComplianceDocumentStatusTypes()
		{
			return new ComplianceDocumentStatusTypes();
		}

		string IComplianceDocumentStatusProvider.GetComplianceDocumentStatus(string statusType)
		{
			var statusTypes = ((IComplianceDocumentStatusProvider)(this)).GetComplianceDocumentStatusTypes() as CodeDescriptionPairList;
			var invoiceStatusType = statusTypes?[statusType] as CodeDescriptionPair;
			return invoiceStatusType?.CodeAndDescription;
		}

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		static readonly ImmutableHashSet<string> EligibleComplianceSubTypeListForEInvoicing = new[]
		{
			ComplianceSubTypeCodes.ETA,
			ComplianceSubTypeCodes.ETB,
			ComplianceSubTypeCodes.TXA,
			ComplianceSubTypeCodes.TXB,
			ComplianceSubTypeCodes.FDA,
			ComplianceSubTypeCodes.FDB,
		}.ToImmutableHashSet();

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => EligibleComplianceSubTypeListForEInvoicing;

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType) => EligibleComplianceSubTypeListForEInvoicing.Contains(complianceSubType);

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

		#region IComplianceSubTypeAndNumberUpdateRules

		bool IComplianceSubTypeAndNumberUpdateRules.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed => false;

		ZString IComplianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(AccTransactionHeader[] transactionHeaders)
		{
			var aRInvoices = transactionHeaders.Where(x => x.AH_Ledger == LedgerTypes.AccountsReceivable && x.AH_TransactionType == TransactionTypes.Invoice).ToList();
			var result = ZString.Empty;

			if (aRInvoices.Count == 0)
			{
				result = ZString.Empty;
			}
			else if (aRInvoices.Count == 1)
			{
				result = GetErrorMessageForARComplianceSubTypeAndNumberUpdateForTransaction(aRInvoices.FirstOrDefault());
			}
			else
			{
				foreach (var aRInvoice in aRInvoices)
				{
					var errorMessage = GetErrorMessageForARComplianceSubTypeAndNumberUpdateForTransaction(aRInvoice);

					if (!errorMessage.IsEmpty)
					{
						if (!result.IsEmpty)
						{
							result += "\r\n";
						}

						result += $"AR Invoice {aRInvoice.AH_TransactionNum}:\r\n{errorMessage}";
					}
				}
			}

			return result;
		}

		ZString GetErrorMessageForARComplianceSubTypeAndNumberUpdateForTransaction(AccTransactionHeader transactionHeader)
		{
			var enableEInvoicingFunctionality = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);

			if (enableEInvoicingFunctionality)
			{
				var hasClientNumber = !string.IsNullOrEmpty(AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(transactionHeader.Company.PK.ToGuid(), transactionHeader.Branch.PK.ToGuid(), Guid.Empty));
				if (hasClientNumber && transactionHeader.AH_SystemCreateTimeUtc > AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty))
				{
					var isStatusValidForUpdate = CheckStatusIsValidForUpdateComplianceSubTypeAndNumber(transactionHeader);
					if (isStatusValidForUpdate)
					{
						return Res.GetString("47449D57-5271-4A9B-96ED-B39C430F866A", "You cannot update the Compliance Sub Type as the Invoice is currently in the process of E-Reporting.");
					}
				}
			}

			return GetTaxIdAndVATNumberErrorMessage(transactionHeader);
		}

		ZString GetTaxIdAndVATNumberErrorMessage(AccTransactionHeader transactionHeader)
		{
			var result = ZString.Empty;
			var debtor = transactionHeader.Factory.Load<OrgHeader>(transactionHeader.AH_OH);
			var hasTaxId = HasTaxId(transactionHeader);

			if (hasTaxId)
			{
				var hasVATNumber = transactionHeader.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.China) != null;

				if ((debtor?.OH_Category ?? ZString.Empty) == OrgConstants.Category.Business && !hasVATNumber && debtor.MainAddress.OA_RN_NKCountryCode == Core.Constants.CountryCodes.China)
				{
					result = Res.GetString("AD5A1FB3-E5FF-498F-9B9D-F334ECDB2333", "You cannot update the compliance sub type as this invoice debtor does not have a valid VAT registration number.");
				}
			}
			else
			{
				result = Res.GetString("BB8B2552-8385-42CE-91EC-C2DF479E5814", "You cannot update the compliance sub type as this is a non tax invoice.");
			}

			return result;
		}

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(AccTransactionHeader transactionHeader) => !NotAllowUpdateComplianceDocDateAndComplianceNumber(transactionHeader);

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceDocDateAllowed(AccTransactionHeader transactionHeader) => transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && !NotAllowUpdateComplianceDocDateAndComplianceNumber(transactionHeader);

		bool NotAllowUpdateComplianceDocDateAndComplianceNumber(AccTransactionHeader transactionHeader) => transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable
			&& transactionHeader.AH_TransactionType == TransactionTypes.Invoice
			&& AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty)
			&& !string.IsNullOrEmpty(AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(transactionHeader.Company.PK.ToGuid(), transactionHeader.Branch.PK.ToGuid(), Guid.Empty))
			&& transactionHeader.AH_SystemCreateTimeUtc > AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.GetFallBackValueAtAllLevels(transactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty)
			&& transactionHeader.Factory.Exists(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AccEInvoicingTransactionPivotSchema.Constants.Prefix), new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionHeader.PK), false);

		bool CheckStatusIsValidForUpdateComplianceSubTypeAndNumber(AccTransactionHeader transactionHeader)
		{
			var allowedStatus = new[] { EInvoicingPivotState.Discarded, EInvoicingPivotState.Pending, EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Failed };
			var pivotType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AccEInvoicingTransactionPivotSchema.Constants.Prefix);
			var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionHeader.PK);
			query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.Submit);
			query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, allowedStatus);

			return transactionHeader.Factory.Exists(pivotType, query, false);
		}

		#endregion

		#region IComplianceSubTypeValidation

		ZString IComplianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader)
		{
			var result = ZString.Empty;

			if (transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && transactionHeader.AH_TransactionType == TransactionTypes.Invoice)
			{
				var hasTaxId = HasTaxId(transactionHeader);

				if (hasTaxId)
				{
					if ((transactionHeader.Header?.OH_Category ?? ZString.Empty) == OrgConstants.Category.Business)
					{
						var hasVATNumber = transactionHeader.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.China) != null;
						var nonAvailableSubType = new List<string> { ComplianceSubTypeCodes.TXA, ComplianceSubTypeCodes.ETA, ComplianceSubTypeCodes.FDA };
						var containsNonAvailableSubType = nonAvailableSubType.Contains(transactionHeader.AH_ComplianceSubType);
						var debtor = transactionHeader.Factory.Load<OrgHeader>(transactionHeader.AH_OH);
						if (hasVATNumber)
						{
							if (transactionHeader.AH_GSTAmount == 0 && containsNonAvailableSubType)
							{
								result = Res.GetString("AAF081F4-2743-4E70-A7A8-4524664F02B0", "You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as the invoice tax amount is zero.");
							}
						}
						else if (debtor.MainAddress.OA_RN_NKCountryCode != Core.Constants.CountryCodes.China && containsNonAvailableSubType)
						{
							result = Res.GetString("650F5CC0-A020-41B8-9648-848A30D67374", "You cannot update the compliance sub type to 'ETA', 'TXA' or 'FDA', as this invoice debtor does not have a valid VAT registration number.");
						}
						else if (debtor.MainAddress.OA_RN_NKCountryCode == Core.Constants.CountryCodes.China && !string.IsNullOrEmpty(transactionHeader.AH_ComplianceSubType))
						{
							result = Res.GetString("270BBB77-5032-49DA-B936-3E2EC9C10045", "You cannot assign a compliance sub type to the invoice as this invoice debtor does not have a valid VAT registration number.");
						}
					}
				}
				else if (!string.IsNullOrEmpty(transactionHeader.AH_ComplianceSubType))
				{
					result = Res.GetString("9934E8FC-9A1D-4AA4-8818-CB3765C5D127", "You cannot assign a compliance sub type to the invoice as this is a non tax invoice.");
				}
			}

			return result;
		}

		ZString IComplianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader)
		{
			return ZString.Empty;
		}

		public IEnumerable<AccTransactionHeader> GetEligibleInvoices(IEnumerable<AccTransactionHeader> selectedTransactions)
		{
			return selectedTransactions.Where(x => !x.AH_IsCancelled
													&& x.AH_Ledger == LedgerTypes.AccountsReceivable
													&& x.AH_TransactionType == TransactionTypes.Invoice
													&& EligibleComplianceSubTypeListForEInvoicing.Contains(x.AH_ComplianceSubType)).ToList();
		}

		public bool ExistActiveDocumentRequestPivot(ZDateTime lastSentTime)
		{
			return lastSentTime > ZDateTime.UtcNow.AddHours(-1);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionDocumentRequest

		public ZString DocumentRequestMenuName => Res.GetString("e816fd79-27d7-45b9-9741-c7300b70ebfc", "Request e-Invoice Status");

		public ZString DocumentRequestActionInformation => Res.GetString("4c1d6207-7d33-4d14-ad55-281c60a6ecbe", @"Your request is being processed.
Please Note:
- A new request is allowed where response from an existing request has not been received after 60 minutes from request submission.");

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProvider

		public bool IsCountryEnableComplianceEInvoicing(bool isAPTransaction = false) => !isAPTransaction && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		public string NotEligibleForRequestsMessage => Res.GetString("6be6e62a-06d5-4922-96de-b4d3ffc58a15", @"The transaction is not eligible for requests due to one of the following conditions:
1. The E-Reporting Status of the transaction is not equal to SUC.");

		#endregion

		#region IComplianceInfoEInvoicingGUIActionQueuePendingInvoice

		public ZString QueuePendingInvoiceMenuName => Res.GetString("D7FF9009-BEA8-4572-A9D8-56219825F482", "Authorize And Send");

		public ZString[] PivotStatusesEligibleForQueuing => new ZString[] { EInvoicingPivotState.Pending };

		public ZString PivotStatusesEligibleForQueuingErrorMessage => Res.GetString("DA84A4EF-94E0-46BF-B889-9E7AAEFFDD29", "You can only Authorize and Send transactions where the E-Reporting status is 'PEN - Pending'.");

		#endregion

		public static Dictionary<ZString, ZString> ComplianceSubTypeDictionary => new Dictionary<ZString, ZString>()
		{
			{ "01", "TXA" },
			{ "02", "TXB" },
			{ "11", "ETA" },
			{ "12", "ETB" },
			{ "13", "FDA" },
			{ "14", "FDB" }
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Dictionary values")]
		public static Dictionary<ZString, ZString> ComplianceDocumentStatusTypeDictionary => new Dictionary<ZString, ZString>()
		{
			{ "订单失效", ComplianceDocumentStatusTypes.CDD.Code },
			{ "订单未开票", ComplianceDocumentStatusTypes.CDN.Code },
			{ "开具失败", ComplianceDocumentStatusTypes.CDF.Code },
			{ "订单处理中", ComplianceDocumentStatusTypes.CDP.Code },
			{ "正常发票", ComplianceDocumentStatusTypes.CDI.Code },
			{ "已作废", ComplianceDocumentStatusTypes.CDV.Code },
			{ "已红冲", ComplianceDocumentStatusTypes.CDR.Code }
		};

		bool HasTaxId(AccTransactionHeader transactionHeader)
		{
			var transactionLines = transactionHeader.Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_AH, transactionHeader.PK)).ToList();

			return transactionLines.All(x => (x.ChargeCode?.AC_ChargeType ?? ZString.Empty) == Constants.ChargeType.Comment || x.AL_AT.IsValid);
		}

		#region IEInvoicingRegistryProvider

		bool IEInvoicingRegistryProvider.ShouldAutoSetEReportingComplianceDate => true;

		#endregion

		#region IComplianceInfoEInvoicingGUIActionQueueReversedTransaction

		bool IComplianceInfoEInvoicingGUIActionQueueReversedTransaction.RejectReQueueForReversedTransaction(string complianceSubType) => true;

		#endregion
	}
}
