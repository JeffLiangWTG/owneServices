using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Interfaces.ComplianceSubTypes;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class KoreaSouthComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoEInvoicingGUIActionQueuePendingInvoice,
		IComplianceInfoEInvoicingRequeueHandler,
		IEInvoicingRegistryProvider,
		IComplianceSubTypeRulesWithMultipleRuleSetProvider,
		IComplianceSubTypeValidation,
		IComplianceSubTypeGUIProvider,
		IComplianceSubTypeTaxInvoiceRulePrecedenceProvider
	{
		public static class CodeTypes
		{
			public const string KBT = "KBT";
			public const string KBC = "KBC";
			public const string AEO = "AEO";
			public const string IndustrialParkCode = "IPC";
			public const string RoadNameCode = "RNA";
			public const string BuildingNumber = "BNO";
			public const string KoreanRegNoForResident = "01";
			//public const string PassportNo = "02"; Use 'PAS' (Passport Number) instead. It is added for all countries
			public const string KoreanRegNoForForeigner = "03";
			//public const string BusinessRegNo = "04"; Use 'GBR' (Government Business Registration Code) instead. It is added for all countries and has been one of primary codes
			public const string UnipassIDForIndividual = "05";
			public const string UnipassIDForOrganization = "06";
			public const string ForeignCompanyID = "07";
			public const string OfficeID = "08";
			public const string ECommerceCompanyID = "CEC";
			//public const string CorporationCode = "09";Use 'GCR (Corporation Code) instead. It is added for all countries and has been one of primary codes
			public const string CourierCompanyID = "SDC";
			public const string CertificateOfOriginExporterNumber = "10";
		}

		public const string ForeignerID = "9999999999999";
		public const string IndustrialParkCusCodeType = "INDPK";

		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.KoreaSouth;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.GovBusinessCode;
		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TZI,
				ComplianceSubTypes.CTI,
				ComplianceSubTypes.CZI,
				ComplianceSubTypes.NTI,
				ComplianceSubTypes.CNI,
				ComplianceSubTypes.ITI,
				ComplianceSubTypes.IZI,
				ComplianceSubTypes.INI,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "101";
			public const string TZI = "102";
			public const string CTI = "201";
			public const string CZI = "202";
			public const string NTI = "301";
			public const string CNI = "401";
			public const string ITI = "201";
			public const string IZI = "202";
			public const string INI = "401";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI { get { return ResString.GetMultilingualString("KRComplianceSubTypeCodeList|TXI", "Original Tax Invoice"); } }
			public static MultilingualString TZI { get { return ResString.GetMultilingualString("KRComplianceSubTypeCodeList|TZI", "Original Zero Tax Invoice"); } }
			public static MultilingualString CTI { get { return ResString.GetMultilingualString("KRComplianceSubTypeCodeList|CTI", "Amendment Credit Note of the Tax Invoice"); } }
			public static MultilingualString CZI { get { return ResString.GetMultilingualString("KRComplianceSubTypeCodeList|CZI", "Amendment Credit Note of the Zero Tax Invoice"); } }
			public static MultilingualString NTI { get { return ResString.GetMultilingualString("KRComplianceSubTypeCodeList|NTI", "Original Non-Tax Invoice"); } }
			public static MultilingualString CNI { get { return ResString.GetMultilingualString("KRComplianceSubTypeCodeList|CNI", "Amendment Credit Note of the Non-Tax Invoice"); } }
			public static MultilingualString ITI { get { return ResString.GetMultilingualString("KRComplianceSubTypeCodeList|ITI", "Amendment Invoice of the Tax Invoice"); } }
			public static MultilingualString IZI { get { return ResString.GetMultilingualString("KRComplianceSubTypeCodeList|IZI", "Amendment Invoice of the Zero Tax Invoice"); } }
			public static MultilingualString INI { get { return ResString.GetMultilingualString("KRComplianceSubTypeCodeList|INI", "Amendment Invoice of the Non-Tax Invoice"); } }
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "일반 세금계산서";
			public const string TZI = "영세율 세금계산서";
			public const string CTI = "수정 일반 세금계산서";
			public const string CZI = "수정 영세율 세금계산서";
			public const string NTI = "일반 계산서";
			public const string CNI = "수정 일반 계산서";
			public const string ITI = "수정 일반 세금계산서";
			public const string IZI = "수정 영세율 세금계산서";
			public const string INI = "일반 계산서";
		}

		static class ComplianceSubTypeInternalImplementationNote
		{
			public const string TXI = "Original Tax Invoice.";
			public const string TZI = "Original Zero Rated Invoice.";
			public const string CTI = "Credit Note created for original tax invoice as Amendment/Reversal.";
			public const string CZI = "Credit Note created for original Zero Rated invoice as Amendment/Reversal.";
			public const string NTI = "Original Non-Tax Invoice.";
			public const string CNI = "Credit Note created for original non-Tax invoice as Amendment/Reversal.";
			public const string ITI = "Invoice created for original tax invoice as Amendment.";
			public const string IZI = "Invoice created for original Zero Rated invoice as Amendment.";
			public const string INI = "Invoice created for original non-Tax invoice as Amendment.";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplementationNote.TXI, ledger: LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV, transactionCreatingMode: TransactionCreatingMode.Original);
			public static ComplianceSubType TZI => new ComplianceSubType(ComplianceSubTypeCodes.TZI, () => ComplianceSubTypeDescriptions.TZI, () => ComplianceSubTypeLocalDescriptions.TZI, () => ComplianceSubTypeInternalImplementationNote.TZI, ledger: LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV, transactionCreatingMode: TransactionCreatingMode.Original);
			public static ComplianceSubType CTI => new ComplianceSubType(ComplianceSubTypeCodes.CTI, () => ComplianceSubTypeDescriptions.CTI, () => ComplianceSubTypeLocalDescriptions.CTI, () => ComplianceSubTypeInternalImplementationNote.CTI, ledger: LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD, transactionCreatingMode: TransactionCreatingMode.Original | TransactionCreatingMode.Amending | TransactionCreatingMode.Reversal);
			public static ComplianceSubType CZI => new ComplianceSubType(ComplianceSubTypeCodes.CZI, () => ComplianceSubTypeDescriptions.CZI, () => ComplianceSubTypeLocalDescriptions.CZI, () => ComplianceSubTypeInternalImplementationNote.CZI, ledger: LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD, transactionCreatingMode: TransactionCreatingMode.Original | TransactionCreatingMode.Amending | TransactionCreatingMode.Reversal);
			public static ComplianceSubType NTI => new ComplianceSubType(ComplianceSubTypeCodes.NTI, () => ComplianceSubTypeDescriptions.NTI, () => ComplianceSubTypeLocalDescriptions.NTI, () => ComplianceSubTypeInternalImplementationNote.NTI, ledger: LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV, transactionCreatingMode: TransactionCreatingMode.Original);
			public static ComplianceSubType CNI => new ComplianceSubType(ComplianceSubTypeCodes.CNI, () => ComplianceSubTypeDescriptions.CNI, () => ComplianceSubTypeLocalDescriptions.CNI, () => ComplianceSubTypeInternalImplementationNote.CNI, ledger: LedgerOfUse.AR, transactionType: TransactionTypeOfUse.CRD, transactionCreatingMode: TransactionCreatingMode.Original | TransactionCreatingMode.Amending | TransactionCreatingMode.Reversal);
			public static ComplianceSubType ITI => new ComplianceSubType(ComplianceSubTypeCodes.ITI, () => ComplianceSubTypeDescriptions.ITI, () => ComplianceSubTypeLocalDescriptions.ITI, () => ComplianceSubTypeInternalImplementationNote.ITI, ledger: LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV, transactionCreatingMode: TransactionCreatingMode.Amending);
			public static ComplianceSubType IZI => new ComplianceSubType(ComplianceSubTypeCodes.IZI, () => ComplianceSubTypeDescriptions.IZI, () => ComplianceSubTypeLocalDescriptions.IZI, () => ComplianceSubTypeInternalImplementationNote.IZI, ledger: LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV, transactionCreatingMode: TransactionCreatingMode.Amending);
			public static ComplianceSubType INI => new ComplianceSubType(ComplianceSubTypeCodes.INI, () => ComplianceSubTypeDescriptions.INI, () => ComplianceSubTypeLocalDescriptions.INI, () => ComplianceSubTypeInternalImplementationNote.INI, ledger: LedgerOfUse.AR, transactionType: TransactionTypeOfUse.INV, transactionCreatingMode: TransactionCreatingMode.Amending);
		}

		#endregion

		#region IComplianceSubTypeRulesWithMultipleRuleSetProvider

		CodeDescriptionPairList IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetRuleSet()
		{
			var rulesetList = new CodeDescriptionPairList();
			rulesetList.AddPair(RuleSetCodes.DEFAULT, RuleSetDescriptions.DEFAULT);

			return rulesetList;
		}

		string IComplianceSubTypeRulesWithMultipleRuleSetProvider.GetDefaultRuleSet()
		{
			return RuleSetCodes.DEFAULT;
		}

		void IComplianceSubTypeRulesWithMultipleRuleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string ruleSetCode)
		{
			switch (ruleSetCode)
			{
				case null:
				case "":
				case RuleSetCodes.DEFAULT:
					AddDefaultComplianceSubTypeAttributionRules(collection);
					break;
				default:
					ErrorReporter.ReportOnce("KoreaSouthComplianceInfo_IncorrectRuleSetCode", $"Incorrect Rule Set Code: {ruleSetCode}.");
					break;
			}
		}

		void AddDefaultComplianceSubTypeAttributionRules(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			AddComplianceSubTypeAttributionRuleConfiguration(collection, ComplianceSubTypeCodes.TXI, TransactionTypes.Invoice, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL, OriginalRuleCodes.OriginalTransactionOnly, RuleSetCodes.DEFAULT);
			AddComplianceSubTypeAttributionRuleConfiguration(collection, ComplianceSubTypeCodes.TZI, TransactionTypes.Invoice, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount, OriginalRuleCodes.OriginalTransactionOnly, RuleSetCodes.DEFAULT);
			AddComplianceSubTypeAttributionRuleConfiguration(collection, ComplianceSubTypeCodes.CTI, TransactionTypes.CreditNote, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL, OriginalRuleCodes.AmendingReversalOnly, RuleSetCodes.DEFAULT, ComplianceSubTypeCodes.TXI);
			AddComplianceSubTypeAttributionRuleConfiguration(collection, ComplianceSubTypeCodes.CZI, TransactionTypes.CreditNote, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount, OriginalRuleCodes.AmendingReversalOnly, RuleSetCodes.DEFAULT, ComplianceSubTypeCodes.TZI);
			AddComplianceSubTypeAttributionRuleConfiguration(collection, ComplianceSubTypeCodes.NTI, TransactionTypes.Invoice, TaxInvoiceRuleCodes.AllWithExemptTaxIDs, OriginalRuleCodes.OriginalTransactionOnly, RuleSetCodes.DEFAULT);
			AddComplianceSubTypeAttributionRuleConfiguration(collection, ComplianceSubTypeCodes.CNI, TransactionTypes.CreditNote, TaxInvoiceRuleCodes.AllWithExemptTaxIDs, OriginalRuleCodes.AmendingReversalOnly, RuleSetCodes.DEFAULT, ComplianceSubTypeCodes.NTI);
			AddComplianceSubTypeAttributionRuleConfiguration(collection, ComplianceSubTypeCodes.ITI, TransactionTypes.Invoice, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL, OriginalRuleCodes.AmendingReversalOnly, RuleSetCodes.DEFAULT, ComplianceSubTypeCodes.TXI);
			AddComplianceSubTypeAttributionRuleConfiguration(collection, ComplianceSubTypeCodes.IZI, TransactionTypes.Invoice, TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount, OriginalRuleCodes.AmendingReversalOnly, RuleSetCodes.DEFAULT, ComplianceSubTypeCodes.TZI);
			AddComplianceSubTypeAttributionRuleConfiguration(collection, ComplianceSubTypeCodes.INI, TransactionTypes.Invoice, TaxInvoiceRuleCodes.AllWithExemptTaxIDs, OriginalRuleCodes.AmendingReversalOnly, RuleSetCodes.DEFAULT, ComplianceSubTypeCodes.NTI);
		}

		void AddComplianceSubTypeAttributionRuleConfiguration(ComplianceSubTypeAttributionRuleConfigurationCollection collection, ZString subType, ZString invoiceType, ZString taxInvoiceRule, ZString originalRule, ZString ruleSetCode, ZString? parentTransactionSubType = null)
		{
			var configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = subType;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = invoiceType;
			configuration.TaxInvoiceRule = taxInvoiceRule;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = originalRule;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.RuleSetCode = ruleSetCode;

			if (parentTransactionSubType.HasValue)
			{
				configuration.ParentTransactionSubType = parentTransactionSubType.Value;
			}
		}

		static class RuleSetCodes
		{
			public const string DEFAULT = "1";
		}

		#region SuppressResourceStringsCheckRegion

		static class RuleSetDescriptions
		{
			public const string DEFAULT = "Default Korea Rule Set";
		}

		#endregion

		#endregion

		#region IComplianceSubTypeTaxInvoiceRulePrecedenceProvider

		ZString[] IComplianceSubTypeTaxInvoiceRulePrecedenceProvider.ComplianceSubTypeTaxInvoiceRulePrecedenceList()
		{
			return new ZString[]
			{
				TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount,
				TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL,
				TaxInvoiceRuleCodes.All,
				ZString.Empty
			};
		}

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccTransactionHeaderReferenceSchema.Constants.AH1_Reference;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.KoreaSouth;

		#endregion

		#region IComplianceInfoEInvoicingGUIActionQueuePendingInvoice

		public ZString QueuePendingInvoiceMenuName => Res.GetString("1AD67B78-B1B4-4AE7-A4AE-B98856236249", "Authorize and Send");

		public ZString[] PivotStatusesEligibleForQueuing => new ZString[] { EInvoicingPivotState.Pending };

		public ZString PivotStatusesEligibleForQueuingErrorMessage => Res.GetString("533E751D-8A8A-40BC-95A9-A4BBF6EB6BC7", "You can only authorize and send transactions of which the E-Reporting status is 'PEN - Pending'.");

		#endregion

		#region IComplianceInfoEInvoicingRequeueHandler

		public bool IsPivotRequeueRestricted(ZString pivotState, out string warningMessage)
		{
			if (EInvoicingPivotState.Sent == pivotState)
			{
				warningMessage = Res.GetString("FA888913-C1FA-4A4D-89EE-B2F9C383F573", "Re-queuing transactions with 'SNT - Sent' status may cause duplicate invoicing. Do you really want to re-queue?");
				return true;
			}
			warningMessage = null;
			return false;
		}

		#endregion

		#region IEInvoicingRegistryProvider

		bool IEInvoicingRegistryProvider.ShouldAutoSetEReportingComplianceDate => true;

		#endregion

		#region IComplianceSubTypeValidation

		ZString IComplianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader)
		{
			if (FeatureControlHelper.IsKoreaSouthComplianceSubTypeFeatureEnabled && !transactionHeader.IsInDatabase && transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && transactionHeader.AH_TransactionType == TransactionTypes.Invoice && transactionHeader.AH_OriginalTransactionNum.IsEmpty && !transactionHeader.AH_ComplianceSubType.IsEmpty)
			{
				if (!new List<string> { ComplianceSubTypeCodes.TXI, ComplianceSubTypeCodes.TZI, ComplianceSubTypeCodes.NTI }.Contains(transactionHeader.AH_ComplianceSubType))
				{
					return Res.GetString("527A1FB6-7CC8-40F6-9D15-084C9C973DF0", "The Compliance Sub Type of the Invoice should be '101', '102' or '301'.");
				}
			}

			return ZString.Empty;
		}

		ZString IComplianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(AccTransactionHeader transactionHeader)
		{
			if (FeatureControlHelper.IsKoreaSouthComplianceSubTypeFeatureEnabled && !transactionHeader.IsInDatabase && transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && transactionHeader.AH_TransactionType == TransactionTypes.Invoice && !transactionHeader.AH_ComplianceSubType.IsEmpty && !transactionHeader.AH_ComplianceSubTypeInfo.HasErrors())
			{
				return Res.GetString("D696476D-F6A8-4412-8D32-0E11FA6466C5", "You have selected a Compliance Sub Type manually. Please note that incorrect Compliance Sub Type allocation may result in a failure E-Reporting submission.");
			}
			return ZString.Empty;
		}

		#endregion

		#region IComplianceSubTypeGUIProvider

		bool IComplianceSubTypeGUIProvider.ComplianceSubTypeIsReadOnly(bool hasBeenCreatedAsAmending, string ledger, string transactionType, bool originalTransactionReferenceIsEmpty)
		{
			return ledger == LedgerTypes.AccountsPayable || !(!hasBeenCreatedAsAmending && transactionType == TransactionTypes.Invoice && originalTransactionReferenceIsEmpty);
		}

		bool IComplianceSubTypeGUIProvider.ShouldClearComplianceSubType(ZGuid originalTransactionReference)
		{
			return originalTransactionReference.IsEmpty;
		}

		#endregion
	}
}
