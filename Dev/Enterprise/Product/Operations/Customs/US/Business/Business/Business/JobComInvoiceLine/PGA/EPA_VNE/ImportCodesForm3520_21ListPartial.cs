namespace Enterprise.Customs.US.Business
{
	partial class ImportCodesForm3520_21List
	{
		public static bool IsEngineFamilyNumberRequired(string code)
		{
			return code == Codes._01 || code == Codes._22;
		}

		public static bool IsModelYearRequired(string code)
		{
			return code == Codes._09 || code == Codes._17 || code == Codes._19 || code == Codes._22 || code == Codes._24A || code == Codes._24B;
		}

		public static bool IsDISFilingRequired(string code)
		{
			return code == Codes._02 || code == Codes._03 || code == Codes._04 || code == Codes._05 || code == Codes._06 || code == Codes._07
					|| code == Codes._08 || code == Codes._11 || code == Codes._12 || code == Codes._13 || code == Codes._14 || code == Codes._15;
		}

		public static bool IsExemptionNumberRequired(string code)
		{
			return code == Codes._02 || code == Codes._10 || code == Codes._11 || code == Codes._12 || code == Codes._18;
		}

		public static bool IsExemptionRemarksRequired(string code)
		{
			return code == Codes._21 || code == Codes._25;
		}

		public static bool IsStorageLocationRequired(string code)
		{
			return code == Codes._24A || code == Codes._24B;
		}

		public static bool IsEnginePowerRequired(string code)
		{
			return code == Codes._19 || code == Codes._22 || code == Codes._23;
		}

		public static bool IsICIRequired(string code)
		{
			return code == Codes._24A || code == Codes._24B || code == Codes._24C;
		}
	}
}
