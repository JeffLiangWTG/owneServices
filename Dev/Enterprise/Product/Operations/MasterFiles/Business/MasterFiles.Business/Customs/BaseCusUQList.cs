using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class BaseCusUQList : CodeDescriptionPairList
	{
		public BaseCusUQList()
		{
			AddPair("G", Res.GetString("MasterFiles|BaseCusUQList|Grams", "Grams"));
			AddPair("KG", Res.GetString("MasterFiles|BaseCusUQList|Kilograms", "Kilograms"));
			AddPair("T", Res.GetString("MasterFiles|BaseCusUQList|Tonne", "Tonne"));
			AddPair("CM", Res.GetString("MasterFiles|BaseCusUQList|Centimetres", "Centimeters"));
			AddPair("M", Res.GetString("MasterFiles|BaseCusUQList|Metres", "Meters"));
			AddPair("M2", Res.GetString("MasterFiles|BaseCusUQList|SquareMetresForImport", "Square Meters - For Import"));
			AddPair("SM", Res.GetString("MasterFiles|BaseCusUQList|SquareMetresForExport", "Square Meters - For Export"));
			AddPair("M3", Res.GetString("MasterFiles|BaseCusUQList|CubicMetresForImport", "Cubic Meters - For Import"));
			AddPair("CU", Res.GetString("MasterFiles|BaseCusUQList|CubicMetresForExport", "Cubic Meters - For Export"));
			AddPair("L", Res.GetString("MasterFiles|BaseCusUQList|Litres", "Liters"));
			AddPair("LA", Res.GetString("MasterFiles|BaseCusUQList|LitresOfAlcohol", "Liters of Alcohol"));
			AddPair("NO", Res.GetString("MasterFiles|BaseCusUQList|Number", "Number"));
			AddPair("PR", Res.GetString("MasterFiles|BaseCusUQList|Pair", "Pair"));
			AddPair("TH", Res.GetString("MasterFiles|BaseCusUQList|Thousand", "Thousand"));
			AddPair("SR", Res.GetString("MasterFiles|BaseCusUQList|NumberOfSets", "Number of Sets"));
			AddPair("BC", Res.GetString("MasterFiles|BaseCusUQList|BasicCarton", "Basic Carton"));
			AddPair("CT", Res.GetString("MasterFiles|BaseCusUQList|Carton", "Carton"));
			AddPair("MC", Res.GetString("MasterFiles|BaseCusUQList|MetricCarat", "Metric Carat"));
			AddPair("IU", Res.GetString("MasterFiles|BaseCusUQList|InternationalUnitNumberOfInternationalUnits", "International Unit (Number of International Units)"));
			AddPair("  ", Res.GetString("MasterFiles|BaseCusUQList|NotRequiredForImport", "Not Required - For Import"));
			AddPair("NR", Res.GetString("MasterFiles|BaseCusUQList|NotRecordedForExport", "Not Recorded - For Export"));
		}
	}
}