using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class EcuadorComplianceInfo : CountryComplianceInfo, IComplianceSubTypeCodeProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Ecuador;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.EcuadorCodeTypes.RUC;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.EcuadorCodeTypes.RUC;
		protected override bool? GetIsReciprocal() => false;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TXV,
				ComplianceSubTypes.XCL,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TCD = "TCD";
			public const string TCR = "TCR";
			public const string TXI = "TXI";
			public const string TXV = "TXV";
			public const string XCL = "XCL";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TCD { get { return ResString.GetMultilingualString("ECComplianceSubTypeCodeList|TCD", "Debit Note"); } }
			public static MultilingualString TCR { get { return ResString.GetMultilingualString("ECComplianceSubTypeCodeList|TCR", "Tax Credit Note"); } }
			public static MultilingualString TXI { get { return ResString.GetMultilingualString("ECComplianceSubTypeCodeList|TXI", "Tax Invoice"); } }
			public static MultilingualString TXV { get { return ResString.GetMultilingualString("ECComplianceSubTypeCodeList|TXV", "Purchase Tax Voucher"); } }
			public static MultilingualString XCL { get { return ResString.GetMultilingualString("ECComplianceSubTypeCodeList|XCL", "Reimbursement/Disbursement/Excluded Supply"); } }
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TCD = "Nota de Débito";
			public const string TCR = "Nota de Crédito";
			public const string TXI = "Factura";
			public const string TXV = "Liquidación de Compras de Bienes y Prestación de Servicios";
			public const string XCL = "Liquidación de Reembolso";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TCD = "Used to identify Invoice (INV) transactions that amend/correct another Invoice transaction.";
			public const string TCR = "Used to identify Credit Note (CRD) transactions that amend/correct an Invoice transaction.";
			public const string TXI = "Used to identify Invoice (INV) transactions that require a \"Factura\" fiscal document.";
			public const string TXV = "Used to record Payables Invoices received from Foreign suppliers when the Login Company must generate the fiscal document explaining the Purchase.";
			public const string XCL = "Used to record Invoice (INV) and Credit Note (CRD) transactions recorded for Disbursement / Reimbursement / Excluded supply purposes and where no Fiscal Document is to be issued / reported. \"Liquidación de Reembolso\" is expected to be issued when transaction only contains Tax ID “EXCLUDE” charges, otherwise a standard compliance sub type document is issued (Invoice/Credit Note).";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR);
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI);
			public static ComplianceSubType TXV => new ComplianceSubType(ComplianceSubTypeCodes.TXV, () => ComplianceSubTypeDescriptions.TXV, () => ComplianceSubTypeLocalDescriptions.TXV, () => ComplianceSubTypeInternalImplemenationNote.TXV, LedgerOfUse.AP);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
		}

		#endregion
	}
}
