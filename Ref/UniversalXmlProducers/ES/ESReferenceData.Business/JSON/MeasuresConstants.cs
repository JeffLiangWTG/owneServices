namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public static class MeasuresConstants
	{
		public static class VAT
		{
			public const string VATCode = "ESIVA";
			public const string IV1 = "IV1";
			public const string IV2 = "IV2";
			public const string IV3 = "IV3";
			public const string Exemption = "0 %";
		}

		public static class IGIC
		{
			public const string IGICCode = "CANIGIC";
			public const string IG1 = "IG1";
			public const string IG2 = "IG2";
			public const string IG3 = "IG3";
			public const string IG4 = "IG4";
			public const string IG5 = "IG5";
			public const string IG6 = "IG6";
		}

		public static class AIEM
		{
			public const string AIEMCode = "CANAIEM";
			public const string AIEM01 = "AIEM01";
			public const string AIEM02 = "AIEM02";
			public const string AIEM03 = "AIEM03";
			public const string AIEM04 = "AIEM04";
			public const string AIEM05 = "AIEM05";
			public const string AIEM06 = "AIEM06";
			public const string AIEM07 = "AIEM07";
			public const string AIEM08 = "AIEM08";
			public const string AIEM09 = "AIEM09";
			public const string AIEM10 = "AIEM10";
			public const string AIEM11 = "AIEM11";
			public const string AIEMRateCode = "3AI";

			public static class RateFormulas
			{
				public const string AIEM01 = "0.05*VFD";
				public const string AIEM02 = "0.10*VFD";
				public const string AIEM03 = "0.15*VFD";
				public const string AIEM04 = "0.25*VFD";
				public const string AIEM05 = "7*[KLT]";
				public const string AIEM06 = "7.5*[KLT]";
				public const string AIEM07 = "8.5*[KLT]";
				public const string AIEM08 = "6.5*[KLT]";
				public const string AIEM09 = "4*[TNE]";
				public const string AIEM10 = "12*[TNE]";
				public const string AIEM11 = "MAX(18*[MIL], 0.15*VFD)";
			}
		}

		public static class UOM
		{
			public const string HLT = "HLT";
			public const string KLT = "KLT";
			public const string TNE = "TNE";
			public const string MIL = "MIL";
			public const string KGM = "KGM";
		}
	}
}
