using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class EgyptComplianceInfo : CountryComplianceInfo,
		IComplianceInfoElectronicInvoicing,
		ITaxMessagesGroupProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Egypt;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => EnvProxy.Instance.IsProductionSystem ? new ZDate(2021, 5, 15) : new ZDate(2021, 4, 1);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Pending;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => ResString.GetMultilingualString("f1f8bce2-aff5-46e2-90aa-7efafe816978", "Pending Digital Signature");

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.Egypt;

		#endregion

		#region ITaxMessagesGroupProvider

		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E00, Description = TaxMessageGroupDescriptions.E00, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E00 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E01, Description = TaxMessageGroupDescriptions.E01, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E01 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E02, Description = TaxMessageGroupDescriptions.E02, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E02 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E03, Description = TaxMessageGroupDescriptions.E03, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E03 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E04, Description = TaxMessageGroupDescriptions.E04, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E04 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E05, Description = TaxMessageGroupDescriptions.E05, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E05 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E06, Description = TaxMessageGroupDescriptions.E06, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E06 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E07, Description = TaxMessageGroupDescriptions.E07, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E07 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E08, Description = TaxMessageGroupDescriptions.E08, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E08 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E09, Description = TaxMessageGroupDescriptions.E09, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E09 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.E10, Description = TaxMessageGroupDescriptions.E10, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.E10 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N01, Description = TaxMessageGroupDescriptions.N01, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N01 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N02, Description = TaxMessageGroupDescriptions.N02, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N02 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.T2, Description = TaxMessageGroupDescriptions.T2, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.T2 },
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string E00 = "E00";
			public const string E01 = "E01";
			public const string E02 = "E02";
			public const string E03 = "E03";
			public const string E04 = "E04";
			public const string E05 = "E05";
			public const string E06 = "E06";
			public const string E07 = "E07";
			public const string E08 = "E08";
			public const string E09 = "E09";
			public const string E10 = "E10";
			public const string N01 = "N01";
			public const string N02 = "N02";
			public const string T2 = "T2";
		}

		public static class GovernmentTaxGroupCodes
		{
			public const string E00 = "";
			public const string E01 = "V001";
			public const string E02 = "V002";
			public const string E03 = "V003";
			public const string E04 = "V004";
			public const string E05 = "V005";
			public const string E06 = "V006";
			public const string E07 = "V007";
			public const string E08 = "V008";
			public const string E09 = "V009";
			public const string E10 = "V010";
			public const string N01 = "OF03";
			public const string N02 = "OF04";
			public const string T2 = "Tbl01";
		}

		#region SuppressResourceStringsCheckRegion

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString E00 => (NoResString)"Not applicable / غير قابل للتطبيق";
			public static MultilingualString E01 => (NoResString)"Export / تصدير للخارج";
			public static MultilingualString E02 => (NoResString)"Export to free areas and other areas / تصدير مناطق حرة وأخرى";
			public static MultilingualString E03 => (NoResString)"Exempted good or service / سلعة أو خدمة معفاة";
			public static MultilingualString E04 => (NoResString)"A non-taxable good or service / سلعة أو خدمة غير خاضعة للضريبة";
			public static MultilingualString E05 => (NoResString)"Exemptions for diplomats, consulates and embassies / إعفاءات دبلوماسين والقنصليات والسفارات";
			public static MultilingualString E06 => (NoResString)"Defence and National Security exemptions / إعفاءات الدفاع والأمن القومى";
			public static MultilingualString E07 => (NoResString)"Agreements exemptions / إعفاءات اتفاقيات";
			public static MultilingualString E08 => (NoResString)"Special exemptions and other reasons / إعفاءات خاصة و أخرى";
			public static MultilingualString E09 => (NoResString)"General item sales / سلع عامة";
			public static MultilingualString E10 => (NoResString)"Other rates / نسب ضريبة أخرى";
			public static MultilingualString N01 => (NoResString)"Other fees (rate) / رسوم أخرى (نسبة)";
			public static MultilingualString N02 => (NoResString)"Other fees (amount) / رسوم أخرى (قطعية)";
			public static MultilingualString T2 => (NoResString)"Table tax (percentage) / ضريبه الجدول (نسبيه)";
		}

		#endregion

		#endregion
	}
}
