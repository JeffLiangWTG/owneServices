namespace Enterprise.Customs.US.Business
{
	public static class FDAIntendedUseCodesHelper
	{
		public static class Codes
		{
			public const string _015000 = "015.000";
			public const string _080000 = "080.000";
			public const string _080012 = "080.012";
			public const string _081000 = "081.000";
			public const string _081001 = "081.001";
			public const string _081002 = "081.002";
			public const string _081003 = "081.003";
			public const string _081004 = "081.004";
			public const string _081005 = "081.005";
			public const string _081006 = "081.006";
			public const string _081007 = "081.007";
			public const string _081008 = "081.008";
			public const string _082000 = "082.000";
			public const string _085000 = "085.000";
			public const string _085003 = "085.003";
			public const string _090000 = "090.000";
			public const string _100000 = "100.000";
			public const string _110000 = "110.000";
			public const string _120000 = "120.000";
			public const string _130000 = "130.000";
			public const string _130037 = "130.037";
			public const string _140000 = "140.000";
			public const string _150000 = "150.000";
			public const string _150007 = "150.007";
			public const string _150013 = "150.013";
			public const string _150017 = "150.017";
			public const string _150020 = "150.020";
			public const string _155000 = "155.000";
			public const string _155009 = "155.009";
			public const string _155010 = "155.010";
			public const string _155011 = "155.011";
			public const string _155012 = "155.012";
			public const string _170000 = "170.000";
			public const string _180000 = "180.000";
			public const string _180001 = "180.001";
			public const string _180009 = "180.009";
			public const string _180010 = "180.010";
			public const string _180014 = "180.014";
			public const string _180015 = "180.015";
			public const string _180016 = "180.016";
			public const string _180017 = "180.017";
			public const string _180018 = "180.018";
			public const string _180026 = "180.026";
			public const string _210000 = "210.000";
			public const string _260000 = "260.000";
			public const string _270000 = "270.000";
			public const string _920000 = "920.000";
			public const string _920001 = "920.001";
			public const string _920002 = "920.002";
			public const string _940000 = "940.000";
			public const string _950001 = "950.001";
			public const string _950002 = "950.002";
			public const string _970000 = "970.000";
			public const string _970001 = "970.001";
			public const string _970002 = "970.002";
			public const string _980000 = "980.000";
			public const string UNK = "UNK";
		}

		public static bool IsAOCRequiredForDRU_DA(string code)
		{
			return code != Codes._100000
				&& code != Codes._130000
				&& code != Codes._180009
				&& code != Codes._180017
				&& code != Codes._970000
				&& code != Codes._150013;
		}

		public static bool IsAOCRequiredForDRU_REG(string code)
		{
			return code != Codes._100000
				&& code != Codes._180009
				&& code != Codes._180017
				&& code != Codes._970000;
		}

		public static bool IsAOCRequiredForDRU_DLS(string code)
		{
			return code != Codes._100000
				&& code != Codes._180009
				&& code != Codes._180017
				&& code != Codes._970000;
		}

		public static bool IsAOCRequiredForDRU_IND(string code)
		{
			return code == Codes._180009;
		}

		public static bool IsFinishedDosageFormDrugs(string code)
		{
			return code == Codes._080000
				|| code == Codes._130000
				|| code == Codes._155009;
		}

		public static bool IsActivePharmaceuticalIngredients(string code)
		{
			return code == Codes._150007
				|| code == Codes._150013
				|| code == Codes._150017;
		}

		public static bool IsInvestigationalOrResearch(string code)
		{
			return code == Codes._180009
				|| code == Codes._180017;
		}
	}
}
