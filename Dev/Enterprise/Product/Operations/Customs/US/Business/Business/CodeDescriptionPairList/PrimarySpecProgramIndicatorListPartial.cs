
namespace Enterprise.Customs.US.Business
{
	partial class PrimarySpecProgramIndicatorList
	{
		/// <summary>
		/// Some indicators are not available in the DB for checking - currently N, W, Y, Z
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public static bool IndicatorIsExcludedFromDBCheck(string spiCode)
		{
			return spiCode == Codes.N || spiCode == Codes.W || spiCode == Codes.Y || spiCode == Codes.Z;
		}

		public static bool IsNonCountrySpecific(string spiCode)
		{
			return spiCode == Codes.C || spiCode == Codes.K || spiCode == Codes.L;
		}

		public static bool HasNAFTAEquivalentSPICountryCode(string spiCode)
		{
			return spiCode == Codes.B || spiCode == Codes.C || spiCode == Codes.K || spiCode == Codes.L;
		}

		public static bool IsDutyFreeSPI(string spiCode)
		{
			return
				spiCode == Codes.A ||
				spiCode == Codes.B ||
				spiCode == Codes.C ||
				spiCode == Codes.D ||
				spiCode == Codes.K ||
				spiCode == Codes.L ||
				spiCode == Codes.N ||
				spiCode == Codes.R ||
				spiCode == Codes.W ||
				spiCode == Codes.Y ||
				spiCode == Codes.Z;
		}
	}
}
