using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.LLIReferenceData.Services.Vessel;

namespace CargoWise.RefDbRepo.LLIReferenceData.Business.Vessel;

public static class VesselParser
{
	const string DataSource = "LLI Vessels List";
	const string OutputFileName = "RefVesselList_LLI_Vessel.xml";
	const string UnknownCountryCode = "XX";
	const int MaxNameLength = 50;

	static readonly IReadOnlyDictionary<string, string> CountryCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "AFG", "AF" },
		{ "AGO", "AO" },
		{ "AIA", "AI" },
		{ "ALB", "AL" },
		{ "ALD", "GG" },
		{ "AND", "AD" },
		{ "ARE", "AE" },
		{ "ARG", "AR" },
		{ "ARM", "AM" },
		{ "ATG", "AG" },
		{ "AUS", "AU" },
		{ "AUT", "AT" },
		{ "AZE", "AZ" },
		{ "AZO", "PT" },
		{ "BDI", "BI" },
		{ "BEL", "BE" },
		{ "BEN", "BJ" },
		{ "BFA", "BF" },
		{ "BGD", "BD" },
		{ "BGR", "BG" },
		{ "BHR", "BH" },
		{ "BHS", "BS" },
		{ "BIH", "BA" },
		{ "BLR", "BY" },
		{ "BLZ", "BZ" },
		{ "BMU", "BM" },
		{ "BOL", "BO" },
		{ "BRA", "BR" },
		{ "BRB", "BB" },
		{ "BRN", "BN" },
		{ "BTN", "BT" },
		{ "BWA", "BW" },
		{ "CAF", "CF" },
		{ "CAN", "CA" },
		{ "CHE", "CH" },
		{ "CHL", "CL" },
		{ "CHN", "CN" },
		{ "CIV", "CI" },
		{ "CMR", "CM" },
		{ "COD", "CD" },
		{ "COG", "CG" },
		{ "COK", "CK" },
		{ "COL", "CO" },
		{ "COM", "KM" },
		{ "CPV", "CV" },
		{ "CRI", "CR" },
		{ "CUB", "CU" },
		{ "CUW", "CW" },
		{ "CYM", "KY" },
		{ "CYP", "CY" },
		{ "CZE", "CZ" },
		{ "DEU", "DE" },
		{ "DIS", "DK" },
		{ "DJI", "DJ" },
		{ "DMA", "DM" },
		{ "DNK", "DK" },
		{ "DOM", "DO" },
		{ "DZA", "DZ" },
		{ "ECU", "EC" },
		{ "EGY", "EG" },
		{ "ERI", "ER" },
		{ "ESP", "ES" },
		{ "EST", "EE" },
		{ "ETH", "ET" },
		{ "FIN", "FI" },
		{ "FJI", "FJ" },
		{ "FLK", "FK" },
		{ "FRA", "FR" },
		{ "FRO", "DK" },
		{ "FSM", "FM" },
		{ "GAB", "GA" },
		{ "GBR", "GB" },
		{ "GEO", "GE" },
		{ "GHA", "GH" },
		{ "GIB", "GI" },
		{ "GIN", "GN" },
		{ "GLP", "GP" },
		{ "GMB", "GM" },
		{ "GNB", "GW" },
		{ "GNQ", "GQ" },
		{ "GRC", "GR" },
		{ "GRD", "GD" },
		{ "GRL", "GL" },
		{ "GTM", "GT" },
		{ "GUF", "GF" },
		{ "GUY", "GY" },
		{ "HKG", "HK" },
		{ "HND", "HN" },
		{ "HRV", "HR" },
		{ "HTI", "HT" },
		{ "HUN", "HU" },
		{ "IDN", "ID" },
		{ "IND", "IN" },
		{ "IOM", "IM" },
		{ "IRL", "IE" },
		{ "IRN", "IR" },
		{ "IRQ", "IQ" },
		{ "ISL", "IS" },
		{ "ISR", "IL" },
		{ "ITA", "IT" },
		{ "JAM", "JM" },
		{ "JOR", "JO" },
		{ "JPN", "JP" },
		{ "KAZ", "KZ" },
		{ "KEN", "KE" },
		{ "KGZ", "KG" },
		{ "KHM", "KH" },
		{ "KIR", "KI" },
		{ "KNA", "KN" },
		{ "KOR", "KR" },
		{ "KWT", "KW" },
		{ "LAO", "LA" },
		{ "LBN", "LB" },
		{ "LBR", "LR" },
		{ "LBY", "LY" },
		{ "LCA", "LC" },
		{ "LIE", "LI" },
		{ "LKA", "LK" },
		{ "LSO", "LS" },
		{ "LTU", "LT" },
		{ "LUX", "LU" },
		{ "LVA", "LV" },
		{ "MAR", "MA" },
		{ "MCO", "MC" },
		{ "MDA", "MD" },
		{ "MDG", "MG" },
		{ "MDV", "MV" },
		{ "MEX", "MX" },
		{ "MHL", "MH" },
		{ "MLI", "ML" },
		{ "MLT", "MT" },
		{ "MMR", "MM" },
		{ "MNE", "ME" },
		{ "MNG", "MN" },
		{ "MOZ", "MZ" },
		{ "MRT", "MR" },
		{ "MTG", "MS" },
		{ "MTQ", "MQ" },
		{ "MUS", "MU" },
		{ "MWI", "MW" },
		{ "MYS", "MY" },
		{ "MYT", "YT" },
		{ "NAM", "NA" },
		{ "NER", "NE" },
		{ "NGA", "NG" },
		{ "NIC", "NI" },
		{ "NIS", "NO" },
		{ "NIU", "NU" },
		{ "NLD", "NL" },
		{ "NOR", "NO" },
		{ "NPL", "NP" },
		{ "NRU", "NR" },
		{ "NZL", "NZ" },
		{ "OMN", "OM" },
		{ "PAK", "PK" },
		{ "PAN", "PA" },
		{ "PER", "PE" },
		{ "PHL", "PH" },
		{ "PLW", "PW" },
		{ "PMD", "PT" },
		{ "PNG", "PG" },
		{ "POL", "PL" },
		{ "PRI", "PR" },
		{ "PRK", "KP" },
		{ "PRT", "PT" },
		{ "PRY", "PY" },
		{ "QAT", "QA" },
		{ "REU", "RE" },
		{ "RIF", "FR" },
		{ "ROM", "RO" },
		{ "ROU", "RO" },
		{ "RUS", "RU" },
		{ "RWA", "RW" },
		{ "SAU", "SA" },
		{ "SDN", "SD" },
		{ "SEN", "SN" },
		{ "SGP", "SG" },
		{ "SHN", "SH" },
		{ "SLB", "SB" },
		{ "SLE", "SL" },
		{ "SLV", "SV" },
		{ "SMR", "SM" },
		{ "SOM", "SO" },
		{ "SPM", "PM" },
		{ "SRB", "RS" },
		{ "STP", "ST" },
		{ "SUR", "SR" },
		{ "SVK", "SK" },
		{ "SVN", "SI" },
		{ "SWE", "SE" },
		{ "SWZ", "SZ" },
		{ "SXM", "SX" },
		{ "SYC", "SC" },
		{ "SYR", "SY" },
		{ "TAH", "PF" },
		{ "TCD", "TD" },
		{ "TGO", "TG" },
		{ "THA", "TH" },
		{ "TJK", "TJ" },
		{ "TKL", "TK" },
		{ "TKM", "TM" },
		{ "TLS", "TL" },
		{ "TON", "TO" },
		{ "TTO", "TT" },
		{ "TUN", "TN" },
		{ "TUR", "TR" },
		{ "TUV", "TV" },
		{ "TWN", "TW" },
		{ "TZA", "TZ" },
		{ "UGA", "UG" },
		{ "UKR", "UA" },
		{ "UNK", UnknownCountryCode },
		{ "URY", "UY" },
		{ "USA", "US" },
		{ "UZB", "UZ" },
		{ "VCT", "VC" },
		{ "VEN", "VE" },
		{ "VGB", "VG" },
		{ "VNM", "VN" },
		{ "VUT", "VU" },
		{ "WSM", "WS" },
		{ "YEM", "YE" },
		{ "ZAF", "ZA" },
		{ "ZMB", "ZM" },
		{ "ZWE", "ZW" }
	};

	static readonly IReadOnlyList<string> CVTypeList = ["BBU", "BCB", "DSS", "GGC", "GPC", "GRF", "OBA", "OHB", "OHL", "ZZZ"];
	static readonly IReadOnlyList<string> RORTypeList = ["MVE", "OFY", "OLC", "ORN", "PRR", "URC", "URR"];
	static readonly IReadOnlyList<string> CNTTypeList = ["UBC", "UCC", "UCR"];

	public static void ParseAndExport(
		LliVesselsData vesselData,
		string outputFolderPath,
		DateTime publishDate)
	{
		var refVessels = Parse(vesselData);
		var outputFileFullName = Path.Combine(outputFolderPath, OutputFileName);
		VesselExporter.ExportToXMLFile(outputFileFullName, DataSource, publishDate, refVessels);
	}

	static IReadOnlyList<RefVessel> Parse(LliVesselsData vesselData)
	{
		var (vessels, vesselsBasicCharacteristics, vesselsAdvancedCharacteristics) = vesselData;
		Console.WriteLine($"Starting parsing {vessels.Count} vessels.");

		var basicCharacteristicsById = vesselsBasicCharacteristics.ToDictionary(b => b.VesselId, StringComparer.OrdinalIgnoreCase);
		var advancedCharacteristicsById = vesselsAdvancedCharacteristics.ToDictionary(a => a.VesselAdvancedChars.VesselId, StringComparer.OrdinalIgnoreCase);

		var refVessels = vessels
			.Select(v =>
			{
				if (!basicCharacteristicsById.TryGetValue(v.VesselId, out var basicCharacteristics))
				{
					throw new InvalidOperationException($"Basic characteristics not found for vessel {v.VesselImo}.");
				}

				if (!advancedCharacteristicsById.TryGetValue(v.VesselId, out var advancedCharacteristics))
				{
					throw new InvalidOperationException($"Advanced characteristics not found for vessel {v.VesselImo}.");
				}

				return new RefVessel
				{
					RV_Code = ConvertVesselName(v.VesselName),
					RV_IsActive = v.VesselStatus == "Live",
					RV_LloydsNumber = v.VesselImo,
					RV_RadioCallSign = basicCharacteristics.Callsign,
					RV_VesselType = ConvertVesselType(basicCharacteristics.GenType, basicCharacteristics.SubType),
					RV_YearOfConstruction = GetShortValue(basicCharacteristics.Built),
					RV_RN_NKCountryOfReg = ConvertCountryCode(basicCharacteristics.Flag),
					RV_MaritimeMobileServiceIdentity = basicCharacteristics.Mmsi,
					RV_StatusCode = null,
					RV_StatCode5 = null,
					RV_IsGearless = GetBooleanValue(advancedCharacteristics.VesselDesignSuperStructure.Gearless),
					RV_Length = GetDecimalValue(
						"RV_Length",
						7,
						3,
						advancedCharacteristics.VesselDimensions.Loa,
						advancedCharacteristics.VesselDimensions.Lrg,
						advancedCharacteristics.VesselDimensions.Lbp),
					RV_Breadth = GetDecimalValue(
						"RV_Breadth",
						7,
						3,
						advancedCharacteristics.VesselDimensions.BreadthExtreme,
						advancedCharacteristics.VesselDimensions.BreadthMoulded),
					RV_Draught = GetDecimalValue("RV_Draught", 7, 3, advancedCharacteristics.VesselDimensions.Draft),
					RV_Deadweight = GetIntValue(basicCharacteristics.DwtTonnage),
					RV_GrossTonnage = GetIntValue(basicCharacteristics.GrossTonnage),
					RV_GrainCapacity = GetDecimalValue("RV_GrainCapacity", 10, 3, advancedCharacteristics.VesselCapacities.Grain),
					RV_LiquidCapacity = GetDecimalValue("RV_LiquidCapacity", 10, 3, advancedCharacteristics.VesselCapacities.Liquid),
					RV_RoroLanesLength = GetDecimalValue("RV_RoroLanesLength", 8, 3, advancedCharacteristics.VesselDesignSuperStructure.LaneLength),
					RV_RoroLanesWidth = GetDecimalValue("RV_RoroLanesWidth", 8, 3, advancedCharacteristics.VesselDesignSuperStructure.LaneWidth),
					RV_RoroLanesClearHeight = 0,
					RV_RoroLanesNumber = 0,
					RV_RoroRampsNumber = 0,
					RV_TEU = GetIntValue(advancedCharacteristics.VesselCapacities.Teu),
					RV_CarsNumber = GetIntValue(advancedCharacteristics.VesselCapacities.Cars),
					RV_ReeferPointsNumber = GetShortValue(advancedCharacteristics.VesselDesignSuperStructure.ReeferPlugs),
					RV_TanksNumber = GetShortValue(advancedCharacteristics.VesselDesignSuperStructure.TotalTanks)
				};
			}).ToList();

		Console.WriteLine($"Finished parsing {vessels.Count} vessels.");

		return refVessels;
	}

	static string ConvertVesselType(string genType, string subType)
	{
		string key = $"{genType}{subType}";

		switch (key)
		{
			case var _ when CVTypeList.Contains(key, StringComparer.OrdinalIgnoreCase):
				return "CV";
			case var _ when RORTypeList.Contains(key, StringComparer.OrdinalIgnoreCase):
				return "ROR";
			case var _ when CNTTypeList.Contains(key, StringComparer.OrdinalIgnoreCase):
				return "CNT";
			default:
				return "CV";
		}
	}

	static string ConvertVesselName(string name)
	{
		name = name.Replace("- ", "-").TrimEnd('.').ToUpperInvariant();

		if (name.Length <= MaxNameLength)
		{
			return name;
		}
		else
		{
			var errorMessage = $"Vessel name '{name}' exceeds the maximum allowed length of {MaxNameLength} and will be truncated.";
			Console.Error.WriteLine(errorMessage);
			return name[..50];
		}
	}

	static string ConvertCountryCode(string flag)
	{
		if (CountryCodeMap.TryGetValue(flag, out string twoCharCode))
		{
			return twoCharCode;
		}
		else
		{
			var errorMessage = $"Unable to parse the country code '{flag}', using the default value '{UnknownCountryCode}'.";
			Console.Error.WriteLine(errorMessage);
			return UnknownCountryCode;
		}
	}

	static decimal GetDecimalValue(string propertyName, int precision, int scale, params double?[] values)
	{
		var value = (decimal?)values.FirstOrDefault(v => v.HasValue) ?? 0;

		string[] parts = Math.Abs(value).ToString(CultureInfo.InvariantCulture).Split('.');
		int integerDigits = parts[0].Length;
		int decimalDigits = (parts.Length > 1) ? parts[1].Length : 0;

		if (integerDigits <= precision - scale && decimalDigits <= scale)
		{
			return value;
		}
		else
		{
			var errorMessage = $"Value '{value}' is not a valid DECIMAL({precision},{scale}) for the property {propertyName}, using the default value '0'.";
			Console.Error.WriteLine(errorMessage);
			return 0;
		}
	}

	static short GetShortValue(int? value) => (short?)value ?? 0;

	static int GetIntValue(int? value) => value ?? 0;

	static int GetIntValue(double? value) => Convert.ToInt32(value ?? 0);

	static bool GetBooleanValue(bool? value) => value ?? false;
}
