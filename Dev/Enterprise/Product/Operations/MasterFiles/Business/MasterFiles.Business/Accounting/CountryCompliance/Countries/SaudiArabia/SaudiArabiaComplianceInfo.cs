using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting.EInvoicing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class SaudiArabiaComplianceInfo : CountryComplianceInfo,
		ITaxMessagesGroupProvider,
		IComplianceInfoElectronicInvoicing
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.SaudiArabia;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.VATCode;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.VATCode;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region IComplianceInfoElectronicInvoicing

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDate() => Env.Instance.IsProductionSystem ? new ZDate(2023, 01, 01) : new ZDate(2022, 11, 01);

		ZDate IComplianceInfoElectronicInvoicing.GetEInvoicingComplianceDateForPayables() => ZDate.Empty;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatus() => Constants.EInvoicingPivotState.Queued;

		string IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingSubmitPivotStatusForPayables(GlbCompany currenCompany) => string.Empty;

		MultilingualString IComplianceInfoElectronicInvoicing.GetDefaultEInvoicingPivotPendingStatusDescription() => IEInvoicingTransactionConstants.DefaultEInvoicingPivotPendingStatusDescription;

		string IComplianceInfoElectronicInvoicing.GetGovernmentAllocatedNumberColumnName() => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		string IComplianceInfoElectronicInvoicing.GetAccTransactionHeaderAuthorisationRecordType() => AccTransactionHeaderAuthorisationRecordTypes.SaudiArabia;

		#endregion

		#region ITaxMessagesGroupProvider

		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N29, Description = TaxMessageGroupDescriptions.N29, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N29 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N297, Description = TaxMessageGroupDescriptions.N297, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N297 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N30, Description = TaxMessageGroupDescriptions.N30, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N30 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N32, Description = TaxMessageGroupDescriptions.N32, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N32 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N33, Description = TaxMessageGroupDescriptions.N33, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N33 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N341, Description = TaxMessageGroupDescriptions.N341, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N341 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N342, Description = TaxMessageGroupDescriptions.N342, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N342 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N343, Description = TaxMessageGroupDescriptions.N343, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N343 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N344, Description = TaxMessageGroupDescriptions.N344, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N344 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N345, Description = TaxMessageGroupDescriptions.N345, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N345 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N35, Description = TaxMessageGroupDescriptions.N35, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N35 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.N36, Description = TaxMessageGroupDescriptions.N36, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.N36 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.EDU, Description = TaxMessageGroupDescriptions.EDU, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.EDU },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.HEA, Description = TaxMessageGroupDescriptions.HEA, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.HEA },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.NON, Description = TaxMessageGroupDescriptions.NON, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.NON },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.OOS, Description = TaxMessageGroupDescriptions.OOS, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.OOS },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.MIL, Description = TaxMessageGroupDescriptions.MIL, Bool = true, RelatedItemCode = GovernmentTaxGroupCodes.MLTRY }
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string N29 = "29";
			public const string N297 = "297";
			public const string N30 = "30";
			public const string N32 = "32";
			public const string N33 = "33";
			public const string N341 = "341";
			public const string N342 = "342";
			public const string N343 = "343";
			public const string N344 = "344";
			public const string N345 = "345";
			public const string N35 = "35";
			public const string N36 = "36";
			public const string EDU = "EDU";
			public const string HEA = "HEA";
			public const string NON = "NON";
			public const string OOS = "OOS";
			public const string MIL = "MIL";
		}

		public static class GovernmentTaxGroupCodes
		{
			public const string N29 = "29";
			public const string N297 = "29-7";
			public const string N30 = "30";
			public const string N32 = "32";
			public const string N33 = "33";
			public const string N341 = "34-1";
			public const string N342 = "34-2";
			public const string N343 = "34-3";
			public const string N344 = "34-4";
			public const string N345 = "34-5";
			public const string N35 = "35";
			public const string N36 = "36";
			public const string EDU = "EDU";
			public const string HEA = "HEA";
			public const string NON = "";
			public const string OOS = "OOS";
			public const string MLTRY = "MLTRY";
		}

		#region SuppressResourceStringsCheckRegion

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString N29 => (NoResString)"Financial services mentioned in Article 29 of the VAT Regulations";
			public static MultilingualString N297 => (NoResString)"Life insurance services mentioned in Article 29 of the VAT Regulations";
			public static MultilingualString N30 => (NoResString)"Real estate transactions mentioned in Article 30 of the VAT Regulations";
			public static MultilingualString N32 => (NoResString)"Export of goods";
			public static MultilingualString N33 => (NoResString)"Export of services";
			public static MultilingualString N341 => (NoResString)"The international transport of goods";
			public static MultilingualString N342 => (NoResString)"International transport of passengers";
			public static MultilingualString N343 => (NoResString)"Services directly connected and incidental to a Supply of international passenger transport";
			public static MultilingualString N344 => (NoResString)"Supply of a qualifying means of transport";
			public static MultilingualString N345 => (NoResString)"Any services relating to Goods or passenger transportation, as defined in article twenty five of these Regulations";
			public static MultilingualString N35 => (NoResString)"Medicines and medical equipment";
			public static MultilingualString N36 => (NoResString)"Qualifying metals";
			public static MultilingualString EDU => (NoResString)"Private education to citizen";
			public static MultilingualString HEA => (NoResString)"Private healthcare to citizen";
			public static MultilingualString NON => (NoResString)"No exemption reason";
			public static MultilingualString OOS => (NoResString)"Not subject to VAT";
			public static MultilingualString MIL => (NoResString)"Supply of qualified military goods";
		}

		#endregion

		#endregion
	}
}
