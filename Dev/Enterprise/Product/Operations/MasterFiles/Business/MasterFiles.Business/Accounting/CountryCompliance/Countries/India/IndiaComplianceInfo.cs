using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class IndiaComplianceInfo : CountryComplianceInfo,
		IComplianceRegistryDefaultProvider,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IComplianceSubTypeAndNumberUpdateRules,
		IOrgCusCodePredicateProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.India;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.GSTCode;
		protected override string GetLocalBusinessRegNoCodeType() => GetConsumptionTaxRegistrationCode();
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region Registry Item Defaults

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry() => AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled)
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled) => null;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => true;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => false;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => true;

		#endregion

		protected override bool? HasExtraTaxInfo() => true;

		protected override IEnumerable<ICodeDescription> GetExtraTaxTypes()
		{
			var extraTypes = base.GetExtraTaxTypes().ToList();
			extraTypes.AddRange(
				new CodeDescriptionPair[]
				{
					new CodeDescriptionPair(AccTaxRate.ExtraTypes.StateGST, Res.GetString("6dd6cf4c-f67a-4eb6-9968-2503d0846fff", "CGST/SGST")),
					new CodeDescriptionPair(AccTaxRate.ExtraTypes.ServiceTax, Res.GetString("141e9181-4b17-4eac-a774-2c8fac4944aa", "SER")),
					new CodeDescriptionPair(AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, Res.GetString("3144e48a-93a9-441c-a2d2-c6494b137d18", "EDU"))
				});
			return extraTypes;
		}

		protected override string GetDescriptionForQCTExtraTaxType() => Res.GetString("5e9e7746-047f-47a6-9c3a-c1acc3d2e01e", "SGST");
		protected override ResourceStringData GetExtraTaxOSAmountCaption() => Res.GetData("ab9b2a18-3479-4a04-9938-1fe2f780cb77", "SGST Amt", "SGST Amount", "");
		protected override ResourceStringData GetExtraTaxLocalAmountCaption() => Res.GetData("2619ed0e-04e2-4d25-9883-a6c9e074a941", "SGST Local");
		protected override ResourceStringData GetTaxOSAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|OSSER", "GST Amount", "CGST/IGST Amount", "");
		protected override ResourceStringData GetTaxLocalAmountCaption() => Res.GetData("Accounting|ExtraTaxTypes|LocalSER", "GST Local", "CGST/IGST Local", "");

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => Env.Instance.IsProductionSystem ? new ZDate(2021, 01, 01) : new ZDate(2020, 11, 01);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		public string GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.India;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			var list = new ComplianceSubTypeList();
			list.Add(ComplianceSubTypes.TXI);
			list.Add(ComplianceSubTypes.TXC);
			list.Add(ComplianceSubTypes.TXD);
			list.Add(ComplianceSubTypes.BSI);
			list.Add(ComplianceSubTypes.BSC);
			list.Add(ComplianceSubTypes.BSD);
			list.Add(ComplianceSubTypes.INV);
			list.Add(ComplianceSubTypes.CRD);
			list.Add(ComplianceSubTypes.DBN);
			list.Add(ComplianceSubTypes.RVI);
			list.Add(ComplianceSubTypes.RVC);
			list.Add(ComplianceSubTypes.RVD);
			list.Add(ComplianceSubTypes.XCI);
			list.Add(ComplianceSubTypes.XCC);
			list.Add(ComplianceSubTypes.XCD);

			return list;
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TXC = "TXC";
			public const string TXD = "TXD";
			public const string BSI = "BSI";
			public const string BSC = "BSC";
			public const string BSD = "BSD";
			public const string INV = "INV";
			public const string CRD = "CRD";
			public const string DBN = "DBN";
			public const string RVI = "RVI";
			public const string RVC = "RVC";
			public const string RVD = "RVD";
			public const string XCI = "XCI";
			public const string XCC = "XCC";
			public const string XCD = "XCD";
		}

		internal static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI => ResString.GetMultilingualString("INComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString TXC => ResString.GetMultilingualString("INComplianceSubTypeCodeList|TXC", "Tax Credit Note");
			public static MultilingualString TXD => ResString.GetMultilingualString("INComplianceSubTypeCodeList|TXD", "Tax Debit Note");
			public static MultilingualString BSI => ResString.GetMultilingualString("INComplianceSubTypeCodeList|BSI", "Bill of Supply");
			public static MultilingualString BSC => ResString.GetMultilingualString("INComplianceSubTypeCodeList|BSC", "Bill of Supply Credit Note");
			public static MultilingualString BSD => ResString.GetMultilingualString("INComplianceSubTypeCodeList|BSD", "Bill of Supply Debit Note");
			public static MultilingualString INV => ResString.GetMultilingualString("INComplianceSubTypeCodeList|INV", "Invoice");
			public static MultilingualString CRD => ResString.GetMultilingualString("INComplianceSubTypeCodeList|CRD", "Credit Note");
			public static MultilingualString DBN => ResString.GetMultilingualString("INComplianceSubTypeCodeList|DBN", "Debit Note");
			public static MultilingualString RVI => ResString.GetMultilingualString("INComplianceSubTypeCodeList|RVI", "Reverse Charge Invoice");
			public static MultilingualString RVC => ResString.GetMultilingualString("INComplianceSubTypeCodeList|RVC", "Reverse Charge Credit Note");
			public static MultilingualString RVD => ResString.GetMultilingualString("INComplianceSubTypeCodeList|RVD", "Reverse Charge Debit Note");
			public static MultilingualString XCI => ResString.GetMultilingualString("INComplianceSubTypeCodeList|XCI", "Reimbursement Invoice");
			public static MultilingualString XCC => ResString.GetMultilingualString("INComplianceSubTypeCodeList|XCC", "Reimbursement Credit Note");
			public static MultilingualString XCD => ResString.GetMultilingualString("INComplianceSubTypeCodeList|XCD", "Reimbursement Debit Note");
		}

		#region SuppressResourceStringsCheckRegion

		internal static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "Tax Invoice";
			public const string TXC = "Tax Credit Note";
			public const string TXD = "Tax Debit Note";
			public const string BSI = "Bill of Supply";
			public const string BSC = "Bill of Supply Credit Note";
			public const string BSD = "Bill of Supply Debit Note";
			public const string INV = "Invoice";
			public const string CRD = "Credit Note";
			public const string DBN = "Debit Note";
			public const string RVI = "Reverse Charge Invoice";
			public const string RVC = "Reverse Charge Credit Note";
			public const string RVD = "Reverse Charge Debit Note";
			public const string XCI = "Reimbursement Invoice";
			public const string XCC = "Reimbursement Credit Note";
			public const string XCD = "Reimbursement Debit Note";
		}

		internal static class ComplianceSubTypeInternalImplemenationNotes
		{
			public const string TXI = "Original Invoice that contains only taxes with Tax ID type RAT and INT.";
			public const string TXC = "Original or Amended Credit Note that contains only taxes with Tax ID type RAT and INT.";
			public const string TXD = "Amendment Invoice that contains only taxes with Tax ID type RAT and INT.";
			public const string BSI = "Original Invoice that contains only taxes with Tax ID type EXT.";
			public const string BSC = "Original or Amended Credit Note that contains only taxes with Tax ID type EXT.";
			public const string BSD = "Amendment Invoice that contains only taxes with Tax ID type EXT.";
			public const string INV = "Original Invoice that contains only taxes with Tax ID type NOT.";
			public const string CRD = "Original or Amended Credit Note that contains only taxes with Tax ID type NOT.";
			public const string DBN = "Amendment Invoice that contains only taxes with Tax ID type NOT.";
			public const string RVI = "Original Invoice that contains only taxes with Tax ID type REV.";
			public const string RVC = "Original or Amended Credit Note that contains only taxes with Tax ID type REV.";
			public const string RVD = "Amendment Invoice that contains only taxes with Tax ID type REV.";
			public const string XCI = "Original Invoice that contains only taxes with Tax ID type EXL.";
			public const string XCC = "Original or Amended Credit Note that contains only taxes with Tax ID type EXL.";
			public const string XCD = "Amendment Invoice that contains only taxes with Tax ID type EXL.";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNotes.TXI);
			public static ComplianceSubType TXC => new ComplianceSubType(ComplianceSubTypeCodes.TXC, () => ComplianceSubTypeDescriptions.TXC, () => ComplianceSubTypeLocalDescriptions.TXC, () => ComplianceSubTypeInternalImplemenationNotes.TXC);
			public static ComplianceSubType TXD => new ComplianceSubType(ComplianceSubTypeCodes.TXD, () => ComplianceSubTypeDescriptions.TXD, () => ComplianceSubTypeLocalDescriptions.TXD, () => ComplianceSubTypeInternalImplemenationNotes.TXD);
			public static ComplianceSubType BSI => new ComplianceSubType(ComplianceSubTypeCodes.BSI, () => ComplianceSubTypeDescriptions.BSI, () => ComplianceSubTypeLocalDescriptions.BSI, () => ComplianceSubTypeInternalImplemenationNotes.BSI);
			public static ComplianceSubType BSC => new ComplianceSubType(ComplianceSubTypeCodes.BSC, () => ComplianceSubTypeDescriptions.BSC, () => ComplianceSubTypeLocalDescriptions.BSC, () => ComplianceSubTypeInternalImplemenationNotes.BSC);
			public static ComplianceSubType BSD => new ComplianceSubType(ComplianceSubTypeCodes.BSD, () => ComplianceSubTypeDescriptions.BSD, () => ComplianceSubTypeLocalDescriptions.BSD, () => ComplianceSubTypeInternalImplemenationNotes.BSD);
			public static ComplianceSubType INV => new ComplianceSubType(ComplianceSubTypeCodes.INV, () => ComplianceSubTypeDescriptions.INV, () => ComplianceSubTypeLocalDescriptions.INV, () => ComplianceSubTypeInternalImplemenationNotes.INV);
			public static ComplianceSubType CRD => new ComplianceSubType(ComplianceSubTypeCodes.CRD, () => ComplianceSubTypeDescriptions.CRD, () => ComplianceSubTypeLocalDescriptions.CRD, () => ComplianceSubTypeInternalImplemenationNotes.CRD);
			public static ComplianceSubType DBN => new ComplianceSubType(ComplianceSubTypeCodes.DBN, () => ComplianceSubTypeDescriptions.DBN, () => ComplianceSubTypeLocalDescriptions.DBN, () => ComplianceSubTypeInternalImplemenationNotes.DBN);
			public static ComplianceSubType RVI => new ComplianceSubType(ComplianceSubTypeCodes.RVI, () => ComplianceSubTypeDescriptions.RVI, () => ComplianceSubTypeLocalDescriptions.RVI, () => ComplianceSubTypeInternalImplemenationNotes.RVI);
			public static ComplianceSubType RVC => new ComplianceSubType(ComplianceSubTypeCodes.RVC, () => ComplianceSubTypeDescriptions.RVC, () => ComplianceSubTypeLocalDescriptions.RVC, () => ComplianceSubTypeInternalImplemenationNotes.RVC);
			public static ComplianceSubType RVD => new ComplianceSubType(ComplianceSubTypeCodes.RVD, () => ComplianceSubTypeDescriptions.RVD, () => ComplianceSubTypeLocalDescriptions.RVD, () => ComplianceSubTypeInternalImplemenationNotes.RVD);
			public static ComplianceSubType XCI => new ComplianceSubType(ComplianceSubTypeCodes.XCI, () => ComplianceSubTypeDescriptions.XCI, () => ComplianceSubTypeLocalDescriptions.XCI, () => ComplianceSubTypeInternalImplemenationNotes.XCI);
			public static ComplianceSubType XCC => new ComplianceSubType(ComplianceSubTypeCodes.XCC, () => ComplianceSubTypeDescriptions.XCC, () => ComplianceSubTypeLocalDescriptions.XCC, () => ComplianceSubTypeInternalImplemenationNotes.XCC);
			public static ComplianceSubType XCD => new ComplianceSubType(ComplianceSubTypeCodes.XCD, () => ComplianceSubTypeDescriptions.XCD, () => ComplianceSubTypeLocalDescriptions.XCD, () => ComplianceSubTypeInternalImplemenationNotes.XCD);
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
					ErrorReporter.ReportOnce("IndiaComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
					break;
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefault(ComplianceSubTypeAttributionRuleConfigurationCollection defaultCollection)
		{
			var configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.BSI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithExemptTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.BSC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithExemptTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.BSD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithExemptTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.INV;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.CRD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.DBN;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.RVI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.RVC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.RVD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.XCI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.XCC;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.XCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.BSI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithExemptTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.BSC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithExemptTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.BSD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithExemptTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.INV;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.CRD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.DBN;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.AllWithNoReportTaxIDs;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.RVI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.RVC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.RVD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.XCI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.XCC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			configuration = defaultCollection.AddNew();
			configuration.Country = Constants.CountryCodes.India;
			configuration.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.XCD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;
		}

		#region SuppressResourceStringsCheckRegion

		public static class RuleSetCodes
		{
			public const string Default = "A";
		}

		static class RuleSetDescriptions
		{
			public const string Default = "Default India Rule Set";
		}

		#endregion

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing()
			=> EligibleComplianceSubTypeListForEInvoicing;

		static readonly ImmutableHashSet<string> EligibleComplianceSubTypeListForEInvoicing = new[]
		{
			ComplianceSubTypeCodes.TXI,
			ComplianceSubTypeCodes.TXC,
			ComplianceSubTypeCodes.TXD,
		}.ToImmutableHashSet();

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
			=> EligibleComplianceSubTypeListForEInvoicing.Contains(complianceSubType);

		#endregion

		#region IComplianceSubTypeAndNumberUpdateRules

		bool IComplianceSubTypeAndNumberUpdateRules.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed => false;

		ZString IComplianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(AccTransactionHeader[] transactionHeaders) => ZString.Empty;

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(AccTransactionHeader transactionHeader) => true;

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceDocDateAllowed(AccTransactionHeader transactionHeader) => false;

		#endregion

		public static bool UseComplianceNumberForEInvoicingMapping(ZDate complianceNumberAppliesFrom, ZDate invoiceDate)
			=> complianceNumberAppliesFrom != ZDate.Empty
			&& invoiceDate >= complianceNumberAppliesFrom;

		public static bool UseTransactionNumberForEInvoicingMapping(ZDate complianceNumberAppliesFrom, ZDate invoiceDate)
			=> !UseComplianceNumberForEInvoicingMapping(complianceNumberAppliesFrom, invoiceDate);

		public static MultilingualString CreateComplianceNumberValidationExceptionMessage(ZString invalidNumber, char invalidCharacter)
			=> ResString.GetMultilingualString("6ce6c8df-bd47-4593-baa4-62f514a93d58", "Generated Compliance Number '{0}' cannot start with '{1}'. Please check your Compliance Number sequence configuration.", invalidNumber, invalidCharacter);

		public static MultilingualString CreateTransactionNumberValidationExceptionMessage(ZString invalidNumber, char invalidCharacter)
			=> ResString.GetMultilingualString("267d3d0c-eff7-4c78-bd1a-5b1c7725eca4", "Generated Transaction Number '{0}' cannot start with '{1}'. Please check your Transaction Number sequence configuration.", invalidNumber, invalidCharacter);

		public const string InvalidTransactionNumberLeadingCharacters = "0/-,.\"':;";

		bool IOrgCusCodePredicateProvider.IncludeForPlaceOfSupplyTaxRegsistration(OrgCusCode orgCusCode) => ValidateOrgCusCodeForPlaceOfSupplyOrTaxRegistration(orgCusCode);

		bool IOrgCusCodePredicateProvider.IncludeForOrgHeaderTaxRegsistration(OrgCusCode orgCusCode) => ValidateOrgCusCodeForPlaceOfSupplyOrTaxRegistration(orgCusCode);

		public readonly static string UnreportedRegistrationNumber = "URP";

		bool ValidateOrgCusCodeForPlaceOfSupplyOrTaxRegistration(OrgCusCode customCode)
		{
			if (customCode != null
				&& customCode.OK_RN_NKCodeCountry.EqualsIgnoringCase(CountryCodes.India)
				&& customCode.OK_CodeType.EqualsIgnoringCase(OrgCusCode.CodeTypes.GSTCode)
				&& customCode.OK_CustomsRegNo.EqualsIgnoringCase(UnreportedRegistrationNumber))
			{
				return false;
			}
			return true;
		}
	}
}
