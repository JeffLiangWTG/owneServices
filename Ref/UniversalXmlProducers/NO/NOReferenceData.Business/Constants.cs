using System;

namespace CargoWise.RefDbRepo.NOReferenceData.Business;

public static class Constants
{
	public static class CountryCodes
	{
		public const string Norway = "NO";
	}

	public static class TradeGroupCodes
	{
		public const string AllCountries = "ALLE";
		public const string OrdinaryCustoms = "TALL";
	}

	public static class CodeTypes
	{
		public const string DC44CodeImport = "DC44I";
		public const string DC44CodeExport = "DC44E";
		public const string ErrorMsgCodes = "ERRCD";
	}

	public static class RateTypes
	{
		public const string BV515 = "BV515";
		public const string BV516 = "BV516";
		public const string BV517 = "BV517";
		public const string BV610 = "BV610";
		public const string BV620 = "BV620";
		public const string BV630 = "BV630";
		public const string BV640 = "BV640";
		public const string BV650 = "BV650";
		public const string OL720 = "OL720";
		public const string OL730 = "OL730";

		public const string RawMaterialDutiesRate = "RT100";
		public const string Duty = "DTY";
		public const string DutyInPercent = "DTP";
	}

	public static class Types
	{
		public const string ExciseImport = "EXC";
		public const string ExciseExport = "EXP";
		public const string Duty = "DTY";
		public const string VAT = "MV";
		public const string RawMaterialDuties = "RTO";
	}

	public static DateTime MinimumDateTime => new DateTime(2000, 01, 01, 00, 00, 00);
	public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

	public const string XmlDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
	public const string YearMonthDateFormat = "yyyy-MM-dd";
	public const string FullDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

	public const string RateGivenInFractionsOfKroner = "RateGivenInFractionsOfKroner";
	public const string RT100Dummyvalue = "999999,99";
}

public static class UomCodes
{
	public const string AlcoholVolumePercentage = "ASV";
	public const string Carat = "HE";
	public const string Fat = "BLL";
	public const string Gram = "GRM";
	public const string Torrvekt = "KSD";
	public const string Kilogram = "KGM";
	public const string Liter = "LTR";
	public const string Meter = "MTR";
	public const string Kvadratmeter = "MTK";
	public const string Kubikkmeter = "MTQ";
	public const string KubikkmeterFast = "MTQ";
	public const string Megawattime = "MWH";
	public const string AntallPar = "NPR";
	public const string AntallEnheter = "NMB";
	public const string ValueForDuty = "VFD";
	public const string MilliLiter = "MLT";
}
