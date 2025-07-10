using System;
using System.Text;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public static class Constants
{
	public static class ProgramFunctions
	{
		public const string ExchangeRates = "EXCHANGERATES";
		public const string VAT = "VAT";
		public const string IGIC = "IGIC";
		public const string AIEM = "AIEM";
		public const string ESEXC = "ESEXC";
		public const string CANEXC = "CANEXC";
		public const string C44DOC = "C44DOC";
		public const string REA = "REA";
		public const string LOCATIONS = "LOCATIONS";
		public const string CPC = "CPC";
		public const string MEA = "MEA";
		public const string REAMeasures = "REAMEASURES";
		public const string Quota = "QUOTA";
		public const string CSVProcessor = "CSVPROCESSOR";
		public const string TariffOne = "TARIFFONE";
		public const string TariffOneRates = "TARIFFONERATES";
	}

	public static class DataSources
	{
		public const string ExchangeRates = "ES Exchange Rates";
		public const string REA_Rates = "ES REA Rates";
		public const string REA_Codes = "ES REA Codes";
		public const string REAMeasures = "ES REA Measures";
		public const string MEA = "ES MEA Codes and Rates";
		public const string Quota = "ES Quota Codes and Rates";
		public const string DC44_Codes = "ES DC44 Codes";
		public const string Export_CPC_Codes = "ES Export CPC Codes";
		public const string Import_CPC_Codes = "ES Import CPC Codes";
		public const string C44AddInfo_Codes = "ES ADDIN Code List";
		public const string Location_Codes = "ES Location Codes";
		public const string VAT = "ES VAT";
		public const string IGIC = "ES IGIC";
		public const string AIEM = "ES AIEM";
		public const string CANEXC = "ES CANEXC";
		public const string ESEXC = "ES ESEXC";
		public const string DC40A = "ES DC40A Codes";
		public const string DC40E = "ES DC40E Codes";
		public const string DC40N = "ES DC40N Codes";
		public const string DC40W = "ES DC40W Codes";
		public const string DC40X = "ES DC40X Codes";
		public const string DC44H = "ES DC44H Codes";
		public const string AI44E = "ES AI44E Codes";
		public const string EXSEC = "ES EXSEC Codes";
		public const string TD44E = "ES TD44E Codes";
		public const string TD44G = "ES TD44G Codes";
		public const string DC40T = "ES DC40T Codes";
		public const string ES_Tariffs = "ES Tariff";
		public const string ES_Nomenclatures = "ES NomenclatureGroups";
		public const string ES_Rates = "ES Spanish Rates";
	}

	public static class RefCusCodeListTypes
	{
		public const string AI44E = "AI44E";
		public const string DC40A = "DC40A";
		public const string DC40E = "DC40E";
		public const string DC40N = "DC40N";
		public const string DC40T = "DC40T";
		public const string DC40W = "DC40W";
		public const string DC40X = "DC40X";
		public const string DC44H = "DC44H";
		public const string EXSEC = "EXSEC";
		public const string TD44E = "TD44E";
		public const string TD44G = "TD44G";
	}

	public static class Excises
	{
		public const string CANEXCCode = "CANIIEE";
		public const string ESEXCCode = "ESIIEE";
	}

	public static class REA
	{
		public const string REACodeForDirectComsuption = "CANAYC";
		public const string REACodeForTransformation = "CANAYT";
		public const string MeasureTypeCode = "CANREA";
	}

	public static class MEA
	{
		public const string MeasureTypeCode = "CANS";
	}

	public static class Quota
	{
		public const string MeasureTypeCode = "CANK";
	}

	public static class RateType
	{
		public const string Duty = "DTY";
	}

	public static class RateCode
	{
		public const string CustomDutiesOnIndustrialProducts = "A00";
	}

	public static class TariffType
	{
		public const string Import = "IMP";
		public const string Export = "EXP";
	}

	public static class ConditionType
	{
		public const string QuotaSuspensions = "ESCIQ";
		public const string MEASuspensions = "ESCIM";
		public const string REASuspensions = "ESCIR";
	}

	public static class ConditionClass
	{
		public const string Rate = "RATE";
	}

	public static class Preference
	{
		public const string GoodsCoveredByQuota = "183";
		public const string GoodsCoveredByMEA = "184";
		public const string GoodsCoveredByREA = "185";
		public const string TobaccoProductCoveredByREA = "186";
		public const string ErgaOmnesThirdCountryDutyRates = "100";
	}

	public static class ConditionValueType
	{
		public const string SupportingDocuments = "SUP";
	}

	public const string CountryCode = "ES";

	public const string EuropeanUnion = "EUN";

	public const string Yes = "Y";

	public const string No = "N";

	public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);

	public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

	public static Encoding ESFileEncoding => Encoding.GetEncoding("UTF-8");
}
