using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class PortugalComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		ITaxMessagesGroupProvider,
		IQRCodeDataProvider,
		IComplianceNumberSequenceConfigurationProvider,
		IComplianceRegistryDefaultProvider,
		IOriginalInvoiceReference,
		ITransactionAuthorizationNumber,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceSequenceValidationProvider,
		IComplianceInfoElectronicInvoicing
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Portugal;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.IVA;
		protected override bool? GetIsReciprocal() => false;

		protected override string GetComplianceSequencePrefixErrorMessage() => Res.GetString("8912409d-949c-43cc-ae51-99a78c89d188", "Series Prefix must contain alphanumeric characters only and no spaces");

		protected override string GetComplianceSequencePrefixRegex() => (NoResString)"^[0-9a-zA-Z]*$";

		#endregion

		protected override bool GetIsTransactionSequencingRequired(string ledger, string transactionType)
			=> ledger == LedgerTypes.AccountsReceivable
				&& (transactionType == TransactionTypes.Invoice || transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.Receipt);

		protected override string GetComplianceVersionNo()
		{
			var currentVersionNumber = ReleaseInfo.Instance.VersionNumber;
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", currentVersionNumber.Major, currentVersionNumber.Minor, currentVersionNumber.Release);
		}

		public static string PortugalAccountCodeForMissingRegistrationNumber => (NoResString)"Consumidor final";

		public static string UnknownData => (NoResString)"Desconhecido";

		public static bool IsTaxRegistrationNumber(string countryCode, string codeType) => countryCode == Core.Constants.CountryCodes.Portugal && codeType == OrgCusCode.CodeTypes.IVA;

		public static bool IsComplianceSubTypeCompatibleWithSourceReference(string complianceSubType) => !string.IsNullOrEmpty(GetSourceReferencePrefix(complianceSubType));

		public static string GetSourceReferencePrefix(string complianceSubType)
		{
			switch (complianceSubType)
			{
				case ComplianceSubTypeCodes.LCR:
					return ReferencePrefixes.NCD;
				case ComplianceSubTypeCodes.LCD:
					return ReferencePrefixes.NDD;
				case ComplianceSubTypeCodes.LTX:
					return ReferencePrefixes.FTD;
				case ComplianceSubTypeCodes.TCM:
					return ReferencePrefixes.NCM;
				case ComplianceSubTypeCodes.TDM:
					return ReferencePrefixes.NDM;
				case ComplianceSubTypeCodes.TXM:
					return ReferencePrefixes.FTM;
				default:
					return string.Empty;
			}
		}

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => string.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => (NoResString)string.Empty;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => string.Empty;

		#endregion

		#region IOriginalInvoiceReference

		bool IOriginalInvoiceReference.ShouldShowOriginalInvoiceReferenceFields(string ledger, string transactionType)
			=> ((ledger == LedgerTypes.AccountsReceivable || ledger == LedgerTypes.AccountsPayable) && transactionType == TransactionTypes.CreditNote)
				|| (ledger == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.Invoice);

		bool IOriginalInvoiceReference.ShouldShowOriginalInvoiceReferenceDatesFields(string ledger, string transactionType)
			=> ledger == LedgerTypes.AccountsReceivable && (transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.Invoice);

		bool IOriginalInvoiceReference.ShouldShowOriginalInvoiceReferenceReasonFields(string ledger, string transactionType)
			=> ledger == LedgerTypes.AccountsReceivable && (transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.Invoice);

		bool IOriginalInvoiceReference.GetAreAllOriginalInvoiceReferenceFieldsEnabled(string ledger, string transactionType, string complianceSubType)
			=> ((ledger == LedgerTypes.AccountsReceivable || ledger == LedgerTypes.AccountsPayable) && transactionType == TransactionTypes.CreditNote)
				|| ((ledger == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.Invoice)
					&& (complianceSubType == ComplianceSubTypeCodes.LCD
						|| complianceSubType == ComplianceSubTypeCodes.TCD
						|| complianceSubType == ComplianceSubTypeCodes.TDM));

		bool IOriginalInvoiceReference.GetAreOriginalTransactionReferenceFieldsMandatory(string ledger, string transactionType)
			=> ledger == LedgerTypes.AccountsReceivable && (transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.Invoice);

		string IOriginalInvoiceReference.GetDocOriginalReferenceReason(string reasonDescription, ZDate originalReferenceStartDate, ZDate originalReferenceEndDate)
		{
			var result = reasonDescription;

			if (originalReferenceStartDate.IsValid && originalReferenceEndDate.IsValid)
			{
				var dateFormat = "yyyy-MM-dd";
				result = string.Format(CultureInfo.InvariantCulture, "{0} - {1}",
					originalReferenceStartDate.ToString(dateFormat, Culture.Invariant),
					originalReferenceEndDate.ToString(dateFormat, Culture.Invariant));
			}

			return result;
		}

		#endregion

		#region ITransactionAuthorizationNumber

		bool ITransactionAuthorizationNumber.IsTransactionAuthorizationNumberEnabled(ZGuid companyPK, ZDateTime transactionDate)
			=> IsTransactionAuthorizationNumberEnabled(companyPK, transactionDate);

		string ITransactionAuthorizationNumber.GetTransactionAuthorizationNumberLabel() => "ATCUD";

		ZDate ITransactionAuthorizationNumber.GetDefaultTransactionAuthorizationNumberFeatureEnabledStartDate() => new ZDate(2023, 1, 1);

		static bool IsTransactionAuthorizationNumberEnabled(ZGuid companyPK, ZDateTime transactionDate)
		{
			var complianceDate = AccountingMasterFilesRegistry.Instance.TransactionAuthorizationNumberDate.GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			return transactionDate.ToDateTime().Date >= complianceDate.Date;
		}

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.CBC,
				ComplianceSubTypes.CBD,
				ComplianceSubTypes.CBI,
				ComplianceSubTypes.LCD,
				ComplianceSubTypes.LCR,
				ComplianceSubTypes.LTX,
				ComplianceSubTypes.PCD,
				ComplianceSubTypes.PCR,
				ComplianceSubTypes.PTX,
				ComplianceSubTypes.SBC,
				ComplianceSubTypes.SBD,
				ComplianceSubTypes.SBI,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCM,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TDM,
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TXM,
				ComplianceSubTypes.XCL,
				ComplianceSubTypes.XCR,
				ComplianceSubTypes.XPC,
				ComplianceSubTypes.XPI,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string CBC = "CBC";
			public const string CBD = "CBD";
			public const string CBI = "CBI";
			public const string LCD = "LCD";
			public const string LCR = "LCR";
			public const string LTX = "LTX";
			public const string PCD = "PCD";
			public const string PCR = "PCR";
			public const string PTX = "PTX";
			public const string SBC = "SBC";
			public const string SBD = "SBD";
			public const string SBI = "SBI";
			public const string TCD = "TCD";
			public const string TCM = "TCM";
			public const string TCR = "TCR";
			public const string TDM = "TDM";
			public const string TXI = "TXI";
			public const string TXM = "TXM";
			public const string XCL = "XCL";
			public const string XCR = "XCR";
			public const string XPC = "XPC";
			public const string XPI = "XPI";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString CBC { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|CBC", "AR Self Billed Credit Note"); } }
			public static MultilingualString CBD { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|CBD", "AR Self Billed Debit Note"); } }
			public static MultilingualString CBI { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|CBI", "AR Self Billed Invoice"); } }
			public static MultilingualString LCD { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|LCD", "Data Loss Recovered Tax Debit Note"); } }
			public static MultilingualString LCR { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|LCR", "Data Loss Recovered Tax Credit Note"); } }
			public static MultilingualString LTX { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|LTX", "Data Loss Recovered Invoice"); } }
			public static MultilingualString PCD { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|PCD", "Payables Debit Note"); } }
			public static MultilingualString PCR { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|PCR", "Payables Tax Credit Note"); } }
			public static MultilingualString PTX { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|PTX", "Payables Tax Invoice"); } }
			public static MultilingualString SBC { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|SBC", "AP Self Billed Credit Note"); } }
			public static MultilingualString SBD { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|SBD", "AP Self Billed Debit Note"); } }
			public static MultilingualString SBI { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|SBI", "AP Self Billed Invoice"); } }
			public static MultilingualString TCD { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|TCD", "Tax Debit Note"); } }
			public static MultilingualString TCM { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|TCM", "Recovered Manual Credit Note"); } }
			public static MultilingualString TCR { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|TCR", "Tax Credit Note"); } }
			public static MultilingualString TDM { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|TDM", "Recovered Manual Debit Note"); } }
			public static MultilingualString TXI { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|TXI", "Tax Invoice"); } }
			public static MultilingualString TXM { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|TXM", "Recovered Manual Invoice"); } }
			public static MultilingualString XCL { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|XCL", "Disbursement Invoice Document"); } }
			public static MultilingualString XCR { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|XCR", "Disbursement Credit Note Document"); } }
			public static MultilingualString XPC { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|XPC", "Disbursement Credit Note Document Payables"); } }
			public static MultilingualString XPI { get { return ResString.GetMultilingualString("PTComplianceSubTypeCodeList|XPI", "Disbursement Invoice Document Payables"); } }
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string CBC = "Nota de Crédito Autofaturação AR";
			public const string CBD = "Nota de Débito Autofaturação AR";
			public const string CBI = "Fatura Autofaturação AR";
			public const string LCD = "Nota de Débito de Recuperação";
			public const string LCR = "Nota de Crédito de Recuperação";
			public const string LTX = "Fatura de Recuperação";
			public const string PCD = "Nota de Débito de Fornecedor";
			public const string PCR = "Nota de Crédito de Fornecedor";
			public const string PTX = "Fatura de Fornecedor";
			public const string SBC = "Nota de Crédito Autofaturação AP";
			public const string SBD = "Nota de Débito Autofaturação AP";
			public const string SBI = "Fatura Autofaturação AP";
			public const string TCD = "Nota de Débito";
			public const string TCM = "Nota de Crédito Manual";
			public const string TCR = "Nota de Crédito";
			public const string TDM = "Nota de Débito Manual";
			public const string TXI = "Fatura";
			public const string TXM = "Fatura Manual";
			public const string XCL = "Fatura de Reembolso";
			public const string XCR = "Nota de Crédito de Reembolso";
			public const string XPC = "Nota de Crédito de Reembolso AP";
			public const string XPI = "Fatura de Reembolso AP";
		}
		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string CBC = "Used in Receivables to sub-classify Self Billed Tax Credit Note transactions that amend a Fatura Autofaturação AR (CBI). Gives right to Input Tax Credit.";
			public const string CBD = "Used in Receivables to sub-classify Self Billed Tax Debit Note transactions, that amend a Nota de Crédito (CBC). Gives right to Input Tax Credit.";
			public const string CBI = "Used in Receivables to sub-classify original Self Billed Tax Invoice transactions. Gives right to Input Tax Credit.";
			public const string LCD = "Used in Receivables to sub-classify Debit notes that were originally issued in CW, but due to database issues, it is necessary to reissue the document again in CW";
			public const string LCR = "Used in Receivables to sub-classify credit notes that were originally issued in CW, but due to database issues, it is necessary to reissue the document again in CW";
			public const string LTX = "Used in Receivables to sub-classify invoice that were originally issued in CW, but due to database issues, it is necessary to reissue the document again in CW";
			public const string PCD = "Used in Payables to sub-classify amendment non-disbursement Invoice transactions that give right to input tax credit.";
			public const string PCR = "Used in Payables to sub-classify Amending/reversing non-disbursement Credit Note transactions that give right to input tax credit.";
			public const string PTX = "Used in Payables to sub-classify original non-disbursement Invoice transactions that give right to input tax credit.";
			public const string SBC = "Used in Payables to sub-classify Self Billed Tax Credit Note transactions that amend a Fatura Autofaturação AP (SBI). Gives right to Input Tax Credit.";
			public const string SBD = "Used in Payables to sub-classify Self Billed Tax Debit Note transactions, that amend a Nota de Crédito(SBC). Gives right to Input Tax Credit.";
			public const string SBI = "Used in Payables to sub-classify original Self Billed Tax Invoice transactions. Gives right to Input Tax Credit.";
			public const string TCD = "Used in Receivables to sub-classify original non-disbursement amending Invoice transactions that give right to input tax credit.";
			public const string TCM = "Used in Receivables to sub-classify credit notes that were originally issued manually due to inoperability of the system";
			public const string TCR = "Used in Receivables to sub-classify Amending/reversing Credit Note transactions that give right to input tax credit.";
			public const string TDM = "Used in Receivables to sub-classify debit notes that were originally issued manually due to inoperability of the system";
			public const string TXI = "Used in Receivables to sub-classify original non-disbursement Invoice transactions that give right to input tax credit.";
			public const string TXM = "Used in Receivables to sub-classify invoices that were originally issued manually due to inoperability of the system";
			public const string XCL = @"Used to formally identify Account Receivables 'Reimbursement' Invoice transactions. These transactions refer to receivables transactions when recovering amounts paid on behalf of the Login Company’s customers.
'Documento de Reembolso' is expected to be issued when transaction only contains Tax ID 'EXCLUDE' charges, otherwise a standard compliance sub type document is issued.";
			public const string XCR = @"Used to formally identify Account Receivables Disbursement Credit Note transactions. These transactions refer to receivables transactions when recovering amounts paid on behalf of the Login Company’s customers.
'Documento de Reembolso' is expected to be issued when transaction only contains Tax ID 'EXCLUDE” charges, otherwise a standard compliance sub type document is issued.";
			public const string XPC = @"Used to formally identify Account Payables Disbursement Credit Note transactions. These transactions refer to payables transactions when recording amounts paid on behalf of the Login Company’s customers that will be reimbursed.
'Documento de Reembolso' is expected to be issued when transaction only contains Tax ID 'EXCLUDE” charges, otherwise a standard compliance sub type document is issued.";
			public const string XPI = @"Used to formally identify Account Payables 'Reimbursement' Invoice transactions. These transactions refer to payables transactions when recording amounts paid on behalf of the Login Company’s customers that will to be reimbursed.
'Documento de Reembolso' is expected to be issued when transaction only contains Tax ID 'EXCLUDE” charges, otherwise a standard compliance sub type document is issued.";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType CBC => new ComplianceSubType(ComplianceSubTypeCodes.CBC, () => ComplianceSubTypeDescriptions.CBC, () => ComplianceSubTypeLocalDescriptions.CBC, () => ComplianceSubTypeInternalImplemenationNote.CBC, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType CBD => new ComplianceSubType(ComplianceSubTypeCodes.CBD, () => ComplianceSubTypeDescriptions.CBD, () => ComplianceSubTypeLocalDescriptions.CBD, () => ComplianceSubTypeInternalImplemenationNote.CBD, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType CBI => new ComplianceSubType(ComplianceSubTypeCodes.CBI, () => ComplianceSubTypeDescriptions.CBI, () => ComplianceSubTypeLocalDescriptions.CBI, () => ComplianceSubTypeInternalImplemenationNote.CBI, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType LCD => new ComplianceSubType(ComplianceSubTypeCodes.LCD, () => ComplianceSubTypeDescriptions.LCD, () => ComplianceSubTypeLocalDescriptions.LCD, () => ComplianceSubTypeInternalImplemenationNote.LCD, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType LCR => new ComplianceSubType(ComplianceSubTypeCodes.LCR, () => ComplianceSubTypeDescriptions.LCR, () => ComplianceSubTypeLocalDescriptions.LCR, () => ComplianceSubTypeInternalImplemenationNote.LCR, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType LTX => new ComplianceSubType(ComplianceSubTypeCodes.LTX, () => ComplianceSubTypeDescriptions.LTX, () => ComplianceSubTypeLocalDescriptions.LTX, () => ComplianceSubTypeInternalImplemenationNote.LTX, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PCD => new ComplianceSubType(ComplianceSubTypeCodes.PCD, () => ComplianceSubTypeDescriptions.PCD, () => ComplianceSubTypeLocalDescriptions.PCD, () => ComplianceSubTypeInternalImplemenationNote.PCD, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType PCR => new ComplianceSubType(ComplianceSubTypeCodes.PCR, () => ComplianceSubTypeDescriptions.PCR, () => ComplianceSubTypeLocalDescriptions.PCR, () => ComplianceSubTypeInternalImplemenationNote.PCR, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType PTX => new ComplianceSubType(ComplianceSubTypeCodes.PTX, () => ComplianceSubTypeDescriptions.PTX, () => ComplianceSubTypeLocalDescriptions.PTX, () => ComplianceSubTypeInternalImplemenationNote.PTX, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType SBC => new ComplianceSubType(ComplianceSubTypeCodes.SBC, () => ComplianceSubTypeDescriptions.SBC, () => ComplianceSubTypeLocalDescriptions.SBC, () => ComplianceSubTypeInternalImplemenationNote.SBC, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType SBD => new ComplianceSubType(ComplianceSubTypeCodes.SBD, () => ComplianceSubTypeDescriptions.SBD, () => ComplianceSubTypeLocalDescriptions.SBD, () => ComplianceSubTypeInternalImplemenationNote.SBD, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType SBI => new ComplianceSubType(ComplianceSubTypeCodes.SBI, () => ComplianceSubTypeDescriptions.SBI, () => ComplianceSubTypeLocalDescriptions.SBI, () => ComplianceSubTypeInternalImplemenationNote.SBI, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCM => new ComplianceSubType(ComplianceSubTypeCodes.TCM, () => ComplianceSubTypeDescriptions.TCM, () => ComplianceSubTypeLocalDescriptions.TCM, () => ComplianceSubTypeInternalImplemenationNote.TCM, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TDM => new ComplianceSubType(ComplianceSubTypeCodes.TDM, () => ComplianceSubTypeDescriptions.TDM, () => ComplianceSubTypeLocalDescriptions.TDM, () => ComplianceSubTypeInternalImplemenationNote.TDM, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXM => new ComplianceSubType(ComplianceSubTypeCodes.TXM, () => ComplianceSubTypeDescriptions.TXM, () => ComplianceSubTypeLocalDescriptions.TXM, () => ComplianceSubTypeInternalImplemenationNote.TXM, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType XCR => new ComplianceSubType(ComplianceSubTypeCodes.XCR, () => ComplianceSubTypeDescriptions.XCR, () => ComplianceSubTypeLocalDescriptions.XCR, () => ComplianceSubTypeInternalImplemenationNote.XCR, LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType XPC => new ComplianceSubType(ComplianceSubTypeCodes.XPC, () => ComplianceSubTypeDescriptions.XPC, () => ComplianceSubTypeLocalDescriptions.XPC, () => ComplianceSubTypeInternalImplemenationNote.XPC, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType XPI => new ComplianceSubType(ComplianceSubTypeCodes.XPI, () => ComplianceSubTypeDescriptions.XPI, () => ComplianceSubTypeLocalDescriptions.XPI, () => ComplianceSubTypeInternalImplemenationNote.XPI, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
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
			public const string Default = "Default Portugal Rule Set";
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
					ErrorReporter.ReportOnce("PortugalComplianceInfo_IncorrectRuleSetCode", "Incorrect Rule Set Code");
					break;
			}
		}

		void AddComplianceSubTypeAttributionRulesForDefault(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			//TCD
			ComplianceSubTypeAttributionRuleConfiguration configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			//TCR
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			//TXI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			//XCL
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			//SBI
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.SBI;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.SelfBillingRule = SelfBillingRuleCodes.SelfBillingTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			//SBC
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.SBC;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.SelfBillingRule = SelfBillingRuleCodes.SelfBillingTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;

			//SBD
			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.SBD;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.SelfBillingRule = SelfBillingRuleCodes.SelfBillingTransactions;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = RuleSetCodes.Default;
			configuration.RuleSetDescription = RuleSetDescriptions.Default;
		}

		#endregion

		#region ITaxMessagesGroupProvider
		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M00, Description = TaxMessageGroupDescriptions.M00, Bool = false },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M01, Description = TaxMessageGroupDescriptions.M01, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M01 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M02, Description = TaxMessageGroupDescriptions.M02, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M02 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M03, Description = TaxMessageGroupDescriptions.M03, Bool = false, RelatedItemCode = TaxMessageGroupCodes.M03 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M04, Description = TaxMessageGroupDescriptions.M04, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M04 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M05, Description = TaxMessageGroupDescriptions.M05, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M05 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M06, Description = TaxMessageGroupDescriptions.M06, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M06 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M07, Description = TaxMessageGroupDescriptions.M07, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M07 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M08, Description = TaxMessageGroupDescriptions.M08, Bool = false, RelatedItemCode = TaxMessageGroupCodes.M08 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M09, Description = TaxMessageGroupDescriptions.M09, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M09 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M10, Description = TaxMessageGroupDescriptions.M10, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M10 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M11, Description = TaxMessageGroupDescriptions.M11, Bool = false, RelatedItemCode = TaxMessageGroupCodes.M11 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M12, Description = TaxMessageGroupDescriptions.M12, Bool = false, RelatedItemCode = TaxMessageGroupCodes.M12 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M13, Description = TaxMessageGroupDescriptions.M13, Bool = false, RelatedItemCode = TaxMessageGroupCodes.M13 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M14, Description = TaxMessageGroupDescriptions.M14, Bool = false, RelatedItemCode = TaxMessageGroupCodes.M14 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M15, Description = TaxMessageGroupDescriptions.M15, Bool = false, RelatedItemCode = TaxMessageGroupCodes.M15 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M16, Description = TaxMessageGroupDescriptions.M16, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M16 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M19, Description = TaxMessageGroupDescriptions.M19, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M19 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M20, Description = TaxMessageGroupDescriptions.M20, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M20 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M21, Description = TaxMessageGroupDescriptions.M21, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M21 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M25, Description = TaxMessageGroupDescriptions.M25, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M25 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M30, Description = TaxMessageGroupDescriptions.M30, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M30 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M31, Description = TaxMessageGroupDescriptions.M31, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M31 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M32, Description = TaxMessageGroupDescriptions.M32, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M32 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M33, Description = TaxMessageGroupDescriptions.M33, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M33 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M40, Description = TaxMessageGroupDescriptions.M40, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M40 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M41, Description = TaxMessageGroupDescriptions.M41, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M41 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M42, Description = TaxMessageGroupDescriptions.M42, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M42 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M43, Description = TaxMessageGroupDescriptions.M43, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M43 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.M99, Description = TaxMessageGroupDescriptions.M99, Bool = true, RelatedItemCode = TaxMessageGroupCodes.M99 }
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string M00 = "M00";
			public const string M01 = "M01";
			public const string M02 = "M02";
			public const string M03 = "M03";
			public const string M04 = "M04";
			public const string M05 = "M05";
			public const string M06 = "M06";
			public const string M07 = "M07";
			public const string M08 = "M08";
			public const string M09 = "M09";
			public const string M10 = "M10";
			public const string M11 = "M11";
			public const string M12 = "M12";
			public const string M13 = "M13";
			public const string M14 = "M14";
			public const string M15 = "M15";
			public const string M16 = "M16";
			public const string M19 = "M19";
			public const string M20 = "M20";
			public const string M21 = "M21";
			public const string M25 = "M25";
			public const string M30 = "M30";
			public const string M31 = "M31";
			public const string M32 = "M32";
			public const string M33 = "M33";
			public const string M40 = "M40";
			public const string M41 = "M41";
			public const string M42 = "M42";
			public const string M43 = "M43";
			public const string M99 = "M99";
		}

		#region SuppressResourceStringsCheckRegion

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString M00 => (NoResString)"N\u00e3o Isen\u00e7\u00e3o";
			public static MultilingualString M01 => (NoResString)"Artigo 16.\u00ba n.\u00ba 6 al\u00ednea c) do CIVA";
			public static MultilingualString M02 => (NoResString)"Artigo 6.\u00ba do Decreto\u2010Lei n.\u00ba 198/90, de 19 de Junho";
			public static MultilingualString M03 => (NoResString)"Exigibilidade de caixa";
			public static MultilingualString M04 => (NoResString)"Isento Artigo 13.\u00ba do CIVA";
			public static MultilingualString M05 => (NoResString)"Isento Artigo 14.\u00ba do CIVA";
			public static MultilingualString M06 => (NoResString)"Isento Artigo 15.\u00ba do CIVA";
			public static MultilingualString M07 => (NoResString)"Isento Artigo 9.\u00ba do CIVA";
			public static MultilingualString M08 => (NoResString)"IVA \u2013 Autoliquida\u00e7\u00e3o";
			public static MultilingualString M09 => (NoResString)"IVA \u2010 n\u00e3o confere direito a dedu\u00e7\u00e3o";
			public static MultilingualString M10 => (NoResString)"IVA \u2013 Regime de isen\u00e7\u00e3o";
			public static MultilingualString M11 => (NoResString)"Regime particular do tabaco";
			public static MultilingualString M12 => (NoResString)"Regime da margem de lucro - Ag\u00eancias de Viagens";
			public static MultilingualString M13 => (NoResString)"Regime da margem de lucro \u2013 Bens em segunda m\u00e3o";
			public static MultilingualString M14 => (NoResString)"Regime da margem de lucro - Objetos de arte";
			public static MultilingualString M15 => (NoResString)"Regime da margem de lucro - Objetos de cole\u00e7\u00e3o e antiguidades";
			public static MultilingualString M16 => (NoResString)"Isento Artigo 14.\u00ba do RITI";
			public static MultilingualString M19 => (NoResString)"Outras isen\u00e7\u00f5es";
			public static MultilingualString M20 => (NoResString)"IVA - regime forfet\u00e1rio";
			public static MultilingualString M21 => (NoResString)"IVA – n\u00e3o confere direito \u00e0 dedu\u00e7\u00e3o (ou express\u00e3o similar)";
			public static MultilingualString M25 => (NoResString)"Mercadorias \u00e0 consigna\u00e7\u00e3o";
			public static MultilingualString M30 => (NoResString)"IVA - autoliquida\u00e7\u00e3o";
			public static MultilingualString M31 => (NoResString)"IVA - autoliquida\u00e7\u00e3o";
			public static MultilingualString M32 => (NoResString)"IVA - autoliquida\u00e7\u00e3o";
			public static MultilingualString M33 => (NoResString)"IVA - autoliquida\u00e7\u00e3o";
			public static MultilingualString M40 => (NoResString)"IVA - autoliquida\u00e7\u00e3o";
			public static MultilingualString M41 => (NoResString)"IVA - autoliquida\u00e7\u00e3o";
			public static MultilingualString M42 => (NoResString)"IVA - autoliquida\u00e7\u00e3o";
			public static MultilingualString M43 => (NoResString)"IVA - autoliquida\u00e7\u00e3o";
			public static MultilingualString M99 => (NoResString)"N\u00e3o sujeito; n\u00e3o tributado (ou similar)";
		}

		#endregion

		#endregion

		#region IComplianceNumberSequenceConfigurationProvider

		ComplianceNumberSequenceConfiguration[] IComplianceNumberSequenceConfigurationProvider.GetMandatoryComplianceNumberSequenceConfiguration()
		{
			var config = new ComplianceNumberSequenceConfiguration();
			config.Code = DefaultMandatoryComplianceNumberSequenceConfigurationCode.Code;
			config.Description = DefaultMandatoryComplianceNumberSequenceConfigurationCode.Description;

			AddNewConfigElement(config.Elements, ComplianceNumberSequenceCustomisationElement.ElementNames.ComplianceSubType, 1);
			AddNewConfigElement(config.Elements, ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace, 2, length: 1);
			AddNewConfigElement(config.Elements, ComplianceNumberSequenceCustomisationElement.ElementNames.SeriesPrefix, 3);
			AddNewConfigElement(config.Elements, ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1, 4, "/");
			AddNewConfigElement(config.Elements, ComplianceNumberSequenceCustomisationElement.ElementNames.SequenceNumber, 5);

			return new[] { config };

			void AddNewConfigElement(ComplianceNumberSequenceCustomisationElementCollection elements, string elementName, ZByte order, string digitCode = "", int length = 0)
			{
				var item = elements.Cast<ComplianceNumberSequenceCustomisationElement>().FirstOrDefault(x => x.ElementName == elementName) ?? elements.AddNew();
				item.Order = order;
				item.Include = true;
				item.ElementName = elementName;
				item.DigitCode = digitCode;

				if (length > 0)
				{
					item.Length = length;
				}
			}
		}

		CodeDescriptionPair IComplianceNumberSequenceConfigurationProvider.DefaultMandatoryComplianceNumberSequenceConfigurationCode
			=> DefaultMandatoryComplianceNumberSequenceConfigurationCode;

		CodeDescriptionPair DefaultMandatoryComplianceNumberSequenceConfigurationCode
			=> new CodeDescriptionPair("MPC", ResString.GetMultilingualString("86f759a0-246b-4ed4-b07c-c70b95d7dfef", "Mandatory Portugal Configuration"));

		ZQuery IComplianceNumberSequenceConfigurationProvider.GetComplianceSequenceFromTransactionReferenceFilter(ZString complianceSubType, ZString transactionReference, ZGuid companyPK)
		{
			var result = ZQuery.NoResultQuery;
			var prefixAndNumber = GetPrefixAndSequenceNumber(transactionReference);
			int maxLength = AccComplianceSequenceSchema.XD_Prefix.MaxLength;
			var prefix = prefixAndNumber.prefix.Length > maxLength ? prefixAndNumber.prefix.Substring(0, maxLength) : prefixAndNumber.prefix;

			ZDecimal numberParsed;
			if (ZDecimal.TryParse(prefixAndNumber.sequenceNumber, out numberParsed))
			{
				var complianceSequenceQuery = new ZDBOnlyQuery(typeof(AccComplianceSequence));
				complianceSequenceQuery.AddToFilter(AccComplianceSequenceSchema.XD_GC_Company, companyPK);
				var parentSubType = AccComplianceSequence.FindParentSubTypeInRegistry(complianceSubType);
				complianceSequenceQuery.AddToFilter(AccComplianceSequenceSchema.XD_SequenceClass, parentSubType.IsEmpty ? complianceSubType : parentSubType);

				var prefixSubquery = new ZQuery(AccComplianceSequenceSchema.XD_Prefix, prefix);
				prefixSubquery.AddToFilter(JoinCondition.Or, AccComplianceSequenceSchema.XD_Prefix, "");
				complianceSequenceQuery.AddToFilter(prefixSubquery);

				var startNumberFilter = string.Format("XD_StartNumber <= Convert(decimal(20, 0), {0})", numberParsed);
				complianceSequenceQuery.AddFilterAndZSQLParameterCollection(startNumberFilter, new ZSqlParameterCollection());

				var endNumberFilter = string.Format("XD_EndNumber >= Convert(decimal(20, 0), {0})", numberParsed);
				complianceSequenceQuery.AddFilterAndZSQLParameterCollection(endNumberFilter, new ZSqlParameterCollection());

				//AH_TransactionReference with MPC format will be: <compliancesubtype><blankspace><prefix><slash><sequenceNumber>
				//Length = <3><1><LEN(XD_Prefix)><1><XD_MaximumNumberDigits> = LEN(XD_Prefix) + XD_MaximumNumberDigits + 5
				var lengthFilter = string.Format("LEN(XD_Prefix) + XD_MaximumNumberDigits + 5 = {0}", transactionReference.Length);
				complianceSequenceQuery.AddFilterAndZSQLParameterCollection(lengthFilter, new ZSqlParameterCollection());
				result = complianceSequenceQuery;
			}
			return result;
		}

		string IComplianceNumberSequenceConfigurationProvider.GetSequenceNumber(string transactionReference) => GetPrefixAndSequenceNumber(transactionReference).sequenceNumber;

		(string prefix, string sequenceNumber) GetPrefixAndSequenceNumber(ZString transactionReference)
		{
			if (transactionReference.IsEmpty || !Regex.IsMatch(transactionReference, @"^[A-Z]{3} [0-9a-zA-Z]*\/[0-9]*$"))
			{
				return (string.Empty, string.Empty);
			}

			var splitTransactionReference = transactionReference.Split('/');
			var sequenceNumber = splitTransactionReference.Last();
			var prefix = transactionReference.Substring(transactionReference.IndexOf(' ') + 1, transactionReference.Length - sequenceNumber.Length - 5);

			return (prefix, sequenceNumber);
		}

		#endregion

		#region QR Code

		string IQRCodeDataProvider.GetTransactionQRCodeString(ITransactionQRCodeDataProvider transactionData) => IsTransactionReadyForQRCode(transactionData.GetTransactionDataForPortugalOnly_Obsolete_MustBeRefactoredToUseMembersOfThisInterface()) ? GenerateTransactionQRCode(transactionData.GetTransactionDataForPortugalOnly_Obsolete_MustBeRefactoredToUseMembersOfThisInterface()) : string.Empty;

		bool IsTransactionReadyForQRCode(ITransactionHeaderWithLines transactionHeaderWithLines) => !(transactionHeaderWithLines as AccTransactionHeader)?.AH_ComplianceSubType.IsEmpty ?? false;

		string GenerateTransactionQRCode(ITransactionHeaderWithLines transactionHeaderWithLines)
		{
			var transactionLines = transactionHeaderWithLines.Lines;
			var transactionHeader = transactionHeaderWithLines as AccTransactionHeader;

			return "A:" + string.Concat(transactionHeader.Company.GC_BusinessRegNo.ToString().Where(c => !char.IsWhiteSpace(c))) + Separator +
				"B:" + GetTaxID(transactionHeader.Header) + Separator +
				"C:" + transactionHeader.Header.CountryCode + Separator +
				"D:" + GetInvoiceType(transactionHeader.AH_ComplianceSubType) + Separator +
				"E:" + GetDocumentStatus(transactionHeader) + Separator +
				"F:" + transactionHeader.AH_InvoiceDate.ToString("yyyyMMdd") + Separator +
				"G:" + transactionHeader.AH_TransactionReference + Separator +
				"H:" + getATCUD(transactionHeader) + Separator +
				GetI1(transactionLines) +
				GetContinentalTaxExemptFromIVA(transactionLines) +
				GetExTaxAmounts("I3", transactionLines, "IVA6") +
				GetTaxAmounts("I4", transactionLines, "IVA6") +
				GetExTaxAmounts("I5", transactionLines, "IVA13") +
				GetTaxAmounts("I6", transactionLines, "IVA13") +
				GetExTaxAmounts("I7", transactionLines, "IVA", "CAPIVA") +
				GetTaxAmounts("I8", transactionLines, "IVA", "CAPIVA") +
				GetFiscalSpace("J1", transactionLines, TaxIDCountryRegions.Azores) +
				GetTerritoryTaxExemptFromIVA("J2", transactionLines, "IVA18") +
				GetExTaxAmounts("J7", transactionLines, "IVA18") +
				GetTaxAmounts("J8", transactionLines, "IVA18") +
				GetFiscalSpace("K1", transactionLines, TaxIDCountryRegions.Madeira) +
				GetTerritoryTaxExemptFromIVA("K2", transactionLines, "IVA22") +
				GetExTaxAmounts("K7", transactionLines, "IVA22") +
				GetTaxAmounts("K8", transactionLines, "IVA22") +
				GetTotalAmounts("L", transactionLines, "NOTREPORT", "EXCLUDE") +
				"N:" + FormatAmountNumber(transactionLines.Sum(y => y.LocalTaxAmount)) + Separator +
				"O:" + FormatAmountNumber(transactionLines.Sum(y => y.LocalTotalAmount)) + Separator +
				DigitalSignature(transactionHeader) +
				"R:" + AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.Value;
		}

		public static string GetInvoiceType(string complianceSubType)
		{
			switch (complianceSubType)
			{
				case ComplianceSubTypeCodes.TCD:
				case ComplianceSubTypeCodes.SBD:
				case ComplianceSubTypeCodes.LCD:
				case ComplianceSubTypeCodes.TDM:
				case ComplianceSubTypeCodes.PCD:
				case ComplianceSubTypeCodes.CBD:
					return "ND";
				case ComplianceSubTypeCodes.TCR:
				case ComplianceSubTypeCodes.TCM:
				case ComplianceSubTypeCodes.SBC:
				case ComplianceSubTypeCodes.XCR:
				case ComplianceSubTypeCodes.LCR:
				case ComplianceSubTypeCodes.PCR:
				case ComplianceSubTypeCodes.XPC:
				case ComplianceSubTypeCodes.CBC:
					return "NC";
				case ComplianceSubTypeCodes.TXI:
				case ComplianceSubTypeCodes.XCL:
				case ComplianceSubTypeCodes.TXM:
				case ComplianceSubTypeCodes.SBI:
				case ComplianceSubTypeCodes.LTX:
				case ComplianceSubTypeCodes.PTX:
				case ComplianceSubTypeCodes.XPI:
				case ComplianceSubTypeCodes.CBI:
					return "FT";
				default:
					throw new ArgumentException("Incorrect ComplianceSubType: " + complianceSubType);
			}
		}

		public static string GetTaxCountryRegion(string taxCode)
		{
			switch (taxCode)
			{
				case "IVA22":
					return TaxIDCountryRegions.Madeira;
				case "IVA18":
					return TaxIDCountryRegions.Azores;
				default:
					return TaxIDCountryRegions.Continental;
			}
		}

		public static string GetTaxType(string taxCode)
		{
			switch (taxCode)
			{
				case "NOTREPORT":
				case "EXCLUDE":
					return "NS";
				default:
					return "IVA";
			}
		}

		static string getATCUD(AccTransactionHeader transactionHeader)
		{
			var result = "0";

			if (IsTransactionAuthorizationNumberEnabled(transactionHeader.AH_GC, transactionHeader.AH_InvoiceDate))
			{
				var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH);
				query.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, transactionHeader.PK);
				var authorizationNumberReference = new BusinessObjectFactory().LoadTop1<AccTransactionHeaderReference>(query);

				if (!string.IsNullOrEmpty(authorizationNumberReference?.AH1_Reference))
				{
					result = authorizationNumberReference.AH1_Reference;
				}
			}

			return result;
		}

		#region QRCode sub methods

		class TaxIDCountryRegions
		{
			public static readonly string Continental = "PT";
			public static readonly string Azores = "PT-AC";
			public static readonly string Madeira = "PT-MA";
		}

		public static class ReferencePrefixes
		{
			public const string FTD = "FTD";
			public const string FTM = "FTM";
			public const string NCD = "NCD";
			public const string NCM = "NCM";
			public const string NDD = "NDD";
			public const string NDM = "NDM";
		}

		static readonly ReadOnlyCollection<string> ContinentalTaxCodes = new ReadOnlyCollection<string>(new List<string> { "IVA", "IVA6", "IVA13", "CAPIVA", "FREEIVA" });

		readonly string Separator = "*";

		string GetDocumentStatus(AccTransactionHeader transactionHeader) => transactionHeader.IsSelfBillingInvoice ? "S" : "N";

		string GetFiscalSpace(string messageCode, ITransactionLine[] transactionLines, string taxIDCountryRegion)
			=> transactionLines.Any(x => GetTaxCountryRegion(x.TaxRateCode) == taxIDCountryRegion)
				? messageCode + ":" + taxIDCountryRegion + Separator
				: string.Empty;

		string GetI1(ITransactionLine[] transactionLines) =>
			transactionLines.All(x => GetTaxType(x.TaxRateCode) == "NS")
				? "I1:0" + Separator
				: GetFiscalSpace("I1", transactionLines, TaxIDCountryRegions.Continental);

		string GetContinentalTaxExemptFromIVA(ITransactionLine[] transactionLines)
		{
			var result = string.Empty;
			var taxableCodesExemptFromIVA = new List<string> { "EXEMPT", "FREEIVA", "IVAREV6", "IVAREV13", "IVAREV", "FREEIVAREV" };
			var i2Amount = transactionLines.Where(x => taxableCodesExemptFromIVA.Contains(x.TaxRateCode)).Sum(y => y.LocalExTaxAmount);

			if (transactionLines.Any(x => ContinentalTaxCodes.Contains(x.TaxRateCode)) && i2Amount > 0)
			{
				result = "I2:" + FormatAmountNumber(i2Amount) + Separator;
			}

			return result;
		}

		string GetTerritoryTaxExemptFromIVA(string messageCode, ITransactionLine[] transactionLines, string territoryTaxID)
		{
			var result = string.Empty;

			if (!transactionLines.Any(x => ContinentalTaxCodes.Contains(x.TaxRateCode)) && transactionLines.Any(x => x.TaxRateCode == territoryTaxID))
			{
				result = messageCode + ":" + FormatAmountNumber(transactionLines.Where(x => x.TaxRateCode == "EXEMPT").Sum(y => y.LocalTotalAmount)) + Separator;
			}

			return result;
		}

		string GetTotalAmounts(string messageCode, ITransactionLine[] transactionLines, params string[] taxIDs)
		{
			var result = string.Empty;

			if (transactionLines.Any(x => taxIDs.ToList().Contains(x.TaxRateCode)))
			{
				result = messageCode + ":" + FormatAmountNumber(transactionLines.Where(x => taxIDs.ToList().Contains(x.TaxRateCode)).Sum(y => y.LocalTotalAmount)) + Separator;
			}

			return result;
		}

		string GetExTaxAmounts(string messageCode, ITransactionLine[] transactionLines, params string[] taxIDs)
		{
			var result = string.Empty;

			if (transactionLines.Any(x => taxIDs.ToList().Contains(x.TaxRateCode)))
			{
				result = messageCode + ":" + FormatAmountNumber(transactionLines.Where(x => taxIDs.ToList().Contains(x.TaxRateCode)).Sum(y => y.LocalExTaxAmount)) + Separator;
			}

			return result;
		}

		string GetTaxAmounts(string messageCode, ITransactionLine[] transactionLines, params string[] taxIDs)
		{
			var result = string.Empty;

			if (transactionLines.Any(x => taxIDs.ToList().Contains(x.TaxRateCode)))
			{
				result = messageCode + ":" + FormatAmountNumber(transactionLines.Where(x => taxIDs.ToList().Contains(x.TaxRateCode)).Sum(y => y.LocalTaxAmount)) + Separator;
			}

			return result;
		}

		string DigitalSignature(AccTransactionHeader transactionHeader)
		{
			var result = string.Empty;

			if (transactionHeader.AH_DigitalSignature_COMPRESSED.IsValid)
			{
				var digitalSignature = Convert.ToBase64String(transactionHeader.AH_DigitalSignature_COMPRESSED);
				if (digitalSignature.Length > 30)
				{
					result = "Q:" + digitalSignature[0].ToString() + digitalSignature[10].ToString() +
						digitalSignature[20].ToString() + digitalSignature[30].ToString() + Separator;
				}
			}

			return result;
		}

		string FormatAmountNumber(decimal value) => Utilities.Round(value, 2).ToString("0.00", CultureInfo.InvariantCulture);

		string GetTaxID(OrgHeader orgHeader)
		{
			var result = AccountingCountrySpecificValidationHelper.EmptyPortugalIVA;

			var ptIvaTaxRegistrationNumber = GetCustomsRegNo(orgHeader, OrgCusCode.CodeTypes.IVA, CountryCode);

			var countryCode = string.Empty;
			var prefixForTaxRegistrationNumber = CountryCode;
			if (ptIvaTaxRegistrationNumber != null)
			{
				countryCode = CountryCode;
				result = ptIvaTaxRegistrationNumber;
			}
			else
			{
				var homeCountryRegistration = orgHeader.GetCountryCodeAndTaxRegistrationWithoutPrefix(orgHeader.MainAddress);

				if (!homeCountryRegistration.countryCode.IsEmpty && !homeCountryRegistration.registrationNumber.IsEmpty)
				{
					countryCode = homeCountryRegistration.countryCode;
					prefixForTaxRegistrationNumber = RefCountry.GetPrefixForTaxRegistrationCode(homeCountryRegistration.countryCode);
					result = homeCountryRegistration.registrationNumber;
				}
			}

			if (result.StartsWith(prefixForTaxRegistrationNumber, StringComparison.OrdinalIgnoreCase))
			{
				var country = orgHeader.CountryCode == countryCode ? orgHeader.Country : orgHeader.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
				if (country?.IsPartOfEuropeanUnion ?? false)
				{
					result = result.Substring(prefixForTaxRegistrationNumber.Length);
				}
			}

			return result;
		}

		string GetCustomsRegNo(OrgHeader header, string codeType, string countryCode)
		{
			return header?.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_CodeType == codeType && x.OK_RN_NKCodeCountry == countryCode).Select(x => x.OK_CustomsRegNo).FirstOrDefault() ?? string.Empty;
		}

		#endregion

		#endregion

		#region Registry Defaults

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry() => AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code;

		string IComplianceRegistryDefaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(bool isEInvoicingEnabled)
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotChangeDefaultValue(proposedValue, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, CountryCode);

		string IComplianceRegistryDefaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(string proposedValue, bool isEInvoicingEnabled)
			=> ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotChangeDefaultValue(proposedValue, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, CountryCode);

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnableGovernmentChargeCodeRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() => null;

		bool? IComplianceRegistryDefaultProvider.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() => null;

		#endregion

		#region IComplianceSequencesValidationProvider

		const int MinimumPrintingAuthorizationNumberLength = 8;
		const string PrintingAuthorizationNumberRegEx = "^[B-DF-HJ-NP-TV-Z2-9]*$";

		bool IComplianceSequenceValidationProvider.IsPrintingAuthorizationNumberLengthValid(string number)
		{
			return (number.Length >= MinimumPrintingAuthorizationNumberLength);
		}

		bool IComplianceSequenceValidationProvider.IsPrintingAuthorizationNumberFormatValid(string number)
		{
			return Regex.IsMatch(number, PrintingAuthorizationNumberRegEx);
		}

		AccComplianceSequenceValidation IComplianceSequenceValidationProvider.GetAccComplianceSequenceValidation(AccComplianceSequence sequence)
		{
			return new AccComplianceSequencePortugalValidation(sequence);
		}

		#endregion

	}
}
