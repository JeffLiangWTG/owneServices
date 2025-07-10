using System.Collections.Generic;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using Microsoft.Extensions.Logging;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public class HsnTariffDTYRatesReader_List1 : HsnTariffDTYRatesReader
	{
		public HsnTariffDTYRatesReader_List1(ILogger logger) : base(logger)
		{ }

		public HsnTariffDTYRatesReader_List1(string inputPath, ILogger logger) : base(inputPath, logger) { }

		static class Constants
		{
			public const string ABAndBK = "AB, BK";
			public const string GUR = "GÜR";
			public const string BHS = "B-HER";
			public const string KOR = "G.KORE";
			public const string MLZ = "MLZ";
			public const string SNG = "SNG";
			public const string KOS = "KOS";
			public const string VNZ = "VNZ";
			public const string TPSOIC = "TPS-OIC";
			public const string D8 = "D-8";
			public const string Others = "DÜ";
		}

		protected override string ListFileName => "I sayìlì Liste 2023.xlsx";

		protected override string ExplanationFileName => "Explanations for List Number I.xlsx";

		protected override HashSet<string> ListHeaders => new HashSet<string>
		{
			DtyRateConstants.TariffCode,
			DtyRateConstants.Footnote,
			Constants.ABAndBK,
			Constants.GUR,
			Constants.BHS,
			Constants.KOR,
			Constants.MLZ,
			Constants.SNG,
			Constants.KOS,
			Constants.VNZ,
			Constants.TPSOIC,
			Constants.D8,
			Constants.Others
		};

		protected override HashSet<string> SearchHeaders => new HashSet<string>
		{
			DtyRateConstants.TariffCode
		};

		protected override HashSet<string> PreferenceHeaders => new HashSet<string> {
			Constants.ABAndBK,
			Constants.GUR,
			Constants.BHS,
			Constants.KOR,
			Constants.MLZ,
			Constants.SNG,
			Constants.KOS,
			Constants.VNZ,
			Constants.TPSOIC,
			Constants.D8,
			Constants.Others
		};

		protected override string TariffCodeHeader => DtyRateConstants.TariffCode;

		protected override string FootnoteHeader => DtyRateConstants.Footnote;
	}
}
