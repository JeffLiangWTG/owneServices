using System.Globalization;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class FijiComplianceInfo : CountryComplianceInfo,
		IComplianceInfoElectronicInvoicing,
		ITaxMessagesGroupProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Fiji;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => false;

		#endregion

		protected override string GetComplianceVersionNo()
		{
			var currentVersionNumber = new VersionNumber(2021, 12, 1, 0);
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", currentVersionNumber.Major, currentVersionNumber.Minor, currentVersionNumber.Release.ToString("D2"));
		}

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => new ZDate(2022, 4, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Fiji;

		#endregion

		#region ITaxMessagesGroupProvider
		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.A, Description = TaxMessageGroupDescriptions.A, Bool = true, RelatedItemCode = TaxMessageGroupCodes.A },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.B, Description = TaxMessageGroupDescriptions.B, Bool = true, RelatedItemCode = TaxMessageGroupCodes.B },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.C, Description = TaxMessageGroupDescriptions.C, Bool = true, RelatedItemCode = TaxMessageGroupCodes.C },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.D, Description = TaxMessageGroupDescriptions.D, Bool = true, RelatedItemCode = TaxMessageGroupCodes.D },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E, Description = TaxMessageGroupDescriptions.E, Bool = false, RelatedItemCode = TaxMessageGroupCodes.E },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.F, Description = TaxMessageGroupDescriptions.F, Bool = false, RelatedItemCode = TaxMessageGroupCodes.F },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N, Description = TaxMessageGroupDescriptions.N, Bool = true, RelatedItemCode = TaxMessageGroupCodes.N },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.P, Description = TaxMessageGroupDescriptions.P, Bool = false, RelatedItemCode = TaxMessageGroupCodes.P },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.NOT_REQ, Description = TaxMessageGroupDescriptions.NOT_REQ, Bool = true, RelatedItemCode = TaxMessageGroupCodes.NOT_REQ }
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string A = "A";
			public const string B = "B";
			public const string C = "C";
			public const string D = "D";
			public const string E = "E";
			public const string F = "F";
			public const string N = "N";
			public const string P = "P";
			public const string NOT_REQ = "0";
		}

		#region SuppressResourceStringsCheckRegion

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString A => (NoResString)"VAT";
			public static MultilingualString B => (NoResString)"VAT-EXPORT";
			public static MultilingualString C => (NoResString)"VAT-EXCL";
			public static MultilingualString D => (NoResString)"VAT-HIGH RATE";
			public static MultilingualString E => (NoResString)"STT";
			public static MultilingualString F => (NoResString)"ECAL";
			public static MultilingualString N => (NoResString)"N-TAX";
			public static MultilingualString P => (NoResString)"PBL";
			public static MultilingualString NOT_REQ => (NoResString)"NOT REQ FOR FISCALIZATION";
		}

		#endregion

		#endregion
	}
}
