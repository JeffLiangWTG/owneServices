using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.ChileOrgCusCodeInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ChileComplianceInfo : CountryComplianceInfo,
		IComplianceSubTypeCodeProvider,
		IComplianceSubTypeRuleProvider,
		IComplianceInfoElectronicInvoicing,
		IComplianceInfoElectronicInvoicingEligibleSubType,
		IComplianceSubTypeAndNumberUpdateRules
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Chile;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.RUT;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.BHE,
				ComplianceSubTypes.BOL,
				ComplianceSubTypes.DCD,
				ComplianceSubTypes.DCR,
				ComplianceSubTypes.DEX,
				ComplianceSubTypes.DXI,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TEX,
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.XCL,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string BHE = "BHE";
			public const string BOL = "BOL";
			public const string DCD = "DCD";
			public const string DCR = "DCR";
			public const string DEX = "DEX";
			public const string DXI = "DXI";
			public const string TCD = "TCD";
			public const string TCR = "TCR";
			public const string TEX = "TEX";
			public const string TXI = "TXI";
			public const string XCL = "XCL";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString BHE { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|BHE", "Electronic Simplified Fee Invoice"); } }
			public static MultilingualString BOL { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|BOL", "Simplified Invoice"); } }
			public static MultilingualString DCD { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|DCD", "Electronic Tax Debit Note"); } }
			public static MultilingualString DCR { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|DCR", "Electronic Tax Credit Note"); } }
			public static MultilingualString DEX { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|DEX", "Electronic Tax Invoice - No Input Tax Credit"); } }
			public static MultilingualString DXI { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|DXI", "Electronic Tax Invoice"); } }
			public static MultilingualString TCD { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|TCD", "Tax Debit Note"); } }
			public static MultilingualString TCR { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|TCR", "Tax Credit Note"); } }
			public static MultilingualString TEX { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|TEX", "Tax Invoice - No Input Tax Credit"); } }
			public static MultilingualString TXI { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|TXI", "Tax Invoice"); } }
			public static MultilingualString XCL { get { return ResString.GetMultilingualString("CLComplianceSubTypeCodeList|XCL", "Reimbursement / Disbursement / Excluded Supply"); } }
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string BHE = "Boleta de Honorarios Electrónica";
			public const string BOL = "Boleta (Otras Boletas)";
			public const string DCD = "Nota de Débito Electrónica";
			public const string DCR = "Nota de Crédito Electrónica";
			public const string DEX = "Factura No Afecta o Exenta Electrónica";
			public const string DXI = "Factura Electrónica";
			public const string TCD = "Nota de Débito";
			public const string TCR = "Nota de Crédito";
			public const string TEX = "Factura de Ventas Y Servicios no Afectos O Exentos de I.V.A";
			public const string TXI = "Factura";
			public const string XCL = "Documento de Reembolso";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string BHE = "Used when recording an Account Payable Boleta de Honorarios Electrónica invoice received from a supplier. This compliance sub type is issued by a natural person or society that exercise liberal professions.";
			public const string BOL = "Used in Payables when recording a document labelled as 'Boleta' (e.g.. 'Boleta de Compraventa y Servicios’) Note: Received ‘Boleta de Honorarios’ should be recorded using the BHE compliance sub type. ";
			public const string DCD = "Used in both Receivables and Payables to sub-classify Amending/reversing Invoice (INV) transactions.";
			public const string DCR = "Used in both Receivables and Payables to sub-classify Amending/reversing Credit Note (CRD) transactions.";
			public const string DEX = "Used in both Receivables and Payables to sub-classify Original Invoice (INV) transactions that DO NOT contain an amount of VAT.";
			public const string DXI = "Used in both Receivables and Payables to sub-classify Original Invoice (INV) transactions containing an amount of VAT.";
			public const string TCD = "Used in Payables when the Chile supplier has issued a NOTA DE DÉBITO. This is a paper version of CL DCD. Chile Login companies are not expected to issue this sub type.";
			public const string TCR = "Used in Payables when the Chile supplier has issued a NOTA DE CRÉDITO. This is a paper version of CL DCR. Chile Login companies are not expected to issue this sub type.";
			public const string TEX = "Used in Payables when the Chile supplier has issued a FACTURA NO AFECTA O EXENTA. This is a paper version of CL DEX. Chile Login companies are not expected to issue this sub type.";
			public const string TXI = "Used in Payables when the Chile supplier has issued a FACTURA. This is a paper version of CL DXI. Chile Login companies are not expected to issue this sub type.";
			public const string XCL = "Used to formally identify Reimbursement / Disbursement / Excluded Supply transactions. Used in conjuction with the Exclude Tax ID. This type is used to identify transctions where no reportable supply was made or received and no fiscal document is required.";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType BHE => new ComplianceSubType(ComplianceSubTypeCodes.BHE, () => ComplianceSubTypeDescriptions.BHE, () => ComplianceSubTypeLocalDescriptions.BHE, () => ComplianceSubTypeInternalImplemenationNote.BHE, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType BOL => new ComplianceSubType(ComplianceSubTypeCodes.BOL, () => ComplianceSubTypeDescriptions.BOL, () => ComplianceSubTypeLocalDescriptions.BOL, () => ComplianceSubTypeInternalImplemenationNote.BOL, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType DCD => new ComplianceSubType(ComplianceSubTypeCodes.DCD, () => ComplianceSubTypeDescriptions.DCD, () => ComplianceSubTypeLocalDescriptions.DCD, () => ComplianceSubTypeInternalImplemenationNote.DCD, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType DCR => new ComplianceSubType(ComplianceSubTypeCodes.DCR, () => ComplianceSubTypeDescriptions.DCR, () => ComplianceSubTypeLocalDescriptions.DCR, () => ComplianceSubTypeInternalImplemenationNote.DCR, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType DEX => new ComplianceSubType(ComplianceSubTypeCodes.DEX, () => ComplianceSubTypeDescriptions.DEX, () => ComplianceSubTypeLocalDescriptions.DEX, () => ComplianceSubTypeInternalImplemenationNote.DEX, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType DXI => new ComplianceSubType(ComplianceSubTypeCodes.DXI, () => ComplianceSubTypeDescriptions.DXI, () => ComplianceSubTypeLocalDescriptions.DXI, () => ComplianceSubTypeInternalImplemenationNote.DXI, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.CRD);
			public static ComplianceSubType TEX => new ComplianceSubType(ComplianceSubTypeCodes.TEX, () => ComplianceSubTypeDescriptions.TEX, () => ComplianceSubTypeLocalDescriptions.TEX, () => ComplianceSubTypeInternalImplemenationNote.TEX, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI, LedgerOfUse.AP, transactionType: TransactionTypeOfUse.INV);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
		}

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => ZDate.Empty;

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Chile;

		#endregion

		#region IComplianceInfoElectronicInvoicingEligibleSubType

		bool IComplianceInfoElectronicInvoicingEligibleSubType.IsComplianceSubTypeElegibleForEInvoicing(ZString complianceSubType)
		{
			return GetEInvoicingEligibleComplianceSubTypeList().Contains(complianceSubType);
		}

		IReadOnlyCollection<string> IComplianceInfoElectronicInvoicingEligibleSubType.GetEligibleComplianceSubTypeListForEInvoicing() => GetEInvoicingEligibleComplianceSubTypeList();

		public static HashSet<string> GetEInvoicingEligibleComplianceSubTypeList()
		{
			return new HashSet<string>
			{
				ComplianceSubTypeCodes.DXI,
				ComplianceSubTypeCodes.DCR,
				ComplianceSubTypeCodes.DCD,
				ComplianceSubTypeCodes.DEX
			};
		}

		#endregion

		#region IComplianceSubTypeRuleProvider

		void IComplianceSubTypeRuleProvider.SetComplianceSubTypeAttributionRuleConfigurations(ComplianceSubTypeAttributionRuleConfigurationCollection collection)
		{
			#region AR

			ComplianceSubTypeAttributionRuleConfiguration configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DCD;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DXI;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DCR;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DEX;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DEX;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCR;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DCD;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DEX;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.DCD;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.DCR;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AmendingReversalOnly;
			configuration.ParentTransactionSubType = ComplianceSubTypeCodes.XCL;

			#endregion

			#region AP

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;

			configuration = collection.AddNew();
			configuration.Country = CountryCode;
			configuration.SubType = ComplianceSubTypeCodes.XCL;
			configuration.LedgerType = LedgerTypes.AccountsPayable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.OrganisationLocation = ZString.Empty;

			#endregion AP
		}

		#endregion

		#region IComplianceSubTypeAndNumberUpdateRules

		bool IComplianceSubTypeAndNumberUpdateRules.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed => true;

		ZString IComplianceSubTypeAndNumberUpdateRules.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(AccTransactionHeader[] transactionHeaders) => ZString.Empty;

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceNumberAllowed(AccTransactionHeader transactionHeader) => true;

		bool IComplianceSubTypeAndNumberUpdateRules.IsComplianceDocDateAllowed(AccTransactionHeader transactionHeader) => false;

		#endregion
	}
}
