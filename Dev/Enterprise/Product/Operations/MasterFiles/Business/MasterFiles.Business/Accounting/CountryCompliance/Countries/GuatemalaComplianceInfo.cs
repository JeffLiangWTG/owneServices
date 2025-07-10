using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class GuatemalaComplianceInfo : CountryComplianceInfo, IComplianceSubTypeCodeProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Guatemala;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.IVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria;
		protected override bool? GetIsReciprocal() => true;

		protected override bool? GetDefaultValueForDisplayRecipientTaxIDRegistry() => true;

		#endregion

		#region IComplianceSubTypeCodeProvider

		IComplianceSubTypeList IComplianceSubTypeCodeProvider.GetComplianceSubTypes()
		{
			return new ComplianceSubTypeList
			{
				ComplianceSubTypes.TXI,
				ComplianceSubTypes.TCD,
				ComplianceSubTypes.TCR,
				ComplianceSubTypes.XCL,
				ComplianceSubTypes.XCR,
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TCD = "TCD";
			public const string TCR = "TCR";
			public const string XCL = "XCL";
			public const string XCR = "XCR";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI => ResString.GetMultilingualString("GTComplianceSubTypeCodeList|TXI", "Tax Invoice");
			public static MultilingualString TCD => ResString.GetMultilingualString("GTComplianceSubTypeCodeList|TCD", "Debit Note");
			public static MultilingualString TCR => ResString.GetMultilingualString("GTComplianceSubTypeCodeList|TCR", "Tax Credit Note");
			public static MultilingualString XCL => ResString.GetMultilingualString("GTComplianceSubTypeCodeList|XCL", "Reimbursement/Disbursement/Excluded Supply");
			public static MultilingualString XCR => ResString.GetMultilingualString("GTComplianceSubTypeCodeList|XCR", "Credit Note of Reimbursement/Disbursement/Excluded Supply");
		}

		#region SuppressResourceStringsCheckRegion

		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TXI = "Factura";
			public const string TCD = "Nota de Débito";
			public const string TCR = "Nota de Crédito";
			public const string XCL = "Nota de Remisión";
			public const string XCR = "Nota de Crédito de Nota de Remisión";
		}

		static class ComplianceSubTypeInternalImplementationNote
		{
			public const string TXI = "Used in both Receivables and Payables to sub-classify original Invoice Transactions.";
			public const string TCD = "Used in both Receivables and Payables to sub-classify Amending Invoice transactions.";
			public const string TCR = "Used in both Receivables and Payables to sub-classify Amending Credit Note transactions.";
			public const string XCL = "Used to formally identify Invoice transactions recorded purely for disbursement / reimbursement purposes.  Note: should be used in conjuction with the EXCLUDE tax ID.";
			public const string XCR = "Used to formally identify Credit Note  transactions recorded purely for disbursement / reimbursement purposes.  Note: should be used in conjuction with the EXCLUDE tax ID.";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplementationNote.TXI);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplementationNote.TCD);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplementationNote.TCR);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplementationNote.XCL);
			public static ComplianceSubType XCR => new ComplianceSubType(ComplianceSubTypeCodes.XCR, () => ComplianceSubTypeDescriptions.XCR, () => ComplianceSubTypeLocalDescriptions.XCR, () => ComplianceSubTypeInternalImplementationNote.XCR);
		}

		#endregion

	}
}
