using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff
{
	public sealed class MeasureMappingProvider : IMeasureMappingProvider
	{
		IEnumerable<string> IMeasureMappingProvider.ConvertPreferences(IEnumerable<string> defaultPreferences, string geographicalArea, IEnumerable<string> footnotes)
		{
			var preferences = defaultPreferences.ToList();
			var key = geographicalArea ?? string.Empty;

			if (preferences.Any(x => x != null && x.StartsWith("2", StringComparison.Ordinal)) && preferences.Any(x => x != null && x.StartsWith("3", StringComparison.Ordinal)))
			{
				preferences = DCTSTradeGroups.Contains(key) ?
								preferences.Where(x => !x.StartsWith("3", StringComparison.Ordinal)).ToList() :
								preferences.Where(x => !x.StartsWith("2", StringComparison.Ordinal)).ToList();
			}

			if (geographicalArea == ERGA_OMNES && preferences.Contains(NormalThirdCountryPreference) && footnotes.Contains(CNSubHeadingFootnote))
			{
				preferences.Add(CDSubHeadingPreference);
			}

			return preferences;
		}

		string IMeasureMappingProvider.ConvertRateCode(string defaultRateCode, Measure measure)
		{
			var rateCode = defaultRateCode;

			if (string.IsNullOrEmpty(rateCode))
			{
				switch (measure.MeasureType)
				{
					case "306":
						rateCode = GetExciseRateCode(measure);
						break;
					default:
						rateCode = MeasureHelper.GetDutyRateCode(measure);
						break;
				}
			}

			return rateCode;
		}
		
		string IMeasureMappingProvider.GetVatCode(Measure measure)
		{
			var result = string.Empty;
			var dutyAmount = measure.Components?.FirstOrDefault()?.DutyAmount;

			if (measure.ConditionClass == MeasureHelper.ConditionClass.Vat && dutyAmount.HasValue)
			{
				result = GetVatCodeFromDutyAmount(dutyAmount.Value);
			}

			return result;
		}

		Dictionary<string, MeasureTypeMapping> IMeasureMappingProvider.GetMeasureTypeMappings() => new Dictionary<string, MeasureTypeMapping>
		{
			{ "103", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "100" }, AuthorisedUsePreferences = new[] { "140" } } }, // C: Third country duty
			{ "105", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "140" } } }, // C: Non preferential duty under end-use
			{ "106", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "400" } } }, // C: Customs Union Duty
			{ "109", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, SupplementaryUnit = true } }, // O: Supplementary unit
			{ "110", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, SupplementaryUnit = true } }, // O: Supplementary unit import
			{ "112", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "110" }, AuthorisedUsePreferences = new[] { "115" } } }, // C: Autonomous tariff suspension
			{ "115", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "115" } } }, // C: Autonomous suspension under end-use
			{ "117", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "140" } } }, // C: Suspension - goods for certain categories of ships, boats and other vessels and for drilling or production platforms
			{ "119", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "119" } } }, // C: Airworthiness tariff suspension
			{ "122", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "120", "125", "128" }, AuthorisedUsePreferences = new[] { "123" } } }, // C: Non preferential tariff quota
			{ "123", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "123" } } }, // C: Non preferential tariff quota under end-use
			{ "140", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Skip = true } }, // C: Outward processing tariff preference
			{ "141", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "310" }, AuthorisedUsePreferences = new[] { "315" } } }, // C: Preferential suspension
			{ "142", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "200", "300" }, AuthorisedUsePreferences = new[] { "240", "340" } } }, // C: Tariff preference
			{ "143", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "220", "225", "320", "325" }, AuthorisedUsePreferences = new[] { "223", "323" } } }, // C: Preferential tariff quota
			{ "144", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "200", "300" } } }, // C: Preferential ceiling
			{ "145", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "240", "340" } } }, // C: Preference under end-use
			{ "146", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "223", "323" } } }, // C: Preferential tariff quota under end-use
			{ "147", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "420" } } }, // C: Customs Union Quota
			{ "277", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // A: Import prohibition
			{ "278", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // A: Export prohibition
			{ "305", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Vat, RateType = string.Empty } }, // P: Value added tax
			{ "306", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises } }, // Q: Excises
			{ "350", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Animal Health Certificate
			{ "351", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Health and Safety Executive Import Licensing Firearms and Ammunition
			{ "352", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Attestation Document (horticulture and potatoes)
			{ "353", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: DCMS Open General Export Licence
			{ "354", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Home Office Controlled Drugs (export)
			{ "355", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: HMI Conformity Certificate (fruit and veg) issued in UK
			{ "356", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Common Veterinary Entry Document (CVED)
			{ "357", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Certificate of Conformity
			{ "358", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Home Office pre-cursor chemical authorisation
			{ "359", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Health and Safety Executive (imports)
			{ "360", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Phytosanitary Certificate (import)
			{ "361", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Home Office Pre-cursor chemicals
			{ "362", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Home Office Controlled Drugs (import)
			{ "363", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Quarantine Release Certificate
			{ "410", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Veterinary control
			{ "420", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Entry into free circulation (prior surveillance)
			{ "430", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Control of particulars of the declaration (suspicious value/net weight or value/supplementary unit)
			{ "431", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Control of particulars of the declaration (suspicious net weight/supplementary unit)
			{ "440", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Public Import Monitoring
			{ "442", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Confidential Import Monitoring
			{ "445", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Public Export Monitoring
			{ "447", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Confidential Export Monitoring
			{ "450", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Statistical surveillance - all imports, except Regulation 1555/96
			{ "455", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Confidential Surveillance (Tariff classification)
			{ "456", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Confidential Surveillance (Other)
			{ "457", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Confidential Surveillance (Licence TQs)
			{ "460", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Computer surveillance
			{ "461", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Community surveillance - reference quantities
			{ "462", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Posterior import surveillance
			{ "463", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Posterior export surveillance
			{ "464", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Declaration of subheading submitted to end-use provisions
			{ "465", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Restriction on entry into free circulation
			{ "467", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Restriction on export
			{ "468", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: GSP Confidential Surveillance
			{ "469", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Confidential surveillance other than GSP
			{ "470", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Export surveillance
			{ "471", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Export surveillance (TQS)
			{ "472", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // N: Export surveillance
			{ "473", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export authorization
			{ "474", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Entry into free circulation (quantitative limitation)
			{ "475", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Restriction on entry into free circulation
			{ "476", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Restriction on export
			{ "477", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Entry into free circulation (outward processing traffic)
			{ "478", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export authorization (Dual use)
			{ "479", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control on dangerous chemicals
			{ "481", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // A: Declaration of subheading submitted to restrictions (import)
			{ "482", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Class, RateType = string.Empty } }, // B: Declaration of subheading submitted to restrictions (net weight/supplementary unit)
			{ "483", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Class, RateType = string.Empty } }, // B: Declaration of subheading submitted to restrictions (value)
			{ "484", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Class, RateType = string.Empty } }, // R: Declaration of subheading submitted to physical restrictions (net weight/supplementary unit)
			{ "485", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // A: Declaration of subheading submitted to restrictions (export)
			{ "488", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Skip = true } }, // M: Unit price
			{ "489", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Skip = true } }, // M: Representative price
			{ "490", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Skip = true } }, // M: Standard import value
			{ "551", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A35" } }, // D: Provisional anti-dumping duty
			{ "552", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A30" } }, // D: Definitive anti-dumping duty
			{ "553", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Countervailing, RateCode = "A45" } }, // D: Provisional countervailing duty
			{ "554", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Countervailing, RateCode = "A40" } }, // D: Definitive countervailing duty
			{ "555", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A35" } }, // D: Anti-dumping/countervailing duty - Pending collection
			{ "561", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A35" } }, // D: Notice of initiation of an anti-dumping or countervailing proceeding
			{ "562", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A35" } }, // D: Suspended anti-dumping or countervailing duty
			{ "564", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A35" } }, // D: Anti-dumping or countervailing registration
			{ "565", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A35" } }, // D: Anti-dumping/countervailing review
			{ "566", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A35" } }, // D: Anti-dumping/countervailing statistic
			{ "570", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A35" } }, // D: Anti-dumping/countervailing duty - Control
			{ "651", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Security, RateCode = "SEC" } }, // S: Security based on representative price
			{ "652", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Security, RateCode = "SEC" } }, // S: Additional duty based on cif price
			{ "653", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Security, RateCode = "SEC" } }, // S: Security based on representative price, reduced under the benefit of a tariff quota
			{ "654", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Security, RateCode = "SEC" } }, // S: Additional duty based on CIF price, reduced under the benefit of a tariff quota
			{ "655", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Security, RateCode = "SEC" } }, // S: Security (poultry) based on representative price
			{ "656", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Security, RateCode = "SEC" } }, // S: Additional duty (poultry) based on cif price
			{ "657", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "240", "340" } } }, // S: Reduced security based on representative price
			{ "658", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Preferences = new List<string> { "240", "340" } } }, // S: Reduced additional duty based on CIF price
			{ "672", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, RateCode = "A20" } }, // F: Amount of additional duty on sugar
			{ "673", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, RateCode = "A20" } }, // F: Amount of additional duty on flour
			{ "674", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350" } }, // E: Agricultural component
			{ "680", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350" } }, // E: Export refund (basic products)
			{ "681", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350" } }, // E: Export refund (ingredients - information)
			{ "683", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350" } }, // E: Export refund (ingredients - amounts)
			{ "684", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350" } }, // E: Export refunds for cereal contents
			{ "685", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350" } }, // E: Export refunds for rice contents
			{ "686", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350" } }, // E: Export refund for eggs contents
			{ "687", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350" } }, // E: Export refunds for sugar contents
			{ "688", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350" } }, // E: Export refunds for milk products contents
			{ "690", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Countervailing, RateCode = "A45" } }, // J: Countervailing charge
			{ "695", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, RateCode = "A20" } }, // J: Additional duties
			{ "696", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, RateCode = "A20" } }, // J: Additional duties (safeguard)
			{ "705", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import prohibition on goods for torture and repression
			{ "706", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Goods for torture and repression, export prohibition
			{ "707", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control
			{ "708", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Goods for torture and repression, export restriction
			{ "709", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control
			{ "710", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control - CITES
			{ "711", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control on restricted goods and technologies
			{ "712", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control - IAS
			{ "713", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control on genetically modified organisms (GMO) and products containing GMOs
			{ "714", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control
			{ "715", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control - CITES
			{ "716", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control measure for fish
			{ "717", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control on restricted goods and technologies
			{ "718", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control on luxury goods
			{ "719", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Control on illegal, unreported and unregulated fishing
			{ "722", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Entry into free circulation (restriction - feed and food)
			{ "724", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control of fluorinated greenhouse gases
			{ "725", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control on ozone-depleting substances
			{ "728", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control on luxury goods
			{ "730", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Compliance with the pre-export checks requirements
			{ "735", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control on cultural goods
			{ "740", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control on cat and dog fur
			{ "745", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control on cat and dog fur
			{ "746", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control on seal products
			{ "747", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control of timber and timber products subject to the FLEGT licensing scheme
			{ "748", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control of mercury
			{ "749", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control of mercury
			{ "750", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control of organic products
			{ "751", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control - Waste
			{ "755", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control - waste
			{ "760", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control
			{ "761", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control on REACH
			{ "766", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Export control
			{ "770", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control of timber and timber products subject to the FLEGT licensing scheme-Ghana
			{ "771", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control of timber and timber products subject to the FLEGT licensing scheme-Cameroon
			{ "772", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control of timber and timber products subject to the FLEGT licensing scheme-Republic of Congo
			{ "773", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control of timber and timber products subject to the FLEGT licensing scheme-Central Africa Republic
			{ "774", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Import control of timber and timber products subject to the FLEGT licensing scheme-Vietnam
			{ "AHC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Animal Health Certificate
			{ "AIL", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Health and Safety Executive Import Licensing Firearms and Ammunition
			{ "ATT", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Attestation Document (horticulture and potatoes)
			{ "CEX", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: DCMS Open General Export Licence
			{ "CHM", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Controlled Chemical licence / authorisation
			{ "COE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Home Office Controlled Drugs (export)
			{ "COI", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: HMI Conformity Certificate (fruit and veg) issued in UK
			{ "CVD", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Common Veterinary Entry Document (CVED)
			{ "DAA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "411" } }, // Q: EXCISE - FULL, 411, SPARKLING WINE OF FRESH GRAPE, 8.5% AND ABOVE, BUT NOT EXCEEDING 15%
			{ "DAB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "412" } }, // Q: EXCISE - FULL, 412, SPARKLING WINE OF FRESH GRAPE, EXCEEDING 5.5% BUT LESS THAN 8.5%
			{ "DAC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "413" } }, // Q: EXCISE - FULL, 413, STILL WINE EXC 5.5% NOT EXC 15%
			{ "DAE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "415" } }, // Q: EXCISE - FULL, 415, STILL OR SPARKLING EXC 15% BUT NOT EXC 22%
			{ "DAI", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "419" } }, // Q: EXCISE - FULL, 419, WINE OF GREATER THAN 22% VOL
			{ "DBA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "421" } }, // Q: EXCISE - FULL, 421, SPARKLING MADE-WINE EXC 8.5% AND ABOVE BUT NOT EXCEEDING 15%
			{ "DBB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "422" } }, // Q: EXCISE - FULL, 422, SPARKLING MADE-WINE EXCEEDING 5.5% BUT LESS THAN 8.5%
			{ "DBC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "423" } }, // Q: EXCISE - FULL, 423, STILL MADE-WINE EXC 5.5% NOT EXC 15%
			{ "DBE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "425" } }, // Q: EXCISE - FULL, 425, MADE-WINE OF BETWEEN 15%-22% VOL
			{ "DBI", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "429" } }, // Q: EXCISE - FULL, 429, MADE-WINE OF GREATER THAN 22% VOL
			{ "DCA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "431" } }, // Q: EXCISE - FULL, 431, WINE BASED BEVERAGE OF LESS THAN 1.2% VOL
			{ "DCC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "433" } }, // Q: EXCISE - FULL, 433, WINE, MADE-WINE EXC 1.2% VOL NOT EXC 4% VOL.
			{ "DCE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "435" } }, // Q: EXCISE - FULL, 435, WINE, MADE-WINE EXC 4% VOL NOT EXC 5.5% VOL.
			{ "DCH", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "438" } }, // Q: EXCISE - FULL, 438, SPIRIT-BASED COOLERS
			{ "DDA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "441" } }, // Q: EXCISE - FULL, 441, IMPORTED BEER
			{ "DDB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "442" } }, // Q: EXCISE - FULL, 442, BEER MADE IN THE UK
			{ "DDC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "443" } }, // Q: EXCISE - FULL, 443, IMPORTED BEER
			{ "DDD", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "444" } }, // Q: EXCISE - FULL, 444, UK BEER
			{ "DDE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "445" } }, // Q: EXCISE - FULL, 445, UK BEER
			{ "DDF", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "446" } }, // Q: EXCISE - FULL, 446, IMPORTED BEER
			{ "DDG", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "447" } }, // Q: EXCISE - FULL, 447, IMPORTED BEER
			{ "DDJ", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "440" } }, // Q: EXCISE - FULL, 440, BEER MADE IN THE UK
			{ "DEA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "451" } }, // Q: EXCISE - FULL, 451, SPIRITS
			{ "DFA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "461" } }, // Q: EXCISE - FULL, 461, WHISKY - WHOLLY MALT
			{ "DFB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "462" } }, // Q: EXCISE - FULL, 462, WHISKY - WHOLLY GRAIN
			{ "DFC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "463" } }, // Q: EXCISE - FULL, 463, WHISKY - BLENDED
			{ "DGC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "473" } }, // Q: EXCISE - FULL, 473, BEER BASED BEVERAGE EXCEEDING 1.2% VOL.
			{ "DHA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "481" } }, // Q: EXCISE - FULL, 481, CIDER AND PERRY
			{ "DHC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "483" } }, // Q: EXCISE - FULL, 483, CIDER AND PERRY EXCEEDING 7.5% BUT LESS THAN 8.5%
			{ "DHE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "485" } }, // Q: EXCISE - FULL, 485, SPARKLING CIDER and PERRY, STRENGTH EXCEEDING 5.5% BUT LESS THAN 8.5%
			{ "DPO", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Documentary Proof of Origin
			{ "EAA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "511" } }, // Q: EXCISE - FULL, 511, UNREBATED LIGHT OIL, AVIATION GASOLINE
			{ "EAE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "515" } }, // Q: EXCISE - FULL, 515, UNREBATED LIGHT OIL, LEADED MOTOR SPIRIT
			{ "EBA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "521" } }, // Q: EXCISE - FULL, 521, REBATED LIGHT OIL, FURNACE FUEL
			{ "EBB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "522" } }, // Q: EXCISE - FULL, 522, REBATED LIGHT OIL, UNLEADED FUEL
			{ "EBE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "525" } }, // Q: EXCISE - FULL, 525, ULTRA LOW SULPHUR PETROL
			{ "EBJ", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "520" } }, // Q: EXCISE - FULL, 520, UNREBATED LIGHT OIL, OTHER
			{ "ECM", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Controlled Chemical licence / authorisation
			{ "EDA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "541" } }, // Q: EXCISE - FULL, 541, UNREBATED HEAVY OIL
			{ "EDB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "542" } }, // Q: EXCISE - FULL, 542, KEROSENE AS OFF-ROAD MOTOR VEHICLE FUEL
			{ "EDE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "545" } }, // Q: EXCISE - FULL, 545, ULTRA LOW SULPHER DIESEL
			{ "EDJ", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "540" } }, // Q: EXCISE – FULL, 540, OTHER (UNMARKED) HEAVY OIL (OTHER THAN KEROSENE)
			{ "EEA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "551" } }, // Q: EXCISE - FULL, 551, REBATED HEAVY OIL, KEROSENE
			{ "EEF", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "556" } }, // Q: EXCISE - FULL, 556, REBATED HEAVY OIL, GAS OIL
			{ "EFA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "561" } }, // Q: EXCISE - FULL, 561, REBATED HEAVY OIL, FUEL OIL
			{ "EFJ", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "560" } }, // Q: EXCISE - FULL, 560, ULTRA LOW SULPHUR GAS OIL
			{ "EGA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "571" } }, // Q: EXCISE - FULL, 571, BIODIESEL FOR NON-ROAD USE
			{ "EGB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "572" } }, // Q: EXCISE - FULL, 572, BIODIESEL BLENDED WITH KEROSENE
			{ "EGJ", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "570" } }, // Q: EXCISE - FULL, 570, REBATED HEAVY OIL, OTHER
			{ "EHC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: DEFRA - Health Certificate  (e.g. Live Animals)
			{ "EHI", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "589" } }, // Q: EXCISE - FULL, 589, BIODIESEL - PURE BIODIESEL
			{ "EIA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "591" } }, // Q: EXCISE - FULL, 591, NATURAL GAS
			{ "EIB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "592" } }, // Q: EXCISE - FULL, 592, OTHER GAS
			{ "EIC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "593" } }, // Q: EXCISE - FULL, 593, OTHER RCT EX DTY SULPH FREE DIESEL AND RPT EX DTY SULPH FREE DIESEL
			{ "EID", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "594" } }, // Q: EXCISE - FULL, 594, OTHER RCT EX DTY SULPH FREE PETROL AND RPT EX DTY SULPH FREE PETROL
			{ "EIE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "595" } }, // Q: EXCISE - FULL, 595, BIOETHANOL
			{ "EIJ", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "590" } }, // Q: EXCISE - FULL, 590, BIODIESEL - BLENDED
			{ "EQC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Certificate of Conformity
			{ "EWP", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: DEFRA Waste Shipment Permit
			{ "EXA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Levies, RateCode = "990" } }, // Q: CLIMATE CHANGE LEVY (CCL), 990, solid
			{ "EXB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Levies, RateCode = "990" } }, // Q: CLIMATE CHANGE LEVY (CCL), 990, oil
			{ "EXC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Levies, RateCode = "990" } }, // Q: CLIMATE CHANGE LEVY (CCL), 990, gas
			{ "EXD", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Levies, RateCode = "990" } }, // Q: CLIMATE CHANGE LEVY (CCL), 990, electric
			{ "FAA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "611" } }, // Q: EXCISE - FULL, 611, CIGARATTES
			{ "FAE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "615" } }, // Q: EXCISE - FULL, 615, CIGARS
			{ "FAI", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "619" } }, // Q: EXCISE - FULL, 619, HAND ROLLING TOBACCO
			{ "FBC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "623" } }, // Q: EXCISE - FULL, 623, OTHER SMOKING TOBACCO
			{ "FBG", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "627" } }, // Q: EXCISE - FULL, 627, CHEWING TOBACCO
			{ "HOP", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Home Office pre-cursor chemical authorisation
			{ "HSE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Health and Safety Executive (imports)
			{ "IWP", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: EA Import waste permit
			{ "LAA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "511" } }, // Q: EXCISE - SUSPENSION, 511, UNREBATED LIGHT OIL, AVIATION GASOLINE
			{ "LAE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "515" } }, // Q: EXCISE - SUSPENSION, 515, UNREBATED LIGHT OIL, LEADED MOTOR SPIRIT
			{ "LBA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "521" } }, // Q: EXCISE - SUSPENSION, 521, REBATED LIGHT OIL, FURNACE FUEL
			{ "LBB", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "522" } }, // Q: EXCISE - SUSPENSION, 522, REBATED LIGHT OIL, UNLEADED FUEL
			{ "LBE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "525" } }, // Q: EXCISE - SUSPENSION, 525, ULTRA LOW SULPHUR PETROL
			{ "LBJ", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "520" } }, // Q: EXCISE - SUSPENSION, 520, UNREBATED LIGHT OIL, OTHER
			{ "LDA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "541" } }, // Q: EXCISE - SUSPENSION, 541, UNREBATED HEAVY OIL
			{ "LEA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "551" } }, // Q: EXCISE - SUSPENSION, 551, REBATED HEAVY OIL, KEROSENE
			{ "LEF", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "556" } }, // Q: EXCISE - SUSPENSION, 556, REBATED HEAVY OIL, GAS OIL
			{ "LFA", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "561" } }, // Q: EXCISE - SUSPENSION, 561, REBATED HEAVY OIL, FUEL OIL
			{ "LGJ", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "570" } }, // Q: EXCISE - SUSPENSION, 570, REBATED HEAVY OIL, OTHER
			{ "PHC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Phytosanitary Certificate (import)
			{ "PRE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Home Office Pre-cursor chemicals
			{ "PRT", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Home Office Controlled Drugs (import)
			{ "QRC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Quarantine Release Certificate
			{ "SFS", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }, // B: Seal Fur
			{ "VTA", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // P: VAT reduced rate 5%
			{ "VTE", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // P: VAT exempt
			{ "VTS", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // P: VAT standard rate
			{ "VTZ", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty } }, // P: VAT zero rate
			// Expired - Skip = True
			{ "046", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // Z: Tariff quota/ceiling
			{ "072", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // B: Import licence
			{ "081", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // Z: MOB
			{ "082", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // Z: MOB+MCM
			{ "083", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // Z: MOB+MCM+ACA
			{ "084", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // Z: MOB+ACA
			{ "086", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // G: MCM + ACA
			{ "089", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // B: The Washington Convention
			{ "092", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // B: The Washington Convention (export)
			{ "102", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Skip = true } }, // C: Autonomous duty
			{ "104", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Skip = true } }, // C: Conventional duty
			{ "111", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, SupplementaryUnit = true, Skip = true } }, // O: Supplementary unit export
			{ "166", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // R: Provisional exclusion
			{ "466", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // N: Outward processing post. surveillance
			{ "487", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, Skip = true } }, // M: Representative price (poultry)
			{ "563", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.AntiDumping, RateCode = "A35", Skip = true } }, // D: Dump Undertakings
			{ "624", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // K: Reference price
			{ "625", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // K: Statistic - reference price fish
			{ "630", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // K: Differences in prices for basic products
			{ "670", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350", Skip = true } }, // E: Amount of levy
			{ "676", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // G: Monetary compen. amount - import
			{ "677", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // H: Accesion compensatory amount (ACA) import
			{ "678", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // G: Monetary compel. amount for export
			{ "679", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // H: Accession comp. amount (ACA) export
			{ "682", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Export, RateCode = "350", Skip = true } }, // E: Export tax
			{ "691", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Security, RateCode = "SEC", Skip = true } }, // S: Suplementary amount
			{ "692", new MeasureTypeMapping { ConditionClass = string.Empty, RateType = string.Empty, Skip = true } }, // L: Supplementary trade mechanism
		};

		static string GetExciseRateCode(Measure measure)
		{
			var rateCode = MeasureHelper.RateCodes.ExciseDefault;

			if (measure.AdditionalCode.Length == 3)
			{
				rateCode = measure.AdditionalCode;
			}

			return rateCode;
		}

		static string GetVatCodeFromDutyAmount(decimal dutyAmount)
		{
			switch (dutyAmount)
			{
				case VatCodesAndValues.StandardValue:
					return VatCodesAndValues.StandardCode;
				case VatCodesAndValues.ReducedValue:
					return VatCodesAndValues.ReducedCode;
				case VatCodesAndValues.ZeroRatedValue:
					return VatCodesAndValues.ZeroRatedCode;
				default:
					return string.Empty;
			}
		}

		const string CNSubHeadingFootnote = "CD376";
		const string CDSubHeadingPreference = "150";
		const string NormalThirdCountryPreference = "100";
		const string ERGA_OMNES = "1011";
		static readonly string[] DCTSTradeGroups = { "1060", "1061", "1062", "2005", "2020", "2027" };

		public static class VatCodesAndValues
		{
			public const decimal StandardValue = 20;
			public const string StandardCode = "666";

			public const decimal ReducedValue = 5;
			public const string ReducedCode = "650";

			public const decimal ZeroRatedValue = 0;
			public const string ZeroRatedCode = "673";
		}
	}
}
