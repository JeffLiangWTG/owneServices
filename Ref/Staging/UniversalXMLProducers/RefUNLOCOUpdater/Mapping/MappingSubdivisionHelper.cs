using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Mapping
{
	/// <summary>
	/// https://en.wikipedia.org/wiki/ISO_3166-2:<countryCode>
	/// https://www.iso.org/obp/ui#iso:code:3166:<countryCode>
	/// </summary>
	public static class MappingSubdivisionHelper
	{
		public static List<MappedCountry> GenerateMappedSubdivisions()
		{
			var mappedCountries = new List<MappedCountry>();

			mappedCountries.Add(new MappedCountry("AL", GetALMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("BA", GetBAMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("BH", GetBHMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("CD", GetCDMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("CI", GetCIMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("CZ", GetCZMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("GR", GetGRMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("IS", GetISMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("LU", GetLUMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("MK", GetMKMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("MX", GetMXMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("OM", GetOMMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("TT", GetTTMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("UA", GetUAMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("EE", GetEEMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("FR", GetFRMappedSubdivisions()));
			mappedCountries.Add(new MappedCountry("TH", GetTHMappedSubdivisions()));

			return mappedCountries;
		}

		static List<MappedSubdivisions> GetALMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "12", OldSubdivisionCode = "VL" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "11", OldSubdivisionCode = "KA" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "05", OldSubdivisionCode = "GJ" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "12", OldSubdivisionCode = "SR" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "08", OldSubdivisionCode = "LE" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "06", OldSubdivisionCode = "KO" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "01", OldSubdivisionCode = "BR" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "03", OldSubdivisionCode = "EL" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "04", OldSubdivisionCode = "LU" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "11", OldSubdivisionCode = "TR" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "07", OldSubdivisionCode = "KU" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "10", OldSubdivisionCode = "SH" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetBAMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "BIH", OldSubdivisionCode = "10" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "BIH", OldSubdivisionCode = "09" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "BIH", OldSubdivisionCode = "06" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "BIH", OldSubdivisionCode = "07" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetBHMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "14", OldSubdivisionCode = "16" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetCDMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "HU", OldSubdivisionCode = "OR" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetCIMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "LG", OldSubdivisionCode = "16" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "BS", OldSubdivisionCode = "09" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "LG", OldSubdivisionCode = "01" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "DN", OldSubdivisionCode = "10" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetCZMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "32", OldSubdivisionCode = "PL" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "10", OldSubdivisionCode = "PR" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "53", OldSubdivisionCode = "PA" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "72", OldSubdivisionCode = "ZL" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "42", OldSubdivisionCode = "US" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "20", OldSubdivisionCode = "ST" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "51", OldSubdivisionCode = "LI" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "63", OldSubdivisionCode = "VY" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "52", OldSubdivisionCode = "KR" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "71", OldSubdivisionCode = "OL" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "64", OldSubdivisionCode = "JM" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "31", OldSubdivisionCode = "JC" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "41", OldSubdivisionCode = "KA" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetGRMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "G", OldSubdivisionCode = "01" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "H", OldSubdivisionCode = "03" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "H", OldSubdivisionCode = "04" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "H", OldSubdivisionCode = "06" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "H", OldSubdivisionCode = "07" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "J", OldSubdivisionCode = "11" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "J", OldSubdivisionCode = "12" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "G", OldSubdivisionCode = "13" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "G", OldSubdivisionCode = "14" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "J", OldSubdivisionCode = "15" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "J", OldSubdivisionCode = "16" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "J", OldSubdivisionCode = "17" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "F", OldSubdivisionCode = "21" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "F", OldSubdivisionCode = "22" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "F", OldSubdivisionCode = "23" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "F", OldSubdivisionCode = "24" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "D", OldSubdivisionCode = "31" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "D", OldSubdivisionCode = "32" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "D", OldSubdivisionCode = "33" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "D", OldSubdivisionCode = "34" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "E", OldSubdivisionCode = "41" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "E", OldSubdivisionCode = "42" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "E", OldSubdivisionCode = "43" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "E", OldSubdivisionCode = "44" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "A", OldSubdivisionCode = "52" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "B", OldSubdivisionCode = "53" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "B", OldSubdivisionCode = "54" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "A", OldSubdivisionCode = "55" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "B", OldSubdivisionCode = "57" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "C", OldSubdivisionCode = "58" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "B", OldSubdivisionCode = "59" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "B", OldSubdivisionCode = "61" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "B", OldSubdivisionCode = "62" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "C", OldSubdivisionCode = "63" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "B", OldSubdivisionCode = "64" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "A", OldSubdivisionCode = "71" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "A", OldSubdivisionCode = "72" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "A", OldSubdivisionCode = "73" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "L", OldSubdivisionCode = "81" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "L", OldSubdivisionCode = "82" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "K", OldSubdivisionCode = "83" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "K", OldSubdivisionCode = "84" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "K", OldSubdivisionCode = "85" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "M", OldSubdivisionCode = "92" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "M", OldSubdivisionCode = "94" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "M", OldSubdivisionCode = "HK" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "I", OldSubdivisionCode = "A1" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetISMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "8", OldSubdivisionCode = "0" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetLUMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "DI", OldSubdivisionCode = "D" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "GR", OldSubdivisionCode = "G" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "LU", OldSubdivisionCode = "L" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetMKMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "85", OldSubdivisionCode = "17" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetMXMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "CMX", OldSubdivisionCode = "DIF" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetOMMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "BJ", OldSubdivisionCode = "BA" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetTTMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "TOB", OldSubdivisionCode = "ETO" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "MRC", OldSubdivisionCode = "RCM" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetUAMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "65", OldSubdivisionCode = "XJ" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "71", OldSubdivisionCode = "TW" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetEEMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "65", OldSubdivisionCode = "XJ" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetFRMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "71", OldSubdivisionCode = "TW" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "91", OldSubdivisionCode = "HK" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "92", OldSubdivisionCode = "MO" });
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "65", OldSubdivisionCode = "XJ" });

			return mappedSubdivisions;
		}

		static List<MappedSubdivisions> GetTHMappedSubdivisions()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>();
			mappedSubdivisions.Add(new MappedSubdivisions() { CurrentSubdivisionCode = "33", OldSubdivisionCode = "TW" });

			return mappedSubdivisions;
		}
	}
}
