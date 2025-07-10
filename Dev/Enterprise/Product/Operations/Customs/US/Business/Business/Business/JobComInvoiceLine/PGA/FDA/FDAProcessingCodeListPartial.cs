namespace Enterprise.Customs.US.Business
{
	partial class FDAProcessingCodeList
	{
		public static bool IsBrandNameMandatory(string code)
		{
			return code == Codes.BIO_BDP ||
					code == Codes.BIO_VAC ||
					code == Codes.BIO_BLD ||
					code == Codes.RAD_REP;
		}

		public static bool IsSpeficiAOCRequiredForBIO(string code)
		{
			return code == Codes.BIO_ALG ||
					code == Codes.BIO_VAC ||
					code == Codes.BIO_CGT ||
					code == Codes.BIO_BLO ||
					code == Codes.BIO_BLD ||
					code == Codes.BIO_BDP ||
					code == Codes.BIO_BBA ||
					code == Codes.BIO_PVE;
		}

		public static bool IsAOCRequiredForBIO_ALG_180009(string code)
		{
			return code == Codes.BIO_ALG ||
					code == Codes.BIO_BBA ||
					code == Codes.BIO_BDP ||
					code == Codes.BIO_CGT ||
					code == Codes.BIO_PVE ||
					code == Codes.BIO_VAC ||
					code == Codes.BIO_XEN ||
					code == Codes.BIO_BLD;
		}

		public static bool IsAOCRequiredForBIO_ALG_080000(string code)
		{
			return code == Codes.BIO_ALG ||
					code == Codes.BIO_BDP ||
					code == Codes.BIO_BLD ||
					code == Codes.BIO_BLO ||
					code == Codes.BIO_CGT ||
					code == Codes.BIO_VAC ||
					code == Codes.BIO_XEN;
		}

		public static bool IsAOCRequiredForBIO_BBA_080000(string code)
		{
			return code == Codes.BIO_BBA ||
					code == Codes.BIO_PVE;
		}

		public static bool IsAOCRequiredForBIO_HCT_082000(string code)
		{
			return code == Codes.BIO_HCT;
		}

		public static bool IsAOCRequiredForBIO_ALG_180016(string code)
		{
			return code == Codes.BIO_ALG ||
					code == Codes.BIO_BDP ||
					code == Codes.BIO_BLD ||
					code == Codes.BIO_BLO ||
					code == Codes.BIO_CGT ||
					code == Codes.BIO_VAC ||
					code == Codes.BIO_XEN;
		}

		public static bool IsAOCRequiredForBIO_ALG_155000(string code)
		{
			return code == Codes.BIO_ALG ||
					code == Codes.BIO_BLO ||
					code == Codes.BIO_BLD ||
					code == Codes.BIO_BDP ||
					code == Codes.BIO_BBA ||
					code == Codes.BIO_VAC ||
					code == Codes.BIO_XEN ||
					code == Codes.BIO_CGT ||
					code == Codes.BIO_PVE;
		}

		public static bool IsAOCRequiredForBIO_ALG_150007(string code)
		{
			return code == Codes.BIO_ALG ||
					code == Codes.BIO_BDP ||
					code == Codes.BIO_BLD ||
					code == Codes.BIO_BLO ||
					code == Codes.BIO_CGT ||
					code == Codes.BIO_VAC ||
					code == Codes.BIO_XEN;
		}

		public static bool IsAOCRequiredForBIO_BBA_150007(string code)
		{
			return code == Codes.BIO_BBA ||
					code == Codes.BIO_PVE;
		}

		public static bool IsAOCRequiredForBIO_ALG_970000(string code)
		{
			return code == Codes.BIO_ALG ||
					code == Codes.BIO_BDP ||
					code == Codes.BIO_BLD ||
					code == Codes.BIO_BLO ||
					code == Codes.BIO_CGT ||
					code == Codes.BIO_PVE ||
					code == Codes.BIO_VAC ||
					code == Codes.BIO_XEN;
		}

		public static bool IsAOCRequiredForBIO_ALG_140000(string code)
		{
			return IsAOCRequiredForBIO_ALG_970000(code);
		}

		public static bool IsAOCRequiredForBIO_ALG_170000(string code)
		{
			return code == Codes.BIO_ALG ||
					code == Codes.BIO_BBA ||
					code == Codes.BIO_BDP ||
					code == Codes.BIO_BLD ||
					code == Codes.BIO_BLO ||
					code == Codes.BIO_HCT ||
					code == Codes.BIO_PVE ||
					code == Codes.BIO_VAC ||
					code == Codes.BIO_XEN;
		}

		public static bool IsAOCRequiredForFOO(string code)
		{
			return code == Codes.FOO_NSF ||
				code == Codes.FOO_PRO ||
				code == Codes.FOO_FEE ||
				code == Codes.FOO_ADD ||
				code == Codes.FOO_DSU;
		}
	}
}
