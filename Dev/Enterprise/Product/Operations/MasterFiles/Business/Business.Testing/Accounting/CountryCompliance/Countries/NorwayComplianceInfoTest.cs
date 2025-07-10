using System.Collections.Generic;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(NorwayComplianceInfo))]
	sealed class NorwayComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Norway;

		protected override string ExpectedBusinessRegistrationCode => "GBR";

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "0", Description = (NoResString)"Ingen merverdiavgiftsbehandling", Bool = true, RelatedItemCode = "0" },
				new CodeDescriptionBoolRelatedItem() { Code = "1", Description = (NoResString)"Fradrag for inngående mva, høy sats", Bool = true, RelatedItemCode = "1" },
				new CodeDescriptionBoolRelatedItem() { Code = "11", Description = (NoResString)"Fradrag for inngående mva, middels sats", Bool = true, RelatedItemCode = "11" },
				new CodeDescriptionBoolRelatedItem() { Code = "12", Description = (NoResString)"Fradrag for inngående mva", Bool = true, RelatedItemCode = "12" },
				new CodeDescriptionBoolRelatedItem() { Code = "13", Description = (NoResString)"Fradrag for inngående mva, lav sats", Bool = true, RelatedItemCode = "13" },
				new CodeDescriptionBoolRelatedItem() { Code = "14", Description = (NoResString)"Fradragsberettiget innførselsmerverdiavgift", Bool = true, RelatedItemCode = "14" },
				new CodeDescriptionBoolRelatedItem() { Code = "15", Description = (NoResString)"Fradragsberettiget innførselsmerverdiavgift", Bool = true, RelatedItemCode = "15" },
				new CodeDescriptionBoolRelatedItem() { Code = "20", Description = (NoResString)"Kostnad ved innførsel av varer", Bool = true, RelatedItemCode = "20" },
				new CodeDescriptionBoolRelatedItem() { Code = "21", Description = (NoResString)"Kostnad ved innførsel av varer", Bool = true, RelatedItemCode = "21" },
				new CodeDescriptionBoolRelatedItem() { Code = "22", Description = (NoResString)"Kostnad ved innførsel av varer", Bool = true, RelatedItemCode = "22" },
				new CodeDescriptionBoolRelatedItem() { Code = "3", Description = (NoResString)"Utgående mva, høy sats", Bool = true, RelatedItemCode = "3" },
				new CodeDescriptionBoolRelatedItem() { Code = "31", Description = (NoResString)"Utgående mva, middels sats", Bool = true, RelatedItemCode = "31" },
				new CodeDescriptionBoolRelatedItem() { Code = "32", Description = (NoResString)"Utgående mva", Bool = true, RelatedItemCode = "32" },
				new CodeDescriptionBoolRelatedItem() { Code = "33", Description = (NoResString)"Utgående mva, lav sats", Bool = true, RelatedItemCode = "33" },
				new CodeDescriptionBoolRelatedItem() { Code = "5", Description = (NoResString)"Mvafritt salg", Bool = true, RelatedItemCode = "5" },
				new CodeDescriptionBoolRelatedItem() { Code = "51", Description = (NoResString)"Innenlandsk omsetning med omvendt avgiftplikt", Bool = true, RelatedItemCode = "51" },
				new CodeDescriptionBoolRelatedItem() { Code = "52", Description = (NoResString)"Utførsel av varer og tjenester", Bool = true, RelatedItemCode = "52" },
				new CodeDescriptionBoolRelatedItem() { Code = "6", Description = (NoResString)"Omsetning utenfor merverdiavgiftsloven", Bool = true, RelatedItemCode = "6" },
				new CodeDescriptionBoolRelatedItem() { Code = "7 ", Description = (NoResString)"Ingen merverdiavgiftsbehandling (inntekter)", Bool = true, RelatedItemCode = "7" },
				new CodeDescriptionBoolRelatedItem() { Code = "81", Description = (NoResString)"Grunnlag innførsel av varer med fradragsrett for innførselsmerverdiavgift", Bool = true, RelatedItemCode = "81" },
				new CodeDescriptionBoolRelatedItem() { Code = "82", Description = (NoResString)"Grunnlag innførsel av varer uten fradragsrett for innførselsmerverdiavgift", Bool = true, RelatedItemCode = "82" },
				new CodeDescriptionBoolRelatedItem() { Code = "83", Description = (NoResString)"Grunnlag innførsel av varer med fradragsrett for innførselsmerverdiavgift", Bool = true, RelatedItemCode = "83" },
				new CodeDescriptionBoolRelatedItem() { Code = "84", Description = (NoResString)"Grunnlag innførsel av varer uten fradragsrett for innførselsmerverdiavgift", Bool = true, RelatedItemCode = "84" },
				new CodeDescriptionBoolRelatedItem() { Code = "85", Description = (NoResString)"Grunnlag innførsel av varer som det ikke skal beregnes merverdiavgift av", Bool = true, RelatedItemCode = "85" },
				new CodeDescriptionBoolRelatedItem() { Code = "86", Description = (NoResString)"Tjenester kjøpt fra utlandet med fradrag for mva, høy sats", Bool = true, RelatedItemCode = "86" },
				new CodeDescriptionBoolRelatedItem() { Code = "87", Description = (NoResString)"Kjøp av klimakvoter eller gull uten fradragsrett for merverdiavgift", Bool = true, RelatedItemCode = "87" },
				new CodeDescriptionBoolRelatedItem() { Code = "88", Description = (NoResString)"Kjøp av klimakvoter eller gull med fradragsrett for merverdiavgift", Bool = true, RelatedItemCode = "88" },
				new CodeDescriptionBoolRelatedItem() { Code = "89", Description = (NoResString)"Kjøp av klimakvoter eller gull uten fradragsrett for merverdiavgift", Bool = true, RelatedItemCode = "89" },
				new CodeDescriptionBoolRelatedItem() { Code = "91", Description = (NoResString)"Kjøp av klimakvoter eller gull med kompensasjon for merverdiavgift", Bool = true, RelatedItemCode = "91" },
				new CodeDescriptionBoolRelatedItem() { Code = "92", Description = (NoResString)"Kjøp av klimakvoter eller gull uten fradragsrett for merverdiavgift", Bool = true, RelatedItemCode = "92" }
			};
	}
}
