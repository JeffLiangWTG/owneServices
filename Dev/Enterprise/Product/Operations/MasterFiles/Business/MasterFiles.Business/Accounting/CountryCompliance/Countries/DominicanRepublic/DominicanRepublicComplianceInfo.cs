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
using static Enterprise.MasterFiles.Business.DominicanRepublicOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class DominicanRepublicComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceSubTypeTaxRegistrationTypeRuleProvider,
		IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IComplianceRegistryDefaultProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.DominicanRepublic;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.RNC;
		protected override string GetConsumptionTaxCode() => "ITBIS"; // TODO: Investigate this code, and how it integrates with OrgCusCodes. It has a 5 character long name that breaks assumptions.
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.RNC;
		protected override bool? GetIsReciprocal() => true;
		protected override bool? GetIsRightHandSideAdressCountry() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXG,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TXS,
				ComplianceSubTypes.VGM,
				ComplianceSubTypes.VIS,
				ComplianceSubTypes.XCL,
				ComplianceSubTypes.TXF,
				ComplianceSubTypes.TEI,
				ComplianceSubTypes.TEF,
				ComplianceSubTypes.TED,
				ComplianceSubTypes.TEC,
				ComplianceSubTypes.VES,
				ComplianceSubTypes.VEM,
				ComplianceSubTypes.TES,
				ComplianceSubTypes.TEG,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXG = "TXG";
			public const string TCD = "TCD";
			public const string TCR = "TCR";
			public const string TXF = "TXF";
			public const string TXI = "TXI";
			public const string TXS = "TXS";
			public const string VGM = "VGM";
			public const string VIS = "VIS";
			public const string XCL = "XCL";
			public const string TEI = "TEI";
			public const string TEF = "TEF";
			public const string TED = "TED";
			public const string TEC = "TEC";
			public const string VES = "VES";
			public const string VEM = "VEM";
			public const string TES = "TES";
			public const string TEG = "TEG";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TCD { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TCD", "Debit Note"); } }
			public static MultilingualString TCR { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TCR", "Credit Note"); } }
			public static MultilingualString TXF { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TXF", "Final Consumer Invoice"); } }
			public static MultilingualString TXG { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TXG", "Government Recipient Invoice"); } }
			public static MultilingualString TXI { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TXI", "Tax Invoice"); } }
			public static MultilingualString TXS { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TXS", "Special Tax Treatment Invoice"); } }
			public static MultilingualString VGM { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|VGM", "Minor Expenses Voucher"); } }
			public static MultilingualString VIS { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|VIS", "Supplier Voucher"); } }
			public static MultilingualString XCL { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|XCL", "Reimbursement/Disbursement/Excluded Supply"); } }
			public static MultilingualString TEI { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TEI", "Electronic Tax Invoice"); } }
			public static MultilingualString TEF { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TEF", "Electronic Final Consumer Invoice"); } }
			public static MultilingualString TED { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TED", "Electronic Debit Note"); } }
			public static MultilingualString TEC { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TEC", "Electronic Credit Note"); } }
			public static MultilingualString VES { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|VES", "Electronic Supplier Voucher"); } }
			public static MultilingualString VEM { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|VEM", "Electronic Minor Expenses Voucher"); } }
			public static MultilingualString TES { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TES", "Electronic Special Tax Treatment Invoice"); } }
			public static MultilingualString TEG { get { return ResString.GetMultilingualString("DOComplianceSubTypeCodeList|TEG", "Electronic Government Recipient Invoice"); } }
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TCD = "Nota de Débito";
			public const string TCR = "Nota de Crédito";
			public const string TXF = "Factura de Consumo";
			public const string TXG = "Comprobante Gubernamental";
			public const string TXI = "Factura de Crédito Fiscal";
			public const string TXS = "Comprobante de Regímenes Especiales";
			public const string VGM = "Comprobante Registro de Gastos Menores";
			public const string VIS = "Comprobante de Compras";
			public const string XCL = "Documento de Reembolso";
			public const string TEI = "Factura de Crédito Fiscal Electrónica";
			public const string TEF = "Factura de Consumo Electrónica";
			public const string TED = "Nota de Débito Electrónica";
			public const string TEC = "Nota de Crédito Electrónica";
			public const string VES = "Compras Electrónico";
			public const string VEM = "Gastos Menores Electrónico";
			public const string TES = "Regímenes Especiales Electrónico";
			public const string TEG = "Gubernamental Electrónico";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TCD = "Used in both Receivables and Payables to record '03' Debit Notes. Used to identify Invoice (INV) transactions issued to amend Parent transactions codes 01, 02, 04, 14 or 15. Original Debit Notes can not be issued.";
			public const string TCR = "Used in both Receivables and Payables to record '04' Credit Notes. Used to identify Credit note (CRD) transactions issued to amend Parent transactions codes 01, 02, 03, 14 or 15. Original Credit Notes can not be issued.";
			public const string TXF = "Used in both Receivables and Payables to record '02' Invoices. Used to identify Invoice (INV) transactions issued to Final Consumers.";
			public const string TXG = "Used to record '15' Invoices. Used to identify Invoice (INV) transactions where the Receivables Organisation is a DO Government Organization.";
			public const string TXI = "Used in both Receivables and Payables to record '01' Invoices. Used to identify Invoices (INV) transactions where the Organization is inscribed in the Common Regime or the Simplified Taxation Regime. Only the Organizations inscribed in the Common Regime are able to deduct the ITBIS tax, for organizations inscribed in the Simplified Taxation Regime, the ITBIS included in the subtype is just informative.";
			public const string TXS = "Used in both Receivables and Payables to record '14' Invoices. Used to identify Invoice (INV) transactions where the Receivables Organisation is granted a special exemption from ITBIS by the DO Government and supports the exemption granted with a valid Exporter Exemption Certificate.";
			public const string VGM = "Used in Payables to record '13' Invoices. Used In Payables to sub-clasify Invoice (INV) transactions issued by the buyer to identify minor expenses related to work activity incurred by employees.";
			public const string VIS = "Used in Payables to record '11' Invoices. Used in Payables to sub-classify Invoice (INV) transactions issued by the buyer when purchasing goods or services to a supplier that is not registered as a taxpayer.";
			public const string XCL = "Used in both Receivables and Payables to record Invoice (INV) and Credit Note (CRD) transactions recorded for Disbursement / Reimbursement / Excluded supply purposes and where no Fiscal Document is to be issued / reported.";
			public const string TEI = "Used in both Receivables and Payables to record '31' Electronic Invoices. Used to identify electronic Invoices (INV) transactions where the Organization is inscribed in the Common Regime or the Simplified Taxation Regime. Only the Organizations inscribed in the Common Regime are able to deduct the ITBIS tax, for organizations inscribed in the Simplified Taxation Regime, the ITBIS included in the subtype is just informative.";
			public const string TEF = "Used in both Receivables and Payables to record '32' Electronic Invoices. Used to identify electronic Invoice (INV) transactions issued to Final Consumers.";
			public const string TED = "Used in both Receivables and Payables to record '33' Electronic Debit Notes. Used to identify electronic Invoice (INV) transactions issued to amend Parent transactions codes 31, 32, 34, 44, 45 or its non-electronic equivalent documents (Codes 01, 02, 04, 14 or 15). Original Debit Notes can not be issued.";
			public const string TEC = "Used in both Receivables and Payables to record '34' Electronic Credit Notes. Used to identify electronic Credit note (CRD) transactions issued to amend Parent transactions codes 31, 32, 33, 44, 45 or its non-electronic equivalent documents (Codes 01, 02, 03, 14 or 15). Original Credit Notes can not be issued.";
			public const string VES = "Used in Payables to record '41' Electronic Invoices. Used in Payables to sub-classify electronic Invoice (INV) transactions issued by the buyer when purchasing goods or services to a supplier that is not registered as a taxpayer.";
			public const string VEM = "Used in Payables to record '43' Electronic Invoices. Used In Payables to sub-classify electronic Invoice (INV) transactions issued by the buyer to identify minor expenses related to work activity incurred by employees.";
			public const string TES = "Used in both Receivables and Payables to record '44' Electronic Invoices. Used to identify electronic Invoice (INV) transactions where the Receivables Organization is granted a special exemption from ITBIS by the DO Government and supports the exemption granted with a valid Exporter Exemption Certificate.";
			public const string TEG = "Used in Receivables to record '45' Electronic Invoices. Used to identify electronic Invoice (INV) transactions where the Receivables Organization is a DO Government Organization.";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXG => new ComplianceSubType(ComplianceSubTypeCodes.TXG, () => ComplianceSubTypeDescriptions.TXG, () => ComplianceSubTypeLocalDescriptions.TXG, () => ComplianceSubTypeInternalImplemenationNote.TXG, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXS => new ComplianceSubType(ComplianceSubTypeCodes.TXS, () => ComplianceSubTypeDescriptions.TXS, () => ComplianceSubTypeLocalDescriptions.TXS, () => ComplianceSubTypeInternalImplemenationNote.TXS, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType VGM => new ComplianceSubType(ComplianceSubTypeCodes.VGM, () => ComplianceSubTypeDescriptions.VGM, () => ComplianceSubTypeLocalDescriptions.VGM, () => ComplianceSubTypeInternalImplemenationNote.VGM, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType VIS => new ComplianceSubType(ComplianceSubTypeCodes.VIS, () => ComplianceSubTypeDescriptions.VIS, () => ComplianceSubTypeLocalDescriptions.VIS, () => ComplianceSubTypeInternalImplemenationNote.VIS, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
			public static ComplianceSubType TXF => new ComplianceSubType(ComplianceSubTypeCodes.TXF, () => ComplianceSubTypeDescriptions.TXF, () => ComplianceSubTypeLocalDescriptions.TXF, () => ComplianceSubTypeInternalImplemenationNote.TXF, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TEI => new ComplianceSubType(ComplianceSubTypeCodes.TEI, () => ComplianceSubTypeDescriptions.TEI, () => ComplianceSubTypeLocalDescriptions.TEI, () => ComplianceSubTypeInternalImplemenationNote.TEI, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TEF => new ComplianceSubType(ComplianceSubTypeCodes.TEF, () => ComplianceSubTypeDescriptions.TEF, () => ComplianceSubTypeLocalDescriptions.TEF, () => ComplianceSubTypeInternalImplemenationNote.TEF, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TED => new ComplianceSubType(ComplianceSubTypeCodes.TED, () => ComplianceSubTypeDescriptions.TED, () => ComplianceSubTypeLocalDescriptions.TED, () => ComplianceSubTypeInternalImplemenationNote.TED, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TEC => new ComplianceSubType(ComplianceSubTypeCodes.TEC, () => ComplianceSubTypeDescriptions.TEC, () => ComplianceSubTypeLocalDescriptions.TEC, () => ComplianceSubTypeInternalImplemenationNote.TEC, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType VES => new ComplianceSubType(ComplianceSubTypeCodes.VES, () => ComplianceSubTypeDescriptions.VES, () => ComplianceSubTypeLocalDescriptions.VES, () => ComplianceSubTypeInternalImplemenationNote.VES, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType VEM => new ComplianceSubType(ComplianceSubTypeCodes.VEM, () => ComplianceSubTypeDescriptions.VEM, () => ComplianceSubTypeLocalDescriptions.VEM, () => ComplianceSubTypeInternalImplemenationNote.VEM, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TES => new ComplianceSubType(ComplianceSubTypeCodes.TES, () => ComplianceSubTypeDescriptions.TES, () => ComplianceSubTypeLocalDescriptions.TES, () => ComplianceSubTypeInternalImplemenationNote.TES, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TEG => new ComplianceSubType(ComplianceSubTypeCodes.TEG, () => ComplianceSubTypeDescriptions.TEG, () => ComplianceSubTypeLocalDescriptions.TEG, () => ComplianceSubTypeInternalImplemenationNote.TEG, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
		}

		#endregion

		#region TaxRegistrationType

		public static class TaxRegistrationTypeCodes
		{
			public const string GovernmentOrganizations = "REG";
			public const string CommonSimplifiedRegime = "RCS";
		}

		public static class TaxRegistrationTypeDescriptions
		{
			public static string GovernmentOrganizations => Res.GetString("F4DD3F7D-182C-487F-8EB0-B44984C02578", "{0} / Government Organizations", "Empresa Gubernamental");
			public static string CommonSimplifiedRegime => Res.GetString("968582A9-CBB1-490B-AAB5-9D05DC2C46BD", "{0} / Common/Simplified Regime", "Régimen Común/Simplificado de Tributación");
		}

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.DominicanRepublic;

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
		{
			return GetEInvoicingEligibleComplianceSubTypeList().Contains(complianceSubType.ToString());
		}

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => GetEInvoicingEligibleComplianceSubTypeList();

		static string[] GetEInvoicingEligibleComplianceSubTypeList()
		{
			return new string[]
			{
				ComplianceSubTypeCodes.TEI,
				ComplianceSubTypeCodes.TEF,
				ComplianceSubTypeCodes.TEC,
				ComplianceSubTypeCodes.TED,
				ComplianceSubTypeCodes.TES,
				ComplianceSubTypeCodes.TEG,
			};
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.TaxDocumentsAndExcludedSupply, RuleSetDescriptions.TaxDocumentsAndExcludedSupply);
			rulesetList.AddPair(RuleSetCodes.ElectronicTaxsAndReimbTransaction, RuleSetDescriptions.ElectronicTaxsAndReimbTransaction);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ?
				RuleSetCodes.ElectronicTaxsAndReimbTransaction : RuleSetCodes.TaxDocumentsAndExcludedSupply;
		}

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			if (ruleSetCode == RuleSetCodes.TaxDocumentsAndExcludedSupply)
			{
				AddComplianceSubTypeAttributionRulesForTaxDoumentsAndExcludeSupply(collection);
			}
			else if (ruleSetCode == RuleSetCodes.ElectronicTaxsAndReimbTransaction)
			{
				AddComplianceSubTypeAttributionRulesForElectronicTaxAndReimbTransactions(collection);
			}
			else if (string.IsNullOrEmpty(ruleSetCode)) //this is valid for UT and CountryComplianceInfoDisplayForm
			{
				AddComplianceSubTypeAttributionRulesForTaxDoumentsAndExcludeSupply(collection);
				AddComplianceSubTypeAttributionRulesForElectronicTaxAndReimbTransactions(collection);
			}
			else
			{
				ErrorReporter.ReportOnce("DominicanRepublicComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
			}
		}

		void AddComplianceSubTypeAttributionRulesForTaxDoumentsAndExcludeSupply(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			#region AR

			var configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = OrgCusCodes.RCS;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TXS;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXS;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXS;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TXF;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXF;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXF;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TXG;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXG;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXG;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			#endregion

			#region AP

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = string.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = string.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = string.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = string.Empty;
			configuration.RuleSetCode = RuleSetCodes.TaxDocumentsAndExcludedSupply;
			configuration.RuleSetDescription = RuleSetDescriptions.TaxDocumentsAndExcludedSupply;

			#endregion
		}

		void AddComplianceSubTypeAttributionRulesForElectronicTaxAndReimbTransactions(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			#region AR

			var configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEI;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEF;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEI;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEC;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TES;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEC;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEF;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEC;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEG;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEC;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TES;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEG;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXS;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXF;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXG;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TED;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCR;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEI;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TED;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TES;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TED;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEF;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TED;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TEG;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TED;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXI;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXS;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXF;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TXG;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.TEC;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.TCD;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.CommonSimplifiedRegime;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.Exempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = TaxRegistrationTypeCodes.GovernmentOrganizations;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = ExporterExemptionCodes.NotExempt;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			#endregion

			#region AP

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = string.Empty;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = string.Empty;
			configuration.ExporterExemption = string.Empty;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = string.Empty;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.DominicanRepublic;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = string.Empty;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;
			configuration.ExporterExemption = string.Empty;
			configuration.RuleSetCode = RuleSetCodes.ElectronicTaxsAndReimbTransaction;
			configuration.RuleSetDescription = RuleSetDescriptions.ElectronicTaxsAndReimbTransaction;

			#endregion
		}

		#region SuppressResourceStringsCheckRegion

		public static class RuleSetCodes
		{
			public const string TaxDocumentsAndExcludedSupply = "1";
			public const string ElectronicTaxsAndReimbTransaction = "2";
		}

		static class RuleSetDescriptions
		{
			public const string TaxDocumentsAndExcludedSupply = "Tax, Special Regime, Government , Final Consumer and Reimbursement Transactions";
			public const string ElectronicTaxsAndReimbTransaction = "e-CF Electronic Tax, Special Regime, Government, Final Consumer and Reimb. Transactions";
		}

		#endregion

		#endregion

		#region ComplianceSubTypeTaxRegistration

		bool IComplianceSubTypeTaxRegistrationTypeRuleProvider.IsTaxRegistrationTypeRuleApplicable(IAccComplianceRule rule, OrgHeader header)
		{
			return (header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.REG, CountryCode) == null && rule.TaxRegistrationType.IsEmpty)
					|| (header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.RCS, CountryCode) == null && rule.TaxRegistrationType.IsEmpty)
					|| (header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.REG, CountryCode) != null && rule.TaxRegistrationType == TaxRegistrationTypeCodes.GovernmentOrganizations)
					|| (header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCodes.RCS, CountryCode) != null && rule.TaxRegistrationType == TaxRegistrationTypeCodes.CommonSimplifiedRegime);
		}

		#endregion

		#region IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider

		CodeDescriptionPairList IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider.GetTaxRegistrationTypeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(TaxRegistrationTypeCodes.CommonSimplifiedRegime, TaxRegistrationTypeDescriptions.CommonSimplifiedRegime);
			list.AddPair(TaxRegistrationTypeCodes.GovernmentOrganizations, TaxRegistrationTypeDescriptions.GovernmentOrganizations);
			return list;
		}

		#endregion

		#region IComplianceRegistryDefaultProvider

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry() => AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled)
			=> isEInvoicingEnabled ? AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post : null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => null;

		#endregion
	}
}
