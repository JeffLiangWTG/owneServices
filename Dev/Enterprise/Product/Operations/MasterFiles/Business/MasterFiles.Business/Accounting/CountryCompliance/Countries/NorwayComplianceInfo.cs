using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class NorwayComplianceInfo : CountryComplianceInfo, ITaxMessagesGroupProvider
	{
		#region CountryComplianceInfo

		public override ZString CountryCode => Core.Constants.CountryCodes.Norway;
		protected override string GetBusinessRegistrationCode() => OrgCusCode.CodeTypes.GovBusinessCode;
		protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.NorwayCodeTypes.MVA;
		protected override string GetConsumptionTaxCode() => OrgCusCode.NorwayCodeTypes.MVA;
		protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.NorwayCodeTypes.MVA;
		protected override bool? GetIsReciprocal() => true;

		#endregion

		#region ITaxMessagesGroupProvider
		CodeDescriptionBoolRelatedItemCollection ITaxMessagesGroupProvider.GetTaxMessageGroup()
		{
			return new CodeDescriptionBoolRelatedItemCollection
			{
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code0, Description = TaxMessageGroupDescriptions.Code0, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code0 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code1, Description = TaxMessageGroupDescriptions.Code1, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code1 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code11, Description = TaxMessageGroupDescriptions.Code11, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code11 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code12, Description = TaxMessageGroupDescriptions.Code12, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code12 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code13, Description = TaxMessageGroupDescriptions.Code13, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code13 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code14, Description = TaxMessageGroupDescriptions.Code14, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code14 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code15, Description = TaxMessageGroupDescriptions.Code15, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code15 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code20, Description = TaxMessageGroupDescriptions.Code20, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code20 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code21, Description = TaxMessageGroupDescriptions.Code21, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code21 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code22, Description = TaxMessageGroupDescriptions.Code22, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code22 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code3, Description = TaxMessageGroupDescriptions.Code3, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code3 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code31, Description = TaxMessageGroupDescriptions.Code31, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code31 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code32, Description = TaxMessageGroupDescriptions.Code32, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code32 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code33, Description = TaxMessageGroupDescriptions.Code33, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code33 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code5, Description = TaxMessageGroupDescriptions.Code5, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code5 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code51, Description = TaxMessageGroupDescriptions.Code51, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code51 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code52, Description = TaxMessageGroupDescriptions.Code52, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code52 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code6, Description = TaxMessageGroupDescriptions.Code6, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code6 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code7, Description = TaxMessageGroupDescriptions.Code7, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code7 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code81, Description = TaxMessageGroupDescriptions.Code81, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code81 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code82, Description = TaxMessageGroupDescriptions.Code82, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code82 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code83, Description = TaxMessageGroupDescriptions.Code83, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code83 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code84, Description = TaxMessageGroupDescriptions.Code84, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code84 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code85, Description = TaxMessageGroupDescriptions.Code85, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code85 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code86, Description = TaxMessageGroupDescriptions.Code86, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code86 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code87, Description = TaxMessageGroupDescriptions.Code87, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code87 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code88, Description = TaxMessageGroupDescriptions.Code88, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code88 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code89, Description = TaxMessageGroupDescriptions.Code89, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code89 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code91, Description = TaxMessageGroupDescriptions.Code91, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code91 },
				new CodeDescriptionBoolRelatedItem() { Code = TaxMessageGroupCodes.Code92, Description = TaxMessageGroupDescriptions.Code92, Bool = true, RelatedItemCode = TaxMessageGroupCodes.Code92 },
			};
		}

		public static class TaxMessageGroupCodes
		{
			public const string Code0 = "0";
			public const string Code1 = "1";
			public const string Code11 = "11";
			public const string Code12 = "12";
			public const string Code13 = "13";
			public const string Code14 = "14";
			public const string Code15 = "15";
			public const string Code20 = "20";
			public const string Code21 = "21";
			public const string Code22 = "22";
			public const string Code3 = "3";
			public const string Code31 = "31";
			public const string Code32 = "32";
			public const string Code33 = "33";
			public const string Code5 = "5";
			public const string Code51 = "51";
			public const string Code52 = "52";
			public const string Code6 = "6";
			public const string Code7 = "7";
			public const string Code81 = "81";
			public const string Code82 = "82";
			public const string Code83 = "83";
			public const string Code84 = "84";
			public const string Code85 = "85";
			public const string Code86 = "86";
			public const string Code87 = "87";
			public const string Code88 = "88";
			public const string Code89 = "89";
			public const string Code91 = "91";
			public const string Code92 = "92";
		}

		#region SuppressResourceStringsCheckRegion

		static class TaxMessageGroupDescriptions
		{
			public static MultilingualString Code0 => (NoResString)"Ingen merverdiavgiftsbehandling";
			public static MultilingualString Code1 => (NoResString)"Fradrag for inngående mva, høy sats";
			public static MultilingualString Code11 => (NoResString)"Fradrag for inngående mva, middels sats";
			public static MultilingualString Code12 => (NoResString)"Fradrag for inngående mva";
			public static MultilingualString Code13 => (NoResString)"Fradrag for inngående mva, lav sats";
			public static MultilingualString Code14 => (NoResString)"Fradragsberettiget innførselsmerverdiavgift";
			public static MultilingualString Code15 => (NoResString)"Fradragsberettiget innførselsmerverdiavgift";
			public static MultilingualString Code20 => (NoResString)"Kostnad ved innførsel av varer";
			public static MultilingualString Code21 => (NoResString)"Kostnad ved innførsel av varer";
			public static MultilingualString Code22 => (NoResString)"Kostnad ved innførsel av varer";
			public static MultilingualString Code3 => (NoResString)"Utgående mva, høy sats";
			public static MultilingualString Code31 => (NoResString)"Utgående mva, middels sats";
			public static MultilingualString Code32 => (NoResString)"Utgående mva";
			public static MultilingualString Code33 => (NoResString)"Utgående mva, lav sats";
			public static MultilingualString Code5 => (NoResString)"Mvafritt salg";
			public static MultilingualString Code51 => (NoResString)"Innenlandsk omsetning med omvendt avgiftplikt";
			public static MultilingualString Code52 => (NoResString)"Utførsel av varer og tjenester";
			public static MultilingualString Code6 => (NoResString)"Omsetning utenfor merverdiavgiftsloven";
			public static MultilingualString Code7 => (NoResString)"Ingen merverdiavgiftsbehandling (inntekter)";
			public static MultilingualString Code81 => (NoResString)"Grunnlag innførsel av varer med fradragsrett for innførselsmerverdiavgift";
			public static MultilingualString Code82 => (NoResString)"Grunnlag innførsel av varer uten fradragsrett for innførselsmerverdiavgift";
			public static MultilingualString Code83 => (NoResString)"Grunnlag innførsel av varer med fradragsrett for innførselsmerverdiavgift";
			public static MultilingualString Code84 => (NoResString)"Grunnlag innførsel av varer uten fradragsrett for innførselsmerverdiavgift";
			public static MultilingualString Code85 => (NoResString)"Grunnlag innførsel av varer som det ikke skal beregnes merverdiavgift av";
			public static MultilingualString Code86 => (NoResString)"Tjenester kjøpt fra utlandet med fradrag for mva, høy sats";
			public static MultilingualString Code87 => (NoResString)"Kjøp av klimakvoter eller gull uten fradragsrett for merverdiavgift";
			public static MultilingualString Code88 => (NoResString)"Kjøp av klimakvoter eller gull med fradragsrett for merverdiavgift";
			public static MultilingualString Code89 => (NoResString)"Kjøp av klimakvoter eller gull uten fradragsrett for merverdiavgift";
			public static MultilingualString Code91 => (NoResString)"Kjøp av klimakvoter eller gull med kompensasjon for merverdiavgift";
			public static MultilingualString Code92 => (NoResString)"Kjøp av klimakvoter eller gull uten fradragsrett for merverdiavgift";
		}

		#endregion

		#endregion

	}
}
