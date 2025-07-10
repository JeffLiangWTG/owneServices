namespace Enterprise.Customs.US.Business
{
	partial class ImportCodesForm3520_1List
	{
		public static bool IsImportedByICI(string code)
		{
			return code == Codes.A || code == Codes.C || code == Codes.J || code == Codes.Z;
		}

		public static bool IsPolicyNumberRequired(string code)
		{
			return code == Codes.G || code == Codes.I || code == Codes.K || code == Codes.J;
		}

		public static bool IsEngineFamilyNumberRequired(string code)
		{
			return code == Codes.B || code == Codes.F;
		}

		public static bool IsDISFilingRequired(string code)
		{
			return code == Codes.EE || code == Codes.FF || code == Codes.M || code == Codes.N;
		}

		public static bool IsExemptionNumberRequired(string code)
		{
			return code == Codes.G || code == Codes.I || code == Codes.L || code == Codes.K || code == Codes.O;
		}

		public static bool IsModelYearRequired(string code)
		{
			return code == Codes.A || code == Codes.C || code == Codes.U || code == Codes.Y || code == Codes.Z;
		}
	}
}
