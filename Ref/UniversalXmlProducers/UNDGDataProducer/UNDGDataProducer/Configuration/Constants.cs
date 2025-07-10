using System.Text;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class Constants
	{
		public static class ProgramArgs
		{
			public const string IATA = "IATA";
			public const string IMO = "IMO";
			public const string PSA = "PSA";
			public const string ADR = "ADR";
			public const string RID = "RID";
			public const string ADN = "ADN";
			public const string CFR = "CFR";
			public const string JTT = "JTT";
			public const string COUNTRYREFERENCE = "COUNTRYREFERENCE";
			public const string COUNTRYREFERENCEPSA = "COUNTRYREFERENCEPSA";
		}

		public static class AttributeTypes
		{
			public const string QualifyingDescriptive = "QDT";
			public const string OtherNames = "OTN";
			public const string SpecialProvisions = "SPP";
			public const string ProperShippingName = "PSN";
		}

		public static class UNDGStandards
		{
			public const string IATADangerousGoodsCode = "IAT";
			public const string IMODangerousGoodsCode = "IMO";
			public const string ADRDangerousGoodsCode = "ADR";
			public const string RIDDangerousGoodsCode = "RID";
			public const string ADNDangerousGoodsCode = "ADN";
			public const string CFRDangerousGoodsCode = "CFR";
			public const string JTTDangerousGoodsCode = "JTT";
		}

		public static class AmtTypes
		{
			public const string ForbiddenAmtType = "FOB";
			public const string NonApplicableAmtType = "NAP";
			public const string NotRestrictedAmtType = "NRE";
			public const string NoLimitAmtType = "NLT";
			public const string NetWeightLimitAmtType = "NLM";
			public const string GrossWeightLimitAmtType = "GLM";
		}

		public static class IMOFiles
		{
			public const string List = "subs.txt";
			public const string SpecProv = "specprov.txt";
			public const string StowSeg = "stowseg.txt";
			public const string Property = "props.txt";
			public const string QualifyingDescriptiveText = "qdt.txt";
		}

		public static class CFRFiles
		{
			public const string List = "cfr.txt";
			public const string QualifyingDescriptiveText = "qdt.txt";
		}

		public static class DataSources
		{
			public const string IATA = "UNDG IATA List";
			public const string IMO = "UNDG IMO List";
			public const string CommonData = "UNDG Common Data";
			public const string ADR = "UNDG ADR List";
			public const string RID = "UNDG RID List";
			public const string ADN = "UNDG ADN List";
			public const string JTT = "UNDG JTT List";
			public const string CFR = "UNDG CFR List";

			public const string UNDGCountryReference = "UNDG Country Reference";
			public const string PSA = "PSA";
			public const string PSACountry = "SG";
			public const string PSAGroup = "Singapore PSA Group";
			public const string PSAGroupURL = @"https://www.portnet.com/DGWebPublic/com/pn2/dg/web/newdgchemicalpublic/searchChemicalList.do?%7bactionForm.searchBean.unNoFr%7d={0}";
		}

		public static class Encodings
		{
			public static readonly Encoding IMOZipFile = Encoding.GetEncoding("iso-8859-1");
			public static readonly Encoding CFRZipFile = Encoding.GetEncoding("iso-8859-1");
			public static readonly Encoding IATAFile = Encoding.UTF8;
			public static readonly Encoding ADRFile = Encoding.UTF8;
			public static readonly Encoding RIDFile = Encoding.UTF8;
			public static readonly Encoding ADNFile = Encoding.UTF8;
			public static readonly Encoding JTTFile = Encoding.UTF8;
			public static readonly Encoding UNDGCountryReferenceFile = Encoding.UTF8;
		}

		public static class Smartproxy
		{
			public const string ProxyUrl = "http://gate.smartproxy.com:7000";
		}

		public static class WebSraper
		{
			public const int DefaultMaxAttempts = 3;
		}

		public const int FetchPSAGroupBatchSize = 5;
	}
}
