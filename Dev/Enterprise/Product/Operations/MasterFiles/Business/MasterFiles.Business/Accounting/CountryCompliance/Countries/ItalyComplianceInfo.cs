using System;
using System.Collections.Generic;
using CargoWise.Common;
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
	public class ItalyComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		ITaxMessagesGroupProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IComplianceInfoEInvoicingGUIActionQueuePendingInvoice,
		IComplianceRegistryDefaultProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceSubTypeDependencyConfigurationProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Italy;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		protected override bool? GetIsReciprocal() => false;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		protected override bool? HasExtraTaxInfo() => true;
		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSSPV", "SPV Amt", "SPV Amount", "");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalSPV", "SPV Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSIVA", "IVA Amt", "IVA Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalIVA", "IVA Local");

		#endregion

		#region IComplianceSubTypeDependencyConfigurationProvider

		void IComplianceSubTypeDependencyConfigurationProvider.GetDefaults(ComplianceSubTypeDependencyConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.G26;
			configuration.ParentSubType = ComplianceSubTypeCodes.ARI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.ChildSubType = ComplianceSubTypeCodes.G28;
			configuration.ParentSubType = ComplianceSubTypeCodes.API;
		}

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.API,
				ComplianceSubTypes.APS,
				ComplianceSubTypes.APV,
				ComplianceSubTypes.ARE,
				ComplianceSubTypes.ARI,
				ComplianceSubTypes.ARN,
				ComplianceSubTypes.ARS,
				ComplianceSubTypes.ARV,
				ComplianceSubTypes.G26,
				ComplianceSubTypes.G28,
				ComplianceSubTypes.INI,
				ComplianceSubTypes.INT,
				ComplianceSubTypes.XAP,
				ComplianceSubTypes.XCL,
				ComplianceSubTypes.XLP
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string API = "API";
			public const string APS = "APS";
			public const string APV = "APV";
			public const string ARE = "ARE";
			public const string ARI = "ARI";
			public const string ARN = "ARN";
			public const string ARS = "ARS";
			public const string ARV = "ARV";
			public const string G26 = "G26";
			public const string G28 = "G28";
			public const string INI = "INI";
			public const string INT = "INT";
			public const string XAP = "XAP";
			public const string XCL = "XCL";
			public const string XLP = "XLP";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString API => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|API", "AP Tax Invoice");
			public static MultilingualString APS => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|APS", "AP Autofattura Non EU Reverse Charge");
			public static MultilingualString APV => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|APV", "AP Tax Voucher (Simplified Invoice)");
			public static MultilingualString ARE => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|ARE", "AR Tax Invoice to EU Business Organization");
			public static MultilingualString ARI => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|ARI", "AR Tax Invoice");
			public static MultilingualString ARN => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|ARN", "AR Tax Invoice to Non EU Business Organization");
			public static MultilingualString ARS => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|ARS", "AR Self Billed Tax Invoice");
			public static MultilingualString ARV => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|ARV", "AR Tax Voucher (Simplified Invoice)");
			public static MultilingualString G26 => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|G26", "AR Tax Invoice (Assets)");
			public static MultilingualString G28 => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|G28", "AP Tax Invoice (San Marino Paper)");
			public static MultilingualString INI => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|INI", "AP Integration Domestic Reverse Charge");
			public static MultilingualString INT => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|INT", "AP Integration EU Reverse Charge");
			public static MultilingualString XAP => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|XAP", "AP Reimbursement / Disbursement / Excluded Supply");
			public static MultilingualString XCL => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|XCL", "AR Reimbursement / Disbursement / Excluded Supply");
			public static MultilingualString XLP => ResString.GetMultilingualString("ITComplianceSubTypeCodeList|XLP", "AP Customs Duties Reimbursement / Disbursement");
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string API = "Acquisti - Fattura / Nota di Credito";
			public const string APS = "Acquisti - Autofattura (Extra UE / Conto Del Fornitore)";
			public const string APV = "Acquisti - Fattura Semplificata";
			public const string ARE = "Vendite - Fattura / Nota di Credito (UE)";
			public const string ARI = "Vendite - Fattura / Nota di Credito";
			public const string ARN = "Vendite - Fattura / Nota di Credito (Extra UE)";
			public const string ARS = "Fattura / Credito Per Conto Del Fornitore";
			public const string ARV = "Vendite - Fattura Semplificata";
			public const string G26 = "Vendite - Fattura Vendita Cespite";
			public const string G28 = "Acquisti - Fattura Cartacea (San Marino)";
			public const string INI = "Acquisti - Integrazione (Italia)";
			public const string INT = "Acquisti - Integrazione (UE)";
			public const string XAP = "Acquisti - Escluse - Rimborso Documentato";
			public const string XCL = "Vendite - Fattura/Nota di Credito Anticipo Diritti Doganali";
			public const string XLP = "Acquisti - Fattura/Nota di Credito Anticipo Diritti Doganali";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string API = "Payables Sub Type. Used in Payables to record Italy Tax Invoices and Credit Notes received from other Italy Suppliers.";
			public const string APS = "Payables Sub Type. Used in Payables to record Invoice and Credit Note purchases from Non-EU suppliers. Used because the Italy Login Company has not received a fiscal document (Invoice or Credit Note) that is recognised under Italy VAT law. Is also used when the Login Company creates their own 'Purchaser created invoice' (CW1 Self Billed Invoice features) when the Login Company itself initiates and documents the purchase transaction because no Invoice or Credit Note was received from the supplier.";
			public const string APV = "Payables Sub Type. Used in Payables to record the occasional low value administration, miscellaneous or non-job-related invoice or credit note transaction.";
			public const string ARE = "Receivables Sub Type. Used in Receivables when the Italy Login Company prefers to distinctly and separately track Invoice (INV) and Credit Note (CRD) transactions recorded against Receivables organizations in other EU Community countries.";
			public const string ARI = "Receivables Sub Type. Used in Receivables to sub-classify Invoice (INV) and Credit Note (CRD) transactions posted by an Italy Login Company for Italy, EU and Non-EU Businesses. Most Invoices and Credit Notes issued by an Italy Login Company fall into this category. ";
			public const string ARN = "Receivables Sub Type. Used in Receivables when the Italy Login Company prefers to distinctly and separately track Invoice (INV) and Credit Note (CRD) transactions recorded against Receivables organizations in Non-EU countries.";
			public const string ARS = "Receivables Sub Type. Used to record Receivables Transactions received from Customers that have ‘Self-Billed’ the Login Company. The Italy Login Company will record the Receivable's organization's self-purchase using a special Receivables data entry feature. This sub type is used when the Receivables organization invoiced themselves for services provided to them by the Italy Login Company. ";
			public const string ARV = "Receivables Sub Type. Used in Receivables to record the occasional low value administration, miscellaneous or non-job-related invoice. This sub type is rarely, if ever, used.";
			public const string G26 = "Receivables Sub Type. Used in Receivables to sub-classify Invoice (INV) transactions related to Assets.";
			public const string G28 = "Payables Sub Type. Used in Payables to record Tax Paper Invoice and Tax Paper Credit Notes received from Suppliers located in San Marino.";
			public const string INI = "Payables Sub Type. Used in Payables to record Italy Domestic Reverse Charge Tax Invoices and Credit Notes. By way of example, Cleaning Services billed by an Italy business can be subject to Domestic Reverse Charge IVA treatment.";
			public const string INT = "Payables Sub Type. Used in Payables to record EU Reverse Charge Tax Invoice and Credit Notes received from Suppliers in other EU countries.";
			public const string XAP = "Payables Sub Type. Used to expressly identify Accounts Payables Invoice (INV) and Credit Note transactions (CRD) where the Italy Login Company is not the actual recipient of the services recorded in the Payables transaction. eg. Disbursment paid on behalf of a customer that is posted exclusively with Tax Id type Excluded.";
			public const string XCL = "Receivables Sub Type. Used to identify Invoice (INV) and Credit Note (CRD) transactions that do not require e-Invoice documentation, because no taxable supply was made or received (eg. Disbursement / Reimbursement only transactions).";
			public const string XLP = "Payables Sub Type. Used to identify Invoice (INV) and Credit Note (CRD) transactions that do not require e-Invoice documentation, because no taxable supply was made or received specifically for Customs Duties.";
		}
		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType API => new ComplianceSubType(ComplianceSubTypeCodes.API, () => ComplianceSubTypeDescriptions.API, () => ComplianceSubTypeLocalDescriptions.API, () => ComplianceSubTypeInternalImplemenationNote.API, LedgerOfUse.AP);
			public static ComplianceSubType APS => new ComplianceSubType(ComplianceSubTypeCodes.APS, () => ComplianceSubTypeDescriptions.APS, () => ComplianceSubTypeLocalDescriptions.APS, () => ComplianceSubTypeInternalImplemenationNote.APS, LedgerOfUse.AP);
			public static ComplianceSubType APV => new ComplianceSubType(ComplianceSubTypeCodes.APV, () => ComplianceSubTypeDescriptions.APV, () => ComplianceSubTypeLocalDescriptions.APV, () => ComplianceSubTypeInternalImplemenationNote.APV, LedgerOfUse.AP);
			public static ComplianceSubType ARE => new ComplianceSubType(ComplianceSubTypeCodes.ARE, () => ComplianceSubTypeDescriptions.ARE, () => ComplianceSubTypeLocalDescriptions.ARE, () => ComplianceSubTypeInternalImplemenationNote.ARE, LedgerOfUse.AR);
			public static ComplianceSubType ARI => new ComplianceSubType(ComplianceSubTypeCodes.ARI, () => ComplianceSubTypeDescriptions.ARI, () => ComplianceSubTypeLocalDescriptions.ARI, () => ComplianceSubTypeInternalImplemenationNote.ARI, LedgerOfUse.AR);
			public static ComplianceSubType ARN => new ComplianceSubType(ComplianceSubTypeCodes.ARN, () => ComplianceSubTypeDescriptions.ARN, () => ComplianceSubTypeLocalDescriptions.ARN, () => ComplianceSubTypeInternalImplemenationNote.ARN, LedgerOfUse.AR);
			public static ComplianceSubType ARS => new ComplianceSubType(ComplianceSubTypeCodes.ARS, () => ComplianceSubTypeDescriptions.ARS, () => ComplianceSubTypeLocalDescriptions.ARS, () => ComplianceSubTypeInternalImplemenationNote.ARS, LedgerOfUse.AR);
			public static ComplianceSubType ARV => new ComplianceSubType(ComplianceSubTypeCodes.ARV, () => ComplianceSubTypeDescriptions.ARV, () => ComplianceSubTypeLocalDescriptions.ARV, () => ComplianceSubTypeInternalImplemenationNote.ARV, LedgerOfUse.AR);
			public static ComplianceSubType G26 => new ComplianceSubType(ComplianceSubTypeCodes.G26, () => ComplianceSubTypeDescriptions.G26, () => ComplianceSubTypeLocalDescriptions.G26, () => ComplianceSubTypeInternalImplemenationNote.G26, LedgerOfUse.AR);
			public static ComplianceSubType G28 => new ComplianceSubType(ComplianceSubTypeCodes.G28, () => ComplianceSubTypeDescriptions.G28, () => ComplianceSubTypeLocalDescriptions.G28, () => ComplianceSubTypeInternalImplemenationNote.G28, LedgerOfUse.AP);
			public static ComplianceSubType INI => new ComplianceSubType(ComplianceSubTypeCodes.INI, () => ComplianceSubTypeDescriptions.INI, () => ComplianceSubTypeLocalDescriptions.INI, () => ComplianceSubTypeInternalImplemenationNote.INI, LedgerOfUse.AP);
			public static ComplianceSubType INT => new ComplianceSubType(ComplianceSubTypeCodes.INT, () => ComplianceSubTypeDescriptions.INT, () => ComplianceSubTypeLocalDescriptions.INT, () => ComplianceSubTypeInternalImplemenationNote.INT, LedgerOfUse.AP);
			public static ComplianceSubType XAP => new ComplianceSubType(ComplianceSubTypeCodes.XAP, () => ComplianceSubTypeDescriptions.XAP, () => ComplianceSubTypeLocalDescriptions.XAP, () => ComplianceSubTypeInternalImplemenationNote.XAP, LedgerOfUse.AP);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL, LedgerOfUse.AR);
			public static ComplianceSubType XLP => new ComplianceSubType(ComplianceSubTypeCodes.XLP, () => ComplianceSubTypeDescriptions.XLP, () => ComplianceSubTypeLocalDescriptions.XLP, () => ComplianceSubTypeInternalImplemenationNote.XLP, LedgerOfUse.AP);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.Default, RuleSetDescriptions.Default);
			rulesetList.AddPair(RuleSetCodes.DefaultWithDisbursementAR, RuleSetDescriptions.DefaultWithDisbursementAR);
			rulesetList.AddPair(RuleSetCodes.DefaultWithARByCountry, RuleSetDescriptions.DefaultWithARByCountry);
			rulesetList.AddPair(RuleSetCodes.DefaultWithARByCountryAndDisbursement, RuleSetDescriptions.DefaultWithARByCountryAndDisbursement);
			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.Default;
		}

		public static class RuleSetCodes
		{
			public const string Default = "1";
			public const string DefaultWithDisbursementAR = "2";
			//public const string DefaultWithDisbursement = "5"; TODO: restore when fixed XLP management
			public const string DefaultWithARByCountry = "3";
			public const string DefaultWithARByCountryAndDisbursement = "4";
		}

		#region SuppressResourceStringsCheckRegion
		static class RuleSetDescriptions
		{
			public const string Default = "Vendite ARI ARS, Acquisti API APS INI INT XAP";
			public const string DefaultWithDisbursementAR = "Vendite ARI ARS XCL, Acquisti API APS INI INT XAP";
			//public const string DefaultWithDisbursement = "Vendite ARI ARS XCL, Acquisti API APS INI INT XAP XLP"; TODO: restore when fixed XLP management
			public const string DefaultWithARByCountry = "Vendite ARI ARS ARE ARN, Acquisti API APS INI INT XAP";
			public const string DefaultWithARByCountryAndDisbursement = "Vendite ARI ARS ARE ARN XCL, Acquisti API APS INI INT XAP"; //TODO: add XLP
		}

		#endregion

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			switch (ruleSetCode)
			{
				case RuleSetCodes.Default:
					AddComplianceSubTypeAttributionRulesForDefault(collection);
					break;
				case RuleSetCodes.DefaultWithDisbursementAR:
					AddComplianceSubTypeAttributionRulesForDefaultWithDisbursementAR(collection);
					break;
				case RuleSetCodes.DefaultWithARByCountry:
					AddComplianceSubTypeAttributionRulesForDefaultWithARByCountry(collection);
					break;
				case RuleSetCodes.DefaultWithARByCountryAndDisbursement:
					AddComplianceSubTypeAttributionRulesForDefaultWithARByCountryAndDisbursement(collection);
					break;
				case null: //this is valid for UT and CountryComplianceInfoDisplayForm
				case "":
					AddComplianceSubTypeAttributionRulesForDefault(collection);
					AddComplianceSubTypeAttributionRulesForDefaultWithDisbursementAR(collection);
					AddComplianceSubTypeAttributionRulesForDefaultWithARByCountry(collection);
					AddComplianceSubTypeAttributionRulesForDefaultWithARByCountryAndDisbursement(collection);
					break;
				default:
					ErrorReporter.ReportOnce("ItalyComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
					break;
			}
		}

		void AddCommonComplianceSubTypeAttributionRules(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARS;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.SelfBillingTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARS;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.SelfBillingTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.API;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = CountryCode;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.API;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = CountryCode;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.APS;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.NotEU;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.APS;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.NotEU;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.APS;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.SelfBillingTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.APS;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.SelfBillingTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.INI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = CountryCode;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.INI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = CountryCode;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.INT;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = TaxRegistrationLocationRuleCodes.EUExcludingLoginCountry;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.INT;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = TaxRegistrationLocationRuleCodes.EUExcludingLoginCountry;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XAP;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XAP;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XAP;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = CountryCode;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XAP;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = CountryCode;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XAP;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.NotEU;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XAP;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.NotEU;
			configuration.TaxRegistrationLocationRule = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;
		}

		void SetCommonSubTypeConfigProperties(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode, string ruleSetDesc)
		{
			foreach (ComplianceSubTypeAttributionRuleConfiguration cfg in collection)
			{
				if (cfg.Country == CountryCode && cfg.RuleSetCode.IsEmpty)
				{
					cfg.OriginalRule = OriginalRuleCodes.AllTransactions;
					cfg.VATGroupRule = VatGroupListCodes.ExcludeVATGroupMembers;
					cfg.RuleSetCode = ruleSetCode;
					cfg.RuleSetDescription = ruleSetDesc;
				}
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefault(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			AddCommonComplianceSubTypeAttributionRules(collection);
			SetCommonSubTypeConfigProperties(collection, RuleSetCodes.Default, RuleSetDescriptions.Default);
		}

		void AddComplianceSubTypeAttributionRulesForDefaultWithDisbursementAR(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			AddCommonComplianceSubTypeAttributionRules(collection);

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			SetCommonSubTypeConfigProperties(collection, RuleSetCodes.DefaultWithDisbursementAR, RuleSetDescriptions.DefaultWithDisbursementAR);
		}

		void AddComplianceSubTypeAttributionRulesForDefaultWithARByCountry(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = CountryCode;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = CountryCode;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.EUExcludingLoginCountry;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.EUExcludingLoginCountry;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.NotEU;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.NotEU;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			AddCommonComplianceSubTypeAttributionRules(collection);
			SetCommonSubTypeConfigProperties(collection, RuleSetCodes.DefaultWithARByCountry, RuleSetDescriptions.DefaultWithARByCountry);
		}

		void AddComplianceSubTypeAttributionRulesForDefaultWithARByCountryAndDisbursement(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OrganisationLocation = CountryCode;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OrganisationLocation = CountryCode;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.EUExcludingLoginCountry;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARE;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.EUExcludingLoginCountry;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.NotEU;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.ARN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			configuration.OrganisationLocation = TaxRegistrationLocationRuleCodes.NotEU;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			AddCommonComplianceSubTypeAttributionRules(collection);

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.DisbursementOnly;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			SetCommonSubTypeConfigProperties(collection, RuleSetCodes.DefaultWithARByCountryAndDisbursement, RuleSetDescriptions.DefaultWithARByCountryAndDisbursement);
		}

		#endregion

		#region ITaxMessagesGroupProvider
		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N0, Description = TaxMessageGroupDescriptions.N0, Bool = true },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N1, Description = TaxMessageGroupDescriptions.N1, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N1 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N2, Description = TaxMessageGroupDescriptions.N2, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N2 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N21, Description = TaxMessageGroupDescriptions.N21, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N21 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N22, Description = TaxMessageGroupDescriptions.N22, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N22 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N3, Description = TaxMessageGroupDescriptions.N3, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N3 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N31, Description = TaxMessageGroupDescriptions.N31, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N31 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N32, Description = TaxMessageGroupDescriptions.N32, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N32 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N33, Description = TaxMessageGroupDescriptions.N33, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N33 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N34, Description = TaxMessageGroupDescriptions.N34, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N34 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N35, Description = TaxMessageGroupDescriptions.N35, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N35 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N36, Description = TaxMessageGroupDescriptions.N36, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N36 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N4, Description = TaxMessageGroupDescriptions.N4, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N4 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N5, Description = TaxMessageGroupDescriptions.N5, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N5 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N6, Description = TaxMessageGroupDescriptions.N6, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N6 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N61, Description = TaxMessageGroupDescriptions.N61, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N61 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N62, Description = TaxMessageGroupDescriptions.N62, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N62 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N63, Description = TaxMessageGroupDescriptions.N63, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N63 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N64, Description = TaxMessageGroupDescriptions.N64, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N64 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N65, Description = TaxMessageGroupDescriptions.N65, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N65 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N66, Description = TaxMessageGroupDescriptions.N66, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N66 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N67, Description = TaxMessageGroupDescriptions.N67, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N67 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N68, Description = TaxMessageGroupDescriptions.N68, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N68 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N69, Description = TaxMessageGroupDescriptions.N69, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N69 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N7, Description = TaxMessageGroupDescriptions.N7, Bool = true, RelatedItemCode = TaxMessageGroupGovtCodes.N7 }
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string N0 = "N0";
			public const string N1 = "N1";
			public const string N2 = "N2";
			public const string N21 = "N21";
			public const string N22 = "N22";
			public const string N3 = "N3";
			public const string N31 = "N31";
			public const string N32 = "N32";
			public const string N33 = "N33";
			public const string N34 = "N34";
			public const string N35 = "N35";
			public const string N36 = "N36";
			public const string N4 = "N4";
			public const string N5 = "N5";
			public const string N6 = "N6";
			public const string N61 = "N61";
			public const string N62 = "N62";
			public const string N63 = "N63";
			public const string N64 = "N64";
			public const string N65 = "N65";
			public const string N66 = "N66";
			public const string N67 = "N67";
			public const string N68 = "N68";
			public const string N69 = "N69";
			public const string N7 = "N7";
		}

		#region SuppressResourceStringsCheckRegion

		public static class TaxMessageGroupDescriptions
		{
			public static MultilingualString N0 => (NoResString)"No Natura";
			public static MultilingualString N1 => (NoResString)"Escluse ex art.15";
			public static MultilingualString N2 => (NoResString)"Non soggette";
			public static MultilingualString N21 => (NoResString)"Non soggette ad IVA ai sensi degli artt. da 7 a 7-septies del DPR 633/72";
			public static MultilingualString N22 => (NoResString)"Non soggette - altri casi";
			public static MultilingualString N3 => (NoResString)"Non imponibili";
			public static MultilingualString N31 => (NoResString)"Non imponibili - esportazioni";
			public static MultilingualString N32 => (NoResString)"Non imponibili - cessioni intracomunitarie";
			public static MultilingualString N33 => (NoResString)"Non imponibili - cessioni verso San Marino";
			public static MultilingualString N34 => (NoResString)"Non imponibili - operazioni assimilate alle cessioni all'esportazione";
			public static MultilingualString N35 => (NoResString)"Non imponibili - a seguito di dichiarazioni d'intento";
			public static MultilingualString N36 => (NoResString)"Non imponibili - altre operazioni che non concorrono alla formazione del plafond";
			public static MultilingualString N4 => (NoResString)"Esente";
			public static MultilingualString N5 => (NoResString)"Regime del margine/IVA non esposta in fattura";
			public static MultilingualString N6 => (NoResString)"Reverse charge";
			public static MultilingualString N61 => (NoResString)"Reverse charge - cessione di rottami e altri materiali di recupero";
			public static MultilingualString N62 => (NoResString)"Reverse charge - cessione di oro e argento puro";
			public static MultilingualString N63 => (NoResString)"Reverse charge - subappalto nel settore edile";
			public static MultilingualString N64 => (NoResString)"Reverse charge - cessione di fabbricati";
			public static MultilingualString N65 => (NoResString)"Reverse charge - cessione di telefoni cellulari";
			public static MultilingualString N66 => (NoResString)"Reverse charge - cessione di prodotti elettronici";
			public static MultilingualString N67 => (NoResString)"Reverse charge - prestazioni comparto edile e settori connessi";
			public static MultilingualString N68 => (NoResString)"Reverse charge - operazioni settore energetico";
			public static MultilingualString N69 => (NoResString)"Reverse charge - altri casi";
			public static MultilingualString N7 => (NoResString)"IVA assolta in altro Stato UE";
		}

		#endregion

		public static class TaxMessageGroupGovtCodes
		{
			public const string N0 = "N0";
			public const string N1 = "N1";
			public const string N2 = "N2";
			public const string N21 = "N2.1";
			public const string N22 = "N2.2";
			public const string N3 = "N3";
			public const string N31 = "N3.1";
			public const string N32 = "N3.2";
			public const string N33 = "N3.3";
			public const string N34 = "N3.4";
			public const string N35 = "N3.5";
			public const string N36 = "N3.6";
			public const string N4 = "N4";
			public const string N5 = "N5";
			public const string N6 = "N6";
			public const string N61 = "N6.1";
			public const string N62 = "N6.2";
			public const string N63 = "N6.3";
			public const string N64 = "N6.4";
			public const string N65 = "N6.5";
			public const string N66 = "N6.6";
			public const string N67 = "N6.7";
			public const string N68 = "N6.8";
			public const string N69 = "N6.9";
			public const string N7 = "N7";
		}

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => new ZDate(2019, 1, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => new ZDate(2022, 7, 1);

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		public string GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany = null) =>
			AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatusForPayablesOnlyItaly.GetValueWithoutFallback(currenCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty);

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => string.Empty;

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => Array.Empty<string>();

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType) => false;

		#endregion

		#region IComplianceInfoEInvoicingGUIActionQueuePendingInvoice

		public ZString QueuePendingInvoiceMenuName => ZString.Empty;

		public ZString[] PivotStatusesEligibleForQueuing => new ZString[] { EInvoicingPivotState.Pending, EInvoicingPivotState.AwaitingReview };

		public ZString PivotStatusesEligibleForQueuingErrorMessage => Res.GetString("DF0F0686-7C95-4EB6-AD67-1FE0B83F37F3", "You can only Authorize and Send transactions where the E-Reporting status is 'PEN - Pending' or 'AWA - Awaiting Review'.");

		#endregion

		#region IComplianceRegistryDefaultProvider

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry()
			=> AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled) => null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => null;

		#endregion
	}
}
