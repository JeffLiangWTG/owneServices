using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	internal static class UomCodeLookup
	{
		internal static IEnumerable<UomCode> UomCodes { get; } = new List<UomCode>
		{
			new UomCode("ASV X", "ASVX"),
			new UomCode("ASV", "ASV"),
			new UomCode("CCT", "CCT"),
			new UomCode("CEN", "CEN"),
			new UomCode("CTM", "CTM"),
			new UomCode("DAP", "DAP"),
			new UomCode("DHS", "DHS"),
			new UomCode("DTN E", "DTNE"),
			new UomCode("DTN F", "DTNF"),
			new UomCode("DTN G", "DTNG"),
			new UomCode("DTN L", "DTNL"),
			new UomCode("DTN M", "DTNM"),
			new UomCode("DTN R", "DTNR"),
			new UomCode("DTN S", "DTNS"),
			new UomCode("DTN Z", "DTNZ"),
			new UomCode("DTN", "DTN"),
			new UomCode("ENC ENP", "ENP"),
			new UomCode("GFI", "GFI"),
			new UomCode("GRM", "GRM"),
			new UomCode("GRT", "GRT"),
			new UomCode("HLT", "HLT"),
			new UomCode("HMT", "HMT"),
			new UomCode("KAC", "KAC"),
			new UomCode("KCC", "KCC"),
			new UomCode("KCL", "KCL"),
			new UomCode("KGM A", "KGMA"),
			new UomCode("KGM E", "KGME"),
			new UomCode("KGM G", "KGMG"),
			new UomCode("KGM P", "KGMP"),
			new UomCode("KGM S", "KGMS"),
			new UomCode("KGM T", "KGMT"),
			new UomCode("KGM", "KGM"),
			new UomCode("KLT", "KLT"),
			new UomCode("KMA", "KMA"),
			new UomCode("KMT", "KMT"),
			new UomCode("KNI", "KNI"),
			new UomCode("KNS", "KNS"),
			new UomCode("KPH", "KPH"),
			new UomCode("KPO", "KPO"),
			new UomCode("KPP", "KPP"),
			new UomCode("KSD", "KSD"),
			new UomCode("KSH", "KSH"),
			new UomCode("KUR", "KUR"),
			new UomCode("LPA", "LPA"),
			new UomCode("LTR A", "LTRA"),
			new UomCode("LTR", "LTR"),
			new UomCode("MIL", "MIL"),
			new UomCode("MPR", "MPR"),
			new UomCode("MTK", "MTK"),
			new UomCode("MTQ C", "MTQC"),
			new UomCode("MTQ", "MTQ"),
			new UomCode("MTR", "MTR"),
			new UomCode("MWH", "MWH"),
			new UomCode("NAR B", "NARB"),
			new UomCode("NAR", "NAR"),
			new UomCode("NCL", "NCL"),
			new UomCode("NPR", "NPR"),
			new UomCode("TJO", "TJO"),
			new UomCode("TNE E", "TNEE"),
			new UomCode("TNE I", "TNEI"),
			new UomCode("TNE J", "TNEJ"),
			new UomCode("TNE K", "TNEK"),
			new UomCode("TNE M", "TNEM"),
			new UomCode("TNE R", "TNER"),
			new UomCode("TNE Z", "TNEZ"),
			new UomCode("TNE", "TNE"),
			new UomCode("WAT", "WAT")
		};
	}

	internal class UomCode
	{
		public UomCode(string rawUomCode, string actualUomCode)
		{
			Argument.NotNullOrEmpty(rawUomCode, nameof(rawUomCode));
			Argument.NotNullOrEmpty(actualUomCode, nameof(actualUomCode));

			RawUomCode = rawUomCode;
			ActualUomCode = actualUomCode;
		}

		public string RawUomCode { get; }
		public string ActualUomCode { get; }
	}
}
