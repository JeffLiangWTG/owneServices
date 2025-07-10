using System;
using System.Collections.Immutable;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public class UomCodeLookup : IUomCodeLookup
	{
		public UomCodeLookup()
		{
			lookupListLazy = new Lazy<(string, string)[]>(GetLookupList);
		}

		public string Lookup(string rawRateFormula)
		{
			if (string.IsNullOrEmpty(rawRateFormula))
			{
				return null;
			}

			return LookupList
				.Where(x => rawRateFormula.EndsWith(x.Abbreviation, StringComparison.InvariantCulture))
				.Select(x => x.Code)
				.FirstOrDefault();
		}

		protected virtual (string Code, string Abbreviation)[] GetLookupList() => defaultLookupList.ToArray();

		(string Code, string Abbreviation)[] LookupList => lookupListLazy.Value;

		readonly Lazy<(string, string)[]> lookupListLazy;

		#region Default Lookup List

		static readonly ImmutableArray<(string Code, string Description)> defaultLookupList = new (string, string)[]
		{
			("ASV", "vol" ),
			("ASVX", "vol/hl" ),
			("ASVX", "100 l/Grado Plato" ),
			("CCT", "ct/l" ),
			("CEN", "100 p/st" ),
			("CTM", "c/k" ),
			("DAP", "10 000 kg/polar" ),
			("DHS", "kg DHS" ),
			("DTN", "100 kg" ),
			("DTNE", "100 kg/net eda" ),
			("DTNF", "100 kg common wheat" ),
			("DTNG", "100 kg/br" ),
			("DTNL", "100 kg live weight" ),
			("DTNM", "100 kg/net mas" ),
			("DTNR", "100 kg std qual" ),
			("DTNS", "100 kg raw sugar" ),
			("DTNZ", "100 kg/net/%sacchar" ),
			("EUR", "EUR" ),
			("GFI", "gi F/S" ),
			("GRT", "GT" ),
			("HLT", "hl" ),
			("HMT", "100 m" ),
			("KAC", "Kg net Ace K" ),
			("KCC", "kg C₅H₁₄ClNO" ),
			("KCL", "tonne KCl" ),
			("KGMA", "kg/tot/alc" ),
			("KGME", "kg.net eda" ),
			("KGMG", "GKG" ),
			("KGMP", "kg/lactic matter" ),
			("KGMS", "kg/raw sugar" ),
			("KGMT", "kg/dry lactic matter" ),
			("KLT", "1000 l" ),
			("KMA", "kg methylamines" ),
			("KMT", "KM" ),
			("KNI", "kg N" ),
			("KNS", "kg H₂O₂" ),
			("KPH", "kg KOH" ),
			("KPO", "kg K₂O" ),
			("KPP", "kg P₂O₅" ),
			("KSD", "kg 90% sdt" ),
			("KSH", "kg NaOH" ),
			("KUR", "kg U" ),
			("LPA", "l alc 100%" ),
			("LPA", "l alc. 100%" ),
			("LTRA","L total alc" ),
			("MIL", "1000 p/st" ),
			("MPR", "1000 pa" ),
			("MTK", "m²" ),
			("MTQ", "m³" ),
			("MTQ", "m3" ),
			("MTQC","1000 m³" ),
			("MWH",  "1000 kWh" ),
			("NAR",  "p/st" ),
			("NARB","b/f" ),
			("NCL",  "ce/el" ),
			("NPR",  "pa" ),
			("TJO",  "TJ" ),
			("TNE",  "1000 kg" ),
			("TNEE","1000 kg/net eda" ),
			("TNEI","1000 kg/biodiesel" ),
			("TNEJ","1000 kg/fuel content" ),
			("TNEK","1000 kg/bioethanol" ),
			("TNEM","1000 kg/net mas" ),
			("TNER","1000 kg std qual" ),
			("TNEZ","1000 kg/net/%saccha." ),
			("WAT",  "Watt" ),
			("KGM",  "kg" ),
			("GRM",  "g" ),
			("LTR",  "l" ),
			("MTR", "m")
		}.ToImmutableArray();

		#endregion
	}
}
