using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.HondurasOrgCusCodeInfo;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class HondurasComplianceInfo : CountryComplianceInfo, IComplianceSubTypeCodeProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Honduras;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCodes.RTN;
		protected override string GetConsumptionTaxCode() => OrgCusCodes.ISV;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCodes.RTN;
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
			};
		}

		public static class ComplianceSubTypeCodes
		{
			public const string TXI = "TXI";
			public const string TCD = "TCD";
			public const string TCR = "TCR";
			public const string XCL = "XCL";
		}

		static class ComplianceSubTypeDescriptions
		{
			public static MultilingualString TXI { get { return ResString.GetMultilingualString("HNComplianceSubTypeCodeList|TXI", "Tax Invoice"); } }
			public static MultilingualString TCD { get { return ResString.GetMultilingualString("HNComplianceSubTypeCodeList|TCD", "Debit Note"); } }
			public static MultilingualString TCR { get { return ResString.GetMultilingualString("HNComplianceSubTypeCodeList|TCR", "Tax Credit Note"); } }
			public static MultilingualString XCL { get { return ResString.GetMultilingualString("HNComplianceSubTypeCodeList|XCL", "Reimbursement/Disbursement/Excluded Supply"); } }
		}

		#region SuppressResourceStringsCheckRegion
		static class ComplianceSubTypeLocalDescriptions
		{
			public const string TCD = "Nota de Débito";
			public const string TCR = "Nota de Crédito";
			public const string TXI = "Factura";
			public const string XCL = "Documento de Reembolso";
		}

		static class ComplianceSubTypeInternalImplemenationNote
		{
			public const string TXI = "Used in both Receivables and Payables to sub-classify Original Invoice (INV) transactions. ";
			public const string TCD = "Used in both Receivables and Payables to sub-classify Amending/Reversing Invoice (INV) transactions. ";
			public const string TCR = "Used in both Receivables and Payables to sub-classify Amending/reversing Credit Note (CRD) transactions.";
			public const string XCL = "Used to formally identify Reimbursement / Disbursement / Excluded Supply transactions. Used in conjuction with the Exclude TAx ID. This type is used to identify transctions where no reportable supply was made or received and no fiscal document is required. ";
		}

		#endregion

		static class ComplianceSubTypes
		{
			public static ComplianceSubType TXI => new ComplianceSubType(ComplianceSubTypeCodes.TXI, () => ComplianceSubTypeDescriptions.TXI, () => ComplianceSubTypeLocalDescriptions.TXI, () => ComplianceSubTypeInternalImplemenationNote.TXI);
			public static ComplianceSubType TCD => new ComplianceSubType(ComplianceSubTypeCodes.TCD, () => ComplianceSubTypeDescriptions.TCD, () => ComplianceSubTypeLocalDescriptions.TCD, () => ComplianceSubTypeInternalImplemenationNote.TCD);
			public static ComplianceSubType TCR => new ComplianceSubType(ComplianceSubTypeCodes.TCR, () => ComplianceSubTypeDescriptions.TCR, () => ComplianceSubTypeLocalDescriptions.TCR, () => ComplianceSubTypeInternalImplemenationNote.TCR);
			public static ComplianceSubType XCL => new ComplianceSubType(ComplianceSubTypeCodes.XCL, () => ComplianceSubTypeDescriptions.XCL, () => ComplianceSubTypeLocalDescriptions.XCL, () => ComplianceSubTypeInternalImplemenationNote.XCL);
		}

		#endregion
	}
}
